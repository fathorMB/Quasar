using System;
using Microsoft.Extensions.DependencyInjection;
using Quasar.Identity.Persistence.Relational.EfCore.Seeding;

namespace Quasar.Identity.Web;

public static partial class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddQuasarIdentitySeed(this IServiceCollection services, Action<IdentitySeedBuilder> configure)
    {
        var builder = new IdentitySeedBuilder(new IdentitySeedSet { Name = "Default" });
        configure(builder);

        services.AddIdentityDataSeeding(options =>
        {
            options.Sets.Add(builder.GetSet());
        });

        return services;
    }
}
