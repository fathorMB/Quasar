using Microsoft.AspNetCore.SignalR;
using Quasar.RealTime;

namespace Quasar.RealTime.SignalR;

public interface ISignalRDispatcher<TClient, in TPayload>
    where TClient : class
{
    Task DispatchAsync(TClient client, TPayload payload, CancellationToken cancellationToken = default);
}

public sealed class SignalRNotifier<THub, TClient, TPayload> : IRealTimeNotifier<TPayload>
    where THub : RealTimeHub<TClient>
    where TClient : class
{
    private readonly IHubContext<THub, TClient> _context;
    private readonly ISignalRDispatcher<TClient, TPayload> _dispatcher;

    public SignalRNotifier(IHubContext<THub, TClient> context, ISignalRDispatcher<TClient, TPayload> dispatcher)
    {
        _context = context;
        _dispatcher = dispatcher;
    }

    public Task NotifyAsync(TPayload payload, CancellationToken cancellationToken = default)
        => _dispatcher.DispatchAsync(_context.Clients.All, payload, cancellationToken);
}
