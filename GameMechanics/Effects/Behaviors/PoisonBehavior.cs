using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Threa.Dal.Dto;

namespace GameMechanics.Effects.Behaviors;

/// <summary>
/// Defines the type of damage a poison deals.
/// </summary>
public enum PoisonDamageType
{
  /// <summary>
  /// Deals only fatigue damage (sleepy/sedative poisons).
  /// </summary>
  FatigueOnly,

  /// <summary>
  /// Deals only vitality damage (lethal poisons).
  /// </summary>
  VitalityOnly,

  /// <summary>
  /// Deals both fatigue and vitality damage.
  /// </summary>
  Combined
}

/// <summary>
/// State data stored in BehaviorState for poison effects.
/// </summary>
public class PoisonState
{
  /// <summary>
  /// The type of damage this poison deals.
  /// </summary>
  public PoisonDamageType DamageType { get; set; } = PoisonDamageType.FatigueOnly;

  /// <summary>
  /// Initial/base fatigue damage per tick.
  /// </summary>
  public int BaseFatigueDamage { get; set; }

  /// <summary>
  /// Initial/base vitality damage per tick.
  /// </summary>
  public int BaseVitalityDamage { get; set; }

  /// <summary>
  /// Rounds between damage applications.
  /// </summary>
  public int TickIntervalRounds { get; set; } = 10;

  /// <summary>
  /// Rounds until next damage tick.
  /// </summary>
  public int RoundsUntilNextTick { get; set; }

  /// <summary>
  /// Total duration of the poison in rounds.
  /// </summary>
  public int TotalDurationRounds { get; set; }

  /// <summary>
  /// Rounds elapsed since poison was applied.
  /// </summary>
  public int ElapsedRounds { get; set; }

  /// <summary>
  /// Initial AS penalty when poison is fresh.
  /// </summary>
  public int BaseASPenalty { get; set; }

  /// <summary>
  /// Whether this poison can create wounds on severe ticks.
  /// </summary>
  public bool CanCreateWounds { get; set; }

  /// <summary>
  /// Threshold (as % of duration remaining) below which wounds may occur.
  /// For example, 0.25 means wounds can occur in the first 25% of the poison's duration.
  /// </summary>
  public double WoundThreshold { get; set; } = 0.25;

  /// <summary>
  /// Current number of stacks (for intensifying poisons).
  /// </summary>
  public int Stacks { get; set; } = 1;

  /// <summary>
  /// Maximum stacks this poison can have.
  /// </summary>
  public int MaxStacks { get; set; } = 3;

  /// <summary>
  /// Name of the poison for display purposes.
  /// </summary>
  public string PoisonName { get; set; } = "Poison";

  /// <summary>
  /// Gets the current effectiveness multiplier (1.0 at start, decreasing to 0 at end).
  /// </summary>
  public double EffectivenessMultiplier
  {
    get
    {
      if (TotalDurationRounds <= 0) return 1.0;
      var remaining = Math.Max(0, TotalDurationRounds - ElapsedRounds);
      return (double)remaining / TotalDurationRounds;
    }
  }

  /// <summary>
  /// Gets the current fatigue damage per tick (diminishes over time).
  /// </summary>
  public int CurrentFatigueDamage
  {
    get
    {
      var baseDmg = BaseFatigueDamage * Stacks;
      var scaled = (int)Math.Ceiling(baseDmg * EffectivenessMultiplier);
      return Math.Max(0, scaled);
    }
  }

  /// <summary>
  /// Gets the current vitality damage per tick (diminishes over time).
  /// </summary>
  public int CurrentVitalityDamage
  {
    get
    {
      var baseDmg = BaseVitalityDamage * Stacks;
      var scaled = (int)Math.Ceiling(baseDmg * EffectivenessMultiplier);
      return Math.Max(0, scaled);
    }
  }

  /// <summary>
  /// Gets the current AS penalty (diminishes over time).
  /// </summary>
  public int CurrentASPenalty
  {
    get
    {
      // Work with absolute value, then negate
      var absBase = Math.Abs(BaseASPenalty) * Stacks;
      var scaled = (int)Math.Ceiling(absBase * EffectivenessMultiplier);
      return -scaled; // Always negative or zero
    }
  }

  /// <summary>
  /// Whether the poison is in its most dangerous phase (can create wounds).
  /// </summary>
  public bool IsInWoundPhase => CanCreateWounds && EffectivenessMultiplier >= (1.0 - WoundThreshold);

  /// <summary>
  /// Serializes this state to JSON for storage in BehaviorState.
  /// </summary>
  public string Serialize() => JsonSerializer.Serialize(this);

  /// <summary>
  /// Deserializes poison state from BehaviorState JSON.
  /// </summary>
  public static PoisonState Deserialize(string? json)
  {
    if (string.IsNullOrEmpty(json))
      return new PoisonState();
    return JsonSerializer.Deserialize<PoisonState>(json) ?? new PoisonState();
  }

  /// <summary>
  /// Creates a weak/sleepy poison (FAT only).
  /// </summary>
  public static PoisonState CreateWeakPoison(int durationRounds = 200, int tickInterval = 20)
  {
    return new PoisonState
    {
      PoisonName = "Weak Poison",
      DamageType = PoisonDamageType.FatigueOnly,
      BaseFatigueDamage = 2,
      BaseVitalityDamage = 0,
      TickIntervalRounds = tickInterval,
      RoundsUntilNextTick = tickInterval,
      TotalDurationRounds = durationRounds,
      BaseASPenalty = -1,
      CanCreateWounds = false
    };
  }

  /// <summary>
  /// Creates a strong/lethal poison (VIT damage, can wound).
  /// </summary>
  public static PoisonState CreateStrongPoison(int durationRounds = 100, int tickInterval = 10)
  {
    return new PoisonState
    {
      PoisonName = "Strong Poison",
      DamageType = PoisonDamageType.VitalityOnly,
      BaseFatigueDamage = 0,
      BaseVitalityDamage = 2,
      TickIntervalRounds = tickInterval,
      RoundsUntilNextTick = tickInterval,
      TotalDurationRounds = durationRounds,
      BaseASPenalty = -2,
      CanCreateWounds = true,
      WoundThreshold = 0.25
    };
  }

  /// <summary>
  /// Creates a deadly poison (both damage types, wounds).
  /// </summary>
  public static PoisonState CreateDeadlyPoison(int durationRounds = 60, int tickInterval = 5)
  {
    return new PoisonState
    {
      PoisonName = "Deadly Poison",
      DamageType = PoisonDamageType.Combined,
      BaseFatigueDamage = 3,
      BaseVitalityDamage = 2,
      TickIntervalRounds = tickInterval,
      RoundsUntilNextTick = tickInterval,
      TotalDurationRounds = durationRounds,
      BaseASPenalty = -3,
      CanCreateWounds = true,
      WoundThreshold = 0.5
    };
  }

  /// <summary>
  /// Creates a sleep poison (high FAT damage, no VIT).
  /// </summary>
  public static PoisonState CreateSleepPoison(int durationRounds = 100, int tickInterval = 5)
  {
    return new PoisonState
    {
      PoisonName = "Sleep Poison",
      DamageType = PoisonDamageType.FatigueOnly,
      BaseFatigueDamage = 4,
      BaseVitalityDamage = 0,
      TickIntervalRounds = tickInterval,
      RoundsUntilNextTick = tickInterval,
      TotalDurationRounds = durationRounds,
      BaseASPenalty = -2,
      CanCreateWounds = false
    };
  }
}

/// <summary>
/// Behavior implementation for poison effects.
/// Poisons deal periodic damage that diminishes over time.
/// </summary>
public class PoisonBehavior : IEffectBehavior
{
  public EffectType EffectType => EffectType.Poison;

  public EffectAddResult OnAdding(EffectRecord effect, CharacterEdit character)
  {
    var newState = PoisonState.Deserialize(effect.BehaviorState);

    // Check for existing poison of the same name
    var existingPoison = character.Effects
      .Where(e => e.EffectType == EffectType.Poison && e.IsActive)
      .FirstOrDefault(e =>
      {
        var state = PoisonState.Deserialize(e.BehaviorState);
        return state.PoisonName == newState.PoisonName;
      });

    if (existingPoison != null)
    {
      var existingState = PoisonState.Deserialize(existingPoison.BehaviorState);

      // Stack if not at max
      if (existingState.Stacks < existingState.MaxStacks)
      {
        existingState.Stacks++;
        // Reset duration on stack
        existingState.ElapsedRounds = 0;
        existingState.RoundsUntilNextTick = existingState.TickIntervalRounds;
        existingPoison.BehaviorState = existingState.Serialize();

        return EffectAddResult.Reject($"Poison stacked to {existingState.Stacks}");
      }
      else
      {
        // At max stacks - just refresh duration
        existingState.ElapsedRounds = 0;
        existingState.RoundsUntilNextTick = existingState.TickIntervalRounds;
        existingPoison.BehaviorState = existingState.Serialize();

        return EffectAddResult.Reject("Poison refreshed (max stacks)");
      }
    }

    // New poison - initialize state
    if (newState.RoundsUntilNextTick == 0)
      newState.RoundsUntilNextTick = newState.TickIntervalRounds;

    effect.BehaviorState = newState.Serialize();
    return EffectAddResult.AddNormally();
  }

  public void OnApply(EffectRecord effect, CharacterEdit character)
  {
    // Poison doesn't deal immediate damage on application
    // (though we could add a "first strike" damage here if desired)
  }

  public EffectTickResult OnTick(EffectRecord effect, CharacterEdit character)
  {
    var state = PoisonState.Deserialize(effect.BehaviorState);

    // Advance elapsed time
    state.ElapsedRounds++;

    // Check if poison has run its course
    if (state.ElapsedRounds >= state.TotalDurationRounds)
    {
      effect.BehaviorState = state.Serialize();
      return EffectTickResult.ExpireEarly("Poison has run its course");
    }

    // Check for damage tick
    state.RoundsUntilNextTick--;
    if (state.RoundsUntilNextTick <= 0)
    {
      // Apply damage based on poison type
      ApplyPoisonDamage(state, character);

      // Reset tick counter
      state.RoundsUntilNextTick = state.TickIntervalRounds;
    }

    effect.BehaviorState = state.Serialize();
    return EffectTickResult.Continue();
  }

  private void ApplyPoisonDamage(PoisonState state, CharacterEdit character)
  {
    // Apply fatigue damage if applicable
    if (state.DamageType == PoisonDamageType.FatigueOnly ||
        state.DamageType == PoisonDamageType.Combined)
    {
      var fatDamage = state.CurrentFatigueDamage;
      if (fatDamage > 0)
      {
        character.Fatigue.PendingDamage += fatDamage;
      }
    }

    // Apply vitality damage if applicable
    if (state.DamageType == PoisonDamageType.VitalityOnly ||
        state.DamageType == PoisonDamageType.Combined)
    {
      var vitDamage = state.CurrentVitalityDamage;
      if (vitDamage > 0)
      {
        character.Vitality.PendingDamage += vitDamage;
      }
    }

    // Check for wound creation during dangerous phase
    // Note: We can't create wounds directly here without the effect portal
    // Instead, we deal extra VIT damage that may overflow into wounds
    if (state.IsInWoundPhase && state.CurrentVitalityDamage > 0)
    {
      // During wound phase, poison is more aggressive
      // Add extra damage that may cascade into wounds via VIT overflow
      character.Vitality.PendingDamage += 1;
    }
  }

  public void OnExpire(EffectRecord effect, CharacterEdit character)
  {
    // Poison wears off naturally - no special effects
  }

  public void OnRemove(EffectRecord effect, CharacterEdit character)
  {
    // Poison is cured/neutralized - no lingering effects
  }

  public IEnumerable<EffectModifier> GetAttributeModifiers(EffectRecord effect, string attributeName, int baseValue)
  {
    // Poisons don't directly modify attributes
    return [];
  }

  public IEnumerable<EffectModifier> GetAbilityScoreModifiers(EffectRecord effect, string skillName, string attributeName, int currentAS)
  {
    var state = PoisonState.Deserialize(effect.BehaviorState);

    var penalty = state.CurrentASPenalty;
    if (penalty >= 0)
      return [];

    return
    [
      new EffectModifier
      {
        Description = $"{state.PoisonName} ({state.Stacks}x)",
        Value = penalty
      }
    ];
  }

  public IEnumerable<EffectModifier> GetSuccessValueModifiers(EffectRecord effect, string actionType, int currentSV)
  {
    // Poisons don't modify success values directly
    return [];
  }

  /// <summary>
  /// Applies a poison to a character.
  /// </summary>
  /// <param name="character">The character to poison.</param>
  /// <param name="poisonState">The poison configuration.</param>
  /// <param name="effectPortal">Portal for creating the effect.</param>
  public static void ApplyPoison(CharacterEdit character, PoisonState poisonState, Csla.IChildDataPortal<EffectRecord> effectPortal)
  {
    var effect = effectPortal.CreateChild(
      EffectType.Poison,
      poisonState.PoisonName,
      null, // No location for poison
      poisonState.TotalDurationRounds,
      poisonState.Serialize());

    character.Effects.AddEffect(effect);
  }

  /// <summary>
  /// Checks if a character is poisoned.
  /// </summary>
  public static bool IsPoisoned(CharacterEdit character)
  {
    return character.Effects.HasEffect(EffectType.Poison);
  }

  /// <summary>
  /// Gets all active poison states on a character.
  /// </summary>
  public static IEnumerable<(string Name, PoisonState State)> GetActivePoisons(CharacterEdit character)
  {
    return character.Effects
      .Where(e => e.EffectType == EffectType.Poison && e.IsActive)
      .Select(e => (e.Name, PoisonState.Deserialize(e.BehaviorState)));
  }

  /// <summary>
  /// Gets the total AS penalty from all active poisons.
  /// </summary>
  public static int GetTotalPoisonPenalty(CharacterEdit character)
  {
    return character.Effects
      .Where(e => e.EffectType == EffectType.Poison && e.IsActive)
      .Sum(e => PoisonState.Deserialize(e.BehaviorState).CurrentASPenalty);
  }
}
