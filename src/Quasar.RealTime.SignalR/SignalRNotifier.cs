using Microsoft.AspNetCore.SignalR;
using Quasar.RealTime;

namespace Quasar.RealTime.SignalR;

/// <summary>
/// Dispatches payloads to the specified SignalR client proxy.
/// </summary>
public interface ISignalRDispatcher<TClient, in TPayload>
    where TClient : class
{
    /// <summary>
    /// Sends the <paramref name="payload"/> to the provided <paramref name="client"/> proxy.
    /// </summary>
    Task DispatchAsync(TClient client, TPayload payload, CancellationToken cancellationToken = default);
}

/// <summary>
/// Bridges the generic real-time notifier abstraction with SignalR hubs.
/// </summary>
public sealed class SignalRNotifier<THub, TClient, TPayload> : IRealTimeNotifier<TPayload>
    where THub : RealTimeHub<TClient>
    where TClient : class
{
    private readonly IHubContext<THub, TClient> _context;
    private readonly ISignalRDispatcher<TClient, TPayload> _dispatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignalRNotifier{THub, TClient, TPayload}"/> class.
    /// </summary>
    public SignalRNotifier(IHubContext<THub, TClient> context, ISignalRDispatcher<TClient, TPayload> dispatcher)
    {
        _context = context;
        _dispatcher = dispatcher;
    }

    /// <inheritdoc />
    public Task NotifyAsync(TPayload payload, CancellationToken cancellationToken = default)
        => _dispatcher.DispatchAsync(_context.Clients.All, payload, cancellationToken);
}
