using Quasar.Cqrs;
using Quasar.Core;
using BEAM.App.Domain.Devices;

namespace BEAM.App.Services;

/// <summary>
/// Background service that monitors device heartbeats and marks devices offline
/// if they fail to send heartbeats within their expected interval.
/// </summary>
public class DeviceHeartbeatMonitor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeviceHeartbeatMonitor> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(10);

    public DeviceHeartbeatMonitor(
        IServiceProvider serviceProvider,
        ILogger<DeviceHeartbeatMonitor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Device Heartbeat Monitor started");

        // Wait a bit before starting to allow the app to fully initialize
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckDeviceHeartbeatsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking device heartbeats");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Device Heartbeat Monitor stopped");
    }

    private async Task CheckDeviceHeartbeatsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var queryHandler = scope.ServiceProvider.GetRequiredService<IQueryHandler<ListDevicesQuery, PagedResult<DeviceReadModel>>>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Get all devices
        var result = await queryHandler.Handle(new ListDevicesQuery(1, 1000), cancellationToken);

        foreach (var device in result.Items)
        {
            // Only check devices that are currently marked as connected
            if (!device.IsConnected)
                continue;

            // Calculate timeout: heartbeat interval × 2.5 (gives some grace time for network delays)
            var heartbeatInterval = device.HeartbeatIntervalSeconds > 0 
                ? device.HeartbeatIntervalSeconds 
                : 60; // Default to 60 seconds if not set

            var timeout = TimeSpan.FromSeconds(heartbeatInterval * 2.5);
            var lastSeen = device.LastSeenAt ?? device.RegisteredAt;
            var timeSinceLastSeen = DateTimeOffset.UtcNow - lastSeen;

            if (timeSinceLastSeen > timeout)
            {
                _logger.LogWarning(
                    "Device {DeviceId} ({DeviceName}) hasn't sent heartbeat for {Duration:0.0}s (timeout: {Timeout:0.0}s). Marking as offline.",
                    device.Id,
                    device.DeviceName,
                    timeSinceLastSeen.TotalSeconds,
                    timeout.TotalSeconds
                );

                // Mark device as offline
                var command = new UpdateDeviceConnectionStateCommand(
                    Guid.Empty,
                    device.Id,
                    IsConnected: false
                );

                try
                {
                    await mediator.Send(command, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to mark device {DeviceId} as offline", device.Id);
                }
            }
        }
    }
}
