namespace HomeOS.Models.Reminders;

// Polymorphic source - see Docs/04_Model_Podataka.md, section 6.
public enum ReminderSourceType
{
    Manual,
    Task,
    Bill,
    Document,
    Event
}
