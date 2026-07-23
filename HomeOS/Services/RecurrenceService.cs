namespace HomeOS.Services;

// Simple recurrence format for this scope (see Docs/04_Model_Podataka.md):
// "daily", "weekly", "monthly", "yearly" - not a full RFC 5545 RRULE parser.
public class RecurrenceService : IRecurrenceService
{
    public DateTime? GetNextOccurrence(string? recurrenceRule, DateTime fromUtc)
    {
        if (string.IsNullOrWhiteSpace(recurrenceRule))
            return null;

        return recurrenceRule.Trim().ToLowerInvariant() switch
        {
            "daily" => fromUtc.AddDays(1),
            "weekly" => fromUtc.AddDays(7),
            "monthly" => fromUtc.AddMonths(1),
            "yearly" => fromUtc.AddYears(1),
            _ => null
        };
    }
}
