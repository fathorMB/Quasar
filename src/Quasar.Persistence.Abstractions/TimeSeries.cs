namespace Quasar.Persistence.Abstractions;

/// <summary>
/// Represents a single data point in a time series metric.
/// </summary>
/// <param name="Timestamp">The moment the measurement was captured (UTC).</param>
/// <param name="Tags">Label values describing the measurement.</param>
/// <param name="Fields">Numeric fields recorded for the measurement.</param>
public sealed record TimeSeriesPoint(
    DateTime Timestamp,
    IReadOnlyDictionary<string, string> Tags,
    IReadOnlyDictionary<string, double> Fields);

/// <summary>
/// Writes time series data points to the backing store.
/// </summary>
public interface ITimeSeriesWriter
{
    /// <summary>
    /// Writes the provided <paramref name="points"/> to the metric identified by <paramref name="metric"/>.
    /// </summary>
    Task WriteAsync(string metric, IEnumerable<TimeSeriesPoint> points, CancellationToken cancellationToken = default);
}

/// <summary>
/// Reads time series data from the backing store.
/// </summary>
public interface ITimeSeriesReader
{
    /// <summary>
    /// Retrieves all points for the given <paramref name="metric"/> between <paramref name="fromUtc"/> and <paramref name="toUtc"/>.
    /// </summary>
    Task<IReadOnlyList<TimeSeriesPoint>> ReadAsync(string metric, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
}
