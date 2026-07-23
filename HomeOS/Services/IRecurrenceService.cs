namespace HomeOS.Services;

// Core service - shared by every module with a RecurrenceRule field (Tasks,
// Calendar, Reminders, Finance/Bill). See Docs/02_Pravila_Programiranja.md,
// section 1.3 - do not write a second recurrence implementation per module.
public interface IRecurrenceService
{
    // Returns the next occurrence after fromUtc, or null if the rule is
    // missing/unrecognized (a one-off item with no recurrence).
    DateTime? GetNextOccurrence(string? recurrenceRule, DateTime fromUtc);
}
