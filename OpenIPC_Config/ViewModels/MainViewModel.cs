using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenIPC_Config.Events;
using OpenIPC_Config.Models;
using OpenIPC_Config.Services;
using Prism.Events;
using Serilog;

namespace OpenIPC_Config.ViewModels;

public partial class MainViewModel : ViewModelBase
{

    [ObservableProperty]
    private bool isVRXEnabled;

    [ObservableProperty]
    private DeviceConfig _deviceConfig;

    [ObservableProperty]
    private string selectedTab;
    
    public WfbTabViewModel WfbTabViewModel { get; }
    public WfbGSTabViewModel WfbGSTabViewModel { get; }
    public TelemetryTabViewModel TelemetryTabViewModel { get; }
    public CameraSettingsTabViewModel CameraSettingsTabViewModel { get; }
    public VRXTabViewModel VRXTabViewModel { get; }
    public SetupTabViewModel SetupTabViewModel { get; }
    public ConnectControlsViewModel ConnectControlsViewModel { get; }
    public LogViewerViewModel LogViewerViewModel { get; }
    public StatusBarViewModel StatusBarViewModel { get; }

    public MainViewModel(ILogger logger,
        ISshClientService sshClientService,
        IEventAggregator eventAggregator)
        : base(logger, sshClientService, eventAggregator)
    {
        
        IsVRXEnabled = false;

        LoadSettings();

        // Subscribe to device type change events
        EventAggregator.GetEvent<DeviceTypeChangeEvent>().Subscribe(OnDeviceTypeChangeEvent);
    }

    private void LoadSettings()
    {
        // Load settings via the SettingsManager
        var settings = SettingsManager.LoadSettings(EventAggregator);
        _deviceConfig = DeviceConfig.Instance;

        // Publish the initial device type
        EventAggregator.GetEvent<DeviceTypeChangeEvent>().Publish(settings.DeviceType);
    }

    private void OnDeviceTypeChangeEvent(DeviceType deviceTypeEvent)
    {
        Log.Debug($"Device type changed to: {deviceTypeEvent}");

        // Update IsVRXEnabled based on the device type
        IsVRXEnabled = deviceTypeEvent == DeviceType.Radxa || deviceTypeEvent == DeviceType.NVR;

        // Update the selected tab based on the device type
        SelectedTab = IsVRXEnabled ? "WFB-GS" : "WFB";

        // Notify the view of tab changes
        EventAggregator.GetEvent<TabSelectionChangeEvent>().Publish(SelectedTab);
    }
}
