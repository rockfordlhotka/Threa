namespace GameMechanics.Time.Calendars;

/// <summary>
/// The Common Calendar (Calendar of The Seven), used across most of the Southern Continent.
/// 364-day year, 13 months of 28 days, 7-day weeks named for The Seven.
/// Epoch 0 = midnight on Day 1 of Seedmont, Year 1 AR (Age of Rebirth).
/// </summary>
public class CommonCalendar : ThreanCalendarBase
{
    public override string Id => "common";
    public override string Name => "Common Calendar";
    public override string Theme => "fantasy";
    public override bool IsThemeDefault => true;
    public override long EpochOffset => 0;
    public override string YearSuffix => "AR";

    public override string[] DayNames =>
        ["Melday", "Abday", "Ozday", "Lothday", "Fathday", "Thralday", "Infday"];

    public override string[] MonthNamesFull =>
        ["Seedmont", "Floodmont", "Windmont", "Firemont", "Crownmont", "Blademont",
         "Scalemont", "Veilmont", "Rootmont", "Shadowmont", "Frostmont", "Dreammont", "Darkmont"];

    public override string[] MonthAbbreviations =>
        ["Sed", "Fld", "Wnd", "Fir", "Crw", "Bld", "Scl", "Vel", "Rot", "Shd", "Frs", "Drm", "Drk"];

    public override string CheatSheet =>
        """
        ## Common Calendar Reference

        ### The Week
        Melday, Abday, Ozday, Lothday, Fathday, Thralday, Infday

        ### The Year (364 days = 52 weeks = 13 months = 4 seasons)
        | Season | Months | Days |
        |---|---|---|
        | Spring | Seedmont, Floodmont, Windmont | 1–91 |
        | Summer | Firemont, Crownmont, Blademont | 92–182 |
        | Autumn | Scalemont, Veilmont, Rootmont | 183–273 |
        | Winter | Shadowmont, Frostmont, Dreammont, Darkmont | 274–364 |

        ### Moon Cycles
        - Belerdar (white): 7 days — Full every Lothday
        - Pherdar (violet): 28 days — Full on day 15 of each month
        - Solendar (green): 91 days — Full mid-season (Days 46, 137, 228, 319)

        ### Valdris Days (magic enhanced)
        Days 1, 73, 146, 219, 292

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
}
