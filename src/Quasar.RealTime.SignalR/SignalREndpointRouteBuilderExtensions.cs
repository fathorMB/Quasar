using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;

namespace Quasar.RealTime.SignalR;

public static class SignalREndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapRealTimeHub<THub>(this IEndpointRouteBuilder endpoints, string pattern)
        where THub : Hub
    {
        endpoints.MapHub<THub>(pattern);
        return endpoints;
    }
}
