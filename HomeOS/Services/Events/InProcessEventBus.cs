using Microsoft.Extensions.DependencyInjection;

namespace HomeOS.Services.Events;

// Lightweight in-process synchronous bus - enough to decouple modules for
// this scope. A production system would move to a real message bus or
// MediatR notifications (Docs/02_Pravila_Programiranja.md, section 9); the
// IEventBus contract stays the same, so publishers/handlers would not change.
public class InProcessEventBus : IEventBus
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InProcessEventBus> _logger;

    public InProcessEventBus(IServiceProvider serviceProvider, ILogger<InProcessEventBus> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent integrationEvent) where TEvent : IIntegrationEvent
    {
        var handlers = _serviceProvider.GetServices<IEventHandler<TEvent>>();

        foreach (var handler in handlers)
        {
            try
            {
                await handler.HandleAsync(integrationEvent);
            }
            catch (Exception ex)
            {
                // One misbehaving subscriber must not break the publisher or
                // the other subscribers - a module stays a good citizen even
                // when another module's handler fails.
                _logger.LogError(ex, "Handler {Handler} failed for event {Event}.",
                    handler.GetType().Name, typeof(TEvent).Name);
            }
        }
    }
}
