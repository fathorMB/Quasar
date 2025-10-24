using Npgsql;
using System.Text;

namespace Quasar.Persistence.TimeSeries.Timescale;

internal static class TimescaleInitializer
{
    public static async Task EnsureSchemaAsync(TimescaleOptions options, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(options.ConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var tableIdentifier = $"{options.Schema}.{options.MetricsTable}";
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
}
