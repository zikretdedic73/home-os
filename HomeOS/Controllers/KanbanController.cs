using HomeOS.Data;
using HomeOS.Models.Common;
using HomeOS.Models.Kanban;
using HomeOS.Models.Tasks;
using HomeOS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Controllers;

[Authorize]
public class KanbanController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentHouseholdService _household;

    public KanbanController(ApplicationDbContext context, ICurrentHouseholdService household)
    {
        _context = context;
        _household = household;
    }

    // GET: /Kanban - list of boards (+ create).
    public async Task<IActionResult> Index()
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var memberId = await _household.GetCurrentMemberIdAsync();

        var boards = await _context.Boards
            .Where(b => b.HouseholdId == householdId && !b.IsDeleted)
            .VisibleTo(memberId)
            .OrderBy(b => b.Name)
            .ToListAsync();

        return View(boards);
    }

    // GET: /Kanban/Board/5 - the board with columns and cards.
    public async Task<IActionResult> Board(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var memberId = await _household.GetCurrentMemberIdAsync();

        var board = await _context.Boards
            .Include(b => b.Columns.OrderBy(c => c.SortOrder))
                .ThenInclude(c => c.Cards.OrderBy(cd => cd.SortOrder))
                    .ThenInclude(cd => cd.TaskItem)
            .FirstOrDefaultAsync(b => b.Id == id && b.HouseholdId == householdId && !b.IsDeleted);

        if (board == null) return NotFound();

        // Don't leak another member's private task through a shared board card.
        var visibleTaskIds = await _context.Tasks
            .Where(t => t.HouseholdId == householdId && !t.IsDeleted)
            .VisibleTo(memberId)
            .Select(t => t.Id)
            .ToListAsync();
        var visibleSet = visibleTaskIds.ToHashSet();

        foreach (var column in board.Columns)
            column.Cards = column.Cards.Where(cd => visibleSet.Contains(cd.TaskItemId)).ToList();

        // Tasks not yet carded on this board (candidates for "add card").
        var cardedTaskIds = board.Columns.SelectMany(c => c.Cards).Select(cd => cd.TaskItemId).ToHashSet();
        ViewBag.AvailableTasks = await _context.Tasks
            .Where(t => t.HouseholdId == householdId && !t.IsDeleted && !cardedTaskIds.Contains(t.Id))
            .VisibleTo(memberId)
            .OrderBy(t => t.Title)
            .ToListAsync();

        return View(board);
    }

    // POST: /Kanban/CreateBoard - creates the board and seeds its columns.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBoard(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return RedirectToAction(nameof(Index));

        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var memberId = await _household.GetCurrentMemberIdAsync();

        var board = new Board
        {
            HouseholdId = householdId,
            OwnerId = memberId,
            Name = name.Trim(),
            Columns = new List<Column>
            {
                new() { Name = "To do", SortOrder = 0, MappedStatus = TaskState.Open },
                new() { Name = "In progress", SortOrder = 1, MappedStatus = TaskState.InProgress },
                new() { Name = "Done", SortOrder = 2, MappedStatus = TaskState.Done }
            }
        };

        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Board), new { id = board.Id });
    }

    // POST: /Kanban/DeleteBoard
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBoard(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == id && b.HouseholdId == householdId);

        if (board != null)
        {
            board.IsDeleted = true;
            board.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /Kanban/AddCard - places an existing task on the board, in the
    // column that matches the task's current status.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCard(int boardId, int taskItemId)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();

        var board = await _context.Boards
            .Include(b => b.Columns).ThenInclude(c => c.Cards)
            .FirstOrDefaultAsync(b => b.Id == boardId && b.HouseholdId == householdId && !b.IsDeleted);
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskItemId && t.HouseholdId == householdId);

        if (board != null && task != null)
        {
            var column = board.Columns.FirstOrDefault(c => c.MappedStatus == task.Status)
                         ?? board.Columns.OrderBy(c => c.SortOrder).First();
            var nextOrder = column.Cards.Count == 0 ? 0 : column.Cards.Max(cd => cd.SortOrder) + 1;

            _context.Cards.Add(new Card { ColumnId = column.Id, TaskItemId = task.Id, SortOrder = nextOrder });
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Board), new { id = boardId });
    }

    // POST: /Kanban/RemoveCard - takes the task off the board (keeps the task).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveCard(int cardId, int boardId)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var card = await _context.Cards
            .Include(c => c.Column).ThenInclude(col => col!.Board)
            .FirstOrDefaultAsync(c => c.Id == cardId);

        if (card?.Column?.Board != null && card.Column.Board.HouseholdId == householdId)
        {
            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Board), new { id = boardId });
    }

    // POST: /Kanban/MoveCard - AJAX drag-and-drop. Updates the card's column
    // AND the linked task's status in one transaction (Docs/04, section 4).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveCard(int cardId, int toColumnId, int newIndex)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();

        var card = await _context.Cards
            .Include(c => c.Column).ThenInclude(col => col!.Board)
            .Include(c => c.TaskItem)
            .FirstOrDefaultAsync(c => c.Id == cardId);

        var toColumn = await _context.Columns
            .Include(c => c.Board)
            .FirstOrDefaultAsync(c => c.Id == toColumnId);

        if (card?.Column?.Board == null || toColumn?.Board == null
            || card.Column.Board.HouseholdId != householdId
            || toColumn.Board.HouseholdId != householdId
            || card.Column.BoardId != toColumn.BoardId)
        {
            return Json(new { success = false });
        }

        await using var tx = await _context.Database.BeginTransactionAsync();

        card.ColumnId = toColumnId;
        card.SortOrder = newIndex;
        if (card.TaskItem != null)
        {
            card.TaskItem.Status = toColumn.MappedStatus;
            card.TaskItem.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        return Json(new { success = true });
    }
}
