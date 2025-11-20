using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Quasar.Features;

public record FeatureInfo(
    string Id,
    string Name,
    string Category,
    string Description,
    string Status = "enabled",
    string? Details = null);

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class QuasarFeatureAttribute : Attribute
{
    public QuasarFeatureAttribute(string id, string name, string category, string description, string status = "enabled", string? details = null)
    {
        Id = id;
        Name = name;
        Category = category;
        Description = description;
        Status = status;
        Details = details;
    }

    public string Id { get; }
    public string Name { get; }
    public string Category { get; }
    public string Description { get; }
    public string Status { get; }
    public string? Details { get; }

    internal FeatureInfo ToFeatureInfo() => new(Id, Name, Category, Description, Status, Details);
}

/// <summary>
/// Optional hook for assemblies to emit feature metadata dynamically (e.g., provider, config).
/// </summary>
public interface IQuasarFeatureContributor
{
    IEnumerable<FeatureInfo> DescribeFeatures(IServiceProvider services);
}

/// <summary>
/// In-memory registry describing which Quasar modules are active. Supports reflection-based loading
/// from assembly-level attributes and optional contributors.
/// </summary>
public class FeatureRegistry
{
    private readonly List<FeatureInfo> _features = new();
    private static readonly IServiceProvider NoServices = new NullServiceProvider();

    public FeatureRegistry Add(FeatureInfo feature)
    {
        var existing = _features.FindIndex(f => f.Id == feature.Id);
        if (existing >= 0)
        {
            _features[existing] = feature;
        }
        else
        {
            _features.Add(feature);
        }
        return this;
    }

    public FeatureRegistry AddRange(IEnumerable<FeatureInfo> features)
    {
        foreach (var feature in features)
        {
            Add(feature);
        }
        return this;
    }

    public FeatureRegistry LoadFromAssemblies(IEnumerable<Assembly> assemblies, IServiceProvider? services = null)
    {
        foreach (var assembly in assemblies)
        {
            foreach (var attribute in assembly.GetCustomAttributes<QuasarFeatureAttribute>())
            {
                Add(attribute.ToFeatureInfo());
            }

            foreach (var contributor in DiscoverContributors(assembly))
            {
                try
                {
                    var described = contributor.DescribeFeatures(services ?? NoServices);
                    if (described != null)
                    {
                        AddRange(described);
                    }
                }
                catch
                {
                    // Ignore contributor failures to avoid breaking app startup.
                }
            }
        }

        return this;
    }

    public IEnumerable<FeatureInfo> GetAll() =>
        _features.OrderBy(f => f.Category).ThenBy(f => f.Name);

    private static IEnumerable<IQuasarFeatureContributor> DiscoverContributors(Assembly assembly)
    {
        IEnumerable<Type> candidates;
        try
        {
            candidates = assembly
                .GetTypes()
                .Where(t => typeof(IQuasarFeatureContributor).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
        }
        catch (ReflectionTypeLoadException ex)
        {
            candidates = ex.Types.Where(t => t is not null && typeof(IQuasarFeatureContributor).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)!;
        }

        foreach (var type in candidates)
        {
            IQuasarFeatureContributor? contributor = null;
            try
            {
                contributor = Activator.CreateInstance(type) as IQuasarFeatureContributor;
            }
            catch
            {
                // Ignore instantiation failures and continue scanning.
            }

            if (contributor != null)
            {
                yield return contributor;
            }
        }
    }

    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
