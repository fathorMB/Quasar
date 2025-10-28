using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quasar.EventSourcing.Abstractions;
using Quasar.EventSourcing.Outbox;

namespace Quasar.EventSourcing.Outbox.EfCore;

/// <summary>
/// Registration helpers for the EF Core outbox/inbox stores.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers EF Core backed implementations of the outbox and inbox stores using <typeparamref name="TContext"/>.
    /// </summary>
    public static IServiceCollection AddQuasarOutboxEfCore<TContext>(this IServiceCollection services)
        where TContext : OutboxDbContext
    {
        services.AddQuasarOutboxCore();
        services.AddScoped<OutboxDbContext, TContext>();
        services.TryAddScoped<IOutboxStore, EfCoreOutboxStore>();
        services.TryAddScoped<IInboxStore, EfCoreInboxStore>();
        return services;
    }
}


