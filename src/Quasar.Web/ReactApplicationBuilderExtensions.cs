using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace Quasar.Web;

/// <summary>
/// Helpers for wiring up the React-based Quasar UI static assets.
/// </summary>
public static class ReactApplicationBuilderExtensions
{
    /// <summary>
    /// Adds middleware to serve the packaged React UI and fall back to the SPA entry point.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    /// <param name="fileProvider">
    /// Optional file provider that points at the React build output. Defaults to the host's <c>WebRootFileProvider</c>.
    /// </param>
    /// <param name="indexFileName">The SPA entry file name (defaults to <c>index.html</c>).</param>
    public static WebApplication UseQuasarReactUi(
        this WebApplication app,
        IFileProvider? fileProvider = null,
        string indexFileName = "index.html")
    {
        fileProvider ??= app.Environment.WebRootFileProvider;

        app.UseDefaultFiles(new DefaultFilesOptions
        {
            FileProvider = fileProvider
        });

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = fileProvider
        });

        app.MapFallback(async context =>
        {
            var indexFile = fileProvider.GetFileInfo(indexFileName);
            if (!indexFile.Exists)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            context.Response.ContentType = "text/html";
            await context.Response.SendFileAsync(indexFile, context.RequestAborted).ConfigureAwait(false);
        });

        return app;
    }
}
