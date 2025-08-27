namespace TestB;

using Common.Services.EventBroker.Core.Ports;
using Common.Services.EventBroker.Core.Entities;

public class OrderHandler : IEventHandler
{
    public string EventName => "OrderCreated";

    public async Task HandleAsync(EventEntity eventEntity)
    {
        
        var deserializedRecord = eventEntity.DeserializedRecord!;
        
        try
        { 
            int id = (int)deserializedRecord["id"];
            string description = (string)deserializedRecord["description"];
            Console.WriteLine($"Received event {eventEntity.Name}: id={id}, description={description}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        await Task.CompletedTask;
    }
}

