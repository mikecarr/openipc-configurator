using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
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
    private readonly ISshClientService _sshClientService;


    [ObservableProperty] private bool _canConnect;
    private readonly IEventAggregator _eventAggregator;
    [ObservableProperty] private bool _isOnboardRecOff;
    [ObservableProperty] private bool _isOnboardRecOn;
    [ObservableProperty] private string _selectedAggregate;
    [ObservableProperty] private string _selectedBaudRate;
    [ObservableProperty] private string _selectedMcsIndex;
    [ObservableProperty] private string _selectedRcChannel;
    [ObservableProperty] private string _selectedRouter;
    [ObservableProperty] private string _selectedSerialPort;
    [ObservableProperty] private string _telemetryContent;


    public TelemetryTabViewModel()
    {
        InitializeCollections();

        _eventAggregator = App.EventAggregator;

        _eventAggregator?.GetEvent<TelemetryContentUpdatedEvent>().Subscribe(OnTelemetryContentUpdated);
        _eventAggregator.GetEvent<AppMessageEvent>().Subscribe(OnAppMessage);


        _sshClientService = new SshClientService(_eventAggregator);

        EnableUART0Command = new RelayCommand(() => EnableUART0());
        DisableUART0Command = new RelayCommand(() => DisableUART0());
        OnBoardRecCommand = new RelayCommand(() => OnBoardRec());
        AddMavlinkCommand = new RelayCommand(() => AddMavlink());
        UploadMSPOSDCommand = new RelayCommand(() => UploadMSPOSD());
        UploadINavCommand = new RelayCommand(() => UploadINav());
        MSPOSDExtraCommand = new RelayCommand(() => AddMSPOSDExtra());
        SaveAndRestartTelemetryCommand = new RelayCommand(() => SaveAndRestartTelemetry());
    }
    //private bool CanConnect { get; set; }

    // ObservableCollections
    public ObservableCollection<string> SerialPorts { get; set; }
    public ObservableCollection<string> BaudRates { get; set; }
    public ObservableCollection<string> McsIndex { get; set; }
    public ObservableCollection<string> Aggregate { get; set; }
    public ObservableCollection<string> RC_Channel { get; set; }
    public ObservableCollection<string> Router { get; set; }

    public ICommand EnableUART0Command { get; private set; }
    public ICommand DisableUART0Command { get; private set; }
    public ICommand AddMavlinkCommand { get; private set; }
    public ICommand UploadMSPOSDCommand { get; private set; }
    public ICommand UploadINavCommand { get; private set; }
    
    public ICommand MSPOSDExtraCommand { get; private set; }
    public ICommand OnBoardRecCommand { get; private set; }

    public ICommand SaveAndRestartTelemetryCommand { get; private set; }

    private void OnAppMessage(AppMessage appMessage)
    {
        if (appMessage.CanConnect) CanConnect = appMessage.CanConnect;
        //Log.Information($"CanConnect {CanConnect.ToString()}");
    }

    private void InitializeCollections()
    {
        SerialPorts = new ObservableCollectionExtended<string> { "/dev/ttyS0", "/dev/ttyS1", "/dev/ttyS2" };
        BaudRates = new ObservableCollectionExtended<string> { "4800", "9600", "19200", "38400", "57600", "115200" };
        McsIndex = new ObservableCollectionExtended<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        Aggregate = new ObservableCollectionExtended<string> { "0", "1", "2", "4", "6", "8", "10", "12", "14", "15" };
        RC_Channel = new ObservableCollectionExtended<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8" };
        Router = new ObservableCollectionExtended<string> { "0", "1", "2" };
    }


    private async void DisableUART0()
    {
        Log.Debug("DisableUART0Command executed");

        _sshClientService.ExecuteCommandAsync(DeviceConfig.Instance,
            DeviceCommands.UART0OffCommand); //await SaveDisableUART0Command();
    }

    private async void EnableUART0()
    {
        Log.Debug("EnableUART0Command executed");
        _sshClientService.ExecuteCommandAsync(DeviceConfig.Instance, DeviceCommands.UART0OnCommand);
    }

    private async void OnBoardRec()
    {
        if (IsOnboardRecOn)
            _sshClientService.ExecuteCommandAsync(DeviceConfig.Instance, "yaml-cli .records.enabled true");
        else if (IsOnboardRecOff)
            _sshClientService.ExecuteCommandAsync(DeviceConfig.Instance, "yaml-cli .records.enabled false");
    }

    private async void AddMavlink()
    {
        Log.Debug("AddMavlinkCommand executed");
        _sshClientService.ExecuteCommandAsync(DeviceConfig.Instance, TelemetryCommands.Extra);
        _sshClientService.ExecuteCommandAsync(DeviceConfig.Instance, DeviceCommands.RebootCommand);
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
            var box = MessageBoxManager
                .GetMessageBoxStandard("File not found!", "File " + msposdFile + " not found!");
            await box.ShowAsync();
            return;
        }

        // killall -q msposd
        await _sshClientService.ExecuteCommandAsync(DeviceConfig.Instance, "killall -q msposd");
        // upload msposd
        await _sshClientService.UploadBinaryAsync(DeviceConfig.Instance, Models.OpenIPC.RemoteBinariesFolder, "msposd_star6e");
        await _sshClientService.ExecuteCommandAsync(DeviceConfig.Instance, "mv /usr/bin/msposd_star6e /usr/bin/msposd");
        // chmod +x /usr/bin/msposd
        await _sshClientService.ExecuteCommandAsync(DeviceConfig.Instance, "chmod +x /usr/bin/msposd");

        // upload betaflight fonts
        await _sshClientService.ExecuteCommandAsync(DeviceConfig.Instance, $"mkdir {Models.OpenIPC.RemoteFontsFolder}");
        await _sshClientService.UploadBinaryAsync(DeviceConfig.Instance, Models.OpenIPC.RemoteFontsFolder,
            Models.OpenIPC.FileType.BetaFlightFonts, "font.png");
        await _sshClientService.UploadBinaryAsync(DeviceConfig.Instance, Models.OpenIPC.RemoteFontsFolder,
            Models.OpenIPC.FileType.BetaFlightFonts, "font_hd.png");

        // upload vtxmenu.ini /etc
        await _sshClientService.UploadBinaryAsync(DeviceConfig.Instance, Models.OpenIPC.RemoteEtcFolder, "vtxmenu.ini");


        // ensure file is unix formatted
        await _sshClientService.ExecuteCommandAsync(DeviceConfig.Instance, "dos2unix /etc/vtxmenu.ini");

        // reboot
        await _sshClientService.ExecuteCommandAsync(DeviceConfig.Instance, DeviceCommands.RebootCommand);

        //Thread.Sleep(3000);
        
        var MsgBox = MessageBoxManager
            .GetMessageBoxStandard("Done!", "MSPOSD setup, wait for device to restart!", ButtonEnum.Ok);
        await MsgBox.ShowAsync();
    }

    private async void AddMSPOSDExtra()
    {
        // if "%1" == "mspextra" (
        // 	plink -ssh root@%2 -pw %3 sed -i 's/echo \"Starting wifibroadcast service...\"/echo \"\&L70 \&F35 CPU:\&C \&B Temp:\&T\" ">"\/tmp\/MSPOSD.msg "\&"/' /etc/init.d/S98datalink
        // 	plink -ssh root@%2 -pw %3 reboot	
        // )
        // 
        Log.Debug("MSPOSDExtra executed"); 
        await _sshClientService.ExecuteCommandAsync(DeviceConfig.Instance, DeviceCommands.MSPOSDExtraCommand);
        await _sshClientService.ExecuteCommandAsync(DeviceConfig.Instance, DeviceCommands.DataLinkRestart);
        
        //TODO: do we need to restart the camera?
        //await _sshClientService.ExecuteCommandAsync(DeviceConfig.Instance, DeviceCommands.RebootCommand);
        
        var MsgBox = MessageBoxManager
            .GetMessageBoxStandard("Done!", "Please wait fir datalink to restart!", ButtonEnum.Ok);
        await MsgBox.ShowAsync();
        
    }
    
    private async void UploadINav()
    {
        Log.Debug("UploadINavCommand executed");
        // upload betaflight fonts
        await _sshClientService.ExecuteCommandAsync(DeviceConfig.Instance, $"mkdir {Models.OpenIPC.RemoteFontsFolder}");
        
        await _sshClientService.UploadBinaryAsync(DeviceConfig.Instance, Models.OpenIPC.RemoteFontsFolder,
            Models.OpenIPC.FileType.iNavFonts, "font.png");
        await _sshClientService.UploadBinaryAsync(DeviceConfig.Instance, Models.OpenIPC.RemoteFontsFolder,
            Models.OpenIPC.FileType.iNavFonts, "font_hd.png");
    }

    private async void SaveAndRestartTelemetry()
    {
        Log.Information("SaveAndRestartTelemetryCommand executed");
        //await SaveSaveAndRestartTelemetryCommand();

        var newSerial = SelectedSerialPort;
        var newBaudRate = SelectedBaudRate;
        var newRouter = SelectedRouter;
        var newMcsIndex = SelectedMcsIndex;
        var newAggregate = SelectedAggregate;
        var newRcChannel = SelectedRcChannel;

        var updatedTelemetryContent = UpdateTelemetryContent(
            newSerial,
            newBaudRate,
            newRouter,
            newMcsIndex,
            newAggregate,
            newRcChannel
        );

        TelemetryContent = updatedTelemetryContent;

        Log.Debug($"Uploading new : {Models.OpenIPC.TelemetryConfFileLoc}");
        _sshClientService.UploadFileStringAsync(DeviceConfig.Instance, Models.OpenIPC.TelemetryConfFileLoc,
            TelemetryContent);

        Log.Debug("Restarting Telemetry");
        _sshClientService.ExecuteCommandAsync(DeviceConfig.Instance, DeviceCommands.TelemetryRestartCommand);
    }


    // Method to parse the telemetryContent
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

    private async void OnTelemetryContentUpdated(TelemetryContentUpdatedMessage message)
    {
        TelemetryContent = message.Content;
        //CanConnect = true;
        ParseTelemetryContent();
    }

    private string UpdateTelemetryContent(
        string newSerial,
        string newBaudRate,
        string newRouter,
        string newMcsIndex,
        string newAggregate,
        string newRcChannel
    )
    {
        // Logic to update WfbConfContent with the new values
        var lines = TelemetryContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var regex = new Regex(
            @"(frequency|channels|driver_txpower_override|frequency24|txpower|mcsindex|stbc|ldpc|feck|fecN|router)=.*");
        var updatedContent = regex.Replace(TelemetryContent, match =>
        {
            switch (match.Groups[1].Value)
            {
                case Telemetry.Serial:
                    return $"serial={newSerial}";
                case Telemetry.Baud:
                    return $"baud={newBaudRate}";
                case Telemetry.Router:
                    return $"router={newRouter}";
                case Telemetry.McsIndex:
                    return $"mcsindex={newMcsIndex}";
                case Telemetry.Aggregate:
                    return $"aggregate={newAggregate}";
                case Telemetry.RcChannel:
                    return $"channels={newRcChannel}";

                default:
                    return match.Value;
            }
        });
        return updatedContent;
    }
}