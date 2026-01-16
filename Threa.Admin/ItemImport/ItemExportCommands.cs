using System.Globalization;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using GameMechanics.Combat;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using Threa.Dal;
using Threa.Dal.Dto;

namespace Threa.Admin.ItemImport;

public class ExportItemSettings : CommandSettings
{
    [CommandArgument(0, "<DIRECTORY>")]
    [Description("Directory path where export files will be created")]
    public string OutputDirectory { get; set; } = string.Empty;

    [CommandOption("--tsv")]
    [Description("Export as tab-separated values instead of CSV")]
    public bool UseTsv { get; set; }

    [CommandOption("--skip-validation-info")]
    [Description("Skip printing validation restrictions")]
    public bool SkipValidationInfo { get; set; }
}

/// <summary>
/// Export items to CSV/TSV files, one per item category.
/// Always creates files with headers for all categories.
/// </summary>
public class ExportItemsCommand : AsyncCommand<ExportItemSettings>
{
    private readonly IItemTemplateDal _dal;

    public ExportItemsCommand(IItemTemplateDal dal) => _dal = dal;

    public override ValidationResult Validate(CommandContext context, ExportItemSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.OutputDirectory))
            return ValidationResult.Error("Output directory is required");

        return ValidationResult.Success();
    }

    public override async Task<int> ExecuteAsync(CommandContext context, ExportItemSettings settings)
    {
        // Ensure directory exists
        if (!Directory.Exists(settings.OutputDirectory))
        {
            try
            {
                Directory.CreateDirectory(settings.OutputDirectory);
                AnsiConsole.MarkupLine($"[grey]Created directory: {Markup.Escape(settings.OutputDirectory)}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error creating directory:[/] {Markup.Escape(ex.Message)}");
                return 1;
            }
        }

        // Print validation info first
        if (!settings.SkipValidationInfo)
        {
            PrintValidationInfo();
            AnsiConsole.WriteLine();
        }

        // Load all items
        var allItems = await _dal.GetAllTemplatesAsync();
        AnsiConsole.MarkupLine($"[grey]Loaded {allItems.Count} item templates[/]");

        // Categorize items
        var basicItems = new List<ItemTemplate>();
        var meleeWeapons = new List<ItemTemplate>();
        var rangedWeapons = new List<(ItemTemplate Item, RangedWeaponProperties Props)>();
        var armor = new List<ItemTemplate>();
        var ammunition = new List<(ItemTemplate Item, AmmunitionProperties? Props)>();

        foreach (var item in allItems)
        {
            switch (item.ItemType)
            {
                case ItemType.Weapon:
                    var rangedProps = RangedWeaponProperties.FromJson(item.CustomProperties);
                    if (rangedProps != null)
                        rangedWeapons.Add((item, rangedProps));
                    else
                        meleeWeapons.Add(item);
                    break;

                case ItemType.Armor:
                case ItemType.Shield:
                    armor.Add(item);
                    break;

                case ItemType.Ammunition:
                    var ammoProps = AmmunitionProperties.FromJson(item.CustomProperties);
                    ammunition.Add((item, ammoProps));
                    break;

                default:
                    basicItems.Add(item);
                    break;
            }
        }

        var extension = settings.UseTsv ? ".tsv" : ".csv";
        var delimiter = settings.UseTsv ? "\t" : ",";
        int totalExported = 0;

        // Show category counts
        AnsiConsole.MarkupLine("[grey]Item counts by category:[/]");
        AnsiConsole.MarkupLine($"  Basic items: {basicItems.Count}");
        AnsiConsole.MarkupLine($"  Melee weapons: {meleeWeapons.Count}");
        AnsiConsole.MarkupLine($"  Ranged weapons: {rangedWeapons.Count}");
        AnsiConsole.MarkupLine($"  Armor/shields: {armor.Count}");
        AnsiConsole.MarkupLine($"  Ammunition: {ammunition.Count}");
        AnsiConsole.WriteLine();

        // Export each category (always create files, even if empty - headers only)
        AnsiConsole.MarkupLine("[grey]Exporting files:[/]");

        var itemsPath = Path.Combine(settings.OutputDirectory, $"items{extension}");
        ExportBasicItems(itemsPath, basicItems, delimiter);
        AnsiConsole.MarkupLine(basicItems.Count > 0
            ? $"  [green]items{extension}[/]: {basicItems.Count} basic items"
            : $"  [yellow]items{extension}[/]: headers only (no data)");
        totalExported += basicItems.Count;

        var weaponsPath = Path.Combine(settings.OutputDirectory, $"weapons{extension}");
        ExportMeleeWeapons(weaponsPath, meleeWeapons, delimiter);
        AnsiConsole.MarkupLine(meleeWeapons.Count > 0
            ? $"  [green]weapons{extension}[/]: {meleeWeapons.Count} melee weapons"
            : $"  [yellow]weapons{extension}[/]: headers only (no data)");
        totalExported += meleeWeapons.Count;

        var rangedPath = Path.Combine(settings.OutputDirectory, $"ranged-weapons{extension}");
        ExportRangedWeapons(rangedPath, rangedWeapons, delimiter);
        AnsiConsole.MarkupLine(rangedWeapons.Count > 0
            ? $"  [green]ranged-weapons{extension}[/]: {rangedWeapons.Count} ranged weapons"
            : $"  [yellow]ranged-weapons{extension}[/]: headers only (no data)");
        totalExported += rangedWeapons.Count;

        var armorPath = Path.Combine(settings.OutputDirectory, $"armor{extension}");
        ExportArmor(armorPath, armor, delimiter);
        AnsiConsole.MarkupLine(armor.Count > 0
            ? $"  [green]armor{extension}[/]: {armor.Count} armor items"
            : $"  [yellow]armor{extension}[/]: headers only (no data)");
        totalExported += armor.Count;

        var ammoPath = Path.Combine(settings.OutputDirectory, $"ammo{extension}");
        ExportAmmunition(ammoPath, ammunition, delimiter);
        AnsiConsole.MarkupLine(ammunition.Count > 0
            ? $"  [green]ammo{extension}[/]: {ammunition.Count} ammunition items"
            : $"  [yellow]ammo{extension}[/]: headers only (no data)");
        totalExported += ammunition.Count;

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]Export complete![/] {totalExported} total items exported to {Markup.Escape(settings.OutputDirectory)}");

        return 0;
    }

    private void PrintValidationInfo()
    {
        AnsiConsole.Write(new Rule("[yellow]Import Validation Requirements[/]").LeftJustified());
        AnsiConsole.WriteLine();

        // Basic Items
        AnsiConsole.MarkupLine("[bold]items.csv/tsv (Basic Items)[/]");
        AnsiConsole.MarkupLine("  [grey]Required:[/] Name (unique)");
        AnsiConsole.MarkupLine("  [grey]Optional:[/] Description, ShortDescription, ItemType, Weight (>=0), Volume, Value (>=0), Rarity, IsStackable, MaxStackSize, EquipmentSlot");
        AnsiConsole.MarkupLine("  [grey]ItemType values:[/] Miscellaneous, Consumable, Container, Jewelry, Clothing, Tool, Component, QuestItem, Currency");
        AnsiConsole.MarkupLine("  [grey]Rarity values:[/] Common, Uncommon, Rare, Epic, Legendary, Artifact");
        AnsiConsole.MarkupLine("  [grey]EquipmentSlot values:[/] None, Head, Face, Neck, Shoulders, Chest, Arms, Hands, Waist, Legs, Feet, MainHand, OffHand, Ring, Back");
        AnsiConsole.WriteLine();

        // Melee Weapons
        AnsiConsole.MarkupLine("[bold]weapons.csv/tsv (Melee Weapons)[/]");
        AnsiConsole.MarkupLine("  [grey]Required:[/] Name (unique), Skill, DamageClass (1-6), DamageType");
        AnsiConsole.MarkupLine("  [grey]Optional:[/] Description, ShortDescription, Weight (>=0), Volume, Value (>=0), Rarity, WeaponType, SVModifier, AVModifier, EquipmentSlot, MinSkillLevel");
        AnsiConsole.MarkupLine("  [grey]DamageType values:[/] Bashing, Cutting, Piercing, Projectile, Energy");
        AnsiConsole.MarkupLine("  [grey]WeaponType values:[/] None, Sword, Axe, Mace, Spear, Dagger, Staff, Unarmed, Bow, Crossbow, Thrown, Pistol, Rifle, Shotgun, SMG, Wand");
        AnsiConsole.WriteLine();

        // Ranged Weapons
        AnsiConsole.MarkupLine("[bold]ranged-weapons.csv/tsv (Ranged Weapons)[/]");
        AnsiConsole.MarkupLine("  [grey]Required:[/] Name (unique), Skill (ranged skill), DamageClass (1-6), ShortRange (>0), MediumRange (>Short), LongRange (>Medium), ExtremeRange (>Long)");
        AnsiConsole.MarkupLine("  [grey]Required (non-thrown):[/] Capacity (>0), AmmoType");
        AnsiConsole.MarkupLine("  [grey]Optional:[/] Description, ShortDescription, Weight, Volume, Value, Rarity, WeaponType, MeleeSkill, DamageType, SVModifier, AVModifier, IsThrown, ChamberCapacity, ReloadType, AcceptsLooseAmmo, FireModes, BurstSize, SuppressiveRounds, IsDodgeable");
        AnsiConsole.MarkupLine("  [grey]Inherent AOE:[/] IsInherentAOE (for grenades/rockets), DefaultBlastRadius (meters), DefaultBlastFalloff (Linear/Steep/Flat)");
        AnsiConsole.MarkupLine("  [grey]Skill columns:[/] Skill = ranged use (Archery, Pistols, Throwing), MeleeSkill = melee use (Brawling, Light Blades)");
        AnsiConsole.MarkupLine("  [grey]WeaponType values:[/] None, Sword, Axe, Mace, Spear, Dagger, Staff, Unarmed, Bow, Crossbow, Thrown, Pistol, Rifle, Shotgun, SMG, Wand");
        AnsiConsole.MarkupLine("  [grey]ReloadType values:[/] None (thrown weapons), Magazine, SingleRound, Cylinder, Belt, Battery");
        AnsiConsole.MarkupLine("  [grey]FireModes:[/] Comma-separated list of: Single, Burst, Suppression");
        AnsiConsole.MarkupLine("  [grey]Note:[/] For thrown weapons (IsThrown=true), AmmoType and Capacity are not required - the weapon itself is consumed");
        AnsiConsole.MarkupLine("  [grey]Note:[/] AOE is NOT a fire mode - use IsInherentAOE for weapons or ammo with AOE properties");
        AnsiConsole.WriteLine();

        // Armor
        AnsiConsole.MarkupLine("[bold]armor.csv/tsv (Armor & Shields)[/]");
        AnsiConsole.MarkupLine("  [grey]Required:[/] Name (unique), EquipmentSlot");
        AnsiConsole.MarkupLine("  [grey]Optional:[/] Description, ShortDescription, Weight (>=0), Volume, Value, Rarity, Skill, DodgeModifier");
        AnsiConsole.MarkupLine("  [grey]Absorption columns:[/] AbsorbBashing (0-20), AbsorbCutting (0-20), AbsorbPiercing (0-20), AbsorbProjectile (0-20), AbsorbEnergy (0-20)");
        AnsiConsole.MarkupLine("  [grey]Note:[/] EquipmentSlot=OffHand creates Shield, other slots create Armor");
        AnsiConsole.WriteLine();

        // Ammunition
        AnsiConsole.MarkupLine("[bold]ammo.csv/tsv (Ammunition)[/]");
        AnsiConsole.MarkupLine("  [grey]Required:[/] Name (unique), AmmoType");
        AnsiConsole.MarkupLine("  [grey]Optional:[/] Description, ShortDescription, Weight, Volume, Value, Rarity, MaxStackSize, DamageModifier, SpecialEffect, DamageType, IsContainer, ContainerCapacity");
        AnsiConsole.MarkupLine("  [grey]AOE fields:[/] IsAOE (for explosive ammo), BlastRadius (meters), BlastFalloff (Linear/Steep/Flat), DirectHitBonus (extra SV for direct hit)");
        AnsiConsole.MarkupLine("  [grey]Note:[/] If IsContainer=true, ContainerCapacity must be > 0 (for magazines)");
        AnsiConsole.MarkupLine("  [grey]Note:[/] If IsAOE=true, BlastRadius should be > 0. AOE ammo enables AOE attacks with any compatible weapon.");
        AnsiConsole.WriteLine();

        AnsiConsole.Write(new Rule().LeftJustified());
    }

    private void ExportBasicItems(string path, List<ItemTemplate> items, string delimiter)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = delimiter };
        using var writer = new StreamWriter(path);
        using var csv = new CsvWriter(writer, config);

        // Write header
        csv.WriteField("Name");
        csv.WriteField("Description");
        csv.WriteField("ShortDescription");
        csv.WriteField("ItemType");
        csv.WriteField("Weight");
        csv.WriteField("Volume");
        csv.WriteField("Value");
        csv.WriteField("Rarity");
        csv.WriteField("IsStackable");
        csv.WriteField("MaxStackSize");
        csv.WriteField("EquipmentSlot");
        csv.NextRecord();

        foreach (var item in items)
        {
            csv.WriteField(item.Name);
            csv.WriteField(item.Description);
            csv.WriteField(item.ShortDescription);
            csv.WriteField(item.ItemType.ToString());
            csv.WriteField(item.Weight);
            csv.WriteField(item.Volume);
            csv.WriteField(item.Value);
            csv.WriteField(item.Rarity.ToString());
            csv.WriteField(item.IsStackable);
            csv.WriteField(item.MaxStackSize);
            csv.WriteField(item.EquipmentSlot.ToString());
            csv.NextRecord();
        }
    }

    private void ExportMeleeWeapons(string path, List<ItemTemplate> items, string delimiter)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = delimiter };
        using var writer = new StreamWriter(path);
        using var csv = new CsvWriter(writer, config);

        // Write header
        csv.WriteField("Name");
        csv.WriteField("Description");
        csv.WriteField("ShortDescription");
        csv.WriteField("Weight");
        csv.WriteField("Volume");
        csv.WriteField("Value");
        csv.WriteField("Rarity");
        csv.WriteField("WeaponType");
        csv.WriteField("Skill");
        csv.WriteField("DamageClass");
        csv.WriteField("DamageType");
        csv.WriteField("SVModifier");
        csv.WriteField("AVModifier");
        csv.WriteField("EquipmentSlot");
        csv.WriteField("MinSkillLevel");
        csv.NextRecord();

        foreach (var item in items)
        {
            csv.WriteField(item.Name);
            csv.WriteField(item.Description);
            csv.WriteField(item.ShortDescription);
            csv.WriteField(item.Weight);
            csv.WriteField(item.Volume);
            csv.WriteField(item.Value);
            csv.WriteField(item.Rarity.ToString());
            csv.WriteField(item.WeaponType.ToString());
            csv.WriteField(item.RelatedSkill);
            csv.WriteField(item.DamageClass);
            csv.WriteField(item.DamageType);
            csv.WriteField(item.SVModifier);
            csv.WriteField(item.AVModifier);
            csv.WriteField(item.EquipmentSlot.ToString());
            csv.WriteField(item.MinSkillLevel);
            csv.NextRecord();
        }
    }

    private void ExportRangedWeapons(string path, List<(ItemTemplate Item, RangedWeaponProperties Props)> items, string delimiter)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = delimiter };
        using var writer = new StreamWriter(path);
        using var csv = new CsvWriter(writer, config);

        // Write header
        csv.WriteField("Name");
        csv.WriteField("Description");
        csv.WriteField("ShortDescription");
        csv.WriteField("Weight");
        csv.WriteField("Volume");
        csv.WriteField("Value");
        csv.WriteField("Rarity");
        csv.WriteField("WeaponType");
        csv.WriteField("Skill");
        csv.WriteField("MeleeSkill");
        csv.WriteField("DamageClass");
        csv.WriteField("DamageType");
        csv.WriteField("SVModifier");
        csv.WriteField("AVModifier");
        csv.WriteField("IsThrown");
        csv.WriteField("ShortRange");
        csv.WriteField("MediumRange");
        csv.WriteField("LongRange");
        csv.WriteField("ExtremeRange");
        csv.WriteField("Capacity");
        csv.WriteField("ChamberCapacity");
        csv.WriteField("ReloadType");
        csv.WriteField("AcceptsLooseAmmo");
        csv.WriteField("AmmoType");
        csv.WriteField("FireModes");
        csv.WriteField("BurstSize");
        csv.WriteField("SuppressiveRounds");
        csv.WriteField("IsDodgeable");
        csv.WriteField("IsInherentAOE");
        csv.WriteField("DefaultBlastRadius");
        csv.WriteField("DefaultBlastFalloff");
        csv.NextRecord();

        foreach (var (item, props) in items)
        {
            csv.WriteField(item.Name);
            csv.WriteField(item.Description);
            csv.WriteField(item.ShortDescription);
            csv.WriteField(item.Weight);
            csv.WriteField(item.Volume);
            csv.WriteField(item.Value);
            csv.WriteField(item.Rarity.ToString());
            csv.WriteField(item.WeaponType.ToString());
            // Skill = ranged skill (from RangedWeaponProperties, with fallback to RelatedSkill for old data)
            var rangedSkill = !string.IsNullOrWhiteSpace(props.RangedSkill) ? props.RangedSkill : item.RelatedSkill;
            csv.WriteField(rangedSkill);
            // MeleeSkill = optional melee skill (from ItemTemplate, but only if RangedSkill was set separately)
            var meleeSkill = !string.IsNullOrWhiteSpace(props.RangedSkill) ? item.RelatedSkill : null;
            csv.WriteField(meleeSkill);
            csv.WriteField(item.DamageClass);
            csv.WriteField(item.DamageType);
            csv.WriteField(item.SVModifier);
            csv.WriteField(item.AVModifier);
            csv.WriteField(props.IsThrown);
            csv.WriteField(props.ShortRange);
            csv.WriteField(props.MediumRange);
            csv.WriteField(props.LongRange);
            csv.WriteField(props.ExtremeRange);
            csv.WriteField(props.Capacity);
            csv.WriteField(props.ChamberCapacity);
            csv.WriteField(props.ReloadType);
            csv.WriteField(props.AcceptsLooseAmmo);
            csv.WriteField(props.AmmoType);
            csv.WriteField(string.Join(",", props.FireModes));
            csv.WriteField(props.BurstSize);
            csv.WriteField(props.SuppressiveRounds);
            csv.WriteField(props.IsDodgeable);
            csv.WriteField(props.IsInherentAOE);
            csv.WriteField(props.DefaultBlastRadius);
            csv.WriteField(props.DefaultBlastFalloff ?? "");
            csv.NextRecord();
        }
    }

    private void ExportArmor(string path, List<ItemTemplate> items, string delimiter)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = delimiter };
        using var writer = new StreamWriter(path);
        using var csv = new CsvWriter(writer, config);

        // Write header
        csv.WriteField("Name");
        csv.WriteField("Description");
        csv.WriteField("ShortDescription");
        csv.WriteField("Weight");
        csv.WriteField("Volume");
        csv.WriteField("Value");
        csv.WriteField("Rarity");
        csv.WriteField("EquipmentSlot");
        csv.WriteField("Skill");
        csv.WriteField("DodgeModifier");
        csv.WriteField("AbsorbBashing");
        csv.WriteField("AbsorbCutting");
        csv.WriteField("AbsorbPiercing");
        csv.WriteField("AbsorbProjectile");
        csv.WriteField("AbsorbEnergy");
        csv.NextRecord();

        foreach (var item in items)
        {
            // Parse absorption JSON
            var absorption = ParseArmorAbsorption(item.ArmorAbsorption);

            csv.WriteField(item.Name);
            csv.WriteField(item.Description);
            csv.WriteField(item.ShortDescription);
            csv.WriteField(item.Weight);
            csv.WriteField(item.Volume);
            csv.WriteField(item.Value);
            csv.WriteField(item.Rarity.ToString());
            csv.WriteField(item.EquipmentSlot.ToString());
            csv.WriteField(item.RelatedSkill);
            csv.WriteField(item.DodgeModifier);
            csv.WriteField(absorption.GetValueOrDefault("Bashing", 0));
            csv.WriteField(absorption.GetValueOrDefault("Cutting", 0));
            csv.WriteField(absorption.GetValueOrDefault("Piercing", 0));
            csv.WriteField(absorption.GetValueOrDefault("Projectile", 0));
            csv.WriteField(absorption.GetValueOrDefault("Energy", 0));
            csv.NextRecord();
        }
    }

    private void ExportAmmunition(string path, List<(ItemTemplate Item, AmmunitionProperties? Props)> items, string delimiter)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = delimiter };
        using var writer = new StreamWriter(path);
        using var csv = new CsvWriter(writer, config);

        // Write header
        csv.WriteField("Name");
        csv.WriteField("Description");
        csv.WriteField("ShortDescription");
        csv.WriteField("Weight");
        csv.WriteField("Volume");
        csv.WriteField("Value");
        csv.WriteField("Rarity");
        csv.WriteField("MaxStackSize");
        csv.WriteField("AmmoType");
        csv.WriteField("DamageModifier");
        csv.WriteField("SpecialEffect");
        csv.WriteField("DamageType");
        csv.WriteField("IsContainer");
        csv.WriteField("ContainerCapacity");
        csv.WriteField("IsAOE");
        csv.WriteField("BlastRadius");
        csv.WriteField("BlastFalloff");
        csv.WriteField("DirectHitBonus");
        csv.NextRecord();

        foreach (var (item, props) in items)
        {
            csv.WriteField(item.Name);
            csv.WriteField(item.Description);
            csv.WriteField(item.ShortDescription);
            csv.WriteField(item.Weight);
            csv.WriteField(item.Volume);
            csv.WriteField(item.Value);
            csv.WriteField(item.Rarity.ToString());
            csv.WriteField(item.MaxStackSize);
            csv.WriteField(props?.AmmoType ?? "");
            csv.WriteField(props?.DamageModifier ?? 0);
            csv.WriteField(props?.SpecialEffect ?? "");
            csv.WriteField(item.DamageType);
            csv.WriteField(props?.IsContainer ?? false);
            csv.WriteField(props?.ContainerCapacity ?? 0);
            csv.WriteField(props?.IsAOE ?? false);
            csv.WriteField(props?.BlastRadius ?? 0);
            csv.WriteField(props?.BlastFalloff ?? "");
            csv.WriteField(props?.DirectHitBonus ?? 0);
            csv.NextRecord();
        }
    }

    private Dictionary<string, int> ParseArmorAbsorption(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, int>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, int>>(json)
                ?? new Dictionary<string, int>();
        }
        catch
        {
            return new Dictionary<string, int>();
        }
    }
}
