using Microsoft.AspNetCore.Builder;

namespace Quasar.Telemetry;

/// <summary>
/// Extension methods for wiring Quasar Telemetry middleware into the application pipeline.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the Quasar Telemetry middleware to the application pipeline.
    /// This middleware tracks HTTP request metrics including latency, status codes, and exceptions.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseQuasarTelemetry(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TelemetryMiddleware>();
    }
}
