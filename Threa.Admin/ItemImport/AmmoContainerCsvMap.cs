using CsvHelper.Configuration;
using Threa.Dal.Dto;

namespace Threa.Admin.ItemImport;

/// <summary>
/// Intermediate class for importing ammo containers (magazines, quivers, speedloaders).
/// </summary>
public class AmmoContainerImportRow
{
    // Core fields
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal Volume { get; set; }
    public int Value { get; set; }
    public ItemRarity Rarity { get; set; } = ItemRarity.Common;

    // Ammo container specific
    public string AmmoType { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string ContainerType { get; set; } = "Magazine";
    public string? AllowedAmmoTypes { get; set; }
}

public sealed class AmmoContainerImportCsvMap : ClassMap<AmmoContainerImportRow>
{
    public AmmoContainerImportCsvMap()
    {
        // Core fields
        Map(m => m.Name).Name("Name");
        Map(m => m.Description).Name("Description").Optional();
        Map(m => m.ShortDescription).Name("ShortDescription").Optional();
        Map(m => m.Weight).Name("Weight").Optional();
        Map(m => m.Volume).Name("Volume").Optional();
        Map(m => m.Value).Name("Value").Optional();
        Map(m => m.Rarity).Name("Rarity").Optional();

        // Ammo container specific
        Map(m => m.AmmoType).Name("AmmoType");
        Map(m => m.Capacity).Name("Capacity");
        Map(m => m.ContainerType).Name("ContainerType").Optional();
        Map(m => m.AllowedAmmoTypes).Name("AllowedAmmoTypes").Optional();
    }
}
