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

    /// <summary>
    /// Sends a heartbeat / connection update for a device.
    /// </summary>
    public async Task<(bool ok, string? error)> SendHeartbeatAsync(string serverUrl, Guid deviceId, bool isConnected = true)
    {
        try
        {
            var endpoint = new Uri(new Uri(serverUrl), "/api/devices/heartbeat");
            var payload = new { deviceId, isConnected };
            var response = await _httpClient.PostAsJsonAsync(endpoint, payload);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return (false, $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
            }
            return (true, null);
        }
        catch (Exception ex)
        {
            var msg = $"Heartbeat error: {ex.Message}";
            Console.WriteLine(msg);
            return (false, msg);
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
