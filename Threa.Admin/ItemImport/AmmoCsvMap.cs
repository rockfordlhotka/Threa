using CsvHelper.Configuration;
using Threa.Dal.Dto;

namespace Threa.Admin.ItemImport;

/// <summary>
/// Intermediate class for importing ammunition.
/// </summary>
public class AmmoImportRow
{
    // Core fields
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal Volume { get; set; }
    public int Value { get; set; }
    public ItemRarity Rarity { get; set; } = ItemRarity.Common;
    public int MaxStackSize { get; set; } = 100;

    // Ammo specific
    public string AmmoType { get; set; } = string.Empty;
    public int DamageModifier { get; set; }
    public string? SpecialEffect { get; set; }
    public string? DamageType { get; set; }

    // If this is an ammo container (magazine, clip, etc.)
    public bool IsContainer { get; set; }
    public int ContainerCapacity { get; set; }

    // AOE properties (for explosive ammo like HE rounds)
    public bool IsAOE { get; set; }
    public int BlastRadius { get; set; }
    public string? BlastFalloff { get; set; }
    public int DirectHitBonus { get; set; }
}

public sealed class AmmoImportCsvMap : ClassMap<AmmoImportRow>
{
    public AmmoImportCsvMap()
    {
        // Core fields
        Map(m => m.Name).Name("Name");
        Map(m => m.Description).Name("Description").Optional();
        Map(m => m.ShortDescription).Name("ShortDescription").Optional();
        Map(m => m.Weight).Name("Weight").Optional();
        Map(m => m.Volume).Name("Volume").Optional();
        Map(m => m.Value).Name("Value").Optional();
        Map(m => m.Rarity).Name("Rarity").Optional();
        Map(m => m.MaxStackSize).Name("MaxStackSize").Optional();

        // Ammo specific
        Map(m => m.AmmoType).Name("AmmoType");
        Map(m => m.DamageModifier).Name("DamageModifier").Optional();
        Map(m => m.SpecialEffect).Name("SpecialEffect").Optional();
        Map(m => m.DamageType).Name("DamageType").Optional();

        // Container (magazine) fields
        Map(m => m.IsContainer).Name("IsContainer").Optional();
        Map(m => m.ContainerCapacity).Name("ContainerCapacity").Optional();

        // AOE properties (for explosive ammo)
        Map(m => m.IsAOE).Name("IsAOE").Optional();
        Map(m => m.BlastRadius).Name("BlastRadius").Optional();
        Map(m => m.BlastFalloff).Name("BlastFalloff").Optional();
        Map(m => m.DirectHitBonus).Name("DirectHitBonus").Optional();
    }
}
