using CsvHelper.Configuration;
using Threa.Dal.Dto;

namespace Threa.Admin.ItemImport;

/// <summary>
/// Base CSV mapping for common item fields shared across all item types.
/// </summary>
public sealed class BaseItemCsvMap : ClassMap<ItemTemplate>
{
    public BaseItemCsvMap()
    {
        // Core fields for all items
        Map(m => m.Name).Name("Name");
        Map(m => m.Description).Name("Description").Optional();
        Map(m => m.ShortDescription).Name("ShortDescription").Optional();
        Map(m => m.ItemType).Name("ItemType");
        Map(m => m.Weight).Name("Weight").Optional();
        Map(m => m.Volume).Name("Volume").Optional();
        Map(m => m.Value).Name("Value").Optional();
        Map(m => m.Rarity).Name("Rarity").Optional();
        Map(m => m.IsStackable).Name("IsStackable").Optional();
        Map(m => m.MaxStackSize).Name("MaxStackSize").Optional();
        Map(m => m.EquipmentSlot).Name("EquipmentSlot").Optional();
    }
}
