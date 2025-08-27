namespace Common.Services.EventBroker.Core.Entities;

using Common.Entities;
using Avro.Generic;

public class EventEntity : BaseEntity
{
    public string Topic { get; }
    public string Name { get; }
    public byte[] Payload { get; }
    public int SchemaId { get; }
    
    public GenericRecord? DeserializedRecord { get; set; }
    public EventStatus Status { get; private set; } = EventStatus.New;

    public void MarkProcessed()
    {
        Status = EventStatus.Processed;
        SetUpdatedAt();
    }

    public void MarkFailed()
    {
        Status = EventStatus.Failed;
        SetUpdatedAt();
    }

    public EventEntity(string topic, string name, byte[] payload, int schemaId)
    {
        SchemaId = schemaId;
        Topic = topic;
        Name = name;
        Payload = payload;
    }
}

public enum EventStatus
{
    New,
    Processed,
    Failed
}