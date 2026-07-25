using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HomeOS.Models.Common;

namespace HomeOS.Models.Finance;

// A recurring or one-off bill/subscription with a due date. Extends BaseEntity
// for household isolation + visibility. When created, Finance publishes an event
// so the Reminders module schedules a warning before the due date - it does not
// build its own reminder logic (Docs/01 - "koristi postojeći Reminder modul").
public class Bill : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public DateOnly DueDate { get; set; }

    // Same simple format as tasks/reminders ("monthly", "yearly", ...); null = one-off.
    public string? RecurrenceRule { get; set; }

    public bool IsPaid { get; set; }
}
