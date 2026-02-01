using System;

namespace Threa.Client.Components.Shared;

/// <summary>
/// Result returned from the ReloadWeaponModal when user confirms.
/// Used for reloading weapons from inventory (weapon doesn't need to be equipped).
/// </summary>
public class ReloadWeaponResult
{
    public Guid WeaponItemId { get; set; }
    public Guid SourceItemId { get; set; }
    public int RoundsToLoad { get; set; }
    public bool IsLooseAmmo { get; set; }
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
