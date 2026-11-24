using CsvHelper;
using Threa.Dal.Dto;

namespace Threa.Admin.ItemImport;

/// <summary>
/// Intermediate class for importing ammo containers (magazines, quivers, speedloaders).
/// </summary>
/// <remarks>
/// Every column is optional: a missing column leaves its default value, and
/// <see cref="ItemValidator"/> reports missing required values.
/// </remarks>
[CsvRecord]
public partial class AmmoContainerImportRow
{
    // Core fields
    [CsvColumn(Optional = true)] public string Name { get; set; } = string.Empty;
    [CsvColumn(Optional = true)] public string Description { get; set; } = string.Empty;
    [CsvColumn(Optional = true)] public string ShortDescription { get; set; } = string.Empty;
    [CsvColumn(Optional = true)] public decimal Weight { get; set; }
    [CsvColumn(Optional = true)] public decimal Volume { get; set; }
    [CsvColumn(Optional = true)] public int Value { get; set; }
    [CsvColumn(Optional = true)] public ItemRarity Rarity { get; set; } = ItemRarity.Common;

    // Ammo container specific
    [CsvColumn(Optional = true)] public string AmmoType { get; set; } = string.Empty;
    [CsvColumn(Optional = true)] public int Capacity { get; set; }
    [CsvColumn(Optional = true)] public string ContainerType { get; set; } = "Magazine";
    [CsvColumn(Optional = true)] public string? AllowedAmmoTypes { get; set; }
}
