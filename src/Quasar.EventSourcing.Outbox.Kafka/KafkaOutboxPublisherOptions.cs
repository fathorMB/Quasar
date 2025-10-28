namespace Quasar.EventSourcing.Outbox.Kafka;

/// <summary>
/// Provides configuration for the Kafka outbox publisher.
/// </summary>
public sealed class KafkaOutboxPublisherOptions
{
    /// <summary>
    /// Gets or sets the logical name used when resolving this publisher. Defaults to "kafka".
    /// </summary>
    public string Name { get; set; } = "kafka";

    /// <summary>
    /// Gets or sets the bootstrap servers used to connect to the Kafka cluster.
    /// </summary>
    public string BootstrapServers { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default topic to publish messages to when none is specified via metadata.
    /// </summary>
    public string DefaultTopic { get; set; } = string.Empty;

    /// <summary>
    /// Optional prefix appended to the topic resolved from the outbox message metadata.
    /// </summary>
    public string? TopicPrefix { get; set; }
}

