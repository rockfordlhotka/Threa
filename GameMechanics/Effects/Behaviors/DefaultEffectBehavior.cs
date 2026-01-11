using System.Collections.Generic;
using Threa.Dal.Dto;

namespace GameMechanics.Effects.Behaviors;

/// <summary>
/// Default behavior for effect types that don't have a specific implementation.
/// Provides basic duration-based expiration with no special effects.
/// </summary>
public class DefaultEffectBehavior : IEffectBehavior
{
  public EffectType EffectType => EffectType.Condition;

  public EffectAddResult OnAdding(EffectRecord effect, CharacterEdit character)
  {
    return EffectAddResult.AddNormally();
  }

  public void OnApply(EffectRecord effect, CharacterEdit character)
  {
    // No default apply behavior
  }

  public EffectTickResult OnTick(EffectRecord effect, CharacterEdit character)
  {
    return EffectTickResult.Continue();
  }

  public void OnExpire(EffectRecord effect, CharacterEdit character)
  {
    // No default expire behavior
  }

  public void OnRemove(EffectRecord effect, CharacterEdit character)
  {
    // No default remove behavior
  }

  public IEnumerable<EffectModifier> GetAttributeModifiers(EffectRecord effect, string attributeName, int baseValue)
  {
    return [];
  }

  public IEnumerable<EffectModifier> GetAbilityScoreModifiers(EffectRecord effect, string skillName, string attributeName, int currentAS)
  {
    return [];
  }

  public IEnumerable<EffectModifier> GetSuccessValueModifiers(EffectRecord effect, string actionType, int currentSV)
  {
    return [];
  }
}
