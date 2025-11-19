namespace Quasar.Ui.Navigation;

internal static class QuasarUiSlug
{
    public static string Slugify(string label)
    {
        var slug = (label ?? string.Empty).Trim().ToLowerInvariant();
        slug = string.Join('-', slug.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return slug;
    }
}
