using BEAM.Emulator.Discovery;
using BEAM.Emulator.Registration;

namespace BEAM.Emulator
{
    public partial class MainForm : Form
    {
        private List<DiscoveryResponse> _discoveredServers = new();
        private DiscoveryClient? _discoveryClient;
        private readonly DeviceRegistrationClient _apiClient = new();
        private Guid _deviceId = Guid.NewGuid();
        private readonly System.Windows.Forms.Timer _heartbeatTimer;
        private bool _heartbeatRunning;
        private string? _currentServerBase;

        public MainForm()
        {
            InitializeComponent();
            _heartbeatTimer = new System.Windows.Forms.Timer
            {
                Interval = 5000
            };
            _heartbeatTimer.Tick += async (_, _) => await SendHeartbeatAsync();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            GenerateDeviceInfo();
            Log("Device emulator initialized");
            Log($"Device ID: {_deviceId}");
            Log($"MAC Address: {txtMacAddress.Text}");
            Log("Click 'Discover Servers' to find BEAM.App instances on the network");
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            GenerateDeviceInfo();
            Log($"Generated new Device ID: {_deviceId}");
            Log($"Generated new MAC Address: {txtMacAddress.Text}");
        }

        private void GenerateDeviceInfo()
        {
            _deviceId = Guid.NewGuid();
            txtDeviceId.Text = _deviceId.ToString();

            var random = new Random();
            var macBytes = new byte[6];
            random.NextBytes(macBytes);
            macBytes[0] = (byte)((macBytes[0] & 0xFE) | 0x02);
            var macAddress = string.Join(":", macBytes.Select(b => b.ToString("X2")));
            txtMacAddress.Text = macAddress;

            txtDeviceName.Text = $"Emulator-{Environment.MachineName}";
            if (cmbDeviceType.SelectedIndex < 0)
            {
                cmbDeviceType.SelectedIndex = 0;
            }
        }

        private async void btnDiscover_Click(object sender, EventArgs e)
        {
            btnDiscover.Enabled = false;
            Log("Broadcasting discovery request...");

            try
            {
                _discoveryClient = new DiscoveryClient();
                _discoveredServers = await _discoveryClient.DiscoverServersAsync(3000);

                lstServers.Items.Clear();

                if (_discoveredServers.Count == 0)
                {
                    Log("No servers found. Make sure BEAM.App is running and discovery is enabled.");
                }
                else
                {
                    foreach (var server in _discoveredServers)
                    {
                        var displayText = $"{server.ServiceName} @ {server.Endpoints.Http}";
                        lstServers.Items.Add(displayText);
                    }

                    Log($"Found {_discoveredServers.Count} server(s)");
                    if (_discoveredServers.Count > 0)
                    {
                        lstServers.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Discovery error: {ex.Message}");
            }
            finally
            {
                _discoveryClient?.Dispose();
                btnDiscover.Enabled = true;
            }
        }

        private async void btnRegister_Click(object sender, EventArgs e)
        {
            if (lstServers.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a server first", "No Server Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDeviceName.Text))
            {
                MessageBox.Show("Please enter a device name", "Device Name Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnRegister.Enabled = false;
            Log("Registering device...");

            try
            {
                var selectedServer = _discoveredServers[lstServers.SelectedIndex];

                var deviceInfo = new DeviceInfo(
                    _deviceId,
                    txtDeviceName.Text,
                    cmbDeviceType.Text,
                    txtMacAddress.Text);

                var result = await _apiClient.RegisterDeviceAsync(selectedServer.Endpoints.Http, deviceInfo);

                if (result != null)
                {
                    _currentServerBase = selectedServer.Endpoints.Http;
                    Log("Device registered successfully!");
                    Log($"  Server: {selectedServer.ServiceName}");
                    Log($"  Device ID: {result.DeviceId}");
                    Log($"  Device Name: {txtDeviceName.Text}");
                    Log($"  Device Type: {cmbDeviceType.Text}");

                    MessageBox.Show(
                        $"Device registered successfully!\nDevice ID: {result.DeviceId}",
                        "Registration Successful",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    Log("Registration failed - check server logs for details");
                    MessageBox.Show("Registration failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Log($"Registration error: {ex.Message}");
                MessageBox.Show($"Registration error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRegister.Enabled = true;
            }
        }

        private async Task SendHeartbeatAsync(bool? isConnectedOverride = null)
        {
            if (!_heartbeatRunning && isConnectedOverride is null)
            {
                return;
            }

            if (lstServers.SelectedIndex < 0 && string.IsNullOrEmpty(_currentServerBase))
            {
                Log("No server selected for heartbeat.");
                return;
            }

            var serverBase = _currentServerBase ?? _discoveredServers[lstServers.SelectedIndex].Endpoints.Http;
            var isConnected = isConnectedOverride ?? true;

            var (ok, err) = await _apiClient.SendHeartbeatAsync(serverBase, _deviceId, isConnected);
            if (ok)
            {
                Log($"Heartbeat sent ({(isConnected ? "online" : "offline")})");
            }
            else
            {
                Log($"Heartbeat failed: {err ?? "unknown error"}");
            }
        }

        private async void btnStartHeartbeat_Click(object sender, EventArgs e)
        {
            if (lstServers.SelectedIndex < 0 && string.IsNullOrEmpty(_currentServerBase))
            {
                MessageBox.Show("Please select a server first", "No Server Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_heartbeatRunning)
            {
                return;
            }

            _heartbeatRunning = true;
            _heartbeatTimer.Start();
            await SendHeartbeatAsync(true);
            Log("Heartbeat started (every 5s)");
        }

        private async void btnStopHeartbeat_Click(object sender, EventArgs e)
        {
            if (!_heartbeatRunning)
            {
                return;
            }

            _heartbeatRunning = false;
            _heartbeatTimer.Stop();
            await SendHeartbeatAsync(false);
            Log("Heartbeat stopped (sent offline)");
        }

        private void Log(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.AppendText($"[{timestamp}] {message}\r\n");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _discoveryClient?.Dispose();
            _apiClient.Dispose();
            _heartbeatTimer.Dispose();
            base.OnFormClosing(e);
        }
    }
}
