using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Routing;

namespace Quasar.Discovery;

/// <summary>
/// Background service that listens for UDP discovery requests and responds with server information.
/// </summary>
public sealed class DiscoveryService : BackgroundService
{
    private readonly ILogger<DiscoveryService> _logger;
    private readonly DiscoveryOptions _options;
    private readonly IServer? _server;
    private readonly EndpointDataSource? _endpointDataSource;
    private UdpClient? _udpClient;

    public DiscoveryService(
        ILogger<DiscoveryService> logger,
        IOptions<DiscoveryOptions> options,
        IServer? server = null,
        EndpointDataSource? endpointDataSource = null)
    {
        _logger = logger;
        _options = options.Value;
        _server = server;
        _endpointDataSource = endpointDataSource;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Discovery service is disabled");
            return;
        }

        try
        {
            _udpClient = new UdpClient(_options.Port);
            _logger.LogInformation("Discovery service listening on UDP port {Port}", _options.Port);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = await _udpClient.ReceiveAsync(stoppingToken);
                    _ = Task.Run(() => HandleRequest(result.Buffer, result.RemoteEndPoint), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error receiving UDP packet");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start discovery service on port {Port}", _options.Port);
        }
    }

    private async Task HandleRequest(byte[] data, IPEndPoint remoteEndPoint)
    {
        try
        {
            var request = Encoding.UTF8.GetString(data);
            _logger.LogDebug("Received discovery request from {RemoteEndPoint}: {Request}", remoteEndPoint, request);

            // Parse and validate request
            using var requestDoc = JsonDocument.Parse(request);
            var type = requestDoc.RootElement.GetProperty("type").GetString();

            if (type != "discover")
            {
                _logger.LogWarning("Invalid discovery request type: {Type}", type);
                return;
            }

            // Optional delay to prevent network flooding
            if (_options.ResponseDelayMs > 0)
            {
                await Task.Delay(_options.ResponseDelayMs);
            }

            // Build response
            var response = BuildResponse();
            var responseJson = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            // Send response
            var responseBytes = Encoding.UTF8.GetBytes(responseJson);
            await _udpClient!.SendAsync(responseBytes, responseBytes.Length, remoteEndPoint);

            _logger.LogInformation("Sent discovery response to {RemoteEndPoint}", remoteEndPoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling discovery request from {RemoteEndPoint}", remoteEndPoint);
        }
    }

    private object BuildResponse()
    {
        // Get server addresses from features
        var addresses = _server?.Features.Get<IServerAddressesFeature>();
        var serverAddresses = addresses?.Addresses.ToList() ?? new List<string>();

        // Extract host and port from server addresses
        var httpEndpoint = serverAddresses.FirstOrDefault() ?? "http://localhost:5000";
        var uri = new Uri(httpEndpoint);

        var response = new
        {
            Type = "response",
            ServiceName = _options.ServiceName,
            Version = "1.0",
            Hostname = uri.Host,
            Port = uri.Port,
            Endpoints = new
            {
                Http = httpEndpoint,
                Ws = httpEndpoint.Replace("http://", "ws://").Replace("https://", "wss://")
            },
            Metadata = _options.Metadata,
            RegisteredEndpoints = _options.IncludeEndpoints ? GetRegisteredEndpoints() : null
        };

        return response;
    }

    private List<object>? GetRegisteredEndpoints()
    {
        try
        {
            if (_endpointDataSource == null)
            {
                return null;
            }

            var endpoints = _endpointDataSource.Endpoints
                .OfType<RouteEndpoint>()
                .Select(e => new
                {
                    Path = e.RoutePattern.RawText,
                    Methods = e.Metadata.GetMetadata<Microsoft.AspNetCore.Routing.HttpMethodMetadata>()?.HttpMethods.ToList(),
                    DisplayName = e.DisplayName
                })
                .Where(e => e.Path != null)
                .ToList<object>();

            return endpoints.Count > 0 ? endpoints : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve registered endpoints");
            return null;
        }
    }

    public override void Dispose()
    {
        _udpClient?.Close();
        _udpClient?.Dispose();
        base.Dispose();
    }
}
