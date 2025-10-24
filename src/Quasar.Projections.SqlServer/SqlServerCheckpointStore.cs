using System.Data.Common;

namespace Quasar.Projections.SqlServer;

public sealed class SqlServerCheckpointStore : Quasar.Projections.Abstractions.ICheckpointStore
{
    private readonly Func<DbConnection> _connectionFactory;
    private readonly string _table;

    public SqlServerCheckpointStore(Func<DbConnection> connectionFactory, string table = "ProjectionCheckpoints")
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
        cmd.CommandText = $"SELECT [Position] FROM [dbo].[{_table}] WHERE [Projector] = @p";
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
        cmd.CommandText = $@"MERGE [dbo].[{_table}] AS t
USING (SELECT @p AS [Projector], @pos AS [Position]) AS s
ON t.[Projector] = s.[Projector]
WHEN MATCHED THEN UPDATE SET t.[Position] = s.[Position]
WHEN NOT MATCHED THEN INSERT ([Projector],[Position]) VALUES (s.[Projector], s.[Position]);";
        var p = cmd.CreateParameter(); p.ParameterName = "@p"; p.Value = projectorName; cmd.Parameters.Add(p);
        var pos = cmd.CreateParameter(); pos.ParameterName = "@pos"; pos.Value = position; cmd.Parameters.Add(pos);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task EnsureSchemaAsync(CancellationToken ct)
    {
        await using var conn = _connectionFactory();
        if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $@"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{_table}]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[{_table}] (
        [Projector] NVARCHAR(256) NOT NULL PRIMARY KEY,
        [Position] BIGINT NOT NULL
    );
END";
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }
}

