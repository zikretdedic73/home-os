namespace HomeOS.Models.Reminders;

// Composite key (ReminderId, MemberId) - see Docs/04_Model_Podataka.md, section 6.
public class ReminderRecipient
{
    public int ReminderId { get; set; }
    public Reminder? Reminder { get; set; }
    public int MemberId { get; set; }
    public bool NotifiedViaEmail { get; set; }
    public DateTime? NotifiedInAppAtUtc { get; set; }
}
