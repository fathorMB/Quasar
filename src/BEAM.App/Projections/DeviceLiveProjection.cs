using Quasar.Projections.Abstractions;
using BEAM.App.Domain.Devices;
using BEAM.App.ReadModels;
using BEAM.App.Services;
using BEAM.App.Models;

namespace BEAM.App.Projections;

/// <summary>
/// Live projection that updates device read models in real-time and broadcasts changes
/// immediately to all connected clients via SignalR.
/// </summary>
public sealed class DeviceLiveProjection : 
    ILiveProjection<DeviceRegistered>,
    ILiveProjection<DeviceActivated>,
    ILiveProjection<DeviceDeactivated>,
    ILiveProjection<DeviceConnectionStateChanged>,
    ILiveProjection<DeviceNameUpdated>,
    ILiveProjection<DeviceHeartbeatIntervalUpdated>
{
    private readonly ILiveReadModelStore _store;
    private readonly ILiveReadModelNotifier _notifier;
    private readonly IDeviceConnectionManager _deviceConnectionManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceLiveProjection"/> class.
    /// </summary>
    public DeviceLiveProjection(
        ILiveReadModelStore store, 
        ILiveReadModelNotifier notifier,
        IDeviceConnectionManager deviceConnectionManager)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
        _deviceConnectionManager = deviceConnectionManager ?? throw new ArgumentNullException(nameof(deviceConnectionManager));
    }

    /// <summary>
    /// Handles device registration events.
    /// </summary>
    public async Task HandleAsync(DeviceRegistered @event, CancellationToken cancellationToken = default)
    {
        var model = new BEAM.App.Domain.Devices.DeviceReadModel
        {
            Id = @event.DeviceId,
            DeviceName = @event.DeviceName,
            DeviceType = @event.DeviceType,
            MacAddress = @event.MacAddress,
            RegisteredAt = @event.RegisteredAt,
            IsActive = false,
            IsConnected = false,
            LastSeenAt = DateTimeOffset.UtcNow
        };

        await _store.UpsertAsync(@event.DeviceId.ToString(), model, cancellationToken);
        await _notifier.NotifyUpsertAsync(@event.DeviceId.ToString(), model, cancellationToken);
    }

    /// <summary>
    /// Handles device activation events.
    /// </summary>
    public async Task HandleAsync(DeviceActivated @event, CancellationToken cancellationToken = default)
    {
        var existing = await _store.GetAsync<BEAM.App.Domain.Devices.DeviceReadModel>(@event.DeviceId.ToString(), cancellationToken);
        if (existing is null)
            return;

        existing.IsActive = true;
        existing.LastSeenAt = DateTimeOffset.UtcNow;

        await _store.UpsertAsync(@event.DeviceId.ToString(), existing, cancellationToken);
        await _notifier.NotifyUpsertAsync(@event.DeviceId.ToString(), existing, cancellationToken);
    }

    /// <summary>
    /// Handles device deactivation events.
    /// </summary>
    public async Task HandleAsync(DeviceDeactivated @event, CancellationToken cancellationToken = default)
    {
        var existing = await _store.GetAsync<BEAM.App.Domain.Devices.DeviceReadModel>(@event.DeviceId.ToString(), cancellationToken);
        if (existing is null)
            return;

        existing.IsActive = false;
        existing.IsConnected = false;
        existing.LastSeenAt = DateTimeOffset.UtcNow;

        await _store.UpsertAsync(@event.DeviceId.ToString(), existing, cancellationToken);
        await _notifier.NotifyUpsertAsync(@event.DeviceId.ToString(), existing, cancellationToken);
    }

    /// <summary>
    /// Handles device connection state changes.
    /// </summary>
    public async Task HandleAsync(DeviceConnectionStateChanged @event, CancellationToken cancellationToken = default)
    {
        var existing = await _store.GetAsync<BEAM.App.Domain.Devices.DeviceReadModel>(@event.DeviceId.ToString(), cancellationToken);
        if (existing is null)
            return;

        existing.IsConnected = @event.IsConnected;
        existing.LastSeenAt = DateTimeOffset.UtcNow;

        await _store.UpsertAsync(@event.DeviceId.ToString(), existing, cancellationToken);
        await _notifier.NotifyUpsertAsync(@event.DeviceId.ToString(), existing, cancellationToken);
    }

    /// <summary>
    /// Handles device name update events and pushes configuration to connected devices.
    /// </summary>
    public async Task HandleAsync(DeviceNameUpdated @event, CancellationToken cancellationToken = default)
    {
        var existing = await _store.GetAsync<BEAM.App.Domain.Devices.DeviceReadModel>(@event.DeviceId.ToString(), cancellationToken);
        if (existing is null)
            return;

        existing.DeviceName = @event.NewName;
        existing.LastSeenAt = DateTimeOffset.UtcNow;

        await _store.UpsertAsync(@event.DeviceId.ToString(), existing, cancellationToken);
        await _notifier.NotifyUpsertAsync(@event.DeviceId.ToString(), existing, cancellationToken);

        // Push configuration update to connected device
        if (_deviceConnectionManager.IsDeviceConnected(@event.DeviceId))
        {
            await _deviceConnectionManager.SendConfigurationAsync(@event.DeviceId, new DeviceConfiguration
            {
                DeviceName = @event.NewName,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }
    }

    /// <summary>
    /// Handles device heartbeat interval update events and pushes configuration to connected devices.
    /// </summary>
    public async Task HandleAsync(DeviceHeartbeatIntervalUpdated @event, CancellationToken cancellationToken = default)
    {
        var existing = await _store.GetAsync<BEAM.App.Domain.Devices.DeviceReadModel>(@event.DeviceId.ToString(), cancellationToken);
        if (existing is null)
            return;

        existing.HeartbeatIntervalSeconds = @event.IntervalSeconds;
        existing.LastSeenAt = DateTimeOffset.UtcNow;

        await _store.UpsertAsync(@event.DeviceId.ToString(), existing, cancellationToken);
        await _notifier.NotifyUpsertAsync(@event.DeviceId.ToString(), existing, cancellationToken);

        // Push configuration update to connected device
        if (_deviceConnectionManager.IsDeviceConnected(@event.DeviceId))
        {
            await _deviceConnectionManager.SendConfigurationAsync(@event.DeviceId, new DeviceConfiguration
            {
                HeartbeatIntervalSeconds = @event.IntervalSeconds,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }
    }
}
