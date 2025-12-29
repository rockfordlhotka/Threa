namespace Threa.Dal.Dto;

/// <summary>
/// Determines how multiple instances of the same effect interact.
/// </summary>
public enum StackBehavior
{
    /// <summary>
    /// New effect replaces the old one (resets duration).
    /// </summary>
    Replace = 0,

    /// <summary>
    /// Duration is extended, intensity unchanged.
    /// </summary>
    Extend = 1,

    /// <summary>
    /// Effect intensity increases, duration unchanged.
    /// </summary>
    Intensify = 2,

    /// <summary>
    /// Multiple separate instances are tracked independently.
    /// </summary>
    Independent = 3
}
