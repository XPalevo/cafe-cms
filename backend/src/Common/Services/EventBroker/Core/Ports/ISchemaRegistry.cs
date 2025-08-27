namespace Common.Services.EventBroker.Core.Ports;

public interface ISchemaRegistry
{
    Task<int> RegisterSchemaAsync(string subject, string schemaJson);
    Task<string> GetSchemaAsync(int schemaId);
}