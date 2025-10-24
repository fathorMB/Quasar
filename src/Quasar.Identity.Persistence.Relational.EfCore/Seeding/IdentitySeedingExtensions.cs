using Microsoft.Extensions.DependencyInjection;
using Quasar.Seeding;
using System;

namespace Quasar.Identity.Persistence.Relational.EfCore.Seeding;

public static class IdentitySeedingExtensions
{
    public static IServiceCollection AddIdentityDataSeeding(this IServiceCollection services, Action<IdentitySeedOptions>? configure = null)
    {
        services.AddOptions<IdentitySeedOptions>();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.AddDataSeed<IdentityDataSeed>();
        return services;
    }
}
