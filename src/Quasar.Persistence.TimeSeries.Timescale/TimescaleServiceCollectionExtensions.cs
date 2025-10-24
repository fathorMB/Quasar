using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Quasar.Persistence.Abstractions;

namespace Quasar.Persistence.TimeSeries.Timescale;

/// <summary>
/// Service registration helpers for TimescaleDB time series support.
/// </summary>
public static class TimescaleServiceCollectionExtensions
{
    /// <summary>
    /// Registers Timescale-backed implementations of <see cref="ITimeSeriesWriter"/> and <see cref="ITimeSeriesReader"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">Delegate used to configure <see cref="TimescaleOptions"/>.</param>
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
