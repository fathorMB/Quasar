namespace Quasar.RealTime.SignalR;

public interface IRealTimePublisher<THub, TClient>
    where THub : RealTimeHub<TClient>
    where TClient : class
{
    Task PublishAsync(Func<TClient, Task> publish, CancellationToken cancellationToken = default);
}
