using CsvHelper.Configuration;
using Threa.Dal.Dto;

namespace Threa.Admin.ItemImport;

/// <summary>
/// Intermediate class for importing ranged weapons with all properties.
/// </summary>
public class RangedWeaponImportRow
{
    // Core fields
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal Volume { get; set; }
    public int Value { get; set; }
    public ItemRarity Rarity { get; set; } = ItemRarity.Common;

    // Weapon fields
    /// <summary>
    /// The primary skill for using this weapon at range (e.g., "Archery", "Pistols", "Throwing - Light").
    /// </summary>
    public string Skill { get; set; } = string.Empty;

    /// <summary>
    /// Optional skill for using this weapon in melee (e.g., "Brawling" for pistol-whip, "Light Blades" for dagger stab).
    /// </summary>
    public string? MeleeSkill { get; set; }

    public WeaponType WeaponType { get; set; } = WeaponType.None;
    public int DamageClass { get; set; }
    public string DamageType { get; set; } = "Projectile";
    public int SVModifier { get; set; }
    public int AVModifier { get; set; }

    // Ranged weapon specific
    /// <summary>
    /// True for thrown weapons (throwing knife, javelin) where the weapon is consumed when thrown.
    /// </summary>
    public bool IsThrown { get; set; }
    public int ShortRange { get; set; }
    public int MediumRange { get; set; }
    public int LongRange { get; set; }
    public int ExtremeRange { get; set; }
    public int Capacity { get; set; }
    public int ChamberCapacity { get; set; }
    public string ReloadType { get; set; } = "Magazine";
    public bool AcceptsLooseAmmo { get; set; }
    public string AmmoType { get; set; } = string.Empty;
    public string FireModes { get; set; } = "Single"; // Comma-separated list
    public int BurstSize { get; set; } = 3;
    public int SuppressiveRounds { get; set; } = 10;
    public bool IsDodgeable { get; set; }

    // Inherent AOE (for grenades, rockets, etc.)
    public bool IsInherentAOE { get; set; }
    public int DefaultBlastRadius { get; set; }
    public string? DefaultBlastFalloff { get; set; }
}

public sealed class RangedWeaponImportCsvMap : ClassMap<RangedWeaponImportRow>
{
    public RangedWeaponImportCsvMap()
    {
        // Core fields
        Map(m => m.Name).Name("Name");
        Map(m => m.Description).Name("Description").Optional();
        Map(m => m.ShortDescription).Name("ShortDescription").Optional();
        Map(m => m.Weight).Name("Weight").Optional();
        Map(m => m.Volume).Name("Volume").Optional();
        Map(m => m.Value).Name("Value").Optional();
        Map(m => m.Rarity).Name("Rarity").Optional();

        // Weapon fields
        Map(m => m.Skill).Name("Skill");
        Map(m => m.MeleeSkill).Name("MeleeSkill").Optional();
        Map(m => m.WeaponType).Name("WeaponType").Optional();
        Map(m => m.DamageClass).Name("DamageClass");
        Map(m => m.DamageType).Name("DamageType").Optional();
        Map(m => m.SVModifier).Name("SVModifier").Optional();
        Map(m => m.AVModifier).Name("AVModifier").Optional();

        // Ranged weapon specific
        Map(m => m.IsThrown).Name("IsThrown").Optional();
        Map(m => m.ShortRange).Name("ShortRange");
        Map(m => m.MediumRange).Name("MediumRange");
        Map(m => m.LongRange).Name("LongRange");
        Map(m => m.ExtremeRange).Name("ExtremeRange");
        Map(m => m.Capacity).Name("Capacity");
        Map(m => m.ChamberCapacity).Name("ChamberCapacity").Optional();
        Map(m => m.ReloadType).Name("ReloadType").Optional();
        Map(m => m.AcceptsLooseAmmo).Name("AcceptsLooseAmmo").Optional();
        Map(m => m.AmmoType).Name("AmmoType");
        Map(m => m.FireModes).Name("FireModes").Optional();
        Map(m => m.BurstSize).Name("BurstSize").Optional();
        Map(m => m.SuppressiveRounds).Name("SuppressiveRounds").Optional();
        Map(m => m.IsDodgeable).Name("IsDodgeable").Optional();

        // Inherent AOE (for grenades, rockets, etc.)
        Map(m => m.IsInherentAOE).Name("IsInherentAOE").Optional();
        Map(m => m.DefaultBlastRadius).Name("DefaultBlastRadius").Optional();
        Map(m => m.DefaultBlastFalloff).Name("DefaultBlastFalloff").Optional();
    }
}
