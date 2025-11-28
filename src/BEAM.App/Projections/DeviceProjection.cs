using Microsoft.EntityFrameworkCore;
using Quasar.Projections.Abstractions;
using Quasar.Persistence.Relational.EfCore;
using BEAM.App.Domain.Devices;
using BEAM.App.ReadModels;

namespace BEAM.App.Projections;

/// <summary>
/// Projection that maintains device read models from device events.
/// </summary>
public sealed class DeviceProjection :
    IProjection<DeviceRegistered>,
    IProjection<DeviceActivated>,
    IProjection<DeviceDeactivated>,
    IProjection<DeviceConnectionStateChanged>,
    IProjection<DeviceNameUpdated>,
    IProjection<DeviceHeartbeatIntervalUpdated>
{
    private readonly ReadModelContext<BeamReadModelStore> _db;
    private readonly DbSet<DeviceReadModel> _devices;
    private readonly Microsoft.Extensions.Logging.ILogger<DeviceProjection> _logger;

    public DeviceProjection(
        ReadModelContext<BeamReadModelStore> db,
        Microsoft.Extensions.Logging.ILogger<DeviceProjection> logger)
    {
        _db = db;
        _devices = db.Set<DeviceReadModel>();
        _logger = logger;
    }

    public async Task HandleAsync(DeviceRegistered @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Projection: Processing DeviceRegistered for {DeviceId}", @event.DeviceId);
        
        var device = new DeviceReadModel
        {
            Id = @event.DeviceId,
            DeviceName = @event.DeviceName,
            DeviceType = @event.DeviceType,
            MacAddress = @event.MacAddress,
            RegisteredAt = @event.RegisteredAt,
            IsActive = false,
            IsConnected = false
        };

        await _devices.AddAsync(device, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Projection: Device {DeviceId} added to read model", @event.DeviceId);
    }

    public async Task HandleAsync(DeviceActivated @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Projection: Processing DeviceActivated for {DeviceId}", @event.DeviceId);
        
        var device = await _devices.FirstOrDefaultAsync(d => d.Id == @event.DeviceId, cancellationToken);
        if (device == null)
        {
            _logger.LogWarning("Projection: Device {DeviceId} not found for activation", @event.DeviceId);
            return;
        }

        device.IsActive = true;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task HandleAsync(DeviceDeactivated @event, CancellationToken cancellationToken = default)
    {
        var device = await _devices.FirstOrDefaultAsync(d => d.Id == @event.DeviceId, cancellationToken);
        if (device == null) return;

        device.IsActive = false;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task HandleAsync(DeviceConnectionStateChanged @event, CancellationToken cancellationToken = default)
    {
        var device = await _devices.FirstOrDefaultAsync(d => d.Id == @event.DeviceId, cancellationToken);
        if (device == null) return;

        device.IsConnected = @event.IsConnected;
        device.IsActive = @event.IsConnected;
        device.LastSeenAt = @event.Timestamp;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task HandleAsync(DeviceNameUpdated @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Projection: Processing DeviceNameUpdated for {DeviceId}", @event.DeviceId);
        
        var device = await _devices.FirstOrDefaultAsync(d => d.Id == @event.DeviceId, cancellationToken);
        if (device == null)
        {
            _logger.LogWarning("Projection: Device {DeviceId} not found for name update", @event.DeviceId);
            return;
        }

        device.DeviceName = @event.NewName;
        await _db.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Projection: Device {DeviceId} name updated to {NewName}", @event.DeviceId, @event.NewName);
    }

    public async Task HandleAsync(DeviceHeartbeatIntervalUpdated @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Projection: Processing DeviceHeartbeatIntervalUpdated for {DeviceId}", @event.DeviceId);
        
        var device = await _devices.FirstOrDefaultAsync(d => d.Id == @event.DeviceId, cancellationToken);
        if (device == null)
        {
            _logger.LogWarning("Projection: Device {DeviceId} not found for heartbeat interval update", @event.DeviceId);
            return;
        }

        device.HeartbeatIntervalSeconds = @event.IntervalSeconds;
        await _db.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Projection: Device {DeviceId} heartbeat interval updated to {Interval}s", @event.DeviceId, @event.IntervalSeconds);
    }
}
