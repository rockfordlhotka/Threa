using System.Collections.Generic;

namespace GameMechanics.Time.Calendars;

/// <summary>
/// Discovers and provides game calendars by theme.
/// Uses DI to automatically find all registered IGameCalendar implementations.
/// </summary>
public interface IGameCalendarProvider
{
    /// <summary>All calendars available for a theme</summary>
    IReadOnlyList<IGameCalendar> GetCalendars(string theme);

    /// <summary>Theme's natural default calendar (fallback if table has no preference)</summary>
    IGameCalendar GetDefaultCalendar(string theme);

    /// <summary>Specific calendar by theme + ID. Returns null if not found.</summary>
    IGameCalendar? GetCalendar(string theme, string? calendarId);

    /// <summary>All supported themes</summary>
    IReadOnlyList<string> GetThemes();
}
