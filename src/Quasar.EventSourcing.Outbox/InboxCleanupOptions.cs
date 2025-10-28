namespace Quasar.EventSourcing.Outbox;

/// <summary>
/// Configures the background cleanup that prunes processed inbox entries.
/// </summary>
public sealed class InboxCleanupOptions
{
    /// <summary>
    /// Gets or sets how frequently the inbox cleanup job runs.
    /// </summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets how long processed messages are retained before being removed.
    /// </summary>
    public TimeSpan Retention { get; set; } = TimeSpan.FromHours(24);
}

