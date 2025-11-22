using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Quasar.Web;

/// <summary>
/// Extension methods for mapping Quasar UI endpoints.
/// </summary>
public static class UiEndpoints
{
    /// <summary>
    /// Maps the Quasar UI configuration endpoint.
    /// </summary>
    public static IEndpointRouteBuilder MapQuasarUiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/config/ui", (UiSettings settings) => Results.Ok(settings))
           .WithName("GetUiConfig")
           .WithTags("Configuration")
           .AllowAnonymous();

        app.MapGet("/api/features", (Quasar.Features.FeatureRegistry registry) => Results.Ok(registry.GetAll()))
           .WithName("GetFeatures")
           .WithTags("Administration")
           .RequireAuthorization();

        return app;
    }
}
