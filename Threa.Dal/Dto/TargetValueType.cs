namespace Threa.Dal.Dto;

/// <summary>
/// Defines how the Target Value (TV) is determined for an action.
/// </summary>
public enum TargetValueType
{
    /// <summary>
    /// Fixed TV based on difficulty level.
    /// Uses standard difficulty table (Easy=4, Routine=6, Moderate=8, etc.)
    /// </summary>
    Fixed = 0,

    /// <summary>
    /// TV is determined by opponent's opposing skill roll.
    /// TV = Opponent's AS + 4dF+
    /// </summary>
    Opposed = 1,

    /// <summary>
    /// TV is opponent's AS - 1 (no roll).
    /// Used when opponent cannot or chooses not to actively defend.
    /// </summary>
    Passive = 2
}
