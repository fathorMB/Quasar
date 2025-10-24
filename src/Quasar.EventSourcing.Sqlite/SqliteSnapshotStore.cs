using System.Data;
using System.Data.Common;
using Quasar.EventSourcing.Abstractions;

namespace Quasar.EventSourcing.Sqlite;

public sealed class SqliteSnapshotStore : ISnapshotStore
{
    private readonly SqliteEventStoreOptions _options;
    public SqliteSnapshotStore(SqliteEventStoreOptions options) => _options = options;

    public async Task<(bool found, int version, byte[] data)> TryGetAsync(Guid streamId, CancellationToken cancellationToken = default)
    {
        var ambient = SqliteExecutionContext.Current;
        var ownsConnection = ambient is null;
        await using var conn = ownsConnection ? _options.ConnectionFactory() : null;
        var useConn = ambient?.Connection ?? conn!;
        if (useConn.State != ConnectionState.Open)
            await useConn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var table = _options.SnapshotsTable;
        var cmd = useConn.CreateCommand();
        cmd.CommandText = $"SELECT Version, Data FROM {table} WHERE StreamId = @StreamId";
        AddParam(cmd, "@StreamId", streamId.ToString());
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var version = reader.GetInt32(0);
            var bytes = (byte[])reader[1];
            return (true, version, bytes);
        }
        return (false, 0, Array.Empty<byte>());
    }

    public async Task SaveAsync(Guid streamId, int version, byte[] data, CancellationToken cancellationToken = default)
    {
        var ambient = SqliteExecutionContext.Current;
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
            var cmd = useConn.CreateCommand();
            cmd.Transaction = (DbTransaction)tx;
            // UPSERT using SQLite ON CONFLICT
            cmd.CommandText = $@"INSERT INTO {table} (StreamId, Version, Data)
                                VALUES (@StreamId, @Version, @Data)
                                ON CONFLICT(StreamId) DO UPDATE SET Version = excluded.Version, Data = excluded.Data";
            AddParam(cmd, "@StreamId", streamId.ToString());
            AddParam(cmd, "@Version", version);
            AddParam(cmd, "@Data", data);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            if (ownsTx)
                await ((DbTransaction)tx).CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            if (ownsTx)
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

