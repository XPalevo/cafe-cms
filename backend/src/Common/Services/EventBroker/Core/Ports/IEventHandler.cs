namespace Common.Services.EventBroker.Core.Ports;

using Common.Services.EventBroker.Core.Entities;

public interface IEventHandler
{
    public string EventName { get; }

    Task HandleAsync(EventEntity eventEntity);
}