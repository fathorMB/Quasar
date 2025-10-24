using Microsoft.Extensions.DependencyInjection;
using Quasar.RealTime;

namespace Quasar.RealTime.SignalR;

public static class SignalRServiceCollectionExtensions
{
    public static IServiceCollection AddQuasarSignalR(this IServiceCollection services)
    {
        services.AddSignalR();
        return services;
    }

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
