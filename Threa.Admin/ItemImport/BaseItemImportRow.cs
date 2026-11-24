using CsvHelper;
using Threa.Dal.Dto;

namespace Threa.Admin.ItemImport;

/// <summary>
/// CSV row for basic/miscellaneous items (the common fields shared by all item types).
/// </summary>
/// <remarks>
/// Every column is optional: a missing column leaves its default value, and
/// <see cref="ItemValidator"/> reports missing required values.
/// </remarks>
[CsvRecord]
public sealed partial class BaseItemImportRow
{
    [CsvColumn(Optional = true)] public string Name { get; set; } = string.Empty;
    [CsvColumn(Optional = true)] public string Description { get; set; } = string.Empty;
    [CsvColumn(Optional = true)] public string ShortDescription { get; set; } = string.Empty;
    [CsvColumn(Optional = true)] public ItemType ItemType { get; set; }
    [CsvColumn(Optional = true)] public decimal Weight { get; set; }
    [CsvColumn(Optional = true)] public decimal Volume { get; set; }
    [CsvColumn(Optional = true)] public int Value { get; set; }
    [CsvColumn(Optional = true)] public ItemRarity Rarity { get; set; } = ItemRarity.Common;
    [CsvColumn(Optional = true)] public bool IsStackable { get; set; }
    [CsvColumn(Optional = true)] public int MaxStackSize { get; set; } = 1;
    [CsvColumn(Optional = true)] public EquipmentSlot EquipmentSlot { get; set; }

    public ItemTemplate ToItemTemplate() => new()
    {
        Name = Name,
        Description = Description,
        ShortDescription = ShortDescription,
        ItemType = ItemType,
        Weight = Weight,
        Volume = Volume,
        Value = Value,
        Rarity = Rarity,
        IsStackable = IsStackable,
        MaxStackSize = MaxStackSize,
        EquipmentSlot = EquipmentSlot
    };
}
