using Microsoft.Extensions.DependencyInjection;
using Quasar.RealTime;

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
        services.AddSingleton<IRealTimeNotifier<TPayload>, SignalRNotifier<THub, TClient, TPayload>>();
        return services;
    }
}
