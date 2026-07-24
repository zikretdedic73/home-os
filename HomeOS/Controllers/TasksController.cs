using HomeOS.Data;
using HomeOS.Models.Common;
using HomeOS.Models.Events;
using HomeOS.Models.Households;
using HomeOS.Models.Tasks;
using HomeOS.Services;
using HomeOS.Services.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Controllers;

[Authorize]
public class TasksController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentHouseholdService _household;
    private readonly IStringLocalizer<TasksController> _localizer;
    private readonly IEventBus _eventBus;

    public TasksController(ApplicationDbContext context, ICurrentHouseholdService household, IStringLocalizer<TasksController> localizer, IEventBus eventBus)
    {
        _context = context;
        _household = household;
        _localizer = localizer;
        _eventBus = eventBus;
    }

    // GET: /Tasks
    public async Task<IActionResult> Index(TaskState? statusFilter, TaskPriority? priorityFilter, int? assigneeFilter, string? sortBy)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var memberId = await _household.GetCurrentMemberIdAsync();

        var query = _context.Tasks
            .Where(t => t.HouseholdId == householdId && !t.IsDeleted)
            .VisibleTo(memberId)
            .Include(t => t.SubTasks)
            .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
            .AsQueryable();

        if (statusFilter.HasValue)
            query = query.Where(t => t.Status == statusFilter.Value);

        if (priorityFilter.HasValue)
            query = query.Where(t => t.Priority == priorityFilter.Value);

        if (assigneeFilter.HasValue)
            query = query.Where(t => t.AssigneeId == assigneeFilter.Value);

        var orderedQuery = sortBy switch
        {
            "priority" => query.OrderByDescending(t => t.Priority),
            "assignee" => query.OrderBy(t => t.AssigneeId == null).ThenBy(t => t.AssigneeId),
            _ => query.OrderBy(t => t.DueDate == null).ThenBy(t => t.DueDate)
        };

        var tasks = await orderedQuery.ToListAsync();

        ViewBag.Members = await GetMembersAsync(householdId);
        ViewBag.StatusFilter = statusFilter;
        ViewBag.PriorityFilter = priorityFilter;
        ViewBag.AssigneeFilter = assigneeFilter;
        ViewBag.SortBy = sortBy ?? "duedate";

        return View(tasks);
    }

    // GET: /Tasks/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Members = await GetMembersAsync(await _household.GetCurrentHouseholdIdAsync());
        return View(new TaskItem());
    }

    // POST: /Tasks/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskItem model, string? tagsInput)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();

        if (!ModelState.IsValid)
        {
            ViewBag.Members = await GetMembersAsync(householdId);
            return View(model);
        }

        model.HouseholdId = householdId;
        model.OwnerId = await _household.GetCurrentMemberIdAsync();
        model.CreatedAtUtc = DateTime.UtcNow;

        _context.Tasks.Add(model);
        await _context.SaveChangesAsync();

        await SaveTagsAsync(model, tagsInput, householdId);

        // Publish a "key moment" - the Reminders module reacts (auto-creates a
        // reminder at the due date) without Tasks calling it directly.
        if (model.DueDate.HasValue)
        {
            await _eventBus.PublishAsync(new TaskWithDueDateCreatedEvent(
                householdId, model.Id, model.OwnerId, model.Title, model.DueDate.Value));
        }

        TempData["Success"] = _localizer["TaskCreatedSuccessMessage"].Value;
        return RedirectToAction(nameof(Index));
    }

    // GET: /Tasks/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();

        var task = await _context.Tasks
            .Include(t => t.SubTasks)
            .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
            .FirstOrDefaultAsync(t => t.Id == id && t.HouseholdId == householdId);

        if (task == null) return NotFound();

        ViewBag.Members = await GetMembersAsync(householdId);
        ViewBag.TagsInput = string.Join(", ", task.TaskTags
            .Where(tt => tt.Tag != null)
            .Select(tt => tt.Tag!.Name));

        return View(task);
    }

    // POST: /Tasks/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TaskItem model, string? tagsInput)
    {
        if (id != model.Id) return NotFound();

        var householdId = await _household.GetCurrentHouseholdIdAsync();

        var task = await _context.Tasks
            .Include(t => t.TaskTags)
            .Include(t => t.SubTasks)
            .FirstOrDefaultAsync(t => t.Id == id && t.HouseholdId == householdId);

        if (task == null) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.Members = await GetMembersAsync(householdId);
            return View(model);
        }

        task.Title = model.Title;
        task.Description = model.Description;
        task.DueDate = model.DueDate;
        task.Priority = model.Priority;
        task.Status = model.Status;
        task.AssigneeId = model.AssigneeId;
        task.RecurrenceRule = model.RecurrenceRule;
        task.Visibility = model.Visibility;
        task.UpdatedAtUtc = DateTime.UtcNow;

        _context.TaskTags.RemoveRange(task.TaskTags);
        await _context.SaveChangesAsync();
        await SaveTagsAsync(task, tagsInput, householdId);

        TempData["Success"] = _localizer["TaskUpdatedSuccessMessage"].Value;
        return RedirectToAction(nameof(Index));
    }

    // POST: /Tasks/AddSubTask
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSubTask(int taskItemId, string title)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            var maxOrder = await _context.SubTasks
                .Where(s => s.TaskItemId == taskItemId)
                .Select(s => (int?)s.SortOrder)
                .MaxAsync() ?? 0;

            _context.SubTasks.Add(new SubTask
            {
                TaskItemId = taskItemId,
                Title = title.Trim(),
                SortOrder = maxOrder + 1
            });
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Edit), new { id = taskItemId });
    }

    // POST: /Tasks/ToggleSubTask
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleSubTask(int id, int taskItemId)
    {
        var subTask = await _context.SubTasks.FindAsync(id);
        if (subTask != null)
        {
            subTask.IsDone = !subTask.IsDone;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Edit), new { id = taskItemId });
    }

    // GET: /Tasks/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.HouseholdId == householdId);

        if (task == null) return NotFound();
        return View(task);
    }

    // POST: /Tasks/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.HouseholdId == householdId);

        if (task != null)
        {
            // Soft delete - rule from Docs/02_Pravila_Programiranja.md, section 3
            task.IsDeleted = true;
            task.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        TempData["Success"] = _localizer["TaskDeletedSuccessMessage"].Value;
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<Member>> GetMembersAsync(int householdId)
    {
        return await _context.Members
            .Where(m => m.HouseholdId == householdId)
            .OrderBy(m => m.DisplayName)
            .ToListAsync();
    }

    private async Task SaveTagsAsync(TaskItem task, string? tagsInput, int householdId)
    {
        if (string.IsNullOrWhiteSpace(tagsInput)) return;

        var tagNames = tagsInput
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var name in tagNames)
        {
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.HouseholdId == householdId && t.Name == name);

            if (tag == null)
            {
                tag = new Tag { HouseholdId = householdId, Name = name };
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();
            }

            _context.TaskTags.Add(new TaskTag { TaskItemId = task.Id, TagId = tag.Id });
        }

        await _context.SaveChangesAsync();
    }
}
