using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using BEAM.App.Hubs;
using BEAM.App.Models;

namespace BEAM.App.Services;

/// <summary>
/// Manages device SignalR connections and tracks device connection state.
/// </summary>
public interface IDeviceConnectionManager
{
    /// <summary>
    /// Registers a device connection.
    /// </summary>
    Task DeviceConnectedAsync(Guid deviceId, string connectionId);

    /// <summary>
    /// Removes a device connection by connection ID.
    /// Returns the device ID that was disconnected, if found.
    /// </summary>
    Task<Guid?> DeviceDisconnectedAsync(string connectionId);

    /// <summary>
    /// Updates the last seen timestamp for a device.
    /// </summary>
    Task UpdateLastSeenAsync(Guid deviceId);

    /// <summary>
    /// Checks if a device is currently connected.
    /// </summary>
    bool IsDeviceConnected(Guid deviceId);

    /// <summary>
    /// Sends a configuration update to a specific device.
    /// </summary>
    Task SendConfigurationAsync(Guid deviceId, DeviceConfiguration config);
}

/// <summary>
/// Implementation of device connection management.
/// </summary>
public class DeviceConnectionManager : IDeviceConnectionManager
{
    private readonly ConcurrentDictionary<Guid, string> _deviceConnections = new();
    private readonly ConcurrentDictionary<string, Guid> _connectionDevices = new();
    private readonly ConcurrentDictionary<Guid, DateTimeOffset> _lastSeen = new();
    private readonly IHubContext<DeviceHub, IDeviceClient> _hubContext;
    private readonly ILogger<DeviceConnectionManager> _logger;

    public DeviceConnectionManager(
        IHubContext<DeviceHub, IDeviceClient> hubContext,
        ILogger<DeviceConnectionManager> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public Task DeviceConnectedAsync(Guid deviceId, string connectionId)
    {
        _deviceConnections[deviceId] = connectionId;
        _connectionDevices[connectionId] = deviceId;
        _lastSeen[deviceId] = DateTimeOffset.UtcNow;

        _logger.LogInformation("Device {DeviceId} connected with connection {ConnectionId}", deviceId, connectionId);
        return Task.CompletedTask;
    }

    public Task<Guid?> DeviceDisconnectedAsync(string connectionId)
    {
        if (_connectionDevices.TryRemove(connectionId, out var deviceId))
        {
            _deviceConnections.TryRemove(deviceId, out _);
            _logger.LogInformation("Device {DeviceId} disconnected (connection {ConnectionId})", deviceId, connectionId);
            return Task.FromResult<Guid?>(deviceId);
        }

        return Task.FromResult<Guid?>(null);
    }

    public Task UpdateLastSeenAsync(Guid deviceId)
    {
        _lastSeen[deviceId] = DateTimeOffset.UtcNow;
        return Task.CompletedTask;
    }

    public bool IsDeviceConnected(Guid deviceId)
    {
        return _deviceConnections.ContainsKey(deviceId);
    }

    public async Task SendConfigurationAsync(Guid deviceId, DeviceConfiguration config)
    {
        if (_deviceConnections.TryGetValue(deviceId, out var connectionId))
        {
            try
            {
                await _hubContext.Clients.Client(connectionId).ReceiveConfigurationUpdate(config);
                _logger.LogInformation("Sent configuration update to device {DeviceId}", deviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send configuration to device {DeviceId}", deviceId);
            }
        }
        else
        {
            _logger.LogWarning("Cannot send configuration to device {DeviceId} - not connected", deviceId);
        }
    }
}
