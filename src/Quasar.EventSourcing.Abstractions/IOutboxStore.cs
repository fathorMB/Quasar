namespace Quasar.EventSourcing.Abstractions;

/// <summary>
/// Persists outbox messages so that they can be published to external transports reliably.
/// </summary>
public interface IOutboxStore
{
    /// <summary>
    /// Persists the supplied <paramref name="messages"/> in the underlying store.
    /// </summary>
    Task EnqueueAsync(IReadOnlyList<OutboxMessage> messages, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves pending messages ordered by creation time.
    /// </summary>
    Task<IReadOnlyList<OutboxPendingMessage>> GetPendingAsync(int batchSize, int maxAttempts, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records the outcome of a dispatch attempt.
    /// </summary>
    Task RecordDispatchOutcomeAsync(Guid messageId, DateTime attemptUtc, bool succeeded, string? error = null, CancellationToken cancellationToken = default);
}
