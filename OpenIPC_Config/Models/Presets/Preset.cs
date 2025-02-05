using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class Preset
{
    /* Name of preset */
    
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public ObservableCollection<string> Tags { get; set; } = new();
    public string Author { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ObservableCollection<FileModification> FileModifications { get; set; } = new();
    
    
    /* States can be used for Official, Community, Untested, etc. */
    
    public string State { get; set; }
    public string? Sensor { get; set; }
    public Dictionary<string, Dictionary<string, string>> Files { get; set; } = new();

    [YamlIgnore] public string FolderPath { get; set; }


    // // Bindable collection for UI
    // [YamlIgnore]
    // public ObservableCollection<FileModification> FileModifications { get; set; } = new();

    public string FileModificationsSummary => string.Join(", ", 
        FileModifications.Select(fm => $"{fm.FileName}: {string.Join(", ", fm.Changes.Select(c => $"{c.Key} = {c.Value}"))}"));

    /// <summary>
    /// Load a Preset object from a YAML file.
    /// </summary>
    /// <param name="configPath">Path to the preset-config.yaml file.</param>
    /// <returns>Loaded Preset object.</returns>
    public static Preset LoadFromFile(string configPath)
    {
        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException($"Preset configuration file not found: {configPath}");
        }

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var yamlContent = File.ReadAllText(configPath);
        var preset = deserializer.Deserialize<Preset>(yamlContent);
        preset.FolderPath = Path.GetDirectoryName(configPath);
        preset.InitializeFileModifications(); // Populate FileModifications
        return preset;
    }

    /// <summary>
    /// Convert Files dictionary into a bindable collection of FileModifications.
    /// </summary>
    public void InitializeFileModifications()
    {
        FileModifications.Clear();
        foreach (var file in Files)
        {
            FileModifications.Add(new FileModification
            {
                FileName = file.Key,
                Changes = new ObservableCollection<KeyValuePair<string, string>>(file.Value)
            });
        }
    }
}

public class FileModification
{
    public string FileName { get; set; }
    public ObservableCollection<KeyValuePair<string, string>> Changes { get; set; }
}
