using System;
using System.Collections.Generic;

namespace Quasar.Ui.Navigation;

public sealed class QuasarUiNavItem
{
    private readonly Dictionary<string, object?> _parameters = new();

    public QuasarUiNavItem(string label, Type componentType, string slug, bool isDefault)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Label is required", nameof(label));
        }
        if (componentType is null)
        {
            throw new ArgumentNullException(nameof(componentType));
        }

        Label = label;
        ComponentType = componentType;
        Slug = (slug ?? string.Empty).Trim('/');
        IsDefault = isDefault;
    }

    public string Label { get; }

    public string Slug { get; }

    public Type ComponentType { get; }

    public bool IsDefault { get; internal set; }

    public string Href => string.IsNullOrWhiteSpace(Slug) ? "/" : $"/app/{Slug}";

    public IDictionary<string, object?> Parameters => _parameters;

    public void SetParameter(string name, object? value)
    {
        _parameters[name] = value;
    }
}
