using System.ComponentModel.DataAnnotations;
using HomeOS.Models.Common;

namespace HomeOS.Models.Tasks;

public class TaskItem : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public TaskState Status { get; set; } = TaskState.Open;

    public int? AssigneeId { get; set; }

    // Simple format (e.g. "weekly", "monthly") - interpreted by IRecurrenceService
    // once it is added (not part of Day 1, see Docs/01_Roadmap.md).
    public string? RecurrenceRule { get; set; }

    // Set when this task was generated as the next instance of a recurring task.
    public int? ParentTaskId { get; set; }

    public ICollection<SubTask> SubTasks { get; set; } = new List<SubTask>();
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();

    // Computed, not persisted - drives the "overdue" visual indicator in
    // Views/Tasks/Index.cshtml.
    public bool IsOverdue =>
        DueDate.HasValue &&
        DueDate.Value.Date < DateTime.UtcNow.Date &&
        Status != TaskState.Done;
}
