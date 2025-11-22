using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Quasar.Persistence.Relational.EfCore;

/// <summary>
/// Extensions for registering read model infrastructure services.
/// </summary>
public static class ReadModelServiceCollectionExtensions
{
    /// <summary>
    /// Registers a read model definition type.
    /// </summary>
    public static IServiceCollection AddReadModelDefinition<TDefinition>(this IServiceCollection services)
        where TDefinition : class, IReadModelDefinition
    {
        services.AddSingleton<IReadModelDefinition, TDefinition>();
        return services;
    }

    /// <summary>
    /// Scans the provided assemblies (or the store assembly by default) for <see cref="IReadModelDefinition"/>
    /// types whose <see cref="IReadModelDefinition.Store"/> matches <typeparamref name="TStore"/> and registers them.
    /// </summary>
    /// <typeparam name="TStore">Marker type identifying the read model store.</typeparam>
    /// <param name="services">Service collection to populate.</param>
    /// <param name="assemblies">
    /// Optional assemblies to scan. If none are provided, the assembly containing <typeparamref name="TStore"/> is used.
    /// </param>
    public static IServiceCollection AddReadModelDefinitionsFromAssembliesForStore<TStore>(
        this IServiceCollection services,
        params Assembly[] assemblies)
        where TStore : class, IReadModelStoreMarker
    {
        if (assemblies is null || assemblies.Length == 0)
        {
            assemblies = new[] { typeof(TStore).Assembly };
        }

        var discovered = new List<Type>();
        foreach (var assembly in assemblies)
        {
            foreach (var type in SafeGetTypes(assembly))
            {
                if (type is null || type.IsAbstract || type.IsInterface)
                    continue;

                if (!typeof(IReadModelDefinition).IsAssignableFrom(type))
                    continue;

                if (!MatchesStore(type, typeof(TStore)))
                    continue;

                if (discovered.Contains(type))
                    continue;

                discovered.Add(type);
            }
        }

        foreach (var definitionType in discovered)
        {
            // Avoid duplicate registrations if the caller already registered the definition explicitly.
            var alreadyRegistered = services.Any(d =>
                d.ServiceType == typeof(IReadModelDefinition) &&
                (d.ImplementationType == definitionType || d.ImplementationInstance?.GetType() == definitionType));

            if (!alreadyRegistered)
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IReadModelDefinition), definitionType));
            }
        }

        return services;
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }

    private static bool MatchesStore(Type definitionType, Type storeType)
    {
        // Fast path: handle subclasses of ReadModelDefinition<TStore>
        var current = definitionType;
        while (current is not null && current != typeof(object))
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(ReadModelDefinition<>))
            {
                var genericArg = current.GetGenericArguments()[0];
                return genericArg == storeType;
            }
            current = current.BaseType!;
        }

        // Fallback: instantiate and inspect Store property (may fail if no default ctor).
        if (typeof(IReadModelDefinition).IsAssignableFrom(definitionType))
        {
            try
            {
                var instance = Activator.CreateInstance(definitionType) as IReadModelDefinition;
                if (instance?.Store == storeType)
                {
                    return true;
                }
            }
            catch
            {
                // Ignore instantiation errors; treat as non-matching.
            }
        }

        return false;
    }
}
