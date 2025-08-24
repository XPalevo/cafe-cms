namespace Common.Services.EventBroker.Core.Ports;

public interface IEventHandler
{
    Task HandleAsync(string payload);
}