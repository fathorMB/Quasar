using System;
using Microsoft.Extensions.DependencyInjection;
using Quasar.Ui.Branding;
using Quasar.Ui.Navigation;

namespace Quasar.Ui;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQuasarUi(
        this IServiceCollection services,
        Action<QuasarUiNavigationBuilder>? configureNavigation = null,
        Action<QuasarUiBrandingOptions>? configureBranding = null)
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
