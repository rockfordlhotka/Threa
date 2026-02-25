using System;
using GameMechanics.Combat;

namespace Threa.Client.Components.Pages.GamePlay;

/// <summary>
/// Information about an equipped ranged weapon for the UI.
/// </summary>
public class RangedWeaponInfo
{
    /// <summary>
    /// The CharacterItem ID.
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// The ItemTemplate ID of the weapon. Used to fetch on-hit effects.
    /// </summary>
    public int TemplateId { get; set; }

    /// <summary>
    /// Weapon name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// The ranged skill name for this weapon.
    /// </summary>
    public string SkillName { get; set; } = "";

    /// <summary>
    /// The character's skill AS for this weapon type.
    /// </summary>
    public int SkillAS { get; set; }

    /// <summary>
    /// Weapon's AV modifier (quality, magic, etc.).
    /// </summary>
    public int WeaponAVModifier { get; set; }

    /// <summary>
    /// Base SV modifier for damage.
    /// </summary>
    public int BaseSV { get; set; }

    /// <summary>
    /// Currently loaded ammo count.
    /// </summary>
    public int LoadedAmmo { get; set; }

    /// <summary>
    /// Maximum ammo capacity.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Ammo type currently loaded.
    /// </summary>
    public string? LoadedAmmoType { get; set; }

    /// <summary>
    /// Ammo type the weapon accepts (from weapon properties).
    /// Used for reload filtering.
    /// </summary>
    public string? AcceptedAmmoType { get; set; }

    /// <summary>
    /// How the weapon is reloaded (Magazine, SingleRound, Cylinder, etc.).
    /// Used to filter compatible ammo sources.
    /// </summary>
    public string ReloadType { get; set; } = "Magazine";

    /// <summary>
    /// Whether the weapon can accept loose ammo directly (arrows, individual rounds).
    /// </summary>
    public bool AcceptsLooseAmmo { get; set; }

    /// <summary>
    /// ID of the currently loaded magazine (if any).
    /// </summary>
    public Guid? LoadedMagazineId { get; set; }

    /// <summary>
    /// Damage modifier from special ammo.
    /// </summary>
    public int AmmoDamageModifier { get; set; }

    /// <summary>
    /// Special effect string from the loaded ammo (e.g., "Incendiary", "Cryo", "AP").
    /// Null if the loaded ammo has no special effect.
    /// </summary>
    public string? LoadedAmmoSpecialEffect { get; set; }

    /// <summary>
    /// Whether the weapon can be dodged (thrown, arrows).
    /// </summary>
    public bool IsDodgeable { get; set; }

    /// <summary>
    /// Supported fire modes.
    /// </summary>
    public bool SupportsSingle { get; set; } = true;
    public bool SupportsBurst { get; set; }
    public bool SupportsSuppression { get; set; }

    /// <summary>
    /// Number of rounds in a burst (for burst fire mode).
    /// </summary>
    public int BurstSize { get; set; } = 3;

    /// <summary>
    /// Number of rounds for suppressive fire.
    /// </summary>
    public int SuppressiveRounds { get; set; } = 10;

    // ========== AOE Properties (from weapon or loaded ammo) ==========

    /// <summary>
    /// Whether the weapon is currently AOE capable (from weapon or loaded ammo).
    /// </summary>
    public bool IsAOECapable { get; set; }

    /// <summary>
    /// Effective blast radius in meters (from weapon or ammo).
    /// </summary>
    public int BlastRadius { get; set; }

    /// <summary>
    /// Blast falloff type: "Linear", "Steep", or "Flat".
    /// </summary>
    public string? BlastFalloff { get; set; }

    /// <summary>
    /// Whether the AOE comes from ammo (true) or the weapon itself (false).
    /// </summary>
    public bool AOEFromAmmo { get; set; }

    /// <summary>
    /// Direct hit bonus SV for primary target.
    /// </summary>
    public int DirectHitBonus { get; set; }

    /// <summary>
    /// Range categories for the weapon.
    /// </summary>
    public int ShortRange { get; set; }
    public int MediumRange { get; set; }
    public int LongRange { get; set; }
    public int ExtremeRange { get; set; }
}

/// <summary>
/// Data returned when a ranged attack is completed.
/// </summary>
public class RangedAttackCompleteData
{
    /// <summary>
    /// Message to log.
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// The CharacterItem ID of the weapon used.
    /// </summary>
    public Guid WeaponItemId { get; set; }

    /// <summary>
    /// Amount of ammo consumed.
    /// </summary>
    public int AmmoConsumed { get; set; }

    /// <summary>
    /// Ammo remaining after the attack.
    /// </summary>
    public int AmmoRemaining { get; set; }
}
