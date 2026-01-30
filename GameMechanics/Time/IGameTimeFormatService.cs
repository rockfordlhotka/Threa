using Threa.Dal.Dto;

namespace GameMechanics.Time;

/// <summary>
/// Service for formatting game time values. Consolidates all time formatting logic
/// and enables different implementations for fantasy vs sci-fi settings.
/// </summary>
public interface IGameTimeFormatService
{
    #region Time Constants

    /// <summary>
    /// Seconds per combat round (3 seconds).
    /// </summary>
    int SecondsPerRound { get; }

    /// <summary>
    /// Rounds per minute (20 rounds).
    /// </summary>
    int RoundsPerMinute { get; }

    /// <summary>
    /// Rounds per hour (1200 rounds).
    /// </summary>
    int RoundsPerHour { get; }

    /// <summary>
    /// Seconds per minute (60).
    /// </summary>
    long SecondsPerMinute { get; }

    /// <summary>
    /// Seconds per hour (3600).
    /// </summary>
    long SecondsPerHour { get; }

    /// <summary>
    /// Seconds per day (86400).
    /// </summary>
    long SecondsPerDay { get; }

    #endregion

    #region Epoch-based Formatting (Calendar Time)

    /// <summary>
    /// Breaks down total seconds into individual time components (years, months, days, hours, minutes, seconds).
    /// </summary>
    /// <param name="totalSeconds">Total seconds from epoch 0.</param>
    /// <returns>Components broken down by time unit.</returns>
    GameTimeComponents GetComponents(long totalSeconds);

    /// <summary>
    /// Formats time as "Y years, M months, D days, HH:MM:SS".
    /// Omits zero values for cleaner display.
    /// </summary>
    /// <param name="totalSeconds">Total seconds from epoch 0.</param>
    /// <returns>Full formatted time string.</returns>
    string FormatFull(long totalSeconds);

    /// <summary>
    /// Formats time as "Y/M/D HH:MM:SS" in a compact form.
    /// </summary>
    /// <param name="totalSeconds">Total seconds from epoch 0.</param>
    /// <returns>Compact formatted time string.</returns>
    string FormatCompact(long totalSeconds);

    /// <summary>
    /// Formats time showing only the most significant non-zero units (e.g., "2h 30m").
    /// Good for displaying elapsed time or durations.
    /// </summary>
    /// <param name="totalSeconds">Total seconds from epoch 0.</param>
    /// <returns>Duration formatted time string.</returns>
    string FormatDuration(long totalSeconds);

    #endregion

    #region Round-based Formatting

    /// <summary>
    /// Formats a number of rounds into a human-readable time string.
    /// Uses appropriate units based on duration (rounds, minutes, hours).
    /// </summary>
    /// <param name="rounds">Number of combat rounds.</param>
    /// <returns>Formatted time string (e.g., "5 rounds", "2 minutes", "1 hour").</returns>
    string FormatRounds(int rounds);

    /// <summary>
    /// Formats a number of rounds into a detailed time string showing multiple units.
    /// </summary>
    /// <param name="rounds">Number of combat rounds.</param>
    /// <returns>Detailed time string (e.g., "5 min 30 sec").</returns>
    string FormatRoundsDetailed(int rounds);

    #endregion

    #region DurationType-based Formatting

    /// <summary>
    /// Formats a duration based on its type and value.
    /// </summary>
    /// <param name="durationType">The type of duration (Rounds, Minutes, Hours, etc.).</param>
    /// <param name="value">The numeric value.</param>
    /// <returns>Formatted duration string (e.g., "5 rnd", "10 min", "2 hrs").</returns>
    string FormatDuration(DurationType durationType, int value);

    #endregion

    #region Progress Formatting

    /// <summary>
    /// Formats duration progress showing elapsed vs total time.
    /// Uses appropriate units based on total duration.
    /// </summary>
    /// <param name="elapsedSeconds">Elapsed seconds.</param>
    /// <param name="totalSeconds">Total seconds for the duration.</param>
    /// <returns>Progress string (e.g., "5 / 10 rounds", "2 / 5 minutes").</returns>
    string FormatProgress(long elapsedSeconds, long totalSeconds);

    /// <summary>
    /// Formats remaining duration in the most appropriate units.
    /// Uses rounds for short durations, calendar time for longer ones.
    /// </summary>
    /// <param name="remainingSeconds">Remaining seconds.</param>
    /// <returns>Remaining duration string (e.g., "5 rnd", "2m", "1h 30m").</returns>
    string FormatRemaining(long remainingSeconds);

    #endregion

    #region Conversion Helpers

    /// <summary>
    /// Converts rounds to seconds.
    /// </summary>
    /// <param name="rounds">Number of rounds.</param>
    /// <returns>Equivalent seconds.</returns>
    long RoundsToSeconds(int rounds);

    /// <summary>
    /// Converts seconds to rounds (rounded down).
    /// </summary>
    /// <param name="seconds">Number of seconds.</param>
    /// <returns>Equivalent rounds.</returns>
    int SecondsToRounds(long seconds);

    /// <summary>
    /// Converts a DurationType and value to total seconds.
    /// </summary>
    /// <param name="durationType">The duration type.</param>
    /// <param name="value">The numeric value.</param>
    /// <returns>Total seconds, or null for permanent/until-removed types.</returns>
    long? DurationToSeconds(DurationType durationType, int value);

    #endregion
}
