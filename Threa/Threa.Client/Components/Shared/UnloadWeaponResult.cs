using System;

namespace Threa.Client.Components.Shared;

/// <summary>
/// Result returned from the UnloadWeaponModal when user confirms.
/// Used for unloading weapons from inventory (weapon doesn't need to be equipped).
/// Unloading takes half the time of loading.
/// </summary>
public class UnloadWeaponResult
{
    public Guid WeaponItemId { get; set; }
    public int RoundsToUnload { get; set; }
    public string? AmmoType { get; set; }
    public string? WeaponName { get; set; }
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
