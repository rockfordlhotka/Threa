using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Threa.Dal.Dto;

namespace GameMechanics.Effects.Behaviors;

/// <summary>
/// State data stored in BehaviorState for item effects.
/// Uses the same modifier structure as SpellBuffBehavior for consistency.
/// </summary>
public class ItemEffectState
{
  /// <summary>
  /// Display name of the item effect.
  /// </summary>
  public string EffectName { get; set; } = "Item Effect";

  /// <summary>
  /// Description of what the effect does.
  /// </summary>
  public string Description { get; set; } = string.Empty;

  /// <summary>
  /// The item that provides this effect.
  /// </summary>
  public string ItemName { get; set; } = string.Empty;

  /// <summary>
  /// The modifiers this effect applies.
  /// Uses the same BuffModifier structure for consistency.
  /// </summary>
  public List<BuffModifier> Modifiers { get; set; } = [];

  /// <summary>
  /// For healing/damage over time: rounds between ticks.
  /// </summary>
  public int TickIntervalRounds { get; set; } = 10;

  /// <summary>
  /// Rounds until next tick (tracked during gameplay).
  /// </summary>
  public int RoundsUntilTick { get; set; }

  /// <summary>
  /// Whether this effect has been revealed to the character.
  /// Hidden effects (like curses) may not be shown until identified.
  /// </summary>
  public bool IsRevealed { get; set; } = true;

  /// <summary>
  /// Difficulty to identify this effect (for hidden/cursed items).
  /// </summary>
  public int IdentifyDifficulty { get; set; } = 8;

  /// <summary>
  /// Difficulty to remove this effect via Remove Curse or similar.
  /// </summary>
  public int RemovalDifficulty { get; set; } = 10;

  /// <summary>
  /// Serializes this state to JSON for storage in BehaviorState.
  /// </summary>
  public string Serialize() => JsonSerializer.Serialize(this);

  /// <summary>
  /// Deserializes item effect state from BehaviorState JSON.
  /// </summary>
  public static ItemEffectState Deserialize(string? json)
  {
    if (string.IsNullOrEmpty(json))
      return new ItemEffectState();
    try
    {
      return JsonSerializer.Deserialize<ItemEffectState>(json) ?? new ItemEffectState();
    }
    catch
    {
      return new ItemEffectState();
    }
  }

  #region Factory Methods

  /// <summary>
  /// Creates an attribute modifier effect.
  /// </summary>
  public static ItemEffectState CreateAttributeModifier(string itemName, string effectName, string attribute, int bonus)
  {
    return new ItemEffectState
    {
      ItemName = itemName,
      EffectName = effectName,
      Description = bonus >= 0 ? $"+{bonus} to {attribute}" : $"{bonus} to {attribute}",
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
  /// Creates a healing over time effect.
  /// </summary>
  public static ItemEffectState CreateHealingOverTime(string itemName, string effectName, string pool, int healAmount, int intervalRounds = 10)
  {
    return new ItemEffectState
    {
      ItemName = itemName,
      EffectName = effectName,
      Description = $"Heals {healAmount} {pool} every {intervalRounds} rounds",
      TickIntervalRounds = intervalRounds,
      RoundsUntilTick = intervalRounds,
      Modifiers =
      [
        new BuffModifier
        {
          Type = BuffModifierType.HealingOverTime,
          Target = pool,
          Value = healAmount,
          HealIntervalRounds = intervalRounds
        }
      ]
    };
  }

  /// <summary>
  /// Creates a damage over time effect (for cursed items).
  /// </summary>
  public static ItemEffectState CreateDamageOverTime(string itemName, string effectName, string pool, int damageAmount, int intervalRounds = 10, bool isCursed = true)
  {
    return new ItemEffectState
    {
      ItemName = itemName,
      EffectName = effectName,
      Description = $"Deals {damageAmount} {pool} damage every {intervalRounds} rounds",
      TickIntervalRounds = intervalRounds,
      RoundsUntilTick = intervalRounds,
      IsRevealed = !isCursed, // Cursed effects are hidden initially
      Modifiers =
      [
        new BuffModifier
        {
          Type = BuffModifierType.HealingOverTime, // Negative value = damage
          Target = pool,
          Value = -damageAmount,
          HealIntervalRounds = intervalRounds
        }
      ]
    };
  }

  /// <summary>
  /// Creates a skill bonus effect.
  /// </summary>
  public static ItemEffectState CreateSkillBonus(string itemName, string effectName, string skillName, int bonus)
  {
    return new ItemEffectState
    {
      ItemName = itemName,
      EffectName = effectName,
      Description = bonus >= 0 ? $"+{bonus} to {skillName}" : $"{bonus} to {skillName}",
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
  /// Creates a global success value modifier effect.
  /// </summary>
  public static ItemEffectState CreateSuccessValueModifier(string itemName, string effectName, int bonus)
  {
    return new ItemEffectState
    {
      ItemName = itemName,
      EffectName = effectName,
      Description = bonus >= 0 ? $"+{bonus} to all success values" : $"{bonus} to all success values",
      Modifiers =
      [
        new BuffModifier
        {
          Type = BuffModifierType.SuccessValueGlobal,
          Value = bonus
        }
      ]
    };
  }

  #endregion
}

/// <summary>
/// Behavior for effects that come from items (equipment, magic items, tech, implants).
/// Handles the lifecycle of item-based buffs, debuffs, and curses.
/// </summary>
public class ItemEffectBehavior : IEffectBehavior
{
  public EffectType EffectType => EffectType.ItemEffect;

  public EffectAddResult OnAdding(EffectRecord effect, CharacterEdit character)
  {
    // Item effects don't stack from the same item - they replace
    var existingFromSameItem = character.Effects
      .FirstOrDefault(e => e.SourceItemId == effect.SourceItemId 
                        && e.Name == effect.Name 
                        && e.IsActive);

    if (existingFromSameItem != null)
    {
      // Replace the existing effect
      return EffectAddResult.Replace(existingFromSameItem.Id, "Replacing existing item effect");
    }

    return EffectAddResult.AddNormally();
  }

  public void OnApply(EffectRecord effect, CharacterEdit character)
  {
    // Initialize tick counter for healing/damage over time effects
    var state = ItemEffectState.Deserialize(effect.BehaviorState);
    if (state.Modifiers.Exists(m => m.Type == BuffModifierType.HealingOverTime))
    {
      state.RoundsUntilTick = state.TickIntervalRounds;
      effect.BehaviorState = state.Serialize();
    }
  }

  public EffectTickResult OnTick(EffectRecord effect, CharacterEdit character)
  {
    var state = ItemEffectState.Deserialize(effect.BehaviorState);
    var stateChanged = false;

    // Process healing/damage over time
    foreach (var modifier in state.Modifiers.Where(m => m.Type == BuffModifierType.HealingOverTime))
    {
      state.RoundsUntilTick--;
      stateChanged = true;

      if (state.RoundsUntilTick <= 0)
      {
        // Apply the heal/damage using PendingHealing/PendingDamage
        if (modifier.Target?.ToUpperInvariant() == "FAT")
        {
          if (modifier.Value > 0)
            character.Fatigue.PendingHealing += modifier.Value;
          else
            character.Fatigue.PendingDamage += -modifier.Value;
        }
        else if (modifier.Target?.ToUpperInvariant() == "VIT")
        {
          if (modifier.Value > 0)
            character.Vitality.PendingHealing += modifier.Value;
          else
            character.Vitality.PendingDamage += -modifier.Value;
        }

        // Reset tick counter
        state.RoundsUntilTick = state.TickIntervalRounds;
      }
    }

    if (stateChanged)
    {
      effect.BehaviorState = state.Serialize();
    }

    // Item effects don't expire naturally - they last until the item is removed
    return EffectTickResult.Continue();
  }

  public void OnExpire(EffectRecord effect, CharacterEdit character)
  {
    // Item effects with null duration don't expire naturally
    // This is called if there's a duration override
  }

  public void OnRemove(EffectRecord effect, CharacterEdit character)
  {
    // Clean up when item is unequipped/dropped
    // No special cleanup needed for most item effects
  }

  public IEnumerable<EffectModifier> GetAttributeModifiers(EffectRecord effect, string attributeName, int baseValue)
  {
    var state = ItemEffectState.Deserialize(effect.BehaviorState);

    foreach (var modifier in state.Modifiers.Where(m => 
      m.Type == BuffModifierType.Attribute && 
      m.Target?.Equals(attributeName, StringComparison.OrdinalIgnoreCase) == true))
    {
      yield return new EffectModifier
      {
        Description = $"{state.ItemName}: {state.EffectName}",
        Value = modifier.Value,
        TargetAttribute = attributeName
      };
    }
  }

  public IEnumerable<EffectModifier> GetAbilityScoreModifiers(EffectRecord effect, string skillName, string attributeName, int currentAS)
  {
    var state = ItemEffectState.Deserialize(effect.BehaviorState);

    foreach (var modifier in state.Modifiers)
    {
      switch (modifier.Type)
      {
        case BuffModifierType.AbilityScoreGlobal:
          yield return new EffectModifier
          {
            Description = $"{state.ItemName}: {state.EffectName}",
            Value = modifier.Value
          };
          break;

        case BuffModifierType.AbilityScoreSkill when modifier.Target?.Equals(skillName, StringComparison.OrdinalIgnoreCase) == true:
          yield return new EffectModifier
          {
            Description = $"{state.ItemName}: {state.EffectName}",
            Value = modifier.Value,
            TargetSkill = skillName
          };
          break;

        case BuffModifierType.AbilityScoreAttribute when modifier.Target?.Equals(attributeName, StringComparison.OrdinalIgnoreCase) == true:
          yield return new EffectModifier
          {
            Description = $"{state.ItemName}: {state.EffectName}",
            Value = modifier.Value,
            TargetAttribute = attributeName
          };
          break;
      }
    }
  }

  public IEnumerable<EffectModifier> GetSuccessValueModifiers(EffectRecord effect, string actionType, int currentSV)
  {
    var state = ItemEffectState.Deserialize(effect.BehaviorState);

    foreach (var modifier in state.Modifiers)
    {
      switch (modifier.Type)
      {
        case BuffModifierType.SuccessValueGlobal:
          yield return new EffectModifier
          {
            Description = $"{state.ItemName}: {state.EffectName}",
            Value = modifier.Value
          };
          break;

        case BuffModifierType.SuccessValueAction when modifier.Target?.Equals(actionType, StringComparison.OrdinalIgnoreCase) == true:
          yield return new EffectModifier
          {
            Description = $"{state.ItemName}: {state.EffectName}",
            Value = modifier.Value
          };
          break;
      }
    }
  }
}
