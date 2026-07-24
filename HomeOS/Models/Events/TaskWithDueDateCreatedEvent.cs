using HomeOS.Services.Events;

namespace HomeOS.Models.Events;

// Published by the Tasks module when a task with a due date is created. Lives
// in a module-neutral namespace so any module can subscribe without depending
// on Tasks internals. The Reminders module reacts to it (see
// TaskWithDueDateCreatedHandler) - "ako ovo, onda ono" without direct coupling.
public record TaskWithDueDateCreatedEvent(
    int HouseholdId,
    int TaskId,
    int OwnerMemberId,
    string Title,
    DateTime DueDateUtc) : IIntegrationEvent;
