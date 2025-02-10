using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData.Binding;
using MsBox.Avalonia;
using OpenIPC_Config.Events;
using OpenIPC_Config.Models;
using OpenIPC_Config.Services;
using Serilog;

namespace OpenIPC_Config.ViewModels;

public partial class WfbTabViewModel : ViewModelBase
{
    private readonly Dictionary<int, string> _24FrequencyMapping = FrequencyMappings.Frequency24GHz;
    private readonly Dictionary<int, string> _58FrequencyMapping = FrequencyMappings.Frequency58GHz;
    private bool _isDisposed;
    
    private readonly IYamlConfigService _yamlConfigService;
    private readonly Dictionary<string, string> _wfbYamlConfig = new();
    
    private bool _isWfbYamlEnabled = false;

    public WfbTabViewModel(ILogger logger,
        ISshClientService sshClientService,
        IEventSubscriptionService eventSubscriptionService,
        IYamlConfigService yamlConfigService)
        : base(logger, sshClientService, eventSubscriptionService)
    {
        _yamlConfigService = yamlConfigService;
        InitializeCollections();

        RestartWfbCommand = new RelayCommand(RestartWfb);

        SubscribeToEvents();
    }

    public ICommand RestartWfbCommand { get; }

    #region Observable Properties

    [ObservableProperty] private bool _canConnect;

    [ObservableProperty] private string _wfbConfContent;

    [ObservableProperty] private int _selectedChannel;

    [ObservableProperty] private int _selectedPower24GHz;

    [ObservableProperty] private int _selectedBandwidth;

    [ObservableProperty] private int _selectedPower;

    [ObservableProperty] private int _selectedLdpc;

    [ObservableProperty] private int _selectedMcsIndex;

    [ObservableProperty] private int _selectedStbc;

    [ObservableProperty] private int _selectedFecK;

    [ObservableProperty] private int _selectedFecN;

    [ObservableProperty] private string _selectedFrequency24String;

    [ObservableProperty] private string _selectedFrequency58String;

    #endregion

    #region Collections

    [ObservableProperty] private ObservableCollection<string> _frequencies58GHz;

    [ObservableProperty] private ObservableCollection<string> _frequencies24GHz;

    [ObservableProperty] private ObservableCollection<int> _power58GHz;

    [ObservableProperty] private ObservableCollection<int> _power24GHz;

    [ObservableProperty] private ObservableCollection<int> _bandwidth;

    [ObservableProperty] private ObservableCollection<int> _mcsIndex;

    [ObservableProperty] private ObservableCollection<int> _stbc;

    [ObservableProperty] private ObservableCollection<int> _ldpc;

    [ObservableProperty] private ObservableCollection<int> _fecK;

    [ObservableProperty] private ObservableCollection<int> _fecN;

    #endregion

    #region Initialization

    private void InitializeCollections()
    {
        Frequencies58GHz = new ObservableCollectionExtended<string>(_58FrequencyMapping.Values);
        Frequencies24GHz = new ObservableCollectionExtended<string>(_24FrequencyMapping.Values);
        Power58GHz = new ObservableCollectionExtended<int> { 1, 5, 10, 15, 20, 25, 30 };
        Power24GHz = new ObservableCollectionExtended<int> { 1, 20, 25, 30, 35, 40 };
        Bandwidth = new ObservableCollectionExtended<int> { 20, 40 };
        McsIndex = new ObservableCollectionExtended<int>(Enumerable.Range(1, 31));
        Stbc = new ObservableCollectionExtended<int> { 0, 1 };
        Ldpc = new ObservableCollectionExtended<int> { 0, 1 };
        FecK = new ObservableCollectionExtended<int>(Enumerable.Range(0, 13));
        FecN = new ObservableCollectionExtended<int>(Enumerable.Range(0, 13));
    }

    private void SubscribeToEvents()
    {
        EventSubscriptionService.Subscribe<WfbConfContentUpdatedEvent, WfbConfContentUpdatedMessage>(
            OnWfbConfContentUpdated);
        EventSubscriptionService.Subscribe<WfbYamlContentUpdatedEvent, WfbYamlContentUpdatedMessage>(
            OnWfbYamlContentUpdated);
        
        EventSubscriptionService.Subscribe<AppMessageEvent, AppMessage>(OnAppMessage);
    }

    #endregion

    #region Methods

    private void OnWfbConfContentUpdated(WfbConfContentUpdatedMessage message)
    {
        WfbConfContent = message.Content;
        ParseWfbConfContent();
    }
    
    private void OnWfbYamlContentUpdated(WfbYamlContentUpdatedMessage message)
    {
         
        WfbConfContent = message.Content;
        if(!string.IsNullOrEmpty(message.Content))
            _isWfbYamlEnabled = true;
        
        // parse file
        _yamlConfigService.ParseYaml(message.Content, _wfbYamlConfig);

        // update ui
        UpdateViewModelPropertiesFromYaml();
    }
    
    private void UpdateViewModelPropertiesFromYaml()
    {
        if (_wfbYamlConfig.TryGetValue("wireless.txpower", out var txPower)) SelectedPower = TryParseInt(txPower, SelectedPower);
        if (_wfbYamlConfig.TryGetValue("wireless.channel", out var txChannel))
        {
            SelectedChannel = TryParseInt(txChannel, SelectedChannel);
            HandleFrequencyKey(txChannel);
            
        }
        if (_wfbYamlConfig.TryGetValue("broadcast.fec_k", out var broadcastFecK)) SelectedFecK = TryParseInt(broadcastFecK, SelectedFecK);
        if (_wfbYamlConfig.TryGetValue("broadcast.fec_n", out var broadcastFecN)) SelectedFecN = TryParseInt(broadcastFecN, SelectedFecN);

    }

    private void OnAppMessage(AppMessage message)
    {
        CanConnect = message.CanConnect;
    }

    partial void OnWfbConfContentChanged(string value)
    {
        if (!string.IsNullOrEmpty(value)) ParseWfbConfContent();
    }

    partial void OnSelectedFrequency24StringChanged(string value)
    {
        if (!string.IsNullOrEmpty(value)) HandleFrequencyChange(value, _24FrequencyMapping);
    }

    partial void OnSelectedFrequency58StringChanged(string value)
    {
        if (!string.IsNullOrEmpty(value)) HandleFrequencyChange(value, _58FrequencyMapping);
    }

    private void ParseWfbConfContent()
    {
        if (string.IsNullOrEmpty(WfbConfContent))
        {
            Logger.Debug("WfbConfContent is empty.");
            return;
        }

        var lines = WfbConfContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var parts = line.Split('=');
            if (parts.Length != 2) continue;

            var key = parts[0].Trim();
            var value = parts[1].Trim();

            // Map values to properties
            MapWfbKeyToProperty(key, value);
        }
    }

    private void MapWfbKeyToProperty(string key, string value)
    {
        switch (key)
        {
            case Wfb.Frequency:
                HandleFrequencyKey(value);
                break;
            case Wfb.Txpower:
                SelectedPower24GHz = TryParseInt(value, SelectedPower24GHz);
                break;
            case Wfb.DriverTxpowerOverride:
                SelectedPower = TryParseInt(value, SelectedPower);
                break;
            case Wfb.Bandwidth:
                SelectedBandwidth = TryParseInt(value, SelectedBandwidth);
                break;
            case Wfb.McsIndex:
                SelectedMcsIndex = TryParseInt(value, SelectedMcsIndex);
                break;
            case Wfb.Ldpc:
                SelectedLdpc = TryParseInt(value, SelectedLdpc);
                break;
            case Wfb.Stbc:
                SelectedStbc = TryParseInt(value, SelectedStbc);
                break;
            case Wfb.FecK:
                SelectedFecK = TryParseInt(value, SelectedFecK);
                break;
            case Wfb.FecN:
                SelectedFecN = TryParseInt(value, SelectedFecN);
                break;
            case Wfb.Channel:
                SelectedChannel = TryParseInt(value, SelectedChannel);
                HandleFrequencyKey(value);
                break;
        }
    }

    private void HandleFrequencyKey(string value)
    {
        if (int.TryParse(value, out var frequency))
        {
            SelectedFrequency58String = _58FrequencyMapping.ContainsKey(frequency)
                ? _58FrequencyMapping[frequency]
                : SelectedFrequency58String;

            SelectedFrequency24String = _24FrequencyMapping.ContainsKey(frequency)
                ? _24FrequencyMapping[frequency]
                : SelectedFrequency24String;
        }
    }

    private int TryParseInt(string value, int fallback)
    {
        return int.TryParse(value, out var result) ? result : fallback;
    }

    private async void RestartWfb()
    {
        UpdateUIMessage("Restarting WFB...");

        EventSubscriptionService.Publish<TabMessageEvent, string>("Restart Pushed");

        UpdateUIMessage("Getting new content");

        var newFrequency58 = SelectedFrequency58String;
        var newFrequency24 = SelectedFrequency24String;

        var newPower58 = SelectedPower;
        var newPower24 = SelectedPower24GHz;
        var newBandwidth = SelectedBandwidth;
        var newMcsIndex = SelectedMcsIndex;
        var newStbc = SelectedStbc;
        var newLdpc = SelectedLdpc;
        var newFecK = SelectedFecK;
        var newFecN = SelectedFecN;
        var newChannel = SelectedChannel;

        // Update WfbConfContent with the new values
        var updatedWfbConfContent = UpdateWfbConfContent(
            WfbConfContent,
            newFrequency58,
            newFrequency24,
            newPower58,
            newPower24,
            newBandwidth,
            newMcsIndex,
            newStbc,
            newLdpc,
            newFecK,
            newFecN,
            newChannel
        );

        WfbConfContent = updatedWfbConfContent;

        // make sure we are not uploading an empty string/file
        if (string.IsNullOrEmpty(updatedWfbConfContent))
            await MessageBoxManager.GetMessageBoxStandard("Error", "WfbConfContent is empty").ShowAsync();

        UpdateUIMessage($"Uploading new {OpenIPC.WfbConfFileLoc}");


        Logger.Information($"Uploading new : {OpenIPC.WfbConfFileLoc}");
        await SshClientService.UploadFileStringAsync(DeviceConfig.Instance, OpenIPC.WfbConfFileLoc, WfbConfContent);

        UpdateUIMessage("Restarting Wfb");

        await SshClientService.ExecuteCommandAsync(DeviceConfig.Instance, DeviceCommands.WfbRestartCommand);
        UpdateUIMessage("Restarting Wfb..done");
    }

    private void HandleFrequencyChange(string newValue, Dictionary<int, string> frequencyMapping)
    {
        // Reset the other frequency collection to its first value
        if (frequencyMapping == _24FrequencyMapping)
        {
            SelectedFrequency58String = Frequencies58GHz.FirstOrDefault();
            SelectedPower = Power58GHz.FirstOrDefault();
        }
        else if (frequencyMapping == _58FrequencyMapping)
        {
            SelectedFrequency24String = Frequencies24GHz.FirstOrDefault();
            SelectedPower24GHz = Power24GHz.FirstOrDefault();
        }

        // Extract the channel number using a regular expression
        var match = Regex.Match(newValue, @"\[(\d+)\]");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var channel))
            SelectedChannel = channel;
        else
            SelectedChannel = -1; // Default value if parsing fails
    }


    private string UpdateWfbConfContent(
        string wfbConfContent,
        string newFrequency58,
        string newFrequency24,
        int newPower58,
        int newPower24,
        int newBandwidth,
        int newMcsIndex,
        int newStbc,
        int newLdpc,
        int newFecK,
        int newFecN,
        int newChannel
    )
    {
        // Logic to update WfbConfContent with the new values
        var lines = wfbConfContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        // This regex matches configuration lines in the `wfbConfContent` that define specific settings,
        // while ignoring lines that start with a `#` (comments). It looks for lines that:
        // - Do NOT start with a `#` (negative lookahead: `^(?!#.*)`).
        // - Contain one of the following keys: `frequency`, `channel`, `driver_txpower_override`,
        //   `frequency24`, `bandwidth`, `txpower`, `mcs_index`, `stbc`, `ldpc`, `fec_k`, or `fec_n`.
        // - Have an equals sign (`=`) followed by any value (`=.*`).
        // The `RegexOptions.Multiline` ensures that each line in the input is treated separately,
        // allowing the regex to match lines individually in a multi-line string.
        var regex = new Regex(
            @"^(?!#.*)(frequency|channel|driver_txpower_override|frequency24|bandwidth|txpower|mcs_index|stbc|ldpc|fec_k|fec_n)=.*",
            RegexOptions.Multiline);

        var updatedContent = regex.Replace(wfbConfContent, match =>
        {
            var key = match.Groups[1].Value;
            Logger.Debug($"Updating key: {key}");

            return key switch
            {
                "channel" => $"channel={newChannel}",
                "driver_txpower_override" => $"driver_txpower_override={newPower58}",
                "txpower" => $"txpower={newPower24}",
                "bandwidth" => $"bandwidth={newBandwidth}",
                "mcs_index" => $"mcs_index={newMcsIndex}",
                "stbc" => $"stbc={newStbc}",
                "ldpc" => $"ldpc={newLdpc}",
                "fec_k" => $"fec_k={newFecK}",
                "fec_n" => $"fec_n={newFecN}",
                _ => match.Value // Default: return the original line
            };
        });
        return updatedContent;
    }

    #endregion
}