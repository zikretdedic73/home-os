namespace HomeOS.Services.Events;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent integrationEvent) where TEvent : IIntegrationEvent;
}
