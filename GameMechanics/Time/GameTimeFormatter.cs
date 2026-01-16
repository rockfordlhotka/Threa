using System.Collections.Generic;

namespace GameMechanics.Time;

/// <summary>
/// Formats game time (in seconds from epoch 0) into human-readable strings.
/// Uses a simplified calendar: 60 sec/min, 60 min/hour, 24 hours/day, 30 days/month, 12 months/year.
/// </summary>
public static class GameTimeFormatter
{
    // Time constants in seconds
    public const long SecondsPerMinute = 60;
    public const long SecondsPerHour = 3600;        // 60 * 60
    public const long SecondsPerDay = 86400;        // 24 * 60 * 60
    public const long SecondsPerMonth = 2592000;    // 30 * 24 * 60 * 60
    public const long SecondsPerYear = 31104000;    // 12 * 30 * 24 * 60 * 60

    /// <summary>
    /// Breaks down total seconds into individual time components.
    /// </summary>
    public static GameTimeComponents GetComponents(long totalSeconds)
    {
        long remaining = totalSeconds;

        long years = remaining / SecondsPerYear;
        remaining %= SecondsPerYear;

        long months = remaining / SecondsPerMonth;
        remaining %= SecondsPerMonth;

        long days = remaining / SecondsPerDay;
        remaining %= SecondsPerDay;

        long hours = remaining / SecondsPerHour;
        remaining %= SecondsPerHour;

        long minutes = remaining / SecondsPerMinute;
        long seconds = remaining % SecondsPerMinute;

        return new GameTimeComponents
        {
            Years = years,
            Months = months,
            Days = days,
            Hours = hours,
            Minutes = minutes,
            Seconds = seconds
        };
    }

    /// <summary>
    /// Formats time as "Y years, M months, D days, HH:MM:SS"
    /// Omits zero values for cleaner display.
    /// </summary>
    public static string FormatFull(long totalSeconds)
    {
        var c = GetComponents(totalSeconds);
        var parts = new List<string>();

        if (c.Years > 0) parts.Add($"{c.Years} year{(c.Years != 1 ? "s" : "")}");
        if (c.Months > 0) parts.Add($"{c.Months} month{(c.Months != 1 ? "s" : "")}");
        if (c.Days > 0) parts.Add($"{c.Days} day{(c.Days != 1 ? "s" : "")}");

        // Always show time portion
        parts.Add($"{c.Hours:D2}:{c.Minutes:D2}:{c.Seconds:D2}");

        return string.Join(", ", parts);
    }

    /// <summary>
    /// Formats time as "Y/M/D HH:MM:SS" in a compact form.
    /// </summary>
    public static string FormatCompact(long totalSeconds)
    {
        var c = GetComponents(totalSeconds);
        return $"{c.Years}/{c.Months + 1}/{c.Days + 1} {c.Hours:D2}:{c.Minutes:D2}:{c.Seconds:D2}";
    }

    /// <summary>
    /// Formats time showing only the most significant non-zero units.
    /// Good for displaying elapsed time or durations.
    /// </summary>
    public static string FormatDuration(long totalSeconds)
    {
        if (totalSeconds < 0) return "0 seconds";

        var c = GetComponents(totalSeconds);
        var parts = new List<string>();

        if (c.Years > 0) parts.Add($"{c.Years}y");
        if (c.Months > 0) parts.Add($"{c.Months}mo");
        if (c.Days > 0) parts.Add($"{c.Days}d");
        if (c.Hours > 0) parts.Add($"{c.Hours}h");
        if (c.Minutes > 0) parts.Add($"{c.Minutes}m");
        if (c.Seconds > 0 || parts.Count == 0) parts.Add($"{c.Seconds}s");

        return string.Join(" ", parts);
    }

    /// <summary>
    /// Parses a time string in format "Y/M/D H:M:S" or just total seconds.
    /// Returns total seconds from epoch 0.
    /// </summary>
    public static long ParseTimeString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return 0;

        // Try parsing as plain number first
        if (long.TryParse(input.Trim(), out long plainSeconds))
            return plainSeconds;

        // TODO: Add support for parsing "Y/M/D H:M:S" format in the future
        return 0;
    }
}

/// <summary>
/// Represents the individual components of a game time value.
/// </summary>
public class GameTimeComponents
{
    public long Years { get; init; }
    public long Months { get; init; }
    public long Days { get; init; }
    public long Hours { get; init; }
    public long Minutes { get; init; }
    public long Seconds { get; init; }

    /// <summary>
    /// Total seconds represented by these components.
    /// </summary>
    public long TotalSeconds =>
        Years * GameTimeFormatter.SecondsPerYear +
        Months * GameTimeFormatter.SecondsPerMonth +
        Days * GameTimeFormatter.SecondsPerDay +
        Hours * GameTimeFormatter.SecondsPerHour +
        Minutes * GameTimeFormatter.SecondsPerMinute +
        Seconds;
}
