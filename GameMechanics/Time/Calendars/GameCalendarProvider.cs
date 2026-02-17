using System;
using System.Collections.Generic;
using System.Linq;

namespace GameMechanics.Time.Calendars;

/// <summary>
/// DI-driven calendar provider that discovers all registered IGameCalendar implementations.
/// Adding a new calendar = create the class + register in DI, and this provider finds it automatically.
/// </summary>
public class GameCalendarProvider : IGameCalendarProvider
{
    private readonly Dictionary<string, List<IGameCalendar>> _calendarsByTheme;
    private readonly Dictionary<string, IGameCalendar> _defaults;

    public GameCalendarProvider(IEnumerable<IGameCalendar> calendars)
    {
        _calendarsByTheme = calendars
            .GroupBy(c => c.Theme, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.ToList(),
                StringComparer.OrdinalIgnoreCase);

        _defaults = _calendarsByTheme.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.FirstOrDefault(c => c.IsThemeDefault) ?? kvp.Value.First(),
            StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<IGameCalendar> GetCalendars(string theme)
    {
        return _calendarsByTheme.TryGetValue(theme, out var calendars)
            ? calendars
            : [];
    }

    public IGameCalendar GetDefaultCalendar(string theme)
    {
        if (_defaults.TryGetValue(theme, out var calendar))
            return calendar;

        // Fall back to fantasy default if theme not found
        if (_defaults.TryGetValue("fantasy", out var fallback))
            return fallback;

        throw new InvalidOperationException($"No calendars registered for theme '{theme}' and no fantasy fallback available.");
    }

    public IGameCalendar? GetCalendar(string theme, string? calendarId)
    {
        if (string.IsNullOrEmpty(calendarId))
            return GetDefaultCalendar(theme);

        if (!_calendarsByTheme.TryGetValue(theme, out var calendars))
            return GetDefaultCalendar(theme);

        return calendars.FirstOrDefault(c => c.Id.Equals(calendarId, StringComparison.OrdinalIgnoreCase))
            ?? GetDefaultCalendar(theme);
    }

    public IReadOnlyList<string> GetThemes()
    {
        return _calendarsByTheme.Keys.ToList();
    }
}
