namespace HomeOS.Models.Calendar;

// Composite key (EventId, MemberId) - see Docs/04_Model_Podataka.md, section 5.
public class EventAttendee
{
    public int EventId { get; set; }
    public Event? Event { get; set; }
    public int MemberId { get; set; }
}
