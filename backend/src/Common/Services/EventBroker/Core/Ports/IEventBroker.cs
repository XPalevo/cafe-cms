namespace Common.Services.EventBroker.Core.Ports;

public interface IEventBroker
{
    Task PublishAsync(string topic, string name, object payload);

    void Subscribe(string topic, Func<string, Task> handler);

    void Unsubscribe(string topic, Func<string, Task> handler);
}