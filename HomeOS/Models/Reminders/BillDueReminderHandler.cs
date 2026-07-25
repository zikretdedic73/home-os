using HomeOS.Data;
using HomeOS.Models.Events;
using HomeOS.Services.Events;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Models.Reminders;

// Reminders reacts to a Finance "key moment": when a bill with a due date is
// created, schedule a reminder a few days before it is due (Docs/01 - bills
// "koristi postojeći Reminder modul", ne graditi novu logiku). Finance only
// publishes the event; it never touches Reminders directly.
public class BillDueReminderHandler : IEventHandler<BillDueDateCreatedEvent>
{
    // Warn this many days ahead of the due date ("račun pred naplatu").
    private const int WarningLeadDays = 3;

    private readonly ApplicationDbContext _context;

    public BillDueReminderHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task HandleAsync(BillDueDateCreatedEvent integrationEvent)
    {
        // Idempotent: don't create a second auto-reminder for the same bill.
        var alreadyExists = await _context.Reminders.AnyAsync(r =>
            r.HouseholdId == integrationEvent.HouseholdId &&
            r.SourceType == ReminderSourceType.Bill &&
            r.SourceId == integrationEvent.BillId);

        if (alreadyExists)
            return;

        // Fire a few days before due, but never in the past for a due date
        // that is already close.
        var triggerAt = integrationEvent.DueDateUtc.AddDays(-WarningLeadDays);
        if (triggerAt < DateTime.UtcNow)
            triggerAt = integrationEvent.DueDateUtc;

        var reminder = new Reminder
        {
            HouseholdId = integrationEvent.HouseholdId,
            OwnerId = integrationEvent.OwnerMemberId,
            Title = integrationEvent.Name,
            TriggerAtUtc = triggerAt,
            SourceType = ReminderSourceType.Bill,
            SourceId = integrationEvent.BillId
        };

        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();

        _context.ReminderRecipients.Add(new ReminderRecipient
        {
            ReminderId = reminder.Id,
            MemberId = integrationEvent.OwnerMemberId
        });
        await _context.SaveChangesAsync();
    }
}
