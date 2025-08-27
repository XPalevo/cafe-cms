namespace TestB;

using Common.Services.EventBroker.Core.Ports;
using Microsoft.Extensions.DependencyInjection;

public static class ModuleBExtensions
{
    public static void AddHandlersB(this IServiceCollection services)
    {
        services.AddSingleton<IEventHandler, OrderHandler>();
    }
}