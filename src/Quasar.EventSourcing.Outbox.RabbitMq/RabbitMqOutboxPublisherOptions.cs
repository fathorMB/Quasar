namespace Quasar.EventSourcing.Outbox.RabbitMq;

/// <summary>
/// Configures the RabbitMQ outbox publisher.
/// </summary>
public sealed class RabbitMqOutboxPublisherOptions
{
    /// <summary>
    /// Gets or sets the logical name used to resolve the publisher.
    /// </summary>
    public string Name { get; set; } = "rabbitmq";

    /// <summary>
    /// Gets or sets the host name of the RabbitMQ broker.
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets the username used when connecting to RabbitMQ.
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// Gets or sets the password used when connecting to RabbitMQ.
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Gets or sets the virtual host to connect to. Defaults to "/".
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Gets or sets the exchange that messages are published to.
    /// </summary>
    public string Exchange { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional routing key to use when none is provided in metadata.
    /// </summary>
    public string? RoutingKey { get; set; }
}

