using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Quasar.Persistence.Relational.EfCore;

/// <summary>
/// Hosted service that runs schema initialization for a specific read model context.
/// </summary>
/// <typeparam name="TContext">Context whose schema should be initialized.</typeparam>
public sealed class ReadModelSchemaInitializerHostedService<TContext> : IHostedService
    where TContext : ReadModelContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReadModelSchemaInitializerHostedService<TContext>> _logger;

    public ReadModelSchemaInitializerHostedService(
        IServiceProvider serviceProvider,
        ILogger<ReadModelSchemaInitializerHostedService<TContext>> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var initializer = scope.ServiceProvider.GetService<IReadModelSchemaInitializer<TContext>>();
        if (initializer is null)
        {
            _logger.LogDebug("No schema initializer registered for {ContextType}. Skipping read model schema bootstrapping.", typeof(TContext).Name);
            return;
        }

        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        await initializer.InitializeAsync(context, cancellationToken).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
