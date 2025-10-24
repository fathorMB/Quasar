using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;

namespace Quasar.RealTime.SignalR;

/// <summary>
/// Endpoint routing helpers for exposing SignalR hubs.
/// </summary>
public static class SignalREndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the specified <typeparamref name="THub"/> to the provided <paramref name="pattern"/>.
    /// </summary>
    public static IEndpointRouteBuilder MapRealTimeHub<THub>(this IEndpointRouteBuilder endpoints, string pattern)
        where THub : Hub
    {
        endpoints.MapHub<THub>(pattern);
        return endpoints;
    }
}
