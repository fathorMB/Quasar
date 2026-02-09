using Microsoft.Extensions.DependencyInjection;
using Quasar.RealTime;
using Quasar.RealTime.Notifications;
using Quasar.RealTime.SignalR.Notifications;

namespace Quasar.RealTime.SignalR;

/// <summary>
/// Extension methods for configuring SignalR based real-time services.
/// </summary>
public static class SignalRServiceCollectionExtensions
{
    /// <summary>
    /// Adds SignalR services required by Quasar real-time integrations.
    /// </summary>
    public static IServiceCollection AddQuasarSignalR(this IServiceCollection services)
    {
        services.AddSignalR();
        return services;
    }

    /// <summary>
    /// Registers a typed notifier that bridges <typeparamref name="THub"/> with the generic real-time infrastructure.
    /// </summary>
    public static IServiceCollection AddSignalRNotifier<THub, TClient, TPayload, TDispatcher>(this IServiceCollection services)
        where THub : RealTimeHub<TClient>
        where TClient : class
        where TDispatcher : class, ISignalRDispatcher<TClient, TPayload>
    {
        services.AddSignalR();
        services.AddSingleton<ISignalRDispatcher<TClient, TPayload>, TDispatcher>();
        
        services.AddSingleton<SignalRNotifier<THub, TClient, TPayload>>();
        services.AddSingleton<IRealTimeNotifier<TPayload>>(sp => sp.GetRequiredService<SignalRNotifier<THub, TClient, TPayload>>());
        services.AddSingleton<ITargetedRealTimeNotifier<TPayload>>(sp => sp.GetRequiredService<SignalRNotifier<THub, TClient, TPayload>>());
        return services;
    }

    /// <summary>
    /// Registers the Quasar Notification SignalR hub, dispatcher, notifier, and <see cref="PersistentNotificationService"/>.
    /// Consumer apps still need to register an <see cref="INotificationStore"/> implementation separately.
    /// </summary>
    public static IServiceCollection AddNotificationSignalR(this IServiceCollection services)
    {
        // Register the built-in hub infrastructure
        services.AddSignalRNotifier<
            NotificationHub,
            INotificationClient,
            Notification,
            NotificationSignalRDispatcher>();

        // Register the notification service that persists + broadcasts
        services.AddScoped<INotificationService, PersistentNotificationService>();

        return services;
    }
}
