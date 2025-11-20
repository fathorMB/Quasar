using Microsoft.Extensions.DependencyInjection;
using Quasar.Cqrs;
using Quasar.EventSourcing.Abstractions;
using Quasar.EventSourcing.InMemory;
using Quasar.EventSourcing.Outbox;
using Quasar.EventSourcing.SqlServer;
using Quasar.EventSourcing.Sqlite;
using Quasar.Persistence.Relational.EfCore;
using Quasar.Projections.Abstractions;
using Quasar.Projections.SqlServer;
using Quasar.Projections.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quasar.Security;
using Quasar.Persistence.Abstractions;
using Quasar.Persistence.TimeSeries.Timescale;

namespace Quasar.Web;

/// <summary>
/// Extension methods for wiring Quasar components into an <see cref="IServiceCollection"/>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the mediator pipeline and, optionally, the default behaviors.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="addDefaultBehaviors">When <see langword="true"/> registers validation, transaction, and authorization behaviors.</param>
    public static IServiceCollection AddQuasarMediator(this IServiceCollection services, bool addDefaultBehaviors = true)
    {
        services.AddScoped<IMediator, Mediator>();
        if (addDefaultBehaviors)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        }
        return services;
    }

    /// <summary>
    /// Registers event-sourcing repositories and supporting infrastructure.
    /// </summary>
    public static IServiceCollection AddQuasarEventSourcingCore(this IServiceCollection services)
    {
        services.AddQuasarOutboxCore();
        services.AddScoped(typeof(IEventSourcedRepository<>), typeof(EventSourcedRepository<>));
        return services;
    }

    /// <summary>
    /// Registers a <see cref="IEventSerializer"/> using the provided type map.
    /// </summary>
    public static IServiceCollection AddQuasarEventSerializer(this IServiceCollection services, IEventTypeMap typeMap)
    {
        services.AddSingleton<IEventSerializer>(sp => new SystemTextJsonEventSerializer(typeMap));
        return services;
    }

    /// <summary>
    /// Configures SQL Server backed event store components.
    /// </summary>
    public static IServiceCollection UseSqlServerEventStore(this IServiceCollection services, SqlEventStoreOptions options)
    {
        services.AddSingleton(options);
        services.AddScoped<IEventStore, SqlEventStore>();
        services.AddScoped<ISnapshotStore, SqlSnapshotStore>();
        return services;
    }

    /// <summary>
    /// Registers the in-memory event store implementation. Intended for development and testing scenarios.
    /// </summary>
    public static IServiceCollection UseInMemoryEventStore(this IServiceCollection services)
    {
        services.AddSingleton<IEventStore, InMemoryEventStore>();
        return services;
    }

    /// <summary>
    /// Registers the SQL Server command transaction implementation.
    /// </summary>
    public static IServiceCollection UseSqlServerCommandTransaction(this IServiceCollection services)
    {
        services.AddScoped<ICommandTransaction, SqlCommandTransaction>();
        return services;
    }

    /// <summary>
    /// Configures SQLite backed event store components.
    /// </summary>
    public static IServiceCollection UseSqliteEventStore(this IServiceCollection services, SqliteEventStoreOptions options)
    {
        services.AddSingleton(options);
        services.AddScoped<IEventStore, SqliteEventStore>();
        services.AddScoped<ISnapshotStore, SqliteSnapshotStore>();
        return services;
    }

    /// <summary>
    /// Registers the SQLite command transaction implementation.
    /// </summary>
    public static IServiceCollection UseSqliteCommandTransaction(this IServiceCollection services)
    {
        services.AddScoped<ICommandTransaction, SqliteCommandTransaction>();
        return services;
    }

    /// <summary>
    /// Configures EF Core-backed read models using SQL Server.
    /// </summary>
    public static IServiceCollection UseEfCoreSqlServerReadModels<TStore>(this IServiceCollection services, string connectionString, bool registerRepositories = true)
        where TStore : class, IReadModelStoreMarker
    {
        services.TryAddSingleton<IReadModelModelSource, ReadModelModelSource>();

        services.AddDbContext<ReadModelContext<TStore>>(o =>
        {
            o.UseSqlServer(connectionString);
            o.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IReadModelSchemaInitializer<ReadModelContext<TStore>>, SqlServerReadModelSchemaInitializer<ReadModelContext<TStore>>>();
        services.AddSingleton<IReadModelSchemaBootstrapper, ReadModelSchemaBootstrapper<ReadModelContext<TStore>>>();
        services.AddHostedService<ReadModelSchemaInitializerHostedService<ReadModelContext<TStore>>>();

        if (registerRepositories)
        {
            services.AddScoped<ReadModelContext>(sp => sp.GetRequiredService<ReadModelContext<TStore>>());
            services.AddScoped(typeof(IReadRepository<>), typeof(EfReadRepository<>));
        }

        return services;
    }

    /// <summary>
    /// Configures EF Core-backed read models using SQLite.
    /// </summary>
    public static IServiceCollection UseEfCoreSqliteReadModels<TStore>(this IServiceCollection services, string connectionString, bool registerRepositories = true)
        where TStore : class, IReadModelStoreMarker
    {
        services.TryAddSingleton<IReadModelModelSource, ReadModelModelSource>();

        services.AddDbContext<ReadModelContext<TStore>>(o =>
        {
            o.UseSqlite(connectionString);
            o.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IReadModelSchemaInitializer<ReadModelContext<TStore>>, SqliteReadModelSchemaInitializer<ReadModelContext<TStore>>>();
        services.AddSingleton<IReadModelSchemaBootstrapper, ReadModelSchemaBootstrapper<ReadModelContext<TStore>>>();
        services.AddHostedService<ReadModelSchemaInitializerHostedService<ReadModelContext<TStore>>>();

        if (registerRepositories)
        {
            services.AddScoped<ReadModelContext>(sp => sp.GetRequiredService<ReadModelContext<TStore>>());
            services.AddScoped(typeof(IReadRepository<>), typeof(EfReadRepository<>));
        }

        return services;
    }

    /// <summary>
    /// Placeholder for future projection configuration.
    /// </summary>
    public static IServiceCollection AddProjections(this IServiceCollection services)
    {
        return services;
    }

    /// <summary>
    /// Configures SQL Server backed checkpoint storage for projections.
    /// </summary>
    public static IServiceCollection UseSqlServerProjectionCheckpoints(this IServiceCollection services, Func<DbConnection> connectionFactory)
    {
        services.AddSingleton<ICheckpointStore>(sp => new SqlServerCheckpointStore(connectionFactory));
        return services;
    }

    /// <summary>
    /// Configures SQLite backed checkpoint storage for projections.
    /// </summary>
    public static IServiceCollection UseSqliteProjectionCheckpoints(this IServiceCollection services, Func<DbConnection> connectionFactory)
    {
        services.AddSingleton<ICheckpointStore>(sp => new SqliteCheckpointStore(connectionFactory));
        return services;
    }

    /// <summary>
    /// Registers a background service that polls the event store and dispatches projections.
    /// </summary>
    public static IServiceCollection AddPollingProjector(this IServiceCollection services, string projectorName, IEnumerable<Guid> streamIds, TimeSpan? interval = null)
    {
        services.AddHostedService(sp => new PollingProjector(
            sp,
            projectorName,
            streamIds,
            interval));
        return services;
    }

    /// <summary>
    /// Adds TimescaleDB based time series support.
    /// </summary>
    public static IServiceCollection UseTimescaleTimeSeries(this IServiceCollection services, Action<TimescaleOptions> configure)
    {
        TimescaleServiceCollectionExtensions.UseTimescaleTimeSeries(services, configure);
        return services;
    }

    /// <summary>
    /// Configures the Quasar UI settings (application name, theme, etc.).
    /// </summary>
    public static IServiceCollection AddQuasarUi(this IServiceCollection services, Action<UiSettings>? configure = null)
    {
        var settings = new UiSettings();
        configure?.Invoke(settings);
        services.AddSingleton(settings);
        return services;
    }
}
