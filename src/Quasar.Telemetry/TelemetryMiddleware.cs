using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Quasar.Telemetry;

public sealed class TelemetryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMetricsAggregator _aggregator;

    public TelemetryMiddleware(RequestDelegate next, IMetricsAggregator aggregator)
    {
        _next = next;
        _aggregator = aggregator;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var endpoint = context.Request.Path.Value ?? "/";
        
        // Skip metrics endpoint to avoid noise
        if (endpoint.StartsWith("/api/metrics", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _aggregator.RecordException(ex, endpoint);
            throw;
        }
        finally
        {
            sw.Stop();
            _aggregator.RecordRequest(
                endpoint,
                context.Response.StatusCode,
                sw.Elapsed.TotalMilliseconds);
                
            Console.WriteLine($"[Telemetry] Recorded request: {endpoint} ({context.Response.StatusCode})");
        }
    }
}
