using System.ComponentModel.DataAnnotations;
using HomeOS.Models.Common;

namespace HomeOS.Models.Reminders;

public class Reminder : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public DateTime TriggerAtUtc { get; set; }

    // Simple format (e.g. "daily", "weekly") - interpreted by IRecurrenceService.
    public string? RecurrenceRule { get; set; }

    public ReminderSourceType SourceType { get; set; } = ReminderSourceType.Manual;

    // Id of the source entity (e.g. TaskItem.Id when SourceType = Task) - no
    // strict FK since it targets different tables; checked in application code.
    public int? SourceId { get; set; }

    public bool IsResolved { get; set; }

    public DateTime? SnoozedUntilUtc { get; set; }

    public ICollection<ReminderRecipient> Recipients { get; set; } = new List<ReminderRecipient>();

    public bool IsDue(DateTime nowUtc) =>
        !IsResolved &&
        TriggerAtUtc <= nowUtc &&
        (SnoozedUntilUtc == null || SnoozedUntilUtc <= nowUtc);
}
