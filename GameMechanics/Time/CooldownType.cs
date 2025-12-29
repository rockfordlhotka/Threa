namespace GameMechanics.Time;

/// <summary>
/// Defines how a cooldown behaves when interrupted.
/// </summary>
public enum CooldownBehavior
{
    /// <summary>
    /// Cooldown progress resets to 0% when interrupted.
    /// Example: Concentration spells, aiming actions.
    /// </summary>
    Resettable,

    /// <summary>
    /// Cooldown progress freezes when interrupted and resumes when continued.
    /// Example: Reloading a crossbow, retrieving items from backpack.
    /// </summary>
    Pausable
}

/// <summary>
/// The current state of a cooldown.
/// </summary>
public enum CooldownState
{
    /// <summary>
    /// The cooldown is not active (action is available).
    /// </summary>
    Ready,

    /// <summary>
    /// The cooldown is actively counting down.
    /// </summary>
    Active,

    /// <summary>
    /// The cooldown was interrupted and is paused (for Pausable cooldowns).
    /// </summary>
    Paused,

    /// <summary>
    /// The cooldown was interrupted and reset (for Resettable cooldowns).
    /// </summary>
    Reset
}
