using System.Data;
using System.Data.Common;
using Quasar.Cqrs;

namespace Quasar.EventSourcing.Sqlite;

public sealed class SqliteCommandTransaction : ICommandTransaction
{
    private readonly SqliteEventStoreOptions _options;
    public SqliteCommandTransaction(SqliteEventStoreOptions options) => _options = options;

    public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default)
    {
        await using var connection = _options.ConnectionFactory();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        SqliteExecutionContext.Current = new AmbientSqliteContext
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
            SqliteExecutionContext.Current = null;
        }
    }
}

