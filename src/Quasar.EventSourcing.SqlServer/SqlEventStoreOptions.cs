using System.Data.Common;

namespace Quasar.EventSourcing.SqlServer;

public sealed class SqlEventStoreOptions
{
    public required Func<DbConnection> ConnectionFactory { get; init; }
    public string EventsTable { get; init; } = "Events";
    public string SnapshotsTable { get; init; } = "Snapshots";
}

public static class SqlEventStoreSchema
{
    public static string CreateEventsTable(string table) => $@"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{table}]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[{table}] (
        [StreamId] UNIQUEIDENTIFIER NOT NULL,
        [Version] INT NOT NULL,
        [Type] NVARCHAR(256) NOT NULL,
        [Data] NVARCHAR(MAX) NOT NULL,
        [Metadata] NVARCHAR(MAX) NULL,
        [CreatedUtc] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT [PK_{table}] PRIMARY KEY CLUSTERED ([StreamId] ASC, [Version] ASC)
    );
    CREATE INDEX [IX_{table}_CreatedUtc] ON [dbo].[{table}] ([CreatedUtc]);
    CREATE INDEX [IX_{table}_Type] ON [dbo].[{table}] ([Type]);
END";

    public static string CreateSnapshotsTable(string table) => $@"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{table}]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[{table}] (
        [StreamId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Version] INT NOT NULL,
        [Data] VARBINARY(MAX) NOT NULL,
        [CreatedUtc] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END";
}

public static class SqlEventStoreInitializer
{
    public static async Task EnsureSchemaAsync(SqlEventStoreOptions options, CancellationToken cancellationToken = default)
    {
        await using var conn = options.ConnectionFactory();
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = SqlEventStoreSchema.CreateEventsTable(options.EventsTable) + ";\n" + SqlEventStoreSchema.CreateSnapshotsTable(options.SnapshotsTable);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
