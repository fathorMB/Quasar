using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Quasar.Cqrs;
using Quasar.Core;
using BEAM.App.Domain.Devices;
using BEAM.App.Models;
using BEAM.App.Services;

namespace BEAM.App.Hubs;

/// <summary>
/// Client-side interface for device SignalR connections.
/// Defines methods that the server can invoke on connected devices.
/// </summary>
public interface IDeviceClient
{
    /// <summary>
    /// Sends a configuration update to the device.
    /// </summary>
    Task ReceiveConfigurationUpdate(DeviceConfiguration config);
}

/// <summary>
/// SignalR hub for real-time communication with IoT devices.
/// Handles device connections, heartbeats, and configuration push.
/// </summary>
[Authorize]
public class DeviceHub : Hub<IDeviceClient>
{
    private readonly IDeviceConnectionManager _connectionManager;
    private readonly IMediator _mediator;
    private readonly ILogger<DeviceHub> _logger;

    public DeviceHub(
        IDeviceConnectionManager connectionManager,
        IMediator mediator,
        ILogger<DeviceHub> logger)
    {
        _connectionManager = connectionManager;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Registers a device with the hub.
    /// Called by the device when first connecting.
    /// </summary>
    public async Task RegisterDevice()
    {
        if (!Guid.TryParse(Context.UserIdentifier, out var deviceId))
        {
            throw new HubException("Invalid device identity");
        }

        var connectionId = Context.ConnectionId;
        _logger.LogInformation("Device {DeviceId} registering with connection {ConnectionId}", deviceId, connectionId);

        await _connectionManager.DeviceConnectedAsync(deviceId, connectionId);

        // Mark device as connected
        var command = new UpdateDeviceConnectionStateCommand(
            Guid.Empty,
            deviceId,
            IsConnected: true
        );

        await _mediator.Send(command);
    }

    /// <summary>
    /// Receives a heartbeat from a device.
    /// Updates the device's last seen timestamp.
    /// </summary>
    public async Task SendHeartbeat()
    {
        if (!Guid.TryParse(Context.UserIdentifier, out var deviceId))
        {
            return;
        }

        await _connectionManager.UpdateLastSeenAsync(deviceId);

        // Update connection state to ensure it's marked as online
        var command = new UpdateDeviceConnectionStateCommand(
            Guid.Empty,
            deviceId,
            IsConnected: true
        );

        await _mediator.Send(command);
    }

    /// <summary>
    /// Called when a device connection is established.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Device connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a device disconnects from the hub.
    /// Immediately marks the device as offline.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation("Device dIsConnected: {ConnectionId}", connectionId);

        var deviceId = await _connectionManager.DeviceDisconnectedAsync(connectionId);
        
        if (deviceId.HasValue)
        {
            // Mark device as disconnected
            var command = new UpdateDeviceConnectionStateCommand(
                Guid.Empty,
                deviceId.Value,
                IsConnected: false
            );

            await _mediator.Send(command);
        }

        await base.OnDisconnectedAsync(exception);
    }
}
