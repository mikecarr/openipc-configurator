using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using OpenIPC_Config.Events;
using OpenIPC_Config.Services;
using Prism.Events;
using Serilog;

namespace OpenIPC_Config.ViewModels;

public partial class WfbGSTabViewModel : ObservableObject
{
    IEventAggregator _eventAggregator;
    
    [ObservableProperty] private bool _canConnect;
    
    [ObservableProperty] private ObservableCollection<string> _frequencies58GHz;
    [ObservableProperty] private ObservableCollection<int> _power58GHz;
    
    [ObservableProperty] private string _selectedFrequency58String;
    [ObservableProperty] private int _selectedPower;
    
    [ObservableProperty] private string _wifiRegion;
    
    
    private readonly Dictionary<int, string> _58frequencyMapping = new()
    {
        { 36, "5180 MHz [36]" },
        { 40, "5200 MHz [40]" },
        { 44, "5220 MHz [44]" },
        { 48, "5240 MHz [48]" },
        { 52, "5260 MHz [52]" },
        { 56, "5280 MHz [56]" },
        { 60, "5300 MHz [60]" },
        { 64, "5320 MHz [64]" },
        { 100, "5500 MHz [100]" },
        { 104, "5520 MHz [104]" },
        { 108, "5540 MHz [108]" },
        { 112, "5560 MHz [112]" },
        { 116, "5580 MHz [116]" },
        { 120, "5600 MHz [120]" },
        { 124, "5620 MHz [124]" },
        { 128, "5640 MHz [128]" },
        { 132, "5660 MHz [132]" },
        { 136, "5680 MHz [136]" },
        { 140, "5700 MHz [140]" },
        { 144, "5720 MHz [144]" },
        { 149, "5745 MHz [149]" },
        { 153, "5765 MHz [153]" },
        { 157, "5785 MHz [157]" },
        { 161, "5805 MHz [161]" },
        { 165, "5825 MHz [165]" },
        { 169, "5845 MHz [169]" },
        { 173, "5865 MHz [173]" },
        { 177, "5885 MHz [177]" }
    };
    
    public WfbGSTabViewModel()
    {
        InitializeCollections();
        _eventAggregator = App.EventAggregator;
        _eventAggregator?.GetEvent<RadxaContentUpdateChangeEvent>().Subscribe(OnRadxaContentUpdateChange);
        
        _eventAggregator.GetEvent<AppMessageEvent>().Subscribe(OnAppMessage);

    }

    
    private void OnAppMessage(AppMessage appMessage)
    {
        if (appMessage.CanConnect)
        {
            CanConnect = appMessage.CanConnect;
            Log.Information($"CanConnect {CanConnect.ToString()}");
        }

    }
    
    
    [RelayCommand]
    private async void RestartWfb()
    {
        await MessageBoxManager.GetMessageBoxStandard("Warning", "Not implemented yet", ButtonEnum.Ok).ShowAsync();

        // update the files here
    }

    private void InitializeCollections()
    {
        Frequencies58GHz = new ObservableCollection<string>(_58frequencyMapping.Values);
        Power58GHz = new ObservableCollection<int> { 1, 5, 10, 15, 20, 25, 30 };
    }

    private void OnRadxaContentUpdateChange(RadxaContentUpdatedMessage radxaContentUpdatedMessage)
    {
        var wifiBroadcastContent = radxaContentUpdatedMessage.WifiBroadcastContent;
        if (!string.IsNullOrEmpty(wifiBroadcastContent))
        {
            var configContent = radxaContentUpdatedMessage.WifiBroadcastContent;
            var parser = new WifiConfigParser();
            parser.ParseConfigString(configContent);

            var channel = parser.WifiChannel;
            
            string frequencyString;
            if (_58frequencyMapping.TryGetValue(channel, out frequencyString))
            {
                SelectedFrequency58String = frequencyString;
            }
            
            var wifiRegion = parser.WifiRegion;
            if (!string.IsNullOrEmpty(wifiRegion))
            {
                WifiRegion = parser.WifiRegion;
            }

        }
        
        var wfbConfContent = radxaContentUpdatedMessage.WfbConfContent;
        if (!string.IsNullOrEmpty(wfbConfContent))
        {
            var parser = new WfbGsConfigParser();
            parser.ParseConfigString(wfbConfContent);
            var power = parser.TxPower;
            if (int.TryParse(power, out var parsedPower))
            {
                SelectedPower = parsedPower;
            }

        }
    }
    
   
}