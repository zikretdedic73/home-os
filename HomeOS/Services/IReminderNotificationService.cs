using HomeOS.Models.Reminders;

namespace HomeOS.Services;

// Cross-module capability that Reminders exposes to the rest of the app (the
// Dashboard triggers this on load) - see Docs/00_Specifikacija_Izvor.md,
// "Postojeće aplikacije kao dobri građani platforme".
public interface IReminderNotificationService
{
    // Finds due reminders for the household and sends e-mail/marks in-app
    // notification for any recipient not yet notified for this trigger.
    Task ProcessDueRemindersAsync(int householdId);

    Task<List<Reminder>> GetActiveRemindersForMemberAsync(int householdId, int memberId);
}
