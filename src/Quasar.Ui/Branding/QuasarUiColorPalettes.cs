namespace Quasar.Ui.Branding;

public static class QuasarUiColorPalettes
{
    public static QuasarUiColorPalette Default { get; } = new(
        Background: "#060b1c",
        Panel: "#0f172a",
        Navigation: "#050818",
        Border: "rgba(148, 163, 184, 0.2)",
        AccentGradientStart: "#2563eb",
        AccentGradientEnd: "#7c3aed",
        PrimaryBadge: "#2563eb",
        NavActive: "rgba(148, 163, 184, 0.2)");

    public static QuasarUiColorPalette Ember { get; } = new(
        Background: "#1a0b06",
        Panel: "#2c1510",
        Navigation: "#190b06",
        Border: "rgba(249, 115, 22, 0.25)",
        AccentGradientStart: "#f97316",
        AccentGradientEnd: "#facc15",
        PrimaryBadge: "#f97316",
        NavActive: "rgba(249, 115, 22, 0.25)");

    public static QuasarUiColorPalette Grove { get; } = new(
        Background: "#04130d",
        Panel: "#0a1f16",
        Navigation: "#04130d",
        Border: "rgba(16, 185, 129, 0.25)",
        AccentGradientStart: "#10b981",
        AccentGradientEnd: "#34d399",
        PrimaryBadge: "#10b981",
        NavActive: "rgba(16, 185, 129, 0.3)");
}
