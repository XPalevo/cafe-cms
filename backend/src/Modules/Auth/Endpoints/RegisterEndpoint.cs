using Auth.Models;
using Common.Services.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;

namespace Auth.Endpoints;

internal class RegisterEndpoint : IEndpoint
{
    public async Task<IResult> HandleAsync(
        HttpContext context,
        UserManager<AppUser> userManager
    )
    {
        var result = await userManager.CreateAsync(new() { });
        await Task.Delay(100);
        return Results.Ok();
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/register", HandleAsync);
    }
}