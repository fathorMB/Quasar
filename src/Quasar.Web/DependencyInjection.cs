using Microsoft.Extensions.DependencyInjection;
using Quasar.Cqrs;
using Quasar.EventSourcing.Abstractions;
using Quasar.EventSourcing.InMemory;
using Quasar.EventSourcing.SqlServer;
using Quasar.EventSourcing.Sqlite;
using Quasar.Persistence.Relational.EfCore;
using Quasar.Projections.Abstractions;
using Quasar.Projections.SqlServer;
using Quasar.Projections.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Quasar.Security;
using Quasar.Persistence.Abstractions;

namespace Quasar.Web;

public static class DependencyInjection
{
    // CQRS + Mediator + default behaviors
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

    // Event sourcing core services
    public static IServiceCollection AddQuasarEventSourcingCore(this IServiceCollection services)
    {
        services.AddScoped(typeof(IEventSourcedRepository<>), typeof(EventSourcedRepository<>));
        return services;
    }

    public static IServiceCollection AddQuasarEventSerializer(this IServiceCollection services, IEventTypeMap typeMap)
    {
        services.AddSingleton<IEventSerializer>(sp => new SystemTextJsonEventSerializer(typeMap));
        return services;
    }

    // Providers
    public static IServiceCollection UseSqlServerEventStore(this IServiceCollection services, SqlEventStoreOptions options)
    {
        services.AddSingleton(options);
        services.AddScoped<IEventStore, SqlEventStore>();
        services.AddScoped<ISnapshotStore, SqlSnapshotStore>();
        return services;
    }

    public static IServiceCollection UseInMemoryEventStore(this IServiceCollection services)
    {
        services.AddSingleton<IEventStore, InMemoryEventStore>();
        return services;
    }

    public static IServiceCollection UseSqlServerCommandTransaction(this IServiceCollection services)
    {
        services.AddScoped<ICommandTransaction, SqlCommandTransaction>();
        return services;
    }

    public static IServiceCollection UseSqliteEventStore(this IServiceCollection services, SqliteEventStoreOptions options)
    {
        services.AddSingleton(options);
        services.AddScoped<IEventStore, SqliteEventStore>();
        services.AddScoped<ISnapshotStore, SqliteSnapshotStore>();
        return services;
    }

    public static IServiceCollection UseSqliteCommandTransaction(this IServiceCollection services)
    {
        services.AddScoped<ICommandTransaction, SqliteCommandTransaction>();
        return services;
    }

    // Read models via EF Core
    public static IServiceCollection UseEfCoreSqlServerReadModels<TContext>(this IServiceCollection services, string connectionString)
        where TContext : ReadModelContext
    {
        services.AddDbContext<TContext>(o =>
        {
            o.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(TContext).Assembly.FullName));
            o.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });
        services.AddScoped<ReadModelContext>(sp => sp.GetRequiredService<TContext>());
        services.AddScoped(typeof(IReadRepository<>), typeof(EfReadRepository<>));
        return services;
    }

    public static IServiceCollection UseEfCoreSqliteReadModels<TContext>(this IServiceCollection services, string connectionString)
        where TContext : ReadModelContext
    {
        services.AddDbContext<TContext>(o =>
        {
            o.UseSqlite(connectionString, b => b.MigrationsAssembly(typeof(TContext).Assembly.FullName));
            o.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });
        services.AddScoped<ReadModelContext>(sp => sp.GetRequiredService<TContext>());
        services.AddScoped(typeof(IReadRepository<>), typeof(EfReadRepository<>));
        return services;
    }

    // Projections
    public static IServiceCollection AddProjections(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection UseSqlServerProjectionCheckpoints(this IServiceCollection services, Func<DbConnection> connectionFactory)
    {
        services.AddSingleton<ICheckpointStore>(sp => new SqlServerCheckpointStore(connectionFactory));
        return services;
    }

    public static IServiceCollection UseSqliteProjectionCheckpoints(this IServiceCollection services, Func<DbConnection> connectionFactory)
    {
        services.AddSingleton<ICheckpointStore>(sp => new SqliteCheckpointStore(connectionFactory));
        return services;
    }

    public static IServiceCollection AddPollingProjector(this IServiceCollection services, string projectorName, IEnumerable<Guid> streamIds, TimeSpan? interval = null)
    {
        services.AddHostedService(sp => new PollingProjector(
            sp,
            projectorName,
            streamIds,
            interval));
        return services;
    }
}
