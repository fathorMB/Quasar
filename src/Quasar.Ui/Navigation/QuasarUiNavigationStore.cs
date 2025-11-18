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
}
