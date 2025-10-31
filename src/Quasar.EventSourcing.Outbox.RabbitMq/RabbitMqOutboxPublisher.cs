
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quasar.EventSourcing.Abstractions;
using Quasar.EventSourcing.Outbox;
using RabbitMQ.Client;
using System.Threading;

namespace Quasar.EventSourcing.Outbox.RabbitMq;

/// <summary>
/// RabbitMQ implementation of <see cref="IOutboxPublisher"/>.
/// </summary>
public sealed class RabbitMqOutboxPublisher : IOutboxPublisher, IDisposable, IAsyncDisposable
{
    private const string RoutingKeyMetadataKey = "rabbitmq.routing-key";

    private readonly RabbitMqOutboxPublisherOptions _options;
    private readonly ILogger<RabbitMqOutboxPublisher> _logger;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;
    private bool _disposed;

    public RabbitMqOutboxPublisher(IOptions<RabbitMqOutboxPublisherOptions> options, ILogger<RabbitMqOutboxPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public string Name => _options.Name;

    public async Task PublishAsync(OutboxPendingMessage message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var channel = await EnsureChannelAsync(cancellationToken).ConfigureAwait(false);
        var body = Encoding.UTF8.GetBytes(message.Payload);
        var routingKey = ResolveRoutingKey(message);

        var properties = new BasicProperties
        {
            Persistent = true,
            MessageId = message.MessageId.ToString(),
            Type = message.EventName,
            Headers = BuildHeaders(message)
        };

        await channel.BasicPublishAsync(
            exchange: _options.Exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        _logger.LogDebug("RabbitMQ delivery succeeded for outbox message {MessageId} -> {Exchange}:{RoutingKey}", message.MessageId, _options.Exchange, routingKey);
    }

    private async Task<IChannel> EnsureChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
        {
            return _channel;
        }

        await _initializationLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_channel is not null)
            {
                return _channel;
            }

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            return _channel;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    private static IDictionary<string, object?>? BuildHeaders(OutboxPendingMessage message)
    {
        if (message.Metadata is null || message.Metadata.Count == 0)
        {
            return null;
        }

        var headers = new Dictionary<string, object?>(message.Metadata.Count, StringComparer.OrdinalIgnoreCase);
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
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        if (_channel is not null)
        {
            switch (_channel)
            {
                case IAsyncDisposable asyncChannel:
                    await asyncChannel.DisposeAsync().ConfigureAwait(false);
                    break;
                default:
                    _channel.Dispose();
                    break;
            }
        }

        if (_connection is not null)
        {
            switch (_connection)
            {
                case IAsyncDisposable asyncConnection:
                    await asyncConnection.DisposeAsync().ConfigureAwait(false);
                    break;
                default:
                    _connection.Dispose();
                    break;
            }
        }

        _initializationLock.Dispose();
    }
}
