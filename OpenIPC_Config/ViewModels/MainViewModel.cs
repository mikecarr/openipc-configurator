using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenIPC_Config.Events;
using OpenIPC_Config.Models;
using OpenIPC_Config.Services;
using OpenIPC_Config.Views;
using Prism.Events;
using Serilog;

namespace OpenIPC_Config.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IEventAggregator _eventAggregator;

    [ObservableProperty] private bool isVRXEnabled;
    
    [ObservableProperty] private DeviceConfig _deviceConfig;

    public MainViewModel()
    {
        IsVRXEnabled = false;
        _eventAggregator = EventAggregator.Current;
        
        LoadSettings();

        _eventAggregator.GetEvent<DeviceTypeChangeEvent>().Subscribe(onDeviceTypeChangeEvent);
    }

    private void LoadSettings()
    {
        var settings = SettingsManager.LoadSettings(_eventAggregator);
        _deviceConfig = DeviceConfig.Instance;
        var selectedDeviceType = settings.DeviceType;
        _eventAggregator.GetEvent<DeviceTypeChangeEvent>().Publish(selectedDeviceType);
        
    }
    
    // This method is automatically called when `isVRXEnabled` changes
    partial void OnIsVRXEnabledChanged(bool value)
    {
        if (true) MainView.TabControlInstance.InvalidateVisual();
        
    }

    private void onDeviceTypeChangeEvent(DeviceType deviceTypeEvent)
    {
        Log.Debug($"Device type changed to: {deviceTypeEvent}");
 
        if (deviceTypeEvent == DeviceType.Radxa || deviceTypeEvent == DeviceType.NVR)
        {
            IsVRXEnabled = true;
            var targetTab = MainView.TabControlInstance.Items
                .OfType<TabItem>()
                .FirstOrDefault(tab => tab.Header?.ToString() == "WFB-GS");
            targetTab.IsSelected = true;
        }
        else
        {
            IsVRXEnabled = false;
            var targetTab = MainView.TabControlInstance.Items
                .OfType<TabItem>()
                .FirstOrDefault(tab => tab.Header?.ToString() == "WFB");
            targetTab.IsSelected = true;
        }
    }
}