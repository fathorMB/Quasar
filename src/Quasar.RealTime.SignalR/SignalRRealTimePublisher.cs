using Microsoft.AspNetCore.SignalR;

namespace Quasar.RealTime.SignalR;

internal sealed class SignalRRealTimePublisher<THub, TClient> : IRealTimePublisher<THub, TClient>
    where THub : RealTimeHub<TClient>
    where TClient : class
{
    private readonly IHubContext<THub, TClient> _context;

    public SignalRRealTimePublisher(IHubContext<THub, TClient> context)
    {
        _context = context;
    }

    public Task PublishAsync(Func<TClient, Task> publish, CancellationToken cancellationToken = default)
    {
        var clientProxy = _context.Clients.All;
        return publish(clientProxy);
    }
}
