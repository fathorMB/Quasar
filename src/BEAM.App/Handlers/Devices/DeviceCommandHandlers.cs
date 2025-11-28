using Quasar.Core;
using Quasar.Cqrs;
using Quasar.EventSourcing.Abstractions;
using Microsoft.Extensions.Logging;
using BEAM.App.Domain.Devices;

namespace BEAM.App.Handlers.Devices;

/// <summary>
/// Handles device registration commands.
/// </summary>
public sealed class RegisterDeviceHandler : ICommandHandler<RegisterDeviceCommand, Result<Guid>>
{
    private readonly IEventSourcedRepository<DeviceAggregate> _repository;
    private readonly ILogger<RegisterDeviceHandler> _logger;

    public RegisterDeviceHandler(
        IEventSourcedRepository<DeviceAggregate> repository,
        ILogger<RegisterDeviceHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(RegisterDeviceCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            // Load the shared registry aggregate
            var aggregate = await _repository.GetAsync(DeviceConstants.DeviceStreamId, cancellationToken);

            // Register the new device (aggregate handles idempotency)
            aggregate.Register(command.DeviceId, command.DeviceName, command.DeviceType, command.MacAddress);

            // Persist to shared stream
            await _repository.SaveAsync(aggregate, cancellationToken);

            _logger.LogInformation("Device registered: {DeviceId} ({DeviceName}, {DeviceType})", 
                command.DeviceId, command.DeviceName, command.DeviceType);

            return Result<Guid>.Success(command.DeviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering device {DeviceId}", command.DeviceId);
            return Result<Guid>.Failure(new Error("device.registration_failed", "Failed to register device"));
        }
    }
}

/// <summary>
/// Handles activate device commands.
/// </summary>
public sealed class ActivateDeviceHandler : ICommandHandler<ActivateDeviceCommand, Result>
{
    private readonly IEventSourcedRepository<DeviceAggregate> _repository;
    private readonly ILogger<ActivateDeviceHandler> _logger;

    public ActivateDeviceHandler(
        IEventSourcedRepository<DeviceAggregate> repository,
        ILogger<ActivateDeviceHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result> Handle(ActivateDeviceCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            // Load shared registry
            var aggregate = await _repository.GetAsync(DeviceConstants.DeviceStreamId, cancellationToken);
            
            // We don't check for empty aggregate here because if it's empty, no devices are registered anyway,
            // so Activate will throw/fail as expected.
            
            aggregate.Activate(command.DeviceId);
            
            await _repository.SaveAsync(aggregate, cancellationToken);

            _logger.LogInformation("Device activated: {DeviceId}", command.DeviceId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating device {DeviceId}", command.DeviceId);
            return Result.Failure(new Error("device.activation_failed", "Failed to activate device"));
        }
    }
}

/// <summary>
/// Handles device heartbeat / connection state updates.
/// </summary>
public sealed class UpdateDeviceConnectionStateHandler : ICommandHandler<UpdateDeviceConnectionStateCommand, Result>
{
    private readonly IEventSourcedRepository<DeviceAggregate> _repository;
    private readonly ILogger<UpdateDeviceConnectionStateHandler> _logger;

    public UpdateDeviceConnectionStateHandler(
        IEventSourcedRepository<DeviceAggregate> repository,
        ILogger<UpdateDeviceConnectionStateHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateDeviceConnectionStateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var aggregate = await _repository.GetAsync(DeviceConstants.DeviceStreamId, cancellationToken);
            aggregate.UpdateConnectionState(command.DeviceId, command.IsConnected);
            await _repository.SaveAsync(aggregate, cancellationToken);

            _logger.LogInformation("Device heartbeat: {DeviceId} => {State}", command.DeviceId, command.IsConnected ? "connected" : "disconnected");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating device connection state {DeviceId}", command.DeviceId);
            return Result.Failure(new Error("device.connection_state_failed", "Failed to update device connection state"));
        }
    }
}

/// <summary>
/// Handles device name update commands.
/// </summary>
public sealed class UpdateDeviceNameHandler : ICommandHandler<UpdateDeviceNameCommand, Result>
{
    private readonly IEventSourcedRepository<DeviceAggregate> _repository;
    private readonly IQueryHandler<ListDevicesQuery, PagedResult<DeviceReadModel>> _listDevicesHandler;
    private readonly ILogger<UpdateDeviceNameHandler> _logger;

    public UpdateDeviceNameHandler(
        IEventSourcedRepository<DeviceAggregate> repository,
        IQueryHandler<ListDevicesQuery, PagedResult<DeviceReadModel>> listDevicesHandler,
        ILogger<UpdateDeviceNameHandler> logger)
    {
        _repository = repository;
        _listDevicesHandler = listDevicesHandler;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateDeviceNameCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check for unique name constraint
            var existingDevices = await _listDevicesHandler.Handle(new ListDevicesQuery(1, 1000), cancellationToken);
            if (existingDevices.Items.Any(d => d.DeviceName.Equals(command.NewName, StringComparison.OrdinalIgnoreCase) && d.Id != command.DeviceId))
            {
                _logger.LogWarning("Device name '{Name}' already exists", command.NewName);
                return Result.Failure(new Error("device.name_already_exists", $"A device with the name '{command.NewName}' already exists"));
            }

            var aggregate = await _repository.GetAsync(DeviceConstants.DeviceStreamId, cancellationToken);
            aggregate.UpdateDeviceName(command.DeviceId, command.NewName);
            await _repository.SaveAsync(aggregate, cancellationToken);

            _logger.LogInformation("Device name updated: {DeviceId} => {NewName}", command.DeviceId, command.NewName);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Device {DeviceId} not found for name update", command.DeviceId);
            return Result.Failure(new Error("device.not_found", "Device not found"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid device name for {DeviceId}", command.DeviceId);
            return Result.Failure(new Error("device.invalid_name", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating device name {DeviceId}", command.DeviceId);
            return Result.Failure(new Error("device.name_update_failed", "Failed to update device name"));
        }
    }
}

/// <summary>
/// Handles device heartbeat interval update commands.
/// </summary>
public sealed class UpdateDeviceHeartbeatIntervalHandler : ICommandHandler<UpdateDeviceHeartbeatIntervalCommand, Result>
{
    private readonly IEventSourcedRepository<DeviceAggregate> _repository;
    private readonly ILogger<UpdateDeviceHeartbeatIntervalHandler> _logger;

    public UpdateDeviceHeartbeatIntervalHandler(
        IEventSourcedRepository<DeviceAggregate> repository,
        ILogger<UpdateDeviceHeartbeatIntervalHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateDeviceHeartbeatIntervalCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var aggregate = await _repository.GetAsync(DeviceConstants.DeviceStreamId, cancellationToken);
            aggregate.UpdateHeartbeatInterval(command.DeviceId, command.IntervalSeconds);
            await _repository.SaveAsync(aggregate, cancellationToken);

            _logger.LogInformation("Device heartbeat interval updated: {DeviceId} => {Interval}s", command.DeviceId, command.IntervalSeconds);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Device {DeviceId} not found for heartbeat interval update", command.DeviceId);
            return Result.Failure(new Error("device.not_found", "Device not found"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid heartbeat interval for {DeviceId}", command.DeviceId);
            return Result.Failure(new Error("device.invalid_interval", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating device heartbeat interval {DeviceId}", command.DeviceId);
            return Result.Failure(new Error("device.heartbeat_interval_update_failed", "Failed to update heartbeat interval"));
        }
    }
}

