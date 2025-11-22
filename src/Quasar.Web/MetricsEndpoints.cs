using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Quasar.Telemetry;

namespace Quasar.Web;

/// <summary>
/// Extension methods for mapping Quasar Telemetry and Metrics endpoints.
/// </summary>
public static class MetricsEndpoints
{
    /// <summary>
    /// Adds Quasar Metrics services including SignalR support.
    /// </summary>
    public static IServiceCollection AddQuasarMetrics(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddHostedService<MetricsBroadcaster>();
        return services;
    }

    /// <summary>
    /// Maps the Quasar Metrics endpoint.
    /// </summary>
    public static IEndpointRouteBuilder MapQuasarMetricsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/metrics", (IMetricsAggregator aggregator) =>
        {
            var snapshot = aggregator.GetSnapshot();
            Console.WriteLine($"[Telemetry] Returning snapshot: {snapshot.TotalRequests} requests, {snapshot.TopEndpoints.Count} endpoints");
            return Results.Ok(snapshot);
        })
        .RequireAuthorization()
        .WithName("GetMetrics")
        .WithTags("Telemetry");

        return app;
    }

    /// <summary>
    /// Maps the Quasar Metrics SignalR Hub.
    /// </summary>
    public static IEndpointRouteBuilder MapQuasarMetricsHub(this IEndpointRouteBuilder app)
    {
        app.MapHub<MetricsHub>("/hubs/metrics");
        return app;
    }
}
