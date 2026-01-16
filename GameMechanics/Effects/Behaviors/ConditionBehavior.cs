using System.Collections.Generic;
using Threa.Dal.Dto;

namespace GameMechanics.Effects.Behaviors;

/// <summary>
/// Behavior for Condition effects (Stunned, Blinded, Prone, etc.).
/// Handles special effects based on the condition name.
/// </summary>
public class ConditionBehavior : IEffectBehavior
{
  public EffectType EffectType => EffectType.Condition;

  public EffectAddResult OnAdding(EffectRecord effect, CharacterEdit character)
  {
    // Check if this condition is already active
    var existing = character.Effects.HasEffect(effect.Name);
    if (existing)
    {
      // Most conditions don't stack - refresh duration instead
      return EffectAddResult.Reject("Condition already active");
    }
    return EffectAddResult.AddNormally();
  }

  public void OnApply(EffectRecord effect, CharacterEdit character)
  {
    switch (effect.Name.ToLowerInvariant())
    {
      case "stunned":
        // Stunned sets FAT to 0 and clears pending FAT damage
        character.Fatigue.Value = 0;
        character.Fatigue.PendingDamage = 0;
        break;
      // Add other conditions as needed
    }
  }

  public EffectTickResult OnTick(EffectRecord effect, CharacterEdit character)
  {
    switch (effect.Name.ToLowerInvariant())
    {
      case "stunned":
        // If FAT healing is pending, the stun is broken
        if (character.Fatigue.PendingHealing > 0)
        {
          // Let the healing go through - don't clear it
          return EffectTickResult.ExpireEarly("Stunned broken by FAT healing");
        }
        // While stunned, FAT stays at 0
        if (character.Fatigue.Value > 0)
        {
          character.Fatigue.Value = 0;
        }
        break;
    }
    return EffectTickResult.Continue();
  }

  public void OnExpire(EffectRecord effect, CharacterEdit character)
  {
    // Conditions expire naturally, FAT will recover during subsequent rounds
  }

  public void OnRemove(EffectRecord effect, CharacterEdit character)
  {
    // Same as expire - FAT will recover naturally
  }

  public IEnumerable<EffectModifier> GetAttributeModifiers(EffectRecord effect, string attributeName, int baseValue)
  {
    return [];
  }

  public IEnumerable<EffectModifier> GetAbilityScoreModifiers(EffectRecord effect, string skillName, string attributeName, int currentAS)
  {
    // Some conditions affect ability scores
    switch (effect.Name.ToLowerInvariant())
    {
      case "blinded":
        // Blinded affects perception and ranged combat
        if (skillName.Contains("Perception", System.StringComparison.OrdinalIgnoreCase) ||
            skillName.Contains("Ranged", System.StringComparison.OrdinalIgnoreCase))
        {
          return [new EffectModifier { Description = "Blinded", Value = -4 }];
        }
        break;
      case "prone":
        // Prone affects melee defense
        if (skillName.Contains("Dodge", System.StringComparison.OrdinalIgnoreCase) ||
            skillName.Contains("Parry", System.StringComparison.OrdinalIgnoreCase))
        {
          return [new EffectModifier { Description = "Prone", Value = -2 }];
        }
        break;
    }
    return [];
  }

  public IEnumerable<EffectModifier> GetSuccessValueModifiers(EffectRecord effect, string actionType, int currentSV)
  {
    return [];
  }
}
