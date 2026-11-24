using CsvHelper;
using Threa.Dal.Dto;

namespace Threa.Admin.ItemImport;

/// <summary>
/// CSV row for armor and shields, with separate absorption columns that are
/// converted to the ArmorAbsorption JSON. ItemType is set from the slot.
/// </summary>
/// <remarks>
/// Every column is optional: a missing column leaves its default value, and
/// <see cref="ItemValidator"/> reports missing required values.
/// </remarks>
[CsvRecord]
public partial class ArmorImportRow
{
    [CsvColumn(Optional = true)] public string Name { get; set; } = string.Empty;
    [CsvColumn(Optional = true)] public string Description { get; set; } = string.Empty;
    [CsvColumn(Optional = true)] public string ShortDescription { get; set; } = string.Empty;
    [CsvColumn(Optional = true)] public decimal Weight { get; set; }
    [CsvColumn(Optional = true)] public decimal Volume { get; set; }
    [CsvColumn(Optional = true)] public int Value { get; set; }
    [CsvColumn(Optional = true)] public ItemRarity Rarity { get; set; } = ItemRarity.Common;
    [CsvColumn(Optional = true)] public EquipmentSlot EquipmentSlot { get; set; }
    [CsvColumn(Optional = true)] public string? Skill { get; set; }
    [CsvColumn(Optional = true)] public int DodgeModifier { get; set; }

    // Absorption columns
    [CsvColumn(Optional = true)] public int AbsorbBashing { get; set; }
    [CsvColumn(Optional = true)] public int AbsorbCutting { get; set; }
    [CsvColumn(Optional = true)] public int AbsorbPiercing { get; set; }
    [CsvColumn(Optional = true)] public int AbsorbProjectile { get; set; }
    [CsvColumn(Optional = true)] public int AbsorbEnergy { get; set; }
}
