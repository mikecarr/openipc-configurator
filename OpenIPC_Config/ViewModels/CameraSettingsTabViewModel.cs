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
    private readonly IYamlConfigService _yamlConfigService;
    private readonly IEventSubscriptionService _eventSubscriptionService;

    private readonly Dictionary<string, string> _yamlConfig = new();

    [ObservableProperty] private bool _canConnect;
    
    [ObservableProperty] private ObservableCollection<string> _bitrate;

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
    public ICommand RestartMajesticCommand { get; }

    public CameraSettingsTabViewModel(
        ILogger logger,
        ISshClientService sshClientService,
        IEventSubscriptionService eventSubscriptionService,
        IYamlConfigService yamlConfigService)
        : base(logger, sshClientService, eventSubscriptionService)
    {
        _yamlConfigService = yamlConfigService ?? throw new ArgumentNullException(nameof(yamlConfigService));
        _eventSubscriptionService = eventSubscriptionService ??
                                    throw new ArgumentNullException(nameof(eventSubscriptionService));

        InitializeCollections();

        RestartMajesticCommand = new RelayCommand(async () => await SaveRestartMajesticCommand());

        _eventSubscriptionService.Subscribe<MajesticContentUpdatedEvent, MajesticContentUpdatedMessage>(
            OnMajesticContentUpdated);
        
        _eventSubscriptionService.Subscribe<AppMessageEvent, AppMessage>(
            OnAppMessageEvent);
    }

    private void OnAppMessageEvent(AppMessage appMessage)
    {
        if(appMessage != null)
        {
            // controls buttons
            CanConnect = appMessage.CanConnect;
        }
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
        
        Contrast = new ObservableCollection<string>(Enumerable.Range(1, 100).Select(i => (i * 5).ToString()));
        Hue = new ObservableCollection<string>(Enumerable.Range(1, 100).Select(i => (i * 5).ToString()));
        Saturation = new ObservableCollection<string>(Enumerable.Range(1, 100).Select(i => (i * 5).ToString()));
        Luminance = new ObservableCollection<string>(Enumerable.Range(1, 100).Select(i => (i * 5).ToString()));
        
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

    public void OnMajesticContentUpdated(MajesticContentUpdatedMessage message)
    {
        Logger.Debug("Processing MajesticContentUpdatedMessage.");
        _yamlConfigService.ParseYaml(message.Content, _yamlConfig);
        UpdateViewModelPropertiesFromYaml();
    }

    private void UpdateViewModelPropertiesFromYaml()
    {
        if (_yamlConfig.TryGetValue(Majestic.VideoSize, out var resolution))
        {
            SelectedResolution = resolution;
        }

        if (_yamlConfig.TryGetValue(Majestic.VideoFps, out var fps))
        {
            SelectedFps = fps;
        }

        if (_yamlConfig.TryGetValue(Majestic.VideoCodec, out var codec))
        {
            SelectedCodec = codec;
        }
        
        if (_yamlConfig.TryGetValue(Majestic.VideoBitrate, out var bitrate))
        {
            SelectedBitrate = bitrate;
        }
        
        if (_yamlConfig.TryGetValue(Majestic.IspExposure, out var exposure))
        {
            SelectedExposure = exposure;
        }
        
        if (_yamlConfig.TryGetValue(Majestic.ImageContrast, out var contrast))
        {
            SelectedContrast = contrast;
        }
        
        if (_yamlConfig.TryGetValue(Majestic.ImageHue, out var hue))
        {
            SelectedHue = hue;
        }
        
        if (_yamlConfig.TryGetValue(Majestic.ImageSaturation, out var saturation))
        {
            SelectedSaturation = saturation;
        }
        
        if (_yamlConfig.TryGetValue(Majestic.ImageLuminance, out var luminance))
        {
            SelectedLuminance = luminance;
        }
        
        if (_yamlConfig.TryGetValue(Majestic.ImageFlip, out var flip))
        {
            SelectedFlip = flip;
        }
        
        if (_yamlConfig.TryGetValue(Majestic.ImageMirror, out var mirror))
        {
            SelectedMirror = mirror;
        }
        
        if (_yamlConfig.TryGetValue(Majestic.FpvEnabled, out var fpvEnabled))
        {
            SelectedFpvEnabled = fpvEnabled;
        }
        
        if (_yamlConfig.TryGetValue(Majestic.FpvNoiseLevel, out var fpvNoiseLevel))
        {
            SelectedFpvNoiseLevel = fpvNoiseLevel;
        }
        
        if (_yamlConfig.TryGetValue(Majestic.FpvRoiQp, out var fpvRoiQp))
        {
            SelectedFpvRoiQp = fpvRoiQp;
        }
        
        if (_yamlConfig.TryGetValue(Majestic.FpvRefEnhance, out var fpvRefEnhance))
        {
            SelectedFpvRefEnhance= fpvRefEnhance;
        }
        
        if (_yamlConfig.TryGetValue(Majestic.FpvRefPred, out var fpvRefPred))
        {
            SelectedFpvRefPred= fpvRefPred;
        }
        
        if (_yamlConfig.TryGetValue(Majestic.FpvIntraLine, out var fpvIntraLine))
        {
            SelectedFpvIntraLine= fpvIntraLine;
        }
        
        if (_yamlConfig.TryGetValue(Majestic.FpvIntraQp, out var fpvIntraQp))
        {
            SelectedFpvIntraQp = fpvIntraQp;
        }
        
        if (_yamlConfig.TryGetValue(Majestic.FpvRoiRect, out var fpvRoiRect))
        {
            var parts = fpvRoiRect.Split('x');
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
                Log.Warning($"Invalid format for FpvRoiRect value: {fpvRoiRect}");
            }
            
        }
        
    }

    private async Task SaveRestartMajesticCommand()
    {
        Logger.Debug("Preparing to Save Majestic YAML file.");
        var updatedYamlContent = _yamlConfigService.UpdateYaml(_yamlConfig);

        try
        {
            await SshClientService.UploadFileStringAsync(DeviceConfig.Instance, OpenIPC.MajesticFileLoc,
                updatedYamlContent);
            await SshClientService.ExecuteCommandAsync(DeviceConfig.Instance, DeviceCommands.MajesticRestartCommand);
            Logger.Information("Majestic configuration updated and service restarted successfully.");
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to update Majestic configuration: {ExceptionMessage}", ex.Message);
        }
    }
}