using System;
using System.Collections.Generic;

namespace Quasar.Ui.Navigation;

public sealed class QuasarUiNavItem
{
    private readonly Dictionary<string, object?> _parameters = new();

    public QuasarUiNavItem(
        string label,
        Type componentType,
        string slug,
        bool isDefault,
        QuasarUiNavItem? parent = null,
        IEnumerable<string>? allowedRoles = null)
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
        Parent = parent;
        Slug = (slug ?? string.Empty).Trim('/');
        IsDefault = isDefault;
        AllowedRoles = allowedRoles?.Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? Array.Empty<string>();
    }

    public string Label { get; }

    public string Slug { get; }

    public Type ComponentType { get; }

    public bool IsDefault { get; internal set; }

    public QuasarUiNavItem? Parent { get; }

    public IList<QuasarUiNavItem> Children { get; } = new List<QuasarUiNavItem>();

    public IReadOnlyCollection<string> AllowedRoles { get; }

    public string Href => string.IsNullOrWhiteSpace(Slug) ? "/" : $"/app/{Slug}";

    public IDictionary<string, object?> Parameters => _parameters;

    public void SetParameter(string name, object? value)
    {
        _parameters[name] = value;
    }

    public string BuildChildSlug(string slugFragment)
    {
        var childSlug = (slugFragment ?? string.Empty).Trim('/');
        if (string.IsNullOrEmpty(childSlug))
        {
            return Slug;
        }

        if (string.IsNullOrEmpty(Slug))
        {
            return childSlug;
        }

        return $"{Slug}/{childSlug}";
    }
}
