using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Layout;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using OpenIPC_Config.Events;
using OpenIPC_Config.Models;
using OpenIPC_Config.Services;
using Prism.Events;
using Serilog;

namespace OpenIPC_Config.ViewModels;

public class ConnectControlsViewModel : ViewModelBase
{
    private readonly CancellationTokenSource _cancellationTokenSource;

    private readonly DispatcherTimer _dispatcherTimer;

    private readonly IEventAggregator _eventAggregator;
    private readonly SolidColorBrush _offlineColorBrush = new(Colors.Red);
    private readonly SolidColorBrush _onlineColorBrush = new(Colors.Green);
    private readonly Ping _ping = new();


    private bool _canConnect;

    private DeviceConfig _deviceConfig;

    private string _ipAddress;

    private string _password;

    private SolidColorBrush _pingStatusColor = new(Colors.Red);

    private int _port = 22;

    private DeviceType _selectedDeviceType;

    private readonly ISshClientService _sshClientService;

    private readonly TimeSpan _pingInterval = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _pingTimeout = TimeSpan.FromMilliseconds(500);


    public ConnectControlsViewModel()
    {
        _sshClientService = new SshClientService(_eventAggregator);
        _eventAggregator = App.EventAggregator;

        SetDefaults();
        LoadSettings();

        ConnectCommand = new RelayCommand(() => Connect());

        _cancellationTokenSource = new CancellationTokenSource();
        _dispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _dispatcherTimer.Tick += DispatcherTimer_Tick;
        _dispatcherTimer.Start();

        UpdateUIMessage("Ready");
    }

    public ICommand ConnectCommand { get; private set; }

    public Orientation Orientation
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return Orientation.Horizontal;
            return Orientation.Vertical;
        }
    }

    public int Port
    {
        get => _port;
        set
        {
            SetProperty(ref _port, value);
            CheckIfCanConnect();
        }
    }

    public string? IpAddress
    {
        get => _ipAddress;
        set
        {
            SetProperty(ref _ipAddress, value);
            CheckIfCanConnect();
        }
    }

    public string? Password
    {
        get => _password;
        set
        {
            SetProperty(ref _password, value);
            CheckIfCanConnect();
        }
    }


    public bool CanConnect
    {
        get => _canConnect;
        set => SetProperty(ref _canConnect, value);
    }

    public SolidColorBrush PingStatusColor
    {
        get => _pingStatusColor;
        set
        {
            _pingStatusColor = value;
            OnPropertyChanged();
        }
    }

    public DeviceType SelectedDeviceType
    {
        get => _selectedDeviceType;
        set
        {
            // Ignore setting to None if it's due to a binding update
            if (value == DeviceType.None) return;

            if (_selectedDeviceType != value)
            {
                _selectedDeviceType = value;

                // Now only send the message with the selected device type
                SendDeviceTypeMessage(_selectedDeviceType);

                // Trigger any other actions, like OnPropertyChanged if needed
                OnPropertyChanged();
                CheckIfCanConnect();
            }
        }
    }

    private void LoadSettings()
    {
        var settings = SettingsManager.LoadSettings(_eventAggregator);
        _deviceConfig = DeviceConfig.Instance;
        IpAddress = settings.IpAddress;
        Password = settings.Password;
        SelectedDeviceType = settings.DeviceType;
    }

    private async void DispatcherTimer_Tick(object sender, EventArgs e)
    {
        await StartPingingAsync();
    }

    public async Task StartPingingAsync()
    {
        //_cancellationTokenSource = new CancellationTokenSource();

        await Task.Run(async () =>
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    var ipAddress = IPAddress.Parse(IpAddress);
                    var reply = await _ping.SendPingAsync(ipAddress, (int)_pingTimeout.TotalMilliseconds);
                    Log.Debug("Ping!");
                    if (reply.Status == IPStatus.Success)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() => PingStatusColor = _onlineColorBrush);
                        _eventAggregator.GetEvent<AppMessageEvent>().Publish(new AppMessage
                        { Status = "Ping OK", DeviceConfig = _deviceConfig });
                    }
                    else
                    {
                        await Dispatcher.UIThread.InvokeAsync(() => PingStatusColor = _offlineColorBrush);
                        _eventAggregator.GetEvent<AppMessageEvent>().Publish(new AppMessage
                        { Status = "No device", DeviceConfig = _deviceConfig });
                    }
                }
                catch (OperationCanceledException)
                {
                    // Gracefully exit when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    Log.Error($"Error pinging device: {ex.Message}");
                }

                await Task.Delay(_pingInterval, _cancellationTokenSource.Token);
            }
        });
    }

    private void SetDefaults()
    {
        PingStatusColor = _offlineColorBrush;
        IpAddress = "192.168.1.10";
    }

    private void SendDeviceTypeMessage(DeviceType deviceType)
    {
        // Insert logic to send a message based on the selected device type
        // For example, use an event aggregator, messenger, or direct call
        //Log.Debug($"Device type selected: {deviceType}");
        //Console.WriteLine($"Device type selected: {deviceType}");
        _eventAggregator.GetEvent<DeviceTypeChangeEvent>().Publish(deviceType);
    }

    private void UpdateUIMessage(string message)
    {
        _eventAggregator.GetEvent<AppMessageEvent>().Publish(new AppMessage { Message = message });
    }

    private void CheckIfCanConnect()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            CanConnect = !string.IsNullOrWhiteSpace(Password)
                         && !string.IsNullOrWhiteSpace(IpAddress)
                         && !SelectedDeviceType.Equals(DeviceType.None);
        });
    }

    private async void Connect()
    {
        var appMessage = new AppMessage();
        //DeviceConfig deviceConfig = new DeviceConfig();
        _deviceConfig.Username = "root";
        _deviceConfig.IpAddress = IpAddress;
        _deviceConfig.Password = Password;
        _deviceConfig.Port = Port;
        _deviceConfig.DeviceType = SelectedDeviceType;

        _eventAggregator.GetEvent<AppMessageEvent>().Publish(new AppMessage { Message = "Getting hostname" });

        await getHostname(_deviceConfig);
        if (_deviceConfig.Hostname == string.Empty)
        {
            Log.Error("Failed to get hostname, stopping");
            return;
        }

        if ((_deviceConfig.Hostname.Contains("radxa") && _deviceConfig.DeviceType != DeviceType.Radxa) ||
            (_deviceConfig.Hostname.Contains("openipc") && _deviceConfig.DeviceType != DeviceType.Camera))
        {
            _eventAggregator.GetEvent<AppMessageEvent>().Publish(new AppMessage { Message = "Hostname Error!" });
            var msBox = MessageBoxManager.GetMessageBoxStandard("Hostname Error!",
                $"Hostname does not match device type! \nHostname: {_deviceConfig.Hostname} Device Type: {_selectedDeviceType}.\nPlease check device..\nOk to continue anyway\nCancel to quit",
                ButtonEnum.OkCancel);

            var result = await msBox.ShowAsync();
            if (result == ButtonResult.Cancel)
            {
                Log.Debug("Device selection and hostname mismatch, stopping");
                return;
            }
        }

        // Save the config to app settings
        SaveConfig();

        _eventAggregator?.GetEvent<AppMessageEvent>()
            .Publish(new AppMessage { DeviceConfig = _deviceConfig });


        appMessage.DeviceConfig = _deviceConfig;

        if (_deviceConfig != null)
        {
            if (_deviceConfig.DeviceType == DeviceType.Camera)
            {
                _eventAggregator.GetEvent<AppMessageEvent>()
                    .Publish(new AppMessage { Message = "Processing Camera..." });
                processCameraFiles();
                _eventAggregator.GetEvent<AppMessageEvent>()
                    .Publish(new AppMessage { Message = "Processing Camera...done" });
            }
            else if (_deviceConfig.DeviceType == DeviceType.Radxa)
            {
                _eventAggregator.GetEvent<AppMessageEvent>()
                    .Publish(new AppMessage { Message = "Processing Radxa..." });
                processRadxaFiles();
                _eventAggregator.GetEvent<AppMessageEvent>()
                    .Publish(new AppMessage { Message = "Processing Radxa...done" });
            }
        }

        UpdateUIMessage("Connected");
    }

    private async void processRadxaFiles()
    {
        try
        {
            _eventAggregator.GetEvent<AppMessageEvent>()
                .Publish(new AppMessage { Message = "Downloading wifibroadcast.cfg" });

            // get /etc/wifibroadcast.cfg
            var wifibroadcastContent =
                await _sshClientService.DownloadFileAsync(_deviceConfig, Models.OpenIPC.WifiBroadcastFileLoc);

            if (!string.IsNullOrEmpty(wifibroadcastContent))
            {
                var radxaContentUpdatedMessage = new RadxaContentUpdatedMessage();
                radxaContentUpdatedMessage.WifiBroadcastContent = wifibroadcastContent;

                _eventAggregator?.GetEvent<RadxaContentUpdateChangeEvent>()
                    .Publish(new RadxaContentUpdatedMessage
                    {
                        WifiBroadcastContent = wifibroadcastContent
                    });
            }
            else
            {
                await MessageBoxManager.GetMessageBoxStandard("Error", "Failed to download /etc/wifibroadcast.cfg")
                    .ShowAsync();
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            throw;
        }


        try
        {
            _eventAggregator.GetEvent<AppMessageEvent>()
                .Publish(new AppMessage { Message = "Downloading modprod.d/wfb.conf" });
            // get /etc/modprobe.d/wfb.conf
            var wfbModProbeContent =
                await _sshClientService.DownloadFileAsync(_deviceConfig, Models.OpenIPC.WifiBroadcastModProbeFileLoc);

            if (wfbModProbeContent != null)
            {
                var radxaContentUpdatedMessage = new RadxaContentUpdatedMessage();
                radxaContentUpdatedMessage.WfbConfContent = wfbModProbeContent;

                _eventAggregator?.GetEvent<RadxaContentUpdateChangeEvent>()
                    .Publish(new RadxaContentUpdatedMessage
                    {
                        WfbConfContent = wfbModProbeContent
                    });
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            throw;
        }


        try
        {
            _eventAggregator.GetEvent<AppMessageEvent>()
                .Publish(new AppMessage { Message = "Downloading screen-mode" });
            // get /home/radxa/scripts/screen-mode
            var screenModeContent =
                await _sshClientService.DownloadFileAsync(_deviceConfig, Models.OpenIPC.ScreenModeFileLoc);

            if (screenModeContent != null)
            {
                var radxaContentUpdatedMessage = new RadxaContentUpdatedMessage();
                radxaContentUpdatedMessage.ScreenModeContent = screenModeContent;

                _eventAggregator?.GetEvent<RadxaContentUpdateChangeEvent>()
                    .Publish(new RadxaContentUpdatedMessage
                    {
                        ScreenModeContent = screenModeContent
                    });
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            throw;
        }

        try
        {
            _eventAggregator.GetEvent<AppMessageEvent>().Publish(new AppMessage { Message = "Downloading gskey" });

            var gsKeyContent =
                await _sshClientService.DownloadFileAsync(_deviceConfig, Models.OpenIPC.RemoteGsKeyPath);

            if (!string.IsNullOrEmpty(gsKeyContent))
            {
                var droneKey = Utilities.ComputeSha256Hash(gsKeyContent);

                var deviceContentUpdatedMessage = new DeviceContentUpdatedMessage();
                _deviceConfig = DeviceConfig.Instance;
                _deviceConfig.KeyChksum = droneKey;
                deviceContentUpdatedMessage.DeviceConfig = _deviceConfig;

                _eventAggregator?.GetEvent<DeviceContentUpdateEvent>()
                    .Publish(deviceContentUpdatedMessage);

                _eventAggregator.GetEvent<AppMessageEvent>()
                    .Publish(new AppMessage { Message = "Downloading gskey...done" });
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            throw;
        }

        _eventAggregator?.GetEvent<AppMessageEvent>().Publish(new AppMessage
        {
            CanConnect = DeviceConfig.Instance.CanConnect,
            DeviceConfig = _deviceConfig
        });
    }

    private async void processCameraFiles()
    {
        // download file wfb.conf
        var wfbConfContent = await _sshClientService.DownloadFileAsync(_deviceConfig, Models.OpenIPC.WfbConfFileLoc);

        if (wfbConfContent != null)
            _eventAggregator?.GetEvent<WfbConfContentUpdatedEvent>()
                .Publish(new WfbConfContentUpdatedMessage(wfbConfContent));

        try
        {
            var majesticContent =
                await _sshClientService.DownloadFileAsync(_deviceConfig, Models.OpenIPC.MajesticFileLoc);
            // Publish a message to WfbSettingsTabViewModel
            _eventAggregator?.GetEvent<MajesticContentUpdatedEvent>()
                .Publish(new MajesticContentUpdatedMessage(majesticContent));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Error(e.Message);
        }

        try
        {
            var telemetryContent =
                await _sshClientService.DownloadFileAsync(_deviceConfig, Models.OpenIPC.TelemetryConfFileLoc);
            // Publish a message to WfbSettingsTabViewModel
            _eventAggregator?.GetEvent<TelemetryContentUpdatedEvent>()
                .Publish(new TelemetryContentUpdatedMessage(telemetryContent));
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            throw;
        }

        try
        {
            // get /home/radxa/scripts/screen-mode
            var droneKeyContent =
                await _sshClientService.DownloadFileAsync(_deviceConfig, Models.OpenIPC.RemoteDroneKeyPath);

            if (!string.IsNullOrEmpty(droneKeyContent))
            {
                var droneKey = Utilities.ComputeSha256Hash(droneKeyContent);

                var deviceContentUpdatedMessage = new DeviceContentUpdatedMessage();
                _deviceConfig = DeviceConfig.Instance;
                _deviceConfig.KeyChksum = droneKey;
                deviceContentUpdatedMessage.DeviceConfig = _deviceConfig;

                _eventAggregator?.GetEvent<DeviceContentUpdateEvent>()
                    .Publish(deviceContentUpdatedMessage);
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            throw;
        }

        _eventAggregator?.GetEvent<AppMessageEvent>().Publish(new AppMessage
        {
            CanConnect = DeviceConfig.Instance.CanConnect,
            DeviceConfig = _deviceConfig
        });
    }

    /// <summary>
    ///     Retrieves the hostname of the device asynchronously using SSH.
    ///     <para>
    ///         The command execution is cancelled after 10 seconds if no response is received.
    ///         If the command execution times out, a message box is displayed with an error message.
    ///     </para>
    /// </summary>
    /// <param name="deviceConfig">The device configuration to use for the SSH connection.</param>
    private async Task getHostname(DeviceConfig deviceConfig)
    {
        deviceConfig.Hostname = string.Empty;

        var cts = new CancellationTokenSource(10000); // 10 seconds
        var cancellationToken = cts.Token;

        var cmdResult =
            await _sshClientService.ExecuteCommandWithResponse(deviceConfig, DeviceCommands.GetHostname,
                cancellationToken);

        // If the command execution takes longer than 10 seconds, the task will be cancelled
        if (cmdResult == null)
        {
            // Handle the timeout
            // .
            var resp = MessageBoxManager.GetMessageBoxStandard("Timeout Error!",
                "The command took too long to execute. Please check device..");
            await resp.ShowAsync();
            return;
        }

        var hostName = Utilities.RemoveSpecialCharacters(cmdResult.Result);
        deviceConfig.Hostname = hostName;
        //_deviceConfig.Hostname = hostName;
        //Hostname = hostName;

        // Cleanup
        cts.Dispose();
    }

    private void SaveConfig()
    {
        _deviceConfig.DeviceType = SelectedDeviceType;
        _deviceConfig.IpAddress = IpAddress;
        _deviceConfig.Port = Port;
        _deviceConfig.Password = Password;

        // save config to file
        SettingsManager.SaveSettings(_deviceConfig);
    }
}