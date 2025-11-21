using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using Quasar.Cqrs;

namespace Quasar.Telemetry;

/// <summary>
/// Extension methods for wiring Quasar Telemetry components into an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Quasar Telemetry pipeline behavior and configures OpenTelemetry.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">An optional action to configure the OpenTelemetry tracer provider.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddQuasarTelemetry(this IServiceCollection services, Action<TracerProviderBuilder>? configure = null)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TelemetryBehavior<,>));
        
        // Register metrics aggregator
        services.AddSingleton<IMetricsAggregator, InMemoryMetricsAggregator>();

        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .AddSource(QuasarActivitySource.Name)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                // Allow the consumer to add their own configuration
                configure?.Invoke(builder);
            });

        return services;
    }
}
