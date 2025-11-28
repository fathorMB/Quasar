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
    Guid DeviceId,
    string Token);

public sealed record LoginRequest(string Username, string Password);
public sealed record LoginResponse(string AccessToken);

/// <summary>
/// HTTP client for registering devices with BEAM.App.
/// </summary>
public sealed class DeviceRegistrationClient : IDisposable
{
    private readonly HttpClient _httpClient;

    public DeviceRegistrationClient()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            AllowAutoRedirect = false
        };
        _httpClient = new HttpClient(handler);
    }

    /// <summary>
    /// Logs in as an admin to get an access token.
    /// </summary>
    public async Task<string?> LoginAsync(string serverUrl, string username, string password)
    {
        try
        {
            var endpoint = new Uri(new Uri(serverUrl), "/auth/login");
            var response = await _httpClient.PostAsJsonAsync(endpoint, new LoginRequest(username, password));
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var preview = errorContent.Length > 500 ? errorContent.Substring(0, 500) + "..." : errorContent;
                throw new Exception($"Login failed: {response.StatusCode}. Content: {preview}");
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return result?.AccessToken;
        }
        catch (Exception ex)
        {
            // Re-throw to let the UI log it
            throw new Exception($"Login error: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Registers a device with the specified BEAM.App server.
    /// </summary>
    public async Task<RegistrationResponse?> RegisterDeviceAsync(string serverUrl, string adminToken, DeviceInfo deviceInfo)
    {
        try
        {
            var endpoint = new Uri(new Uri(serverUrl), "/api/devices/register");
            
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            request.Content = JsonContent.Create(deviceInfo);

            var response = await _httpClient.SendAsync(request);
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
