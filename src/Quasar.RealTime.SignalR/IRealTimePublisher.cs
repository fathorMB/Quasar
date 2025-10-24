namespace Quasar.RealTime.SignalR;

/// <summary>
/// Publishes messages to SignalR hubs without requiring the caller to depend on SignalR infrastructure.
/// </summary>
/// <typeparam name="THub">Hub type broadcasting the updates.</typeparam>
/// <typeparam name="TClient">Client contract exposed by the hub.</typeparam>
public interface IRealTimePublisher<THub, TClient>
    where THub : RealTimeHub<TClient>
    where TClient : class
{
    /// <summary>
    /// Executes the provided <paramref name="publish"/> callback against all connected clients.
    /// </summary>
    Task PublishAsync(Func<TClient, Task> publish, CancellationToken cancellationToken = default);
}
