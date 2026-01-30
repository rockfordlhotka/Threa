using System.Collections.Generic;
using Threa.Dal.Dto;

namespace GameMechanics.Time;

/// <summary>
/// Default implementation of <see cref="IGameTimeFormatService"/> that provides
/// standard fantasy/medieval time formatting.
/// </summary>
public class DefaultGameTimeFormatService : IGameTimeFormatService
{
    #region Time Constants

    /// <inheritdoc />
    public int SecondsPerRound => 3;

    /// <inheritdoc />
    public int RoundsPerMinute => 20;

    /// <inheritdoc />
    public int RoundsPerHour => 1200;

    /// <inheritdoc />
    public long SecondsPerMinute => 60;

    /// <inheritdoc />
    public long SecondsPerHour => 3600;

    /// <inheritdoc />
    public long SecondsPerDay => 86400;

    // Calendar constants (simplified: 30 days/month, 12 months/year)
    private const long SecondsPerMonth = 2592000;    // 30 * 24 * 60 * 60
    private const long SecondsPerYear = 31104000;    // 12 * 30 * 24 * 60 * 60
    private const long SecondsPerWeek = 604800;      // 7 * 24 * 60 * 60

    #endregion

    #region Epoch-based Formatting

    /// <inheritdoc />
    public GameTimeComponents GetComponents(long totalSeconds)
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

    /// <inheritdoc />
    public string FormatFull(long totalSeconds)
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

    /// <inheritdoc />
    public string FormatCompact(long totalSeconds)
    {
        var c = GetComponents(totalSeconds);
        return $"{c.Years}/{c.Months + 1}/{c.Days + 1} {c.Hours:D2}:{c.Minutes:D2}:{c.Seconds:D2}";
    }

    /// <inheritdoc />
    public string FormatDuration(long totalSeconds)
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

    #endregion

    #region Round-based Formatting

    /// <inheritdoc />
    public string FormatRounds(int rounds)
    {
        if (rounds <= 0) return "0 seconds";

        int totalSeconds = rounds * SecondsPerRound;

        // Under 1 minute - show in rounds or seconds
        if (totalSeconds < 60)
        {
            return rounds == 1 ? "1 round" : $"{rounds} rounds";
        }

        // Under 1 hour - show in minutes
        if (totalSeconds < 3600)
        {
            int minutes = totalSeconds / 60;
            return minutes == 1 ? "1 minute" : $"{minutes} minutes";
        }

        // 1 hour or more
        int hours = totalSeconds / 3600;
        return hours == 1 ? "1 hour" : $"{hours} hours";
    }

    /// <inheritdoc />
    public string FormatRoundsDetailed(int rounds)
    {
        if (rounds <= 0) return "0 seconds";

        int totalSeconds = rounds * SecondsPerRound;

        if (totalSeconds < 60)
            return $"{totalSeconds} seconds";

        if (totalSeconds < 3600)
        {
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return seconds > 0 ? $"{minutes} min {seconds} sec" : $"{minutes} minutes";
        }

        int hours = totalSeconds / 3600;
        int remainingMinutes = (totalSeconds % 3600) / 60;
        return remainingMinutes > 0 ? $"{hours} hr {remainingMinutes} min" : $"{hours} hours";
    }

    #endregion

    #region DurationType-based Formatting

    /// <inheritdoc />
    public string FormatDuration(DurationType durationType, int value)
    {
        if (durationType == DurationType.Permanent || durationType == DurationType.UntilRemoved)
            return "Permanent";

        return durationType switch
        {
            DurationType.Rounds => $"{value} rnd{(value != 1 ? "s" : "")}",
            DurationType.Minutes => $"{value} min",
            DurationType.Hours => $"{value} hr{(value != 1 ? "s" : "")}",
            DurationType.Days => $"{value} day{(value != 1 ? "s" : "")}",
            DurationType.Weeks => $"{value} wk{(value != 1 ? "s" : "")}",
            _ => "Unknown"
        };
    }

    #endregion

    #region Progress Formatting

    /// <inheritdoc />
    public string FormatProgress(long elapsedSeconds, long totalSeconds)
    {
        elapsedSeconds = System.Math.Max(0, elapsedSeconds);

        // For short durations (< 10 minutes), show as rounds
        if (totalSeconds < 600)
        {
            var elapsedRounds = elapsedSeconds / SecondsPerRound;
            var totalRounds = totalSeconds / SecondsPerRound;
            return $"{elapsedRounds} / {totalRounds} rounds";
        }

        // For longer durations, show in appropriate calendar units
        if (totalSeconds < 3600) // < 1 hour, show minutes
        {
            var elapsedMin = elapsedSeconds / 60;
            var totalMin = totalSeconds / 60;
            return $"{elapsedMin} / {totalMin} minutes";
        }

        if (totalSeconds < SecondsPerDay) // < 1 day, show hours
        {
            var elapsedHours = elapsedSeconds / 3600;
            var totalHours = totalSeconds / 3600;
            return $"{elapsedHours} / {totalHours} hours";
        }

        // >= 1 day
        var elapsedDays = elapsedSeconds / SecondsPerDay;
        var totalDays = totalSeconds / SecondsPerDay;
        return $"{elapsedDays} / {totalDays} days";
    }

    /// <inheritdoc />
    public string FormatRemaining(long remainingSeconds)
    {
        // Show rounds only for short combat durations (under 1 minute / 20 rounds)
        if (remainingSeconds < 60)
        {
            var rounds = (int)(remainingSeconds / SecondsPerRound);
            return $"{rounds} rnd";
        }

        // Show minutes for durations under 1 hour
        if (remainingSeconds < 3600)
        {
            var minutes = remainingSeconds / 60;
            return $"{minutes}m";
        }

        // Show hours for durations under 1 day
        if (remainingSeconds < SecondsPerDay)
        {
            var hours = remainingSeconds / 3600;
            var minutes = (remainingSeconds % 3600) / 60;
            return minutes > 0 ? $"{hours}h {minutes}m" : $"{hours}h";
        }

        // Show days (with weeks for 7+ days)
        var days = remainingSeconds / SecondsPerDay;
        if (days >= 7 && days % 7 == 0)
        {
            var weeks = days / 7;
            return $"{weeks}w";
        }
        if (days >= 7)
        {
            var weeks = days / 7;
            var remainingDays = days % 7;
            return remainingDays > 0 ? $"{weeks}w {remainingDays}d" : $"{weeks}w";
        }

        var remainingHours = (remainingSeconds % SecondsPerDay) / 3600;
        return remainingHours > 0 ? $"{days}d {remainingHours}h" : $"{days}d";
    }

    #endregion

    #region Conversion Helpers

    /// <inheritdoc />
    public long RoundsToSeconds(int rounds)
    {
        return rounds * SecondsPerRound;
    }

    /// <inheritdoc />
    public int SecondsToRounds(long seconds)
    {
        return (int)(seconds / SecondsPerRound);
    }

    /// <inheritdoc />
    public long? DurationToSeconds(DurationType durationType, int value)
    {
        return durationType switch
        {
            DurationType.Rounds => value * SecondsPerRound,
            DurationType.Minutes => value * SecondsPerMinute,
            DurationType.Hours => value * SecondsPerHour,
            DurationType.Days => value * SecondsPerDay,
            DurationType.Weeks => value * SecondsPerWeek,
            DurationType.Permanent => null,
            DurationType.UntilRemoved => null,
            _ => null
        };
    }

    #endregion
}
