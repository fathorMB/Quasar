using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quasar.Core;
using Quasar.EventSourcing.Abstractions;

namespace Quasar.EventSourcing.Outbox;

/// <summary>
/// Periodically prunes processed inbox entries to keep storage lean.
/// </summary>
public sealed class InboxCleanupService : BackgroundService
{
    private readonly IInboxStore? _store;
    private readonly InboxCleanupOptions _options;
    private readonly IClock _clock;
    private readonly ILogger<InboxCleanupService> _logger;

    public InboxCleanupService(
        IEnumerable<IInboxStore> stores,
        IOptions<InboxCleanupOptions> options,
        IClock clock,
        ILogger<InboxCleanupService> logger)
    {
        _store = stores.FirstOrDefault();
        _options = options.Value;
        _clock = clock;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_store is null)
        {
            _logger.LogInformation("Inbox cleanup service disabled because no IInboxStore is registered.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var retention = _options.Retention;
                if (retention <= TimeSpan.Zero)
                {
                    _logger.LogWarning("Inbox cleanup retention is non-positive; skipping cycle.");
                }
                else
                {
                    var cutoff = _clock.UtcNow - retention;
                    await _store.PurgeAsync(cutoff, stoppingToken).ConfigureAwait(false);
                    _logger.LogDebug("Purged inbox messages older than {Cutoff}.", cutoff);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning inbox messages. Next run in {Interval}.", _options.Interval);
            }

            var delay = _options.Interval;
            if (delay <= TimeSpan.Zero)
            {
                delay = TimeSpan.FromMinutes(1);
            }

            await Task.Delay(delay, stoppingToken).ConfigureAwait(false);
        }
    }
}

