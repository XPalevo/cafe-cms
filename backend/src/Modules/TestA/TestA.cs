namespace TestA;

using Common.Services.EventBroker.Core.Ports;
using Common.Services.EventBroker.Core.Entities;

public class ServiceA
{
    private readonly IEventBroker _eventBroker;
    private readonly ISchemaRegistry _schemaRegistry;

    public ServiceA(IEventBroker eventBroker, ISchemaRegistry schemaRegistry)
    {
        _eventBroker = eventBroker;
        _schemaRegistry = schemaRegistry;
    }

    public async Task SendOrder(int id, string? description)
    {
        string schemaJson =  @"{
          ""type"": ""record"",
          ""name"": ""OrderCreated"",
          ""fields"": [
            { ""name"": ""id"", ""type"": ""int"" },
            { ""name"": ""description"", ""type"": [""null"", ""string""], ""default"": null }
          ]
        }";
        
        int schemaId = await _schemaRegistry.RegisterSchemaAsync("OrderCreated-payload", schemaJson);

        var avroSchema = Avro.Schema.Parse(schemaJson);
        var recordSchema = (Avro.RecordSchema)avroSchema;

        var record = new Avro.Generic.GenericRecord(recordSchema);
        record.Add("id", id);
        record.Add("description", description);

        byte[] payload;
        using (var ms = new MemoryStream())
        {
            var encoder = new Avro.IO.BinaryEncoder(ms);
            var writer = new Avro.Generic.GenericWriter<Avro.Generic.GenericRecord>(recordSchema);
            writer.Write(record, encoder);
            encoder.Flush();
            payload = ms.ToArray();
        }

        var evt = new EventEntity(
            topic: "orders",
            name: "OrderCreated",
            payload: payload,
            schemaId: schemaId
        );

        bool success = await _eventBroker.PublishAsync(evt);
        Console.WriteLine($"Order published: {success}");
    }

    public async Task CommentOrder(int id, string message)
    {
        string schemaJson =  @"{
          ""type"": ""record"",
          ""name"": ""OrderCommented"",
          ""fields"": [
            { ""name"": ""id"", ""type"": ""int"" },
            { ""name"": ""message"", ""type"": ""string"" }
          ]
        }";
        
        int schemaId = await _schemaRegistry.RegisterSchemaAsync("OrderCommented-payload", schemaJson);
        
        var avroSchema = Avro.Schema.Parse(schemaJson);
        var recordSchema = (Avro.RecordSchema)avroSchema;
        
        var record = new Avro.Generic.GenericRecord(recordSchema);
        record.Add("id", id);
        record.Add("message", message);
        
        byte[] payload;
        using (var ms = new MemoryStream())
        {
            var encoder = new Avro.IO.BinaryEncoder(ms);
            var writer = new Avro.Generic.GenericWriter<Avro.Generic.GenericRecord>(recordSchema);
            writer.Write(record, encoder);
            encoder.Flush();
            payload = ms.ToArray();
        }

        var evt = new EventEntity(
            topic: "orders",
            name: "OrderCommented",
            payload: payload,
            schemaId: schemaId
        );

        bool success = await _eventBroker.PublishAsync(evt);
        Console.WriteLine($"Order published: {success}");   
    }
}