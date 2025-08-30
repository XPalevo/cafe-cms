using System.Reflection;
using Microsoft.AspNetCore.Routing;

namespace Common.Services.Endpoints;

public static class EndpointsExtension
{
    public static void MapEndpoints(this IEndpointRouteBuilder routeBuilder, Assembly assembly)
    {
        var implementations = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IEndpoint).IsAssignableFrom(t))
            .Select(t => (IEndpoint)Activator.CreateInstance(t)!);

        foreach (var implementation in implementations)
        {
            implementation.MapEndpoint(routeBuilder);
        }
    }
}