using Microsoft.Data.Sqlite;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using Threa.Dal;
using Threa.Dal.Sqlite;
using GameMechanics.Player;

namespace Threa.Admin;

class Program
{
    static int Main(string[] args)
    {
        var app = new CommandApp();
        app.Configure(config =>
        {
            config.SetApplicationName("threa-admin");
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
        });
        return app.Run(args);
    }
}

public class CommonSettings : CommandSettings
{
    [CommandOption("--db <PATH>")]
    [Description("Path to SQLite database")]
    [DefaultValue("threa.db")]
    public string DatabasePath { get; set; } = "threa.db";

    public IPlayerDal CreateDal()
    {
        var connectionString = $"Data Source={DatabasePath}";
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        return new PlayerDal(connection);
    }
}

public class EmailSettings : CommonSettings
{
    [CommandArgument(0, "<EMAIL>")]
    [Description("User email address")]
    public string Email { get; set; } = string.Empty;
}

public class RoleSettings : EmailSettings
{
    [CommandArgument(1, "<ROLE>")]
    [Description("Role name (Administrator, GameMaster, Player)")]
    public string Role { get; set; } = string.Empty;
}

public class ListCommand : AsyncCommand<CommonSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, CommonSettings settings)
    {
        var dal = settings.CreateDal();
        var players = await dal.GetAllPlayersAsync();

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
    public override async Task<int> ExecuteAsync(CommandContext context, EmailSettings settings)
    {
        var dal = settings.CreateDal();
        var player = await dal.GetPlayerByEmailAsync(settings.Email);

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
        var dal = settings.CreateDal();
        var player = await dal.GetPlayerByEmailAsync(settings.Email);

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
        await dal.SavePlayerAsync(player);

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
    public override async Task<int> ExecuteAsync(CommandContext context, RoleSettings settings)
    {
        var dal = settings.CreateDal();
        var player = await dal.GetPlayerByEmailAsync(settings.Email);

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
        await dal.SavePlayerAsync(player);

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
    public override async Task<int> ExecuteAsync(CommandContext context, EmailSettings settings)
    {
        var dal = settings.CreateDal();
        var player = await dal.GetPlayerByEmailAsync(settings.Email);

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
        await dal.SavePlayerAsync(player);

        AnsiConsole.MarkupLine($"[green]Enabled user:[/] {Markup.Escape(settings.Email)}");
        return 0;
    }
}

public class DisableCommand : AsyncCommand<EmailSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, EmailSettings settings)
    {
        var dal = settings.CreateDal();
        var player = await dal.GetPlayerByEmailAsync(settings.Email);

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
        await dal.SavePlayerAsync(player);

        AnsiConsole.MarkupLine($"[green]Disabled user:[/] {Markup.Escape(settings.Email)}");
        return 0;
    }
}
