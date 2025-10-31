using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Quasar.Persistence.Relational.EfCore;

public sealed class ReadModelSchemaBootstrapper<TContext> : IReadModelSchemaBootstrapper
    where TContext : ReadModelContext
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ReadModelSchemaBootstrapper(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var initializer = scope.ServiceProvider.GetService<IReadModelSchemaInitializer<TContext>>();
        if (initializer is null)
        {
            return;
        }

        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        await initializer.InitializeAsync(context, cancellationToken).ConfigureAwait(false);
    }
}

