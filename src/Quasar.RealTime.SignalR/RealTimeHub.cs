using Microsoft.AspNetCore.SignalR;

namespace Quasar.RealTime.SignalR;

/// <summary>
/// Base SignalR hub that enforces a strongly typed client contract.
/// </summary>
public abstract class RealTimeHub<TClient> : Hub<TClient>
    where TClient : class
{
}
