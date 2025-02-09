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

public class PresetsTabViewModel : ViewModelBase
{

    // Master collection of all presets
    private readonly ObservableCollection<Preset> AllPresets = new();
    
    
    // Constructor with Dependencies (for DI)
    public PresetsTabViewModel(ILogger logger, ISshClientService sshClientService,
        IEventSubscriptionService eventSubscriptionService)
        : base(logger, sshClientService, eventSubscriptionService)
    {
        InitializeCommands();
        LoadPresets();
    }

    // Bindable Properties
    public ObservableCollection<Repository> Repositories { get; set; } = new();
    public Repository? SelectedRepository { get; set; }

    public ObservableCollection<Preset> Presets { get; set; } = new();
    public Preset? SelectedPreset { get; set; }

    public string? NewRepositoryUrl { get; set; }
    public string? LogMessage { get; set; }

    // Commands
    public ICommand AddRepositoryCommand { get; private set; }
    public ICommand RemoveRepositoryCommand { get; private set; }
    public ICommand FetchPresetsCommand { get; private set; }
    public ICommand ApplyPresetCommand { get; private set; }
    public ICommand FilterCommand { get; private set; }
    
    public ICommand FilterPresetsCommand { get; }
    public string? SearchQuery { get; set; }
    public ObservableCollection<string> Categories { get; } = new();
    public ObservableCollection<string> Tags { get; } = new();
    public ObservableCollection<string> Authors { get; } = new();
    public ObservableCollection<string> StatusOptions { get; } = new();

    public string? SelectedCategory { get; set; }
    public string? SelectedTag { get; set; }
    public string? SelectedAuthor { get; set; }
    public string? SelectedStatus { get; set; }

    // Initialize Commands (to avoid duplication)
    private void InitializeCommands()
    {
        AddRepositoryCommand = new RelayCommand(async () => await AddRepositoryAsync(),
            () => !string.IsNullOrWhiteSpace(NewRepositoryUrl));
        RemoveRepositoryCommand = new RelayCommand(RemoveSelectedRepository, () => SelectedRepository != null);
        // FetchPresetsCommand = new RelayCommand(async () => await FetchPresetsAsync(), () => SelectedRepository != null);
        FetchPresetsCommand = new RelayCommand(FetchPresetsAsync);
        ApplyPresetCommand = new RelayCommand<Preset>(async preset => await ApplyPresetAsync(preset));
        FilterCommand = new RelayCommand(FilterPresets);
    }

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
    
    // Load Presets from Directory
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
            preset.InitializeFileModifications(); // Populate bindable FileModifications
            Presets.Add(preset);
        }

        Logger.Information("Presets loaded successfully.");
    }

    // Command Handlers
    private async Task AddRepositoryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewRepositoryUrl))
            return;

        var repository = new Repository { Name = "New Repo", Url = NewRepositoryUrl };
        Repositories.Add(repository);
        NewRepositoryUrl = string.Empty;

        LogMessage = $"Repository '{repository.Name}' added.";
    }

    private void RemoveSelectedRepository()
    {
        if (SelectedRepository == null)
            return;

        Repositories.Remove(SelectedRepository);
        LogMessage = $"Repository '{SelectedRepository.Name}' removed.";
    }

    private async void FetchPresetsAsync()
    {
        // if (SelectedRepository == null)
        //     return;

        // Simulate fetching presets
        //await Task.Delay(500);
        
        //TODO: Fetch presets
        GenerateRandomPresets(10);
        

        // Update dropdown values
        LoadDropdownValues();

        //LogMessage = $"Fetched presets for repository '{SelectedRepository.Name}'.";
    }

    private async Task ApplyPresetAsync(Preset preset)
    {
        if (preset == null) return;

        // Implement logic to apply preset
        LogMessage = $"Applied preset '{preset.Name}'.";
    }

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

    private void ApplyTagFilter(string tag)
    {
        LogMessage = $"Filtered presets by tag: {tag}.";
    }
    
    // For testing only
    public void GenerateRandomPresets(int count)
    {
        var random = new Random();
        var categories = new[] { "Long Range", "Freestyle", "Racing", "Cinematic" };
        var authors = new[] { "John Doe", "Jane Smith", "Alex Johnson", "Emily Davis" };
        var statuses = new[] { "Community", "Official", "Experimental" };
        var tagsList = new[] { "Tag1", "Tag2", "Tag3", "Tag4", "Tag5" };

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

}