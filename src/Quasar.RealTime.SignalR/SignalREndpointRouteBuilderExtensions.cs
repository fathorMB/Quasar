using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Quasar.RealTime.SignalR.Notifications;

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

    /// <summary>
    /// Maps the framework-provided <see cref="NotificationHub"/> to <c>/hubs/notifications</c>.
    /// </summary>
    public static IEndpointRouteBuilder MapNotificationHub(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<NotificationHub>("/hubs/notifications");
        return endpoints;
    }
}
