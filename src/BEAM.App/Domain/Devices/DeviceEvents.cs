using Quasar.EventSourcing.Abstractions;

namespace BEAM.App.Domain.Devices;

/// <summary>
/// Event emitted when a new device is registered with the system.
/// </summary>
public sealed record DeviceRegistered(
    Guid DeviceId,
    string DeviceName,
    string DeviceType,
    string MacAddress,
    DateTimeOffset RegisteredAt) : IEvent;

/// <summary>
/// Event emitted when a device is activated.
/// </summary>
public sealed record DeviceActivated(
    Guid DeviceId,
    DateTimeOffset ActivatedAt) : IEvent;

/// <summary>
/// Event emitted when a device is deactivated.
/// </summary>
public sealed record DeviceDeactivated(
    Guid DeviceId,
    DateTimeOffset DeactivatedAt,
    string? Reason) : IEvent;

/// <summary>
/// Event emitted when a device's connection state changes.
/// </summary>
public sealed record DeviceConnectionStateChanged(
    Guid DeviceId,
    bool IsConnected,
    DateTimeOffset Timestamp) : IEvent;

/// <summary>
/// Event emitted when a device's name is updated.
/// </summary>
public sealed record DeviceNameUpdated(
    Guid DeviceId,
    string NewName,
    DateTimeOffset UpdatedAt) : IEvent;

/// <summary>
/// Event emitted when a device's heartbeat interval is updated.
/// </summary>
public sealed record DeviceHeartbeatIntervalUpdated(
    Guid DeviceId,
    int IntervalSeconds,
    DateTimeOffset UpdatedAt) : IEvent;
