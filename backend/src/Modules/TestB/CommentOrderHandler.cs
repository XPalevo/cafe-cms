namespace TestB;

using Common.Services.EventBroker.Core.Ports;
using Common.Services.EventBroker.Core.Entities;

public class CommentOrderHandler : IEventHandler
{
    public string EventName => "OrderCommented";

    public async Task HandleAsync(EventEntity eventEntity)
    {
        
        var deserializedRecord = eventEntity.DeserializedRecord!;
        
        try
        { 
            int id = (int)deserializedRecord["id"];
            string message = (string)deserializedRecord["message"];
            Console.WriteLine($"Received event {eventEntity.Name}: id={id}, message={message}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        await Task.CompletedTask;
    }
}

