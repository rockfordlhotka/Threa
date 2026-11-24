using CsvHelper;
using Threa.Dal.Dto;

namespace Threa.Admin.ItemImport;

/// <summary>
/// Intermediate class for importing ranged weapons with all properties.
/// </summary>
/// <remarks>
/// Every column is optional: a missing column leaves its default value, and
/// <see cref="ItemValidator"/> reports missing required values.
/// </remarks>
[CsvRecord]
public partial class RangedWeaponImportRow
{
    // Core fields
    [CsvColumn(Optional = true)] public string Name { get; set; } = string.Empty;
    [CsvColumn(Optional = true)] public string Description { get; set; } = string.Empty;
    [CsvColumn(Optional = true)] public string ShortDescription { get; set; } = string.Empty;
    [CsvColumn(Optional = true)] public decimal Weight { get; set; }
    [CsvColumn(Optional = true)] public decimal Volume { get; set; }
    [CsvColumn(Optional = true)] public int Value { get; set; }
    [CsvColumn(Optional = true)] public ItemRarity Rarity { get; set; } = ItemRarity.Common;

    // Weapon fields
    /// <summary>
    /// The primary skill for using this weapon at range (e.g., "Archery", "Pistols", "Throwing - Light").
    /// </summary>
    [CsvColumn(Optional = true)] public string Skill { get; set; } = string.Empty;

    /// <summary>
    /// Optional skill for using this weapon in melee (e.g., "Brawling" for pistol-whip, "Light Blades" for dagger stab).
    /// </summary>
    [CsvColumn(Optional = true)] public string? MeleeSkill { get; set; }

    [CsvColumn(Optional = true)] public WeaponType WeaponType { get; set; } = WeaponType.None;
    [CsvColumn(Optional = true)] public int DamageClass { get; set; }
    [CsvColumn(Optional = true)] public string DamageType { get; set; } = "Projectile";
    [CsvColumn(Optional = true)] public int SVModifier { get; set; }
    [CsvColumn(Optional = true)] public int AVModifier { get; set; }

    // Ranged weapon specific
    /// <summary>
    /// True for thrown weapons (throwing knife, javelin) where the weapon is consumed when thrown.
    /// </summary>
    [CsvColumn(Optional = true)] public bool IsThrown { get; set; }
    [CsvColumn(Optional = true)] public int ShortRange { get; set; }
    [CsvColumn(Optional = true)] public int MediumRange { get; set; }
    [CsvColumn(Optional = true)] public int LongRange { get; set; }
    [CsvColumn(Optional = true)] public int ExtremeRange { get; set; }
    [CsvColumn(Optional = true)] public int Capacity { get; set; }
    [CsvColumn(Optional = true)] public int ChamberCapacity { get; set; }
    [CsvColumn(Optional = true)] public string ReloadType { get; set; } = "Magazine";
    [CsvColumn(Optional = true)] public bool AcceptsLooseAmmo { get; set; }
    [CsvColumn(Optional = true)] public string AmmoType { get; set; } = string.Empty;
    [CsvColumn(Optional = true)] public string FireModes { get; set; } = "Single"; // Comma-separated list
    [CsvColumn(Optional = true)] public int BurstSize { get; set; } = 3;
    [CsvColumn(Optional = true)] public int SuppressiveRounds { get; set; } = 10;
    [CsvColumn(Optional = true)] public bool IsDodgeable { get; set; }

    // Inherent AOE (for grenades, rockets, etc.)
    [CsvColumn(Optional = true)] public bool IsInherentAOE { get; set; }
    [CsvColumn(Optional = true)] public int DefaultBlastRadius { get; set; }
    [CsvColumn(Optional = true)] public string? DefaultBlastFalloff { get; set; }
}
