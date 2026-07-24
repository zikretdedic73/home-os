namespace HomeOS.Services.Events;

// A module subscribes to another module's event by registering a handler in
// DI. The bus resolves every handler for the published event type - no module
// calls another module's service directly.
public interface IEventHandler<in TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent integrationEvent);
}
