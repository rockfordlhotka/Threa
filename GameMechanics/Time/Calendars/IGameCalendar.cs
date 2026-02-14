namespace GameMechanics.Time.Calendars;

/// <summary>
/// Represents a game calendar system that converts epoch seconds to narrative date/time displays.
/// Different game settings (fantasy, sci-fi) can have multiple calendar systems.
/// </summary>
public interface IGameCalendar
{
    /// <summary>Unique identifier (e.g., "standard-fantasy", "confederate")</summary>
    string Id { get; }

    /// <summary>Display name (e.g., "Confederate Standard", "Imperial Calendar")</summary>
    string Name { get; }

    /// <summary>Theme this calendar belongs to ("fantasy", "scifi")</summary>
    string Theme { get; }

    /// <summary>Whether this is the natural default for its theme</summary>
    bool IsThemeDefault { get; }

    /// <summary>Full date/time display (e.g., "3rd of Harvest Moon, Year 2, 14:30")</summary>
    string FormatDateTime(long epochSeconds);

    /// <summary>Date only (e.g., "3rd of Harvest Moon, Year 2")</summary>
    string FormatDate(long epochSeconds);

    /// <summary>Compact display for headers/badges (e.g., "Y2/HM/3 14:30")</summary>
    string FormatCompact(long epochSeconds);

    /// <summary>
    /// Parse a calendar-formatted string back to epoch seconds.
    /// Designed now, full implementations deferred.
    /// </summary>
    bool TryParse(string input, out long epochSeconds);

    /// <summary>
    /// Markdown reference content explaining this calendar's time units
    /// and their relationship to game-mechanic time periods.
    /// </summary>
    string CheatSheet { get; }
}
