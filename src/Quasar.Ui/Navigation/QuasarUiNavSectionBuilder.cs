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

    public QuasarUiNavSectionBuilder AddItem(
        string label,
        Type componentType,
        string? slug = null,
        bool isDefault = false,
        IReadOnlyDictionary<string, object?>? parameters = null,
        Action<QuasarUiNavItemChildBuilder>? configureChildren = null,
        IEnumerable<string>? allowedRoles = null)
    {
        slug ??= QuasarUiSlug.Slugify(label);
        var item = new QuasarUiNavItem(label, componentType, slug, isDefault, allowedRoles: allowedRoles);
        if (parameters is not null)
        {
            foreach (var pair in parameters)
            {
                item.SetParameter(pair.Key, pair.Value);
            }
        }
        if (configureChildren is not null)
        {
            var childBuilder = new QuasarUiNavItemChildBuilder(item);
            configureChildren(childBuilder);
        }
        _section.Items.Add(item);
        return this;
    }
}
