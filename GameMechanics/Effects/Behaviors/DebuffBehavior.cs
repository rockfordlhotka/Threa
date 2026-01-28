using System;
using System.Collections.Generic;
using System.Text.Json;
using Threa.Dal.Dto;

namespace GameMechanics.Effects.Behaviors;

/// <summary>
/// State data stored in BehaviorState for debuff effects.
/// Used for combat penalties like Physicality failure debuffs.
/// </summary>
public class DebuffState
{
  /// <summary>
  /// Penalty to attack ability scores (negative value).
  /// Applied to all attack-type skill checks.
  /// </summary>
  public int AttackPenalty { get; set; }

  /// <summary>
  /// Penalty to defense ability scores (negative value).
  /// Applied to all defense-type skill checks.
  /// </summary>
  public int DefensePenalty { get; set; }

  /// <summary>
  /// Penalty to all ability scores (negative value).
  /// Applied to all skill checks.
  /// </summary>
  public int GlobalPenalty { get; set; }

  public string Serialize() => JsonSerializer.Serialize(this);

  public static DebuffState Deserialize(string? json)
  {
    if (string.IsNullOrEmpty(json))
      return new DebuffState();
    try
    {
      return JsonSerializer.Deserialize<DebuffState>(json) ?? new DebuffState();
    }
    catch
    {
      return new DebuffState();
    }
  }
}

/// <summary>
/// Behavior for temporary debuff effects.
/// Handles combat penalties from failed Physicality checks, etc.
/// </summary>
public class DebuffBehavior : IEffectBehavior
{
  public EffectType EffectType => EffectType.Debuff;

  public EffectAddResult OnAdding(EffectRecord effect, CharacterEdit character)
  {
    // Debuffs stack - don't replace existing ones
    return EffectAddResult.AddNormally();
  }

  public void OnApply(EffectRecord effect, CharacterEdit character)
  {
    // Initialize state if not set
    if (string.IsNullOrEmpty(effect.BehaviorState))
    {
      var state = new DebuffState();
      effect.BehaviorState = state.Serialize();
    }
  }

  public EffectTickResult OnTick(EffectRecord effect, CharacterEdit character)
  {
    // Debuffs expire based on duration
#pragma warning disable CS0618 // Legacy round-based expiration check for backward compatibility
    if (effect.DurationRounds.HasValue && effect.ElapsedRounds >= effect.DurationRounds.Value)
#pragma warning restore CS0618
    {
      return EffectTickResult.ExpireEarly("Debuff expired");
    }
    return EffectTickResult.Continue();
  }

  public void OnExpire(EffectRecord effect, CharacterEdit character)
  {
    // No special cleanup needed
  }

  public void OnRemove(EffectRecord effect, CharacterEdit character)
  {
    // No special cleanup needed
  }

  public IEnumerable<EffectModifier> GetAttributeModifiers(EffectRecord effect, string attributeName, int baseValue)
  {
    // Debuffs don't modify base attributes
    return [];
  }

  public IEnumerable<EffectModifier> GetAbilityScoreModifiers(EffectRecord effect, string skillName, string attributeName, int currentAS)
  {
    var state = DebuffState.Deserialize(effect.BehaviorState);

    // Apply global penalty to all skills
    if (state.GlobalPenalty != 0)
    {
      yield return new EffectModifier
      {
        Description = effect.Name,
        Value = state.GlobalPenalty,
        TargetSkill = skillName
      };
    }

    // Apply attack penalty (for now, applies to all skills - could be refined based on skill type)
    if (state.AttackPenalty != 0)
    {
      yield return new EffectModifier
      {
        Description = $"{effect.Name} (Attack)",
        Value = state.AttackPenalty,
        TargetSkill = skillName
      };
    }

    // Apply defense penalty
    if (state.DefensePenalty != 0)
    {
      yield return new EffectModifier
      {
        Description = $"{effect.Name} (Defense)",
        Value = state.DefensePenalty,
        TargetSkill = skillName
      };
    }
  }

  public IEnumerable<EffectModifier> GetSuccessValueModifiers(EffectRecord effect, string actionType, int currentSV)
  {
    // Debuffs modify AS, not SV directly
    return [];
  }
}
