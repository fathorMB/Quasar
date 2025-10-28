
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quasar.EventSourcing.Abstractions;
using Quasar.EventSourcing.Outbox;
using RabbitMQ.Client;

namespace Quasar.EventSourcing.Outbox.RabbitMq;

/// <summary>
/// RabbitMQ implementation of <see cref="IOutboxPublisher"/>.
/// </summary>
public sealed class RabbitMqOutboxPublisher : IOutboxPublisher, IDisposable
{
    private const string RoutingKeyMetadataKey = "rabbitmq.routing-key";

    private readonly RabbitMqOutboxPublisherOptions _options;
    private readonly ILogger<RabbitMqOutboxPublisher> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqOutboxPublisher(IOptions<RabbitMqOutboxPublisherOptions> options, ILogger<RabbitMqOutboxPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public string Name => _options.Name;

    public Task PublishAsync(OutboxPendingMessage message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var body = Encoding.UTF8.GetBytes(message.Payload);
        var routingKey = ResolveRoutingKey(message);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.MessageId = message.MessageId.ToString();
        properties.Type = message.EventName;
        properties.Headers = BuildHeaders(message);

        _channel.BasicPublish(
            exchange: _options.Exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body);

        _logger.LogDebug("RabbitMQ delivery succeeded for outbox message {MessageId} -> {Exchange}:{RoutingKey}", message.MessageId, _options.Exchange, routingKey);
        return Task.CompletedTask;
    }

    private IDictionary<string, object>? BuildHeaders(OutboxPendingMessage message)
    {
        if (message.Metadata is null || message.Metadata.Count == 0)
        {
            return null;
        }

        var headers = new Dictionary<string, object>(message.Metadata.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in message.Metadata)
        {
            headers[key] = Encoding.UTF8.GetBytes(value);
        }

        return headers;
    }

    private string ResolveRoutingKey(OutboxPendingMessage message)
    {
        if (message.Metadata is { Count: > 0 } && message.Metadata.TryGetValue(RoutingKeyMetadataKey, out var routingKey) && !string.IsNullOrWhiteSpace(routingKey))
        {
            return routingKey;
        }

        if (!string.IsNullOrWhiteSpace(_options.RoutingKey))
        {
            return _options.RoutingKey;
        }

        return message.EventName;
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
