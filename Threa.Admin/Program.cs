using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Threa.Dal;
using Threa.Dal.Dto;
using GameMechanics.Player;
using Threa.Admin.ItemImport;

namespace Threa.Admin;

class Program
{
    static int Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // Build service provider using shared DAL configuration
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSqlite();

        // Register commands for DI
        services.AddTransient<CreateCommand>();
        services.AddTransient<ListCommand>();
        services.AddTransient<RolesCommand>();
        services.AddTransient<GrantCommand>();
        services.AddTransient<RevokeCommand>();
        services.AddTransient<EnableCommand>();
        services.AddTransient<DisableCommand>();
        services.AddTransient<DeleteCommand>();
        services.AddTransient<SetPasswordCommand>();
        services.AddTransient<ImportSkillsCommand>();
        services.AddTransient<ExportSkillsCommand>();

        // Item import/export commands
        services.AddTransient<ImportItemsCommand>();
        services.AddTransient<ImportWeaponsCommand>();
        services.AddTransient<ImportArmorCommand>();
        services.AddTransient<ImportRangedWeaponsCommand>();
        services.AddTransient<ImportAmmoCommand>();
        services.AddTransient<ImportAmmoContainersCommand>();
        services.AddTransient<ExportItemsCommand>();

        var serviceProvider = services.BuildServiceProvider();

        var app = new CommandApp(new TypeRegistrar(serviceProvider));
        app.Configure(config =>
        {
            config.SetApplicationName("threa-admin");
            config.AddCommand<CreateCommand>("create")
                .WithDescription("Create a new user");
            config.AddCommand<ListCommand>("list")
                .WithDescription("List all users with their roles");
            config.AddCommand<RolesCommand>("roles")
                .WithDescription("Show roles for a specific user");
            config.AddCommand<GrantCommand>("grant")
                .WithDescription("Add a role to a user");
            config.AddCommand<RevokeCommand>("revoke")
                .WithDescription("Remove a role from a user");
            config.AddCommand<EnableCommand>("enable")
                .WithDescription("Enable a user account");
            config.AddCommand<DisableCommand>("disable")
                .WithDescription("Disable a user account");
            config.AddCommand<DeleteCommand>("delete")
                .WithDescription("Delete a user and all their data");
            config.AddCommand<SetPasswordCommand>("set-password")
                .WithDescription("Set a new password for a user");
            config.AddCommand<ImportSkillsCommand>("import-skills")
                .WithDescription("Import skills from a CSV or TSV file");
            config.AddCommand<ExportSkillsCommand>("export-skills")
                .WithDescription("Export skills to a CSV or TSV file");

            // Item import commands
            config.AddCommand<ImportItemsCommand>("import-items")
                .WithDescription("Import basic/miscellaneous items from CSV or TSV");
            config.AddCommand<ImportWeaponsCommand>("import-weapons")
                .WithDescription("Import melee weapons from CSV or TSV");
            config.AddCommand<ImportArmorCommand>("import-armor")
                .WithDescription("Import armor and shields from CSV or TSV");
            config.AddCommand<ImportRangedWeaponsCommand>("import-ranged-weapons")
                .WithDescription("Import ranged weapons (guns, bows) from CSV or TSV");
            config.AddCommand<ImportAmmoCommand>("import-ammo")
                .WithDescription("Import ammunition from CSV or TSV");
            config.AddCommand<ImportAmmoContainersCommand>("import-ammo-containers")
                .WithDescription("Import ammo containers (magazines, quivers) from CSV or TSV");
            config.AddCommand<ExportItemsCommand>("export-items")
                .WithDescription("Export items to CSV or TSV files by category");
        });
        return app.Run(args);
    }
}

/// <summary>
/// Bridges Spectre.Console.Cli with Microsoft.Extensions.DependencyInjection
/// </summary>
public sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceProvider _serviceProvider;

    public TypeRegistrar(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ITypeResolver Build() => new TypeResolver(_serviceProvider);

    public void Register(Type service, Type implementation)
    {
        // Commands are resolved from the container, no dynamic registration needed
    }

    public void RegisterInstance(Type service, object implementation)
    {
        // Commands are resolved from the container, no dynamic registration needed
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
        // Commands are resolved from the container, no dynamic registration needed
    }
}

public sealed class TypeResolver : ITypeResolver
{
    private readonly IServiceProvider _serviceProvider;

    public TypeResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public object? Resolve(Type? type)
    {
        if (type == null) return null;
        return _serviceProvider.GetService(type) ?? Activator.CreateInstance(type);
    }
}

public class CommonSettings : CommandSettings
{
}

public class EmailSettings : CommonSettings
{
    [CommandArgument(0, "<EMAIL>")]
    [Description("User email address")]
    public string Email { get; set; } = string.Empty;
}

public class CreateSettings : EmailSettings
{
    [CommandOption("--password <PASSWORD>")]
    [Description("Initial password (will prompt if not provided)")]
    public string? Password { get; set; }

    [CommandOption("--admin")]
    [Description("Grant Administrator role")]
    public bool Admin { get; set; }
}

public class RoleSettings : EmailSettings
{
    [CommandArgument(1, "<ROLE>")]
    [Description("Role name (Administrator, GameMaster, Player)")]
    public string Role { get; set; } = string.Empty;
}

public class PasswordSettings : EmailSettings
{
    [CommandOption("--password <PASSWORD>")]
    [Description("New password (will prompt if not provided)")]
    public string? Password { get; set; }
}

public class CreateCommand : AsyncCommand<CreateSettings>
{
    private readonly IPlayerDal _dal;

    public CreateCommand(IPlayerDal dal) => _dal = dal;

    public override async Task<int> ExecuteAsync(CommandContext context, CreateSettings settings)
    {
        // Check if user already exists
        var existing = await _dal.GetPlayerByEmailAsync(settings.Email);
        if (existing != null)
        {
            AnsiConsole.MarkupLine($"[red]User already exists:[/] {Markup.Escape(settings.Email)}");
            return 1;
        }

        // Get password
        var password = settings.Password;
        if (string.IsNullOrEmpty(password))
        {
            password = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter password:")
                    .Secret());
        }

        // Create user
        var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);

        var player = new Threa.Dal.Dto.Player
        {
            Email = settings.Email,
            Name = settings.Email.Split('@')[0],
            Salt = salt,
            HashedPassword = hashedPassword,
            IsEnabled = true,
            Roles = settings.Admin ? Roles.Administrator : null
        };

        await _dal.SavePlayerAsync(player);

        AnsiConsole.MarkupLine($"[green]Created user:[/] {Markup.Escape(settings.Email)}");
        if (settings.Admin)
        {
            AnsiConsole.MarkupLine($"[green]Granted role:[/] [red]Administrator[/]");
        }
        return 0;
    }
}

public class ListCommand : AsyncCommand<CommonSettings>
{
    private readonly IPlayerDal _dal;

    public ListCommand(IPlayerDal dal) => _dal = dal;

    public override async Task<int> ExecuteAsync(CommandContext context, CommonSettings settings)
    {
        var players = await _dal.GetAllPlayersAsync();

        var table = new Table();
        table.AddColumn("Email");
        table.AddColumn("Name");
        table.AddColumn("Roles");
        table.AddColumn("Status");

        foreach (var player in players)
        {
            var roles = string.IsNullOrEmpty(player.Roles)
                ? "[grey](none)[/]"
                : FormatRoles(player.Roles);
            var status = player.IsEnabled ? "[green]Enabled[/]" : "[red]Disabled[/]";
            table.AddRow(
                Markup.Escape(player.Email),
                Markup.Escape(player.Name ?? ""),
                roles,
                status
            );
        }

        AnsiConsole.Write(table);
        return 0;
    }

    private static string FormatRoles(string roles)
    {
        var parts = roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var formatted = parts.Select(r => r switch
        {
            "Administrator" => "[red]Administrator[/]",
            "GameMaster" => "[blue]GameMaster[/]",
            "Player" => "[green]Player[/]",
            _ => Markup.Escape(r)
        });
        return string.Join(", ", formatted);
    }
}

public class RolesCommand : AsyncCommand<EmailSettings>
{
    private readonly IPlayerDal _dal;

    public RolesCommand(IPlayerDal dal) => _dal = dal;

    public override async Task<int> ExecuteAsync(CommandContext context, EmailSettings settings)
    {
        var player = await _dal.GetPlayerByEmailAsync(settings.Email);

        if (player == null)
        {
            AnsiConsole.MarkupLine($"[red]User not found:[/] {Markup.Escape(settings.Email)}");
            return 1;
        }

        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("Property");
        table.AddColumn("Value");
        table.AddRow("Email", Markup.Escape(player.Email));
        table.AddRow("Name", Markup.Escape(player.Name ?? "(not set)"));
        table.AddRow("Roles", FormatRoles(player.Roles));
        table.AddRow("Status", player.IsEnabled ? "[green]Enabled[/]" : "[red]Disabled[/]");

        AnsiConsole.Write(table);
        return 0;
    }

    private static string FormatRoles(string? roles)
    {
        if (string.IsNullOrEmpty(roles))
            return "[grey](none)[/]";

        var parts = roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var formatted = parts.Select(r => r switch
        {
            "Administrator" => "[red]Administrator[/]",
            "GameMaster" => "[blue]GameMaster[/]",
            "Player" => "[green]Player[/]",
            _ => Markup.Escape(r)
        });
        return string.Join(", ", formatted);
    }
}

public class GrantCommand : AsyncCommand<RoleSettings>
{
    private readonly IPlayerDal _dal;

    public GrantCommand(IPlayerDal dal) => _dal = dal;

    public override ValidationResult Validate(CommandContext context, RoleSettings settings)
    {
        if (!IsValidRole(settings.Role))
        {
            return ValidationResult.Error($"Invalid role: {settings.Role}. Valid roles are: {Roles.Administrator}, {Roles.GameMaster}, {Roles.Player}");
        }
        return ValidationResult.Success();
    }

    public override async Task<int> ExecuteAsync(CommandContext context, RoleSettings settings)
    {
        var player = await _dal.GetPlayerByEmailAsync(settings.Email);

        if (player == null)
        {
            AnsiConsole.MarkupLine($"[red]User not found:[/] {Markup.Escape(settings.Email)}");
            return 1;
        }

        var currentRoles = ParseRoles(player.Roles);
        if (currentRoles.Contains(settings.Role, StringComparer.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine($"[yellow]User already has role:[/] {settings.Role}");
            return 0;
        }

        currentRoles.Add(settings.Role);
        player.Roles = string.Join(",", currentRoles);
        await _dal.SavePlayerAsync(player);

        AnsiConsole.MarkupLine($"[green]Granted role[/] [blue]{settings.Role}[/] [green]to[/] {Markup.Escape(settings.Email)}");
        AnsiConsole.MarkupLine($"Current roles: {FormatRoles(player.Roles)}");
        return 0;
    }

    private static List<string> ParseRoles(string? roles)
    {
        if (string.IsNullOrEmpty(roles))
            return new List<string>();
        return roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    private static bool IsValidRole(string role) =>
        role.Equals(Roles.Administrator, StringComparison.OrdinalIgnoreCase)
        || role.Equals(Roles.GameMaster, StringComparison.OrdinalIgnoreCase)
        || role.Equals(Roles.Player, StringComparison.OrdinalIgnoreCase);

    private static string FormatRoles(string roles)
    {
        var parts = roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var formatted = parts.Select(r => r switch
        {
            "Administrator" => "[red]Administrator[/]",
            "GameMaster" => "[blue]GameMaster[/]",
            "Player" => "[green]Player[/]",
            _ => Markup.Escape(r)
        });
        return string.Join(", ", formatted);
    }
}

public class RevokeCommand : AsyncCommand<RoleSettings>
{
    private readonly IPlayerDal _dal;

    public RevokeCommand(IPlayerDal dal) => _dal = dal;

    public override async Task<int> ExecuteAsync(CommandContext context, RoleSettings settings)
    {
        var player = await _dal.GetPlayerByEmailAsync(settings.Email);

        if (player == null)
        {
            AnsiConsole.MarkupLine($"[red]User not found:[/] {Markup.Escape(settings.Email)}");
            return 1;
        }

        var currentRoles = ParseRoles(player.Roles);
        var roleToRemove = currentRoles.FirstOrDefault(r => r.Equals(settings.Role, StringComparison.OrdinalIgnoreCase));

        if (roleToRemove == null)
        {
            AnsiConsole.MarkupLine($"[yellow]User does not have role:[/] {settings.Role}");
            return 0;
        }

        currentRoles.Remove(roleToRemove);
        player.Roles = currentRoles.Count > 0 ? string.Join(",", currentRoles) : null;
        await _dal.SavePlayerAsync(player);

        AnsiConsole.MarkupLine($"[green]Revoked role[/] [blue]{settings.Role}[/] [green]from[/] {Markup.Escape(settings.Email)}");
        var rolesDisplay = string.IsNullOrEmpty(player.Roles) ? "[grey](none)[/]" : FormatRoles(player.Roles);
        AnsiConsole.MarkupLine($"Current roles: {rolesDisplay}");
        return 0;
    }

    private static List<string> ParseRoles(string? roles)
    {
        if (string.IsNullOrEmpty(roles))
            return new List<string>();
        return roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    private static string FormatRoles(string roles)
    {
        var parts = roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var formatted = parts.Select(r => r switch
        {
            "Administrator" => "[red]Administrator[/]",
            "GameMaster" => "[blue]GameMaster[/]",
            "Player" => "[green]Player[/]",
            _ => Markup.Escape(r)
        });
        return string.Join(", ", formatted);
    }
}

public class EnableCommand : AsyncCommand<EmailSettings>
{
    private readonly IPlayerDal _dal;

    public EnableCommand(IPlayerDal dal) => _dal = dal;

    public override async Task<int> ExecuteAsync(CommandContext context, EmailSettings settings)
    {
        var player = await _dal.GetPlayerByEmailAsync(settings.Email);

        if (player == null)
        {
            AnsiConsole.MarkupLine($"[red]User not found:[/] {Markup.Escape(settings.Email)}");
            return 1;
        }

        if (player.IsEnabled)
        {
            AnsiConsole.MarkupLine($"[yellow]User is already enabled:[/] {Markup.Escape(settings.Email)}");
            return 0;
        }

        player.IsEnabled = true;
        await _dal.SavePlayerAsync(player);

        AnsiConsole.MarkupLine($"[green]Enabled user:[/] {Markup.Escape(settings.Email)}");
        return 0;
    }
}

public class DisableCommand : AsyncCommand<EmailSettings>
{
    private readonly IPlayerDal _dal;

    public DisableCommand(IPlayerDal dal) => _dal = dal;

    public override async Task<int> ExecuteAsync(CommandContext context, EmailSettings settings)
    {
        var player = await _dal.GetPlayerByEmailAsync(settings.Email);

        if (player == null)
        {
            AnsiConsole.MarkupLine($"[red]User not found:[/] {Markup.Escape(settings.Email)}");
            return 1;
        }

        if (!player.IsEnabled)
        {
            AnsiConsole.MarkupLine($"[yellow]User is already disabled:[/] {Markup.Escape(settings.Email)}");
            return 0;
        }

        player.IsEnabled = false;
        await _dal.SavePlayerAsync(player);

        AnsiConsole.MarkupLine($"[green]Disabled user:[/] {Markup.Escape(settings.Email)}");
        return 0;
    }
}

public class DeleteCommand : AsyncCommand<EmailSettings>
{
    private readonly IPlayerDal _playerDal;
    private readonly ICharacterDal _characterDal;

    public DeleteCommand(IPlayerDal playerDal, ICharacterDal characterDal)
    {
        _playerDal = playerDal;
        _characterDal = characterDal;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, EmailSettings settings)
    {
        var player = await _playerDal.GetPlayerByEmailAsync(settings.Email);
        if (player == null)
        {
            AnsiConsole.MarkupLine($"[red]User not found:[/] {Markup.Escape(settings.Email)}");
            return 1;
        }

        // Confirm deletion
        var confirm = AnsiConsole.Confirm($"[yellow]Delete user {Markup.Escape(settings.Email)} and all their data?[/]", false);
        if (!confirm)
        {
            AnsiConsole.MarkupLine("[grey]Cancelled[/]");
            return 0;
        }

        // Delete all characters
        var characters = await _characterDal.GetCharactersAsync(player.Id);
        foreach (var character in characters)
        {
            await _characterDal.DeleteCharacterAsync(character.Id);
            AnsiConsole.MarkupLine($"[grey]Deleted character:[/] {Markup.Escape(character.Name)}");
        }

        // Delete player
        await _playerDal.DeletePlayerAsync(player.Id);

        AnsiConsole.MarkupLine($"[green]Deleted user:[/] {Markup.Escape(settings.Email)}");
        if (characters.Count > 0)
        {
            AnsiConsole.MarkupLine($"[grey]Deleted {characters.Count} character(s)[/]");
        }
        return 0;
    }
}

public class SetPasswordCommand : AsyncCommand<PasswordSettings>
{
    private readonly IPlayerDal _dal;

    public SetPasswordCommand(IPlayerDal dal) => _dal = dal;

    public override async Task<int> ExecuteAsync(CommandContext context, PasswordSettings settings)
    {
        var player = await _dal.GetPlayerByEmailAsync(settings.Email);

        if (player == null)
        {
            AnsiConsole.MarkupLine($"[red]User not found:[/] {Markup.Escape(settings.Email)}");
            return 1;
        }

        // Get password
        var password = settings.Password;
        if (string.IsNullOrEmpty(password))
        {
            password = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter new password:")
                    .Secret());

            var confirm = AnsiConsole.Prompt(
                new TextPrompt<string>("Confirm new password:")
                    .Secret());

            if (password != confirm)
            {
                AnsiConsole.MarkupLine("[red]Passwords do not match[/]");
                return 1;
            }
        }

        // Update password
        player.Salt = BCrypt.Net.BCrypt.GenerateSalt(12);
        player.HashedPassword = BCrypt.Net.BCrypt.HashPassword(password, player.Salt);
        await _dal.SavePlayerAsync(player);

        AnsiConsole.MarkupLine($"[green]Password updated for:[/] {Markup.Escape(settings.Email)}");
        return 0;
    }
}

// === Skill Import/Export Commands ===

public class FileSettings : CommonSettings
{
    [CommandArgument(0, "<FILE>")]
    [Description("Path to the file")]
    public string FilePath { get; set; } = string.Empty;
}

public class ImportSkillsSettings : FileSettings
{
    [CommandOption("--dry-run")]
    [Description("Validate the file without saving to database")]
    public bool DryRun { get; set; }
}

public class ImportSkillsCommand : AsyncCommand<ImportSkillsSettings>
{
    private readonly ISkillDal _dal;

    public ImportSkillsCommand(ISkillDal dal) => _dal = dal;

    public override ValidationResult Validate(CommandContext context, ImportSkillsSettings settings)
    {
        if (!File.Exists(settings.FilePath))
        {
            return ValidationResult.Error($"File not found: {settings.FilePath}");
        }

        var ext = Path.GetExtension(settings.FilePath).ToLowerInvariant();
        if (ext != ".csv" && ext != ".tsv")
        {
            return ValidationResult.Error("File must have .csv or .tsv extension");
        }

        return ValidationResult.Success();
    }

    public override async Task<int> ExecuteAsync(CommandContext context, ImportSkillsSettings settings)
    {
        var ext = Path.GetExtension(settings.FilePath).ToLowerInvariant();
        var delimiter = ext == ".tsv" ? "\t" : ",";

        // Read and parse the file
        List<Skill> skills;
        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter,
                HeaderValidated = null,
                MissingFieldFound = null,
                TrimOptions = TrimOptions.Trim
            };

            using var reader = new StreamReader(settings.FilePath);
            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<SkillCsvMap>();

            skills = csv.GetRecords<Skill>().ToList();
        }
        catch (CsvHelperException ex)
        {
            AnsiConsole.MarkupLine($"[red]Error parsing file:[/] {Markup.Escape(ex.Message)}");
            return 1;
        }

        if (skills.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]File is empty or contains only headers[/]");
            return 1;
        }

        // Validate all skills
        var errors = SkillValidator.ValidateSkills(skills);
        if (errors.Count > 0)
        {
            AnsiConsole.MarkupLine($"[red]Validation failed with {errors.Count} error(s):[/]");
            AnsiConsole.WriteLine();
            foreach (var error in errors)
            {
                AnsiConsole.MarkupLine($"  Row {error.RowNumber}, [yellow]{error.ColumnName}[/]: {Markup.Escape(error.Message)}");
            }
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[red]No skills were imported. Please fix the errors and try again.[/]");
            return 1;
        }

        AnsiConsole.MarkupLine($"[green]Validated {skills.Count} skill(s)[/]");

        if (settings.DryRun)
        {
            AnsiConsole.MarkupLine("[yellow]Dry run - no changes made to database[/]");
            return 0;
        }

        // Get existing skills to track inserts vs updates
        var existingSkills = await _dal.GetAllSkillsAsync();
        var existingIds = existingSkills.Select(s => s.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

        int inserted = 0;
        int updated = 0;

        AnsiConsole.MarkupLine("[green]Importing skills...[/]");
        foreach (var skill in skills)
        {
            // Normalize attribute values to uppercase
            skill.PrimaryAttribute = skill.PrimaryAttribute.ToUpperInvariant();
            if (!string.IsNullOrWhiteSpace(skill.SecondaryAttribute))
                skill.SecondaryAttribute = skill.SecondaryAttribute.ToUpperInvariant();
            if (!string.IsNullOrWhiteSpace(skill.TertiaryAttribute))
                skill.TertiaryAttribute = skill.TertiaryAttribute.ToUpperInvariant();

            await _dal.SaveSkillAsync(skill);

            if (existingIds.Contains(skill.Id))
                updated++;
            else
                inserted++;
        }

        AnsiConsole.MarkupLine($"  [grey]Inserted:[/] {inserted} new skill(s)");
        AnsiConsole.MarkupLine($"  [grey]Updated:[/] {updated} existing skill(s)");
        AnsiConsole.MarkupLine("[green]Import complete![/]");

        return 0;
    }
}

public class ExportSkillsCommand : AsyncCommand<FileSettings>
{
    private readonly ISkillDal _dal;

    public ExportSkillsCommand(ISkillDal dal) => _dal = dal;

    public override ValidationResult Validate(CommandContext context, FileSettings settings)
    {
        var ext = Path.GetExtension(settings.FilePath).ToLowerInvariant();
        if (ext != ".csv" && ext != ".tsv")
        {
            return ValidationResult.Error("File must have .csv or .tsv extension");
        }

        var dir = Path.GetDirectoryName(settings.FilePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            return ValidationResult.Error($"Directory does not exist: {dir}");
        }

        return ValidationResult.Success();
    }

    public override async Task<int> ExecuteAsync(CommandContext context, FileSettings settings)
    {
        var ext = Path.GetExtension(settings.FilePath).ToLowerInvariant();
        var delimiter = ext == ".tsv" ? "\t" : ",";

        // Fetch existing skills from database
        var skills = await _dal.GetAllSkillsAsync();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter
        };

        using var writer = new StreamWriter(settings.FilePath);
        using var csv = new CsvWriter(writer, config);
        csv.Context.RegisterClassMap<SkillCsvMap>();

        // Write header
        csv.WriteHeader<Skill>();
        await csv.NextRecordAsync();

        // Write all skills
        foreach (var skill in skills.OrderBy(s => s.Category).ThenBy(s => s.Name))
        {
            csv.WriteRecord(skill);
            await csv.NextRecordAsync();
        }

        if (skills.Count > 0)
        {
            AnsiConsole.MarkupLine($"[green]Exported {skills.Count} skill(s) to:[/] {Markup.Escape(settings.FilePath)}");
        }
        else
        {
            AnsiConsole.MarkupLine($"[yellow]No skills in database. Created file with headers only:[/] {Markup.Escape(settings.FilePath)}");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Import validation requirements:[/]");
        AnsiConsole.MarkupLine($"  [grey]Valid categories:[/] {SkillValidator.GetValidCategoriesMessage()}");
        AnsiConsole.MarkupLine($"  [grey]Valid attributes:[/] {SkillValidator.GetValidAttributesMessage()}");

        return 0;
    }
}
