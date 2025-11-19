using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Quasar.Ui.Branding;
using Quasar.Ui.Navigation;
using Quasar.Ui.Security;

namespace Quasar.Ui;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQuasarUi(
        this IServiceCollection services,
        Action<QuasarUiNavigationBuilder>? configureNavigation = null,
        Action<QuasarUiBrandingOptions>? configureBranding = null,
        Action<QuasarUiSecurityOptions>? configureSecurity = null)
    {
        services.AddSingleton(provider =>
        {
            var options = CreateDefaultNavigation();
            var builder = new QuasarUiNavigationBuilder(options);
            configureNavigation?.Invoke(builder);
            builder.EnsureDefaults();
            return new QuasarUiNavigationStore(options);
        });

        services.AddSingleton(provider =>
        {
            var branding = new QuasarUiBrandingOptions();
            configureBranding?.Invoke(branding);
            branding.ApplicationName = string.IsNullOrWhiteSpace(branding.ApplicationName) ? "Quasar Server" : branding.ApplicationName.Trim();
            branding.LogoGlyph = string.IsNullOrWhiteSpace(branding.LogoGlyph)
                ? branding.ApplicationName.Substring(0, 1).ToUpperInvariant()
                : branding.LogoGlyph.Trim();
            branding.Palette = branding.Palette.Equals(default) ? QuasarUiColorPalettes.Default : branding.Palette;
            return branding;
        });

        var securityOptions = new QuasarUiSecurityOptions();
        configureSecurity?.Invoke(securityOptions);
        var hasIdentity = services.Any(sd => sd.ServiceType.FullName == "Quasar.Identity.Web.IJwtTokenService");
        if (!securityOptions.RequireAuthenticationExplicitlySet && securityOptions.AutoDetectAuthentication && hasIdentity)
        {
            securityOptions.RequireAuthentication = true;
        }
        services.AddSingleton(securityOptions);

        services.AddAntiforgery();
        services.AddCascadingAuthenticationState();

        services.AddAuthentication()
            .AddCookie(QuasarUiAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = "quasar_ui_auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/login";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = securityOptions.SessionDuration;
            });

        services.AddHttpContextAccessor();
        services.AddScoped<QuasarUiAuthenticationService>();

        services.AddAuthorizationCore();

        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        return services;
    }

    private static QuasarUiNavigationOptions CreateDefaultNavigation()
    {
        var options = new QuasarUiNavigationOptions();
        var spaces = new QuasarUiNavSection("Spaces");

        var overview = new QuasarUiNavItem("Overview", typeof(Components.Panels.OverviewPanel), string.Empty, isDefault: true);
        spaces.Items.Add(overview);

        AddPlaceholder(spaces, "Identity", "identity");
        AddPlaceholder(spaces, "Event stream", "event-stream");
        AddPlaceholder(spaces, "Docs", "docs");

        options.Sections.Add(spaces);
        return options;
    }

    private static void AddPlaceholder(QuasarUiNavSection section, string label, string slug)
    {
        var placeholder = new QuasarUiNavItem(label, typeof(Components.Panels.PlaceholderPanel), slug, isDefault: false);
        placeholder.SetParameter(nameof(Components.Panels.PlaceholderPanel.Title), label);
        section.Items.Add(placeholder);
    }
}
