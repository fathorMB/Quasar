using System.Threading;
using System.Threading.Tasks;

namespace Quasar.Persistence.Relational.EfCore;

/// <summary>
/// Applies database schema changes required for a read model context.
/// </summary>
/// <typeparam name="TContext">Read model context type.</typeparam>
public interface IReadModelSchemaInitializer<in TContext> where TContext : ReadModelContext
{
    /// <summary>
    /// Ensures the underlying database schema is compatible with the configured read model definitions.
    /// </summary>
    Task InitializeAsync(TContext context, CancellationToken cancellationToken = default);
}
