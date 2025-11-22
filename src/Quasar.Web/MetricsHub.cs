using Microsoft.AspNetCore.SignalR;
using Quasar.Telemetry;
using System.Threading.Tasks;

namespace Quasar.Web;

/// <summary>
/// Client interface for receiving real-time metrics updates.
/// </summary>
public interface IMetricsClient
{
    /// <summary>
    /// Receives a new metrics snapshot.
    /// </summary>
    Task ReceiveSnapshot(MetricsSnapshot snapshot);
}

/// <summary>
/// SignalR hub for broadcasting telemetry metrics.
/// </summary>
public sealed class MetricsHub : Hub<IMetricsClient>
{
    // Clients only listen, so no methods needed here for now.
}
