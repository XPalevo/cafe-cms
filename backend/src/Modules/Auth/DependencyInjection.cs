using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Builder;

namespace Auth;

public static class DependencyInjection
{
    public static IServiceCollection AddAuth<TDbContext>(this IServiceCollection services, JwtOptions jwtOptions)
        where TDbContext : DbContext
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

        services.AddIdentityCore<AppUser>()
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<TDbContext>();
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
        builder.MapIdentityApi<AppUser>();
        return builder;
    }
}
