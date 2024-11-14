using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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

public partial class WfbGSTabViewModel : ObservableObject
{
    private readonly ISshClientService _sshClientService;
    
    IEventAggregator _eventAggregator;
    
    [ObservableProperty] private bool _canConnect;
    
    [ObservableProperty] private ObservableCollection<string> _frequencies;
    [ObservableProperty] private ObservableCollection<int> _power;
    
    [ObservableProperty] private string _selectedFrequencyString;
    [ObservableProperty] private int _selectedPower;
    
    [ObservableProperty] private string _wifiRegion;
    [ObservableProperty] private string _gsMavlink;
    [ObservableProperty] private string _gsVideo;
    
    
    private readonly Dictionary<int, string> _frequencyMapping = new()
    {
        
        { 1, "2412 MHz [1]" },
        { 2, "2417 MHz [2]" },
        { 3, "2422 MHz [3]" },
        { 4, "2427 MHz [4]" },
        { 5, "2432 MHz [5]" },
        { 6, "2437 MHz [6]" },
        { 7, "2442 MHz [7]" },
        { 8, "2447 MHz [8]" },
        { 9, "2452 MHz [9]" },
        { 10, "2457 MHz [10]" },
        { 11, "2462 MHz [11]" },
        { 12, "2467 MHz [12]" },
        { 13, "2472 MHz [13]" },
        { 14, "2484 MHz [14]" },
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

    private WfbGsConfigParser _wfbGsConfigParser;
    private WifiConfigParser _wifiConfigParser;
    
    public WfbGSTabViewModel()
    {
        _wfbGsConfigParser = new WfbGsConfigParser();
        _wifiConfigParser = new WifiConfigParser();
        
        
        InitializeCollections();
        _eventAggregator = App.EventAggregator;
        _eventAggregator?.GetEvent<RadxaContentUpdateChangeEvent>().Subscribe(OnRadxaContentUpdateChange);
        
        _eventAggregator.GetEvent<AppMessageEvent>().Subscribe(OnAppMessage);
        
        _sshClientService = new SshClientService(_eventAggregator);

    }

    
    private void OnAppMessage(AppMessage appMessage)
    {
        if (appMessage.CanConnect)
        {
            CanConnect = appMessage.CanConnect;
            //Log.Information($"CanConnect {CanConnect.ToString()}");
        }

    }
    
    public int GetChannelNumber(string frequencyString)
    {
        foreach (var kvp in _frequencyMapping)
        {
            if (kvp.Value.Equals(frequencyString, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Key;
            }
        }

        Log.Warning($"Frequency string '{frequencyString}' not found in 5.8 GHz frequency mapping.");
        return -1; // Return -1 or another sentinel value to indicate not found
    }
    
    /// <summary>
    /// //frequency 
    //power
    //region
    /// </summary>
    [RelayCommand]
    private async void RestartWfb()
    {
        //await MessageBoxManager.GetMessageBoxStandard("Warning", "Not implemented yet", ButtonEnum.Ok).ShowAsync();
        Log.Information("Restart WFB button clicked");

        // /etc/wifibroadcast.cfg
        await UpdateWifiBroadcastCfg();
        
        // /etc/modprobe.d/wfb.conf
        await UpdateModprobeWfbConf();
        
        await MessageBoxManager.GetMessageBoxStandard("Success", "Saved!").ShowAsync();
    }

    private async Task UpdateModprobeWfbConf()
    {
        try
        {
            _wfbGsConfigParser.TxPower = _selectedPower.ToString();
            // Update the parser's properties based on user input
            var updatedConfigString = _wfbGsConfigParser.GetUpdatedConfigString();
            
            if (string.IsNullOrEmpty(updatedConfigString))
            {
                await MessageBoxManager.GetMessageBoxStandard("Error", "Updated configuration is empty").ShowAsync();
                return;
            }

            // Upload the updated configuration file
            _sshClientService.UploadFileStringAsync(DeviceConfig.Instance, Models.OpenIPC.WifiBroadcastModProbeFileLoc, updatedConfigString);
            Log.Information("Configuration file updated and uploaded successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    private async Task UpdateWifiBroadcastCfg()
    {
        // update /etc/wifibroadcast.cfg
        try
        {
            // Update the parser's properties based on user input

            _wifiConfigParser.WifiChannel = GetChannelNumber(SelectedFrequencyString);
            _wifiConfigParser.WifiRegion = _wifiRegion;
            _wifiConfigParser.GsMavlinkPeer = _gsMavlink;
            _wifiConfigParser.GsVideoPeer = _gsVideo;

            // Get the updated configuration string
            // Generate the updated configuration string
            string updatedConfigContent = _wifiConfigParser.GetUpdatedConfigString();
            
            if (string.IsNullOrEmpty(updatedConfigContent))
            {
                await MessageBoxManager.GetMessageBoxStandard("Error", "Updated configuration is empty").ShowAsync();
                return;
            }

            // Upload the updated configuration file
            _sshClientService.UploadFileStringAsync(DeviceConfig.Instance, Models.OpenIPC.WifiBroadcastFileLoc, updatedConfigContent);
            Log.Information("Configuration file updated and uploaded successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to update configuration file.");
            await MessageBoxManager.GetMessageBoxStandard("Error", "Failed to update configuration.").ShowAsync();
        }
    }

    private void InitializeCollections()
    {
        Frequencies = new ObservableCollection<string>(_frequencyMapping.Values);
        Power = new ObservableCollection<int> { 1, 5, 10, 15, 20, 25, 30 };
    }

    private void OnRadxaContentUpdateChange(RadxaContentUpdatedMessage radxaContentUpdatedMessage)
    {
        var wifiBroadcastContent = radxaContentUpdatedMessage.WifiBroadcastContent;
        if (!string.IsNullOrEmpty(wifiBroadcastContent))
        {
            var configContent = radxaContentUpdatedMessage.WifiBroadcastContent;
            
            _wifiConfigParser.ParseConfigString(configContent);

            var channel = _wifiConfigParser.WifiChannel;
            
            string frequencyString;
            if (_frequencyMapping.TryGetValue(channel, out frequencyString))
            {
                SelectedFrequencyString = frequencyString;
            }
            
            var wifiRegion = _wifiConfigParser.WifiRegion;
            if (!string.IsNullOrEmpty(wifiRegion))
            {
                WifiRegion = _wifiConfigParser.WifiRegion;
            }
            
            var gsMavlink = _wifiConfigParser.GsMavlinkPeer;
            if (!string.IsNullOrEmpty(gsMavlink))
            {
                GsMavlink = _wifiConfigParser.GsMavlinkPeer;
            }
            
            var gsVideo = _wifiConfigParser.GsVideoPeer;
            if (!string.IsNullOrEmpty(gsVideo))
            {
                GsVideo = _wifiConfigParser.GsVideoPeer;
            }
            

        }
        
        var wfbConfContent = radxaContentUpdatedMessage.WfbConfContent;
        if (!string.IsNullOrEmpty(wfbConfContent))
        {
            _wfbGsConfigParser.ParseConfigString(wfbConfContent);
            var power = _wfbGsConfigParser.TxPower;
            if (int.TryParse(power, out var parsedPower))
            {
                SelectedPower = parsedPower;    
            }
            
        }
    }
    
   
}