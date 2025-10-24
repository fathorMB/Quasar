namespace Quasar.Persistence.Abstractions;

public sealed record TimeSeriesPoint(
    DateTime Timestamp,
    IReadOnlyDictionary<string, string> Tags,
    IReadOnlyDictionary<string, double> Fields);

public interface ITimeSeriesWriter
{
    Task WriteAsync(string metric, IEnumerable<TimeSeriesPoint> points, CancellationToken cancellationToken = default);
}

public interface ITimeSeriesReader
{
    Task<IReadOnlyList<TimeSeriesPoint>> ReadAsync(string metric, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
}

