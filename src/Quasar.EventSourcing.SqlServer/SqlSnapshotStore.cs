using System.Data;
using System.Data.Common;
using Quasar.EventSourcing.Abstractions;

namespace Quasar.EventSourcing.SqlServer;

public sealed class SqlSnapshotStore : ISnapshotStore
{
    private readonly SqlEventStoreOptions _options;

    public SqlSnapshotStore(SqlEventStoreOptions options)
    {
        _options = options;
    }

    public async Task<(bool found, int version, byte[] data)> TryGetAsync(Guid streamId, CancellationToken cancellationToken = default)
    {
        var ambient = SqlExecutionContext.Current;
        var ownsConnection = ambient is null;
        await using var conn = ownsConnection ? _options.ConnectionFactory() : null;
        var useConn = ambient?.Connection ?? conn!;
        if (useConn.State != ConnectionState.Open)
            await useConn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var table = _options.SnapshotsTable;
        var cmd = useConn.CreateCommand();
        cmd.CommandText = $@"SELECT [Version],[Data] FROM [dbo].[{table}] WHERE [StreamId] = @StreamId";
        AddParam(cmd, "@StreamId", streamId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var version = reader.GetInt32(0);
            var data = (byte[])reader[1];
            return (true, version, data);
        }
        return (false, 0, Array.Empty<byte>());
    }

    public async Task SaveAsync(Guid streamId, int version, byte[] data, CancellationToken cancellationToken = default)
    {
        var ambient = SqlExecutionContext.Current;
        var ownsConnection = ambient is null;
        await using var conn = ownsConnection ? _options.ConnectionFactory() : null;
        var useConn = ambient?.Connection ?? conn!;
        if (useConn.State != ConnectionState.Open)
            await useConn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var ownsTx = ambient is null;
        var tx = ownsTx ? await useConn.BeginTransactionAsync(cancellationToken).ConfigureAwait(false) : ambient!.Transaction;
        try
        {
            var table = _options.SnapshotsTable;
            // Upsert snapshot
            var cmd = useConn.CreateCommand();
            cmd.Transaction = (DbTransaction)tx;
            cmd.CommandText = $@"MERGE [dbo].[{table}] AS target
USING (SELECT @StreamId AS [StreamId], @Version AS [Version], @Data AS [Data]) AS src
ON (target.[StreamId] = src.[StreamId])
WHEN MATCHED THEN UPDATE SET [Version] = src.[Version], [Data] = src.[Data]
WHEN NOT MATCHED THEN INSERT ([StreamId],[Version],[Data]) VALUES (src.[StreamId], src.[Version], src.[Data]);";
            AddParam(cmd, "@StreamId", streamId);
            AddParam(cmd, "@Version", version);
            AddParam(cmd, "@Data", data);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            if (ownsTx && tx is not null)
                await ((DbTransaction)tx).CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            if (ownsTx && tx is not null)
                await ((DbTransaction)tx).RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    private static void AddParam(DbCommand cmd, string name, object value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }
}
