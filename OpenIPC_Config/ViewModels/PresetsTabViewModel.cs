using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenIPC_Config.Models;

namespace OpenIPC_Config.ViewModels;

public partial class PresetsTabViewModel : ViewModelBase
{
    public ObservableCollection<string> Categories { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> Authors { get; } = new ObservableCollection<string>();
    public ObservableCollection<Preset> Presets { get; } = new ObservableCollection<Preset>();
    
    [ObservableProperty] private ObservableCollection<Preset> _filteredPresets = new();
    [ObservableProperty] private string _filterText;
    [ObservableProperty] private string _selectedCategory;
    [ObservableProperty] private string _selectedAuthor;

    partial void OnFilterTextChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        ApplyFilters();
    }
    partial void OnSelectedAuthorChanged(string value)
    {
        ApplyFilters();
    }
    
    public IRelayCommand ApplyFiltersCommand { get; }

    public PresetsTabViewModel()
    {
        ApplyFiltersCommand = new RelayCommand(ApplyFilters);


        LoadPresets();
    }

    private void LoadPresets()
    {
        Presets.Add(new Preset(ApplyPreset) { Name = "Preset 1", Author = "OpenIPC", Description = "OpenIPC kicks ass", Category = "FPV" });
        Presets.Add(new Preset(ApplyPreset) { Name = "Preset 2", Author = "Eduardo", Description = "Description 2", Category = "FPV" });
        Presets.Add(new Preset(ApplyPreset) { Name = "Preset 3", Author = "ViperZ28", Description = "Description 3", Category = "FPV" });

        // Populate Categories and Authors as before...
        Categories.Clear();
        foreach (var category in Presets.Select(p => p.Category).Distinct())
        {
            Categories.Add(category);
        }

        Authors.Clear();
        foreach (var author in Presets.Select(p => p.Author).Distinct())
        {
            Authors.Add(author);
        }

        ApplyFilters();
    }

    private void ApplyFilters()
    {
        FilteredPresets.Clear();

        var filtered = Presets.Where(p =>
            (string.IsNullOrEmpty(FilterText) || p.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(SelectedCategory) || p.Category == SelectedCategory) &&
            (string.IsNullOrEmpty(SelectedAuthor) || p.Author == SelectedAuthor)
        );

        foreach (var preset in filtered)
        {
            FilteredPresets.Add(preset);
        }
    }

    private void ApplyPreset(Preset preset)
    {
        // Logic to apply the preset
        Console.WriteLine($"Applying preset: {preset.Name}");
    }
}

public class Preset
{
    public string Name { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    
    // List of commands associated with the preset
    public List<string> Commands { get; set; } = new();
    
    public ICommand ApplyPresetCommand { get; }

    public Preset(Action<Preset> applyPresetAction)
    {
        ApplyPresetCommand = new RelayCommand(() => applyPresetAction(this));
    }
}