using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Quasar.Sagas.Persistence.Relational.EfCore;

internal sealed class SagaDbContextInitializerHostedService : IHostedService
{
    private readonly IServiceProvider _provider;

    public SagaDbContextInitializerHostedService(IServiceProvider provider)
    {
        _provider = provider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetService<SagaDbContext>();
        if (context is null)
        {
            return;
        }

        await context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
        await EnsureSagaTableExistsAsync(context, cancellationToken).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static Task EnsureSagaTableExistsAsync(SagaDbContext context, CancellationToken cancellationToken)
    {
        var provider = context.Database.ProviderName;
        if (string.IsNullOrWhiteSpace(provider))
        {
            return Task.CompletedTask;
        }

        if (provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            var sql = @"
CREATE TABLE IF NOT EXISTS QuasarSagaStates (
    Id TEXT NOT NULL PRIMARY KEY,
    SagaType TEXT NOT NULL,
    StateType TEXT NOT NULL,
    Payload TEXT NOT NULL,
    UpdatedUtc TEXT NOT NULL
);
CREATE INDEX IF NOT EXISTS IX_QuasarSagaStates_SagaType ON QuasarSagaStates (SagaType);
CREATE INDEX IF NOT EXISTS IX_QuasarSagaStates_StateType ON QuasarSagaStates (StateType);
";
            return context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }

        if (provider.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            var sql = @"
IF OBJECT_ID(N'[dbo].[QuasarSagaStates]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[QuasarSagaStates](
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [SagaType] NVARCHAR(256) NOT NULL,
        [StateType] NVARCHAR(256) NOT NULL,
        [Payload] NVARCHAR(MAX) NOT NULL,
        [UpdatedUtc] DATETIMEOFFSET NOT NULL
    );
    CREATE INDEX IX_QuasarSagaStates_SagaType ON [dbo].[QuasarSagaStates] ([SagaType]);
    CREATE INDEX IX_QuasarSagaStates_StateType ON [dbo].[QuasarSagaStates] ([StateType]);
END
";
            return context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }

        return Task.CompletedTask;
    }
}
