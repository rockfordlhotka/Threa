using System.Collections.Generic;
using System.Linq;

namespace GameMechanics.Time;

/// <summary>
/// Represents a participant in initiative order.
/// </summary>
public class InitiativeEntry
{
    /// <summary>
    /// The character or NPC ID.
    /// </summary>
    public int EntityId { get; init; }

    /// <summary>
    /// Display name for the entity.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Whether this is a player character (true) or NPC (false).
    /// </summary>
    public bool IsPlayerCharacter { get; init; }

    /// <summary>
    /// Current available AP (determines initiative order).
    /// </summary>
    public int AvailableAP { get; set; }

    /// <summary>
    /// Secondary tiebreaker: Awareness attribute.
    /// </summary>
    public int Awareness { get; init; }

    /// <summary>
    /// Whether this entity has acted this round.
    /// </summary>
    public bool HasActed { get; set; }

    /// <summary>
    /// Whether this entity is delaying their action.
    /// </summary>
    public bool IsDelaying { get; set; }

    /// <summary>
    /// Whether this entity passed (took no action intentionally).
    /// </summary>
    public bool HasPassed { get; set; }

    /// <summary>
    /// Whether this entity can still act (not incapacitated).
    /// </summary>
    public bool CanAct { get; set; } = true;
}

/// <summary>
/// Calculates and manages initiative order for combat rounds.
/// Initiative is determined by Available AP (highest goes first).
/// </summary>
public class InitiativeCalculator
{
    private readonly List<InitiativeEntry> _participants = new();
    private int _currentIndex = -1;

    /// <summary>
    /// Gets all participants in current initiative order.
    /// </summary>
    public IReadOnlyList<InitiativeEntry> InitiativeOrder => _participants;

    /// <summary>
    /// Gets the current active participant (whose turn it is).
    /// </summary>
    public InitiativeEntry? CurrentParticipant => 
        _currentIndex >= 0 && _currentIndex < _participants.Count 
            ? _participants[_currentIndex] 
            : null;

    /// <summary>
    /// Gets whether all participants have acted or passed this round.
    /// </summary>
    public bool IsRoundComplete => _participants.All(p => p.HasActed || p.HasPassed || !p.CanAct);

    /// <summary>
    /// Adds a participant to the initiative tracker.
    /// </summary>
    public void AddParticipant(int entityId, string name, int availableAP, int awareness, bool isPC = true)
    {
        _participants.Add(new InitiativeEntry
        {
            EntityId = entityId,
            Name = name,
            AvailableAP = availableAP,
            Awareness = awareness,
            IsPlayerCharacter = isPC
        });
    }

    /// <summary>
    /// Removes a participant from the initiative tracker.
    /// </summary>
    public void RemoveParticipant(int entityId)
    {
        var entry = _participants.FirstOrDefault(p => p.EntityId == entityId);
        if (entry != null)
        {
            _participants.Remove(entry);
            // Adjust current index if needed
            if (_currentIndex >= _participants.Count)
                _currentIndex = _participants.Count - 1;
        }
    }

    /// <summary>
    /// Updates a participant's Available AP (called after they take actions).
    /// </summary>
    public void UpdateAvailableAP(int entityId, int newAvailableAP)
    {
        var entry = _participants.FirstOrDefault(p => p.EntityId == entityId);
        if (entry != null)
        {
            entry.AvailableAP = newAvailableAP;
        }
    }

    /// <summary>
    /// Sets whether a participant can act (false if incapacitated).
    /// </summary>
    public void SetCanAct(int entityId, bool canAct)
    {
        var entry = _participants.FirstOrDefault(p => p.EntityId == entityId);
        if (entry != null)
        {
            entry.CanAct = canAct;
        }
    }

    /// <summary>
    /// Calculates initiative order for a new round.
    /// Orders by Available AP (descending), then Awareness (descending).
    /// </summary>
    public void CalculateInitiative()
    {
        // Reset round state
        foreach (var p in _participants)
        {
            p.HasActed = false;
            p.HasPassed = false;
            p.IsDelaying = false;
        }

        // Sort by Available AP (descending), then Awareness (descending)
        var ordered = _participants
            .Where(p => p.CanAct)
            .OrderByDescending(p => p.AvailableAP)
            .ThenByDescending(p => p.Awareness)
            .ToList();

        // Rebuild the list with incapacitated at the end
        var incapacitated = _participants.Where(p => !p.CanAct).ToList();
        
        _participants.Clear();
        _participants.AddRange(ordered);
        _participants.AddRange(incapacitated);

        _currentIndex = _participants.Count > 0 ? 0 : -1;
    }

    /// <summary>
    /// Marks the current participant as having acted and advances to next.
    /// </summary>
    public InitiativeEntry? ActAndAdvance()
    {
        if (CurrentParticipant != null)
        {
            CurrentParticipant.HasActed = true;
        }
        return AdvanceToNext();
    }

    /// <summary>
    /// Marks the current participant as passing and advances to next.
    /// Passing preserves AP for future rounds.
    /// </summary>
    public InitiativeEntry? PassAndAdvance()
    {
        if (CurrentParticipant != null)
        {
            CurrentParticipant.HasPassed = true;
        }
        return AdvanceToNext();
    }

    /// <summary>
    /// Marks the current participant as delaying.
    /// They can act later in the round when they choose to stop delaying.
    /// </summary>
    public void Delay()
    {
        if (CurrentParticipant != null)
        {
            CurrentParticipant.IsDelaying = true;
        }
        AdvanceToNext();
    }

    /// <summary>
    /// Allows a delaying participant to act now.
    /// Inserts them into the initiative after the current participant.
    /// </summary>
    public void StopDelaying(int entityId)
    {
        var entry = _participants.FirstOrDefault(p => p.EntityId == entityId && p.IsDelaying);
        if (entry != null)
        {
            entry.IsDelaying = false;
            // They can act when their turn comes around again,
            // or GM can manually set them as current
        }
    }

    /// <summary>
    /// Advances to the next participant who can act.
    /// Returns the next participant or null if round is complete.
    /// </summary>
    public InitiativeEntry? AdvanceToNext()
    {
        if (_participants.Count == 0)
            return null;

        // Look for next participant who hasn't acted and can act
        for (int i = 1; i <= _participants.Count; i++)
        {
            var nextIndex = (_currentIndex + i) % _participants.Count;
            var next = _participants[nextIndex];
            
            if (!next.HasActed && !next.HasPassed && !next.IsDelaying && next.CanAct)
            {
                _currentIndex = nextIndex;
                return next;
            }
        }

        // Check if there are any delaying participants who can still act
        var delaying = _participants.FirstOrDefault(p => p.IsDelaying && p.CanAct);
        if (delaying != null)
        {
            _currentIndex = _participants.IndexOf(delaying);
            return delaying;
        }

        // Round is complete
        _currentIndex = -1;
        return null;
    }

    /// <summary>
    /// Gets the list of participants who haven't acted yet this round.
    /// </summary>
    public IReadOnlyList<InitiativeEntry> GetRemainingParticipants()
    {
        return _participants
            .Where(p => !p.HasActed && !p.HasPassed && p.CanAct)
            .ToList();
    }

    /// <summary>
    /// Gets the list of participants who are delaying.
    /// </summary>
    public IReadOnlyList<InitiativeEntry> GetDelayingParticipants()
    {
        return _participants.Where(p => p.IsDelaying && p.CanAct).ToList();
    }

    /// <summary>
    /// Resets the initiative tracker completely.
    /// </summary>
    public void Clear()
    {
        _participants.Clear();
        _currentIndex = -1;
    }
}
