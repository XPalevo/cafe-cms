namespace Common.Utilities.Json;

using System.Text.Json;
using Confluent.Kafka;

public static class JsonConvert
{
    public static string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, Options);
    }

    public static T Deserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException("Cannot deserialize empty JSON");

        return JsonSerializer.Deserialize<T>(json, Options)!;
    }
    
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = false
    };
}