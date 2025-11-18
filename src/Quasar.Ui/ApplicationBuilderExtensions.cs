using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Endpoints;

namespace Quasar.Ui;

public static class ApplicationBuilderExtensions
{
    public static RazorComponentsEndpointConventionBuilder MapQuasarUi(this WebApplication app)
    {
        return app.MapRazorComponents<Components.App>()
            .AddInteractiveServerRenderMode();
    }
}
