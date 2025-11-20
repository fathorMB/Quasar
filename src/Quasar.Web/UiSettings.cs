namespace Quasar.Web;

/// <summary>
/// Configuration for the Quasar UI appearance and branding.
/// </summary>
public sealed class UiSettings
{
    /// <summary>
    /// The application name displayed in the UI (e.g., header, sidebar).
    /// </summary>
    public string ApplicationName { get; set; } = "BEAM";

    /// <summary>
    /// The theme to apply. Valid values: "dark", "orange", "green".
    /// </summary>
    public string Theme { get; set; } = "dark";
}
