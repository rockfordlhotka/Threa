namespace GameMechanics.Time.Calendars;

/// <summary>
/// Imperial Calendar for sci-fi settings.
/// Based on the Imperial homeworld's astronomical cycles.
/// Uses the Founding Count (IF) for continuous dating, with 12 named months.
/// Imperial day = 24 bells (~25.1 Earth hours). Year = 375 Imperial days.
/// </summary>
public class ImperialCalendar : IGameCalendar
{
    public string Id => "imperial";
    public string Name => "Imperial Calendar";
    public string Theme => "scifi";
    public bool IsThemeDefault => false;

    long IGameCalendar.SecondsPerYear => SecondsPerYear;

    // Imperial time in seconds
    // 1 bell (Imperial hour) = 62.75 Earth minutes = 3,765 seconds
    // 1 Imperial day = 24 bells = 90,360 seconds (~25.1 Earth hours)
    private const long SecondsPerBell = 3_765;
    private const long SecondsPerDay = 90_360;
    private const int DaysPerYear = 375;
    private static readonly long SecondsPerYear = (long)DaysPerYear * SecondsPerDay; // 33,885,000

    private static readonly int[] MonthDays = [31, 31, 31, 31, 32, 31, 31, 31, 32, 31, 31, 32];

    public static readonly string[] MonthNames =
        ["Nisaren", "Ayren", "Simaran", "Tammaren", "Aburen", "Ullaren",
         "Tashren", "Arachen", "Kislaren", "Tebbaren", "Shabar", "Addaren"];

    private static readonly string[] MonthAbbreviations =
        ["Nis", "Ayr", "Sim", "Tam", "Abu", "Ull", "Tas", "Ara", "Kis", "Teb", "Sha", "Add"];

    private static readonly string[] WatchNames =
        ["Morning Watch", "Afternoon Watch", "Evening Watch", "Night Watch"];

    public string CheatSheet =>
        """
        ## Imperial Time Reference

        ### Combat & Action Scale
        | Game Mechanic | Imperial "Close Enough" |
        |---|---|
        | 1 round (3 sec) | 3 seconds |
        | 1 minute (20 rounds) | ~1 minute |
        | 1 turn (10 min) | ~10 minutes |
        | 30 min | 1 glass |
        | 1 hour (60 min) | ~1 bell (~63 min) |

        ### Travel & Planning Scale
        | Game Mechanic | Imperial "Close Enough" |
        |---|---|
        | 2 hours | 2 bells |
        | 6 hours | 1 watch (6 bells) |
        | 1 day (24 hours) | ~1 Imperial day (~25.1 hrs) |
        | 1 week (7 days) | ~7 days (no standard "week") |
        | 1 month (30 days) | ~1 Imperial month (~31 days) |
        | 1 year (365 days) | ~1 IF year (375 Imperial days) |

        ### NPC Says → GM Thinks
        | NPC says... | GM thinks... |
        |---|---|
        | "Give me a glass" | ~30 minutes |
        | "A bell or two" | 1-2 hours |
        | "By next watch" | ~6 hours |
        | "Mid-Tashren" | Middle of a specific month |
        | "First of Nisaren" | Imperial New Year |

        ### Important Mismatches
        - Imperial days are **~25.1 hours** (1 hour longer than Earth)
        - IF years are **375 Imperial days**
        - No standard "week" — use month fractions or counted days
        - Reign years reset with each Emperor
        """;

    public string FormatDateTime(long epochSeconds)
    {
        var (year, month, day, bell) = DecomposeDateTime(epochSeconds);
        int watch = bell / 6;
        return $"{day + 1} {MonthNames[month]}, IF {year} - {WatchNames[watch]}, {Ordinal(bell + 1)} Bell";
    }

    public string FormatDate(long epochSeconds)
    {
        var (year, month, day, _) = DecomposeDateTime(epochSeconds);
        return $"{day + 1} {MonthNames[month]}, IF {year}";
    }

    public string FormatCompact(long epochSeconds)
    {
        var (year, month, day, bell) = DecomposeDateTime(epochSeconds);
        return $"IF{year}.{MonthAbbreviations[month]}.{day + 1:D2} {bell + 1}b";
    }

    public bool TryParse(string input, out long epochSeconds)
    {
        epochSeconds = 0;
        return false;
    }

    /// <summary>
    /// Decomposes epoch seconds into Imperial calendar fields.
    /// Month, day, and bell are 0-indexed (month 0 = Nisaren, day 0 = 1st, bell 0 = 1st Bell).
    /// </summary>
    public static (long year, int month, int day, int bell) DecomposeDateTime(long epochSeconds)
    {
        long remaining = epochSeconds;

        long year = remaining / SecondsPerYear;
        remaining %= SecondsPerYear;
        if (remaining < 0) { year--; remaining += SecondsPerYear; }

        int dayOfYear = (int)(remaining / SecondsPerDay);
        remaining %= SecondsPerDay;

        // Convert day-of-year to month and day-within-month
        int month = 0;
        int dayInMonth = dayOfYear;
        for (int i = 0; i < MonthDays.Length; i++)
        {
            if (dayInMonth < MonthDays[i])
            {
                month = i;
                break;
            }
            dayInMonth -= MonthDays[i];
        }

        int bell = (int)(remaining / SecondsPerBell);

        return (year, month, dayInMonth, bell);
    }

    /// <summary>
    /// Composes Imperial calendar fields into epoch seconds.
    /// Month, day, and bell are 0-indexed (month 0 = Nisaren, day 0 = 1st, bell 0 = 1st Bell).
    /// </summary>
    public static long ComposeDateTime(long year, int month, int day, int bell)
    {
        int dayOfYear = 0;
        for (int i = 0; i < month; i++)
            dayOfYear += MonthDays[i];
        dayOfYear += day;

        return year * SecondsPerYear
            + (long)dayOfYear * SecondsPerDay
            + (long)bell * SecondsPerBell;
    }

    /// <summary>
    /// Gets the number of days in the specified month (0-indexed).
    /// </summary>
    public static int GetDaysInMonth(int month)
    {
        if (month < 0 || month >= MonthDays.Length) return 31;
        return MonthDays[month];
    }

    private static string Ordinal(long n)
    {
        var lastTwo = n % 100;
        var lastOne = n % 10;
        var suffix = (lastTwo >= 11 && lastTwo <= 13) ? "th"
            : lastOne == 1 ? "st"
            : lastOne == 2 ? "nd"
            : lastOne == 3 ? "rd"
            : "th";
        return $"{n}{suffix}";
    }
}
