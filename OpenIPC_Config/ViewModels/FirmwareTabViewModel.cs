using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using OpenIPC_Config.Events;
using OpenIPC_Config.Models;
using OpenIPC_Config.Services;
using Serilog;
using SharpCompress.Archives;

namespace OpenIPC_Config.ViewModels;

public partial class FirmwareTabViewModel : ViewModelBase
{
    private readonly HttpClient _httpClient;
    private readonly SysUpgradeService _sysupgradeService;
    private CancellationTokenSource _cancellationTokenSource;

    [ObservableProperty] private bool _canConnect;
    
    [ObservableProperty] private bool isConnected;
    [ObservableProperty] private bool isFirmwareSelected;
    [ObservableProperty] private bool isManufacturerSelected;
    
    // Derived property to determine if dropdowns should be enabled
    public bool CanUseDropdowns => IsConnected && !IsFirmwareSelected;
    public bool CanUseSelectFirmware => IsConnected && !IsManufacturerSelected;

    private FirmwareData _firmwareData;

    [ObservableProperty] private bool _canDownloadFirmware;

    [ObservableProperty] private bool _isManualUpdateEnabled = true;

    [ObservableProperty] private string _selectedDevice;

    [ObservableProperty] private string _selectedFirmware;

    [ObservableProperty] private string _selectedManufacturer;

    [ObservableProperty] private string _manualFirmwareFile;

    // [ObservableProperty] private string _progressLog;

    [ObservableProperty] private int _progressValue;

    public FirmwareTabViewModel(ILogger logger, ISshClientService sshClientService,
        IEventSubscriptionService eventSubscriptionService)
        : base(logger, sshClientService, eventSubscriptionService)
    {
        _httpClient = new HttpClient();

        SubscribeToEvents();

        _sysupgradeService = new SysUpgradeService(sshClientService, logger);

        CanConnect = false; // Default to disabled
        IsConnected = false;
        IsFirmwareSelected = false;
        IsManufacturerSelected = false;
        
        // Initialize DownloadFirmwareAsyncCommand with CanExecute logic
        DownloadFirmwareAsyncCommand = new RelayCommand(
            async () => await PerformFirmwareUpgradeAsync(),
            CanExecuteDownloadFirmware);

        LoadManufacturers();

        PerformFirmwareUpgradeAsyncCommand = new RelayCommand(
            async () => await PerformFirmwareUpgradeAsync()
        );

        SelectFirmwareCommand = new RelayCommand<Window>(async window => await SelectFirmware(window));
        ClearFormCommand = new RelayCommand(() =>
        {
            SelectedManufacturer = string.Empty;
            SelectedDevice = string.Empty;
            SelectedFirmware = string.Empty;
            IsFirmwareSelected = false;
            IsManufacturerSelected = false;
            IsManualUpdateEnabled = true;
            UpdateCanExecuteCommands();
        });
    }

    public ICommand SelectFirmwareCommand { get; }
    public ICommand PerformFirmwareUpgradeAsyncCommand { get; }

    public ICommand ClearFormCommand { get; }
    public IRelayCommand DownloadFirmwareAsyncCommand { get; }


    public ObservableCollection<string> Manufacturers { get; set; } = new();
    public ObservableCollection<string> Devices { get; set; } = new();
    public ObservableCollection<string> Firmwares { get; set; } = new();


    private void SubscribeToEvents()
    {
        EventSubscriptionService.Subscribe<AppMessageEvent, AppMessage>(OnAppMessage);
    }

    private void OnAppMessage(AppMessage message)
    {
        CanConnect = message.CanConnect;

        if (CanConnect)
        {
            IsConnected = true; // Mark as connected
        }
        else
        {
            IsConnected = false; // Reset state if disconnected
            IsFirmwareSelected = false; // Reset firmware selection state
            IsManufacturerSelected = false; // Reset manufacturer selection state
        }

        UpdateCanExecuteCommands(); // Notify UI to update based on new state
    }

    public async Task SelectFirmware(Window window)
    {
        IsFirmwareSelected = true; // Disable dropdowns if Select Firmware is clicked
        IsManufacturerSelected = false; // Reset manufacturer selection
        
        UpdateCanExecuteCommands();

        var dialog = new OpenFileDialog
        {
            Title = "Select a File",
            Filters = new List<FileDialogFilter>
            {
                new() { Name = "Compressed", Extensions = { "tgz" } },
                new() { Name = "Bin Files", Extensions = { "bin" } },
                new() { Name = "All Files", Extensions = { "*" } }
            }
        };

        // Show the dialog and get the selected file(s)
        var result = await dialog.ShowAsync(window);

        if (result != null && result.Length > 0)
        {
            var selectedFile = result[0]; // Get the first selected file
            var fileName = Path.GetFileName(selectedFile); // Extract just the filename
            Console.WriteLine($"Selected File: {selectedFile}");
            ManualFirmwareFile = selectedFile;
        }
    }

    partial void OnSelectedManufacturerChanged(string value)
    {
        LoadDevices(value);
        IsManufacturerSelected = !string.IsNullOrEmpty(value); // Disable Select Firmware if manufacturer is selected
        IsFirmwareSelected = false; // Reset if the manufacturer is selected
        UpdateCanExecuteCommands();
        //IsManualUpdateEnabled = false;
    }
    

    partial void OnSelectedDeviceChanged(string value)
    {
        LoadFirmwares(value);
        UpdateCanExecuteCommands();
    }

    partial void OnSelectedFirmwareChanged(string value)
    {
        UpdateCanExecuteCommands();
    }

    public async void LoadManufacturers()
    {
        try
        {
            Logger.Information("Loading firmware list...");
            var data = await FetchFirmwareListAsync();

            if (data?.Manufacturers != null && data.Manufacturers.Any())
            {
                Manufacturers.Clear();

                foreach (var manufacturer in data.Manufacturers)
                {
                    // Check if the manufacturer has valid firmware types
                    var hasValidFirmwareType = manufacturer.Devices.Any(device =>
                        device.Firmware.Any(firmware => firmware.Contains("fpv") || firmware.Contains("rubyfpv")));

                    if (hasValidFirmwareType) Manufacturers.Add(manufacturer.Name);
                }

                if (!Manufacturers.Any()) Logger.Warning("No manufacturers with valid firmware types found.");
            }
            else
            {
                Logger.Warning("No manufacturers found in the fetched firmware data.");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error loading manufacturers.");
        }
    }


    public void LoadDevices(string manufacturer)
    {
        Devices.Clear();

        if (string.IsNullOrEmpty(manufacturer))
        {
            Logger.Warning("Manufacturer is null or empty. Devices cannot be loaded.");
            return;
        }

        var manufacturerData = _firmwareData?.Manufacturers.FirstOrDefault(m => m.Name == manufacturer);

        if (manufacturerData == null || !manufacturerData.Devices.Any())
        {
            Logger.Warning($"No devices found for manufacturer: {manufacturer}");
            return;
        }

        foreach (var device in manufacturerData.Devices) Devices.Add(device.Name);

        Logger.Information($"Loaded {Devices.Count} devices for manufacturer: {manufacturer}");
    }


    public void LoadFirmwares(string device)
    {
        Firmwares.Clear();

        if (string.IsNullOrEmpty(device))
        {
            Logger.Warning("Device is null or empty. Firmwares cannot be loaded.");
            return;
        }

        var deviceData = _firmwareData?.Manufacturers
            .FirstOrDefault(m => m.Name == SelectedManufacturer)?.Devices
            .FirstOrDefault(d => d.Name == device);

        if (deviceData == null || !deviceData.Firmware.Any())
        {
            Logger.Warning($"No firmware found for device: {device}");
            return;
        }

        foreach (var firmware in deviceData.Firmware)
        {
            // Extract the firmware type (e.g., 'fpv' or 'rubyfpv') from the firmware identifier
            var components = firmware.Split('-');
            if (components.Length > 0)
            {
                var firmwareType = components[0]; // The first component is the firmware type
                if (!Firmwares.Contains(firmwareType)) Firmwares.Add(firmwareType); // Add only the firmware type
            }
        }

        Logger.Information($"Loaded {Firmwares.Count} firmware types for device: {device}");
    }


    private async Task<FirmwareData> FetchFirmwareListAsync()
    {
        try
        {
            var url = "https://api.github.com/repos/OpenIPC/builder/releases/latest";
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; OpenIPC-Config/1.0)");
            var response = await _httpClient.GetStringAsync(url);

            var releaseData = JObject.Parse(response);
            var assets = releaseData["assets"];
            var filenames =
                assets?.Select(asset => asset["name"]?.ToString()).Where(name => !string.IsNullOrEmpty(name)) ??
                Enumerable.Empty<string>();

            Logger.Information($"Fetched {filenames.Count()} firmware files.");

            var firmwareData = new FirmwareData { Manufacturers = new ObservableCollection<Manufacturer>() };

            foreach (var filename in filenames)
            {
                var match = Regex.Match(filename,
                    @"^(?<sensor>[^_]+)_(?<firmwareType>[^_]+)_(?<manufacturer>[^-]+)-(?<device>.+?)-(?<memoryType>nand|nor)");
                if (match.Success)
                {
                    var sensor = match.Groups["sensor"].Value;
                    var firmwareType = match.Groups["firmwareType"].Value;
                    var manufacturerName = match.Groups["manufacturer"].Value;
                    var deviceName = match.Groups["device"].Value;
                    var memoryType = match.Groups["memoryType"].Value;

                    Debug.WriteLine(
                        $"Parsed file: Sensor={sensor}, FirmwareType={firmwareType}, Manufacturer={manufacturerName}, Device={deviceName}, MemoryType={memoryType}");

                    var manufacturer = firmwareData.Manufacturers.FirstOrDefault(m => m.Name == manufacturerName);
                    if (manufacturer == null)
                    {
                        manufacturer = new Manufacturer
                        {
                            Name = manufacturerName,
                            Devices = new ObservableCollection<Device>()
                        };
                        firmwareData.Manufacturers.Add(manufacturer);
                    }

                    var device = manufacturer.Devices.FirstOrDefault(d => d.Name == deviceName);
                    if (device == null)
                    {
                        device = new Device
                        {
                            Name = deviceName,
                            Firmware = new ObservableCollection<string>()
                        };
                        manufacturer.Devices.Add(device);
                    }

                    var firmwareIdentifier = $"{firmwareType}-{sensor}-{memoryType}";
                    if (!device.Firmware.Contains(firmwareIdentifier)) device.Firmware.Add(firmwareIdentifier);
                }
                else
                {
                    Debug.WriteLine($"Filename '{filename}' does not match the expected pattern.");
                }
            }

            _firmwareData = firmwareData;
            return firmwareData;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error fetching firmware list.");
            return null;
        }
    }


    private async Task<string> DownloadFirmwareAsync(string url = null, string filename = null)
    {
        try
        {
            //ProgressLog += "Downloading firmware file...\n";

            var filePath = Path.Combine(Path.GetTempPath(), filename);
            Logger.Information($"Downloading firmware from {url} to {filePath}");

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await response.Content.CopyToAsync(fs);

                Logger.Information($"Firmware successfully downloaded to: {filePath}");
                return filePath;
            }
            else
            {
                Logger.Warning($"Failed to download firmware. Status code: {response.StatusCode}");
                ProgressValue = 100;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error downloading firmware");
            return null;
        }
    }


    private string DecompressTgzToTar(string tgzFilePath)
    {
        try
        {
            //ProgressLog += "Decompressing tgz to tar file...\n";
            var tarFilePath = Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(tgzFilePath)}.tar");

            using (var fileStream = File.OpenRead(tgzFilePath))
            using (var gzipStream =
                   new System.IO.Compression.GZipStream(fileStream, System.IO.Compression.CompressionMode.Decompress))
            using (var tarFileStream = File.Create(tarFilePath))
            {
                gzipStream.CopyTo(tarFileStream);
            }

            Logger.Information($"Decompressed .tgz to .tar: {tarFilePath}");
            return tarFilePath;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error decompressing .tgz file");
            throw;
        }
    }

    private string UncompressFirmware(string tgzFilePath)
    {
        try
        {
            //ProgressLog += "Decompressing firmware...\n";
            // Step 1: Decompress the `.tgz` to a `.tar` file
            var tarFilePath = DecompressTgzToTar(tgzFilePath);

            ProgressValue = 4;
            // Step 2: Define output directory for extraction
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(tgzFilePath));
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);

            Directory.CreateDirectory(tempDir);

            ProgressValue = 8;
            // Step 3: Extract the `.tar` file using SharpCompress
            using (var archive = SharpCompress.Archives.Tar.TarArchive.Open(tarFilePath))
            {
                foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                {
                    var destinationPath = Path.Combine(tempDir, entry.Key);
                    var directoryPath = Path.GetDirectoryName(destinationPath);

                    if (!Directory.Exists(directoryPath))
                        Directory.CreateDirectory(directoryPath);

                    using (var entryStream = entry.OpenEntryStream())
                    using (var fileStream = File.Create(destinationPath))
                    {
                        entryStream.CopyTo(fileStream);
                    }
                }
            }

            Logger.Information($"Firmware extracted to {tempDir}");
            return tempDir;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error uncompressing firmware file");
            throw;
        }
    }


    private void ValidateFirmwareFiles(string extractedDir)
    {
        try
        {
            //ProgressLog += "Validating firmware files...\n";
            // Fetch all files in the directory
            var allFiles = Directory.GetFiles(extractedDir);

            // Find all `.md5sum` files
            var md5Files = allFiles.Where(file => file.EndsWith(".md5sum")).ToList();

            if (!md5Files.Any())
                throw new InvalidOperationException("No MD5 checksum files found in the extracted directory.");

            // Validate each `.md5sum` file
            foreach (var md5File in md5Files)
            {
                // Get the base file name (without the `.md5sum` extension)
                var baseFileName = Path.GetFileNameWithoutExtension(md5File);

                // Locate the corresponding data file
                var dataFile = allFiles.FirstOrDefault(file => Path.GetFileName(file) == baseFileName);
                if (dataFile == null)
                    throw new FileNotFoundException(
                        $"Data file '{baseFileName}' referenced by '{md5File}' is missing.");

                // Validate MD5 checksum
                ValidateMd5Checksum(md5File, dataFile);
            }

            Logger.Information("Firmware files validated successfully.");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error validating firmware files");
            throw;
        }
    }

    private void ValidateMd5Checksum(string md5FilePath, string dataFilePath)
    {
        try
        {
            // Read the line from the `.md5sum` file
            var md5Line = File.ReadAllText(md5FilePath).Trim();

            // Split the line into the expected MD5 checksum and the filename
            var parts = md5Line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                throw new InvalidOperationException($"Invalid format in MD5 file: {md5FilePath}");
            }

            var expectedMd5 = parts[0]; // The first part is the MD5 checksum
            var expectedFilename = parts[1]; // The second part is the filename

            // Ensure the expected filename matches the actual file
            if (Path.GetFileName(dataFilePath) != expectedFilename)
            {
                throw new InvalidOperationException(
                    $"Filename mismatch: expected '{expectedFilename}', found '{Path.GetFileName(dataFilePath)}'");
            }

            // Compute the actual MD5 checksum of the data file
            using var md5 = System.Security.Cryptography.MD5.Create();
            using var stream = File.OpenRead(dataFilePath);
            var actualMd5 = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();

            // Compare the checksums
            if (expectedMd5 != actualMd5)
            {
                throw new InvalidOperationException(
                    $"MD5 mismatch for file: {Path.GetFileName(dataFilePath)}. Expected: {expectedMd5}, Actual: {actualMd5}");
            }

            Logger.Information($"File '{Path.GetFileName(dataFilePath)}' passed MD5 validation.");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error validating MD5 checksum for file: {dataFilePath}");
            throw;
        }
    }


    public async Task PerformFirmwareUpgradeAsync()
    {
        try
        {
            ProgressValue = 0;

            string firmwareFilePath;

            if (!string.IsNullOrEmpty(ManualFirmwareFile))
            {
                Logger.Information("Performing firmware upgrade using manual file.");
                firmwareFilePath = ManualFirmwareFile;
            }
            else
            {
                if (string.IsNullOrEmpty(SelectedManufacturer) ||
                    string.IsNullOrEmpty(SelectedDevice) ||
                    string.IsNullOrEmpty(SelectedFirmware))
                {
                    Logger.Warning("Cannot perform firmware upgrade. Missing dropdown selections.");
                    return;
                }

                Logger.Information("Performing firmware upgrade using selected dropdown options.");

                // Construct the firmware file URL
                var manufacturer = _firmwareData?.Manufacturers
                    .FirstOrDefault(m => m.Name == SelectedManufacturer);

                var device = manufacturer?.Devices
                    .FirstOrDefault(d => d.Name == SelectedDevice);

                var firmwareIdentifier = device?.Firmware
                    .FirstOrDefault(f => f.StartsWith(SelectedFirmware));

                if (!string.IsNullOrEmpty(manufacturer?.Name) &&
                    !string.IsNullOrEmpty(device?.Name) &&
                    !string.IsNullOrEmpty(firmwareIdentifier))
                {
                    var components = firmwareIdentifier.Split('-');
                    var firmwareType = components[0];
                    var sensor = components[1];
                    var memoryType = components[2];

                    var filename = $"{sensor}_{firmwareType}_{SelectedManufacturer}-{device.Name}-{memoryType}.tgz";
                    var downloadUrl = $"https://github.com/OpenIPC/builder/releases/download/latest/{filename}";

                    firmwareFilePath = await DownloadFirmwareAsync(downloadUrl, filename);
                    
                }
                else
                {
                    Logger.Warning("Failed to construct firmware URL. Missing or invalid data.");
                    return;
                }
            }

            if (!string.IsNullOrEmpty(firmwareFilePath))
            {
                // Proceed with common upgrade process
                await UpgradeFirmwareFromFileAsync(firmwareFilePath);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error performing firmware upgrade");
        }
    }

    public void CancelSysupgrade()
    {
        _cancellationTokenSource?.Cancel();
    }

    private async Task UpgradeFirmwareFromFileAsync(string firmwareFilePath)
    {
        try
        {
            Logger.Information($"Upgrading firmware from file: {firmwareFilePath}");

            ProgressValue = 5;
            Logger.Debug("UncompressFirmware ProgressValue: " + ProgressValue);
            // Step 1: Uncompress the firmware file
            var extractedDir = UncompressFirmware(firmwareFilePath);

            ProgressValue = 10;
            Logger.Debug("ValidateFirmwareFiles ProgressValue: " + ProgressValue);
            // Step 2: Validate firmware files dynamically
            ValidateFirmwareFiles(extractedDir);


            // Step 3: Extract kernel and rootfs paths
            ProgressValue = 20;
            Logger.Debug("Before PerformSysupgradeAsync ProgressValue: " + ProgressValue);
            var kernelFile = Directory.GetFiles(extractedDir)
                .FirstOrDefault(f => f.Contains("uImage") && !f.EndsWith(".md5sum"));

            var rootfsFile = Directory.GetFiles(extractedDir)
                .FirstOrDefault(f => f.Contains("rootfs") && !f.EndsWith(".md5sum"));

            if (kernelFile == null || rootfsFile == null)
                throw new InvalidOperationException("Kernel or RootFS file is missing after validation.");

            // Step 4: Configure device and perform sysupgrade
            var sysupgradeService = new SysUpgradeService(SshClientService, Logger);

            await Task.Run(async () =>
            {
                await sysupgradeService.PerformSysupgradeAsync(DeviceConfig.Instance, kernelFile, rootfsFile,
                    progress =>
                    {
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            //ProgressLog += progress + Environment.NewLine;
                        
                            switch (progress)
                            {
                                case var s when s.Contains("Update kernel from"):
                                    ProgressValue = 30;
                                    Logger.Debug("Update kernel from ProgressValue: " + ProgressValue);
                                    break;
                                case var s when s.Contains("Kernel updated to"):
                                    ProgressValue = 50;
                                    Logger.Debug("Kernel updated to ProgressValue: " + ProgressValue);
                                    break;
                                case var s when s.Contains("Update rootfs from"):
                                    ProgressValue = 60;
                                    Logger.Debug("Update rootfs from ProgressValue: " + ProgressValue);
                                    break;
                                case var s when s.Contains("Root filesystem uploaded successfully"):
                                    ProgressValue = 70;
                                    Logger.Debug("Root filesystem uploaded successfully ProgressValue: " + ProgressValue);
                                    break;
                                case var s when s.Contains("Erase overlay partition"):
                                    ProgressValue = 90;
                                    Logger.Debug("Erase overlay partition ProgressValue: " + ProgressValue);
                                    break;
                            }
                            
                            Logger.Debug(progress);
                        });
                    },
                    CancellationToken.None);


                // Ensure the final progress update is on the UI thread
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ProgressValue = 100;
                    Logger.Information("Firmware upgrade completed successfully.");
                });
            });

            ProgressValue = 100;
            Logger.Information("Firmware upgrade completed successfully.");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error upgrading firmware from file");
        }
    }


    partial void OnManualFirmwareFileChanged(string value)
    {
        UpdateCanExecuteCommands();
    }

    private bool CanExecuteDownloadFirmware()
    {
        return CanConnect &&
               (!string.IsNullOrEmpty(SelectedManufacturer) &&
                !string.IsNullOrEmpty(SelectedDevice) &&
                !string.IsNullOrEmpty(SelectedFirmware)) ||
               !string.IsNullOrEmpty(ManualFirmwareFile);
    }

    // private void UpdateCanExecuteCommands()
    // {
    //     CanDownloadFirmware = CanExecuteDownloadFirmware(); // Update the `CanDownloadFirmware` property.
    //
    //     // Notify the commands to re-evaluate their CanExecute logic.
    //     (DownloadFirmwareAsyncCommand as RelayCommand)?.NotifyCanExecuteChanged();
    //     (PerformFirmwareUpgradeAsyncCommand as RelayCommand)?.NotifyCanExecuteChanged();
    // }
    private void UpdateCanExecuteCommands()
    {
        CanDownloadFirmware = CanExecuteDownloadFirmware();

        // Notify UI of property changes
        OnPropertyChanged(nameof(CanUseDropdowns));
        OnPropertyChanged(nameof(CanUseSelectFirmware));

        (DownloadFirmwareAsyncCommand as RelayCommand)?.NotifyCanExecuteChanged();
        (PerformFirmwareUpgradeAsyncCommand as RelayCommand)?.NotifyCanExecuteChanged();
    }
}

public class FirmwareData
{
    public ObservableCollection<Manufacturer> Manufacturers { get; set; }
}

public class Manufacturer
{
    public string Name { get; set; }
    public ObservableCollection<Device> Devices { get; set; }
}

public class Device
{
    public string Name { get; set; }
    public ObservableCollection<string> Firmware { get; set; }
}