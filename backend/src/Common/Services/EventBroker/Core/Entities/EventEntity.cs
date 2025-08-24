namespace Common.Services.EventBroker.Core.Entities;

using Common.Entities;

public class EventEntity : BaseEntity
{
    public int Version { get; } = 1;
    
    public string Topic { get; }
    
    public string Name { get; }
    
    public string Payload  { get; }
    
    public bool IsProcessed { get; private set; } =  false;

    public void SetProcessed()
    {
        this.IsProcessed = true;
        this.SetUpdatedAt();
    }

    public EventEntity(string topic, string name, string payload)
    {
        this.Topic = topic;
        this.Name = name;
        this.Payload = payload;
    }
}