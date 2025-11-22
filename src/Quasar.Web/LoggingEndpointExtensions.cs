using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Quasar.Logging;

namespace Quasar.Web;

/// <summary>
/// Endpoint mapping helpers for exposing in-memory log buffer.
/// </summary>
public static class LoggingEndpointExtensions
{
    /// <summary>
    /// Maps endpoints that expose recent in-memory log entries.
    /// </summary>
    public static IEndpointRouteBuilder MapLoggingEndpoints(this IEndpointRouteBuilder endpoints, string prefix = "/logs")
    {
        endpoints.MapGet(prefix + "/recent", GetRecentLogs)
            .WithName("GetRecentLogs")
            .WithTags("Diagnostics")
            .RequireAuthorization();

        return endpoints;
    }

    private static IResult GetRecentLogs(
        [FromServices] InMemoryLogBuffer buffer,
        [FromQuery] long? since = null,
        [FromQuery] int take = 200)
    {
        take = Math.Clamp(take, 1, 500);
        var entries = buffer.GetEntries(since)
            .OrderByDescending(e => e.Sequence)
            .Take(take)
            .Select(e => new
            {
                e.Sequence,
                e.TimestampUtc,
                e.Level,
                e.Message,
                e.Exception,
                e.Properties
            });

        return Results.Ok(entries);
    }
}
