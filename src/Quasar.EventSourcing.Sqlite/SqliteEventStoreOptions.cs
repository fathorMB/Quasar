using System.Data.Common;

namespace Quasar.EventSourcing.Sqlite;

public sealed class SqliteEventStoreOptions
{
    public required Func<DbConnection> ConnectionFactory { get; init; }
    public string EventsTable { get; init; } = "events";
    public string SnapshotsTable { get; init; } = "snapshots";
}

public static class SqliteEventStoreSchema
{
    public static string CreateEventsTable(string table) => $@"
CREATE TABLE IF NOT EXISTS {table} (
    StreamId TEXT NOT NULL,
    Version INTEGER NOT NULL,
    Type TEXT NOT NULL,
    Data TEXT NOT NULL,
    Metadata TEXT NULL,
    CreatedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
    PRIMARY KEY (StreamId, Version)
);
CREATE INDEX IF NOT EXISTS IX_{table}_CreatedUtc ON {table} (CreatedUtc);
CREATE INDEX IF NOT EXISTS IX_{table}_Type ON {table} (Type);
";

    public static string CreateSnapshotsTable(string table) => $@"
CREATE TABLE IF NOT EXISTS {table} (
    StreamId TEXT NOT NULL PRIMARY KEY,
    Version INTEGER NOT NULL,
    Data BLOB NOT NULL,
    CreatedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now'))
);
";
}

public static class SqliteEventStoreInitializer
{
    public static async Task EnsureSchemaAsync(SqliteEventStoreOptions options, CancellationToken cancellationToken = default)
    {
        await using var conn = options.ConnectionFactory();
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = SqliteEventStoreSchema.CreateEventsTable(options.EventsTable) + SqliteEventStoreSchema.CreateSnapshotsTable(options.SnapshotsTable);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}

