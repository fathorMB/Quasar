using Microsoft.AspNetCore.SignalR;

namespace Quasar.RealTime.SignalR;

/// <summary>
/// Internal implementation of <see cref="IRealTimePublisher{THub, TClient}"/> that targets SignalR hubs.
/// </summary>
internal sealed class SignalRRealTimePublisher<THub, TClient> : IRealTimePublisher<THub, TClient>
    where THub : RealTimeHub<TClient>
    where TClient : class
{
    private readonly IHubContext<THub, TClient> _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignalRRealTimePublisher{THub, TClient}"/> class.
    /// </summary>
    public SignalRRealTimePublisher(IHubContext<THub, TClient> context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public Task PublishAsync(Func<TClient, Task> publish, CancellationToken cancellationToken = default)
    {
        var clientProxy = _context.Clients.All;
        return publish(clientProxy);
    }
}
