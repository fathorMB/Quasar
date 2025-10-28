using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quasar.EventSourcing.Outbox;

namespace Quasar.EventSourcing.Outbox.Kafka;

/// <summary>
/// Registration helpers for Kafka outbox publishers.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a Kafka-backed <see cref="IOutboxPublisher"/>.
    /// </summary>
    public static IServiceCollection AddKafkaOutboxPublisher(this IServiceCollection services, Action<KafkaOutboxPublisherOptions> configure)
    {
        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        services.AddQuasarOutboxCore();
        services.Configure(configure);
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOutboxPublisher, KafkaOutboxPublisher>());
        return services;
    }
}

