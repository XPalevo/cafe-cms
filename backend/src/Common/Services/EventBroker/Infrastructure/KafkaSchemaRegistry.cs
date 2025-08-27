namespace Common.Services.EventBroker.Infrastructure;

using System.Threading.Tasks;
using Confluent.SchemaRegistry;
using Common.Services.EventBroker.Core.Ports;

public class KafkaSchemaRegistry : ISchemaRegistry
{
    private readonly CachedSchemaRegistryClient _client;
    
    public KafkaSchemaRegistry(string url)
    {
        _client = new CachedSchemaRegistryClient(new SchemaRegistryConfig
        {
            Url = url
        });
    }
    
    public async Task<int> RegisterSchemaAsync(string subject, string schemaJson)
    {
        return await _client.RegisterSchemaAsync(subject, schemaJson);
    }

    public async Task<string> GetSchemaAsync(int schemaId)
    {
        var schema = await _client.GetSchemaAsync(schemaId);
        return schema.SchemaString;
    }
}