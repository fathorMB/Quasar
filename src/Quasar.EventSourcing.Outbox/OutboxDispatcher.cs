using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quasar.Core;
using Quasar.EventSourcing.Abstractions;

namespace Quasar.EventSourcing.Outbox;

/// <summary>
/// Background service that continually drains the outbox store and publishes messages.
/// </summary>
public sealed class OutboxDispatcher : BackgroundService
{
    private readonly IOutboxStore _store;
    private readonly ILogger<OutboxDispatcher> _logger;
    private readonly IDictionary<string, IOutboxPublisher> _publishers;
    private readonly OutboxDispatcherOptions _options;
    private readonly IClock _clock;

    public OutboxDispatcher(
        IOutboxStore store,
        IEnumerable<IOutboxPublisher> publishers,
        IOptions<OutboxDispatcherOptions> options,
        IClock clock,
        ILogger<OutboxDispatcher> logger)
    {
        _store = store;
        _logger = logger;
        _clock = clock;
        _options = options.Value;
        _publishers = publishers.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_publishers.Count == 0)
        {
            _logger.LogWarning("Outbox dispatcher started without any registered publishers. No messages will be delivered.");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var batchSize = Math.Max(1, _options.BatchSize);
                var maxAttempts = Math.Max(1, _options.MaxAttempts);
                var messages = await _store.GetPendingAsync(batchSize, maxAttempts, stoppingToken).ConfigureAwait(false);

                if (messages.Count == 0)
                {
                    await Task.Delay(_options.PollInterval, stoppingToken).ConfigureAwait(false);
                    continue;
                }

                foreach (var message in messages)
                {
                    await DispatchMessageAsync(message, maxAttempts, stoppingToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while dispatching outbox messages. Retrying after delay.");
                await Task.Delay(_options.PollInterval, stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private async Task DispatchMessageAsync(OutboxPendingMessage message, int maxAttempts, CancellationToken cancellationToken)
    {
        var attemptTime = _clock.UtcNow;

        if (message.AttemptCount >= maxAttempts)
        {
            _logger.LogWarning("Skipping outbox message {MessageId} because it exceeded max attempts ({Attempts}/{MaxAttempts}).", message.MessageId, message.AttemptCount, maxAttempts);
            return;
        }

        var publisher = ResolvePublisher(message);
        if (publisher is null)
        {
            var error = $"No outbox publisher registered for destination '{message.Destination ?? "<default>"}'.";
            _logger.LogError(error + " MessageId={MessageId}", message.MessageId);
            await _store.RecordDispatchOutcomeAsync(message.MessageId, attemptTime, succeeded: false, error, cancellationToken).ConfigureAwait(false);
            return;
        }

        try
        {
            await publisher.PublishAsync(message, cancellationToken).ConfigureAwait(false);
            await _store.RecordDispatchOutcomeAsync(message.MessageId, attemptTime, succeeded: true, cancellationToken: cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Dispatched outbox message {MessageId} via {Publisher}.", message.MessageId, publisher.Name);
        }
        catch (Exception ex)
        {
            var error = ex.Message;
            await _store.RecordDispatchOutcomeAsync(message.MessageId, attemptTime, succeeded: false, error, cancellationToken).ConfigureAwait(false);

            var attempt = message.AttemptCount + 1;
            if (attempt >= maxAttempts)
            {
                _logger.LogError(ex, "Outbox message {MessageId} failed after {Attempts} attempts and will not be retried.", message.MessageId, attempt);
            }
            else
            {
                _logger.LogWarning(ex, "Outbox message {MessageId} failed attempt {Attempts}/{MaxAttempts}.", message.MessageId, attempt, maxAttempts);
            }
        }
    }

    private IOutboxPublisher? ResolvePublisher(OutboxPendingMessage message)
    {
        if (_publishers.Count == 0)
        {
            return null;
        }

        var destination = message.Destination ?? _options.DefaultPublisherName;

        if (destination is not null && _publishers.TryGetValue(destination, out var named))
        {
            return named;
        }

        // fallback: if a single publisher is registered, use it
        if (_publishers.Count == 1)
        {
            return _publishers.Values.First();
        }

        return null;
    }
}

