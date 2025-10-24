using System.Data;
using System.Data.Common;
using Quasar.Cqrs;

namespace Quasar.EventSourcing.SqlServer;

public sealed class SqlCommandTransaction : ICommandTransaction, IAsyncDisposable, IDisposable
{
    private readonly SqlEventStoreOptions _options;

    public SqlCommandTransaction(SqlEventStoreOptions options)
    {
        _options = options;
    }

    public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default)
    {
        await using var connection = _options.ConnectionFactory();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        SqlExecutionContext.Current = new AmbientSqlContext
        {
            Connection = connection,
            Transaction = (DbTransaction)transaction
        };

        try
        {
            var result = await action(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
        finally
        {
            SqlExecutionContext.Current = null;
        }
    }

    public ValueTask DisposeAsync()
    {
        SqlExecutionContext.Current = null;
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        SqlExecutionContext.Current = null;
    }
}

