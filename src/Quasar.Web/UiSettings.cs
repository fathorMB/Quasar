namespace Quasar.Web;

/// <summary>
/// Configuration for the Quasar UI appearance and branding.
/// </summary>
public sealed class UiSettings
{
    /// <summary>
    /// The application name displayed in the UI (e.g., header, sidebar).
    /// </summary>
    public string ApplicationName { get; set; } = "Quasar";

    /// <summary>
    /// The theme to apply. Valid values: "dark", "orange", "green".
    /// </summary>
    public string Theme { get; set; } = "dark";

    /// <summary>
    /// The symbol/letter used in the sidebar logo badge.
    /// </summary>
    public string LogoSymbol { get; set; } = "Q";

    /// <summary>
    /// Optional URL for an app-provided UI bundle that registers custom routes/menu.
    /// If set, the shell will load this module at runtime before mounting.
    /// </summary>
    public string? CustomBundleUrl { get; set; }

    /// <summary>
    /// Whether to show the framework's Administration menu section (Users, Roles, Logs, etc.).
    /// Defaults to true. Set to false for player-facing applications.
    /// </summary>
    public bool ShowAdminMenu { get; set; } = true;
}
