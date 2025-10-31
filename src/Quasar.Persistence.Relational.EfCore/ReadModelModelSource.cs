using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Quasar.Persistence.Relational.EfCore;

/// <summary>
/// Provides model configuration for read model stores.
/// </summary>
public interface IReadModelModelSource
{
    /// <summary>
    /// Applies registered definitions for the given store to the <paramref name="builder"/>.
    /// </summary>
    void Configure(ModelBuilder builder, Type storeKey);
}

public sealed class ReadModelModelSource : IReadModelModelSource
{
    private readonly IReadOnlyDictionary<Type, IReadOnlyList<IReadModelDefinition>> _definitions;

    public ReadModelModelSource(IEnumerable<IReadModelDefinition> definitions)
    {
        _definitions = definitions
            .GroupBy(d => d.Store)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<IReadModelDefinition>)g.ToArray());
    }

    public void Configure(ModelBuilder builder, Type storeKey)
    {
        if (!_definitions.TryGetValue(storeKey, out var scopedDefinitions))
        {
            return;
        }

        foreach (var definition in scopedDefinitions)
        {
            definition.Configure(builder);
        }
    }
}
