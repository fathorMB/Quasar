using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Quasar.Projections.Abstractions;

namespace Quasar.RealTime.SignalR;

/// <summary>
/// Broadcast hub for live read model updates.
/// </summary>
public sealed class LiveReadModelBroadcastHub : RealTimeHub<IReadModelClient>
{
}

/// <summary>
/// Client-side interface for receiving live read model notifications.
/// </summary>
public interface IReadModelClient
{
}

/// <summary>
/// SignalR-based notifier for broadcasting live read model changes.
/// </summary>
public sealed class SignalRLiveReadModelNotifier : ILiveReadModelNotifier
{
    private readonly IHubContext<LiveReadModelBroadcastHub> _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignalRLiveReadModelNotifier"/> class.
    /// </summary>
    public SignalRLiveReadModelNotifier(IHubContext<LiveReadModelBroadcastHub> context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public Task NotifyUpsertAsync<TReadModel>(string key, TReadModel model, CancellationToken cancellationToken = default)
        where TReadModel : class
    {
        var modelTypeName = typeof(TReadModel).Name;
        return _context.Clients.All.SendAsync(
            $"ReadModel:Upsert:{modelTypeName}",
            key,
            model,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task NotifyDeleteAsync<TReadModel>(string key, CancellationToken cancellationToken = default)
        where TReadModel : class
    {
        var modelTypeName = typeof(TReadModel).Name;
        return _context.Clients.All.SendAsync(
            $"ReadModel:Delete:{modelTypeName}",
            key,
            cancellationToken);
    }
}

/// <summary>
/// Extension methods for configuring live read model SignalR integration.
/// </summary>
public static class LiveReadModelSignalRServiceCollectionExtensions
{
    /// <summary>
    /// Registers a SignalR hub and notifier for live read model updates.
    /// </summary>
    public static IServiceCollection AddLiveReadModelHub(this IServiceCollection services)
    {
        services.AddSignalR();
        return services;
    }

    /// <summary>
    /// Registers the live read model notifier.
    /// </summary>
    public static IServiceCollection AddLiveReadModelNotifier(this IServiceCollection services)
    {
        services.AddSingleton<ILiveReadModelNotifier, SignalRLiveReadModelNotifier>();
        return services;
    }
}
