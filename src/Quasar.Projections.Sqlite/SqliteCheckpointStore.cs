using System.Data.Common;

namespace Quasar.Projections.Sqlite;

public sealed class SqliteCheckpointStore : Quasar.Projections.Abstractions.ICheckpointStore
{
    private readonly Func<DbConnection> _connectionFactory;
    private readonly string _table;

    public SqliteCheckpointStore(Func<DbConnection> connectionFactory, string table = "projection_checkpoints")
    {
        _connectionFactory = connectionFactory;
        _table = table;
    }

    public async Task<long> GetCheckpointAsync(string projectorName, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken).ConfigureAwait(false);
        await using var conn = _connectionFactory();
        if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT Position FROM {_table} WHERE Projector = @p";
        var p = cmd.CreateParameter(); p.ParameterName = "@p"; p.Value = projectorName; cmd.Parameters.Add(p);
        var val = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (val == null || val == DBNull.Value) return 0L;
        return Convert.ToInt64(val);
    }

    public async Task SaveCheckpointAsync(string projectorName, long position, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken).ConfigureAwait(false);
        await using var conn = _connectionFactory();
        if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $@"INSERT INTO {_table} (Projector, Position)
VALUES (@p, @pos)
ON CONFLICT(Projector) DO UPDATE SET Position = excluded.Position";
        var p = cmd.CreateParameter(); p.ParameterName = "@p"; p.Value = projectorName; cmd.Parameters.Add(p);
        var pos = cmd.CreateParameter(); pos.ParameterName = "@pos"; pos.Value = position; cmd.Parameters.Add(pos);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task EnsureSchemaAsync(CancellationToken ct)
    {
        await using var conn = _connectionFactory();
        if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $@"CREATE TABLE IF NOT EXISTS {_table} (
    Projector TEXT NOT NULL PRIMARY KEY,
    Position INTEGER NOT NULL
);";
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }
}

