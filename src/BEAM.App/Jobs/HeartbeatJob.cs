using Microsoft.Extensions.Logging;
using Quasar.Scheduling.Quartz;

namespace BEAM.App.Jobs;

/// <summary>
/// Simple demo job: logs a heartbeat to show Quartz is running.
/// </summary>
public sealed class HeartbeatJob : IQuasarJob
{
    private readonly ILogger<HeartbeatJob> _logger;

    public HeartbeatJob(ILogger<HeartbeatJob> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(QuasarJobContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Heartbeat job executed at {UtcNow}", DateTime.UtcNow);
        return Task.CompletedTask;
    }
}
