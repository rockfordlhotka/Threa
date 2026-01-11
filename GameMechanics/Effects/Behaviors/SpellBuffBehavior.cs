using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Threa.Dal.Dto;

namespace GameMechanics.Effects.Behaviors;

/// <summary>
/// Defines what type of modifier a buff applies.
/// </summary>
public enum BuffModifierType
{
  /// <summary>
  /// Modifies a specific attribute (STR, DEX, etc.).
  /// </summary>
  Attribute,

  /// <summary>
  /// Modifies ability score for all skills.
  /// </summary>
  AbilityScoreGlobal,

  /// <summary>
  /// Modifies ability score for a specific skill.
  /// </summary>
  AbilityScoreSkill,

  /// <summary>
  /// Modifies ability score for skills using a specific attribute.
  /// </summary>
  AbilityScoreAttribute,

  /// <summary>
  /// Modifies success value for all actions.
  /// </summary>
  SuccessValueGlobal,

  /// <summary>
  /// Modifies success value for a specific action type.
  /// </summary>
  SuccessValueAction,

  /// <summary>
  /// Provides healing per tick.
  /// </summary>
  HealingOverTime
}

/// <summary>
/// Represents a single modifier that a buff applies.
/// </summary>
public class BuffModifier
{
  /// <summary>
  /// The type of modifier.
  /// </summary>
  public BuffModifierType Type { get; set; }

  /// <summary>
  /// The target of the modifier (attribute name, skill name, action type, or "FAT"/"VIT" for healing).
  /// </summary>
  public string Target { get; set; } = string.Empty;

  /// <summary>
  /// The value of the modifier (positive for buffs, negative for debuffs).
  /// </summary>
  public int Value { get; set; }

  /// <summary>
  /// For healing over time, the interval in rounds between heals.
  /// </summary>
  public int HealIntervalRounds { get; set; } = 10;
}

/// <summary>
/// State data stored in BehaviorState for spell buff effects.
/// </summary>
public class SpellBuffState
{
  /// <summary>
  /// Display name of the buff.
  /// </summary>
  public string BuffName { get; set; } = "Spell Buff";

  /// <summary>
  /// Description of the buff's effects.
  /// </summary>
  public string Description { get; set; } = string.Empty;

  /// <summary>
  /// The spell or source that created this buff.
  /// </summary>
  public string Source { get; set; } = string.Empty;

  /// <summary>
  /// The modifiers this buff applies.
  /// </summary>
  public List<BuffModifier> Modifiers { get; set; } = [];

  /// <summary>
  /// Total duration in rounds.
  /// </summary>
  public int TotalDurationRounds { get; set; }

  /// <summary>
  /// Rounds elapsed since buff was applied.
  /// </summary>
  public int ElapsedRounds { get; set; }

  /// <summary>
  /// Whether the buff diminishes over time (like poison) or stays constant.
  /// </summary>
  public bool DiminishesOverTime { get; set; }

  /// <summary>
  /// Rounds until next healing tick (for HealingOverTime modifiers).
  /// </summary>
  public int RoundsUntilHealTick { get; set; }

  /// <summary>
  /// The caster level or power level of the spell (affects dispel difficulty).
  /// </summary>
  public int CasterLevel { get; set; } = 1;

  /// <summary>
  /// Gets the current effectiveness multiplier (1.0 if constant, diminishes if DiminishesOverTime).
  /// </summary>
  public double EffectivenessMultiplier
  {
    get
    {
      if (!DiminishesOverTime || TotalDurationRounds <= 0)
        return 1.0;
      var remaining = Math.Max(0, TotalDurationRounds - ElapsedRounds);
      return (double)remaining / TotalDurationRounds;
    }
  }

  /// <summary>
  /// Gets the effective value of a modifier, accounting for diminishing.
  /// </summary>
  public int GetEffectiveValue(BuffModifier modifier)
  {
    if (!DiminishesOverTime)
      return modifier.Value;

    var scaled = modifier.Value * EffectivenessMultiplier;
    // Round away from zero to maintain some effect until the end
    return modifier.Value > 0
      ? (int)Math.Ceiling(scaled)
      : (int)Math.Floor(scaled);
  }

  /// <summary>
  /// Serializes this state to JSON for storage in BehaviorState.
  /// </summary>
  public string Serialize() => JsonSerializer.Serialize(this);

  /// <summary>
  /// Deserializes buff state from BehaviorState JSON.
  /// </summary>
  public static SpellBuffState Deserialize(string? json)
  {
    if (string.IsNullOrEmpty(json))
      return new SpellBuffState();
    return JsonSerializer.Deserialize<SpellBuffState>(json) ?? new SpellBuffState();
  }

  #region Factory Methods

  /// <summary>
  /// Creates a simple attribute buff.
  /// </summary>
  public static SpellBuffState CreateAttributeBuff(string name, string attribute, int bonus, int durationRounds, bool diminishes = false)
  {
    return new SpellBuffState
    {
      BuffName = name,
      Description = $"+{bonus} to {attribute}",
      TotalDurationRounds = durationRounds,
      DiminishesOverTime = diminishes,
      Modifiers =
      [
        new BuffModifier
        {
          Type = BuffModifierType.Attribute,
          Target = attribute,
          Value = bonus
        }
      ]
    };
  }

  /// <summary>
  /// Creates a global AS buff (affects all skill checks).
  /// </summary>
  public static SpellBuffState CreateGlobalASBuff(string name, int bonus, int durationRounds, bool diminishes = false)
  {
    return new SpellBuffState
    {
      BuffName = name,
      Description = $"+{bonus} to all ability scores",
      TotalDurationRounds = durationRounds,
      DiminishesOverTime = diminishes,
      Modifiers =
      [
        new BuffModifier
        {
          Type = BuffModifierType.AbilityScoreGlobal,
          Target = "All",
          Value = bonus
        }
      ]
    };
  }

  /// <summary>
  /// Creates a skill-specific AS buff.
  /// </summary>
  public static SpellBuffState CreateSkillBuff(string name, string skillName, int bonus, int durationRounds)
  {
    return new SpellBuffState
    {
      BuffName = name,
      Description = $"+{bonus} to {skillName}",
      TotalDurationRounds = durationRounds,
      Modifiers =
      [
        new BuffModifier
        {
          Type = BuffModifierType.AbilityScoreSkill,
          Target = skillName,
          Value = bonus
        }
      ]
    };
  }

  /// <summary>
  /// Creates a healing over time buff.
  /// </summary>
  public static SpellBuffState CreateHealingBuff(string name, int healPerTick, int tickIntervalRounds, int durationRounds, bool healsFatigue = true, bool healsVitality = false)
  {
    var modifiers = new List<BuffModifier>();

    if (healsFatigue)
    {
      modifiers.Add(new BuffModifier
      {
        Type = BuffModifierType.HealingOverTime,
        Target = "FAT",
        Value = healPerTick,
        HealIntervalRounds = tickIntervalRounds
      });
    }

    if (healsVitality)
    {
      modifiers.Add(new BuffModifier
      {
        Type = BuffModifierType.HealingOverTime,
        Target = "VIT",
        Value = healPerTick,
        HealIntervalRounds = tickIntervalRounds
      });
    }

    return new SpellBuffState
    {
      BuffName = name,
      Description = healsFatigue && healsVitality
        ? $"Heals {healPerTick} FAT and VIT every {tickIntervalRounds} rounds"
        : healsFatigue
          ? $"Heals {healPerTick} FAT every {tickIntervalRounds} rounds"
          : $"Heals {healPerTick} VIT every {tickIntervalRounds} rounds",
      TotalDurationRounds = durationRounds,
      RoundsUntilHealTick = tickIntervalRounds,
      Modifiers = modifiers
    };
  }

  /// <summary>
  /// Creates a combat buff with multiple modifiers.
  /// </summary>
  public static SpellBuffState CreateCombatBuff(string name, int strBonus, int dexBonus, int asBonus, int durationRounds)
  {
    var modifiers = new List<BuffModifier>();

    if (strBonus != 0)
    {
      modifiers.Add(new BuffModifier
      {
        Type = BuffModifierType.Attribute,
        Target = "STR",
        Value = strBonus
      });
    }

    if (dexBonus != 0)
    {
      modifiers.Add(new BuffModifier
      {
        Type = BuffModifierType.Attribute,
        Target = "DEX",
        Value = dexBonus
      });
    }

    if (asBonus != 0)
    {
      modifiers.Add(new BuffModifier
      {
        Type = BuffModifierType.AbilityScoreGlobal,
        Target = "All",
        Value = asBonus
      });
    }

    return new SpellBuffState
    {
      BuffName = name,
      Description = $"Combat enhancement: STR +{strBonus}, DEX +{dexBonus}, AS +{asBonus}",
      TotalDurationRounds = durationRounds,
      Modifiers = modifiers
    };
  }

  #endregion
}

/// <summary>
/// Behavior implementation for spell buff effects.
/// Spell buffs can modify attributes, ability scores, and success values.
/// They can be dispelled by magic and do not stack (recasting fails).
/// </summary>
public class SpellBuffBehavior : IEffectBehavior
{
  public EffectType EffectType => EffectType.Buff;

  public EffectAddResult OnAdding(EffectRecord effect, CharacterEdit character)
  {
    var newState = SpellBuffState.Deserialize(effect.BehaviorState);

    // Check for existing buff with the same name - spell buffs don't stack
    var existingBuff = character.Effects
      .Where(e => e.EffectType == EffectType.Buff && e.IsActive)
      .FirstOrDefault(e =>
      {
        var state = SpellBuffState.Deserialize(e.BehaviorState);
        return state.BuffName == newState.BuffName;
      });

    if (existingBuff != null)
    {
      // Spell buffs don't stack - casting again just fails
      return EffectAddResult.Reject($"{newState.BuffName} is already active");
    }

    // Initialize healing tick counter if needed
    var hasHealing = newState.Modifiers.Any(m => m.Type == BuffModifierType.HealingOverTime);
    if (hasHealing && newState.RoundsUntilHealTick == 0)
    {
      var healMod = newState.Modifiers.First(m => m.Type == BuffModifierType.HealingOverTime);
      newState.RoundsUntilHealTick = healMod.HealIntervalRounds;
      effect.BehaviorState = newState.Serialize();
    }

    return EffectAddResult.AddNormally();
  }

  public void OnApply(EffectRecord effect, CharacterEdit character)
  {
    // Spell buffs don't have immediate effects beyond modifiers
    // (modifiers are applied via the Get*Modifiers methods)
  }

  public EffectTickResult OnTick(EffectRecord effect, CharacterEdit character)
  {
    var state = SpellBuffState.Deserialize(effect.BehaviorState);

    state.ElapsedRounds++;

    // Check for expiration
    if (state.ElapsedRounds >= state.TotalDurationRounds)
    {
      effect.BehaviorState = state.Serialize();
      return EffectTickResult.ExpireEarly("Spell buff has expired");
    }

    // Handle healing over time
    var healingModifiers = state.Modifiers.Where(m => m.Type == BuffModifierType.HealingOverTime).ToList();
    if (healingModifiers.Count > 0)
    {
      state.RoundsUntilHealTick--;
      if (state.RoundsUntilHealTick <= 0)
      {
        ApplyHealing(state, healingModifiers, character);
        // Reset tick counter (use first healing modifier's interval)
        state.RoundsUntilHealTick = healingModifiers[0].HealIntervalRounds;
      }
    }

    effect.BehaviorState = state.Serialize();
    return EffectTickResult.Continue();
  }

  private void ApplyHealing(SpellBuffState state, List<BuffModifier> healingModifiers, CharacterEdit character)
  {
    foreach (var mod in healingModifiers)
    {
      var healAmount = state.GetEffectiveValue(mod);
      if (healAmount <= 0) continue;

      if (mod.Target == "FAT")
      {
        character.Fatigue.PendingHealing += healAmount;
      }
      else if (mod.Target == "VIT")
      {
        character.Vitality.PendingHealing += healAmount;
      }
    }
  }

  public void OnExpire(EffectRecord effect, CharacterEdit character)
  {
    // Spell buffs expire cleanly with no negative effects
  }

  public void OnRemove(EffectRecord effect, CharacterEdit character)
  {
    // Dispelled - no lingering effects
  }

  public IEnumerable<EffectModifier> GetAttributeModifiers(EffectRecord effect, string attributeName, int baseValue)
  {
    var state = SpellBuffState.Deserialize(effect.BehaviorState);

    var attributeModifiers = state.Modifiers
      .Where(m => m.Type == BuffModifierType.Attribute &&
                  m.Target.Equals(attributeName, StringComparison.OrdinalIgnoreCase));

    foreach (var mod in attributeModifiers)
    {
      var value = state.GetEffectiveValue(mod);
      if (value != 0)
      {
        yield return new EffectModifier
        {
          Description = state.BuffName,
          Value = value,
          TargetAttribute = attributeName
        };
      }
    }
  }

  public IEnumerable<EffectModifier> GetAbilityScoreModifiers(EffectRecord effect, string skillName, string attributeName, int currentAS)
  {
    var state = SpellBuffState.Deserialize(effect.BehaviorState);

    foreach (var mod in state.Modifiers)
    {
      var value = state.GetEffectiveValue(mod);
      if (value == 0) continue;

      bool applies = mod.Type switch
      {
        BuffModifierType.AbilityScoreGlobal => true,
        BuffModifierType.AbilityScoreSkill =>
          mod.Target.Equals(skillName, StringComparison.OrdinalIgnoreCase),
        BuffModifierType.AbilityScoreAttribute =>
          attributeName.Contains(mod.Target, StringComparison.OrdinalIgnoreCase),
        _ => false
      };

      if (applies)
      {
        yield return new EffectModifier
        {
          Description = state.BuffName,
          Value = value,
          TargetSkill = skillName
        };
      }
    }
  }

  public IEnumerable<EffectModifier> GetSuccessValueModifiers(EffectRecord effect, string actionType, int currentSV)
  {
    var state = SpellBuffState.Deserialize(effect.BehaviorState);

    foreach (var mod in state.Modifiers)
    {
      var value = state.GetEffectiveValue(mod);
      if (value == 0) continue;

      bool applies = mod.Type switch
      {
        BuffModifierType.SuccessValueGlobal => true,
        BuffModifierType.SuccessValueAction =>
          mod.Target.Equals(actionType, StringComparison.OrdinalIgnoreCase),
        _ => false
      };

      if (applies)
      {
        yield return new EffectModifier
        {
          Description = state.BuffName,
          Value = value
        };
      }
    }
  }

  #region Static Helpers

  /// <summary>
  /// Applies a spell buff to a character.
  /// </summary>
  public static bool ApplyBuff(CharacterEdit character, SpellBuffState buffState, Csla.IChildDataPortal<EffectRecord> effectPortal)
  {
    var effect = effectPortal.CreateChild(
      EffectType.Buff,
      buffState.BuffName,
      null,
      buffState.TotalDurationRounds,
      buffState.Serialize());

    return character.Effects.AddEffect(effect);
  }

  /// <summary>
  /// Checks if a character has a specific buff active.
  /// </summary>
  public static bool HasBuff(CharacterEdit character, string buffName)
  {
    return character.Effects
      .Where(e => e.EffectType == EffectType.Buff && e.IsActive)
      .Any(e =>
      {
        var state = SpellBuffState.Deserialize(e.BehaviorState);
        return state.BuffName.Equals(buffName, StringComparison.OrdinalIgnoreCase);
      });
  }

  /// <summary>
  /// Gets all active spell buffs on a character.
  /// </summary>
  public static IEnumerable<(string Name, SpellBuffState State)> GetActiveBuffs(CharacterEdit character)
  {
    return character.Effects
      .Where(e => e.EffectType == EffectType.Buff && e.IsActive)
      .Select(e => (e.Name, SpellBuffState.Deserialize(e.BehaviorState)));
  }

  /// <summary>
  /// Attempts to dispel a buff by name. Returns true if successful.
  /// </summary>
  /// <param name="character">The character with the buff.</param>
  /// <param name="buffName">The name of the buff to dispel.</param>
  /// <param name="dispelPower">The power of the dispel attempt.</param>
  /// <returns>True if the buff was dispelled.</returns>
  public static bool TryDispel(CharacterEdit character, string buffName, int dispelPower)
  {
    var buffEffect = character.Effects
      .Where(e => e.EffectType == EffectType.Buff && e.IsActive)
      .FirstOrDefault(e =>
      {
        var state = SpellBuffState.Deserialize(e.BehaviorState);
        return state.BuffName.Equals(buffName, StringComparison.OrdinalIgnoreCase);
      });

    if (buffEffect == null)
      return false;

    var state = SpellBuffState.Deserialize(buffEffect.BehaviorState);

    // Dispel succeeds if dispel power >= caster level
    // (Could add dice roll here for more interesting mechanics)
    if (dispelPower >= state.CasterLevel)
    {
      character.Effects.RemoveEffect(buffEffect.Id);
      return true;
    }

    return false;
  }

  #endregion
}
