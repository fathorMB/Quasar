using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace BEAM.Emulator.Discovery;

/// <summary>
/// Response from BEAM.App server discovery broadcast.
/// </summary>
public sealed record DiscoveryResponse(
    string Type,
    string ServiceName,
    string Version,
    string Hostname,
    int Port,
    DiscoveryEndpoints Endpoints,
    Dictionary<string, string>? Metadata);

public sealed record DiscoveryEndpoints(
    string Http,
    string Ws);

/// <summary>
/// UDP client for discovering BEAM.App servers on the network.
/// </summary>
public sealed class DiscoveryClient : IDisposable
{
    private const int DiscoveryPort = 6000;
    private UdpClient? _client;

    public async Task<List<DiscoveryResponse>> DiscoverServersAsync(int timeoutMs = 3000)
    {
        var servers = new List<DiscoveryResponse>();

        try
        {
            _client = new UdpClient();
            _client.EnableBroadcast = true;

            // Send discovery request
            var request = new { type = "discover" };
            var requestJson = JsonSerializer.Serialize(request);
            var requestBytes = Encoding.UTF8.GetBytes(requestJson);

            var endpoint = new IPEndPoint(IPAddress.Broadcast, DiscoveryPort);
            await _client.SendAsync(requestBytes, requestBytes.Length, endpoint);

            // Listen for responses with timeout
            var cts = new CancellationTokenSource(timeoutMs);
            
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    var result = await _client.ReceiveAsync(cts.Token);
                    var responseJson = Encoding.UTF8.GetString(result.Buffer);
                    
                    var response = JsonSerializer.Deserialize<DiscoveryResponse>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (response != null && response.Type == "response")
                    {
                        servers.Add(response);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Timeout reached
                    break;
                }
                catch (Exception ex)
                {
                    // Log or ignore individual packet errors
                    Console.WriteLine($"Error receiving discovery response: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Discovery error: {ex.Message}");
        }

        return servers;
    }

    public void Dispose()
    {
        _client?.Close();
        _client?.Dispose();
    }
}
