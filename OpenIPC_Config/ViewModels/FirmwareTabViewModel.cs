using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using OpenIPC_Config.Services;
using Serilog;

namespace OpenIPC_Config.ViewModels;

public partial class FirmwareTabViewModel : ViewModelBase
{
    private readonly HttpClient _httpClient;

    private FirmwareData _firmwareData;

    [ObservableProperty] private bool _isManualUpdateEnabled = true;

    [ObservableProperty] private string _selectedDevice;

    [ObservableProperty] private string _selectedFirmware;

    [ObservableProperty] private string _selectedManufacturer;

    [ObservableProperty] private string _manualFirmwareFile;

    public FirmwareTabViewModel(ILogger logger, ISshClientService sshClientService,
        IEventSubscriptionService eventSubscriptionService)
        : base(logger, sshClientService, eventSubscriptionService)
    {
        _httpClient = new HttpClient();

        // Initialize DownloadFirmwareAsyncCommand with CanExecute logic
        DownloadFirmwareAsyncCommand = new RelayCommand(
            async () => await DownloadFirmwareAsync(),
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
            IsManualUpdateEnabled = true;
        });
    }

    public ICommand SelectFirmwareCommand { get; }
    public ICommand PerformFirmwareUpgradeAsyncCommand { get; }

    public ICommand ClearFormCommand { get; }
    public IRelayCommand DownloadFirmwareAsyncCommand { get; }
    
    
    public ObservableCollection<string> Manufacturers { get; set; } = new();
    public ObservableCollection<string> Devices { get; set; } = new();
    public ObservableCollection<string> Firmwares { get; set; } = new();

    

    private async Task DisableDropdown()
    {
    }

    public async Task SelectFirmware(Window window)
    {
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
            ManualFirmwareFile = fileName;
        }
    }

    partial void OnSelectedManufacturerChanged(string value)
    {
        LoadDevices(value);
        UpdateCanExecuteCommands();
        IsManualUpdateEnabled = false;
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

    private async void LoadManufacturers()
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


    private void LoadDevices(string manufacturer)
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


    private void LoadFirmwares(string device)
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


    private async Task DownloadFirmwareAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(ManualFirmwareFile))
            {
                Logger.Information($"Manual firmware file selected: {Path.GetFileName(ManualFirmwareFile)}");
                return;
            }

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

                Logger.Information($"Starting download for firmware: {filename}");

                await DownloadFileAsync(downloadUrl, filename);
            }
            else
            {
                Logger.Warning("Firmware URL could not be constructed. Missing data.");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error downloading firmware");
        }
    }
    
    private async Task PerformFirmwareUpgradeAsync()
{
    try
    {
        // Check if ManualFirmwareFile is populated
        if (!string.IsNullOrEmpty(ManualFirmwareFile))
        {
            Logger.Information("Performing firmware upgrade using manual file.");
            await UpgradeFirmwareFromFileAsync(ManualFirmwareFile);
        }
        else
        {
            // Validate dropdown selections
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

                await UpgradeFirmwareFromUrlAsync(downloadUrl);
            }
            else
            {
                Logger.Warning("Failed to construct firmware URL. Missing or invalid data.");
            }
        }
    }
    catch (Exception ex)
    {
        Logger.Error(ex, "Error performing firmware upgrade");
    }
}

private async Task UpgradeFirmwareFromFileAsync(string filePath)
{
    try
    {
        Logger.Information($"Upgrading firmware from file: {filePath}");

        // Add logic to validate and process the firmware file
        await Task.Delay(1000); // Simulate firmware upgrade process

        Logger.Information("Firmware upgrade from file completed successfully.");
    }
    catch (Exception ex)
    {
        Logger.Error(ex, "Error upgrading firmware from file");
    }
}

private async Task UpgradeFirmwareFromUrlAsync(string url)
{
    try
    {
        Logger.Information($"Downloading firmware from: {url}");

        var filename = Path.GetFileName(url);
        var filePath = Path.Combine(Path.GetTempPath(), filename);

        var response = await _httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fs);

            Logger.Information($"Firmware downloaded to: {filePath}");

            // Proceed with firmware upgrade
            await UpgradeFirmwareFromFileAsync(filePath);
        }
        else
        {
            Logger.Warning($"Failed to download firmware. Status code: {response.StatusCode}");
        }
    }
    catch (Exception ex)
    {
        Logger.Error(ex, "Error upgrading firmware from URL");
    }
}

    private async Task DownloadFileAsync(string url, string filename)
    {
        try
        {
            var filePath = Path.Combine(Path.GetTempPath(), filename);
            Logger.Information($"Downloading file from {url} to {filePath}");

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await response.Content.CopyToAsync(fs);

                Logger.Information($"File successfully downloaded to: {filePath}");
            }
            else
            {
                Logger.Warning($"Failed to download file. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error downloading file from {url}");
        }
    }


    partial void OnManualFirmwareFileChanged(string value)
    {
        UpdateCanExecuteCommands();
    }

    private bool CanExecuteDownloadFirmware()
    {
        return (!string.IsNullOrEmpty(SelectedManufacturer) &&
                !string.IsNullOrEmpty(SelectedDevice) &&
                !string.IsNullOrEmpty(SelectedFirmware)) ||
               !string.IsNullOrEmpty(ManualFirmwareFile);
    }

    private void UpdateCanExecuteCommands()
    {
        (DownloadFirmwareAsyncCommand as RelayCommand)?.NotifyCanExecuteChanged();
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