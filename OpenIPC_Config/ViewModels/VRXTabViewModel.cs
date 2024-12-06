using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using OpenIPC_Config.Events;
using OpenIPC_Config.Models;
using OpenIPC_Config.Services;
using Prism.Events;
using Serilog;

namespace OpenIPC_Config.ViewModels;

/// <summary>
///     Interaction logic for VRXTabViewModel.xaml
///     Parses:
///     echo y | pscp -scp -pw %3 root@%2:/etc/wifibroadcast.cfg .
///     echo y | pscp -scp -pw %3 root@%2:/etc/modprobe.d/wfb.conf .
///     echo y | pscp -scp -pw %3 root@%2:/home/radxa/scripts/screen-mode .
/// </summary>
public partial class VRXTabViewModel : ViewModelBase
{
    [ObservableProperty] private bool _canConnect;

    [ObservableProperty] private string _droneKeyChecksum;
    private readonly IEventAggregator _eventAggregator;

    [ObservableProperty] private ObservableCollection<string> _fps;

    [ObservableProperty] private ObservableCollection<string> _resolution;

    [ObservableProperty] private string _selectedFps;

    [ObservableProperty] private string _selectedResolution;

    [ObservableProperty] private string _wfbConfContent;
    
    private readonly ISshClientService _sshClientService;

    public VRXTabViewModel()
    {
        _eventAggregator = App.EventAggregator;
        _eventAggregator.GetEvent<DeviceTypeChangeEvent>().Subscribe(onDeviceTypeChangeEvent);
        _eventAggregator.GetEvent<AppMessageEvent>().Subscribe(OnAppMessage);
        _eventAggregator.GetEvent<RadxaContentUpdateChangeEvent>().Subscribe(OnRadxaContentUpdateChange);
        _sshClientService = new SshClientService(_eventAggregator);
        
        InitializeCollections();
    }

    private void OnAppMessage(AppMessage appMessage)
    {
        if (appMessage.CanConnect) CanConnect = appMessage.CanConnect;
        //Log.Information($"CanConnect {CanConnect.ToString()}");
    }

    private void onDeviceTypeChangeEvent(DeviceType deviceType)
    {
        //throw new System.NotImplementedException();
    }


    private void InitializeCollections()
    {
        Resolution = new ObservableCollection<string>
        {
            "1280x720", "1920x1080"
        };

        Fps = new ObservableCollection<string>
        {
            "20", "30", "40", "50", "60", "70", "80", "90", "100", "110", "120"
        };
    }

    private void OnRadxaContentUpdateChange(RadxaContentUpdatedMessage radxaContentUpdatedMessage)
    {
        Log.Debug("Parsing radxaContentUpdatedMessage.");
        if (!string.IsNullOrEmpty(radxaContentUpdatedMessage.WifiBroadcastContent))
        {
            var configContent = radxaContentUpdatedMessage.WifiBroadcastContent;
            var parser = new WifiConfigParser();
            parser.ParseConfigString(configContent);

            var channel = parser.WifiChannel;
        }

        if (!string.IsNullOrEmpty(radxaContentUpdatedMessage.ScreenModeContent))
        {
            // value comes across as 1920x1080@60
            var screenMode = radxaContentUpdatedMessage.ScreenModeContent;
            var cleanScreenMode = Utilities.RemoveSpecialCharacters(screenMode);
            var screenModeSplit = cleanScreenMode.Split('@');

            var resolution = screenModeSplit[0];
            var fps = screenModeSplit[1];

            if (Fps.Contains(fps)) SelectedFps = fps;
            if (Resolution.Contains(resolution)) SelectedResolution = resolution;
        }

        if (!string.IsNullOrEmpty(radxaContentUpdatedMessage.WfbConfContent))
        {
            //WifiConfigParser parser = new WifiConfigParser();
            //parser.ParseConfigString(radxaContentUpdatedMessage.WfbConfContent);
        }

        if (!string.IsNullOrEmpty(radxaContentUpdatedMessage.DroneKeyContent))
        {
            var droneKey = radxaContentUpdatedMessage.DroneKeyContent;
            DroneKeyChecksum = droneKey;
        }
    }

    [RelayCommand]
    private async Task RestartWfb()
    {
        //await MessageBoxManager.GetMessageBoxStandard("Warning", "Not implemented yet").ShowAsync();

        var resolution = SelectedResolution;
        var fps = SelectedFps;
        
        //format 1920x1080@60
        var screenMode = $"{resolution}@{fps}\n";
        
        //Task UploadFileStringAsync(DeviceConfig deviceConfig, string remotePath, string fileContent)
        await _sshClientService.UploadFileStringAsync(DeviceConfig.Instance, Models.OpenIPC.ScreenModeFileLoc, screenMode);
        
        // update the files here
        // VRX
        // Resolution /config/scripts/screen-mode
        // FPS /config/scripts/screen-mode
        // contents of /etc/default/wifibroadcast
    }


    [RelayCommand]
    private async Task EnableVrxMajestic()
    {
        await MessageBoxManager.GetMessageBoxStandard("Warning", "Not implemented yet").ShowAsync();
    }

    [RelayCommand]
    private async Task EnableVrxMSPDisplayport()
    {
        await MessageBoxManager.GetMessageBoxStandard("Warning", "Not implemented yet").ShowAsync();
    }
}