using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quasar.EventSourcing.Abstractions;
using Quasar.EventSourcing.Outbox;

namespace Quasar.EventSourcing.Outbox.Kafka;

/// <summary>
/// Kafka implementation of <see cref="IOutboxPublisher"/>.
/// </summary>
public sealed class KafkaOutboxPublisher : IOutboxPublisher, IDisposable
{
    private const string TopicMetadataKey = "kafka.topic";

    private readonly ILogger<KafkaOutboxPublisher> _logger;
    private readonly KafkaOutboxPublisherOptions _options;
    private readonly IProducer<string, string> _producer;

    public KafkaOutboxPublisher(IOptions<KafkaOutboxPublisherOptions> options, ILogger<KafkaOutboxPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.BootstrapServers))
        {
            throw new InvalidOperationException("Kafka bootstrap servers must be configured.");
        }

        var config = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public string Name => _options.Name;

    public async Task PublishAsync(OutboxPendingMessage message, CancellationToken cancellationToken = default)
    {
        var topic = ResolveTopic(message);
        var kafkaMessage = new Message<string, string>
        {
            Key = message.StreamId.ToString(),
            Value = message.Payload,
            Headers = BuildHeaders(message)
        };

        var delivery = await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken).ConfigureAwait(false);
        _logger.LogDebug("Kafka delivery succeeded for outbox message {MessageId} -> {Topic}@{Partition}:{Offset}", message.MessageId, delivery.Topic, delivery.Partition, delivery.Offset);
    }

    private Headers BuildHeaders(OutboxPendingMessage message)
    {
        var headers = new Headers
        {
            new Header("event-name", Encoding.UTF8.GetBytes(message.EventName)),
            new Header("stream-id", Encoding.UTF8.GetBytes(message.StreamId.ToString()))
        };

        if (message.Metadata is { Count: > 0 })
        {
            foreach (var (key, value) in message.Metadata)
            {
                headers.Add(key, Encoding.UTF8.GetBytes(value));
            }
        }

        return headers;
    }

    private string ResolveTopic(OutboxPendingMessage message)
    {
        if (message.Metadata is { Count: > 0 } && message.Metadata.TryGetValue(TopicMetadataKey, out var topic) && !string.IsNullOrWhiteSpace(topic))
        {
            return ApplyPrefix(topic);
        }

        if (!string.IsNullOrWhiteSpace(_options.DefaultTopic))
        {
            return ApplyPrefix(_options.DefaultTopic);
        }

        return ApplyPrefix(message.EventName);
    }

    private string ApplyPrefix(string topic)
    {
        if (string.IsNullOrWhiteSpace(_options.TopicPrefix))
        {
            return topic;
        }

        return $"{_options.TopicPrefix}{topic}";
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}

