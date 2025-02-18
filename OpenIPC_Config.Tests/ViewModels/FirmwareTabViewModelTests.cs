using System.Collections.ObjectModel;
using System.Net;
using System.Reflection;
using Moq;
using Moq.Protected;
using OpenIPC_Config.Services;
using OpenIPC_Config.ViewModels;
using Serilog;

namespace OpenIPC_Config.Tests.ViewModels;

[TestFixture]
public class FirmwareTabViewModelTests
{
    private FirmwareTabViewModel _viewModel;
    private Mock<ILogger> _mockLogger;
    private Mock<ISshClientService> _mockSshClientService;
    private Mock<IEventSubscriptionService> _mockEventSubscriptionService;
    private Mock<HttpMessageHandler> _mockHttpMessageHandler;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger>();
        _mockSshClientService = new Mock<ISshClientService>();
        _mockEventSubscriptionService = new Mock<IEventSubscriptionService>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        _viewModel = new FirmwareTabViewModel(
            _mockLogger.Object,
            _mockSshClientService.Object,
            _mockEventSubscriptionService.Object);
    }

    

    [Test]
    public void LoadDevices_ValidManufacturer_PopulatesDevices()
    {
        // Arrange
        _viewModel.GetType()
            .GetField("_firmwareData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_viewModel, new FirmwareData
            {
                Manufacturers = new ObservableCollection<Manufacturer>
                {
                    new Manufacturer
                    {
                        Name = "TestManufacturer",
                        Devices = new ObservableCollection<Device>
                        {
                            new Device { Name = "TestDevice" }
                        }
                    }
                }
            });

        // Act
        _viewModel.LoadDevices("TestManufacturer");

        // Assert
        Assert.That(_viewModel.Devices, Is.Not.Empty);
        Assert.That(_viewModel.Devices, Does.Contain("TestDevice"));
    }

    [Test]
    public void LoadFirmwares_ValidDevice_PopulatesFirmwares()
    {
        // Arrange
        _viewModel.GetType()
            .GetField("_firmwareData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_viewModel, new FirmwareData
            {
                Manufacturers = new ObservableCollection<Manufacturer>
                {
                    new Manufacturer
                    {
                        Name = "TestManufacturer",
                        Devices = new ObservableCollection<Device>
                        {
                            new Device
                            {
                                Name = "TestDevice",
                                Firmware = new ObservableCollection<string> { "fpv-sensor-nand" }
                            }
                        }
                    }
                }
            });

        _viewModel.SelectedManufacturer = "TestManufacturer";

        // Act
        _viewModel.LoadFirmwares("TestDevice");

        // Assert
        Assert.That(_viewModel.Firmwares, Is.Not.Empty);
        Assert.That(_viewModel.Firmwares, Does.Contain("fpv"));
    }

    [Test]
    public async Task PerformFirmwareUpgradeAsync_ManualFile_PerformsUpgradeFromFile()
    {
        // Arrange
        _viewModel.ManualFirmwareFile = "test.tgz";

        // Act
        await _viewModel.PerformFirmwareUpgradeAsync();

        // Assert
        _mockLogger.Verify(
            l => l.Information(It.Is<string>(s => s.Contains("Performing firmware upgrade using manual file"))),
            Times.Once);
    }

    [Test]
    public async Task PerformFirmwareUpgradeAsync_DropdownsSelected_PerformsUpgradeFromUrl()
    {
        // Arrange
        _viewModel.GetType()
            .GetField("_firmwareData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_viewModel, new FirmwareData
            {
                Manufacturers = new ObservableCollection<Manufacturer>
                {
                    new Manufacturer
                    {
                        Name = "TestManufacturer",
                        Devices = new ObservableCollection<Device>
                        {
                            new Device
                            {
                                Name = "TestDevice",
                                Firmware = new ObservableCollection<string> { "fpv-sensor-nand" }
                            }
                        }
                    }
                }
            });

        _viewModel.SelectedManufacturer = "TestManufacturer";
        _viewModel.SelectedDevice = "TestDevice";
        _viewModel.SelectedFirmware = "fpv";

        // Act
        await _viewModel.PerformFirmwareUpgradeAsync();

        // Assert
        _mockLogger.Verify(
            l => l.Information(It.Is<string>(s => s.Contains("Performing firmware upgrade using selected dropdown options"))),
            Times.Once);
    }

    [Test]
    public void CanExecuteDownloadFirmware_ReturnsFalse_IfInvalidState()
    {
        // Arrange
        _viewModel.ManualFirmwareFile = null;
        _viewModel.SelectedManufacturer = null;
        _viewModel.SelectedDevice = null;
        _viewModel.SelectedFirmware = null;

        // Act
        var canExecute = _viewModel.DownloadFirmwareAsyncCommand.CanExecute(null);

        // Assert
        Assert.IsFalse(canExecute);
    }

    [Test]
    public void CanExecuteDownloadFirmware_ReturnsTrue_IfValidState()
    {
        // Arrange
        _viewModel.ManualFirmwareFile = "test.tgz";

        // Act
        var canExecute = _viewModel.DownloadFirmwareAsyncCommand.CanExecute(null);

        // Assert
        Assert.IsTrue(canExecute);
    }
    
    [Test]
    public async Task FetchFirmwareListAsync_ValidResponse_PopulatesFirmwareData()
    {
        // Arrange: Load mock JSON from file
        var fileDir = GetTestFilePath("mock_firmware_data.json");
        
        var jsonResponse = File.ReadAllText(GetTestFilePath("mock_firmware_data.json"));

        // Setup the mocked HTTP response
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        //var result = await _viewModel.FetchFirmwareListAsync();
        _viewModel.ChipType = "ssc338q";
        await _viewModel.LoadManufacturers();

        Assert.IsNotEmpty(_viewModel.Manufacturers);

        // Assert
        // Assert.IsNotNull(result);
        // Assert.IsNotEmpty(result.Manufacturers);
        // Assert.That(result.Manufacturers.Any(m => m.Name == "runcam"));
        // Assert.That(result.Manufacturers.Any(m => m.Name == "generic"));
    }

    private string GetTestFilePath(string filename)
    {
        var resourcePath = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "TestResources",
            "MockData",
            filename
        );

        return resourcePath;
    }
}