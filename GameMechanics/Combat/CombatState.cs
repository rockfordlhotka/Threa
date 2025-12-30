using System;
using System.Collections.Generic;

namespace GameMechanics.Combat
{
  /// <summary>
  /// Tracks combat state for a single participant during an encounter.
  /// </summary>
  public class CombatState
  {
    /// <summary>
    /// Unique identifier for the combatant (character ID, NPC ID, etc.).
    /// </summary>
    public string CombatantId { get; }

    /// <summary>
    /// Number of actions taken this round.
    /// Used to apply multiple action penalty (-1 AS after first action).
    /// </summary>
    public int ActionsThisRound { get; private set; }

    /// <summary>
    /// Whether the combatant is currently in parry mode.
    /// While in parry mode, parry defenses are free (no AP/FAT cost).
    /// </summary>
    public bool IsInParryMode { get; private set; }

    /// <summary>
    /// The skill ID used for parrying while in parry mode.
    /// Could be a weapon skill or shield skill.
    /// </summary>
    public string? ParrySkillId { get; private set; }

    /// <summary>
    /// Whether the combatant has acted this round.
    /// </summary>
    public bool HasActed => ActionsThisRound > 0;

    /// <summary>
    /// Whether the next action will incur the multiple action penalty.
    /// </summary>
    public bool WillHaveMultipleActionPenalty => ActionsThisRound > 0;

    /// <summary>
    /// Creates a new combat state for a combatant.
    /// </summary>
    public CombatState(string combatantId)
    {
      CombatantId = combatantId ?? throw new ArgumentNullException(nameof(combatantId));
    }

    /// <summary>
    /// Records that an action was taken.
    /// If the action is not a parry defense while in parry mode, exits parry mode.
    /// </summary>
    /// <param name="isParryDefense">True if this is a parry defense action.</param>
    /// <param name="isEnteringParryMode">True if this action is entering parry mode.</param>
    public void RecordAction(bool isParryDefense = false, bool isEnteringParryMode = false)
    {
      ActionsThisRound++;

      // Any non-parry, non-parry-entry action breaks parry mode
      if (!isParryDefense && !isEnteringParryMode && IsInParryMode)
      {
        ExitParryMode();
      }
    }

    /// <summary>
    /// Enters parry mode using the specified skill.
    /// Costs an action to enter (1 AP + 1 FAT or 2 AP).
    /// </summary>
    /// <param name="parrySkillId">The weapon or shield skill ID to use for parrying.</param>
    public void EnterParryMode(string parrySkillId)
    {
      if (string.IsNullOrEmpty(parrySkillId))
        throw new ArgumentNullException(nameof(parrySkillId));

      IsInParryMode = true;
      ParrySkillId = parrySkillId;
      RecordAction(isParryDefense: false, isEnteringParryMode: true); // Entering parry mode is an action
    }

    /// <summary>
    /// Exits parry mode.
    /// </summary>
    public void ExitParryMode()
    {
      IsInParryMode = false;
      ParrySkillId = null;
    }

    /// <summary>
    /// Records a parry defense while in parry mode.
    /// Does not count as an action for multiple action penalty purposes.
    /// </summary>
    public void RecordParryDefense()
    {
      if (!IsInParryMode)
        throw new InvalidOperationException("Cannot use parry defense when not in parry mode.");

      // Parry defenses while in parry mode are free - don't increment action count
      // But we still mark it as a parry defense for state tracking
    }

    /// <summary>
    /// Resets state at the start of a new round.
    /// Parry mode persists across rounds until broken.
    /// </summary>
    public void StartNewRound()
    {
      ActionsThisRound = 0;
      // Parry mode persists - only broken by non-parry actions
    }

    /// <summary>
    /// Resets all combat state (e.g., combat ends).
    /// </summary>
    public void Reset()
    {
      ActionsThisRound = 0;
      IsInParryMode = false;
      ParrySkillId = null;
    }
  }

  /// <summary>
  /// Manages combat state for all participants in an encounter.
  /// </summary>
  public class CombatStateManager
  {
    private readonly Dictionary<string, CombatState> _states = new();

    /// <summary>
    /// Gets or creates the combat state for a combatant.
    /// </summary>
    public CombatState GetState(string combatantId)
    {
      if (!_states.TryGetValue(combatantId, out var state))
      {
        state = new CombatState(combatantId);
        _states[combatantId] = state;
      }
      return state;
    }

    /// <summary>
    /// Removes a combatant from the encounter.
    /// </summary>
    public void RemoveCombatant(string combatantId)
    {
      _states.Remove(combatantId);
    }

    /// <summary>
    /// Starts a new round for all combatants.
    /// </summary>
    public void StartNewRound()
    {
      foreach (var state in _states.Values)
      {
        state.StartNewRound();
      }
    }

    /// <summary>
    /// Resets all combat state (combat ends).
    /// </summary>
    public void EndCombat()
    {
      _states.Clear();
    }

    /// <summary>
    /// Gets all active combatant IDs.
    /// </summary>
    public IEnumerable<string> GetCombatantIds() => _states.Keys;
  }
}
