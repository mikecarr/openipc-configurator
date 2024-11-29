using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OpenIPC_Config.ViewModels;

public partial class PreferencesTabViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isDarkMode;
    [ObservableProperty] private string _currentVersion = "1.0.0"; // Example version
    [ObservableProperty] private string _latestVersion;

    
    public ObservableCollection<FileDetail> FileDetails { get; }

    public ICommand ToggleDarkModeCommand { get; }
    public ICommand GetLatestUpdatesCommand { get; }
    public ICommand CheckForNewVersionCommand { get; }

    public PreferencesTabViewModel()
    {
        FileDetails = new ObservableCollection<FileDetail>();

        ToggleDarkModeCommand = new RelayCommand(ToggleDarkMode);
        GetLatestUpdatesCommand = new RelayCommand(async () => await GetLatestUpdates());
        CheckForNewVersionCommand = new RelayCommand(async () => await CheckForNewVersion());

        LoadFileDetails(); // Initialize the file list
    }

    private void ToggleDarkMode()
    {
        // Implement dark mode logic
        IsDarkMode = !IsDarkMode;
    }

    private async Task GetLatestUpdates()
    {
        // Logic to download latest updates
        await Task.Delay(1000); // Simulate network call
        // Notify user of success
    }

    private async Task CheckForNewVersion()
    {
        // Logic to check for the latest version
        await Task.Delay(1000); // Simulate network call
        LatestVersion = "1.1.0"; // Example response
    }

    
    private void LoadFileDetails()
    {
        var files = Directory.GetFiles("binaries"); // Adjust path as necessary
        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            FileDetails.Add(new FileDetail
            {
                FileName = fileInfo.Name,
                DateCreated = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                Checksum = CalculateChecksum(file)
            });
        }
    }

    private string CalculateChecksum(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha256.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}

public class FileDetail
{
    public string FileName { get; set; }
    public string DateCreated { get; set; }
    public string Checksum { get; set; }
}

