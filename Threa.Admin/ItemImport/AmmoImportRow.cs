using CsvHelper;
using Threa.Dal.Dto;

namespace Threa.Admin.ItemImport;

/// <summary>
/// Intermediate class for importing ammunition.
/// </summary>
/// <remarks>
/// Every column is optional: a missing column leaves its default value, and
/// <see cref="ItemValidator"/> reports missing required values.
/// </remarks>
[CsvRecord]
public partial class AmmoImportRow
{
    // Core fields
    [CsvColumn(Optional = true)] public string Name { get; set; } = string.Empty;
    [CsvColumn(Optional = true)] public string Description { get; set; } = string.Empty;
    [CsvColumn(Optional = true)] public string ShortDescription { get; set; } = string.Empty;
    [CsvColumn(Optional = true)] public decimal Weight { get; set; }
    [CsvColumn(Optional = true)] public decimal Volume { get; set; }
    [CsvColumn(Optional = true)] public int Value { get; set; }
    [CsvColumn(Optional = true)] public ItemRarity Rarity { get; set; } = ItemRarity.Common;
    [CsvColumn(Optional = true)] public int MaxStackSize { get; set; } = 100;

    // Ammo specific
    [CsvColumn(Optional = true)] public string AmmoType { get; set; } = string.Empty;
    [CsvColumn(Optional = true)] public int DamageModifier { get; set; }
    [CsvColumn(Optional = true)] public int AVModifier { get; set; }
    [CsvColumn(Optional = true)] public string? SpecialEffect { get; set; }
    [CsvColumn(Optional = true)] public string? DamageType { get; set; }

    // If this is an ammo container (magazine, clip, etc.)
    [CsvColumn(Optional = true)] public bool IsContainer { get; set; }
    [CsvColumn(Optional = true)] public int ContainerCapacity { get; set; }

    // AOE properties (for explosive ammo like HE rounds)
    [CsvColumn(Optional = true)] public bool IsAOE { get; set; }
    [CsvColumn(Optional = true)] public int BlastRadius { get; set; }
    [CsvColumn(Optional = true)] public string? BlastFalloff { get; set; }
    [CsvColumn(Optional = true)] public int DirectHitBonus { get; set; }
}
