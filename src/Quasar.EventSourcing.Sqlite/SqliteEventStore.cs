using System.Data;
using System.Data.Common;
using Quasar.EventSourcing.Abstractions;

namespace Quasar.EventSourcing.Sqlite;

public sealed class SqliteEventStore : IEventStore
{
    private readonly SqliteEventStoreOptions _options;
    private readonly IEventSerializer _serializer;

    public SqliteEventStore(SqliteEventStoreOptions options, IEventSerializer serializer)
    {
        _options = options;
        _serializer = serializer;
    }

    public async Task AppendAsync(Guid streamId, int expectedVersion, IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
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
            var table = _options.EventsTable;

            // Lock emulation: SQLite serializes writes per connection/transaction
            var cmdGet = useConn.CreateCommand();
            cmdGet.Transaction = (DbTransaction)tx;
            cmdGet.CommandText = $"SELECT IFNULL(MAX(Version), 0) FROM {table} WHERE StreamId = @StreamId";
            AddParam(cmdGet, "@StreamId", streamId.ToString());
            var currentVersionObj = await cmdGet.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            var currentVersion = Convert.ToInt32(currentVersionObj);
            if (currentVersion != expectedVersion)
                throw new ConcurrencyException($"Stream {streamId} expected version {expectedVersion} but was {currentVersion}.");

            var now = DateTime.UtcNow.ToString("O");
            var version = currentVersion;
            foreach (var e in events)
            {
                var payload = _serializer.Serialize(e, out var type);
                var cmdIns = useConn.CreateCommand();
                cmdIns.Transaction = (DbTransaction)tx;
                cmdIns.CommandText = $@"INSERT INTO {table} (StreamId,Version,Type,Data,Metadata,CreatedUtc)
                                       VALUES (@StreamId,@Version,@Type,@Data,@Metadata,@CreatedUtc)";
                AddParam(cmdIns, "@StreamId", streamId.ToString());
                AddParam(cmdIns, "@Version", ++version);
                AddParam(cmdIns, "@Type", type);
                AddParam(cmdIns, "@Data", payload);
                AddParam(cmdIns, "@Metadata", DBNull.Value);
                AddParam(cmdIns, "@CreatedUtc", now);

                var rows = await cmdIns.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                if (rows != 1)
                    throw new ConcurrencyException($"Failed to append event at version {version} for stream {streamId}");
            }

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

    public async Task<IReadOnlyList<EventEnvelope>> ReadStreamAsync(Guid streamId, int fromVersion = 0, CancellationToken cancellationToken = default)
    {
        var ambient = SqliteExecutionContext.Current;
        var ownsConnection = ambient is null;
        await using var conn = ownsConnection ? _options.ConnectionFactory() : null;
        var useConn = ambient?.Connection ?? conn!;
        if (useConn.State != ConnectionState.Open)
            await useConn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var table = _options.EventsTable;
        var cmd = useConn.CreateCommand();
        cmd.CommandText = $@"SELECT Version,Type,Data,Metadata,CreatedUtc
                             FROM {table}
                             WHERE StreamId = @StreamId AND Version > @FromVersion
                             ORDER BY Version";
        AddParam(cmd, "@StreamId", streamId.ToString());
        AddParam(cmd, "@FromVersion", fromVersion);

        var list = new List<EventEnvelope>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var version = reader.GetInt32(0);
            var type = reader.GetString(1);
            var data = reader.GetString(2);
            var created = DateTime.Parse(reader.GetString(4));
            var evt = _serializer.Deserialize(data, type);
            list.Add(new EventEnvelope(streamId, version, created, evt, null));
        }
        return list;
    }

    private static void AddParam(DbCommand cmd, string name, object value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }
}

