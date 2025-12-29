using System;

namespace GameMechanics.Time;

/// <summary>
/// Represents an active cooldown for a character's action.
/// Tracks remaining time and handles interruption behavior.
/// </summary>
public class Cooldown
{
    /// <summary>
    /// Unique identifier for this cooldown instance.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// The character this cooldown belongs to.
    /// </summary>
    public int CharacterId { get; init; }

    /// <summary>
    /// Name/identifier of the action this cooldown is for.
    /// Example: "Bow Shot", "Reload Crossbow", "Retrieve Item"
    /// </summary>
    public string ActionName { get; init; } = string.Empty;

    /// <summary>
    /// Optional skill associated with this cooldown (for skill-based cooldowns).
    /// </summary>
    public string? SkillName { get; init; }

    /// <summary>
    /// How this cooldown behaves when interrupted.
    /// </summary>
    public CooldownBehavior Behavior { get; init; } = CooldownBehavior.Resettable;

    /// <summary>
    /// Total duration of the cooldown in seconds.
    /// </summary>
    public double TotalDurationSeconds { get; init; }

    /// <summary>
    /// Remaining duration in seconds.
    /// </summary>
    public double RemainingSeconds { get; private set; }

    /// <summary>
    /// Current state of the cooldown.
    /// </summary>
    public CooldownState State { get; private set; } = CooldownState.Active;

    /// <summary>
    /// When the cooldown was started.
    /// </summary>
    public DateTime StartedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a new cooldown with the specified duration.
    /// </summary>
    public Cooldown(int characterId, string actionName, double durationSeconds, CooldownBehavior behavior = CooldownBehavior.Resettable)
    {
        CharacterId = characterId;
        ActionName = actionName;
        TotalDurationSeconds = durationSeconds;
        RemainingSeconds = durationSeconds;
        Behavior = behavior;
    }

    /// <summary>
    /// Creates a skill-based cooldown (duration varies by skill level).
    /// </summary>
    public static Cooldown ForSkillBasedAction(int characterId, string actionName, string skillName, int skillLevel)
    {
        var duration = CalculateSkillBasedCooldown(skillLevel);
        return new Cooldown(characterId, actionName, duration, CooldownBehavior.Resettable)
        {
            SkillName = skillName
        };
    }

    /// <summary>
    /// Calculates cooldown duration based on skill level (per TIME_SYSTEM.md).
    /// </summary>
    public static double CalculateSkillBasedCooldown(int skillLevel)
    {
        return skillLevel switch
        {
            <= 0 => 6.0,   // 2 rounds
            1 => 5.0,
            2 => 4.0,
            3 => 3.0,      // 1 round
            4 or 5 => 2.0,
            6 or 7 => 1.0,
            8 or 9 => 0.5,
            _ => 0.0       // 10+ = no cooldown
        };
    }

    /// <summary>
    /// Advances the cooldown by one round (3 seconds).
    /// Returns true if the cooldown completed this round.
    /// </summary>
    public bool AdvanceRound()
    {
        if (State != CooldownState.Active)
            return false;

        RemainingSeconds -= 3.0;

        if (RemainingSeconds <= 0)
        {
            RemainingSeconds = 0;
            State = CooldownState.Ready;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Advances the cooldown by a specific number of seconds.
    /// Returns true if the cooldown completed.
    /// </summary>
    public bool AdvanceSeconds(double seconds)
    {
        if (State != CooldownState.Active)
            return false;

        RemainingSeconds -= seconds;

        if (RemainingSeconds <= 0)
        {
            RemainingSeconds = 0;
            State = CooldownState.Ready;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Interrupts the cooldown based on its behavior type.
    /// </summary>
    public void Interrupt()
    {
        if (State != CooldownState.Active)
            return;

        if (Behavior == CooldownBehavior.Resettable)
        {
            RemainingSeconds = TotalDurationSeconds;
            State = CooldownState.Reset;
        }
        else // Pausable
        {
            State = CooldownState.Paused;
        }
    }

    /// <summary>
    /// Resumes a paused cooldown.
    /// </summary>
    public void Resume()
    {
        if (State == CooldownState.Paused)
        {
            State = CooldownState.Active;
        }
    }

    /// <summary>
    /// Restarts a reset cooldown from the beginning.
    /// </summary>
    public void Restart()
    {
        RemainingSeconds = TotalDurationSeconds;
        State = CooldownState.Active;
    }

    /// <summary>
    /// Gets whether this cooldown is complete and the action is available.
    /// </summary>
    public bool IsReady => State == CooldownState.Ready;

    /// <summary>
    /// Gets whether this cooldown is actively counting down.
    /// </summary>
    public bool IsActive => State == CooldownState.Active;

    /// <summary>
    /// Gets the progress as a percentage (0.0 to 1.0).
    /// </summary>
    public double Progress => TotalDurationSeconds > 0 
        ? 1.0 - (RemainingSeconds / TotalDurationSeconds) 
        : 1.0;

    /// <summary>
    /// Gets the remaining time in whole rounds (3 seconds each).
    /// </summary>
    public int RemainingRounds => (int)Math.Ceiling(RemainingSeconds / 3.0);
}
