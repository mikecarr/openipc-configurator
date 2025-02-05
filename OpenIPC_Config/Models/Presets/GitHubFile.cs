using System;

namespace OpenIPC_Config.ViewModels;

public class GitHubFile
{
    /// <summary>
    /// The name of the file (e.g., "preset-config.yaml").
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The relative path of the file in the repository (e.g., "presets/preset-config.yaml").
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// The URL to directly download the file content.
    /// </summary>
    public string DownloadUrl { get; set; }

    /// <summary>
    /// The size of the file in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// The SHA hash of the file content for integrity checking.
    /// </summary>
    public string Sha { get; set; }

    /// <summary>
    /// The type of the file (e.g., "file" or "directory").
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Indicates when the file was last updated.
    /// </summary>
    public DateTime LastUpdated { get; set; }
}