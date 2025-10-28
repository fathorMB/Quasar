namespace Quasar.EventSourcing.Abstractions;

/// <summary>
/// Represents an incoming message processed by the application for idempotency tracking.
/// </summary>
public sealed record InboxMessage(
    string Source,
    string MessageId,
    DateTime ReceivedUtc,
    string? Hash = null,
    DateTime? ProcessedUtc = null);

