using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quasar.EventSourcing.Outbox;

namespace Quasar.EventSourcing.Outbox.RabbitMq;

/// <summary>
/// Registration helpers for RabbitMQ outbox publishers.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a RabbitMQ-backed <see cref="IOutboxPublisher"/>.
    /// </summary>
    public static IServiceCollection AddRabbitMqOutboxPublisher(this IServiceCollection services, Action<RabbitMqOutboxPublisherOptions> configure)
    {
        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        services.AddQuasarOutboxCore();
        services.Configure(configure);
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOutboxPublisher, RabbitMqOutboxPublisher>());
        return services;
    }
}

