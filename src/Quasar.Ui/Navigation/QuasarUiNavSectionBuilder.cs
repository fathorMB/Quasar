using System;
using System.Collections.Generic;

namespace Quasar.Ui.Navigation;

public sealed class QuasarUiNavSectionBuilder
{
    private readonly QuasarUiNavSection _section;

    internal QuasarUiNavSectionBuilder(QuasarUiNavSection section)
    {
        _section = section;
    }

    public QuasarUiNavSectionBuilder AddItem(string label, Type componentType, string? slug = null, bool isDefault = false, IReadOnlyDictionary<string, object?>? parameters = null)
    {
        slug ??= Slugify(label);
        var item = new QuasarUiNavItem(label, componentType, slug, isDefault);
        if (parameters is not null)
        {
            foreach (var pair in parameters)
            {
                item.SetParameter(pair.Key, pair.Value);
            }
        }
        _section.Items.Add(item);
        return this;
    }

    private static string Slugify(string label)
    {
        var slug = label.Trim().ToLowerInvariant();
        slug = string.Join('-', slug.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return slug;
    }
}
