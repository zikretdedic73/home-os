using HomeOS.Data;
using HomeOS.Models.Tasks;

namespace HomeOS.Services;

public class TaskWorkflowService : ITaskWorkflowService
{
    private readonly ApplicationDbContext _context;
    private readonly IRecurrenceService _recurrence;

    public TaskWorkflowService(ApplicationDbContext context, IRecurrenceService recurrence)
    {
        _context = context;
        _recurrence = recurrence;
    }

    public async Task<TaskItem?> ApplyStatusChangeAsync(TaskItem task, TaskState newStatus)
    {
        var wasDone = task.Status == TaskState.Done;
        task.Status = newStatus;
        task.UpdatedAtUtc = DateTime.UtcNow;

        // Only spawn when transitioning INTO Done, so Done->Open->Done or a
        // no-op save doesn't create duplicate occurrences.
        if (wasDone || newStatus != TaskState.Done || string.IsNullOrWhiteSpace(task.RecurrenceRule))
            return null;

        var baseDate = task.DueDate ?? DateTime.UtcNow;
        var nextDue = _recurrence.GetNextOccurrence(task.RecurrenceRule, baseDate);
        if (nextDue == null)
            return null;

        var next = new TaskItem
        {
            HouseholdId = task.HouseholdId,
            OwnerId = task.OwnerId,
            Visibility = task.Visibility,
            Title = task.Title,
            Description = task.Description,
            Priority = task.Priority,
            Status = TaskState.Open,
            AssigneeId = task.AssigneeId,
            DueDate = nextDue,
            RecurrenceRule = task.RecurrenceRule,
            ParentTaskId = task.ParentTaskId ?? task.Id,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Tasks.Add(next);
        await _context.SaveChangesAsync();
        return next;
    }
}
