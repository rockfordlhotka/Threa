using GameMechanics.Combat;
using Threa.Dal.Dto;

namespace Threa.Admin.ItemImport;

/// <summary>
/// Validates item data for import.
/// </summary>
public static class ItemValidator
{
    // Valid discrete values for various fields
    public static readonly string[] ValidDamageTypes = ["Bashing", "Cutting", "Piercing", "Projectile", "Energy"];
    public static readonly string[] ValidReloadTypes = ["None", "Magazine", "SingleRound", "Cylinder", "Belt", "Battery"];
    public static readonly string[] ValidFireModes = ["Single", "Burst", "Suppression"];
    public static readonly string[] ValidContainerTypes = AmmoContainerType.ValidTypes;
    public static readonly string[] ValidRarities = Enum.GetNames<ItemRarity>();
    public static readonly string[] ValidItemTypes = Enum.GetNames<ItemType>();
    public static readonly string[] ValidWeaponTypes = Enum.GetNames<WeaponType>();
    public static readonly string[] ValidEquipmentSlots = Enum.GetNames<EquipmentSlot>();

    // Common ammo types (for guidance, not strict validation since custom types are allowed)
    public static readonly string[] CommonAmmoTypes = [
        "9mm", ".45 ACP", ".357 Magnum", ".44 Magnum",
        "5.56mm", "7.62mm", ".308", ".50 BMG",
        "12 Gauge", "20 Gauge",
        "Arrow", "Bolt", "Dart",
        "Energy Cell", "Power Pack", "Plasma Canister",
        "Grenade", "Rocket", "Missile"
    ];

    public static List<ItemValidationError> ValidateBaseItems(List<ItemTemplate> items)
    {
        var errors = new List<ItemValidationError>();
        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < items.Count; i++)
        {
            int row = i + 2;
            var item = items[i];

            // Name validation
            if (string.IsNullOrWhiteSpace(item.Name))
                errors.Add(new ItemValidationError(row, "Name", "Name is required"));
            else if (seenNames.Contains(item.Name))
                errors.Add(new ItemValidationError(row, "Name", $"Duplicate item name '{item.Name}'"));
            else
                seenNames.Add(item.Name);

            // Numeric validations
            if (item.Weight < 0)
                errors.Add(new ItemValidationError(row, "Weight", "Weight cannot be negative"));

            if (item.Value < 0)
                errors.Add(new ItemValidationError(row, "Value", "Value cannot be negative"));

            if (item.Volume < 0)
                errors.Add(new ItemValidationError(row, "Volume", "Volume cannot be negative"));

            if (item.MaxStackSize < 1)
                errors.Add(new ItemValidationError(row, "MaxStackSize", "MaxStackSize must be at least 1"));
        }

        return errors;
    }

    public static List<ItemValidationError> ValidateWeapons(List<ItemTemplate> items)
    {
        var errors = ValidateBaseItems(items);

        for (int i = 0; i < items.Count; i++)
        {
            int row = i + 2;
            var item = items[i];

            // Skill is required for weapons
            if (string.IsNullOrWhiteSpace(item.RelatedSkill))
                errors.Add(new ItemValidationError(row, "Skill", "Weapon skill is required"));

            // DamageClass validation
            if (item.DamageClass < 1 || item.DamageClass > 6)
                errors.Add(new ItemValidationError(row, "DamageClass", $"DamageClass must be 1-6, got '{item.DamageClass}'"));

            // DamageType validation
            if (!string.IsNullOrWhiteSpace(item.DamageType) && !ValidDamageTypes.Contains(item.DamageType, StringComparer.OrdinalIgnoreCase))
                errors.Add(new ItemValidationError(row, "DamageType",
                    $"Invalid damage type '{item.DamageType}'. Valid values: {string.Join(", ", ValidDamageTypes)}"));

            // WeaponType validation (if specified)
            if (item.WeaponType != WeaponType.None)
            {
                if (!Enum.IsDefined(item.WeaponType))
                    errors.Add(new ItemValidationError(row, "WeaponType",
                        $"Invalid weapon type '{item.WeaponType}'. Valid values: {string.Join(", ", ValidWeaponTypes)}"));
            }

            // EquipmentSlot validation
            if (item.EquipmentSlot != EquipmentSlot.None && item.EquipmentSlot != EquipmentSlot.MainHand &&
                item.EquipmentSlot != EquipmentSlot.OffHand && item.EquipmentSlot != EquipmentSlot.TwoHand)
            {
                errors.Add(new ItemValidationError(row, "EquipmentSlot",
                    $"Weapon equipment slot should be MainHand, OffHand, or TwoHand, got '{item.EquipmentSlot}'"));
            }
        }

        return errors;
    }

    public static List<ItemValidationError> ValidateArmor(List<ArmorImportRow> items)
    {
        var errors = new List<ItemValidationError>();
        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < items.Count; i++)
        {
            int row = i + 2;
            var item = items[i];

            // Name validation
            if (string.IsNullOrWhiteSpace(item.Name))
                errors.Add(new ItemValidationError(row, "Name", "Name is required"));
            else if (seenNames.Contains(item.Name))
                errors.Add(new ItemValidationError(row, "Name", $"Duplicate item name '{item.Name}'"));
            else
                seenNames.Add(item.Name);

            // Weight validation
            if (item.Weight < 0)
                errors.Add(new ItemValidationError(row, "Weight", "Weight cannot be negative"));

            // EquipmentSlot validation - armor needs a body slot
            if (item.EquipmentSlot == EquipmentSlot.None)
                errors.Add(new ItemValidationError(row, "EquipmentSlot",
                    $"Armor requires an equipment slot. Common armor slots: Head, Face, Chest, Shoulders, Back, Arms, Hands, Waist, Legs, Feet. For shields use: OffHand"));

            // Rarity validation
            if (!Enum.IsDefined(item.Rarity))
                errors.Add(new ItemValidationError(row, "Rarity",
                    $"Invalid rarity '{item.Rarity}'. Valid values: {string.Join(", ", ValidRarities)}"));

            // Absorption value validations (0-20 is reasonable)
            ValidateAbsorption(errors, row, "AbsorbBashing", item.AbsorbBashing);
            ValidateAbsorption(errors, row, "AbsorbCutting", item.AbsorbCutting);
            ValidateAbsorption(errors, row, "AbsorbPiercing", item.AbsorbPiercing);
            ValidateAbsorption(errors, row, "AbsorbProjectile", item.AbsorbProjectile);
            ValidateAbsorption(errors, row, "AbsorbEnergy", item.AbsorbEnergy);
        }

        return errors;
    }

    private static void ValidateAbsorption(List<ItemValidationError> errors, int row, string column, int value)
    {
        if (value < 0 || value > 20)
            errors.Add(new ItemValidationError(row, column, $"Absorption must be 0-20, got '{value}'"));
    }

    public static List<ItemValidationError> ValidateRangedWeapons(List<RangedWeaponImportRow> items)
    {
        var errors = new List<ItemValidationError>();
        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < items.Count; i++)
        {
            int row = i + 2;
            var item = items[i];

            // Name validation
            if (string.IsNullOrWhiteSpace(item.Name))
                errors.Add(new ItemValidationError(row, "Name", "Name is required"));
            else if (seenNames.Contains(item.Name))
                errors.Add(new ItemValidationError(row, "Name", $"Duplicate item name '{item.Name}'"));
            else
                seenNames.Add(item.Name);

            // Skill validation - ranged skill is required
            if (string.IsNullOrWhiteSpace(item.Skill))
                errors.Add(new ItemValidationError(row, "Skill", "Ranged skill is required (e.g., Archery, Pistols, Throwing - Light). MeleeSkill is optional."));

            // DamageClass validation
            if (item.DamageClass < 1 || item.DamageClass > 6)
                errors.Add(new ItemValidationError(row, "DamageClass", $"DamageClass must be 1-6, got '{item.DamageClass}'"));

            // DamageType validation
            if (!string.IsNullOrWhiteSpace(item.DamageType) && !ValidDamageTypes.Contains(item.DamageType, StringComparer.OrdinalIgnoreCase))
                errors.Add(new ItemValidationError(row, "DamageType",
                    $"Invalid damage type '{item.DamageType}'. Valid values: {string.Join(", ", ValidDamageTypes)}"));

            // AmmoType validation - required for non-thrown weapons
            if (!item.IsThrown && string.IsNullOrWhiteSpace(item.AmmoType))
                errors.Add(new ItemValidationError(row, "AmmoType",
                    $"AmmoType is required for ranged weapons (not required for thrown weapons). Common types: {string.Join(", ", CommonAmmoTypes.Take(10))}..."));

            // Range validations
            if (item.ShortRange <= 0)
                errors.Add(new ItemValidationError(row, "ShortRange", $"ShortRange must be positive, got '{item.ShortRange}'"));
            if (item.MediumRange <= item.ShortRange)
                errors.Add(new ItemValidationError(row, "MediumRange",
                    $"MediumRange ({item.MediumRange}) must be greater than ShortRange ({item.ShortRange})"));
            if (item.LongRange <= item.MediumRange)
                errors.Add(new ItemValidationError(row, "LongRange",
                    $"LongRange ({item.LongRange}) must be greater than MediumRange ({item.MediumRange})"));
            if (item.ExtremeRange <= item.LongRange)
                errors.Add(new ItemValidationError(row, "ExtremeRange",
                    $"ExtremeRange ({item.ExtremeRange}) must be greater than LongRange ({item.LongRange})"));

            // Capacity validation - required for non-thrown weapons
            if (!item.IsThrown && item.Capacity <= 0)
                errors.Add(new ItemValidationError(row, "Capacity", $"Capacity must be positive for non-thrown weapons, got '{item.Capacity}'"));

            // ReloadType validation
            if (!string.IsNullOrWhiteSpace(item.ReloadType) && !ValidReloadTypes.Contains(item.ReloadType, StringComparer.OrdinalIgnoreCase))
                errors.Add(new ItemValidationError(row, "ReloadType",
                    $"Invalid reload type '{item.ReloadType}'. Valid values: {string.Join(", ", ValidReloadTypes)}"));

            // FireModes validation
            if (!string.IsNullOrWhiteSpace(item.FireModes))
            {
                var modes = item.FireModes.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                foreach (var mode in modes)
                {
                    if (!ValidFireModes.Contains(mode, StringComparer.OrdinalIgnoreCase))
                        errors.Add(new ItemValidationError(row, "FireModes",
                            $"Invalid fire mode '{mode}'. Valid values: {string.Join(", ", ValidFireModes)}"));
                }
            }

            // Rarity validation
            if (!Enum.IsDefined(item.Rarity))
                errors.Add(new ItemValidationError(row, "Rarity",
                    $"Invalid rarity '{item.Rarity}'. Valid values: {string.Join(", ", ValidRarities)}"));

            // BurstSize validation (if burst mode is enabled)
            if (item.FireModes?.Contains("Burst", StringComparison.OrdinalIgnoreCase) == true && item.BurstSize < 2)
                errors.Add(new ItemValidationError(row, "BurstSize",
                    $"BurstSize should be at least 2 when Burst fire mode is enabled, got '{item.BurstSize}'"));

            // SuppressiveRounds validation (if suppression mode is enabled)
            if (item.FireModes?.Contains("Suppression", StringComparison.OrdinalIgnoreCase) == true && item.SuppressiveRounds < 3)
                errors.Add(new ItemValidationError(row, "SuppressiveRounds",
                    $"SuppressiveRounds should be at least 3 when Suppression fire mode is enabled, got '{item.SuppressiveRounds}'"));
        }

        return errors;
    }

    public static List<ItemValidationError> ValidateAmmo(List<AmmoImportRow> items)
    {
        var errors = new List<ItemValidationError>();
        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < items.Count; i++)
        {
            int row = i + 2;
            var item = items[i];

            // Name validation
            if (string.IsNullOrWhiteSpace(item.Name))
                errors.Add(new ItemValidationError(row, "Name", "Name is required"));
            else if (seenNames.Contains(item.Name))
                errors.Add(new ItemValidationError(row, "Name", $"Duplicate item name '{item.Name}'"));
            else
                seenNames.Add(item.Name);

            // AmmoType validation - required, provide examples
            if (string.IsNullOrWhiteSpace(item.AmmoType))
                errors.Add(new ItemValidationError(row, "AmmoType",
                    $"AmmoType is required. Common types: {string.Join(", ", CommonAmmoTypes.Take(10))}..."));

            // Container capacity validation
            if (item.IsContainer && item.ContainerCapacity <= 0)
                errors.Add(new ItemValidationError(row, "ContainerCapacity",
                    $"Container capacity must be positive for magazines/clips, got '{item.ContainerCapacity}'"));

            // Rarity validation
            if (!Enum.IsDefined(item.Rarity))
                errors.Add(new ItemValidationError(row, "Rarity",
                    $"Invalid rarity '{item.Rarity}'. Valid values: {string.Join(", ", ValidRarities)}"));

            // DamageType validation (if specified)
            if (!string.IsNullOrWhiteSpace(item.DamageType) && !ValidDamageTypes.Contains(item.DamageType, StringComparer.OrdinalIgnoreCase))
                errors.Add(new ItemValidationError(row, "DamageType",
                    $"Invalid damage type '{item.DamageType}'. Valid values: {string.Join(", ", ValidDamageTypes)}"));

            // Weight/Value validations
            if (item.Weight < 0)
                errors.Add(new ItemValidationError(row, "Weight", "Weight cannot be negative"));
            if (item.Value < 0)
                errors.Add(new ItemValidationError(row, "Value", "Value cannot be negative"));
            if (item.MaxStackSize < 1)
                errors.Add(new ItemValidationError(row, "MaxStackSize", "MaxStackSize must be at least 1"));
        }

        return errors;
    }

    public static List<ItemValidationError> ValidateAmmoContainers(List<AmmoContainerImportRow> items)
    {
        var errors = new List<ItemValidationError>();
        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < items.Count; i++)
        {
            int row = i + 2;
            var item = items[i];

            // Name validation
            if (string.IsNullOrWhiteSpace(item.Name))
                errors.Add(new ItemValidationError(row, "Name", "Name is required"));
            else if (seenNames.Contains(item.Name))
                errors.Add(new ItemValidationError(row, "Name", $"Duplicate item name '{item.Name}'"));
            else
                seenNames.Add(item.Name);

            // AmmoType validation - required
            if (string.IsNullOrWhiteSpace(item.AmmoType))
                errors.Add(new ItemValidationError(row, "AmmoType",
                    $"AmmoType is required. Common types: {string.Join(", ", CommonAmmoTypes.Take(10))}..."));

            // Capacity validation - must be positive
            if (item.Capacity <= 0)
                errors.Add(new ItemValidationError(row, "Capacity",
                    $"Capacity must be positive, got '{item.Capacity}'"));

            // ContainerType validation
            if (!string.IsNullOrWhiteSpace(item.ContainerType) && !ValidContainerTypes.Contains(item.ContainerType, StringComparer.OrdinalIgnoreCase))
                errors.Add(new ItemValidationError(row, "ContainerType",
                    $"Invalid container type '{item.ContainerType}'. Valid values: {string.Join(", ", ValidContainerTypes)}"));

            // Rarity validation
            if (!Enum.IsDefined(item.Rarity))
                errors.Add(new ItemValidationError(row, "Rarity",
                    $"Invalid rarity '{item.Rarity}'. Valid values: {string.Join(", ", ValidRarities)}"));

            // Weight/Value validations
            if (item.Weight < 0)
                errors.Add(new ItemValidationError(row, "Weight", "Weight cannot be negative"));
            if (item.Value < 0)
                errors.Add(new ItemValidationError(row, "Value", "Value cannot be negative"));
        }

        return errors;
    }

    /// <summary>
    /// Gets a formatted string of all validation requirements for display.
    /// </summary>
    public static string GetValidationSummary()
    {
        return $@"
Item Validation Requirements:
  DamageTypes: {string.Join(", ", ValidDamageTypes)}
  ReloadTypes: {string.Join(", ", ValidReloadTypes)}
  FireModes: {string.Join(", ", ValidFireModes)}
  ContainerTypes: {string.Join(", ", ValidContainerTypes)}
  Rarities: {string.Join(", ", ValidRarities)}
  ItemTypes: {string.Join(", ", ValidItemTypes)}
  WeaponTypes: {string.Join(", ", ValidWeaponTypes)}
  Common AmmoTypes: {string.Join(", ", CommonAmmoTypes)}
";
    }
}

public record ItemValidationError(int RowNumber, string ColumnName, string Message);
