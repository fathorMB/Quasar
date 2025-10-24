using Microsoft.Extensions.Options;
using Npgsql;
using Quasar.Persistence.Abstractions;
using System.Text.Json;

namespace Quasar.Persistence.TimeSeries.Timescale;

internal sealed class TimescaleTimeSeriesReader : ITimeSeriesReader
{
    private readonly TimescaleOptions _options;

    public TimescaleTimeSeriesReader(IOptions<TimescaleOptions> options)
    {
        _options = options.Value;
    }

    public async Task<IReadOnlyList<TimeSeriesPoint>> ReadAsync(string metric, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var results = new List<TimeSeriesPoint>();

        await using var connection = new NpgsqlConnection(_options.ConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var sql = $"""
            SELECT time, tags, fields
            FROM "{_options.Schema}"."{_options.MetricsTable}"
            WHERE metric = @metric
              AND time >= @from
              AND time <= @to
            ORDER BY time;
            """;

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("metric", metric);
        cmd.Parameters.AddWithValue("from", fromUtc);
        cmd.Parameters.AddWithValue("to", toUtc);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var timestamp = reader.GetFieldValue<DateTime>(0);
            var tagsJson = reader.GetFieldValue<string>(1);
            var fieldsJson = reader.GetFieldValue<string>(2);

            var tags = JsonSerializer.Deserialize<Dictionary<string, string>>(tagsJson) ?? new();
            var fields = JsonSerializer.Deserialize<Dictionary<string, double>>(fieldsJson) ?? new();

            results.Add(new TimeSeriesPoint(timestamp, tags, fields));
        }

        return results;
    }
}
