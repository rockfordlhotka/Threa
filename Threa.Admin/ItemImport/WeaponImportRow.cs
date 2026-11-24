using CsvHelper;
using Threa.Dal.Dto;

namespace Threa.Admin.ItemImport;

/// <summary>
/// CSV row for melee weapons: base item fields plus weapon-specific fields.
/// ItemType is not a column; it is always Weapon.
/// </summary>
/// <remarks>
/// Every column is optional: a missing column leaves its default value, and
/// <see cref="ItemValidator"/> reports missing required values.
/// </remarks>
[CsvRecord]
public sealed partial class WeaponImportRow
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
    [CsvColumn(Optional = true)] public WeaponType WeaponType { get; set; }

    /// <summary>
    /// The skill used with the weapon; stored as <see cref="ItemTemplate.RelatedSkill"/>.
    /// </summary>
    [CsvColumn(Optional = true)] public string? Skill { get; set; }
    [CsvColumn(Optional = true)] public int DamageClass { get; set; }
    [CsvColumn(Optional = true)] public string? DamageType { get; set; }
    [CsvColumn(Optional = true)] public int SVModifier { get; set; }
    [CsvColumn(Optional = true)] public int AVModifier { get; set; }
    [CsvColumn(Optional = true)] public EquipmentSlot EquipmentSlot { get; set; }
    [CsvColumn(Optional = true)] public int MinSkillLevel { get; set; }

    public ItemTemplate ToItemTemplate() => new()
    {
        Name = Name,
        Description = Description,
        ShortDescription = ShortDescription,
        ItemType = ItemType.Weapon,
        Weight = Weight,
        Volume = Volume,
        Value = Value,
        Rarity = Rarity,
        WeaponType = WeaponType,
        RelatedSkill = Skill,
        DamageClass = DamageClass,
        DamageType = DamageType,
        SVModifier = SVModifier,
        AVModifier = AVModifier,
        EquipmentSlot = EquipmentSlot,
        MinSkillLevel = MinSkillLevel
    };
}
