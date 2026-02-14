using System.Linq;

namespace GameMechanics;

/// <summary>
/// Constants and utilities for game settings (fantasy, sci-fi, etc.).
/// Eliminates magic strings across the codebase.
/// </summary>
public static class GameSettings
{
    public const string Fantasy = "fantasy";
    public const string SciFi = "scifi";
    public const string Default = Fantasy;

    public static readonly string[] All = [Fantasy, SciFi];

    public static bool IsValid(string? setting)
        => !string.IsNullOrEmpty(setting) && All.Contains(setting);

    public static string DisplayName(string? setting) => setting switch
    {
        Fantasy => "Fantasy (Arcanum)",
        SciFi => "Sci-Fi (Neon Circuit)",
        _ => setting ?? "Unknown"
    };
}
