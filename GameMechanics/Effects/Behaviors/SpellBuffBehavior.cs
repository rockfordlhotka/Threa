using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using GameMechanics.Combat;
using GameMechanics.Combat.Effects;
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
  HealingOverTime,

  /// <summary>
  /// Grants an effect that is applied to targets when the buffed character hits with an attack.
  /// </summary>
  OnHitEffect,

  /// <summary>
  /// Grants bonus damage of a specific type on attacks.
  /// </summary>
  BonusDamage
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

  /// <summary>
  /// For OnHitEffect type, the effect grant to apply to targets on attack hit.
  /// </summary>
  public AttackEffectGrant? OnHitEffectGrant { get; set; }

  /// <summary>
  /// For BonusDamage type, the type of bonus damage.
  /// The damage amount is in the Value property.
  /// </summary>
  public DamageType? BonusDamageType { get; set; }
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
  /// Gets all on-hit effect grants from this buff's modifiers.
  /// </summary>
  public IEnumerable<AttackEffectGrant> GetOnHitEffects()
  {
    return Modifiers
      .Where(m => m.Type == BuffModifierType.OnHitEffect && m.OnHitEffectGrant != null)
      .Select(m => m.OnHitEffectGrant!);
  }

  /// <summary>
  /// Gets all bonus damage grants from this buff's modifiers.
  /// Returns tuples of (damage amount, damage type).
  /// </summary>
  public IEnumerable<(int Damage, DamageType Type)> GetBonusDamage()
  {
    return Modifiers
      .Where(m => m.Type == BuffModifierType.BonusDamage && m.BonusDamageType.HasValue)
      .Select(m => (GetEffectiveValue(m), m.BonusDamageType!.Value));
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

  /// <summary>
  /// Creates an attack buff that grants effects applied to targets on hit and/or bonus damage.
  /// </summary>
  /// <param name="name">Name of the buff (e.g., "Fire Punch", "Shocking Grasp").</param>
  /// <param name="durationRounds">Duration of the buff in rounds.</param>
  /// <param name="onHitEffect">Effect to apply to targets on attack hit (optional).</param>
  /// <param name="bonusDamage">Bonus damage to add to attacks (optional).</param>
  /// <param name="bonusDamageType">Type of bonus damage (required if bonusDamage > 0).</param>
  /// <param name="source">The source of this buff (spell name, item, etc.).</param>
  public static SpellBuffState CreateAttackBuff(
    string name,
    int durationRounds,
    AttackEffectGrant? onHitEffect = null,
    int bonusDamage = 0,
    DamageType? bonusDamageType = null,
    string? source = null)
  {
    var modifiers = new List<BuffModifier>();
    var descriptionParts = new List<string>();

    if (onHitEffect != null)
    {
      modifiers.Add(new BuffModifier
      {
        Type = BuffModifierType.OnHitEffect,
        Target = "Attack",
        OnHitEffectGrant = onHitEffect
      });
      descriptionParts.Add($"applies {onHitEffect.EffectName} on hit");
    }

    if (bonusDamage > 0 && bonusDamageType.HasValue)
    {
      modifiers.Add(new BuffModifier
      {
        Type = BuffModifierType.BonusDamage,
        Target = "Attack",
        Value = bonusDamage,
        BonusDamageType = bonusDamageType.Value
      });
      descriptionParts.Add($"+{bonusDamage} {bonusDamageType.Value} damage");
    }

    return new SpellBuffState
    {
      BuffName = name,
      Description = descriptionParts.Count > 0 ? string.Join(", ", descriptionParts) : "Attack enhancement",
      Source = source ?? name,
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

    // If TotalDurationRounds == 0 and no modifiers, this effect was created with EffectState
    // format (e.g., via the GM effect form). Use epoch-based expiry and EffectState healing.
    if (state.TotalDurationRounds == 0 && state.Modifiers.Count == 0)
    {
      var effectState = EffectState.Deserialize(effect.BehaviorState);
      if (effectState.FatHealingPerTick.HasValue && effectState.FatHealingPerTick.Value > 0)
        character.Fatigue.PendingHealing += effectState.FatHealingPerTick.Value;
      if (effectState.VitHealingPerTick.HasValue && effectState.VitHealingPerTick.Value > 0)
        character.Vitality.PendingHealing += effectState.VitHealingPerTick.Value;
      if (effectState.FatDamagePerTick.HasValue && effectState.FatDamagePerTick.Value > 0)
        character.Fatigue.PendingDamage += effectState.FatDamagePerTick.Value;
      if (effectState.VitDamagePerTick.HasValue && effectState.VitDamagePerTick.Value > 0)
        character.Vitality.PendingDamage += effectState.VitDamagePerTick.Value;
      return EffectTickResult.Continue();
    }

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

    // Fall back to EffectState modifiers for GM-form-created buffs
    if (state.TotalDurationRounds == 0 && state.Modifiers.Count == 0)
    {
      var effectState = EffectState.Deserialize(effect.BehaviorState);
      var modifier = effectState.GetAttributeModifier(attributeName);
      if (modifier != 0)
      {
        yield return new EffectModifier
        {
          Description = effect.Name,
          Value = modifier,
          TargetAttribute = attributeName
        };
      }
      yield break;
    }

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

    // Fall back to EffectState modifiers for GM-form-created buffs
    if (state.TotalDurationRounds == 0 && state.Modifiers.Count == 0)
    {
      var effectState = EffectState.Deserialize(effect.BehaviorState);
      if (effectState.ASModifier.HasValue && effectState.ASModifier.Value != 0)
      {
        yield return new EffectModifier
        {
          Description = effect.Name,
          Value = effectState.ASModifier.Value,
          TargetSkill = skillName
        };
      }
      var skillModifier = effectState.GetSkillModifier(skillName);
      if (skillModifier != 0)
      {
        yield return new EffectModifier
        {
          Description = $"{effect.Name} ({skillName})",
          Value = skillModifier,
          TargetSkill = skillName
        };
      }
      yield break;
    }

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
