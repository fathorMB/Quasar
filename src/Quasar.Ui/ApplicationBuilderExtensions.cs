using System;
using System.Linq;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Quasar.Ui.Security;

namespace Quasar.Ui;

public static class ApplicationBuilderExtensions
{
    public static RazorComponentsEndpointConventionBuilder MapQuasarUi(this WebApplication app)
    {
        var builder = app.MapRazorComponents<Components.App>()
            .AddInteractiveServerRenderMode();

        var security = app.Services.GetRequiredService<QuasarUiSecurityOptions>();
        if (!security.RequireAuthentication && !security.RequireAuthenticationExplicitlySet && security.AutoDetectAuthentication)
        {
            var jwtServiceType = Type.GetType("Quasar.Identity.Web.IJwtTokenService, Quasar.Identity.Web");
            if (jwtServiceType is not null && app.Services.GetService(jwtServiceType) is not null)
            {
                security.RequireAuthentication = true;
            }
        }
        if (security.RequireAuthentication)
        {
            var authorizeData = new AuthorizeAttribute
            {
                AuthenticationSchemes = QuasarUiAuthenticationDefaults.AuthenticationScheme
            };
            builder.RequireAuthorization(authorizeData);

            app.MapPost("/auth/login", async (IAntiforgery antiforgery, HttpContext httpContext, QuasarUiAuthenticationService auth) =>
                {
                    await antiforgery.ValidateRequestAsync(httpContext).ConfigureAwait(false);
                    var formCollection = await httpContext.Request.ReadFormAsync().ConfigureAwait(false);
                    var rememberValues = formCollection["rememberMe"];
                    var form = new LoginForm
                    {
                        Username = formCollection["username"],
                        Password = formCollection["password"],
                        ReturnUrl = formCollection["returnUrl"],
                        RememberMe = rememberValues.Any(v => string.Equals(v, "true", StringComparison.OrdinalIgnoreCase))
                    };

                    var success = await auth.SignInAsync(form.Username, form.Password, form.RememberMe).ConfigureAwait(false);
                    if (!success)
                    {
                        var target = string.IsNullOrEmpty(form.ReturnUrl) ? string.Empty : $"?ReturnUrl={Uri.EscapeDataString(form.ReturnUrl)}";
                        var status = string.IsNullOrEmpty(target) ? "?status=failed" : $"{target}&status=failed";
                        return Results.Redirect($"/login{status}");
                    }

                    var redirect = "/";
                    if (!string.IsNullOrWhiteSpace(form.ReturnUrl) && form.ReturnUrl!.StartsWith("/", StringComparison.Ordinal))
                    {
                        redirect = form.ReturnUrl!;
                    }
                    return Results.Redirect(redirect);
                })
                .AllowAnonymous();

            app.MapPost("/logout", async (QuasarUiAuthenticationService auth) =>
                {
                    await auth.SignOutAsync().ConfigureAwait(false);
                    return Results.Redirect("/login");
                })
                .RequireAuthorization(authorizeData);
        }
        return builder;
    }
}
