namespace Common.Services.EventBroker.Infrastructure;

using Common.Services.EventBroker.Core.Ports;
using Common.Services.EventBroker.Core.Entities;

public class EventHandlerRouter :  IEventHandlerRouter
{
    public Dictionary<string, IEventHandler> Handlers { get; } = new();
    
    public void RegisterHandler(IEventHandler handler)
    {
        Handlers[handler.EventName] = handler;
    }
    
    public async Task RouteAsync(EventEntity evt)
    {
        if (Handlers.TryGetValue(evt.Name, out var handler))
            await handler.HandleAsync(evt);
        else
            Console.WriteLine($"No handler registered for event: {evt.Name}");
    }
}