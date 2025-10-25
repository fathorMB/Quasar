using System.Threading;
using System.Threading.Tasks;

namespace Quasar.Scheduling.Quartz;

/// <summary>
/// Represents a scheduled job that can be executed by the Quasar scheduling infrastructure
/// without taking a direct dependency on Quartz abstractions.
/// </summary>
public interface IQuasarJob
{
    /// <summary>
    /// Executes the job logic.
    /// </summary>
    /// <param name="context">Immutable context describing the current job invocation.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    Task ExecuteAsync(QuasarJobContext context, CancellationToken cancellationToken);
}
