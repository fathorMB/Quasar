using Quasar.Core;
using Quasar.Cqrs;
using System.Text.RegularExpressions;
using BEAM.App.Domain.Devices;

namespace BEAM.App.Validators;

/// <summary>
/// Validates device registration commands.
/// </summary>
public sealed class RegisterDeviceValidator : IValidator<RegisterDeviceCommand>
{
    private static readonly Regex MacAddressRegex = new Regex(
        @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$",
        RegexOptions.Compiled);

    public Task<List<Error>> ValidateAsync(RegisterDeviceCommand instance, CancellationToken cancellationToken = default)
    {
        var errors = new List<Error>();

        if (instance.DeviceId == Guid.Empty)
        {
            errors.Add(new Error("validation.device_id", "Device ID cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(instance.DeviceName))
        {
            errors.Add(new Error("validation.device_name", "Device name is required"));
        }
        else if (instance.DeviceName.Length > 100)
        {
            errors.Add(new Error("validation.device_name", "Device name must be 100 characters or less"));
        }

        if (string.IsNullOrWhiteSpace(instance.DeviceType))
        {
            errors.Add(new Error("validation.device_type", "Device type is required"));
        }
        else if (instance.DeviceType.Length > 50)
        {
            errors.Add(new Error("validation.device_type", "Device type must be 50 characters or less"));
        }

        if (string.IsNullOrWhiteSpace(instance.MacAddress))
        {
            errors.Add(new Error("validation.mac_address", "MAC address is required"));
        }
        else if (!MacAddressRegex.IsMatch(instance.MacAddress))
        {
            errors.Add(new Error("validation.mac_address", "MAC address must be in format AA:BB:CC:DD:EE:FF or AA-BB-CC-DD-EE-FF"));
        }

        return Task.FromResult(errors);
    }
}
