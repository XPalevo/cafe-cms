using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Common.Services.Endpoints;

namespace Auth;

public static class DependencyInjection
{
    public static IServiceCollection AddAuth(this IServiceCollection services, JwtOptions jwtOptions, Action<DbContextOptionsBuilder> optionsBuilderAction)
    {
        services.AddAuthentication().AddJwtBearer(x =>
        {
            x.SaveToken = true;
            x.RequireHttpsMetadata = false;

            x.TokenValidationParameters = new()
            {
                ValidateAudience = jwtOptions.ValidateAudience,
                ValidateIssuer = jwtOptions.ValidateIssuer,
                ValidAudiences = jwtOptions.ValidAudiences,
                ValidIssuers = jwtOptions.ValidIssuers,
                ValidateLifetime = jwtOptions.ValidateLifetime,
                ValidateIssuerSigningKey = jwtOptions.ValidateIssuerSigningKey,
            };
        });

        services.AddAuthorization();

        services.AddDbContext<AuthDbContext>(optionsBuilderAction);

        services.AddIdentityCore<AppUser>()
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<AuthDbContext>();
            // .AddUserManager<UserManager<AppUser>>()
            // .AddRoleManager<RoleManager<AppRole>>()
            // .AddSignInManager();

        return services;
    }

    public static IServiceCollection AddAuthEndpoints(this IServiceCollection services)
    {
        services.AddIdentityApiEndpoints<AppUser>();
        return services;
    }

    public static IApplicationBuilder UseAuth(this IApplicationBuilder builder)
    {
        builder.UseAuthentication();
        builder.UseAuthorization();
        return builder;
    }

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder builder)
    {
        // builder.MapEndpoints(typeof(DependencyInjection).Assembly);
        builder.MapIdentityApi<AppUser>();
        return builder;
    }
}
