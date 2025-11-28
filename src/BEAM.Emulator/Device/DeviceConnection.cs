using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.SignalR.Client;
using System.Windows.Forms;

namespace BEAM.Emulator.Device;

public class DeviceConfiguration
{
    public string DeviceName { get; set; }
    public int? HeartbeatIntervalSeconds { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class DeviceConnection : IAsyncDisposable
{
    private HubConnection _connection;
    private System.Threading.Timer _heartbeatTimer;
    private Guid _deviceId;
    private int _heartbeatInterval = 60;
    private bool _isConnected = false;

    public event Action<DeviceConfiguration> ConfigurationReceived;
    public event Action<bool> ConnectionStateChanged;
    public event Action<string> LogMessage;

    public bool IsConnected => _isConnected;

    public async Task ConnectAsync(string baseUrl, Guid deviceId)
    {
        _deviceId = deviceId;
        var hubUrl = $"{baseUrl.TrimEnd('/')}/hubs/devices";

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<DeviceConfiguration>("ReceiveConfigurationUpdate", OnConfigurationReceived);
        
        _connection.Reconnecting += error =>
        {
            _isConnected = false;
            ConnectionStateChanged?.Invoke(false);
            LogMessage?.Invoke($"Reconnecting... {error?.Message}");
            return Task.CompletedTask;
        };

        _connection.Reconnected += connectionId =>
        {
            _isConnected = true;
            ConnectionStateChanged?.Invoke(true);
            LogMessage?.Invoke($"Reconnected. Connection ID: {connectionId}");
            // Re-register device after reconnection
            return _connection.InvokeAsync("RegisterDevice", _deviceId);
        };

        _connection.Closed += error =>
        {
            _isConnected = false;
            ConnectionStateChanged?.Invoke(false);
            LogMessage?.Invoke($"Connection closed. {error?.Message}");
            return Task.CompletedTask;
        };

        try
        {
            await _connection.StartAsync();
            _isConnected = true;
            ConnectionStateChanged?.Invoke(true);
            LogMessage?.Invoke("Connected to Device Hub.");

            await _connection.InvokeAsync("RegisterDevice", _deviceId);
            LogMessage?.Invoke($"Registered device {_deviceId}");

            // Start heartbeat
            StartHeartbeat();
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"Failed to connect: {ex.Message}");
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        StopHeartbeat();
        if (_connection != null)
        {
            await _connection.StopAsync();
            _isConnected = false;
            ConnectionStateChanged?.Invoke(false);
            LogMessage?.Invoke("Disconnected.");
        }
    }

    public void UpdateHeartbeatInterval(int seconds)
    {
        if (seconds < 5) seconds = 5;
        _heartbeatInterval = seconds;
        RestartHeartbeat();
    }

    private void StartHeartbeat()
    {
        _heartbeatTimer?.Dispose();
        _heartbeatTimer = new System.Threading.Timer(async _ => 
        {
            if (_isConnected)
            {
                try
                {
                    await _connection.InvokeAsync("SendHeartbeat", _deviceId);
                    LogMessage?.Invoke("Heartbeat sent.");
                }
                catch (Exception ex)
                {
                    LogMessage?.Invoke($"Failed to send heartbeat: {ex.Message}");
                }
            }
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(_heartbeatInterval));
    }

    private void StopHeartbeat()
    {
        _heartbeatTimer?.Change(Timeout.Infinite, 0);
        _heartbeatTimer?.Dispose();
        _heartbeatTimer = null;
    }

    private void RestartHeartbeat()
    {
        if (_isConnected && _heartbeatTimer != null)
        {
            StartHeartbeat();
            LogMessage?.Invoke($"Heartbeat interval updated to {_heartbeatInterval}s");
        }
    }

    private void OnConfigurationReceived(DeviceConfiguration config)
    {
        LogMessage?.Invoke("Configuration update received.");
        
        if (config.HeartbeatIntervalSeconds.HasValue && config.HeartbeatIntervalSeconds.Value > 0)
        {
            UpdateHeartbeatInterval(config.HeartbeatIntervalSeconds.Value);
        }

        ConfigurationReceived?.Invoke(config);
    }

    public async ValueTask DisposeAsync()
    {
        StopHeartbeat();
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
    }
}
