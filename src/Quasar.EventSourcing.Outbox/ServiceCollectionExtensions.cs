using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Quasar.Core;
using Quasar.EventSourcing.Abstractions;

namespace Quasar.EventSourcing.Outbox;

/// <summary>
/// Dependency injection helpers for the outbox components.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the default outbox message factory and supporting infrastructure.
    /// </summary>
    public static IServiceCollection AddQuasarOutboxCore(this IServiceCollection services)
    {
        services.TryAddSingleton<IClock, SystemClock>();
        services.TryAddSingleton<IOutboxMessageFactory, DefaultOutboxMessageFactory>();
        return services;
    }

    /// <summary>
    /// Registers the outbox dispatcher background service.
    /// </summary>
    public static IServiceCollection AddQuasarOutboxDispatcher(this IServiceCollection services, Action<OutboxDispatcherOptions>? configure = null)
    {
        services.AddQuasarOutboxCore();
        if (configure is not null)
        {
            services.Configure(configure);
        }
        else
        {
            services.TryAddSingleton<IOptions<OutboxDispatcherOptions>>(_ => Options.Create(new OutboxDispatcherOptions()));
        }

        services.AddHostedService<OutboxDispatcher>();
        return services;
    }

    /// <summary>
    /// Registers the inbox cleanup background service.
    /// </summary>
    public static IServiceCollection AddQuasarInboxCleanup(this IServiceCollection services, Action<InboxCleanupOptions>? configure = null)
    {
        if (configure is not null)
        {
            services.Configure(configure);
        }
        else
        {
            services.TryAddSingleton<IOptions<InboxCleanupOptions>>(_ => Options.Create(new InboxCleanupOptions()));
        }

        services.AddHostedService<InboxCleanupService>();
        return services;
    }
}
