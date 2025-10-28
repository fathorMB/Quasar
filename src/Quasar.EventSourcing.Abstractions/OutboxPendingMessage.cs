namespace Quasar.EventSourcing.Abstractions;

/// <summary>
/// Represents a message pending publication from the outbox store.
/// </summary>
public sealed record OutboxPendingMessage(
    Guid MessageId,
    Guid StreamId,
    int StreamVersion,
    string EventName,
    string Payload,
    DateTime CreatedUtc,
    int AttemptCount,
    DateTime? LastAttemptUtc,
    string? Destination,
    string? LastError,
    IReadOnlyDictionary<string, string>? Metadata);

