using Quasar.Domain;

namespace BEAM.App.Domain.Devices;

/// <summary>
/// Aggregate root for device lifecycle management.
/// Enforces business rules and emits domain events.
/// </summary>
public sealed class DeviceAggregate : AggregateRoot
{
    // Track registered devices to enforce uniqueness and existence
    private readonly HashSet<Guid> _registeredDeviceIds = new();
    
    // We could track active state per device if needed, but for MVP we'll trust the projection/read model
    // or just allow idempotent events.

    public DeviceAggregate()
    {
        // Keep stream id consistent across rehydration and new instances
        Id = DeviceConstants.DeviceStreamId;
    }

    /// <summary>
    /// Registers a new device with the system.
    /// </summary>
    public void Register(Guid deviceId, string deviceName, string deviceType, string macAddress)
    {
        if (deviceId == Guid.Empty)
            throw new ArgumentException("Device ID cannot be empty", nameof(deviceId));
        
        if (_registeredDeviceIds.Contains(deviceId))
        {
            // Idempotency: if already registered, we could ignore or throw.
            // For now, let's assume it's a retry or re-registration.
            // But strictly, we should check if it's the SAME device info.
            // For MVP simplicity, if it's already in our list, we assume it's registered.
            return; 
        }

        ApplyChange(new DeviceRegistered(
            deviceId,
            deviceName,
            deviceType,
            macAddress,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Activates the device.
    /// </summary>
    public void Activate(Guid deviceId)
    {
        if (!_registeredDeviceIds.Contains(deviceId))
            throw new InvalidOperationException($"Device {deviceId} is not registered.");

        ApplyChange(new DeviceActivated(deviceId, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Deactivates the device.
    /// </summary>
    public void Deactivate(Guid deviceId, string? reason = null)
    {
        if (!_registeredDeviceIds.Contains(deviceId))
            return;

        ApplyChange(new DeviceDeactivated(deviceId, DateTimeOffset.UtcNow, reason));
    }

    /// <summary>
    /// Updates the device's connection state.
    /// </summary>
    public void UpdateConnectionState(Guid deviceId, bool isConnected)
    {
        if (!_registeredDeviceIds.Contains(deviceId))
            return;

        ApplyChange(new DeviceConnectionStateChanged(deviceId, isConnected, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Updates the device's name.
    /// </summary>
    public void UpdateDeviceName(Guid deviceId, string newName)
    {
        if (!_registeredDeviceIds.Contains(deviceId))
            throw new InvalidOperationException($"Device {deviceId} is not registered.");

        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Device name cannot be empty.", nameof(newName));

        ApplyChange(new DeviceNameUpdated(deviceId, newName, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Updates the device's heartbeat interval.
    /// </summary>
    public void UpdateHeartbeatInterval(Guid deviceId, int intervalSeconds)
    {
        if (!_registeredDeviceIds.Contains(deviceId))
            throw new InvalidOperationException($"Device {deviceId} is not registered.");

        if (intervalSeconds < 5 || intervalSeconds > 3600)
            throw new ArgumentException("Heartbeat interval must be between 5 and 3600 seconds.", nameof(intervalSeconds));

        ApplyChange(new DeviceHeartbeatIntervalUpdated(deviceId, intervalSeconds, DateTimeOffset.UtcNow));
    }

    // Event handlers (private) - called during replay and new events
    private void When(DeviceRegistered @event)
    {
        _registeredDeviceIds.Add(@event.DeviceId);
        // IMPORTANT: Do NOT overwrite Id here. The Aggregate Id is the Stream Id.
    }

    private void When(DeviceActivated @event)
    {
        // No state change needed for MVP registry
    }

    private void When(DeviceDeactivated @event)
    {
        // No state change needed for MVP registry
    }

    private void When(DeviceConnectionStateChanged @event)
    {
        // No state change needed for MVP registry
    }

    private void When(DeviceNameUpdated @event)
    {
        // No state change needed for MVP registry
    }

    private void When(DeviceHeartbeatIntervalUpdated @event)
    {
        // No state change needed for MVP registry
    }
}
