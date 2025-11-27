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
