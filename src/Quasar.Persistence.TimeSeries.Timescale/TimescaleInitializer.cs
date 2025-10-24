using Npgsql;
using System.Text;

namespace Quasar.Persistence.TimeSeries.Timescale;

/// <summary>
/// Handles database and schema initialization for TimescaleDB integrations.
/// </summary>
internal static class TimescaleInitializer
{
    /// <summary>
    /// Ensures that the target database and hypertable exist for the supplied <paramref name="options"/>.
    /// </summary>
    public static async Task EnsureSchemaAsync(TimescaleOptions options, CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseExistsAsync(options, cancellationToken).ConfigureAwait(false);

        await using var connection = new NpgsqlConnection(options.ConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var commandText = new StringBuilder()
            .AppendLine($"CREATE SCHEMA IF NOT EXISTS \"{options.Schema}\";")
            .AppendLine($"""
                CREATE TABLE IF NOT EXISTS "{options.Schema}"."{options.MetricsTable}" (
                    time TIMESTAMPTZ NOT NULL,
                    metric TEXT NOT NULL,
                    tags JSONB NOT NULL,
                    fields JSONB NOT NULL
                );
                """)
            .AppendLine($"""
                SELECT create_hypertable(
                    '{options.Schema}.{options.MetricsTable}',
                    'time',
                    if_not_exists => TRUE
                );
                """)
            .ToString();

        await using var cmd = new NpgsqlCommand(commandText, connection);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureDatabaseExistsAsync(TimescaleOptions options, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new NpgsqlConnection(options.ConnectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return;
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.InvalidCatalogName)
        {
            // database missing - we will create it
        }

        var builder = new NpgsqlConnectionStringBuilder(options.ConnectionString);
        var databaseName = options.Database ?? builder.Database;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("Timescale options must include a database name.");
        }

        var adminBuilder = new NpgsqlConnectionStringBuilder(options.ConnectionString)
        {
            Database = "postgres"
        };

        await using var adminConnection = new NpgsqlConnection(adminBuilder.ConnectionString);
        await adminConnection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var createCommand = new StringBuilder()
            .Append($"CREATE DATABASE \"{databaseName}\"");
        if (!string.IsNullOrWhiteSpace(adminBuilder.Username))
        {
            createCommand.Append($" OWNER \"{adminBuilder.Username}\"");
        }
        createCommand.Append(';');

        await using var cmd = new NpgsqlCommand(createCommand.ToString(), adminConnection);
        try
        {
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.DuplicateDatabase)
        {
            // already created by another process
        }
    }
}
