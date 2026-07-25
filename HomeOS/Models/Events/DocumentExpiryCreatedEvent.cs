using HomeOS.Services.Events;

namespace HomeOS.Models.Events;

// Published by Life Admin when a document with an expiry date is created. The
// Reminders module reacts by scheduling a renewal warning before it expires -
// same decoupled "ako ovo, onda ono" pattern as tasks and bills.
public record DocumentExpiryCreatedEvent(
    int HouseholdId,
    int DocumentId,
    int OwnerMemberId,
    string Name,
    DateTime ExpiryUtc) : IIntegrationEvent;
