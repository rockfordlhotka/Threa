using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using Threa.Dal;
using GameMechanics.Player;

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
