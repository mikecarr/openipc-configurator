using System;

namespace OpenIPC_Config.Models.Presets;

public class Repository
{
    /// <summary>
    /// The name of the repository (e.g., "OpenIPC Presets").
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The URL of the repository (e.g., "https://github.com/openipc/presets").
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Indicates whether the repository is active (used for fetching presets).
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp of when the repository was added.
    /// </summary>
    public DateTime AddedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional description of the repository.
    /// </summary>
    public string? Description { get; set; }    
}