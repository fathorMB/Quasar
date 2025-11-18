using System;
using System.Linq;

namespace Quasar.Ui.Navigation;

public sealed class QuasarUiNavigationBuilder
{
    private readonly QuasarUiNavigationOptions _options;

    internal QuasarUiNavigationBuilder(QuasarUiNavigationOptions options)
    {
        _options = options;
    }

    public QuasarUiNavigationBuilder ClearSections()
    {
        _options.Sections.Clear();
        return this;
    }

    public QuasarUiNavigationBuilder AddSection(string title, Action<QuasarUiNavSectionBuilder> configure)
    {
        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var section = new QuasarUiNavSection(title);
        var builder = new QuasarUiNavSectionBuilder(section);
        configure(builder);
        if (section.Items.Count > 0)
        {
            _options.Sections.Add(section);
        }
        return this;
    }

    internal void EnsureDefaults()
    {
        if (_options.Sections.Count == 0)
        {
            throw new InvalidOperationException("At least one navigation section with an item is required for Quasar UI.");
        }

        var items = _options.Sections.SelectMany(section => section.Items).ToList();
        if (items.Count == 0)
        {
            throw new InvalidOperationException("At least one navigation item is required for Quasar UI.");
        }

        if (items.Count(item => item.IsDefault) == 0)
        {
            items[0].IsDefault = true;
        }
    }
}
