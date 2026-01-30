using System;

namespace Threa.Client.Components.Shared;

/// <summary>
/// Result returned from the UnloadAmmoContainerModal when user confirms.
/// </summary>
public class UnloadAmmoContainerResult
{
    public Guid ContainerId { get; set; }
    public int RoundsToUnload { get; set; }
    public string? AmmoType { get; set; }
    public string? ContainerName { get; set; }
    public int DurationRounds { get; set; }

    /// <summary>
    /// If true, use 2 AP (no fatigue).
    /// If false, use 1 AP + 1 FAT.
    /// </summary>
    public bool UseTwoApCost { get; set; }

    /// <summary>
    /// Gets the AP cost based on selected option.
    /// </summary>
    public int ApCost => UseTwoApCost ? 2 : 1;

    /// <summary>
    /// Gets the FAT cost based on selected option.
    /// </summary>
    public int FatCost => UseTwoApCost ? 0 : 1;
}
