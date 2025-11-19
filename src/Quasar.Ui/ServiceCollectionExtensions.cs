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
            EnsureAdminSection(options);
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

        AddPlaceholder(spaces, "Identity", "identity", childBuilder =>
        {
            childBuilder.AddChild("Users", typeof(Components.Panels.PlaceholderPanel), "users", new Dictionary<string, object?>
            {
                { nameof(Components.Panels.PlaceholderPanel.Title), "Users" }
            });
            childBuilder.AddChild("Roles", typeof(Components.Panels.PlaceholderPanel), "roles", new Dictionary<string, object?>
            {
                { nameof(Components.Panels.PlaceholderPanel.Title), "Roles" }
            });
        });
        AddPlaceholder(spaces, "Event stream", "event-stream");
        AddPlaceholder(spaces, "Docs", "docs");

        options.Sections.Add(spaces);

        return options;
    }

    private static void EnsureAdminSection(QuasarUiNavigationOptions options)
    {
        var alreadyHasAdmin = options.Sections
            .SelectMany(section => section.Items)
            .Any(item => item.AllowedRoles.Contains(Security.QuasarUiRoles.Admin, StringComparer.OrdinalIgnoreCase));
        if (alreadyHasAdmin)
        {
            return;
        }

        var section = new QuasarUiNavSection("Administration");
        var builder = new QuasarUiNavSectionBuilder(section);
        builder.AddItem(
            "Admin workspace",
            typeof(Components.Panels.PlaceholderPanel),
            slug: "admin",
            parameters: new Dictionary<string, object?>
            {
                { nameof(Components.Panels.PlaceholderPanel.Title), "Admin workspace" }
            },
            allowedRoles: new[] { Security.QuasarUiRoles.Admin });
        options.Sections.Add(section);
    }

    private static void AddPlaceholder(
        QuasarUiNavSection section,
        string label,
        string slug,
        Action<QuasarUiNavItemChildBuilder>? configureChildren = null,
        IEnumerable<string>? allowedRoles = null)
    {
        var placeholder = new QuasarUiNavItem(label, typeof(Components.Panels.PlaceholderPanel), slug, isDefault: false, allowedRoles: allowedRoles);
        placeholder.SetParameter(nameof(Components.Panels.PlaceholderPanel.Title), label);
        if (configureChildren is not null)
        {
            configureChildren(new QuasarUiNavItemChildBuilder(placeholder));
        }
        section.Items.Add(placeholder);
    }
}
