using CsvHelper.Configuration;
using Threa.Dal.Dto;

namespace Threa.Admin.ItemImport;

/// <summary>
/// CSV mapping for melee weapons.
/// Includes base item fields plus weapon-specific fields.
/// </summary>
public sealed class WeaponCsvMap : ClassMap<ItemTemplate>
{
    public WeaponCsvMap()
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
        Map(m => m.WeaponType).Name("WeaponType").Optional();
        Map(m => m.RelatedSkill).Name("Skill");
        Map(m => m.DamageClass).Name("DamageClass");
        Map(m => m.DamageType).Name("DamageType");
        Map(m => m.SVModifier).Name("SVModifier").Optional();
        Map(m => m.AVModifier).Name("AVModifier").Optional();
        Map(m => m.EquipmentSlot).Name("EquipmentSlot").Optional();
        Map(m => m.MinSkillLevel).Name("MinSkillLevel").Optional();

        // Not mapped - set in code
        // ItemType = Weapon (set automatically)
    }
}
