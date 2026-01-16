using System.Text.Json;
using GameMechanics.Combat;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using Threa.Dal;
using Threa.Dal.Dto;

namespace Threa.Admin.ItemImport;

public class ImportItemSettings : CommandSettings
{
    [CommandArgument(0, "<FILE>")]
    [Description("Path to the CSV or TSV file")]
    public string FilePath { get; set; } = string.Empty;

    [CommandOption("--dry-run")]
    [Description("Validate the file without saving to database")]
    public bool DryRun { get; set; }
}

/// <summary>
/// Import basic/miscellaneous items.
/// </summary>
public class ImportItemsCommand : AsyncCommand<ImportItemSettings>
{
    private readonly IItemTemplateDal _dal;

    public ImportItemsCommand(IItemTemplateDal dal) => _dal = dal;

    public override ValidationResult Validate(CommandContext context, ImportItemSettings settings)
        => ImportValidation.ValidateFilePath(settings.FilePath);

    public override async Task<int> ExecuteAsync(CommandContext context, ImportItemSettings settings)
    {
        var items = CsvImportHelper.ReadCsv<ItemTemplate, BaseItemCsvMap>(settings.FilePath);
        if (items == null) return 1;

        var errors = ItemValidator.ValidateBaseItems(items);
        if (errors.Count > 0)
        {
            ImportValidation.PrintErrors(errors);
            return 1;
        }

        AnsiConsole.MarkupLine($"[green]Validated {items.Count} item(s)[/]");

        if (settings.DryRun)
        {
            AnsiConsole.MarkupLine("[yellow]Dry run - no changes made[/]");
            return 0;
        }

        return await ImportValidation.SaveItems(_dal, items);
    }
}

/// <summary>
/// Import melee weapons.
/// </summary>
public class ImportWeaponsCommand : AsyncCommand<ImportItemSettings>
{
    private readonly IItemTemplateDal _dal;

    public ImportWeaponsCommand(IItemTemplateDal dal) => _dal = dal;

    public override ValidationResult Validate(CommandContext context, ImportItemSettings settings)
        => ImportValidation.ValidateFilePath(settings.FilePath);

    public override async Task<int> ExecuteAsync(CommandContext context, ImportItemSettings settings)
    {
        var items = CsvImportHelper.ReadCsv<ItemTemplate, WeaponCsvMap>(settings.FilePath);
        if (items == null) return 1;

        // Set ItemType for all
        foreach (var item in items)
        {
            item.ItemType = ItemType.Weapon;
            if (item.EquipmentSlot == EquipmentSlot.None)
                item.EquipmentSlot = EquipmentSlot.MainHand;
        }

        var errors = ItemValidator.ValidateWeapons(items);
        if (errors.Count > 0)
        {
            ImportValidation.PrintErrors(errors);
            return 1;
        }

        AnsiConsole.MarkupLine($"[green]Validated {items.Count} weapon(s)[/]");

        if (settings.DryRun)
        {
            AnsiConsole.MarkupLine("[yellow]Dry run - no changes made[/]");
            return 0;
        }

        return await ImportValidation.SaveItems(_dal, items);
    }
}

/// <summary>
/// Import armor items (armor and shields).
/// </summary>
public class ImportArmorCommand : AsyncCommand<ImportItemSettings>
{
    private readonly IItemTemplateDal _dal;

    public ImportArmorCommand(IItemTemplateDal dal) => _dal = dal;

    public override ValidationResult Validate(CommandContext context, ImportItemSettings settings)
        => ImportValidation.ValidateFilePath(settings.FilePath);

    public override async Task<int> ExecuteAsync(CommandContext context, ImportItemSettings settings)
    {
        var rows = CsvImportHelper.ReadCsv<ArmorImportRow, ArmorImportCsvMap>(settings.FilePath);
        if (rows == null) return 1;

        var errors = ItemValidator.ValidateArmor(rows);
        if (errors.Count > 0)
        {
            ImportValidation.PrintErrors(errors);
            return 1;
        }

        AnsiConsole.MarkupLine($"[green]Validated {rows.Count} armor item(s)[/]");

        if (settings.DryRun)
        {
            AnsiConsole.MarkupLine("[yellow]Dry run - no changes made[/]");
            return 0;
        }

        var items = rows.Select(ConvertToItemTemplate).ToList();
        return await ImportValidation.SaveItems(_dal, items);
    }

    private static ItemTemplate ConvertToItemTemplate(ArmorImportRow row)
    {
        var isShield = row.EquipmentSlot == EquipmentSlot.OffHand;

        var absorption = new Dictionary<string, int>
        {
            ["Bashing"] = row.AbsorbBashing,
            ["Cutting"] = row.AbsorbCutting,
            ["Piercing"] = row.AbsorbPiercing,
            ["Projectile"] = row.AbsorbProjectile,
            ["Energy"] = row.AbsorbEnergy
        };

        return new ItemTemplate
        {
            Name = row.Name,
            Description = row.Description,
            ShortDescription = row.ShortDescription,
            ItemType = isShield ? ItemType.Shield : ItemType.Armor,
            Weight = row.Weight,
            Volume = row.Volume,
            Value = row.Value,
            Rarity = row.Rarity,
            EquipmentSlot = row.EquipmentSlot,
            RelatedSkill = row.Skill,
            DodgeModifier = row.DodgeModifier,
            ArmorAbsorption = JsonSerializer.Serialize(absorption)
        };
    }
}

/// <summary>
/// Import ranged weapons (guns, bows, etc.).
/// </summary>
public class ImportRangedWeaponsCommand : AsyncCommand<ImportItemSettings>
{
    private readonly IItemTemplateDal _dal;

    public ImportRangedWeaponsCommand(IItemTemplateDal dal) => _dal = dal;

    public override ValidationResult Validate(CommandContext context, ImportItemSettings settings)
        => ImportValidation.ValidateFilePath(settings.FilePath);

    public override async Task<int> ExecuteAsync(CommandContext context, ImportItemSettings settings)
    {
        var rows = CsvImportHelper.ReadCsv<RangedWeaponImportRow, RangedWeaponImportCsvMap>(settings.FilePath);
        if (rows == null) return 1;

        var errors = ItemValidator.ValidateRangedWeapons(rows);
        if (errors.Count > 0)
        {
            ImportValidation.PrintErrors(errors);
            return 1;
        }

        AnsiConsole.MarkupLine($"[green]Validated {rows.Count} ranged weapon(s)[/]");

        // Debug output: show skill values read from CSV
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Debug: Skill values read from CSV:[/]");
        var table = new Table();
        table.AddColumn("Name");
        table.AddColumn("Skill (Ranged)");
        table.AddColumn("MeleeSkill");
        table.AddColumn("WeaponType");
        table.AddColumn("IsThrown");
        foreach (var row in rows)
        {
            table.AddRow(
                row.Name,
                string.IsNullOrWhiteSpace(row.Skill) ? "[red](empty)[/]" : Markup.Escape(row.Skill),
                string.IsNullOrWhiteSpace(row.MeleeSkill) ? "[grey](empty)[/]" : Markup.Escape(row.MeleeSkill),
                row.WeaponType.ToString(),
                row.IsThrown ? "Yes" : "No"
            );
        }
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        if (settings.DryRun)
        {
            AnsiConsole.MarkupLine("[yellow]Dry run - no changes made[/]");
            return 0;
        }

        var items = rows.Select(ConvertToItemTemplate).ToList();

        // Debug output: show what will be saved
        AnsiConsole.MarkupLine("[grey]Debug: Values being saved:[/]");
        foreach (var item in items)
        {
            var props = RangedWeaponProperties.FromJson(item.CustomProperties);
            AnsiConsole.MarkupLine($"  [white]{Markup.Escape(item.Name)}[/]:");
            AnsiConsole.MarkupLine($"    RangedSkill (in JSON): {(string.IsNullOrWhiteSpace(props?.RangedSkill) ? "[red](empty)[/]" : Markup.Escape(props.RangedSkill))}");
            AnsiConsole.MarkupLine($"    RelatedSkill (MeleeSkill): {(string.IsNullOrWhiteSpace(item.RelatedSkill) ? "[grey](empty)[/]" : Markup.Escape(item.RelatedSkill))}");
        }
        AnsiConsole.WriteLine();

        return await ImportValidation.SaveItems(_dal, items);
    }

    private static ItemTemplate ConvertToItemTemplate(RangedWeaponImportRow row)
    {
        var rangedProps = new RangedWeaponProperties
        {
            IsRangedWeapon = true,
            IsThrown = row.IsThrown,
            // The primary skill for ranged use (shooting/throwing)
            RangedSkill = row.Skill,
            ShortRange = row.ShortRange,
            MediumRange = row.MediumRange,
            LongRange = row.LongRange,
            ExtremeRange = row.ExtremeRange,
            // For thrown weapons, capacity/ammo don't apply
            Capacity = row.IsThrown ? 0 : row.Capacity,
            ChamberCapacity = row.IsThrown ? 0 : row.ChamberCapacity,
            ReloadType = row.IsThrown ? "None" : row.ReloadType,
            AcceptsLooseAmmo = row.AcceptsLooseAmmo,
            AmmoType = row.IsThrown ? null : row.AmmoType,
            FireModes = string.IsNullOrWhiteSpace(row.FireModes)
                ? new List<string> { "Single" }
                : row.FireModes.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList(),
            BurstSize = row.BurstSize,
            SuppressiveRounds = row.SuppressiveRounds,
            IsDodgeable = row.IsThrown ? true : row.IsDodgeable, // Thrown weapons are typically dodgeable
            BaseSVModifier = row.SVModifier,
            AccuracyModifier = row.AVModifier,
            // Inherent AOE (for grenades, rockets, etc.)
            IsInherentAOE = row.IsInherentAOE,
            DefaultBlastRadius = row.DefaultBlastRadius,
            DefaultBlastFalloff = row.DefaultBlastFalloff
        };

        return new ItemTemplate
        {
            Name = row.Name,
            Description = row.Description,
            ShortDescription = row.ShortDescription,
            ItemType = ItemType.Weapon,
            WeaponType = row.WeaponType,
            Weight = row.Weight,
            Volume = row.Volume,
            Value = row.Value,
            Rarity = row.Rarity,
            EquipmentSlot = EquipmentSlot.MainHand,
            // Optional melee skill for close combat use
            RelatedSkill = row.MeleeSkill,
            DamageClass = row.DamageClass,
            DamageType = row.DamageType,
            SVModifier = row.SVModifier,
            AVModifier = row.AVModifier,
            // Thrown weapons should be stackable
            IsStackable = row.IsThrown,
            MaxStackSize = row.IsThrown ? 20 : 1,
            CustomProperties = rangedProps.ToJson()
        };
    }
}

/// <summary>
/// Import ammunition.
/// </summary>
public class ImportAmmoCommand : AsyncCommand<ImportItemSettings>
{
    private readonly IItemTemplateDal _dal;

    public ImportAmmoCommand(IItemTemplateDal dal) => _dal = dal;

    public override ValidationResult Validate(CommandContext context, ImportItemSettings settings)
        => ImportValidation.ValidateFilePath(settings.FilePath);

    public override async Task<int> ExecuteAsync(CommandContext context, ImportItemSettings settings)
    {
        var rows = CsvImportHelper.ReadCsv<AmmoImportRow, AmmoImportCsvMap>(settings.FilePath);
        if (rows == null) return 1;

        var errors = ItemValidator.ValidateAmmo(rows);
        if (errors.Count > 0)
        {
            ImportValidation.PrintErrors(errors);
            return 1;
        }

        AnsiConsole.MarkupLine($"[green]Validated {rows.Count} ammo item(s)[/]");

        if (settings.DryRun)
        {
            AnsiConsole.MarkupLine("[yellow]Dry run - no changes made[/]");
            return 0;
        }

        var items = rows.Select(ConvertToItemTemplate).ToList();
        return await ImportValidation.SaveItems(_dal, items);
    }

    private static ItemTemplate ConvertToItemTemplate(AmmoImportRow row)
    {
        var ammoProps = new AmmunitionProperties
        {
            AmmoType = row.AmmoType,
            DamageModifier = row.DamageModifier,
            AccuracyModifier = row.AVModifier,
            SpecialEffect = row.SpecialEffect,
            IsContainer = row.IsContainer,
            ContainerCapacity = row.ContainerCapacity,
            // AOE properties
            IsAOE = row.IsAOE,
            BlastRadius = row.BlastRadius,
            BlastFalloff = row.BlastFalloff,
            DirectHitBonus = row.DirectHitBonus
        };

        return new ItemTemplate
        {
            Name = row.Name,
            Description = row.Description,
            ShortDescription = row.ShortDescription,
            ItemType = ItemType.Ammunition,
            Weight = row.Weight,
            Volume = row.Volume,
            Value = row.Value,
            Rarity = row.Rarity,
            IsStackable = !row.IsContainer,
            MaxStackSize = row.MaxStackSize,
            DamageType = row.DamageType,
            CustomProperties = ammoProps.ToJson()
        };
    }
}

/// <summary>
/// Import ammo containers (magazines, quivers, speedloaders).
/// </summary>
public class ImportAmmoContainersCommand : AsyncCommand<ImportItemSettings>
{
    private readonly IItemTemplateDal _dal;

    public ImportAmmoContainersCommand(IItemTemplateDal dal) => _dal = dal;

    public override ValidationResult Validate(CommandContext context, ImportItemSettings settings)
        => ImportValidation.ValidateFilePath(settings.FilePath);

    public override async Task<int> ExecuteAsync(CommandContext context, ImportItemSettings settings)
    {
        var rows = CsvImportHelper.ReadCsv<AmmoContainerImportRow, AmmoContainerImportCsvMap>(settings.FilePath);
        if (rows == null) return 1;

        var errors = ItemValidator.ValidateAmmoContainers(rows);
        if (errors.Count > 0)
        {
            ImportValidation.PrintErrors(errors);
            return 1;
        }

        AnsiConsole.MarkupLine($"[green]Validated {rows.Count} ammo container(s)[/]");

        if (settings.DryRun)
        {
            AnsiConsole.MarkupLine("[yellow]Dry run - no changes made[/]");
            return 0;
        }

        var items = rows.Select(ConvertToItemTemplate).ToList();
        return await ImportValidation.SaveItems(_dal, items);
    }

    private static ItemTemplate ConvertToItemTemplate(AmmoContainerImportRow row)
    {
        var containerProps = new AmmoContainerProperties
        {
            IsAmmoContainer = true,
            AmmoType = row.AmmoType,
            Capacity = row.Capacity,
            ContainerType = string.IsNullOrWhiteSpace(row.ContainerType) ? "Magazine" : row.ContainerType,
            AllowedAmmoTypes = row.AllowedAmmoTypes
        };

        return new ItemTemplate
        {
            Name = row.Name,
            Description = row.Description,
            ShortDescription = row.ShortDescription,
            ItemType = ItemType.AmmoContainer,
            Weight = row.Weight,
            Volume = row.Volume,
            Value = row.Value,
            Rarity = row.Rarity,
            IsStackable = false,
            MaxStackSize = 1,
            CustomProperties = containerProps.ToJson()
        };
    }
}

/// <summary>
/// Shared validation and save logic for import commands.
/// </summary>
internal static class ImportValidation
{
    public static ValidationResult ValidateFilePath(string filePath)
    {
        if (!File.Exists(filePath))
            return ValidationResult.Error($"File not found: {filePath}");

        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        if (ext != ".csv" && ext != ".tsv")
            return ValidationResult.Error("File must have .csv or .tsv extension");

        return ValidationResult.Success();
    }

    public static void PrintErrors(List<ItemValidationError> errors)
    {
        AnsiConsole.MarkupLine($"[red]Validation failed with {errors.Count} error(s):[/]");
        AnsiConsole.WriteLine();
        foreach (var error in errors)
        {
            AnsiConsole.MarkupLine($"  Row {error.RowNumber}, [yellow]{error.ColumnName}[/]: {Markup.Escape(error.Message)}");
        }
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[red]No items were imported.[/]");
    }

    public static async Task<int> SaveItems(IItemTemplateDal dal, List<ItemTemplate> items)
    {
        var existing = await dal.GetAllTemplatesAsync();
        var existingNames = existing.ToDictionary(e => e.Name, StringComparer.OrdinalIgnoreCase);

        int inserted = 0, updated = 0;
        foreach (var item in items)
        {
            if (existingNames.TryGetValue(item.Name, out var existingItem))
            {
                item.Id = existingItem.Id;
                updated++;
            }
            else
            {
                inserted++;
            }
            await dal.SaveTemplateAsync(item);
        }

        AnsiConsole.MarkupLine($"  [grey]Inserted:[/] {inserted}");
        AnsiConsole.MarkupLine($"  [grey]Updated:[/] {updated}");
        AnsiConsole.MarkupLine("[green]Import complete![/]");
        return 0;
    }
}
