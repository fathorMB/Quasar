using Microsoft.Extensions.DependencyInjection;
using Quasar.EventSourcing.Abstractions;

namespace Quasar.Projections.Abstractions;

/// <summary>
/// Extension methods for configuring live (real-time) projections in the dependency container.
/// </summary>
public static class LiveProjectionServiceCollectionExtensions
{
    /// <summary>
    /// Adds live projection infrastructure with in-memory read model storage.
    /// </summary>
    /// <remarks>
    /// This method:
    /// 1. Registers the live projection pipeline
    /// 2. Registers the in-memory read model store
    /// 3. Wraps the existing event store with live projection decorator
    /// Call AddLiveReadModelHub and AddLiveReadModelNotifier separately to set up SignalR.
    /// </remarks>
    public static IServiceCollection AddLiveProjections(this IServiceCollection services)
    {
        // Register pipeline
        services.AddSingleton<ILiveProjectionPipeline, LiveProjectionPipeline>();

        // Register in-memory store
        services.AddSingleton<ILiveReadModelStore, InMemoryLiveReadModelStore>();

        // Decorate event store (must be done after initial registration)
        services.Decorate<IEventStore>((inner, provider) =>
        {
            var pipeline = provider.GetRequiredService<ILiveProjectionPipeline>();
            return new LiveProjectionEventStoreDecorator(inner, pipeline);
        });

        return services;
    }

    /// <summary>
    /// Registers a live projection handler for a specific event type.
    /// </summary>
    /// <typeparam name="TProjection">The live projection type implementing <see cref="ILiveProjection{TEvent}"/>.</typeparam>
    /// <typeparam name="TEvent">The event type handled by this projection.</typeparam>
    public static IServiceCollection AddLiveProjection<TProjection, TEvent>(
        this IServiceCollection services)
        where TProjection : class, ILiveProjection<TEvent>
        where TEvent : IEvent
    {
        services.AddScoped<TProjection>();
        
        // Register with pipeline
        services.AddSingleton<IConfigureLiveProjections>(provider =>
            new LiteProjectionConfigurator(provider, typeof(TEvent), typeof(TProjection)));

        return services;
    }

    /// <summary>
    /// Configures live projections after all services are registered.
    /// </summary>
    public static IServiceProvider ConfigureLiveProjections(this IServiceProvider provider)
    {
        var pipeline = provider.GetRequiredService<ILiveProjectionPipeline>();
        var configurators = provider.GetServices<IConfigureLiveProjections>();

        foreach (var config in configurators)
        {
            config.Configure(pipeline);
        }

        return provider;
    }
}

/// <summary>
/// Marker interface for live projection configuration.
/// </summary>
public interface IConfigureLiveProjections
{
    /// <summary>
    /// Configures the live projection pipeline.
    /// </summary>
    void Configure(ILiveProjectionPipeline pipeline);
}

/// <summary>
/// Helper for configuring a single live projection.
/// </summary>
internal sealed class LiteProjectionConfigurator : IConfigureLiveProjections
{
    private readonly IServiceProvider _provider;
    private readonly Type _eventType;
    private readonly Type _projectionType;

    public LiteProjectionConfigurator(IServiceProvider provider, Type eventType, Type projectionType)
    {
        _provider = provider;
        _eventType = eventType;
        _projectionType = projectionType;
    }

    void IConfigureLiveProjections.Configure(ILiveProjectionPipeline pipeline)
    {
        pipeline.RegisterProjection(_eventType, _projectionType);
    }
}

/// <summary>
/// Extension method for decorating services with a factory pattern.
/// </summary>
internal static class ServiceCollectionDecoratorExtensions
{
    /// <summary>
    /// Replaces the last registered implementation of <typeparamref name="TService"/>
    /// with a decorated version.
    /// </summary>
    public static IServiceCollection Decorate<TService>(
        this IServiceCollection services,
        Func<TService, IServiceProvider, TService> decorator)
        where TService : class
    {
        var wrappedDescriptor = services.LastOrDefault(s => s.ServiceType == typeof(TService));
        if (wrappedDescriptor is null)
            throw new InvalidOperationException($"Service of type {typeof(TService).Name} is not registered.");

        var objectFactory = ActivatorUtilities.CreateFactory(wrappedDescriptor.ImplementationType ?? typeof(TService), Type.EmptyTypes);

        services.Remove(wrappedDescriptor);
        services.Add(ServiceDescriptor.Describe(
            typeof(TService),
            provider =>
            {
                var instance = objectFactory(provider, null) as TService ?? throw new InvalidOperationException();
                return decorator(instance, provider);
            },
            wrappedDescriptor.Lifetime));

        return services;
    }
}
