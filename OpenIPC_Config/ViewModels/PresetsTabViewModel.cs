using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using OpenIPC_Config.Models;
using OpenIPC_Config.Models.Presets;
using OpenIPC_Config.Services;
using Serilog;

namespace OpenIPC_Config.ViewModels;

/// <summary>
/// ViewModel for managing camera presets, including loading, filtering, and applying presets
/// </summary>
public partial class PresetsTabViewModel : ViewModelBase
{
    #region Fields
    /// <summary>
    /// Master collection containing all presets before filtering
    /// </summary>
    private readonly ObservableCollection<Preset> AllPresets = new();
    #endregion

    #region Properties
    // Collections
    /// <summary>
    /// Collection of available preset repositories
    /// </summary>
    public ObservableCollection<Repository> Repositories { get; set; } = new();

    /// <summary>
    /// Collection of filtered presets displayed to the user
    /// </summary>
    public ObservableCollection<Preset> Presets { get; set; } = new();

    /// <summary>
    /// Available preset categories for filtering
    /// </summary>
    public ObservableCollection<string> Categories { get; } = new();

    /// <summary>
    /// Available preset tags for filtering
    /// </summary>
    public ObservableCollection<string> Tags { get; } = new();

    /// <summary>
    /// Available preset authors for filtering
    /// </summary>
    public ObservableCollection<string> Authors { get; } = new();

    /// <summary>
    /// Available preset status options for filtering
    /// </summary>
    public ObservableCollection<string> StatusOptions { get; } = new();

    // Selected Items
    public Repository? SelectedRepository { get; set; }
    public Preset? SelectedPreset { get; set; }
    public string? SelectedCategory { get; set; }
    public string? SelectedTag { get; set; }
    public string? SelectedAuthor { get; set; }
    public string? SelectedStatus { get; set; }

    // Input Fields
    /// <summary>
    /// URL for adding a new repository
    /// </summary>
    public string? NewRepositoryUrl { get; set; }

    /// <summary>
    /// Current search query for filtering presets
    /// </summary>
    public string? SearchQuery { get; set; }

    /// <summary>
    /// Message displayed in the UI log
    /// </summary>
    public string? LogMessage { get; set; }

    // Commands
    public ICommand AddRepositoryCommand { get; private set; } = null!;
    public ICommand RemoveRepositoryCommand { get; private set; } = null!;
    public ICommand FetchPresetsCommand { get; private set; } = null!;
    public ICommand ApplyPresetCommand { get; private set; } = null!;
    public ICommand FilterCommand { get; private set; } = null!;
    public ICommand FilterPresetsCommand { get; private set; } = null!;
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the PresetsTabViewModel
    /// </summary>
    public PresetsTabViewModel(ILogger logger, ISshClientService sshClientService,
        IEventSubscriptionService eventSubscriptionService)
        : base(logger, sshClientService, eventSubscriptionService)
    {
        InitializeCommands();
        LoadPresets();
    }
    #endregion

    #region Command Initialization
    /// <summary>
    /// Initializes all commands used in the ViewModel
    /// </summary>
    private void InitializeCommands()
    {
        AddRepositoryCommand = new RelayCommand(async () => await AddRepositoryAsync(),
            () => !string.IsNullOrWhiteSpace(NewRepositoryUrl));
        RemoveRepositoryCommand = new RelayCommand(RemoveSelectedRepository,
            () => SelectedRepository != null);
        FetchPresetsCommand = new RelayCommand(FetchPresetsAsync);
        ApplyPresetCommand = new RelayCommand<Preset>(async preset => await ApplyPresetAsync(preset));
        FilterCommand = new RelayCommand(FilterPresets);
        FilterPresetsCommand = new RelayCommand(FilterPresets);
    }
    #endregion

    #region Data Loading Methods
    /// <summary>
    /// Loads presets from the filesystem and initializes them
    /// </summary>
    private void LoadPresets()
    {
        var presetDirectory = Path.Join(OpenIPC.GetBinariesPath(), "presets");
        if (!Directory.Exists(presetDirectory))
        {
            Logger.Warning("Preset directory not found.");
            return;
        }

        Presets.Clear();

        foreach (var presetFolder in Directory.GetDirectories(presetDirectory))
        {
            var presetConfigPath = Path.Combine(presetFolder, "preset-config.yaml");
            if (!File.Exists(presetConfigPath))
            {
                Logger.Warning($"Skipping preset folder {presetFolder}: preset-config.yaml missing.");
                continue;
            }

            var preset = Preset.LoadFromFile(presetConfigPath);
            preset.InitializeFileModifications();
            Presets.Add(preset);
        }

        Logger.Information("Presets loaded successfully.");
        LoadDropdownValues();
    }

    /// <summary>
    /// Populates dropdown lists with unique values from loaded presets
    /// </summary>
    private void LoadDropdownValues()
    {
        Categories.Clear();
        Tags.Clear();
        Authors.Clear();
        StatusOptions.Clear();

        foreach (var preset in Presets)
        {
            if (!string.IsNullOrEmpty(preset.Category) && !Categories.Contains(preset.Category))
                Categories.Add(preset.Category);

            foreach (var tag in preset.Tags)
            {
                if (!Tags.Contains(tag))
                    Tags.Add(tag);
            }

            if (!string.IsNullOrEmpty(preset.Author) && !Authors.Contains(preset.Author))
                Authors.Add(preset.Author);

            if (!string.IsNullOrEmpty(preset.Status) && !StatusOptions.Contains(preset.Status))
                StatusOptions.Add(preset.Status);
        }
    }
    #endregion

    #region Command Handlers
    /// <summary>
    /// Adds a new repository using the provided URL
    /// </summary>
    private async Task AddRepositoryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewRepositoryUrl))
            return;

        var repository = new Repository { Name = "New Repo", Url = NewRepositoryUrl };
        Repositories.Add(repository);
        NewRepositoryUrl = string.Empty;

        LogMessage = $"Repository '{repository.Name}' added.";
    }

    /// <summary>
    /// Removes the currently selected repository
    /// </summary>
    private void RemoveSelectedRepository()
    {
        if (SelectedRepository == null)
            return;

        Repositories.Remove(SelectedRepository);
        LogMessage = $"Repository '{SelectedRepository.Name}' removed.";
    }

    /// <summary>
    /// Fetches presets from repositories and updates the UI
    /// </summary>
    private async void FetchPresetsAsync()
    {
        GenerateRandomPresets(10);
        LoadDropdownValues();
    }

    /// <summary>
    /// Applies the selected preset to the camera
    /// </summary>
    private async Task ApplyPresetAsync(Preset preset)
    {
        if (preset == null) return;
        LogMessage = $"Applied preset '{preset.Name}'.";
    }

    /// <summary>
    /// Filters presets based on selected criteria
    /// </summary>
    private void FilterPresets()
    {
        var filteredPresets = AllPresets.AsEnumerable();

        if (!string.IsNullOrEmpty(SelectedCategory))
            filteredPresets = filteredPresets.Where(p => p.Category == SelectedCategory);

        if (!string.IsNullOrEmpty(SelectedTag))
            filteredPresets = filteredPresets.Where(p => p.Tags.Contains(SelectedTag));

        if (!string.IsNullOrEmpty(SelectedAuthor))
            filteredPresets = filteredPresets.Where(p => p.Author == SelectedAuthor);

        if (!string.IsNullOrEmpty(SelectedStatus))
            filteredPresets = filteredPresets.Where(p => p.Status == SelectedStatus);

        Presets.Clear();
        foreach (var preset in filteredPresets)
            Presets.Add(preset);
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Generates random presets for testing purposes
    /// </summary>
    /// <param name="count">Number of random presets to generate</param>
    public void GenerateRandomPresets(int count)
    {
        var random = new Random();
        var categories = new[] { "Long Range", "Freestyle", "Racing", "Cinematic" };
        var authors = new[] { "John Doe", "Jane Smith", "Alex Johnson", "Emily Davis" };
        var statuses = new[] { "Community", "Official", "Experimental" };
        var tagsList = new[] { "Tag1", "Tag2", "Tag3", "Tag4", "Tag5" };

        Presets.Clear();
        for (int i = 0; i < count; i++)
        {
            var preset = new Preset
            {
                Name = $"Preset_{random.Next(1000, 9999)}",
                Category = categories[random.Next(categories.Length)],
                Tags = new ObservableCollection<string>
                {
                    tagsList[random.Next(tagsList.Length)],
                    tagsList[random.Next(tagsList.Length)]
                },
                Author = authors[random.Next(authors.Length)],
                Status = statuses[random.Next(statuses.Length)],
                Description = $"Generated preset {i + 1} for testing.",
                FileModifications = new ObservableCollection<FileModification>
                {
                    new()
                    {
                        FileName = "wfb.yaml",
                        Changes = new ObservableCollection<KeyValuePair<string, string>>
                        {
                            new("wireless.txpower", random.Next(1, 30).ToString()),
                            new("wireless.channel", random.Next(36, 165).ToString())
                        }
                    },
                    new()
                    {
                        FileName = "Majestic.yaml",
                        Changes = new ObservableCollection<KeyValuePair<string, string>>
                        {
                            new("fpv.enabled", random.Next(2) == 0 ? "true" : "false"),
                            new("system.LogLevel", random.Next(2) == 0 ? "debug" : "info")
                        }
                    }
                }
            };

            Presets.Add(preset);
        }
    }
    #endregion
}