namespace HomeOS.Models.Notes;

// Polymorphic link target for a note (Docs/04_Model_Podataka.md, section 7) -
// same pattern as Reminder.SourceType. Bill/Document are added when those
// modules exist (Day 4).
public enum NoteLinkType
{
    None,
    Task,
    Event
}
