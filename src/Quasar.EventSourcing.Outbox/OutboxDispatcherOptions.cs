namespace Quasar.EventSourcing.Outbox;

/// <summary>
/// Configuration for the background dispatcher that drains the outbox store.
/// </summary>
public sealed class OutboxDispatcherOptions
{
    /// <summary>
    /// Gets or sets the number of messages fetched per polling cycle.
    /// </summary>
    public int BatchSize { get; set; } = 50;

    /// <summary>
    /// Gets or sets how long the dispatcher waits before polling again when no messages are available.
    /// </summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets the maximum number of delivery attempts before a message is considered failed.
    /// </summary>
    public int MaxAttempts { get; set; } = 5;

    /// <summary>
    /// Gets or sets an optional publisher name to use when a message destination is not specified.
    /// </summary>
    public string? DefaultPublisherName { get; set; }
}

