namespace Common.Services.EventBroker.Infrastructure;

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Avro;
using Avro.Generic;
using Avro.IO;
using Common.Services.EventBroker.Core.Entities;
using Common.Services.EventBroker.Core.Ports;

public class KafkaEventBroker : IEventBroker, IDisposable
{
    private readonly IProducer<string, byte[]> _producer;
    private readonly CachedSchemaRegistryClient _schemaRegistry;

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Func<EventEntity, Task>, byte>> 
        _subscriptions = new();

    private readonly ConcurrentDictionary<string, Task> _consumerTasks = new();
    private readonly CancellationTokenSource _cts = new();

    private readonly ConcurrentDictionary<int, RecordSchema> _schemaCache = new();
    private readonly string _bootstrapServers;

    public KafkaEventBroker(string bootstrapServers, string schemaRegistryUrl)
    {
        _bootstrapServers = bootstrapServers;

        _producer = new ProducerBuilder<string, byte[]>(new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        }).Build();

        _schemaRegistry = new CachedSchemaRegistryClient(new SchemaRegistryConfig
        {
            Url = schemaRegistryUrl
        });
    }

    public async Task<bool> PublishAsync(EventEntity eventEntity)
    {
        try
        {
            var message = new Message<string, byte[]>
            {
                Key = eventEntity.Id.ToString(),
                Value = eventEntity.Payload,
                Headers = new Headers
                {
                    { "schemaId", BitConverter.GetBytes(eventEntity.SchemaId) },
                    { "eventName", System.Text.Encoding.UTF8.GetBytes(eventEntity.Name) }
                }
            };

            await _producer.ProduceAsync(eventEntity.Topic, message);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Publish failed: {ex.Message}");
            return false;
        }
    }

    public void Subscribe(string topic, Func<EventEntity, Task> handler)
    {
        var handlers = _subscriptions.GetOrAdd(topic, _ => new ConcurrentDictionary<Func<EventEntity, Task>, byte>());
        handlers.TryAdd(handler, 0);

        _consumerTasks.GetOrAdd(topic, _ => Task.Run(() => StartConsumerLoop(topic, _cts.Token)));
    }

    public void Unsubscribe(string topic, Func<EventEntity, Task> handler)
    {
        if (_subscriptions.TryGetValue(topic, out var handlers))
        {
            handlers.TryRemove(handler, out _);
        }
    }

    private async Task StartConsumerLoop(string topic, CancellationToken ct)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = $"group-{topic}",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<string, byte[]>(consumerConfig).Build();
        consumer.Subscribe(topic);

        try
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var cr = consumer.Consume(ct);

                    int schemaId = -1;
                    if (cr.Message.Headers.TryGetLastBytes("schemaId", out var schemaBytes))
                        schemaId = BitConverter.ToInt32(schemaBytes);

                    string eventName = "unknown";
                    if (cr.Message.Headers.TryGetLastBytes("eventName", out var nameBytes))
                        eventName = System.Text.Encoding.UTF8.GetString(nameBytes);

                    if (!_schemaCache.TryGetValue(schemaId, out var recordSchema))
                    {
                        var schemaFromRegistry = await _schemaRegistry.GetSchemaAsync(schemaId);
                        recordSchema = (RecordSchema)Avro.Schema.Parse(schemaFromRegistry.SchemaString);
                        _schemaCache[schemaId] = recordSchema;
                    }

                    GenericRecord deserializedRecord;
                    using (var ms = new MemoryStream(cr.Message.Value))
                    {
                        var decoder = new BinaryDecoder(ms);
                        var reader = new GenericReader<GenericRecord>(recordSchema, recordSchema);
                        deserializedRecord = reader.Read(null, decoder);
                    }

                    if (_subscriptions.TryGetValue(topic, out var handlers))
                    {
                        EventEntity evt = new EventEntity(
                            topic: topic,
                            name: eventName,
                            payload: cr.Message.Value,
                            schemaId: schemaId
                        )
                        {
                            DeserializedRecord = deserializedRecord
                        };

                        foreach (var handler in handlers.Keys)
                        {
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    await handler(evt);
                                    evt.MarkProcessed();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Handler failed: {ex}");
                                    evt.MarkFailed();
                                }
                            });
                        }
                    }
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Consume error: {e.Error.Reason}");
                }
            }
        }
        finally
        {
            consumer.Close();
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _producer.Dispose();
        _schemaRegistry.Dispose();
    }
}
