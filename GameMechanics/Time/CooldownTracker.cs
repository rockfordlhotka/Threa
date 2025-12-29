using System;
using System.Collections.Generic;
using System.Linq;

namespace GameMechanics.Time;

/// <summary>
/// Manages all active cooldowns for a single character.
/// </summary>
public class CooldownTracker
{
    private readonly Dictionary<Guid, Cooldown> _cooldowns = new();
    private readonly int _characterId;

    public CooldownTracker(int characterId)
    {
        _characterId = characterId;
    }

    /// <summary>
    /// Gets the character ID this tracker belongs to.
    /// </summary>
    public int CharacterId => _characterId;

    /// <summary>
    /// Gets all active cooldowns for this character.
    /// </summary>
    public IReadOnlyCollection<Cooldown> ActiveCooldowns => 
        _cooldowns.Values.Where(c => c.IsActive).ToList();

    /// <summary>
    /// Gets all cooldowns (including paused and ready).
    /// </summary>
    public IReadOnlyCollection<Cooldown> AllCooldowns => _cooldowns.Values.ToList();

    /// <summary>
    /// Starts a new cooldown for an action.
    /// </summary>
    public Cooldown StartCooldown(string actionName, double durationSeconds, CooldownBehavior behavior = CooldownBehavior.Resettable)
    {
        var cooldown = new Cooldown(_characterId, actionName, durationSeconds, behavior);
        _cooldowns[cooldown.Id] = cooldown;
        return cooldown;
    }

    /// <summary>
    /// Starts a skill-based cooldown (duration varies by skill level).
    /// </summary>
    public Cooldown StartSkillBasedCooldown(string actionName, string skillName, int skillLevel)
    {
        var cooldown = Cooldown.ForSkillBasedAction(_characterId, actionName, skillName, skillLevel);
        _cooldowns[cooldown.Id] = cooldown;
        return cooldown;
    }

    /// <summary>
    /// Gets a cooldown by its ID.
    /// </summary>
    public Cooldown? GetCooldown(Guid cooldownId)
    {
        return _cooldowns.TryGetValue(cooldownId, out var cooldown) ? cooldown : null;
    }

    /// <summary>
    /// Gets the cooldown for a specific action, if any.
    /// Returns the most recent cooldown if multiple exist.
    /// </summary>
    public Cooldown? GetCooldownForAction(string actionName)
    {
        return _cooldowns.Values
            .Where(c => c.ActionName == actionName && !c.IsReady)
            .OrderByDescending(c => c.StartedAt)
            .FirstOrDefault();
    }

    /// <summary>
    /// Checks if an action is currently on cooldown.
    /// </summary>
    public bool IsOnCooldown(string actionName)
    {
        return _cooldowns.Values.Any(c => c.ActionName == actionName && c.IsActive);
    }

    /// <summary>
    /// Checks if an action is ready (not on cooldown or cooldown complete).
    /// </summary>
    public bool IsActionReady(string actionName)
    {
        var cooldown = GetCooldownForAction(actionName);
        return cooldown == null || cooldown.IsReady;
    }

    /// <summary>
    /// Advances all active cooldowns by one round.
    /// Returns list of cooldowns that completed this round.
    /// </summary>
    public List<Cooldown> AdvanceRound()
    {
        var completed = new List<Cooldown>();

        foreach (var cooldown in _cooldowns.Values.Where(c => c.IsActive))
        {
            if (cooldown.AdvanceRound())
            {
                completed.Add(cooldown);
            }
        }

        return completed;
    }

    /// <summary>
    /// Interrupts all active cooldowns (e.g., when character takes damage).
    /// </summary>
    public void InterruptAll()
    {
        foreach (var cooldown in _cooldowns.Values.Where(c => c.IsActive))
        {
            cooldown.Interrupt();
        }
    }

    /// <summary>
    /// Interrupts a specific cooldown.
    /// </summary>
    public void Interrupt(Guid cooldownId)
    {
        if (_cooldowns.TryGetValue(cooldownId, out var cooldown))
        {
            cooldown.Interrupt();
        }
    }

    /// <summary>
    /// Interrupts cooldowns for a specific action.
    /// </summary>
    public void InterruptAction(string actionName)
    {
        foreach (var cooldown in _cooldowns.Values.Where(c => c.ActionName == actionName && c.IsActive))
        {
            cooldown.Interrupt();
        }
    }

    /// <summary>
    /// Removes completed cooldowns from tracking.
    /// </summary>
    public void CleanupCompleted()
    {
        var toRemove = _cooldowns.Where(kvp => kvp.Value.IsReady).Select(kvp => kvp.Key).ToList();
        foreach (var id in toRemove)
        {
            _cooldowns.Remove(id);
        }
    }

    /// <summary>
    /// Clears all cooldowns.
    /// </summary>
    public void Clear()
    {
        _cooldowns.Clear();
    }
}
