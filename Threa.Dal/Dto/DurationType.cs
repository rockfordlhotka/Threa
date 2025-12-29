namespace Threa.Dal.Dto;

/// <summary>
/// How effect duration is measured and tracked.
/// </summary>
public enum DurationType
{
    /// <summary>
    /// Effect lasts for a number of combat rounds (3 seconds each).
    /// </summary>
    Rounds = 0,

    /// <summary>
    /// Effect lasts for a number of minutes.
    /// </summary>
    Minutes = 1,

    /// <summary>
    /// Effect lasts for a number of hours.
    /// </summary>
    Hours = 2,

    /// <summary>
    /// Effect lasts for a number of days.
    /// </summary>
    Days = 3,

    /// <summary>
    /// Effect lasts for a number of weeks.
    /// </summary>
    Weeks = 4,

    /// <summary>
    /// Effect never expires on its own.
    /// </summary>
    Permanent = 5,

    /// <summary>
    /// Effect lasts until explicitly removed or a condition is met.
    /// </summary>
    UntilRemoved = 6
}
