using Moq;
using OpenIPC_Config.Events;
using OpenIPC_Config.Models;
using OpenIPC_Config.Services;
using OpenIPC_Config.ViewModels;
using Serilog;

namespace OpenIPC_Config.Tests.ViewModels;

[TestFixture]
public class CameraSettingsTabViewModelTests : ViewModelTestBase
{
    
    private CameraSettingsTabViewModel _viewModel;
    private Mock<ILogger> _mockLogger;
    private Mock<ISshClientService> _mockSshClientService;
    private Mock<IEventSubscriptionService> _mockEventSubscriptionService;
    private Mock<IYamlConfigService> _mockYamlConfigService;
    private Mock<EventAggregator> _mockEventAggregatorMock;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger>();
        _mockSshClientService = new Mock<ISshClientService>();
        _mockEventSubscriptionService = new Mock<IEventSubscriptionService>();
        _mockYamlConfigService = new Mock<IYamlConfigService>();
        _mockEventAggregatorMock = new Mock<EventAggregator>();

        _viewModel = new CameraSettingsTabViewModel(
            _mockLogger.Object,
            _mockSshClientService.Object,
            _mockEventSubscriptionService.Object,
            _mockYamlConfigService.Object

        );
    }
    
    [Test]
    public void Constructor_InitializesCollections()
    {
        // Assert
        Assert.IsNotNull(_viewModel.Resolution);
        Assert.Contains("1920x1080", _viewModel.Resolution);

        Assert.IsNotNull(_viewModel.Fps);
        Assert.Contains("30", _viewModel.Fps);

        Assert.IsNotNull(_viewModel.Codec);
        Assert.Contains("h264", _viewModel.Codec);

        Assert.IsNotNull(_viewModel.Bitrate);
        Assert.Contains("4096", _viewModel.Bitrate);
    }
    
    [Test]
    public void OnMajesticContentUpdated_ParsesYamlAndUpdatesProperties()
    {
        // Arrange
        var yamlContent = "video_size: 1920x1080\nvideo_fps: 30, video_codec: h265, video_bitrate: 4096\nfpv_enabled: true, fpv_roi_rect: 10x20x30x40";
        var message = new MajesticContentUpdatedMessage(yamlContent);

        _mockYamlConfigService.Setup(service => service.ParseYaml(yamlContent, It.IsAny<Dictionary<string, string>>()))
            .Callback<string, Dictionary<string, string>>((_, config) =>
            {
                config[Majestic.VideoSize] = "1920x1080";
                config[Majestic.VideoFps] = "30";
                config[Majestic.VideoCodec] = "h265";
                config[Majestic.VideoBitrate] = "4096";
                
                
                config[Majestic.FpvEnabled] = "true";
                // FPV roiRect - (Left x Top x H x W )
                config[Majestic.FpvRoiRect] = "10x20x30x40";
            });

        // Act
        _viewModel.OnMajesticContentUpdated(message);

        // Assert
        Assert.AreEqual("1920x1080", _viewModel.SelectedResolution);
        Assert.AreEqual("30", _viewModel.SelectedFps);
        Assert.AreEqual("h265", _viewModel.SelectedCodec);
        Assert.AreEqual("4096", _viewModel.SelectedBitrate);
        
        Assert.AreEqual("true", _viewModel.SelectedFpvEnabled);
        
        
        // Assert.AreEqual("10", _viewModel.FpvRoiRectLeft.ToString());
        // Assert.AreEqual("20", _viewModel.FpvRoiRectTop.ToString());
        // Assert.AreEqual("30", _viewModel.FpvRoiRectHeight.ToString());
        // Assert.AreEqual("40", _viewModel.FpvRoiRectWidth.ToString());
    }
}