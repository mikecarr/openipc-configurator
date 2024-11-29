using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenIPC_Config.Events;
using OpenIPC_Config.Models;
using OpenIPC_Config.Services;
using Prism.Events;
using Serilog;
using YamlDotNet.RepresentationModel;

namespace OpenIPC_Config.ViewModels;

public partial class CameraSettingsTabViewModel : ViewModelBase
{
    private readonly ISshClientService _sshClientService;
    private DeviceConfig _deviceConfig;

    private readonly IEventAggregator _eventAggregator;

    
    [ObservableProperty] private ObservableCollection<string> _bitrate;

    [ObservableProperty] private bool _canConnect;
    [ObservableProperty] private ObservableCollection<string> _codec;
    [ObservableProperty] private ObservableCollection<string> _contrast;

    [ObservableProperty] private ObservableCollection<string> _exposure;
    [ObservableProperty] private ObservableCollection<string> _flip;
    [ObservableProperty] private ObservableCollection<string> _fps;
    [ObservableProperty] private ObservableCollection<string> _hue;
    [ObservableProperty] private ObservableCollection<string> _luminance;

    [ObservableProperty] private ObservableCollection<string> _mirror;
    
    [ObservableProperty] private ObservableCollection<string> _resolution;
    [ObservableProperty] private ObservableCollection<string> _saturation;
    
    [ObservableProperty] private ObservableCollection<string> _fpvEnabled;
    [ObservableProperty] private ObservableCollection<string> _fpvNoiseLevel;
    [ObservableProperty] private ObservableCollection<string> _fpvRoiQp;
    [ObservableProperty] private ObservableCollection<string> _fpvRefEnhance;
    [ObservableProperty] private ObservableCollection<string> _fpvRefPred;
    [ObservableProperty] private ObservableCollection<string> _fpvIntraLine;
    [ObservableProperty] private ObservableCollection<string> _fpvIntraQp;
    
    [ObservableProperty] private string _combinedFpvRoiRectValue;

    [ObservableProperty] private ObservableCollection<string> _fpvRoiRectLeft = new ObservableCollection<string> {"" };
    [ObservableProperty] private ObservableCollection<string> _fpvRoiRectTop = new ObservableCollection<string> { ""};
    [ObservableProperty] private ObservableCollection<string> _fpvRoiRectHeight = new ObservableCollection<string> {"" };
    [ObservableProperty] private ObservableCollection<string> _fpvRoiRectWidth = new ObservableCollection<string> {"" };


    [ObservableProperty] private string _selectedBitrate;


    [ObservableProperty] private string _selectedCodec;

    [ObservableProperty] private string _selectedContrast;

    [ObservableProperty] private string _selectedExposure;

    [ObservableProperty] private string _selectedFlip;

    [ObservableProperty] private string _selectedFps;

    [ObservableProperty] private string _selectedHue;

    [ObservableProperty] private string _selectedLuminance;

    [ObservableProperty] private string _selectedMirror;

    [ObservableProperty] private string _selectedResolution;

    [ObservableProperty] private string _selectedSaturation;
    
    [ObservableProperty] private string _selectedFpvEnabled;
    
    [ObservableProperty] private string _selectedFpvNoiseLevel;
    
    [ObservableProperty] private string _selectedFpvRoiQp;
    
    [ObservableProperty] private string _selectedFpvRefEnhance;
    
    [ObservableProperty] private string _selectedFpvRefPred;
    
    [ObservableProperty] private string _selectedFpvIntraLine;
    
    [ObservableProperty] private string _selectedFpvIntraQp;
    
    
    private readonly Dictionary<string, string> _yamlConfig = new();


    public CameraSettingsTabViewModel()
    {
        InitializeCollections();

        _eventAggregator = App.EventAggregator;
        _eventAggregator?.GetEvent<MajesticContentUpdatedEvent>().Subscribe(OnMajesticContentUpdated);
        _eventAggregator.GetEvent<AppMessageEvent>().Subscribe(OnAppMessage);


        RestartMajesticCommand = new RelayCommand(() => RestartMajestic());

        _sshClientService = new SshClientService(_eventAggregator);
    }

    public ICommand RestartMajesticCommand { get; private set; }




    private async void RestartMajestic()
    {
        await SaveRestartMajesticCommand();
    }

    partial void OnSelectedResolutionChanged(string value)
    {
        // Custom logic when the property changes
        Log.Debug($"SelectedResolution updated to {value}");
        UpdateYamlConfig(Majestic.VideoSize, value);
    }

    partial void OnSelectedFpsChanged(string value)
    {
        // Custom logic when the property changes
        Log.Debug($"SelectedFps updated to {value}");
        UpdateYamlConfig(Majestic.VideoFps, value);
    }

    partial void OnSelectedCodecChanged(string value)
    {
        // Custom logic when the property changes
        Log.Debug($"SelectedCodec updated to {value}");
        UpdateYamlConfig(Majestic.VideoCodec, value);
    }

    partial void OnSelectedBitrateChanged(string value)
    {
        // Custom logic when the property changes
        Log.Debug($"SelectedBitrate updated to {value}");
        UpdateYamlConfig(Majestic.VideoBitrate, value);
    }

    partial void OnSelectedExposureChanged(string value)
    {
        // Custom logic when the property changes
        Log.Debug($"SelectedExposure updated to {value}");
        UpdateYamlConfig(Majestic.IspExposure, value);
    }

    partial void OnSelectedHueChanged(string value)
    {
        // Custom logic when the property changes
        Log.Debug($"SelectedHue updated to {value}");
        UpdateYamlConfig(Majestic.ImageHue, value);
    }

    partial void OnSelectedContrastChanged(string value)
    {
        // Custom logic when the property changes
        Log.Debug($"SelectedContrast updated to {value}");
        UpdateYamlConfig(Majestic.ImageContrast, value);
    }

    partial void OnSelectedSaturationChanged(string value)
    {
        // Custom logic when the property changes
        Log.Debug($"SelectedSaturation updated to {value}");
        UpdateYamlConfig(Majestic.ImageSaturation, value);
    }

    partial void OnSelectedLuminanceChanged(string value)
    {
        // Custom logic when the property changes
        Log.Debug($"SelectedLuminance updated to {value}");
        UpdateYamlConfig(Majestic.ImageLuminance, value);
    }

    partial void OnSelectedFlipChanged(string value)
    {
        // Custom logic when the property changes
        Log.Debug($"SelectedFlip updated to {value}");
        UpdateYamlConfig(Majestic.ImageFlip, value);
    }

    partial void OnSelectedMirrorChanged(string value)
    {
        // Custom logic when the property changes
        Log.Debug($"SelectedMirror updated to {value}");
        UpdateYamlConfig(Majestic.ImageMirror, value);
    }

    private void OnAppMessage(AppMessage appMessage)
    {
        if (appMessage.CanConnect) CanConnect = appMessage.CanConnect;
        //Log.Debug($"CanConnect {CanConnect.ToString()}");
    }


    private void OnMajesticContentUpdated(MajesticContentUpdatedMessage message)
    {
        var majesticContent = message.Content;
        CanConnect = true;
        ParseYamlConfig(majesticContent);
    }

    partial void OnSelectedFpvEnabledChanged(string value)
    {
        // Custom logic when the property changes
        Log.Debug($"SlectedFpvEnabledChanged updated to {value}");
        UpdateYamlConfig(Majestic.FpvEnabled, value);
    }
    
    partial void OnSelectedFpvNoiseLevelChanged(string value)
    {
        // Custom logic when the property changes
        Log.Debug($"SelectedFpvNoiseLevelChanged updated to {value}");
        UpdateYamlConfig(Majestic.FpvNoiseLevel, value);
    }
    
    partial void OnSelectedFpvRoiQpChanged(string value)
    {
        // Custom logic when the property changes
        Log.Debug($"SelectedFpvRoiQpChanged updated to {value}");
        UpdateYamlConfig(Majestic.FpvRoiQp, value);
    }
    
    partial void OnSelectedFpvRefEnhanceChanged(string value)
    {
        // Custom logic when the property changes
        Log.Debug($"SelectedFpvRefEnhanceChanged updated to {value}");
        UpdateYamlConfig(Majestic.FpvRefEnhance, value);
    }
    
    partial void OnSelectedFpvRefPredChanged(string value)
    {
        // Custom logic when the property changes
        Log.Debug($"SelectedFpvRefPredChanged updated to {value}");
        UpdateYamlConfig(Majestic.FpvRefPred, value);
    }
    
    partial void OnSelectedFpvIntraLineChanged(string value)
    {
        // Custom logic when the property changes
        Log.Debug($"SelectedFpvIntraLineChanged updated to {value}");
        UpdateYamlConfig(Majestic.FpvIntraLine, value);
    }
    
    partial void OnSelectedFpvIntraQpChanged(string value)
    {
        // Custom logic when the property changes
        Log.Debug($"SelectedFpvIntraQpChanged updated to {value}");
        UpdateYamlConfig(Majestic.FpvIntraQp, value);
    }
    
    
    partial void OnFpvRoiRectLeftChanged(ObservableCollection<string> value)
    {
        Log.Debug($"SelectedFpvRoiRectLeftChanged updated to {value[0]}");
        UpdateCombinedValue();
    }
    
    partial void OnFpvRoiRectTopChanged(ObservableCollection<string> value)
    {
        Log.Debug($"SelectedFpvRoiRectTopChanged updated to {value}");
        UpdateCombinedValue();
    }
    
    partial void OnFpvRoiRectHeightChanged(ObservableCollection<string> value)
    {
        Log.Debug($"SelectedFpvRoiRectHeightChanged updated to {value}");
        UpdateCombinedValue();
    }
    
    partial void OnFpvRoiRectWidthChanged(ObservableCollection<string> value)
    {
        Log.Debug($"SelectedFpvRoiRectWidthChanged updated to {value}");
        UpdateCombinedValue();
    }

    private void UpdateCombinedValue()
    {
        CombinedFpvRoiRectValue = $"{FpvRoiRectLeft[0]}x{FpvRoiRectTop[0]}x{FpvRoiRectHeight[0]}x{FpvRoiRectWidth[0]}";
        Log.Debug($"Combined value updated to {CombinedFpvRoiRectValue}");
        UpdateYamlConfig(Majestic.FpvRoiRect, CombinedFpvRoiRectValue);
    }
    
    private void InitializeCollections()
    {
        Resolution = new ObservableCollection<string>
        {
            "1280x720", "1456x816", "1920x1080", "2104x1184", "2208x1248", "2240x1264", "2312x1304",
            "2512x1416", "2560x1440", "2560x1920", "3200x1800", "3840x2160"
        };

        Fps = new ObservableCollection<string>
        {
            "20", "30", "40", "50", "60", "70", "80", "90", "100", "110", "120"
        };

        Codec = new ObservableCollection<string> { "h264", "h265" };
        Bitrate = new ObservableCollection<string>
            { "1024", "2048", "3072", "4096", "5120", "6144", "7168", "8192", "9216" };
        Exposure = new ObservableCollection<string> { "5", "6", "8", "10", "11", "12", "14", "16", "33", "50" };
        Contrast = new ObservableCollection<string>
            { "1", "5", "10", "20", "30", "40", "50", "60", "70", "80", "90", "100" };
        Hue = new ObservableCollection<string>
            { "1", "5", "10", "20", "30", "40", "50", "60", "70", "80", "90", "100" };
        Saturation = new ObservableCollection<string>
            { "1", "5", "10", "20", "30", "40", "50", "60", "70", "80", "90", "100" };
        Luminance = new ObservableCollection<string>
            { "1", "5", "10", "20", "30", "40", "50", "60", "70", "80", "90", "100" };
        Flip = new ObservableCollection<string> { "true", "false" };
        Mirror = new ObservableCollection<string> { "true", "false" };
        
        FpvEnabled = new ObservableCollection<string> { "true", "false" };
        FpvNoiseLevel = new ObservableCollection<string> { "0", "1", "2" };
        
        // Create an ObservableCollection with values from -30 to 30
        FpvRoiQp = new ObservableCollection<string>(Enumerable.Range(-30, 61).Select(i => i.ToString()));
        
        FpvRefEnhance = new ObservableCollection<string>(Enumerable.Range(0, 10).Select(i => i.ToString()));
        
        FpvRefPred = new ObservableCollection<string> { "true", "false" };
        
        FpvIntraLine = new ObservableCollection<string>(Enumerable.Range(0, 10).Select(i => i.ToString()));
        FpvIntraQp = new ObservableCollection<string>{ "true", "false" };


        FpvRoiRectLeft = new ObservableCollection<string> { "" };


    }

    // YAML parsing and updating methods (unchanged)
    private void ParseYamlConfig(string content)
    {
        using var reader = new StringReader(content);
        var yaml = new YamlStream();
        yaml.Load(reader);

        if (yaml.Documents.Count == 0)
        {
            Log.Debug("No content in yaml");

            return; // empty yaml (no content in yaml)
        }

        var root = (YamlMappingNode)yaml.Documents[0].RootNode;
        foreach (var entry in root.Children) ParseYamlNode(entry.Key.ToString(), entry.Value);
    }

    private void ParseYamlNode(string parentKey, YamlNode node)
    {
        if (node is YamlMappingNode mappingNode)
        {
            foreach (var child in mappingNode.Children)
            {
                var childKey = child.Key.ToString();
                ParseYamlNode($"{parentKey}.{childKey}", child.Value);
            }
        }
        else if (node is YamlScalarNode scalarNode)
        {
            var fullKey = parentKey;
            var value = scalarNode.Value;

            if (_yamlConfig.ContainsKey(fullKey))
                _yamlConfig[fullKey] = value;
            else
                _yamlConfig.Add(fullKey, value);

            Log.Debug($"Camera Found {fullKey}: {scalarNode.Value}");

            // Update UI properties based on the keys found
            switch (fullKey)
            {
                case Majestic.VideoSize:
                    if (Resolution?.Contains(value) ?? false)
                    {
                        SelectedResolution = value;
                    }
                    else
                    {
                        Resolution.Add(value);
                        SelectedResolution = value;
                    }

                    break;
                case Majestic.VideoFps:
                    if (Fps?.Contains(value) ?? false)
                    {
                        SelectedFps = value;
                    }
                    else
                    {
                        Fps.Add(value);
                        SelectedFps = value;
                    }

                    break;
                case Majestic.VideoCodec:
                    if (Codec?.Contains(value) ?? false)
                    {
                        SelectedCodec = value;
                    }
                    else
                    {
                        Codec.Add(value);
                        SelectedCodec = value;
                    }

                    break;
                case Majestic.VideoBitrate:
                    if (Bitrate?.Contains(value) ?? false)
                    {
                        SelectedBitrate = value;
                    }
                    else
                    {
                        Bitrate.Add(value);
                        SelectedBitrate = value;
                    }

                    break;
                case Majestic.IspExposure:
                    if (Exposure?.Contains(value) ?? false)
                    {
                        SelectedExposure = value;
                    }
                    else
                    {
                        Exposure.Add(value);
                        SelectedExposure = value;
                    }

                    break;
                case Majestic.ImageContrast:
                    if (Contrast?.Contains(value) ?? false)
                    {
                        SelectedContrast = value;
                    }
                    else
                    {
                        Contrast.Add(value);
                        SelectedContrast = value;
                    }

                    break;
                case Majestic.ImageHue:
                    if (Hue?.Contains(value) ?? false)
                    {
                        SelectedHue = value;
                    }
                    else
                    {
                        Hue.Add(value);
                        SelectedHue = value;
                    }

                    break;
                case Majestic.ImageSaturation:
                    if (Saturation?.Contains(value) ?? false)
                    {
                        SelectedSaturation = value;
                    }
                    else
                    {
                        Saturation.Add(value);
                        SelectedSaturation = value;
                    }

                    break;
                case Majestic.ImageLuminance:
                    if (Luminance?.Contains(value) ?? false)
                    {
                        SelectedLuminance = value;
                    }
                    else
                    {
                        Luminance.Add(value);
                        SelectedLuminance = value;
                    }

                    break;
                case Majestic.ImageFlip:
                    if (Flip?.Contains(value) ?? false)
                    {
                        SelectedFlip = value;
                    }
                    else
                    {
                        Flip.Add(value);
                        SelectedFlip = value;
                    }

                    break;
                case Majestic.ImageMirror:
                    if (Mirror?.Contains(value) ?? false)
                    {
                        SelectedMirror = value;
                    }
                    else
                    {
                        Mirror.Add(value);
                        SelectedMirror = value;
                    }

                    break;
                case Majestic.FpvEnabled:
                    if (Mirror?.Contains(value) ?? false)
                    {
                        SelectedFpvEnabled = value;
                    }
                    else
                    {
                        FpvEnabled.Add(value);
                        SelectedFpvEnabled = value;
                    }

                    break;
                case Majestic.FpvNoiseLevel:
                    if (FpvNoiseLevel?.Contains(value) ?? false)
                    {
                        SelectedFpvNoiseLevel = value;
                    }
                    else
                    {
                        FpvNoiseLevel.Add(value);
                        SelectedFpvNoiseLevel = value;
                    }

                    break;
                case Majestic.FpvRoiQp:
                    if (FpvRoiQp?.Contains(value) ?? false)
                    {
                        SelectedFpvRoiQp = value;
                    }
                    else
                    {
                        FpvRoiQp.Add(value);
                        SelectedFpvRoiQp = value;
                    }

                    break;
                case Majestic.FpvRefEnhance:
                    if (FpvRefEnhance?.Contains(value) ?? false)
                    {
                        SelectedFpvRefEnhance = value;
                    }
                    else
                    {
                        FpvRefEnhance.Add(value);
                        SelectedFpvRefEnhance = value;
                    }

                    break;
                case Majestic.FpvRefPred:
                    if (FpvRefPred?.Contains(value) ?? false)
                    {
                        SelectedFpvRefPred = value;
                    }
                    else
                    {
                        FpvRefPred.Add(value);
                        SelectedFpvRefPred = value;
                    }

                    break;
                case Majestic.FpvIntraLine:
                    if (FpvIntraLine?.Contains(value) ?? false)
                    {
                        SelectedFpvIntraLine = value;
                    }
                    else
                    {
                        FpvIntraLine.Add(value);
                        SelectedFpvIntraLine = value;
                    }

                    break;
                case Majestic.FpvIntraQp:
                    if (FpvIntraQp?.Contains(value) ?? false)
                    {
                        SelectedFpvIntraQp = value;
                    }
                    else
                    {
                        FpvIntraQp.Add(value);
                        SelectedFpvIntraQp = value;
                    }

                    break;
                case Majestic.FpvRoiRect:
                    // Parse the value and assign to individual properties
                    var parts = value.Split('x');
                    if (parts.Length == 4)
                    {
                        // Update ObservableCollection values
                        // Update the first element of each ObservableCollection
                        if (FpvRoiRectLeft.Count > 0) FpvRoiRectLeft[0] = parts[0];
                        if (FpvRoiRectTop.Count > 0) FpvRoiRectTop[0] = parts[1];
                        if (FpvRoiRectHeight.Count > 0) FpvRoiRectHeight[0] = parts[2];
                        if (FpvRoiRectWidth.Count > 0) FpvRoiRectWidth[0] = parts[3];
                    }
                    else
                    {
                        Log.Warning($"Invalid format for FpvRoiRect value: {value}");
                    }

                    break;
                
                default:
                    Log.Debug($"Unknown key: {fullKey}");
                    break;
            }
        }
    }

    private async Task SaveRestartMajesticCommand()
    {
        Log.Debug("Preparing to Save Majestic file.");
        var majesticYamlContent =
            await _sshClientService.DownloadFileAsync(DeviceConfig.Instance, Models.OpenIPC.MajesticFileLoc);

        try
        {
            var yamlStream = new YamlStream();
            using (var reader = new StringReader(majesticYamlContent))
            {
                yamlStream.Load(reader);
            }

            var root = (YamlMappingNode)yamlStream.Documents[0].RootNode;

            foreach (var update in _yamlConfig) UpdateYamlNode(root, update.Key, update.Value);

            string updatedFileContent;
            using (var writer = new StringWriter())
            {
                yamlStream.Save(writer, false);
                updatedFileContent = writer.ToString();
            }

            await _sshClientService.UploadFileStringAsync(DeviceConfig.Instance, Models.OpenIPC.MajesticFileLoc,
                updatedFileContent);
            await _sshClientService.ExecuteCommandAsync(DeviceConfig.Instance, DeviceCommands.MajesticRestartCommand);
            await Task.Delay(5000);


            Log.Debug("YAML file saved and majestic service restarted successfully.");
        }
        catch (Exception ex)
        {
            Log.Debug($"Failed to save YAML file: {ex.Message}");
        }
    }

    public void UpdateYamlConfig(string key, string newValue)
    {
        if (_yamlConfig.ContainsKey(key))
            _yamlConfig[key] = newValue;
        else
            _yamlConfig.Add(key, newValue);
    }

    // Recursively update YAML node based on key path
    private void UpdateYamlNode(YamlMappingNode root, string keyPath, string newValue)
    {
        var keys = keyPath.Split('.');
        var currentNode = root;

        for (var i = 0; i < keys.Length - 1; i++)
        {
            var key = keys[i];
            if (currentNode.Children.ContainsKey(new YamlScalarNode(key)))
                currentNode = (YamlMappingNode)currentNode.Children[new YamlScalarNode(key)];
            else
                Log.Warning($"Key '{key}' not found in YAML.");
        }

        var lastKey = keys[^1];
        if (currentNode.Children.ContainsKey(new YamlScalarNode(lastKey)))
            currentNode.Children[new YamlScalarNode(lastKey)] = new YamlScalarNode(newValue);
        else
            Log.Warning($"Key '{lastKey}' not found in YAML.");
    }
}