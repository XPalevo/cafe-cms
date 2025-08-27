namespace Common.Services.EventBroker.Core.Ports;

using Common.Services.EventBroker.Core.Entities;

public interface IEventHandlerRouter
{
    Dictionary<string, IEventHandler> Handlers { get; }

    void RegisterHandler(IEventHandler handler);

    Task RouteAsync(EventEntity evt);

}