using Microsoft.AspNetCore.Routing;

namespace Common.Services.Endpoints;

public interface IEndpoint
{ 
    void MapEndpoint(IEndpointRouteBuilder app);
}
