using System;

namespace Threa.Client.Components.Pages.GamePlay;

/// <summary>
/// Information about an ammunition source (magazine or loose ammo) in inventory.
/// </summary>
public class AmmoSourceInfo
{
    /// <summary>
    /// The CharacterItem ID.
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Display name of the ammo source.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Type of ammo (e.g., "9mm", "Arrow").
    /// </summary>
    public string AmmoType { get; set; } = "";

    /// <summary>
    /// Current ammo count (rounds in magazine or stack size for loose ammo).
    /// </summary>
    public int CurrentAmmo { get; set; }

    /// <summary>
    /// Maximum capacity (for magazines) or 0 for loose ammo.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Whether this is loose ammo (vs a magazine).
    /// </summary>
    public bool IsLooseAmmo { get; set; }

    /// <summary>
    /// Damage modifier from this ammo type.
    /// </summary>
    public int DamageModifier { get; set; }

    /// <summary>
    /// Special effect (e.g., "Incendiary", "Hollow-Point").
    /// </summary>
    public string? SpecialEffect { get; set; }
}

/// <summary>
/// Data returned when a reload is completed.
/// </summary>
public class ReloadCompleteData
{
    /// <summary>
    /// Message to log.
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// The weapon item that was reloaded.
    /// </summary>
    public Guid WeaponItemId { get; set; }

    /// <summary>
    /// The ammo source item that was used.
    /// </summary>
    public Guid AmmoSourceItemId { get; set; }

    /// <summary>
    /// Number of rounds loaded.
    /// </summary>
    public int RoundsLoaded { get; set; }

    /// <summary>
    /// New ammo count in the weapon.
    /// </summary>
    public int NewWeaponAmmoCount { get; set; }

    /// <summary>
    /// Remaining ammo in the source after reload.
    /// </summary>
    public int AmmoSourceRemaining { get; set; }

    /// <summary>
    /// Whether the ammo source was loose ammo (vs a magazine).
    /// </summary>
    public bool IsLooseAmmo { get; set; }
}

/// <summary>
/// Data returned when a weapon unload is completed.
/// </summary>
public class UnloadCompleteData
{
    /// <summary>
    /// Message to log.
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// The weapon item that was unloaded.
    /// </summary>
    public Guid WeaponItemId { get; set; }

    /// <summary>
    /// Number of rounds unloaded.
    /// </summary>
    public int RoundsUnloaded { get; set; }

    /// <summary>
    /// Remaining ammo in the weapon after unload.
    /// </summary>
    public int RemainingAmmo { get; set; }

    /// <summary>
    /// Type of ammo that was unloaded.
    /// </summary>
    public string? AmmoType { get; set; }
}
