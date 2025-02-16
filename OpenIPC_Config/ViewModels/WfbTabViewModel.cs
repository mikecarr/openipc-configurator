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

/// <summary>
/// ViewModel for managing WiFi Broadcast (WFB) settings and configuration
/// </summary>
public partial class WfbTabViewModel : ViewModelBase
{
    #region Private Fields
    private readonly Dictionary<int, string> _24FrequencyMapping = FrequencyMappings.Frequency24GHz;
    private readonly Dictionary<int, string> _58FrequencyMapping = FrequencyMappings.Frequency58GHz;
    private bool _isDisposed;
    #endregion

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
    [ObservableProperty] private int _maxPower58GHz = 50;
    [ObservableProperty] private int _maxPower24GHz = 50;
    #endregion

    #region Commands
    /// <summary>
    /// Command to restart the WFB service
    /// </summary>
    public ICommand RestartWfbCommand { get; set; }
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of WfbTabViewModel
    /// </summary>
    public WfbTabViewModel(
        ILogger logger,
        ISshClientService sshClientService,
        IEventSubscriptionService eventSubscriptionService)
        : base(logger, sshClientService, eventSubscriptionService)
    {
        InitializeCollections();
        InitializeCommands();
        SubscribeToEvents();
    }
    #endregion

    #region Initialization Methods
    private void InitializeCollections()
    {
        Frequencies58GHz = new ObservableCollectionExtended<string>(_58FrequencyMapping.Values);
        Frequencies24GHz = new ObservableCollectionExtended<string>(_24FrequencyMapping.Values);

        Power58GHz = new ObservableCollection<int>(Enumerable.Range(1, MaxPower58GHz).Select(i => (i * 5)));
        Power24GHz = new ObservableCollection<int>(Enumerable.Range(1, MaxPower24GHz).Select(i => (i * 5)));

        Bandwidth = new ObservableCollectionExtended<int> { 20, 40 };
        McsIndex = new ObservableCollectionExtended<int>(Enumerable.Range(1, 31));
        Stbc = new ObservableCollectionExtended<int> { 0, 1 };
        Ldpc = new ObservableCollectionExtended<int> { 0, 1 };
        FecK = new ObservableCollectionExtended<int>(Enumerable.Range(0, 13));
        FecN = new ObservableCollectionExtended<int>(Enumerable.Range(0, 13));
    }

    private void InitializeCommands()
    {
        RestartWfbCommand = new RelayCommand(RestartWfb);
    }

    private void SubscribeToEvents()
    {
        EventSubscriptionService.Subscribe<WfbConfContentUpdatedEvent, WfbConfContentUpdatedMessage>(
            OnWfbConfContentUpdated);
        EventSubscriptionService.Subscribe<AppMessageEvent, AppMessage>(OnAppMessage);
    }
    #endregion

    #region Event Handlers
    private void OnWfbConfContentUpdated(WfbConfContentUpdatedMessage message)
    {
        WfbConfContent = message.Content;
        ParseWfbConfContent();
    }

    private void OnAppMessage(AppMessage message)
    {
        CanConnect = message.CanConnect;
    }
    #endregion

    #region Property Change Handlers
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
    #endregion

    #region Command Handlers
    private async void RestartWfb()
    {
        UpdateUIMessage("Restarting WFB...");
        EventSubscriptionService.Publish<TabMessageEvent, string>("Restart Pushed");

        var updatedWfbConfContent = UpdateWfbConfContent(
            WfbConfContent,
            SelectedFrequency58String,
            SelectedFrequency24String,
            SelectedPower,
            SelectedPower24GHz,
            SelectedBandwidth,
            SelectedMcsIndex,
            SelectedStbc,
            SelectedLdpc,
            SelectedFecK,
            SelectedFecN,
            SelectedChannel
        );

        if (string.IsNullOrEmpty(updatedWfbConfContent))
        {
            await MessageBoxManager.GetMessageBoxStandard("Error", "WfbConfContent is empty").ShowAsync();
            return;
        }

        WfbConfContent = updatedWfbConfContent;

        Logger.Information($"Uploading new : {OpenIPC.WfbConfFileLoc}");
        await SshClientService.UploadFileStringAsync(DeviceConfig.Instance, OpenIPC.WfbConfFileLoc, WfbConfContent);

        UpdateUIMessage("Restarting Wfb");
        await SshClientService.ExecuteCommandAsync(DeviceConfig.Instance, DeviceCommands.WfbRestartCommand);
        UpdateUIMessage("Restarting Wfb..done");
    }
    #endregion

    #region Helper Methods
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

        // Extract the channel number
        var match = Regex.Match(newValue, @"\[(\d+)\]");
        SelectedChannel = match.Success && int.TryParse(match.Groups[1].Value, out var channel)
            ? channel
            : -1;
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
        int newChannel)
    {
        var regex = new Regex(
            @"^(?!#.*)(frequency|channel|driver_txpower_override|frequency24|bandwidth|txpower|mcs_index|stbc|ldpc|fec_k|fec_n)=.*",
            RegexOptions.Multiline);

        return regex.Replace(wfbConfContent, match =>
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
                _ => match.Value
            };
        });
    }
    #endregion
}