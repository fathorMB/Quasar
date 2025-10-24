using Microsoft.AspNetCore.SignalR;

namespace Quasar.RealTime.SignalR;

public abstract class RealTimeHub<TClient> : Hub<TClient>
    where TClient : class
{
}
