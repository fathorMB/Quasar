using Quasar.Core;
using Quasar.Cqrs;
using Quasar.Security;

namespace BEAM.App.Domain.Devices;

/// <summary>
/// Command to register a new device with the system.
/// Note: Does not implement IAuthorizableRequest to allow anonymous registration in MVP.
/// </summary>
public sealed record RegisterDeviceCommand(
    Guid SubjectId,
    Guid DeviceId,
    string DeviceName,
    string DeviceType,
    string MacAddress) : ICommand<Result<Guid>>;  // Removed IAuthorizableRequest for anonymous registration

/// <summary>
/// Command to activate a device.
/// </summary>
public sealed record ActivateDeviceCommand(
    Guid SubjectId,
    Guid DeviceId) : ICommand<Result>, IAuthorizableRequest
{
    public string Action => "device.activate";
    public string Resource => $"device:{DeviceId}";
}

/// <summary>
/// Command to update device connection/heartbeat state.
/// </summary>
public sealed record UpdateDeviceConnectionStateCommand(
    Guid SubjectId,
    Guid DeviceId,
    bool IsConnected) : ICommand<Result>;

/// <summary>
/// Query to get a single device by ID.
/// </summary>
public sealed record GetDeviceQuery(Guid DeviceId) : IQuery<DeviceReadModel?>;

/// <summary>
/// Query to list all devices with pagination.
/// </summary>
public sealed record ListDevicesQuery(int Page = 1, int PageSize = 20) : IQuery<PagedResult<DeviceReadModel>>;

/// <summary>
/// Paginated result wrapper.
/// </summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize);

/// <summary>
/// Device read model for queries.
/// </summary>
public sealed class DeviceReadModel
{
    public Guid Id { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsConnected { get; set; }
    public DateTimeOffset RegisteredAt { get; set; }
    public DateTimeOffset? LastSeenAt { get; set; }
}

/// <summary>
/// Constants for device management.
/// </summary>
public static class DeviceConstants
{
    /// <summary>
    /// Stream ID for device events (all devices write to this single stream for simplicity in MVP).
    /// </summary>
    public static readonly Guid DeviceStreamId = Guid.Parse("bbbbbbbb-aaaa-dddd-cccc-111111111111");
}
