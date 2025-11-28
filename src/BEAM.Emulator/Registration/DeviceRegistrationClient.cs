using System.Net.Http.Json;
using System.Text.Json;

namespace BEAM.Emulator.Registration;

/// <summary>
/// Information about a device to register.
/// </summary>
public sealed record DeviceInfo(
    Guid? DeviceId,
    string DeviceName,
    string DeviceType,
    string MacAddress);

/// <summary>
/// Response from device registration API.
/// </summary>
public sealed record RegistrationResponse(
    Guid DeviceId);

/// <summary>
/// HTTP client for registering devices with BEAM.App.
/// </summary>
public sealed class DeviceRegistrationClient : IDisposable
{
    private readonly HttpClient _httpClient;

    public DeviceRegistrationClient()
    {
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Registers a device with the specified BEAM.App server.
    /// </summary>
    public async Task<RegistrationResponse?> RegisterDeviceAsync(string serverUrl, DeviceInfo deviceInfo)
    {
        try
        {
            var endpoint = new Uri(new Uri(serverUrl), "/api/devices/register");
            
            var response = await _httpClient.PostAsJsonAsync(endpoint, deviceInfo);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<RegistrationResponse>();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Registration error: {ex.Message}");
            return null;
        }
    }



    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
