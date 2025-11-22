using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quasar.Telemetry;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Quasar.Web;

/// <summary>
/// Background service that broadcasts metrics snapshots to connected SignalR clients.
/// </summary>
public sealed class MetricsBroadcaster : BackgroundService
{
    private readonly IMetricsAggregator _aggregator;
    private readonly IHubContext<MetricsHub, IMetricsClient> _hubContext;
    private readonly ILogger<MetricsBroadcaster> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(2);

    public MetricsBroadcaster(
        IMetricsAggregator aggregator,
        IHubContext<MetricsHub, IMetricsClient> hubContext,
        ILogger<MetricsBroadcaster> logger)
    {
        _aggregator = aggregator;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Metrics Broadcaster");

        using var timer = new PeriodicTimer(_interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var snapshot = _aggregator.GetSnapshot();
                await _hubContext.Clients.All.ReceiveSnapshot(snapshot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting metrics snapshot");
            }
        }
    }
}
