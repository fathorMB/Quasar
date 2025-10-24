using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Quasar.Seeding;

public static class SeedingExtensions
{
    public static IServiceCollection AddDataSeed<TSeed>(this IServiceCollection services)
        where TSeed : class, IDataSeed
    {
        services.AddTransient<IDataSeed, TSeed>();
        return services;
    }

    public static async Task SeedDataAsync(this IHost host, CancellationToken cancellationToken = default)
    {
        using var scope = host.Services.CreateScope();
        var seeds = scope.ServiceProvider.GetServices<IDataSeed>();
        var ordered = seeds
            .Select(seed => (seed, order: (seed as IOrderedDataSeed)?.Order ?? 0))
            .OrderBy(x => x.order)
            .Select(x => x.seed);

        foreach (var seed in ordered)
        {
            await seed.SeedAsync(scope.ServiceProvider, cancellationToken).ConfigureAwait(false);
        }
    }
}
