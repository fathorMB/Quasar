using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace Quasar.EventSourcing.Sqlite;

/// <summary>
/// Provides access to the ambient SQLite connection/transaction used by the command pipeline.
/// </summary>
public static class SqliteAmbientContextAccessor
{
    /// <summary>
    /// Attempts to retrieve the current ambient SQLite connection and transaction.
    /// </summary>
    public static bool TryGet(out SqliteConnection connection, out DbTransaction transaction)
    {
        var ambient = SqliteExecutionContext.Current;
        if (ambient?.Connection is SqliteConnection sqlite && ambient.Transaction is DbTransaction tx)
        {
            connection = sqlite;
            transaction = tx;
            return true;
        }

        connection = null!;
        transaction = null!;
        return false;
    }
}
