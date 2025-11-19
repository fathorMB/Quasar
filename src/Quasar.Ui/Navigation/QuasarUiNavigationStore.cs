using System.Collections.Generic;
using System.Linq;

namespace Quasar.Ui.Navigation;

public sealed class QuasarUiNavigationStore
{
    private readonly Dictionary<string, QuasarUiNavItem> _lookup;

    public QuasarUiNavigationStore(QuasarUiNavigationOptions options)
    {
        Options = options;
        _lookup = Options.Sections
            .SelectMany(section => section.Items)
            .SelectMany(Flatten)
            .ToDictionary(item => item.Slug, StringComparer.OrdinalIgnoreCase);
    }

    public QuasarUiNavigationOptions Options { get; }

    public IReadOnlyList<QuasarUiNavSection> Sections => Options.Sections.ToList();

    public QuasarUiNavItem? Resolve(string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return Options.DefaultItem;
        }

        return _lookup.TryGetValue(slug, out var item) ? item : null;
    }

    private static IEnumerable<QuasarUiNavItem> Flatten(QuasarUiNavItem root)
    {
        yield return root;
        foreach (var child in root.Children)
        {
            foreach (var nested in Flatten(child))
            {
                yield return nested;
            }
        }
    }
}
