using HomeOS.Data;
using HomeOS.Models.Common;
using HomeOS.Models.Kanban;
using HomeOS.Models.Tasks;
using HomeOS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Controllers;

// Kanban is a live visual view of the household's tasks organized by status
// (Docs/00_Specifikacija_Izvor.md, module 3 - "Vizuelni prikaz zadataka
// organizovanih u kolone"). It owns no data: columns are the task statuses and
// cards are the tasks themselves. Dragging a card changes the task's status.
[Authorize]
public class KanbanController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentHouseholdService _household;
    private readonly ITaskWorkflowService _workflow;

    public KanbanController(ApplicationDbContext context, ICurrentHouseholdService household, ITaskWorkflowService workflow)
    {
        _context = context;
        _household = household;
        _workflow = workflow;
    }

    // GET: /Kanban - one board, columns = statuses, auto-filled from tasks.
    public async Task<IActionResult> Index()
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var memberId = await _household.GetCurrentMemberIdAsync();

        var tasks = await _context.Tasks
            .Where(t => t.HouseholdId == householdId && !t.IsDeleted)
            .VisibleTo(memberId)
            .OrderBy(t => t.DueDate == null).ThenBy(t => t.DueDate)
            .ToListAsync();

        var model = new KanbanBoardViewModel
        {
            Columns = Enum.GetValues<TaskState>()
                .Select(s => new KanbanColumnViewModel(s, tasks.Where(t => t.Status == s).ToList()))
                .ToList()
        };

        return View(model);
    }

    // POST: /Kanban/MoveTask - AJAX drag-and-drop. Changes the task's status
    // (and spawns the next occurrence if it was a recurring task moved to Done).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveTask(int taskId, TaskState toStatus)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var memberId = await _household.GetCurrentMemberIdAsync();

        var task = await _context.Tasks
            .Where(t => t.Id == taskId && t.HouseholdId == householdId && !t.IsDeleted)
            .VisibleTo(memberId)
            .FirstOrDefaultAsync();

        if (task == null)
            return Json(new { success = false });

        await _workflow.ApplyStatusChangeAsync(task, toStatus);
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }
}
