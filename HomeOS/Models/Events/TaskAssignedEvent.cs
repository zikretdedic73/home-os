using HomeOS.Services.Events;

namespace HomeOS.Models.Events;

// Published by the Tasks module when a task is assigned to a member (on create
// or when the assignee changes). A notification handler reacts by emailing the
// assignee - the source spec lists "dodijeljen zadatak" as an email trigger.
// Tasks never calls the email service directly; it only publishes this event,
// keeping the "ako ovo, onda ono" wiring decoupled and running over the bus.
public record TaskAssignedEvent(
    int HouseholdId,
    int TaskId,
    int AssigneeMemberId,
    string Title,
    DateTime? DueDateUtc) : IIntegrationEvent;
