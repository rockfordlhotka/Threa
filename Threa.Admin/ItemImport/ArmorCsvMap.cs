using CsvHelper.Configuration;
using Threa.Dal.Dto;

namespace Threa.Admin.ItemImport;

/// <summary>
/// CSV mapping for armor items.
/// Includes base item fields plus armor-specific absorption values.
/// </summary>
public sealed class ArmorCsvMap : ClassMap<ItemTemplate>
{
    public ArmorCsvMap()
    {
        // Core fields
        Map(m => m.Name).Name("Name");
        Map(m => m.Description).Name("Description").Optional();
        Map(m => m.ShortDescription).Name("ShortDescription").Optional();
        Map(m => m.Weight).Name("Weight").Optional();
        Map(m => m.Volume).Name("Volume").Optional();
        Map(m => m.Value).Name("Value").Optional();
        Map(m => m.Rarity).Name("Rarity").Optional();
        Map(m => m.EquipmentSlot).Name("EquipmentSlot");
        Map(m => m.RelatedSkill).Name("Skill").Optional();
        Map(m => m.DodgeModifier).Name("DodgeModifier").Optional();

        // Armor absorption is handled specially via custom columns
        // AbsorbBashing, AbsorbCutting, AbsorbPiercing, AbsorbProjectile, AbsorbEnergy
        // These get converted to ArmorAbsorption JSON

        // Not mapped - set in code
        // ItemType = Armor (set automatically)
    }
}

/// <summary>
/// Intermediate class for importing armor with separate absorption columns.
/// </summary>
public class ArmorImportRow
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal Volume { get; set; }
    public int Value { get; set; }
    public ItemRarity Rarity { get; set; } = ItemRarity.Common;
    public EquipmentSlot EquipmentSlot { get; set; }
    public string? Skill { get; set; }
    public int DodgeModifier { get; set; }

    // Absorption columns
    public int AbsorbBashing { get; set; }
    public int AbsorbCutting { get; set; }
    public int AbsorbPiercing { get; set; }
    public int AbsorbProjectile { get; set; }
    public int AbsorbEnergy { get; set; }
}

public sealed class ArmorImportCsvMap : ClassMap<ArmorImportRow>
{
    public ArmorImportCsvMap()
    {
        Map(m => m.Name).Name("Name");
        Map(m => m.Description).Name("Description").Optional();
        Map(m => m.ShortDescription).Name("ShortDescription").Optional();
        Map(m => m.Weight).Name("Weight").Optional();
        Map(m => m.Volume).Name("Volume").Optional();
        Map(m => m.Value).Name("Value").Optional();
        Map(m => m.Rarity).Name("Rarity").Optional();
        Map(m => m.EquipmentSlot).Name("EquipmentSlot");
        Map(m => m.Skill).Name("Skill").Optional();
        Map(m => m.DodgeModifier).Name("DodgeModifier").Optional();

        // Absorption columns
        Map(m => m.AbsorbBashing).Name("AbsorbBashing").Optional();
        Map(m => m.AbsorbCutting).Name("AbsorbCutting").Optional();
        Map(m => m.AbsorbPiercing).Name("AbsorbPiercing").Optional();
        Map(m => m.AbsorbProjectile).Name("AbsorbProjectile").Optional();
        Map(m => m.AbsorbEnergy).Name("AbsorbEnergy").Optional();
    }
}
