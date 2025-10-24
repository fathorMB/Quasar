using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Quasar.Persistence.Abstractions;

namespace Quasar.Persistence.TimeSeries.Timescale;

public static class TimescaleServiceCollectionExtensions
{
    public static IServiceCollection UseTimescaleTimeSeries(this IServiceCollection services, Action<TimescaleOptions> configure)
    {
        services.Configure(configure);
        services.TryAddSingleton<ITimeSeriesWriter, TimescaleTimeSeriesWriter>();
        services.TryAddSingleton<ITimeSeriesReader, TimescaleTimeSeriesReader>();
        services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<TimescaleOptions>>().Value);
        services.AddHostedService<TimescaleInitializerHostedService>();
        return services;
    }
}
