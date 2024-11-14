using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using OpenIPC_Config.Events;
using OpenIPC_Config.Models;
using OpenIPC_Config.Services;
using Prism.Events;
using Serilog;

namespace OpenIPC_Config.ViewModels;

/// <summary>
/// Interaction logic for VRXTabViewModel.xaml
/// Parses:
/// 	echo y | pscp -scp -pw %3 root@%2:/etc/wifibroadcast.cfg .
///echo y | pscp -scp -pw %3 root@%2:/etc/modprobe.d/wfb.conf .
///echo y | pscp -scp -pw %3 root@%2:/home/radxa/scripts/screen-mode .
/// </summary>
public partial class VRXTabViewModel : ObservableObject
{
    IEventAggregator _eventAggregator;
    
    [ObservableProperty] private bool _canConnect;
    
    [ObservableProperty] private ObservableCollection<string> _resolution;
    
    [ObservableProperty] private string _selectedResolution;

    [ObservableProperty] private ObservableCollection<string> _fps;
    
    [ObservableProperty] private string _selectedFps;
    
    [ObservableProperty] private string _droneKeyChecksum;
    
    [ObservableProperty] private string _wfbConfContent;

    public VRXTabViewModel()
    {
        _eventAggregator = App.EventAggregator;
        _eventAggregator.GetEvent<DeviceTypeChangeEvent>().Subscribe(onDeviceTypeChangeEvent);
        _eventAggregator.GetEvent<AppMessageEvent>().Subscribe(OnAppMessage);
        _eventAggregator.GetEvent<RadxaContentUpdateChangeEvent>().Subscribe(OnRadxaContentUpdateChange);
        
        InitializeCollections();
    }

    private void OnAppMessage(AppMessage appMessage)
    {
        if (appMessage.CanConnect)
        {
            CanConnect = appMessage.CanConnect;
            //Log.Information($"CanConnect {CanConnect.ToString()}");
        }

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

            if (Fps.Contains(fps))
            {
                SelectedFps = fps;
            }
            if (Resolution.Contains(resolution))
            {
                SelectedResolution = resolution;
            }

        }

        if (!string.IsNullOrEmpty(radxaContentUpdatedMessage.WfbConfContent))
        {
            //WifiConfigParser parser = new WifiConfigParser();
            //parser.ParseConfigString(radxaContentUpdatedMessage.WfbConfContent);
            
        }
        
        if (!string.IsNullOrEmpty(radxaContentUpdatedMessage.DroneKeyContent))
        {
            //var droneKey = radxaContentUpdatedMessage.DroneKeyContent;
            //DroneKeyChecksum = droneKey;
        }

    }
    
    [RelayCommand]
    private async void RestartWfb()
    {
        await MessageBoxManager.GetMessageBoxStandard("Warning", "Not implemented yet", ButtonEnum.Ok).ShowAsync();

        // update the files here
        // VRX
        // Resolution /config/scripts/screen-mode
        // FPS /config/scripts/screen-mode
        // contents of /etc/default/wifibroadcast
        
    }
    
    
    [RelayCommand]
    private async void EnableVrxMajestic()
    {
        await MessageBoxManager.GetMessageBoxStandard("Warning", "Not implemented yet", ButtonEnum.Ok).ShowAsync();
    }

    [RelayCommand]
    private async void EnableVrxMSPDisplayport()
    {
        await MessageBoxManager.GetMessageBoxStandard("Warning", "Not implemented yet", ButtonEnum.Ok).ShowAsync();
    }

}