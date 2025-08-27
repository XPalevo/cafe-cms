namespace Common.Services.EventBroker.Core.Ports;

using Common.Services.EventBroker.Core.Entities;

public interface IEventBroker
{
    Task<bool> PublishAsync(EventEntity eventEntity);
    
    void Subscribe(string topic, Func<EventEntity, Task> handler);
    
    void Unsubscribe(string topic, Func<EventEntity, Task> handler);
}