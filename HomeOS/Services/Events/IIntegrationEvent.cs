namespace HomeOS.Services.Events;

// Marker for a "key moment" one module publishes for others to react to
// (Docs/00_Specifikacija_Izvor.md, "Aplikacije objavljuju događaje i reaguju
// na događaje iz drugih modula"). Event contracts live in a module-neutral
// place (Models/Events) so a subscriber reacts to the contract, not to the
// publishing module directly.
public interface IIntegrationEvent
{
    int HouseholdId { get; }
}
