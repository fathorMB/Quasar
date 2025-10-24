using System.Data;
using System.Data.Common;
using Quasar.EventSourcing.Abstractions;

namespace Quasar.EventSourcing.SqlServer;

public sealed class SqlEventStore : IEventStore
{
    private readonly SqlEventStoreOptions _options;
    private readonly IEventSerializer _serializer;

    public SqlEventStore(SqlEventStoreOptions options, IEventSerializer serializer)
    {
        _options = options;
        _serializer = serializer;
    }

    public async Task AppendAsync(Guid streamId, int expectedVersion, IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
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
            var eventsTable = _options.EventsTable;

            // Lock the stream row range and check expected version
            var cmdGet = useConn.CreateCommand();
            cmdGet.Transaction = (DbTransaction)tx;
            cmdGet.CommandText = $"SELECT ISNULL(MAX([Version]), 0) FROM [dbo].[{eventsTable}] WITH (UPDLOCK, HOLDLOCK) WHERE [StreamId] = @StreamId";
            var pStream = cmdGet.CreateParameter();
            pStream.ParameterName = "@StreamId";
            pStream.Value = streamId;
            cmdGet.Parameters.Add(pStream);
            var currentVersionObj = await cmdGet.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            var currentVersion = currentVersionObj is int i ? i : Convert.ToInt32(currentVersionObj);

            if (currentVersion != expectedVersion)
                throw new ConcurrencyException($"Stream {streamId} expected version {expectedVersion} but was {currentVersion}.");

            var now = DateTime.UtcNow;
            var version = currentVersion;

            foreach (var e in events)
            {
                var payload = _serializer.Serialize(e, out var type);

                var cmdIns = useConn.CreateCommand();
                cmdIns.Transaction = (DbTransaction)tx;
                cmdIns.CommandText = $@"INSERT INTO [dbo].[{eventsTable}] ([StreamId],[Version],[Type],[Data],[Metadata],[CreatedUtc])
                                        VALUES (@StreamId,@Version,@Type,@Data,@Metadata,@CreatedUtc)";

                AddParam(cmdIns, "@StreamId", streamId);
                AddParam(cmdIns, "@Version", ++version);
                AddParam(cmdIns, "@Type", type);
                AddParam(cmdIns, "@Data", payload);
                AddParam(cmdIns, "@Metadata", DBNull.Value);
                AddParam(cmdIns, "@CreatedUtc", now);

                var rows = await cmdIns.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                if (rows != 1)
                    throw new ConcurrencyException($"Failed to append event at version {version} for stream {streamId}");
            }

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

    public async Task<IReadOnlyList<EventEnvelope>> ReadStreamAsync(Guid streamId, int fromVersion = 0, CancellationToken cancellationToken = default)
    {
        var ambient = SqlExecutionContext.Current;
        var ownsConnection = ambient is null;
        await using var conn = ownsConnection ? _options.ConnectionFactory() : null;
        var useConn = ambient?.Connection ?? conn!;
        if (useConn.State != ConnectionState.Open)
            await useConn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var eventsTable = _options.EventsTable;
        var cmd = useConn.CreateCommand();
        cmd.CommandText = $@"SELECT [Version],[Type],[Data],[Metadata],[CreatedUtc]
                             FROM [dbo].[{eventsTable}]
                             WHERE [StreamId] = @StreamId AND [Version] > @FromVersion
                             ORDER BY [Version]";
        AddParam(cmd, "@StreamId", streamId);
        AddParam(cmd, "@FromVersion", fromVersion);

        var list = new List<EventEnvelope>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var version = reader.GetInt32(0);
            var type = reader.GetString(1);
            var data = reader.GetString(2);
            var metadata = !await reader.IsDBNullAsync(3, cancellationToken).ConfigureAwait(false)
                ? reader.GetString(3)
                : null;
            var created = reader.GetDateTime(4);

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
