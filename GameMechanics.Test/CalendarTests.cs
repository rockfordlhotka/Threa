using GameMechanics.Time;
using GameMechanics.Time.Calendars;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameMechanics.Test;

[TestClass]
public class CalendarTests
{
    #region StandardFantasyCalendar Tests

    [TestMethod]
    public void StandardFantasy_FormatCompact_MatchesExistingService()
    {
        var calendar = new StandardFantasyCalendar();
        var service = new DefaultGameTimeFormatService();

        // Test various epoch values
        long[] testValues = [0, 86400, 2592000, 31104000, 62208000, 63849600];

        foreach (var epoch in testValues)
        {
            var calendarResult = calendar.FormatCompact(epoch);
            var serviceResult = service.FormatCompact(epoch);
            Assert.AreEqual(serviceResult, calendarResult,
                $"FormatCompact mismatch at epoch {epoch}");
        }
    }

    [TestMethod]
    public void StandardFantasy_FormatCompact_Zero()
    {
        var calendar = new StandardFantasyCalendar();
        Assert.AreEqual("0/1/1 00:00:00", calendar.FormatCompact(0));
    }

    [TestMethod]
    public void StandardFantasy_FormatCompact_OneDay()
    {
        var calendar = new StandardFantasyCalendar();
        Assert.AreEqual("0/1/2 00:00:00", calendar.FormatCompact(86400));
    }

    [TestMethod]
    public void StandardFantasy_FormatDateTime_Zero()
    {
        var calendar = new StandardFantasyCalendar();
        Assert.AreEqual("Day 1 of Month 1, Year 0, 00:00:00", calendar.FormatDateTime(0));
    }

    [TestMethod]
    public void StandardFantasy_FormatDate_OneYear()
    {
        var calendar = new StandardFantasyCalendar();
        // 1 year = 31,104,000 seconds
        Assert.AreEqual("Day 1 of Month 1, Year 1", calendar.FormatDate(31104000));
    }

    [TestMethod]
    public void StandardFantasy_Properties()
    {
        var calendar = new StandardFantasyCalendar();
        Assert.AreEqual("standard-fantasy", calendar.Id);
        Assert.AreEqual("Standard Fantasy", calendar.Name);
        Assert.AreEqual("fantasy", calendar.Theme);
        Assert.IsFalse(calendar.IsThemeDefault); // CommonCalendar is now the fantasy default
    }

    [TestMethod]
    public void StandardFantasy_TryParse_ReturnsFalse()
    {
        var calendar = new StandardFantasyCalendar();
        Assert.IsFalse(calendar.TryParse("any input", out var result));
        Assert.AreEqual(0, result);
    }

    #endregion

    #region ConfederateCalendar Tests

    [TestMethod]
    public void Confederate_Properties()
    {
        var calendar = new ConfederateCalendar();
        Assert.AreEqual("confederate", calendar.Id);
        Assert.AreEqual("Confederate Standard", calendar.Name);
        Assert.AreEqual("scifi", calendar.Theme);
        Assert.IsTrue(calendar.IsThemeDefault);
    }

    [TestMethod]
    public void Confederate_FormatCompact_Zero()
    {
        var calendar = new ConfederateCalendar();
        // Year 0, Pentade 1, Week 1, Day 1, Watch 1
        Assert.AreEqual("0.1.1/1 W1 T0", calendar.FormatCompact(0));
    }

    [TestMethod]
    public void Confederate_FormatDateTime_Zero()
    {
        var calendar = new ConfederateCalendar();
        Assert.AreEqual(
            "CSY 0, First Pentade, Week 1, Day 1 - First Watch, Tick 0",
            calendar.FormatDateTime(0));
    }

    [TestMethod]
    public void Confederate_FormatDate_Zero()
    {
        var calendar = new ConfederateCalendar();
        Assert.AreEqual(
            "CSY 0, First Pentade, Week 1, Day 1",
            calendar.FormatDate(0));
    }

    [TestMethod]
    public void Confederate_FormatDateTime_OneCivilDay()
    {
        var calendar = new ConfederateCalendar();
        // 1 civil day = 88,000 seconds (12,500 ticks)
        Assert.AreEqual(
            "CSY 0, First Pentade, Week 1, Day 2 - First Watch, Tick 0",
            calendar.FormatDateTime(88_000));
    }

    [TestMethod]
    public void Confederate_OneEarthDay_StillDay1()
    {
        var calendar = new ConfederateCalendar();
        // 86,400 Earth seconds < 88,000 Confederate civil day
        // Should still be Day 1, in the Fifth Watch
        var result = calendar.FormatDate(86_400);
        Assert.AreEqual("CSY 0, First Pentade, Week 1, Day 1", result);
    }

    [TestMethod]
    public void Confederate_FormatCompact_OneWeek()
    {
        var calendar = new ConfederateCalendar();
        // 1 decamyritick (week) = 704,000 seconds
        Assert.AreEqual("0.1.2/1 W1 T0", calendar.FormatCompact(704_000));
    }

    [TestMethod]
    public void Confederate_FormatDate_OnePentade()
    {
        var calendar = new ConfederateCalendar();
        // 1 pentade = 3,520,000 seconds
        Assert.AreEqual(
            "CSY 0, Second Pentade, Week 1, Day 1",
            calendar.FormatDate(3_520_000));
    }

    [TestMethod]
    public void Confederate_FormatCompact_OneYear()
    {
        var calendar = new ConfederateCalendar();
        // 1 CSY = 31,680,000 seconds
        Assert.AreEqual("1.1.1/1 W1 T0", calendar.FormatCompact(31_680_000));
    }

    [TestMethod]
    public void Confederate_WatchProgression()
    {
        var calendar = new ConfederateCalendar();
        // Each watch = 17,600 seconds
        Assert.AreEqual("0.1.1/1 W1 T0", calendar.FormatCompact(0));
        Assert.AreEqual("0.1.1/1 W2 T0", calendar.FormatCompact(17_600));
        Assert.AreEqual("0.1.1/1 W3 T0", calendar.FormatCompact(35_200));
        Assert.AreEqual("0.1.1/1 W4 T0", calendar.FormatCompact(52_800));
        Assert.AreEqual("0.1.1/1 W5 T0", calendar.FormatCompact(70_400));
    }

    [TestMethod]
    public void Confederate_NinePentadesPerYear()
    {
        var calendar = new ConfederateCalendar();
        // Ninth pentade starts at 8 * 3,520,000 = 28,160,000
        Assert.AreEqual(
            "CSY 0, Ninth Pentade, Week 1, Day 1",
            calendar.FormatDate(28_160_000));
    }

    [TestMethod]
    public void Confederate_TryParse_ReturnsFalse()
    {
        var calendar = new ConfederateCalendar();
        Assert.IsFalse(calendar.TryParse("any input", out var result));
        Assert.AreEqual(0, result);
    }

    #endregion

    #region ImperialCalendar Tests

    [TestMethod]
    public void Imperial_Properties()
    {
        var calendar = new ImperialCalendar();
        Assert.AreEqual("imperial", calendar.Id);
        Assert.AreEqual("Imperial Calendar", calendar.Name);
        Assert.AreEqual("scifi", calendar.Theme);
        Assert.IsFalse(calendar.IsThemeDefault);
    }

    [TestMethod]
    public void Imperial_FormatCompact_Zero()
    {
        var calendar = new ImperialCalendar();
        // 1 Nisaren, IF 0, 1st Bell
        Assert.AreEqual("IF0.Nis.01 1b00m", calendar.FormatCompact(0));
    }

    [TestMethod]
    public void Imperial_FormatDateTime_Zero()
    {
        var calendar = new ImperialCalendar();
        Assert.AreEqual(
            "1 Nisaren, IF 0 - Morning Watch, 1st Bell, 0 min",
            calendar.FormatDateTime(0));
    }

    [TestMethod]
    public void Imperial_FormatDate_Zero()
    {
        var calendar = new ImperialCalendar();
        Assert.AreEqual("1 Nisaren, IF 0", calendar.FormatDate(0));
    }

    [TestMethod]
    public void Imperial_FormatDateTime_OneImperialDay()
    {
        var calendar = new ImperialCalendar();
        // 1 Imperial day = 90,360 seconds
        Assert.AreEqual(
            "2 Nisaren, IF 0 - Morning Watch, 1st Bell, 0 min",
            calendar.FormatDateTime(90_360));
    }

    [TestMethod]
    public void Imperial_OneEarthDay_StillDay1()
    {
        var calendar = new ImperialCalendar();
        // 86,400 Earth seconds < 90,360 Imperial day
        // Should still be 1 Nisaren, in the Night Watch
        // Bell: 86400 / 3765 = 22 (0-indexed) → 23rd bell
        // Remaining in bell: 86400 - 22*3765 = 3570 seconds → 3570/60 = 59 min
        Assert.AreEqual(
            "1 Nisaren, IF 0 - Night Watch, 23rd Bell, 59 min",
            calendar.FormatDateTime(86_400));
    }

    [TestMethod]
    public void Imperial_FormatDate_SecondMonth()
    {
        var calendar = new ImperialCalendar();
        // Nisaren has 31 days. First day of Ayren = day 31 (0-indexed)
        // Epoch: 31 * 90,360 = 2,801,160
        Assert.AreEqual("1 Ayren, IF 0", calendar.FormatDate(2_801_160));
    }

    [TestMethod]
    public void Imperial_FormatDate_Aburen()
    {
        var calendar = new ImperialCalendar();
        // Aburen (month 5) starts at day: 31+31+31+31 = 124 (0-indexed)
        // Epoch: 124 * 90,360 = 11,204,640
        Assert.AreEqual("1 Aburen, IF 0", calendar.FormatDate(11_204_640));
    }

    [TestMethod]
    public void Imperial_Aburen_Has32Days()
    {
        var calendar = new ImperialCalendar();
        // Last day of Aburen (32nd day) = day index 124 + 31 = 155 (0-indexed)
        // Epoch: 155 * 90,360 = 14,005,800
        Assert.AreEqual("32 Aburen, IF 0", calendar.FormatDate(14_005_800));

        // Next day should be 1 Ullaren
        Assert.AreEqual("1 Ullaren, IF 0", calendar.FormatDate(14_005_800 + 90_360));
    }

    [TestMethod]
    public void Imperial_FormatCompact_OneYear()
    {
        var calendar = new ImperialCalendar();
        // 1 IF year = 375 * 90,360 = 33,885,000 seconds
        Assert.AreEqual("IF1.Nis.01 1b00m", calendar.FormatCompact(33_885_000));
    }

    [TestMethod]
    public void Imperial_WatchProgression()
    {
        var calendar = new ImperialCalendar();
        // Each bell = 3,765 seconds. Watch changes every 6 bells.
        Assert.AreEqual(
            "1 Nisaren, IF 0 - Morning Watch, 1st Bell, 0 min",
            calendar.FormatDateTime(0));
        Assert.AreEqual(
            "1 Nisaren, IF 0 - Afternoon Watch, 7th Bell, 0 min",
            calendar.FormatDateTime(3_765 * 6));
        Assert.AreEqual(
            "1 Nisaren, IF 0 - Evening Watch, 13th Bell, 0 min",
            calendar.FormatDateTime(3_765 * 12));
        Assert.AreEqual(
            "1 Nisaren, IF 0 - Night Watch, 19th Bell, 0 min",
            calendar.FormatDateTime(3_765 * 18));
    }

    [TestMethod]
    public void Imperial_Ordinal_Suffixes()
    {
        var calendar = new ImperialCalendar();
        // 1st, 2nd, 3rd, 4th bells
        Assert.IsTrue(calendar.FormatDateTime(0).Contains("1st Bell"));
        Assert.IsTrue(calendar.FormatDateTime(3_765).Contains("2nd Bell"));
        Assert.IsTrue(calendar.FormatDateTime(3_765 * 2).Contains("3rd Bell"));
        Assert.IsTrue(calendar.FormatDateTime(3_765 * 3).Contains("4th Bell"));
        // 11th, 12th, 13th (teen exceptions)
        Assert.IsTrue(calendar.FormatDateTime(3_765 * 10).Contains("11th Bell"));
        Assert.IsTrue(calendar.FormatDateTime(3_765 * 11).Contains("12th Bell"));
        Assert.IsTrue(calendar.FormatDateTime(3_765 * 12).Contains("13th Bell"));
        // 21st, 22nd, 23rd
        Assert.IsTrue(calendar.FormatDateTime(3_765 * 20).Contains("21st Bell"));
        Assert.IsTrue(calendar.FormatDateTime(3_765 * 21).Contains("22nd Bell"));
        Assert.IsTrue(calendar.FormatDateTime(3_765 * 22).Contains("23rd Bell"));
    }

    [TestMethod]
    public void Imperial_DifferentFromConfederate()
    {
        var imperial = new ImperialCalendar();
        var confederate = new ConfederateCalendar();

        long testEpoch = 86400 * 100; // 100 days
        Assert.AreNotEqual(
            imperial.FormatCompact(testEpoch),
            confederate.FormatCompact(testEpoch),
            "Imperial and Confederate should produce different output");
    }

    [TestMethod]
    public void Imperial_TryParse_ReturnsFalse()
    {
        var calendar = new ImperialCalendar();
        Assert.IsFalse(calendar.TryParse("any input", out var result));
        Assert.AreEqual(0, result);
    }

    #endregion

    #region CheatSheet Tests

    [TestMethod]
    public void StandardFantasy_CheatSheet_IsNotEmpty()
    {
        var calendar = new StandardFantasyCalendar();
        Assert.IsFalse(string.IsNullOrWhiteSpace(calendar.CheatSheet));
    }

    [TestMethod]
    public void Confederate_CheatSheet_IsNotEmpty()
    {
        var calendar = new ConfederateCalendar();
        Assert.IsFalse(string.IsNullOrWhiteSpace(calendar.CheatSheet));
    }

    [TestMethod]
    public void Imperial_CheatSheet_IsNotEmpty()
    {
        var calendar = new ImperialCalendar();
        Assert.IsFalse(string.IsNullOrWhiteSpace(calendar.CheatSheet));
    }

    [TestMethod]
    public void AllCalendars_CheatSheet_ContainsRoundReference()
    {
        IGameCalendar[] calendars =
        [
            new StandardFantasyCalendar(),
            new ConfederateCalendar(),
            new ImperialCalendar()
        ];

        foreach (var calendar in calendars)
        {
            Assert.IsTrue(calendar.CheatSheet.Contains("Round") || calendar.CheatSheet.Contains("round"),
                $"{calendar.Name} cheat sheet should reference rounds");
        }
    }

    [TestMethod]
    public void Confederate_CheatSheet_ContainsSlang()
    {
        var calendar = new ConfederateCalendar();
        Assert.IsTrue(calendar.CheatSheet.Contains("pip"), "Should reference pip");
        Assert.IsTrue(calendar.CheatSheet.Contains("tick"), "Should reference tick");
        Assert.IsTrue(calendar.CheatSheet.Contains("dec"), "Should reference dec");
        Assert.IsTrue(calendar.CheatSheet.Contains("mark"), "Should reference mark");
        Assert.IsTrue(calendar.CheatSheet.Contains("kat"), "Should reference kat");
    }

    [TestMethod]
    public void Imperial_CheatSheet_ContainsSlang()
    {
        var calendar = new ImperialCalendar();
        Assert.IsTrue(calendar.CheatSheet.Contains("glass"), "Should reference glass");
        Assert.IsTrue(calendar.CheatSheet.Contains("bell"), "Should reference bell");
        Assert.IsTrue(calendar.CheatSheet.Contains("watch"), "Should reference watch");
    }

    #endregion

    #region GameCalendarProvider Tests

    [TestMethod]
    public void Provider_GetCalendars_ReturnsCorrectForTheme()
    {
        var provider = CreateProvider();

        var fantasyCalendars = provider.GetCalendars("fantasy");
        Assert.AreEqual(3, fantasyCalendars.Count); // Common, Triumvirate, StandardFantasy

        var scifiCalendars = provider.GetCalendars("scifi");
        Assert.AreEqual(2, scifiCalendars.Count);
    }

    [TestMethod]
    public void Provider_GetDefaultCalendar_ReturnsThemeDefault()
    {
        var provider = CreateProvider();

        var fantasyDefault = provider.GetDefaultCalendar("fantasy");
        Assert.AreEqual("common", fantasyDefault.Id);

        var scifiDefault = provider.GetDefaultCalendar("scifi");
        Assert.AreEqual("confederate", scifiDefault.Id);
    }

    [TestMethod]
    public void Provider_GetCalendar_FallsBackToDefault_WhenIdIsNull()
    {
        var provider = CreateProvider();

        var calendar = provider.GetCalendar("fantasy", null);
        Assert.IsNotNull(calendar);
        Assert.AreEqual("common", calendar.Id);
    }

    [TestMethod]
    public void Provider_GetCalendar_FallsBackToDefault_WhenIdNotFound()
    {
        var provider = CreateProvider();

        var calendar = provider.GetCalendar("fantasy", "nonexistent");
        Assert.IsNotNull(calendar);
        Assert.AreEqual("common", calendar.Id);
    }

    [TestMethod]
    public void Provider_GetCalendar_ReturnsSpecificCalendar()
    {
        var provider = CreateProvider();

        var calendar = provider.GetCalendar("scifi", "imperial");
        Assert.IsNotNull(calendar);
        Assert.AreEqual("imperial", calendar.Id);
    }

    [TestMethod]
    public void Provider_GetCalendar_ReturnsTriumvirate()
    {
        var provider = CreateProvider();

        var calendar = provider.GetCalendar("fantasy", "triumvirate");
        Assert.IsNotNull(calendar);
        Assert.AreEqual("triumvirate", calendar.Id);
    }

    [TestMethod]
    public void Provider_GetThemes_ReturnsAllThemes()
    {
        var provider = CreateProvider();

        var themes = provider.GetThemes();
        Assert.AreEqual(2, themes.Count);
        CollectionAssert.Contains(themes.ToArray(), "fantasy");
        CollectionAssert.Contains(themes.ToArray(), "scifi");
    }

    [TestMethod]
    public void Provider_GetCalendars_UnknownTheme_ReturnsEmpty()
    {
        var provider = CreateProvider();

        var calendars = provider.GetCalendars("unknown");
        Assert.AreEqual(0, calendars.Count);
    }

    [TestMethod]
    public void Provider_GetDefaultCalendar_UnknownTheme_FallsBackToFantasy()
    {
        var provider = CreateProvider();

        var calendar = provider.GetDefaultCalendar("unknown");
        Assert.AreEqual("common", calendar.Id);
    }

    #endregion

    #region StandardFantasy Compose/Decompose Tests

    [TestMethod]
    [DataRow(0L)]
    [DataRow(86400L)]
    [DataRow(2592000L)]
    [DataRow(31104000L)]
    [DataRow(62208000L)]
    [DataRow(63849600L)]
    public void StandardFantasy_Decompose_Compose_Roundtrip(long epochSeconds)
    {
        var (year, month, day, hour, minute, second) = StandardFantasyCalendar.DecomposeDateTime(epochSeconds);
        var recomposed = StandardFantasyCalendar.ComposeDateTime(year, month, day, hour, minute, second);
        Assert.AreEqual(epochSeconds, recomposed, $"Roundtrip failed for epoch {epochSeconds}");
    }

    [TestMethod]
    public void StandardFantasy_Compose_KnownValues()
    {
        // Year 1, Month 1, Day 1, 00:00:00 = 31,104,000
        Assert.AreEqual(31104000L, StandardFantasyCalendar.ComposeDateTime(1, 0, 0, 0, 0, 0));

        // Year 0, Month 2, Day 1 = 2 * 2,592,000 = 5,184,000
        Assert.AreEqual(5184000L, StandardFantasyCalendar.ComposeDateTime(0, 2, 0, 0, 0, 0));

        // Year 0, Month 1, Day 1, 12:30:45
        var expected = 0 + 0 + 0 + 12 * 3600 + 30 * 60 + 45;
        Assert.AreEqual(expected, StandardFantasyCalendar.ComposeDateTime(0, 0, 0, 12, 30, 45));
    }

    [TestMethod]
    public void StandardFantasy_Compose_Decompose_Roundtrip()
    {
        // Compose known fields → decompose → verify same fields
        long year = 5, month = 3, day = 15, hour = 14, minute = 30, second = 45;
        var epoch = StandardFantasyCalendar.ComposeDateTime(year, month, day, hour, minute, second);
        var (y, m, d, h, min, s) = StandardFantasyCalendar.DecomposeDateTime(epoch);
        Assert.AreEqual(year, y);
        Assert.AreEqual(month, m);
        Assert.AreEqual(day, d);
        Assert.AreEqual(hour, h);
        Assert.AreEqual(minute, min);
        Assert.AreEqual(second, s);
    }

    [TestMethod]
    public void StandardFantasy_Compose_Zero()
    {
        Assert.AreEqual(0L, StandardFantasyCalendar.ComposeDateTime(0, 0, 0, 0, 0, 0));
    }

    #endregion

    #region Confederate Compose/Decompose Tests

    [TestMethod]
    [DataRow(0L)]
    [DataRow(88000L)]
    [DataRow(704000L)]
    [DataRow(3520000L)]
    [DataRow(31680000L)]
    [DataRow(63360000L)]
    public void Confederate_Decompose_Compose_Roundtrip(long epochSeconds)
    {
        var (year, pentade, week, day, watch, _) = ConfederateCalendar.DecomposeDateTime(epochSeconds);
        var recomposed = ConfederateCalendar.ComposeDateTime(year, pentade, week, day, watch);
        Assert.AreEqual(epochSeconds, recomposed, $"Roundtrip failed for epoch {epochSeconds}");
    }

    [TestMethod]
    public void Confederate_Compose_KnownValues()
    {
        // Year 1 = 31,680,000
        Assert.AreEqual(31680000L, ConfederateCalendar.ComposeDateTime(1, 0, 0, 0, 0));

        // First pentade, second week = 704,000
        Assert.AreEqual(704000L, ConfederateCalendar.ComposeDateTime(0, 0, 1, 0, 0));

        // Second pentade = 3,520,000
        Assert.AreEqual(3520000L, ConfederateCalendar.ComposeDateTime(0, 1, 0, 0, 0));
    }

    [TestMethod]
    public void Confederate_Compose_Decompose_Roundtrip()
    {
        long year = 3, pentade = 4, week = 2, day = 5, watch = 3;
        var epoch = ConfederateCalendar.ComposeDateTime(year, pentade, week, day, watch);
        var (y, p, w, d, wa, _) = ConfederateCalendar.DecomposeDateTime(epoch);
        Assert.AreEqual(year, y);
        Assert.AreEqual(pentade, p);
        Assert.AreEqual(week, w);
        Assert.AreEqual(day, d);
        Assert.AreEqual(watch, wa);
    }

    [TestMethod]
    public void Confederate_Compose_Zero()
    {
        Assert.AreEqual(0L, ConfederateCalendar.ComposeDateTime(0, 0, 0, 0, 0));
    }

    #endregion

    #region Imperial Compose/Decompose Tests

    [TestMethod]
    [DataRow(0L)]
    [DataRow(90360L)]
    [DataRow(2801160L)]
    [DataRow(33885000L)]
    [DataRow(67770000L)]
    public void Imperial_Decompose_Compose_Roundtrip(long epochSeconds)
    {
        var (year, month, day, bell, _) = ImperialCalendar.DecomposeDateTime(epochSeconds);
        var recomposed = ImperialCalendar.ComposeDateTime(year, month, day, bell);
        Assert.AreEqual(epochSeconds, recomposed, $"Roundtrip failed for epoch {epochSeconds}");
    }

    [TestMethod]
    public void Imperial_Compose_KnownValues()
    {
        // Year 1 = 33,885,000
        Assert.AreEqual(33885000L, ImperialCalendar.ComposeDateTime(1, 0, 0, 0));

        // Second month (Ayren) = day 31 * 90,360 = 2,801,160
        Assert.AreEqual(2801160L, ImperialCalendar.ComposeDateTime(0, 1, 0, 0));

        // Aburen (month 4, 0-indexed) = day (31+31+31+31) * 90,360 = 124 * 90,360 = 11,204,640
        Assert.AreEqual(11204640L, ImperialCalendar.ComposeDateTime(0, 4, 0, 0));
    }

    [TestMethod]
    public void Imperial_Compose_Decompose_Roundtrip()
    {
        long year = 2;
        int month = 7, day = 20, bell = 15;
        var epoch = ImperialCalendar.ComposeDateTime(year, month, day, bell);
        var (y, m, d, b, _) = ImperialCalendar.DecomposeDateTime(epoch);
        Assert.AreEqual(year, y);
        Assert.AreEqual(month, m);
        Assert.AreEqual(day, d);
        Assert.AreEqual(bell, b);
    }

    [TestMethod]
    public void Imperial_Compose_Zero()
    {
        Assert.AreEqual(0L, ImperialCalendar.ComposeDateTime(0, 0, 0, 0));
    }

    [TestMethod]
    public void Imperial_MonthBoundary_Aburen32Days()
    {
        // Aburen (month 4) has 32 days. Compose day 31 (0-indexed = 32nd day) and verify it roundtrips.
        var epoch = ImperialCalendar.ComposeDateTime(0, 4, 31, 0);
        var (y, m, d, b, _) = ImperialCalendar.DecomposeDateTime(epoch);
        Assert.AreEqual(0L, y);
        Assert.AreEqual(4, m);
        Assert.AreEqual(31, d);
        Assert.AreEqual(0, b);
    }

    [TestMethod]
    public void Imperial_MonthBoundary_Kislaren32Days()
    {
        // Kislaren (month 8) has 32 days
        var epoch = ImperialCalendar.ComposeDateTime(0, 8, 31, 0);
        var (y, m, d, b, _) = ImperialCalendar.DecomposeDateTime(epoch);
        Assert.AreEqual(0L, y);
        Assert.AreEqual(8, m);
        Assert.AreEqual(31, d);
    }

    [TestMethod]
    public void Imperial_MonthBoundary_Addaren32Days()
    {
        // Addaren (month 11) has 32 days - last month of year
        var epoch = ImperialCalendar.ComposeDateTime(0, 11, 31, 0);
        var (y, m, d, b, _) = ImperialCalendar.DecomposeDateTime(epoch);
        Assert.AreEqual(0L, y);
        Assert.AreEqual(11, m);
        Assert.AreEqual(31, d);
    }

    [TestMethod]
    public void Imperial_YearRollover()
    {
        // Last moment of year 0 → first moment of year 1
        var lastDay = ImperialCalendar.ComposeDateTime(0, 11, 31, 23);
        var firstDayNextYear = ImperialCalendar.ComposeDateTime(1, 0, 0, 0);
        Assert.IsTrue(firstDayNextYear > lastDay);
    }

    [TestMethod]
    public void Imperial_GetDaysInMonth()
    {
        // Months with 31 days: 0,1,2,3,5,6,7,9,10
        Assert.AreEqual(31, ImperialCalendar.GetDaysInMonth(0));  // Nisaren
        Assert.AreEqual(31, ImperialCalendar.GetDaysInMonth(1));  // Ayren
        // Months with 32 days: 4,8,11
        Assert.AreEqual(32, ImperialCalendar.GetDaysInMonth(4));  // Aburen
        Assert.AreEqual(32, ImperialCalendar.GetDaysInMonth(8));  // Kislaren
        Assert.AreEqual(32, ImperialCalendar.GetDaysInMonth(11)); // Addaren
    }

    #endregion

    #region Common Calendar Tests

    [TestMethod]
    public void Common_Properties()
    {
        var calendar = new CommonCalendar();
        Assert.AreEqual("common", calendar.Id);
        Assert.AreEqual("Common Calendar", calendar.Name);
        Assert.AreEqual("fantasy", calendar.Theme);
        Assert.IsTrue(calendar.IsThemeDefault);
        Assert.AreEqual(0L, calendar.EpochOffset);
        Assert.AreEqual("AR", calendar.YearSuffix);
    }

    [TestMethod]
    public void Common_FormatDate_EpochZero()
    {
        var calendar = new CommonCalendar();
        // Epoch 0 = Day 1 of Seedmont, Year 1 AR
        Assert.AreEqual("1 Seedmont 1 AR", calendar.FormatDate(0));
    }

    [TestMethod]
    public void Common_FormatDate_OneDay()
    {
        var calendar = new CommonCalendar();
        Assert.AreEqual("2 Seedmont 1 AR", calendar.FormatDate(86_400));
    }

    [TestMethod]
    public void Common_FormatDate_SecondMonth()
    {
        var calendar = new CommonCalendar();
        // Floodmont starts at day 29 = 28 * 86400 = 2,419,200
        Assert.AreEqual("1 Floodmont 1 AR", calendar.FormatDate(2_419_200));
    }

    [TestMethod]
    public void Common_FormatDate_Year2()
    {
        var calendar = new CommonCalendar();
        // Year 2 starts at 31,449,600
        Assert.AreEqual("1 Seedmont 2 AR", calendar.FormatDate(31_449_600));
    }

    [TestMethod]
    public void Common_FormatDate_Darkmont()
    {
        var calendar = new CommonCalendar();
        // Darkmont (13th month) starts at day 337 = 336 * 86400 = 29,030,400
        Assert.AreEqual("1 Darkmont 1 AR", calendar.FormatDate(29_030_400));
    }

    [TestMethod]
    public void Common_FormatDateTime_IncludesDayOfWeek()
    {
        var calendar = new CommonCalendar();
        // Epoch 0 = Melday (index 0)
        var result = calendar.FormatDateTime(0);
        Assert.IsTrue(result.StartsWith("Melday,"), $"Expected day name, got: {result}");
        Assert.IsTrue(result.Contains("Seedmont"));
        Assert.IsTrue(result.Contains("1 AR"));
    }

    [TestMethod]
    public void Common_FormatCompact()
    {
        var calendar = new CommonCalendar();
        Assert.AreEqual("1AR/Sed/01 00:00:00", calendar.FormatCompact(0));
    }

    [TestMethod]
    public void Common_DayOfWeekProgression()
    {
        var calendar = new CommonCalendar();
        // Day 0 = Melday, Day 1 = Abday, ... Day 6 = Infday, Day 7 = Melday again
        Assert.IsTrue(calendar.FormatDateTime(0).StartsWith("Melday"));
        Assert.IsTrue(calendar.FormatDateTime(86_400).StartsWith("Abday"));
        Assert.IsTrue(calendar.FormatDateTime(86_400 * 2).StartsWith("Ozday"));
        Assert.IsTrue(calendar.FormatDateTime(86_400 * 3).StartsWith("Lothday"));
        Assert.IsTrue(calendar.FormatDateTime(86_400 * 4).StartsWith("Fathday"));
        Assert.IsTrue(calendar.FormatDateTime(86_400 * 5).StartsWith("Thralday"));
        Assert.IsTrue(calendar.FormatDateTime(86_400 * 6).StartsWith("Infday"));
        Assert.IsTrue(calendar.FormatDateTime(86_400 * 7).StartsWith("Melday")); // week wraps
    }

    [TestMethod]
    [DataRow(0L)]
    [DataRow(86_400L)]
    [DataRow(2_419_200L)]
    [DataRow(31_449_600L)]
    [DataRow(62_899_200L)]
    [DataRow(100_000_000L)]
    public void Common_Decompose_Compose_Roundtrip(long epochSeconds)
    {
        var calendar = new CommonCalendar();
        var (year, month, day, _, hour, minute, second) = calendar.DecomposeDateTime(epochSeconds);
        var recomposed = calendar.ComposeDateTime(year, month, day, hour, minute, second);
        Assert.AreEqual(epochSeconds, recomposed, $"Roundtrip failed for epoch {epochSeconds}");
    }

    [TestMethod]
    public void Common_Compose_Decompose_Roundtrip()
    {
        var calendar = new CommonCalendar();
        long year = 97;
        int month = 7, day = 14, hour = 14, minute = 30, second = 45; // 15 Veilmont 97 AR
        var epoch = calendar.ComposeDateTime(year, month, day, hour, minute, second);
        var (y, m, d, _, h, min, s) = calendar.DecomposeDateTime(epoch);
        Assert.AreEqual(year, y);
        Assert.AreEqual(month, m);
        Assert.AreEqual(day, d);
        Assert.AreEqual(hour, h);
        Assert.AreEqual(minute, min);
        Assert.AreEqual(second, s);
    }

    [TestMethod]
    public void Common_YearEndBoundary()
    {
        var calendar = new CommonCalendar();
        // Last day of year 1: Day 28 of Darkmont = day 364 (index 363)
        // = 363 * 86400 = 31,363,200
        var (year, month, day, _, _, _, _) = calendar.DecomposeDateTime(31_363_200);
        Assert.AreEqual(1L, year);
        Assert.AreEqual(12, month); // Darkmont (0-indexed)
        Assert.AreEqual(27, day);   // Day 28 (0-indexed)
    }

    [TestMethod]
    public void Common_13Months()
    {
        var calendar = new CommonCalendar();
        Assert.AreEqual(13, calendar.MonthNamesFull.Length);
        Assert.AreEqual("Seedmont", calendar.MonthNamesFull[0]);
        Assert.AreEqual("Darkmont", calendar.MonthNamesFull[12]);
    }

    [TestMethod]
    public void Common_7DayNames()
    {
        var calendar = new CommonCalendar();
        Assert.AreEqual(7, calendar.DayNames.Length);
        Assert.AreEqual("Melday", calendar.DayNames[0]);
        Assert.AreEqual("Infday", calendar.DayNames[6]);
    }

    [TestMethod]
    public void Common_CheatSheet_IsNotEmpty()
    {
        var calendar = new CommonCalendar();
        Assert.IsFalse(string.IsNullOrWhiteSpace(calendar.CheatSheet));
        Assert.IsTrue(calendar.CheatSheet.Contains("Seedmont"));
        Assert.IsTrue(calendar.CheatSheet.Contains("Belerdar"));
        Assert.IsTrue(calendar.CheatSheet.Contains("Round"));
    }

    #endregion

    #region Triumvirate Calendar Tests

    [TestMethod]
    public void Triumvirate_Properties()
    {
        var calendar = new TriumvirateCalendar();
        Assert.AreEqual("triumvirate", calendar.Id);
        Assert.AreEqual("Triumvirate Reckoning", calendar.Name);
        Assert.AreEqual("fantasy", calendar.Theme);
        Assert.IsFalse(calendar.IsThemeDefault);
        Assert.AreEqual(817_689_600L, calendar.EpochOffset);
        Assert.AreEqual("YD", calendar.YearSuffix);
    }

    [TestMethod]
    public void Triumvirate_FormatDate_Year1()
    {
        var calendar = new TriumvirateCalendar();
        // Triumvirate Year 1 = Common Year 27 AR = epoch 817,689,600
        Assert.AreEqual("1 The Awakening, 1 YD", calendar.FormatDate(817_689_600));
    }

    [TestMethod]
    public void Triumvirate_FormatDate_Year70()
    {
        var calendar = new TriumvirateCalendar();
        // Year 70 YD = epoch 817,689,600 + (69 * 31,449,600) = 817,689,600 + 2,169,922,400
        var epoch = 817_689_600L + 69L * 31_449_600L;
        Assert.AreEqual("1 The Awakening, 70 YD", calendar.FormatDate(epoch));
    }

    [TestMethod]
    public void Triumvirate_FormatDateTime_IncludesDayOfWeek()
    {
        var calendar = new TriumvirateCalendar();
        var result = calendar.FormatDateTime(817_689_600);
        Assert.IsTrue(result.StartsWith("Throneday,"), $"Expected Throneday, got: {result}");
        Assert.IsTrue(result.Contains("The Awakening"));
        Assert.IsTrue(result.Contains("1 YD"));
    }

    [TestMethod]
    public void Triumvirate_DayOfWeekProgression()
    {
        var calendar = new TriumvirateCalendar();
        long baseEpoch = 817_689_600; // Start of Triumvirate
        Assert.IsTrue(calendar.FormatDateTime(baseEpoch).StartsWith("Throneday"));
        Assert.IsTrue(calendar.FormatDateTime(baseEpoch + 86_400).StartsWith("Chainday"));
        Assert.IsTrue(calendar.FormatDateTime(baseEpoch + 86_400 * 2).StartsWith("Flameday"));
        Assert.IsTrue(calendar.FormatDateTime(baseEpoch + 86_400 * 3).StartsWith("Duskday"));
        Assert.IsTrue(calendar.FormatDateTime(baseEpoch + 86_400 * 4).StartsWith("Veilday"));
        Assert.IsTrue(calendar.FormatDateTime(baseEpoch + 86_400 * 5).StartsWith("Crownday"));
        Assert.IsTrue(calendar.FormatDateTime(baseEpoch + 86_400 * 6).StartsWith("Ashday"));
        Assert.IsTrue(calendar.FormatDateTime(baseEpoch + 86_400 * 7).StartsWith("Throneday"));
    }

    [TestMethod]
    [DataRow(817_689_600L)]               // Triumvirate epoch start
    [DataRow(817_689_600L + 86_400L)]     // Day 2
    [DataRow(817_689_600L + 2_419_200L)]  // Second month
    [DataRow(817_689_600L + 31_449_600L)] // Year 2
    [DataRow(900_000_000L)]
    public void Triumvirate_Decompose_Compose_Roundtrip(long epochSeconds)
    {
        var calendar = new TriumvirateCalendar();
        var (year, month, day, _, hour, minute, second) = calendar.DecomposeDateTime(epochSeconds);
        var recomposed = calendar.ComposeDateTime(year, month, day, hour, minute, second);
        Assert.AreEqual(epochSeconds, recomposed, $"Roundtrip failed for epoch {epochSeconds}");
    }

    [TestMethod]
    public void Triumvirate_Compose_Decompose_Roundtrip()
    {
        var calendar = new TriumvirateCalendar();
        long year = 70;
        int month = 7, day = 14, hour = 10, minute = 15, second = 30; // 15 The Reckoning, 70 YD
        var epoch = calendar.ComposeDateTime(year, month, day, hour, minute, second);
        var (y, m, d, _, h, min, s) = calendar.DecomposeDateTime(epoch);
        Assert.AreEqual(year, y);
        Assert.AreEqual(month, m);
        Assert.AreEqual(day, d);
        Assert.AreEqual(hour, h);
        Assert.AreEqual(minute, min);
        Assert.AreEqual(second, s);
    }

    [TestMethod]
    public void Triumvirate_CommonConversion()
    {
        // 70 YD = 96 AR (70 + 26 = 96)
        var common = new CommonCalendar();
        var triumvirate = new TriumvirateCalendar();

        var triEpoch = triumvirate.ComposeDateTime(70, 0, 0, 0, 0, 0);
        var (commonYear, _, _, _, _, _, _) = common.DecomposeDateTime(triEpoch);
        Assert.AreEqual(96L, commonYear, "70 YD should equal 96 AR");
    }

    [TestMethod]
    public void Triumvirate_SameDayDifferentNames()
    {
        // Same epoch should show same astronomical day but different names
        var common = new CommonCalendar();
        var triumvirate = new TriumvirateCalendar();

        long epoch = 817_689_600; // Start of Triumvirate = 1 Seedmont 27 AR

        var commonDate = common.FormatDate(epoch);
        var triDate = triumvirate.FormatDate(epoch);

        Assert.AreEqual("1 Seedmont 27 AR", commonDate);
        Assert.AreEqual("1 The Awakening, 1 YD", triDate);
    }

    [TestMethod]
    public void Triumvirate_13Months()
    {
        var calendar = new TriumvirateCalendar();
        Assert.AreEqual(13, calendar.MonthNamesFull.Length);
        Assert.AreEqual("The Awakening", calendar.MonthNamesFull[0]);
        Assert.AreEqual("The Eternal", calendar.MonthNamesFull[12]);
    }

    [TestMethod]
    public void Triumvirate_CheatSheet_IsNotEmpty()
    {
        var calendar = new TriumvirateCalendar();
        Assert.IsFalse(string.IsNullOrWhiteSpace(calendar.CheatSheet));
        Assert.IsTrue(calendar.CheatSheet.Contains("Throneday"));
        Assert.IsTrue(calendar.CheatSheet.Contains("The Awakening"));
    }

    #endregion

    #region Cross-Calendar Consistency Tests

    [TestMethod]
    public void Common_Triumvirate_ShareAstronomicalConstants()
    {
        // Both share 364-day year, 28-day months, 7-day weeks
        var common = new CommonCalendar();
        var tri = new TriumvirateCalendar();

        // Same epoch should decompose to same day-of-month and day-of-week
        long epoch = 900_000_000;
        var (_, cMonth, cDay, cDow, cHour, _, _) = common.DecomposeDateTime(epoch);
        var (_, tMonth, tDay, tDow, tHour, _, _) = tri.DecomposeDateTime(epoch);

        // Same month index and day within month (since both have 13×28)
        Assert.AreEqual(cMonth, tMonth);
        Assert.AreEqual(cDay, tDay);
        Assert.AreEqual(cDow, tDow);
        Assert.AreEqual(cHour, tHour);
    }

    [TestMethod]
    public void Triumvirate_EpochOffset_Is26CommonYears()
    {
        Assert.AreEqual(26L * ThreanCalendarBase.SecondsPerYear, TriumvirateCalendar.TriumvirateEpochOffset);
    }

    #endregion

    private static GameCalendarProvider CreateProvider()
    {
        IGameCalendar[] calendars =
        [
            new CommonCalendar(),
            new TriumvirateCalendar(),
            new StandardFantasyCalendar(),
            new ConfederateCalendar(),
            new ImperialCalendar()
        ];
        return new GameCalendarProvider(calendars);
    }
}
