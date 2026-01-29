using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quasar.Cqrs;
using Quasar.EventSourcing.Abstractions;
using Quasar.EventSourcing.InMemory;
using Quasar.EventSourcing.Outbox;
using Quasar.EventSourcing.SqlServer;
using Quasar.EventSourcing.Sqlite;
using Quasar.Persistence.Abstractions;
using Quasar.Persistence.Relational.EfCore;
using Quasar.Persistence.TimeSeries.Timescale;
using Quasar.Projections.Abstractions;
using Quasar.Projections.SqlServer;
using Quasar.Projections.Sqlite;
using Quasar.Security;
using Quasar.Telemetry;
using System.Data.Common;
using System.Linq;
using System.Reflection;

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
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
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
    public static IServiceCollection UseEfCoreSqlServerReadModels<TStore>(this IServiceCollection services, string connectionString, bool registerRepositories = true, bool registerSchemaServices = true)
        where TStore : class, IReadModelStoreMarker
    {
        services.TryAddSingleton<IReadModelModelSource, ReadModelModelSource>();

        services.AddDbContext<ReadModelContext<TStore>>(o =>
        {
            o.UseSqlServer(connectionString);
            o.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddDbContextFactory<ReadModelContext<TStore>>(o =>
        {
            o.UseSqlServer(connectionString);
            o.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        }, ServiceLifetime.Scoped);

        if (registerSchemaServices)
        {
            services.AddScoped<IReadModelSchemaInitializer<ReadModelContext<TStore>>, SqlServerReadModelSchemaInitializer<ReadModelContext<TStore>>>();
            services.AddSingleton<IReadModelSchemaBootstrapper, ReadModelSchemaBootstrapper<ReadModelContext<TStore>>>();
            services.AddHostedService<ReadModelSchemaInitializerHostedService<ReadModelContext<TStore>>>();
        }

        if (registerRepositories)
        {
            services.AddScoped<ReadModelContext>(sp => sp.GetRequiredService<ReadModelContext<TStore>>());
            services.AddScoped(typeof(IReadRepository<>), typeof(EfReadRepository<>));
        }

        services.AddReadModelDefinitionsFromAssembliesForStore<TStore>();
        services.TryAddScoped<IReadModelSchemaScriptGenerator<ReadModelContext<TStore>>, ReadModelSchemaScriptGenerator<ReadModelContext<TStore>>>();

        return services;
    }

    /// <summary>
    /// Configures EF Core-backed read models using SQLite.
    /// </summary>
    public static IServiceCollection UseEfCoreSqliteReadModels<TStore>(this IServiceCollection services, string connectionString, bool registerRepositories = true, bool registerSchemaServices = true)
        where TStore : class, IReadModelStoreMarker
    {
        services.TryAddSingleton<IReadModelModelSource, ReadModelModelSource>();

        services.AddDbContext<ReadModelContext<TStore>>(o =>
        {
            o.UseSqlite(connectionString);
            o.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddDbContextFactory<ReadModelContext<TStore>>(o =>
        {
            o.UseSqlite(connectionString);
            o.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        }, ServiceLifetime.Scoped);

        if (registerSchemaServices)
        {
            services.AddScoped<IReadModelSchemaInitializer<ReadModelContext<TStore>>, SqliteReadModelSchemaInitializer<ReadModelContext<TStore>>>();
            services.AddSingleton<IReadModelSchemaBootstrapper, ReadModelSchemaBootstrapper<ReadModelContext<TStore>>>();
            services.AddHostedService<ReadModelSchemaInitializerHostedService<ReadModelContext<TStore>>>();
        }

        if (registerRepositories)
        {
            services.AddScoped<ReadModelContext>(sp => sp.GetRequiredService<ReadModelContext<TStore>>());
            services.AddScoped(typeof(IReadRepository<>), typeof(EfReadRepository<>));
        }

        services.AddReadModelDefinitionsFromAssembliesForStore<TStore>();
        services.TryAddScoped<IReadModelSchemaScriptGenerator<ReadModelContext<TStore>>, ReadModelSchemaScriptGenerator<ReadModelContext<TStore>>>();

        return services;
    }

    /// <summary>
    /// Scans assemblies and registers all <see cref="IProjection{TEvent}"/> implementations.
    /// Registered as scoped both as their concrete type and as <see cref="object"/> (for <see cref="PollingProjector"/> resolution).
    /// </summary>
    public static IServiceCollection AddProjections(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies is null || assemblies.Length == 0)
        {
            assemblies = AppDomain.CurrentDomain.GetAssemblies();
        }

        foreach (var assembly in assemblies)
        {
            foreach (var type in SafeGetTypes(assembly))
            {
                if (type is null || type.IsAbstract || type.IsInterface)
                    continue;

                if (!ImplementsProjection(type))
                    continue;

                services.TryAdd(ServiceDescriptor.Scoped(type, type));
                services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(object), type));
            }
        }

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

    /// <summary>
    /// Configures Quasar Identity with SQLite infrastructure (event store, read models, projections).
    /// <summary>
    /// Configures Quasar Identity with SQLite infrastructure (event store, read models, projections).
    /// Automatically registers event type map, initializes schema, and wires up projections.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">
    /// Optional SQLite connection string. If not provided, attempts to read from configuration key "ConnectionStrings:QuasarIdentity".
    /// Defaults to "Data Source=quasar.identity.db" if not found.
    /// </param>
    /// <param name="configureEventStore">Optional callback to customize event store options.</param>
    public static IServiceCollection AddQuasarIdentitySqliteInfrastructure(
        this IServiceCollection services,
        string? connectionString = null,
        Action<SqliteEventStoreOptions>? configureEventStore = null,
        bool initializeSchema = true)
    {
        // Resolve connection string with fallback to configuration
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var sp = services.BuildServiceProvider();
            var configuration = sp.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            connectionString = configuration?["ConnectionStrings:QuasarIdentity"] ?? "Data Source=quasar.identity.db";
        }

        var resolvedConnectionString = ResolveSqliteConnectionString(connectionString);

        // Register event type map only if no serializer exists
        var existingSerializer = services.FirstOrDefault(d => d.ServiceType == typeof(IEventSerializer));
        if (existingSerializer == null)
        {
            services.AddQuasarEventSerializer(Quasar.Identity.IdentityEventTypeMap.Create());
        }

        // Event store configuration
        var sqliteOptions = new SqliteEventStoreOptions
        {
            ConnectionFactory = () => new Microsoft.Data.Sqlite.SqliteConnection(resolvedConnectionString)
        };
        configureEventStore?.Invoke(sqliteOptions);

        // Initialize schema synchronously (required before DI completes)
        if (initializeSchema)
        {
            try
            {
                SqliteEventStoreInitializer.EnsureSchemaAsync(sqliteOptions).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[Quasar] ERROR Initializing Event Store Schema: {ex}");
            }
        }

        services.UseSqliteEventStore(sqliteOptions);
        services.UseSqliteCommandTransaction();

        // Read models
        services.UseEfCoreSqliteReadModels<Quasar.Identity.Persistence.Relational.EfCore.IdentityReadModelStore>(
            resolvedConnectionString,
            registerRepositories: false,
            registerSchemaServices: initializeSchema);

        // Projections
        services.AddScoped<object, Quasar.Identity.Persistence.Relational.EfCore.IdentityProjections>();

        return services;
    }

    /// <summary>
    /// Configures Quasar Identity with SQL Server infrastructure (event store, read models, projections).
    /// Automatically registers event type map, initializes schema, and wires up projections.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">
    /// Optional SQL Server connection string. If not provided, attempts to read from configuration key "ConnectionStrings:QuasarIdentity".
    /// Required if not found in configuration.
    /// </param>
    /// <param name="configureEventStore">Optional callback to customize event store options.</param>
    public static IServiceCollection AddQuasarIdentitySqlServerInfrastructure(
        this IServiceCollection services,
        string? connectionString = null,
        Action<SqlEventStoreOptions>? configureEventStore = null,
        bool initializeSchema = true)
    {
        // Resolve connection string with  fallback to configuration
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var sp = services.BuildServiceProvider();
            var configuration = sp.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            connectionString = configuration?["ConnectionStrings:QuasarIdentity"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "SQL Server connection string is required. Provide it explicitly or configure 'ConnectionStrings:QuasarIdentity' in appsettings.json.");
            }
        }

        // Register event type map only if no serializer exists
        var existingSerializer = services.FirstOrDefault(d => d.ServiceType == typeof(IEventSerializer));
        if (existingSerializer == null)
        {
            services.AddQuasarEventSerializer(Quasar.Identity.IdentityEventTypeMap.Create());
        }

        // Event store configuration
        var sqlServerOptions = new SqlEventStoreOptions
        {
            ConnectionFactory = () => new Microsoft.Data.SqlClient.SqlConnection(connectionString)
        };
        configureEventStore?.Invoke(sqlServerOptions);

        // Initialize schema synchronously
        if (initializeSchema)
        {
            SqlEventStoreInitializer.EnsureSchemaAsync(sqlServerOptions).GetAwaiter().GetResult();
        }

        services.UseSqlServerEventStore(sqlServerOptions);
        services.UseSqlServerCommandTransaction();

        // Read models
        services.UseEfCoreSqlServerReadModels<Quasar.Identity.Persistence.Relational.EfCore.IdentityReadModelStore>(
            connectionString,
            registerRepositories: false,
            registerSchemaServices: initializeSchema);

        // Projections
        services.AddScoped<object, Quasar.Identity.Persistence.Relational.EfCore.IdentityProjections>();

        return services;
    }

    private static string ResolveSqliteConnectionString(string connectionString)
    {
        var builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder();
        builder.ConnectionString = connectionString;
        var dataSource = builder.DataSource;

        if (!System.IO.Path.IsPathRooted(dataSource))
        {
            dataSource = System.IO.Path.Combine(System.AppContext.BaseDirectory, dataSource);
        }

        var directory = System.IO.Path.GetDirectoryName(dataSource);
        if (!string.IsNullOrEmpty(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        builder.DataSource = System.IO.Path.GetFullPath(dataSource);
        builder.Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadWriteCreate;

        return builder.ToString();
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try { return assembly.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t is not null)!; }
    }

    private static bool ImplementsProjection(Type type)
    {
        return type.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IProjection<>));
    }
}
