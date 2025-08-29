namespace Common.Services.EventBroker.Infrastructure;

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Channels;
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

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Func<EventEntity, Task>, byte>> _subscriptions = new();
    private readonly ConcurrentDictionary<string, Task> _consumerTasks = new();
    private readonly CancellationTokenSource _cts = new();

    private readonly ConcurrentDictionary<int, RecordSchema> _schemaCache = new();
    private readonly string _bootstrapServers;

    private readonly Channel<EventEntity> _channel = Channel.CreateUnbounded<EventEntity>();

    public KafkaEventBroker(string bootstrapServers, string schemaRegistryUrl)
    {
        _bootstrapServers = bootstrapServers;

        _producer = new ProducerBuilder<string, byte[]>(new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            LingerMs = 5,
            BatchSize = 64 * 1024,
            CompressionType = CompressionType.Zstd
        }).Build();

        _schemaRegistry = new CachedSchemaRegistryClient(new SchemaRegistryConfig
        {
            Url = schemaRegistryUrl
        });

        Task.Run(() => DispatchLoop(_cts.Token));
    }

    public Task<bool> PublishAsync(EventEntity eventEntity)
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

            _producer.Produce(eventEntity.Topic, message);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Publish failed: {ex.Message}");
            return Task.FromResult(false);
        }
    }

    public void Subscribe(string topic, Func<EventEntity, Task> handler)
    {
        var handlers = _subscriptions.GetOrAdd(topic, _ => new ConcurrentDictionary<Func<EventEntity, Task>, byte>());
        handlers.TryAdd(handler, 0);

        _consumerTasks.GetOrAdd(topic, _ =>
            Task.Run(() => StartConsumerGroup(topic, _cts.Token)));
    }

    public void Unsubscribe(string topic, Func<EventEntity, Task> handler)
    {
        if (_subscriptions.TryGetValue(topic, out var handlers))
        {
            handlers.TryRemove(handler, out _);
        }
    }

    private async Task StartConsumerGroup(string topic, CancellationToken ct)
    {
        int consumerCount = Math.Max(1, Environment.ProcessorCount / 2);

        var tasks = new Task[consumerCount];
        for (int i = 0; i < consumerCount; i++)
        {
            tasks[i] = Task.Run(() => StartConsumerLoop(topic, ct));
        }

        await Task.WhenAll(tasks);
    }

    private async Task StartConsumerLoop(string topic, CancellationToken ct)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = $"group-{topic}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            FetchMinBytes = 1_048_576,
            FetchWaitMaxMs = 100
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

                    var evt = new EventEntity(
                        topic: topic,
                        name: eventName,
                        payload: cr.Message.Value,
                        schemaId: schemaId
                    )
                    {
                        DeserializedRecord = deserializedRecord
                    };

                    await _channel.Writer.WriteAsync(evt, ct);
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

    private async Task DispatchLoop(CancellationToken ct)
    {
        var semaphore = new SemaphoreSlim(Environment.ProcessorCount * 2);

        await foreach (var evt in _channel.Reader.ReadAllAsync(ct))
        {
            if (_subscriptions.TryGetValue(evt.Topic, out var handlers))
            {
                foreach (var handler in handlers.Keys)
                {
                    await semaphore.WaitAsync(ct);
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
                        finally
                        {
                            semaphore.Release();
                        }
                    }, ct);
                }
            }
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _producer.Flush();
        _producer.Dispose();
        _schemaRegistry.Dispose();
    }
}
