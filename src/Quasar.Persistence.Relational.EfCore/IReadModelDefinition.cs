using System;
using Microsoft.EntityFrameworkCore;

namespace Quasar.Persistence.Relational.EfCore;

/// <summary>
/// Describes how a set of read model entities should be mapped for a specific store.
/// </summary>
public interface IReadModelDefinition
{
    /// <summary>
    /// Gets the marker type that identifies the read model store the definition belongs to.
    /// </summary>
    Type Store { get; }

    /// <summary>
    /// Applies entity configuration using the supplied <see cref="ModelBuilder"/>.
    /// </summary>
    void Configure(ModelBuilder builder);
}

/// <summary>
/// Base class for store-scoped read model definitions.
/// </summary>
/// <typeparam name="TStore">Marker type identifying the store.</typeparam>
public abstract class ReadModelDefinition<TStore> : IReadModelDefinition where TStore : class, IReadModelStoreMarker
{
    /// <inheritdoc />
    public Type Store => typeof(TStore);

    /// <inheritdoc />
    public abstract void Configure(ModelBuilder builder);
}
