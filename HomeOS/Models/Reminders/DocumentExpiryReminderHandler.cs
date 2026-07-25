using HomeOS.Data;
using HomeOS.Models.Events;
using HomeOS.Services.Events;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Models.Reminders;

// Reminders reacts to a Life Admin "key moment": when a document with an expiry
// date is created, schedule a renewal reminder a week before it expires
// (Docs/01 section 4.2 - "datumi isteka/obnove -> automatski generišu Reminder").
public class DocumentExpiryReminderHandler : IEventHandler<DocumentExpiryCreatedEvent>
{
    // Warn this many days before the document expires.
    private const int WarningLeadDays = 7;

    private readonly ApplicationDbContext _context;

    public DocumentExpiryReminderHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task HandleAsync(DocumentExpiryCreatedEvent integrationEvent)
    {
        var alreadyExists = await _context.Reminders.AnyAsync(r =>
            r.HouseholdId == integrationEvent.HouseholdId &&
            r.SourceType == ReminderSourceType.Document &&
            r.SourceId == integrationEvent.DocumentId);

        if (alreadyExists)
            return;

        var triggerAt = integrationEvent.ExpiryUtc.AddDays(-WarningLeadDays);
        if (triggerAt < DateTime.UtcNow)
            triggerAt = integrationEvent.ExpiryUtc;

        var reminder = new Reminder
        {
            HouseholdId = integrationEvent.HouseholdId,
            OwnerId = integrationEvent.OwnerMemberId,
            Title = integrationEvent.Name,
            TriggerAtUtc = triggerAt,
            SourceType = ReminderSourceType.Document,
            SourceId = integrationEvent.DocumentId
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
