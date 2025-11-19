using System;
using System.Collections.Generic;

namespace Quasar.Ui.Navigation;

public sealed class QuasarUiNavItemChildBuilder
{
    private readonly QuasarUiNavItem _parent;

    internal QuasarUiNavItemChildBuilder(QuasarUiNavItem parent)
    {
        _parent = parent;
    }

    public QuasarUiNavItemChildBuilder AddChild(string label, Type componentType, string? slug = null, IReadOnlyDictionary<string, object?>? parameters = null)
    {
        slug ??= QuasarUiSlug.Slugify(label);
        var combinedSlug = _parent.BuildChildSlug(slug);
        var child = new QuasarUiNavItem(label, componentType, combinedSlug, isDefault: false, parent: _parent);
        if (parameters is not null)
        {
            foreach (var pair in parameters)
            {
                child.SetParameter(pair.Key, pair.Value);
            }
        }
        _parent.Children.Add(child);
        return this;
    }
}
