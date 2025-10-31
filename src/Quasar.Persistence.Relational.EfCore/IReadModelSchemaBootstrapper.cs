using System.Threading;
using System.Threading.Tasks;

namespace Quasar.Persistence.Relational.EfCore;

/// <summary>
/// Executes schema initialization for a configured read model context.
/// </summary>
public interface IReadModelSchemaBootstrapper
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
