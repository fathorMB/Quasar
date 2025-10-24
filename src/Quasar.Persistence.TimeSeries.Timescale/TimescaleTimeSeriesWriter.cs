using Microsoft.Extensions.Options;
using Npgsql;
using Quasar.Persistence.Abstractions;
using System.Text.Json;

namespace Quasar.Persistence.TimeSeries.Timescale;

internal sealed class TimescaleTimeSeriesWriter : ITimeSeriesWriter
{
    private readonly TimescaleOptions _options;

    public TimescaleTimeSeriesWriter(IOptions<TimescaleOptions> options)
    {
        _options = options.Value;
    }

    public async Task WriteAsync(string metric, IEnumerable<TimeSeriesPoint> points, CancellationToken cancellationToken = default)
    {
        var rows = points.ToArray();
        if (rows.Length == 0) return;

        await using var connection = new NpgsqlConnection(_options.ConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var batchSize = Math.Max(1, _options.WriteBatchSize);
        for (var i = 0; i < rows.Length; i += batchSize)
        {
            var chunk = rows.Skip(i).Take(batchSize).ToArray();
            var copyCommand = $"COPY \"{_options.Schema}\".\"{_options.MetricsTable}\" (time, metric, tags, fields) FROM STDIN (FORMAT BINARY)";
            await using var writer = connection.BeginBinaryImport(copyCommand);

            foreach (var point in chunk)
            {
                await writer.StartRowAsync(cancellationToken).ConfigureAwait(false);
                await writer.WriteAsync(point.Timestamp, NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken).ConfigureAwait(false);
                await writer.WriteAsync(metric, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken).ConfigureAwait(false);
                await writer.WriteAsync(JsonSerializer.Serialize(point.Tags), NpgsqlTypes.NpgsqlDbType.Jsonb, cancellationToken).ConfigureAwait(false);
                await writer.WriteAsync(JsonSerializer.Serialize(point.Fields), NpgsqlTypes.NpgsqlDbType.Jsonb, cancellationToken).ConfigureAwait(false);
            }

            await writer.CompleteAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
