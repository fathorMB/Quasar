using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Quasar.Cqrs;
using Quasar.EventSourcing.Abstractions;
using Quasar.EventSourcing.SqlServer;
using Quasar.EventSourcing.Sqlite;
using Quasar.Projections.Abstractions;
using Quasar.Telemetry;

namespace Quasar.Web;

/// <summary>
/// Opinionated builder that composes common Quasar services (mediator, event sourcing, projections, telemetry).
/// </summary>
public sealed class QuasarBuilder
{
    public IServiceCollection Services { get; }

    public QuasarBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// Adds mediator and pipeline behaviors.
    /// </summary>
    public QuasarBuilder AddMediator(bool addDefaultBehaviors = true)
    {
        Services.AddQuasarMediator(addDefaultBehaviors);
        return this;
    }

    /// <summary>
    /// Adds event sourcing repository and outbox core.
    /// </summary>
    public QuasarBuilder AddEventSourcingCore()
    {
        Services.AddQuasarEventSourcingCore();
        return this;
    }

    /// <summary>
    /// Registers an event serializer using the provided type map (skip if already registered).
    /// </summary>
    public QuasarBuilder AddEventSerializer(IEventTypeMap typeMap)
    {
        var existing = Services.FirstOrDefault(d => d.ServiceType == typeof(IEventSerializer));
        if (existing is null)
        {
            Services.AddQuasarEventSerializer(typeMap);
        }
        return this;
    }

    /// <summary>
    /// Configures SQL Server event store infrastructure.
    /// </summary>
    public QuasarBuilder UseSqlServerEventStore(SqlEventStoreOptions options)
    {
        Services.UseSqlServerEventStore(options);
        Services.UseSqlServerCommandTransaction();
        return this;
    }

    /// <summary>
    /// Configures SQLite event store infrastructure.
    /// </summary>
    public QuasarBuilder UseSqliteEventStore(SqliteEventStoreOptions options)
    {
        Services.UseSqliteEventStore(options);
        Services.UseSqliteCommandTransaction();
        return this;
    }

    /// <summary>
    /// Registers projections discovered in the given assemblies.
    /// </summary>
    public QuasarBuilder AddProjections(params Assembly[] assemblies)
    {
        Services.AddProjections(assemblies);
        return this;
    }

    /// <summary>
    /// Configures telemetry collection and metrics broadcasting.
    /// </summary>
    public QuasarBuilder AddTelemetry()
    {
        Services.AddSingleton<IMetricsAggregator, InMemoryMetricsAggregator>();
        Services.AddHostedService<MetricsBroadcaster>();
        Services.AddSignalR();
        return this;
    }
}

public static class QuasarBuilderServiceCollectionExtensions
{
    /// <summary>
    /// Creates a <see cref="QuasarBuilder"/> to fluently compose Quasar modules.
    /// </summary>
    public static QuasarBuilder AddQuasar(this IServiceCollection services, Action<QuasarBuilder> configure)
    {
        var builder = new QuasarBuilder(services);
        configure(builder);
        return builder;
    }
}
