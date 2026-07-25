namespace HomeOS.Models.Notifications;

// The notification categories a member can individually enable/disable
// (Docs/00 - "uključivanje/isključivanje kategorija obavještenja"). The values
// map to the email triggers the spec lists: reminder due, assigned task, bill
// about to be charged, shared content.
public enum NotificationCategory
{
    ReminderDue = 0,
    TaskAssigned = 1,
    BillDue = 2,
    SharedContent = 3
}
