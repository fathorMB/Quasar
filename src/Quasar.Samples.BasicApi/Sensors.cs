using System.Linq;
using Quasar.Cqrs;
using Quasar.Domain;
using Quasar.EventSourcing.Abstractions;
using Quasar.Persistence.Abstractions;
using Quasar.Security;
using Quasar.Core;
using System.Collections.Generic;

namespace Quasar.Samples.BasicApi;

public sealed record SensorReadingRecorded(Guid DeviceId, string SensorType, double Value, DateTime TimestampUtc) : IEvent;

public sealed record IngestSensorReadingCommand(Guid SubjectId, Guid DeviceId, string SensorType, double Value, DateTime TimestampUtc) : ICommand<Result<bool>>, IAuthorizableRequest
{
    public string Action => "sensor.ingest";
    public string Resource => $"sensor:{DeviceId}";
}

public sealed class SensorStreamAggregate : AggregateRoot
{
    public void Record(Guid deviceId, string sensorType, double value, DateTime timestampUtc)
    {
        if (deviceId == Guid.Empty) throw new ArgumentException("DeviceId required", nameof(deviceId));
        if (string.IsNullOrWhiteSpace(sensorType)) throw new ArgumentException("SensorType required", nameof(sensorType));
        ApplyChange(new SensorReadingRecorded(deviceId, sensorType, value, timestampUtc));
    }

    private void When(SensorReadingRecorded e)
    {
        Id = SampleConfig.SensorStreamId;
    }
}

public sealed class IngestSensorReadingHandler : ICommandHandler<IngestSensorReadingCommand, Result<bool>>
{
    private readonly IEventSourcedRepository<SensorStreamAggregate> _repo;

    public IngestSensorReadingHandler(IEventSourcedRepository<SensorStreamAggregate> repo)
    {
        _repo = repo;
    }

    public async Task<Result<bool>> Handle(IngestSensorReadingCommand command, CancellationToken cancellationToken = default)
    {
        var aggregate = await _repo.GetAsync(SampleConfig.SensorStreamId, cancellationToken);
        if (aggregate.Id == Guid.Empty)
        {
            aggregate = new SensorStreamAggregate();
        }
        aggregate.Record(command.DeviceId, command.SensorType, command.Value, command.TimestampUtc);
        await _repo.SaveAsync(aggregate, cancellationToken);

        return Result<bool>.Success(true);
    }
}

public sealed class SensorIngestValidator : IValidator<IngestSensorReadingCommand>
{
    public Task<List<Error>> ValidateAsync(IngestSensorReadingCommand instance, CancellationToken cancellationToken = default)
    {
        var errors = new List<Error>();
        if (instance.DeviceId == Guid.Empty)
        {
            errors.Add(new Error("validation.device_id", "DeviceId required"));
        }
        if (string.IsNullOrWhiteSpace(instance.SensorType))
        {
            errors.Add(new Error("validation.sensor_type", "SensorType required"));
        }
        return Task.FromResult(errors);
    }
}

public sealed record SensorReadingsQuery(Guid DeviceId, DateTime FromUtc, DateTime ToUtc) : IQuery<IReadOnlyList<TimeSeriesPoint>>;

public sealed class SensorReadingsHandler : IQueryHandler<SensorReadingsQuery, IReadOnlyList<TimeSeriesPoint>>
{
    private readonly ITimeSeriesReader _reader;

    public SensorReadingsHandler(ITimeSeriesReader reader)
    {
        _reader = reader;
    }

    public Task<IReadOnlyList<TimeSeriesPoint>> Handle(SensorReadingsQuery query, CancellationToken cancellationToken = default)
        => FilterByDeviceAsync(query, cancellationToken);

    private async Task<IReadOnlyList<TimeSeriesPoint>> FilterByDeviceAsync(SensorReadingsQuery query, CancellationToken cancellationToken)
    {
        var points = await _reader.ReadAsync(SensorConstants.MetricName, query.FromUtc, query.ToUtc, cancellationToken).ConfigureAwait(false);
        return points
            .Where(p => p.Tags.TryGetValue("deviceId", out var id) && Guid.TryParse(id, out var parsed) && parsed == query.DeviceId)
            .ToArray();
    }
}

public static class SensorConstants
{
    public const string MetricName = "sensor_readings";
}
