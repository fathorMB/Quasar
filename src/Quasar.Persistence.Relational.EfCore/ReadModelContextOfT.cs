using Microsoft.EntityFrameworkCore;

namespace Quasar.Persistence.Relational.EfCore;

/// <summary>
/// Generic read model context bound to a specific store marker.
/// </summary>
/// <typeparam name="TStore">Marker type identifying the store.</typeparam>
public sealed class ReadModelContext<TStore> : ReadModelContext where TStore : class, IReadModelStoreMarker
{
    public ReadModelContext(
        DbContextOptions<ReadModelContext<TStore>> options,
        IReadModelModelSource modelSource)
        : base(options, modelSource, typeof(TStore))
    {
    }
}
