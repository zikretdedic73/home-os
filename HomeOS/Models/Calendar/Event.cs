using System.ComponentModel.DataAnnotations;
using HomeOS.Models.Common;

namespace HomeOS.Models.Calendar;

public class Event : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public DateTime StartsAtUtc { get; set; }

    public DateTime EndsAtUtc { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    // Simple format (e.g. "weekly", "monthly") - same mechanism as TaskItem,
    // interpreted by IRecurrenceService.
    public string? RecurrenceRule { get; set; }

    public ICollection<EventAttendee> Attendees { get; set; } = new List<EventAttendee>();
}
