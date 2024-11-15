using CommunityToolkit.Mvvm.ComponentModel;
using OpenIPC_Config.Events;
using OpenIPC_Config.Models;
using OpenIPC_Config.Views;
using Prism.Events;

namespace OpenIPC_Config.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IEventAggregator _eventAggregator;

    [ObservableProperty] private bool isVRXEnabled;

    public MainViewModel()
    {
        IsVRXEnabled = false;
        _eventAggregator = EventAggregator.Current;

        _eventAggregator.GetEvent<DeviceTypeChangeEvent>().Subscribe(onDeviceTypeChangeEvent);
    }

    //[ObservableProperty] private string _greeting = "Welcome to Avalonia!";

    // This method is automatically called when `isVRXEnabled` changes
    partial void OnIsVRXEnabledChanged(bool value)
    {
        if (true) MainView.TabControlInstance.InvalidateVisual();
    }

    private void onDeviceTypeChangeEvent(DeviceType deviceTypeEvent)
    {
        if (deviceTypeEvent == DeviceType.Radxa)
            IsVRXEnabled = true;
        // var targetTab = MainView.TabControlInstance.Items
        //     .OfType<TabItem>()
        //     .FirstOrDefault(tab => tab.Header?.ToString() == "WFB-GS");
        else
            IsVRXEnabled = false;
    }
}