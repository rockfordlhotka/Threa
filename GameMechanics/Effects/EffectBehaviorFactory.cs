using System.Collections.Generic;
using Threa.Dal.Dto;

namespace GameMechanics.Effects;

/// <summary>
/// Factory for creating effect behavior instances based on effect type.
/// </summary>
public static class EffectBehaviorFactory
{
  private static readonly Dictionary<EffectType, IEffectBehavior> _behaviors = new()
  {
    { EffectType.Wound, new Behaviors.WoundBehavior() },
    { EffectType.Condition, new Behaviors.ConditionBehavior() },
    { EffectType.Poison, new Behaviors.PoisonBehavior() },
    { EffectType.Buff, new Behaviors.SpellBuffBehavior() },
    { EffectType.SpellEffect, new Behaviors.DrugBehavior() }, // Using SpellEffect for drugs temporarily
    { EffectType.ItemEffect, new Behaviors.ItemEffectBehavior() },
    { EffectType.CombatStance, new Behaviors.CombatStanceBehavior() },
    { EffectType.Debuff, new Behaviors.DebuffBehavior() },
    { EffectType.Concentration, new Behaviors.ConcentrationBehavior() },
    // Generic behavior for types that use EffectState modifiers
    { EffectType.Disease, new Behaviors.GenericEffectBehavior() },
    { EffectType.ObjectEffect, new Behaviors.GenericEffectBehavior() },
    { EffectType.Environmental, new Behaviors.GenericEffectBehavior() }
  };

  private static readonly IEffectBehavior _defaultBehavior = new Behaviors.DefaultEffectBehavior();

  /// <summary>
  /// Gets the behavior implementation for a given effect type.
  /// </summary>
  /// <param name="effectType">The effect type.</param>
  /// <returns>The behavior implementation, or a default if none registered.</returns>
  public static IEffectBehavior GetBehavior(EffectType effectType)
  {
    return _behaviors.TryGetValue(effectType, out var behavior) ? behavior : _defaultBehavior;
  }

  /// <summary>
  /// Registers a behavior for an effect type (for extensibility/testing).
  /// </summary>
  /// <param name="effectType">The effect type.</param>
  /// <param name="behavior">The behavior implementation.</param>
  public static void RegisterBehavior(EffectType effectType, IEffectBehavior behavior)
  {
    _behaviors[effectType] = behavior;
  }
}
