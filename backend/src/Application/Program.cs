using Common.Services.EventBroker.Core.Ports;
using Common.Services.EventBroker.Infrastructure;
using TestA;
using TestB;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IEventBroker>(sp =>
    new KafkaEventBroker(
        bootstrapServers: "kafka-broker:9092",
        schemaRegistryUrl: "http://schema-registry:8081"
    )
);

builder.Services.AddSingleton<ISchemaRegistry>(sp =>
    new KafkaSchemaRegistry("http://schema-registry:8081")
);

builder.Services.AddSingleton<IEventHandlerRouter, EventHandlerRouter>();

builder.Services.AddHandlersB();

builder.Services.AddTransient<ServiceA>();
builder.Services.AddTransient<ServiceB>();

builder.Services.AddControllers();

var app = builder.Build();

var router = app.Services.GetRequiredService<IEventHandlerRouter>();
var allHandlers = app.Services.GetServices<IEventHandler>();
foreach (var h in allHandlers)
{
    router.RegisterHandler(h);
}

var serviceB = app.Services.GetRequiredService<ServiceB>();
serviceB.GetOrder();

app.MapControllers();

app.Run();