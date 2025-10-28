
using Quasar.EventSourcing.Abstractions;

namespace Quasar.EventSourcing.Outbox;

/// <summary>
/// Represents a transport-specific publisher that sends outbox messages to an external system.
/// </summary>
public interface IOutboxPublisher
{
    /// <summary>
    /// Gets the logical name of the publisher used to match message destinations.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Publishes the provided <paramref name="message"/> to the underlying transport.
    /// </summary>
    Task PublishAsync(OutboxPendingMessage message, CancellationToken cancellationToken = default);
}
