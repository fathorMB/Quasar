namespace Quasar.EventSourcing.Abstractions;

/// <summary>
/// Tracks inbound messages to guarantee idempotent processing.
/// </summary>
public interface IInboxStore
{
    /// <summary>
    /// Attempts to record an inbound message. Returns <see langword="false"/> when a duplicate exists.
    /// </summary>
    Task<bool> TryEnsureProcessedAsync(InboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes messages older than <paramref name="cutoffUtc"/> from the backing store.
    /// </summary>
    Task PurgeAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default);
}

