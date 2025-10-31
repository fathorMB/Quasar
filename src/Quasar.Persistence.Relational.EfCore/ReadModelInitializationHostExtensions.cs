using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Quasar.Persistence.Relational.EfCore;

/// <summary>
/// Host extensions for read model schema management.
/// </summary>
public static class ReadModelInitializationHostExtensions
{
    /// <summary>
    /// Runs all registered read model schema bootstrappers.
    /// </summary>
    public static async Task InitializeReadModelsAsync(this IHost host, CancellationToken cancellationToken = default)
    {
        using var scope = host.Services.CreateScope();
        var bootstrappers = scope.ServiceProvider.GetServices<IReadModelSchemaBootstrapper>();
        foreach (var bootstrapper in bootstrappers)
        {
            await bootstrapper.InitializeAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
