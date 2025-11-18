namespace Quasar.Ui.Branding;

public sealed class QuasarUiBrandingOptions
{
    public string ApplicationName { get; set; } = "Quasar Server";

    public string LogoGlyph { get; set; } = "Q";

    public QuasarUiColorPalette Palette { get; set; } = QuasarUiColorPalettes.Default;
}
