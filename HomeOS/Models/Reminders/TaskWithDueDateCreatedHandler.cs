using HomeOS.Data;
using HomeOS.Models.Events;
using HomeOS.Services.Events;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Models.Reminders;

// Reminders reacts to a Tasks "key moment" - when a task with a due date is
// created, it auto-creates a reminder at that due date for the task owner.
// This is the seed of the "ako ovo, onda ono" automation from the spec, and
// reuses the existing Reminders module instead of duplicating logic. Tasks
// never calls Reminders directly - it only publishes the event.
public class TaskWithDueDateCreatedHandler : IEventHandler<TaskWithDueDateCreatedEvent>
{
    private readonly ApplicationDbContext _context;

    public TaskWithDueDateCreatedHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task HandleAsync(TaskWithDueDateCreatedEvent integrationEvent)
    {
        // Idempotent: don't create a second auto-reminder for the same task.
        var alreadyExists = await _context.Reminders.AnyAsync(r =>
            r.HouseholdId == integrationEvent.HouseholdId &&
            r.SourceType == ReminderSourceType.Task &&
            r.SourceId == integrationEvent.TaskId);

        if (alreadyExists)
            return;

        var reminder = new Reminder
        {
            HouseholdId = integrationEvent.HouseholdId,
            OwnerId = integrationEvent.OwnerMemberId,
            Title = integrationEvent.Title,
            TriggerAtUtc = integrationEvent.DueDateUtc,
            SourceType = ReminderSourceType.Task,
            SourceId = integrationEvent.TaskId
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
