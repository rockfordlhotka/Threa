namespace GameMechanics.Time.Calendars;

/// <summary>
/// Standard fantasy calendar with simplified 30-day months, 12-month years.
/// Matches the existing DefaultGameTimeFormatService epoch→calendar logic.
/// </summary>
public class StandardFantasyCalendar : IGameCalendar
{
    public string Id => "standard-fantasy";
    public string Name => "Standard Fantasy";
    public string Theme => "fantasy";
    public bool IsThemeDefault => false;

    // Calendar constants (same as DefaultGameTimeFormatService)
    private const long SecondsPerMinute = 60;
    private const long SecondsPerHour = 3600;
    private const long SecondsPerDay = 86400;
    private const long SecondsPerMonth = 2592000;    // 30 * 24 * 60 * 60
    private const long SecondsPerYear = 31104000;    // 12 * 30 * 24 * 60 * 60

    public string FormatDateTime(long epochSeconds)
    {
        var (years, months, days, hours, minutes, seconds) = DecomposeDateTime(epochSeconds);
        return $"Day {days + 1} of Month {months + 1}, Year {years}, {hours:D2}:{minutes:D2}:{seconds:D2}";
    }

    public string FormatDate(long epochSeconds)
    {
        var (years, months, days, _, _, _) = DecomposeDateTime(epochSeconds);
        return $"Day {days + 1} of Month {months + 1}, Year {years}";
    }

    public string FormatCompact(long epochSeconds)
    {
        var (years, months, days, hours, minutes, seconds) = DecomposeDateTime(epochSeconds);
        return $"{years}/{months + 1}/{days + 1} {hours:D2}:{minutes:D2}:{seconds:D2}";
    }

    public string CheatSheet =>
        """
        ## Time Reference
        | Game Mechanic | Calendar Time |
        |---|---|
        | 1 Round | 3 seconds |
        | 1 Minute | 20 rounds (60 seconds) |
        | 1 Turn | 10 minutes (200 rounds) |
        | 1 Hour | 6 turns (60 minutes) |
        | 1 Day | 24 hours |
        | 1 Month | 30 days |
        | 1 Year | 12 months (360 days) |

        ## Recovery Rates
        | Recovery | Timing |
        |---|---|
        | FAT | 1 per round (3 sec) |
        | AP | FAT/4 per round (min 1) |
        | VIT | 1 per hour |
        | Wounds | 1 per 4 hours (resting) |
        """;

    public bool TryParse(string input, out long epochSeconds)
    {
        // Deferred - placeholder implementation
        epochSeconds = 0;
        return false;
    }

    /// <summary>
    /// Decomposes epoch seconds into calendar fields.
    /// All fields except year are 0-indexed (month 0 = Month 1, day 0 = Day 1, etc.).
    /// </summary>
    public static (long year, long month, long day, long hour, long minute, long second) DecomposeDateTime(long epochSeconds)
    {
        long remaining = epochSeconds;

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

        return (years, months, days, hours, minutes, seconds);
    }

    /// <summary>
    /// Composes calendar fields into epoch seconds.
    /// All fields except year are 0-indexed (month 0 = Month 1, day 0 = Day 1, etc.).
    /// </summary>
    public static long ComposeDateTime(long year, long month, long day, long hour, long minute, long second)
    {
        return year * SecondsPerYear
            + month * SecondsPerMonth
            + day * SecondsPerDay
            + hour * SecondsPerHour
            + minute * SecondsPerMinute
            + second;
    }
}
