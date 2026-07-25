using HomeOS.Services.Events;

namespace HomeOS.Models.Events;

// Published by the Finance module when a bill with a due date is created. The
// Reminders module reacts (schedules a warning before the due date) without
// Finance calling Reminders directly - the same "ako ovo, onda ono" decoupling
// as tasks. Lives in the module-neutral events namespace.
public record BillDueDateCreatedEvent(
    int HouseholdId,
    int BillId,
    int OwnerMemberId,
    string Name,
    DateTime DueDateUtc) : IIntegrationEvent;
