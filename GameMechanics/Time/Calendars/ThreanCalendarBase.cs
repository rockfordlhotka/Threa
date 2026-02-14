namespace GameMechanics.Time.Calendars;

/// <summary>
/// Abstract base for all Threan calendars sharing the same astronomical constants:
/// 364-day year, 13 months of 28 days, 7-day weeks defined by three moons.
/// Concrete calendars differ only in names, date format, year suffix, and epoch offset.
/// </summary>
public abstract class ThreanCalendarBase : IGameCalendar
{
    // Shared astronomical constants
    public const long SecondsPerMinute = 60;
    public const long SecondsPerHour = 3_600;
    public const long SecondsPerDay = 86_400;
    public const long SecondsPerWeek = 604_800;       // 7 days
    public const long SecondsPerMonth = 2_419_200;    // 28 days
    public const long SecondsPerSeason = 7_862_400;   // 91 days
    public const long SecondsPerYear = 31_449_600;    // 364 days

    public const int DaysPerWeek = 7;
    public const int DaysPerMonth = 28;
    public const int MonthsPerYear = 13;
    public const int DaysPerYear = 364;

    // Abstract properties for concrete calendars
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string Theme { get; }
    public abstract bool IsThemeDefault { get; }
    public abstract string CheatSheet { get; }

    /// <summary>Seconds offset from the Common Calendar epoch (0 for Common).</summary>
    public abstract long EpochOffset { get; }

    /// <summary>7 day names, index 0 = first day of week.</summary>
    public abstract string[] DayNames { get; }

    /// <summary>13 month names, index 0 = first month.</summary>
    public abstract string[] MonthNamesFull { get; }

    /// <summary>13 three-letter month abbreviations.</summary>
    public abstract string[] MonthAbbreviations { get; }

    /// <summary>Year suffix (e.g. "AR", "YD").</summary>
    public abstract string YearSuffix { get; }

    /// <summary>
    /// Decomposes epoch seconds into Threan calendar fields.
    /// Year is 1-indexed (Year 1 = first year). Month and day are 0-indexed.
    /// Applies this calendar's epoch offset before decomposition.
    /// </summary>
    public (long year, int month, int day, int dayOfWeek, int hour, int minute, int second) DecomposeDateTime(long epochSeconds)
    {
        long adjusted = epochSeconds - EpochOffset;

        long totalDays = adjusted / SecondsPerDay;
        long timeOfDay = adjusted % SecondsPerDay;

        // Handle negative epochs (dates before this calendar's epoch)
        if (timeOfDay < 0)
        {
            totalDays--;
            timeOfDay += SecondsPerDay;
        }

        long yearZero = totalDays / DaysPerYear;
        long dayOfYear = totalDays % DaysPerYear;
        if (dayOfYear < 0)
        {
            yearZero--;
            dayOfYear += DaysPerYear;
        }

        long year = yearZero + 1; // 1-indexed
        int month = (int)(dayOfYear / DaysPerMonth);
        int day = (int)(dayOfYear % DaysPerMonth);

        // Day of week: 0 = first day name
        int dayOfWeek = (int)((totalDays % DaysPerWeek + DaysPerWeek) % DaysPerWeek);

        int hour = (int)(timeOfDay / SecondsPerHour);
        int minute = (int)((timeOfDay % SecondsPerHour) / SecondsPerMinute);
        int second = (int)(timeOfDay % SecondsPerMinute);

        return (year, month, day, dayOfWeek, hour, minute, second);
    }

    /// <summary>
    /// Composes Threan calendar fields into epoch seconds.
    /// Year is 1-indexed. Month and day are 0-indexed.
    /// Adds this calendar's epoch offset to the result.
    /// </summary>
    public long ComposeDateTime(long year, int month, int day, int hour, int minute, int second)
    {
        return (year - 1) * SecondsPerYear
            + (long)month * SecondsPerMonth
            + (long)day * SecondsPerDay
            + (long)hour * SecondsPerHour
            + (long)minute * SecondsPerMinute
            + second
            + EpochOffset;
    }

    public virtual string FormatDateTime(long epochSeconds)
    {
        var (year, month, day, dayOfWeek, hour, minute, second) = DecomposeDateTime(epochSeconds);
        return $"{DayNames[dayOfWeek]}, {day + 1} {MonthNamesFull[month]} {year} {YearSuffix}, {hour:D2}:{minute:D2}:{second:D2}";
    }

    public virtual string FormatDate(long epochSeconds)
    {
        var (year, month, day, _, _, _, _) = DecomposeDateTime(epochSeconds);
        return $"{day + 1} {MonthNamesFull[month]} {year} {YearSuffix}";
    }

    public virtual string FormatCompact(long epochSeconds)
    {
        var (year, month, day, _, hour, minute, second) = DecomposeDateTime(epochSeconds);
        return $"{year}{YearSuffix}/{MonthAbbreviations[month]}/{day + 1:D2} {hour:D2}:{minute:D2}:{second:D2}";
    }

    public bool TryParse(string input, out long epochSeconds)
    {
        epochSeconds = 0;
        return false;
    }
}
