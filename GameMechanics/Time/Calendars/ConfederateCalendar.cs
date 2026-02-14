namespace GameMechanics.Time.Calendars;

/// <summary>
/// Confederate Standard Calendar (CSC) for sci-fi settings.
/// Base-10 time system anchored to the hydrogen hyperfine transition frequency.
/// 1 tick ≈ 7.04 seconds. Civil day = 12,500 ticks (~24.4 hours).
/// Year = 4,500,000 ticks (4.5 megaticks, ~366 Earth days).
/// Date format: year.pentade.week (e.g., "102.3.2").
/// </summary>
public class ConfederateCalendar : IGameCalendar
{
    public string Id => "confederate";
    public string Name => "Confederate Standard";
    public string Theme => "scifi";
    public bool IsThemeDefault => true;

    // Confederate time converted to seconds (1 tick = 7.04 seconds)
    private const long SecondsPerWatch = 17_600;       // 2,500 ticks (~4.9 hours)
    private const long SecondsPerDay = 88_000;         // 12,500 ticks (civil day, ~24.4 hours)
    private const long SecondsPerWeek = 704_000;       // 100,000 ticks (decamyritick, ~8.1 days)
    private const long SecondsPerPentade = 3_520_000;  // 500,000 ticks (~40.5 days)
    private const long SecondsPerYear = 31_680_000;    // 4,500,000 ticks (4.5 megaticks, ~366 days)

    public static readonly string[] PentadeNames =
        ["First", "Second", "Third", "Fourth", "Fifth", "Sixth", "Seventh", "Eighth", "Ninth"];

    public static readonly string[] WatchNames =
        ["First Watch", "Second Watch", "Third Watch", "Fourth Watch", "Fifth Watch"];

    public string CheatSheet =>
        """
        ## Confederate Time Reference

        ### Combat & Action Scale
        | Game Mechanic | Confederate "Close Enough" |
        |---|---|
        | 1 round (3 sec) | ~4 pips |
        | 2 rounds (6 sec) | ~1 tick |
        | 1 minute (20 rounds) | ~1 dec |
        | 1 turn (10 min) | ~1 mark |
        | 1 hour (60 min) | ~half a kat |

        ### Travel & Planning Scale
        | Game Mechanic | Confederate "Close Enough" |
        |---|---|
        | 2 hours | 1 kat |
        | 8 hours | 4 kats (full work shift) |
        | 1 day (24 hours) | 1 civil day (~24.4 hrs) |
        | 1 week (7 days) | ~1 decamyritick (~8 days) |
        | 1 month (30 days) | ~1 pentade/turn (~40 days) |
        | 1 year (365 days) | ~1 CSY (~366 days) |

        ### NPC Says → GM Thinks
        | NPC says... | GM thinks... |
        |---|---|
        | "Give me a pip" | Under 1 sec — 1 round max |
        | "Give me a tick" | 2-3 rounds |
        | "A few ticks" | ~15-20 sec (5-7 rounds) |
        | "A dec or two" | 1-2 min (20-40 rounds) |
        | "A few marks" | 30 min to 1 hour |
        | "Half a kat" | ~1 hour |
        | "A kat" | ~2 hours |

        ### Important Mismatches
        - Confederate "week" (decamyritick) is **~8 days**, not 7
        - A "turn" (pentade) is **~40 days**, longer than an Earth month
        """;

    public string FormatDateTime(long epochSeconds)
    {
        var (year, pentade, week, day, watch) = DecomposeDateTime(epochSeconds);
        return $"CSY {year}, {PentadeNames[pentade]} Pentade, Week {week + 1}, Day {day + 1} - {WatchNames[watch]}";
    }

    public string FormatDate(long epochSeconds)
    {
        var (year, pentade, week, day, _) = DecomposeDateTime(epochSeconds);
        return $"CSY {year}, {PentadeNames[pentade]} Pentade, Week {week + 1}, Day {day + 1}";
    }

    public string FormatCompact(long epochSeconds)
    {
        var (year, pentade, week, day, watch) = DecomposeDateTime(epochSeconds);
        return $"{year}.{pentade + 1}.{week + 1}/{day + 1} W{watch + 1}";
    }

    public bool TryParse(string input, out long epochSeconds)
    {
        epochSeconds = 0;
        return false;
    }

    /// <summary>
    /// Decomposes epoch seconds into Confederate calendar fields.
    /// All fields except year are 0-indexed (pentade 0 = First Pentade, etc.).
    /// </summary>
    public static (long year, long pentade, long week, long day, long watch) DecomposeDateTime(long epochSeconds)
    {
        long remaining = epochSeconds;

        long year = remaining / SecondsPerYear;
        remaining %= SecondsPerYear;

        long pentade = remaining / SecondsPerPentade;
        remaining %= SecondsPerPentade;

        long week = remaining / SecondsPerWeek;
        remaining %= SecondsPerWeek;

        long day = remaining / SecondsPerDay;
        remaining %= SecondsPerDay;

        long watch = remaining / SecondsPerWatch;

        return (year, pentade, week, day, watch);
    }

    /// <summary>
    /// Composes Confederate calendar fields into epoch seconds.
    /// All fields except year are 0-indexed (pentade 0 = First Pentade, etc.).
    /// </summary>
    public static long ComposeDateTime(long year, long pentade, long week, long day, long watch)
    {
        return year * SecondsPerYear
            + pentade * SecondsPerPentade
            + week * SecondsPerWeek
            + day * SecondsPerDay
            + watch * SecondsPerWatch;
    }
}
