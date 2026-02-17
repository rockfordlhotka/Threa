namespace GameMechanics.Time.Calendars;

/// <summary>
/// The Triumvirate Reckoning (Dark Calendar / Scinturean Reckoning).
/// Used within Scinture and its military surrounds.
/// Same astronomical structure as Common Calendar but different names and epoch.
/// Triumvirate Year 1 = Common Year 27 AR (offset of 26 years = 817,689,600 seconds).
/// </summary>
public class TriumvirateCalendar : ThreanCalendarBase
{
    /// <summary>26 full Common years = 26 × 31,449,600 seconds.</summary>
    public const long TriumvirateEpochOffset = 817_689_600;

    public override string Id => "triumvirate";
    public override string Name => "Triumvirate Reckoning";
    public override string Theme => "fantasy";
    public override bool IsThemeDefault => false;
    public override long EpochOffset => TriumvirateEpochOffset;
    public override string YearSuffix => "YD";

    public override string[] DayNames =>
        ["Throneday", "Chainday", "Flameday", "Duskday", "Veilday", "Crownday", "Ashday"];

    public override string[] MonthNamesFull =>
        ["The Awakening", "The Compact", "The Conspiracy", "The Betrayal", "The Imprisonment",
         "The Endurance", "The Whisper", "The Reckoning", "The Breaking", "The Return",
         "The Ascension", "The Dominion", "The Eternal"];

    public override string[] MonthAbbreviations =>
        ["Awk", "Cmp", "Cns", "Btr", "Imp", "End", "Whs", "Rec", "Brk", "Ret", "Asc", "Dom", "Etr"];

    public override string CheatSheet =>
        """
        ## Triumvirate Reckoning Reference

        ### The Week
        Throneday, Chainday, Flameday, Duskday, Veilday, Crownday, Ashday

        ### The Months
        The Awakening, The Compact, The Conspiracy, The Betrayal, The Imprisonment,
        The Endurance, The Whisper, The Reckoning, The Breaking, The Return,
        The Ascension, The Dominion, The Eternal

        ### Date Format
        `15 The Reckoning, 70 YD`

        ### Conversion
        Common Year = Triumvirate Year + 26 (e.g., 70 YD = 96 AR)

        ### Combat & Recovery
        | Game Mechanic | Time |
        |---|---|
        | 1 Round | 3 seconds |
        | 1 Minute | 20 rounds (60 sec) |
        | 1 Turn | 10 minutes |
        | 1 Hour | 6 turns |
        | FAT recovery | 1 per round |
        | AP recovery | FAT/4 per round (min 1) |
        | VIT recovery | 1 per hour |
        | Wound recovery | 1 per 4 hours (resting) |
        """;

    /// <summary>
    /// Triumvirate comma-separated style: "Veilday, 15 The Reckoning, 70 YD, 14:30:00"
    /// </summary>
    public override string FormatDateTime(long epochSeconds)
    {
        var (year, month, day, dayOfWeek, hour, minute, second) = DecomposeDateTime(epochSeconds);
        return $"{DayNames[dayOfWeek]}, {day + 1} {MonthNamesFull[month]}, {year} {YearSuffix}, {hour:D2}:{minute:D2}:{second:D2}";
    }

    /// <summary>
    /// Triumvirate comma-separated style: "15 The Reckoning, 70 YD"
    /// </summary>
    public override string FormatDate(long epochSeconds)
    {
        var (year, month, day, _, _, _, _) = DecomposeDateTime(epochSeconds);
        return $"{day + 1} {MonthNamesFull[month]}, {year} {YearSuffix}";
    }
}
