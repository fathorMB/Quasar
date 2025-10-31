using Microsoft.Extensions.DependencyInjection;

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
}
