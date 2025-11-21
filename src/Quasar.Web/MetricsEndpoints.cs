using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Quasar.Telemetry;

namespace Quasar.Web;

/// <summary>
/// Extension methods for mapping Quasar Telemetry and Metrics endpoints.
/// </summary>
public static class MetricsEndpoints
{
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
}
