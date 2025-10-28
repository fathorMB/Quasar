using Quasar.Domain;

namespace Quasar.EventSourcing.Abstractions;

/// <summary>
/// Represents a domain event captured for deferred publication via an integration message bus.
/// </summary>
public sealed record OutboxMessage(
    Guid MessageId,
    Guid StreamId,
    int StreamVersion,
    IDomainEvent Event,
    DateTime CreatedUtc,
    string EventName,
    string Payload,
    IReadOnlyDictionary<string, string>? Metadata = null,
    string? Destination = null);

