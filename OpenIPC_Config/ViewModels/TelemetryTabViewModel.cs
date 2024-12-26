using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData.Binding;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using OpenIPC_Config.Events;
using OpenIPC_Config.Models;
using OpenIPC_Config.Services;
using Prism.Events;
using Serilog;

namespace OpenIPC_Config.ViewModels;

public partial class TelemetryTabViewModel : ViewModelBase
{
    #region Observable Properties
    [ObservableProperty] private bool _canConnect;
    [ObservableProperty] private bool _isOnboardRecOff;
    [ObservableProperty] private bool _isOnboardRecOn;
    [ObservableProperty] private string _selectedAggregate;
    [ObservableProperty] private string _selectedBaudRate;
    [ObservableProperty] private string _selectedMcsIndex;
    [ObservableProperty] private string _selectedRcChannel;
    [ObservableProperty] private string _selectedRouter;
    [ObservableProperty] private string _selectedSerialPort;
    [ObservableProperty] private string _telemetryContent;
    #endregion

    public bool IsMobile => App.OSType == "Mobile";
    public bool IsEnabledForView => CanConnect && !IsMobile;

    
    #region Collections
    public ObservableCollection<string> SerialPorts { get; private set; }
    public ObservableCollection<string> BaudRates { get; private set; }
    public ObservableCollection<string> McsIndex { get; private set; }
    public ObservableCollection<string> Aggregate { get; private set; }
    public ObservableCollection<string> RC_Channel { get; private set; }
    public ObservableCollection<string> Router { get; private set; }
    #endregion

    #region Commands
    public ICommand EnableUART0Command { get; private set; }
    public ICommand DisableUART0Command { get; private set; }
    public ICommand AddMavlinkCommand { get; private set; }
    public ICommand UploadMSPOSDCommand { get; private set; }
    public ICommand UploadINavCommand { get; private set; }
    public ICommand MSPOSDExtraCommand { get; private set; }
    public ICommand OnBoardRecCommand { get; private set; }
    public ICommand SaveAndRestartTelemetryCommand { get; private set; }
    #endregion
    
    private readonly IMessageBoxService _messageBoxService;

    #region Constructor
    public TelemetryTabViewModel(ILogger logger,
        ISshClientService sshClientService,
        IEventSubscriptionService eventSubscriptionService,
        IMessageBoxService messageBoxService)
        : base(logger, sshClientService, eventSubscriptionService)
    {
        _messageBoxService = messageBoxService;
        
        InitializeCollections();
        InitializeCommands();
        SubscribeToEvents();
    }
    #endregion

    #region Initialization
    private void InitializeCollections()
    {
        SerialPorts = new ObservableCollection<string> { "/dev/ttyS0", "/dev/ttyS1", "/dev/ttyS2" };
        BaudRates = new ObservableCollection<string> { "4800", "9600", "19200", "38400", "57600", "115200" };
        McsIndex = new ObservableCollection<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        Aggregate = new ObservableCollection<string> { "0", "1", "2", "4", "6", "8", "10", "12", "14", "15" };
        RC_Channel = new ObservableCollection<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8" };
        Router = new ObservableCollection<string> { "0", "1", "2" };
    }

    private void InitializeCommands()
    {
        EnableUART0Command = new RelayCommand(EnableUART0);
        DisableUART0Command = new RelayCommand(DisableUART0);
        OnBoardRecCommand = new RelayCommand(OnBoardRec);
        AddMavlinkCommand = new RelayCommand(AddMavlink);
        UploadMSPOSDCommand = new RelayCommand(UploadMSPOSD);
        UploadINavCommand = new RelayCommand(UploadINav);
        MSPOSDExtraCommand = new RelayCommand(AddMSPOSDExtra);
        SaveAndRestartTelemetryCommand = new RelayCommand(SaveAndRestartTelemetry);
    }

    private void SubscribeToEvents()
    {
        EventSubscriptionService.Subscribe<TelemetryContentUpdatedEvent, TelemetryContentUpdatedMessage>(OnTelemetryContentUpdated);
        EventSubscriptionService.Subscribe<AppMessageEvent, AppMessage>(OnAppMessage);
    }
    #endregion

    #region Event Handlers
    private void OnAppMessage(AppMessage appMessage)
    {
        CanConnect = appMessage.CanConnect;
    }

    public virtual void HandleTelemetryContentUpdated(TelemetryContentUpdatedMessage message)
    {
        TelemetryContent = message.Content;
        ParseTelemetryContent();
    }
    
    private async void OnTelemetryContentUpdated(TelemetryContentUpdatedMessage message)
    {
        HandleTelemetryContentUpdated(message);
    }
    #endregion

    #region Commands
    private async void EnableUART0()
    {
        UpdateUIMessage("Enabling UART0...");
        await SshClientService.ExecuteCommandAsync(DeviceConfig.Instance, DeviceCommands.UART0OnCommand);
    }

    private async void DisableUART0()
    {
        UpdateUIMessage("Disabling UART0...");
        await SshClientService.ExecuteCommandAsync(DeviceConfig.Instance, DeviceCommands.UART0OffCommand);
    }

    private async void OnBoardRec()
    {
        UpdateUIMessage("Enabling Onboard Recording...");
        var command = IsOnboardRecOn ? "yaml-cli .records.enabled true" : "yaml-cli .records.enabled false";
        await SshClientService.ExecuteCommandAsync(DeviceConfig.Instance, command);
    }

    private async void AddMavlink()
    {
        UpdateUIMessage("Adding MAVLink...");
        await SshClientService.ExecuteCommandAsync(DeviceConfig.Instance, TelemetryCommands.Extra);
        await SshClientService.ExecuteCommandAsync(DeviceConfig.Instance, DeviceCommands.RebootCommand);
    }

    private async void UploadMSPOSD()
    {
        Log.Debug("UploadMSPOSDCommand executed");

        var msposdFile = "msposd_star6e";

        // Get all files in the binaries folder
        var binariesFolderPath = OpenIPC.GetBinariesPath();

        var files = Directory.GetFiles(binariesFolderPath).Where(f => f.Contains(msposdFile));

        if (files == null || !files.Any())
        {
            _messageBoxService.ShowMessageBox("File not found!", "File " + msposdFile + " not found!"); 
            // var box = MessageBoxManager
            //     .GetMessageBoxStandard("File not found!", "File " + msposdFile + " not found!");
            // await box.ShowAsync();
            return;
        }

        // killall -q msposd
        await SshClientService.ExecuteCommandAsync(DeviceConfig.Instance, "killall -q msposd");
        // upload msposd
        await SshClientService.UploadBinaryAsync(DeviceConfig.Instance, Models.OpenIPC.RemoteBinariesFolder, "msposd_star6e");
        await SshClientService.ExecuteCommandAsync(DeviceConfig.Instance, "mv /usr/bin/msposd_star6e /usr/bin/msposd");
        // chmod +x /usr/bin/msposd
        await SshClientService.ExecuteCommandAsync(DeviceConfig.Instance, "chmod +x /usr/bin/msposd");

        // upload betaflight fonts
        await SshClientService.ExecuteCommandAsync(DeviceConfig.Instance, $"mkdir {Models.OpenIPC.RemoteFontsFolder}");
        await SshClientService.UploadBinaryAsync(DeviceConfig.Instance, Models.OpenIPC.RemoteFontsFolder,
            Models.OpenIPC.FileType.BetaFlightFonts, "font.png");
        await SshClientService.UploadBinaryAsync(DeviceConfig.Instance, Models.OpenIPC.RemoteFontsFolder,
            Models.OpenIPC.FileType.BetaFlightFonts, "font_hd.png");

        // upload vtxmenu.ini /etc
        await SshClientService.UploadBinaryAsync(DeviceConfig.Instance, Models.OpenIPC.RemoteEtcFolder, "vtxmenu.ini");


        // ensure file is unix formatted
        await SshClientService.ExecuteCommandAsync(DeviceConfig.Instance, "dos2unix /etc/vtxmenu.ini");

        // reboot
        await SshClientService.ExecuteCommandAsync(DeviceConfig.Instance, DeviceCommands.RebootCommand);

        //Thread.Sleep(3000);

        _messageBoxService.ShowMessageBox("Done!", "MSPOSD setup, wait for device to restart!");

        // var MsgBox = MessageBoxManager
        //     .GetMessageBoxStandard("Done!", "MSPOSD setup, wait for device to restart!", ButtonEnum.Ok);
        // await MsgBox.ShowAsync();
    }

    private async void UploadINav()
    {
        Log.Debug("UploadINavCommand executed");
        // upload betaflight fonts
        await SshClientService.ExecuteCommandAsync(DeviceConfig.Instance, $"mkdir {Models.OpenIPC.RemoteFontsFolder}");
        
        await SshClientService.UploadBinaryAsync(DeviceConfig.Instance, Models.OpenIPC.RemoteFontsFolder,
            Models.OpenIPC.FileType.iNavFonts, "font.png");
        await SshClientService.UploadBinaryAsync(DeviceConfig.Instance, Models.OpenIPC.RemoteFontsFolder,
            Models.OpenIPC.FileType.iNavFonts, "font_hd.png");
    }

    private async void AddMSPOSDExtra()
    {
        // if "%1" == "mspextra" (
        // 	plink -ssh root@%2 -pw %3 sed -i 's/echo \"Starting wifibroadcast service...\"/echo \"\&L70 \&F35 CPU:\&C \&B Temp:\&T\" ">"\/tmp\/MSPOSD.msg "\&"/' /etc/init.d/S98datalink
        // 	plink -ssh root@%2 -pw %3 reboot	
        // )
        // 
        Log.Debug("MSPOSDExtra executed"); 
        await SshClientService.ExecuteCommandAsync(DeviceConfig.Instance, DeviceCommands.MSPOSDExtraCommand);
        await SshClientService.ExecuteCommandAsync(DeviceConfig.Instance, DeviceCommands.DataLinkRestart);
        
        //TODO: do we need to restart the camera?
        //await SshClientService.ExecuteCommandAsync(DeviceConfig.Instance, DeviceCommands.RebootCommand);
        
        _messageBoxService.ShowMessageBox("Done!", "Please wait for datalink to restart!"); 
        // var MsgBox = MessageBoxManager
        //     .GetMessageBoxStandard("Done!", "Please wait fir datalink to restart!", ButtonEnum.Ok);
        // await MsgBox.ShowAsync();
    }

    private async void SaveAndRestartTelemetry()
    {
        Log.Debug("Saving and restarting telemetry...");
        TelemetryContent = UpdateTelemetryContent(SelectedSerialPort, SelectedBaudRate, SelectedRouter, SelectedMcsIndex, SelectedAggregate, SelectedRcChannel);
        await SshClientService.UploadFileStringAsync(DeviceConfig.Instance, Models.OpenIPC.TelemetryConfFileLoc, TelemetryContent);
        await SshClientService.ExecuteCommandAsync(DeviceConfig.Instance, DeviceCommands.TelemetryRestartCommand);
    }
    #endregion

    #region Helper Methods
    private async Task UploadFonts(Models.OpenIPC.FileType fileType)
    {
        await SshClientService.UploadBinaryAsync(DeviceConfig.Instance, Models.OpenIPC.RemoteFontsFolder, fileType, "font.png");
        await SshClientService.UploadBinaryAsync(DeviceConfig.Instance, Models.OpenIPC.RemoteFontsFolder, fileType, "font_hd.png");
    }

    private void ParseTelemetryContent()
    {
        Log.Debug("Parsing TelemetryContent.");

        if (string.IsNullOrEmpty(TelemetryContent)) return;

        // Logic to parse wfbConfContent, e.g., split by lines or delimiters
        var lines = TelemetryContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            // Example: Parse key-value pairs
            var parts = line.Split('=');
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();

                switch (key)
                {
                    case Telemetry.Serial:
                        if (SerialPorts?.Contains(value) ?? false)
                        {
                            SelectedSerialPort = value;
                        }
                        else
                        {
                            SerialPorts.Add(value);
                            SelectedSerialPort = value;
                        }

                        break;
                    case Telemetry.Baud:
                        if (BaudRates?.Contains(value) ?? false)
                        {
                            SelectedBaudRate = value;
                        }
                        else
                        {
                            BaudRates.Add(value);
                            SelectedBaudRate = value;
                        }

                        break;
                    case Telemetry.Router:
                        if (Router?.Contains(value) ?? false)
                        {
                            SelectedRouter = value;
                        }
                        else
                        {
                            Router.Add(value);
                            SelectedRouter = value;
                        }

                        break;
                    case Telemetry.McsIndex:
                        if (McsIndex?.Contains(value) ?? false)
                        {
                            SelectedMcsIndex = value;
                        }
                        else
                        {
                            McsIndex.Add(value);
                            SelectedMcsIndex = value;
                        }

                        break;
                    case Telemetry.Aggregate:
                        if (Aggregate?.Contains(value) ?? false)
                        {
                            SelectedAggregate = value;
                        }
                        else
                        {
                            Aggregate.Add(value);
                            SelectedAggregate = value;
                        }

                        break;
                    case Telemetry.RcChannel:
                        if (RC_Channel?.Contains(value) ?? false)
                        {
                            SelectedRcChannel = value;
                        }
                        else
                        {
                            RC_Channel.Add(value);
                            SelectedRcChannel = value;
                        }

                        break;
                    default:
                        // Handle other key-value pairs
                        Log.Debug($"Telemetry - Unknown key: {key}, value: {value}");
                        break;
                }


                // Handle parsed data, e.g., store in a dictionary or bind to properties
                Log.Debug($"Telemetry - Key: {key}, Value: {value}");
            }
        }    
    }

    private string UpdateTelemetryContent(string serial, string baudRate, string router, string mcsIndex, string aggregate, string rcChannel)
    {
        // Logic to update WfbConfContent with the new values
        var lines = TelemetryContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var regex = new Regex(
            @"(serial|baud|router|mcs_index|aggregate|channels)=.*");
        var updatedContent = regex.Replace(TelemetryContent, match =>
        {
            switch (match.Groups[1].Value)
            {
                case Telemetry.Serial:
                    return $"serial={serial}";
                case Telemetry.Baud:
                    return $"baud={baudRate}";
                case Telemetry.Router:
                    return $"router={router}";
                case Telemetry.McsIndex:
                    return $"mcs_index={mcsIndex}";
                case Telemetry.Aggregate:
                    return $"aggregate={aggregate}";
                case Telemetry.RcChannel:
                    return $"channels={rcChannel}";

                default:
                    return match.Value;
            }
        });
        return updatedContent;
    }
    #endregion
}