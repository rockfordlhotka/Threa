using Csla;
using GameMechanics.Effects;
using GameMechanics.Effects.Behaviors;
using GameMechanics.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using Threa.Dal.Dto;

namespace GameMechanics;

/// <summary>
/// Collection of active effects on a character.
/// Handles effect lifecycle, stacking rules, and modifier aggregation.
/// </summary>
[Serializable]
public class EffectList : BusinessListBase<EffectList, EffectRecord>
{
  #region Character Access

  internal CharacterEdit Character => (CharacterEdit)Parent;

  #endregion

  #region Effect Management

  /// <summary>
  /// Adds a new effect to the character, handling stacking rules.
  /// </summary>
  /// <param name="effect">The effect to add.</param>
  /// <returns>True if the effect was added, false if rejected.</returns>
  public bool AddEffect(EffectRecord effect)
  {
    var behavior = effect.Behavior;
    var result = behavior.OnAdding(effect, Character);

    switch (result.Outcome)
    {
      case EffectAddOutcome.Add:
        Add(effect);
        behavior.OnApply(effect, Character);
        return true;

      case EffectAddOutcome.Reject:
        // Effect was merged into existing or rejected
        return false;

      case EffectAddOutcome.Replace:
        if (result.ReplaceEffectId.HasValue)
        {
          var existing = this.FirstOrDefault(e => e.Id == result.ReplaceEffectId.Value);
          if (existing != null)
          {
            existing.Behavior.OnRemove(existing, Character);
            Remove(existing);
          }
        }
        Add(effect);
        behavior.OnApply(effect, Character);
        return true;

      case EffectAddOutcome.AddWithSideEffects:
        Add(effect);
        behavior.OnApply(effect, Character);
        // Side effects would need to be created via the data portal
        // This is handled by the caller
        return true;

      default:
        return false;
    }
  }

  /// <summary>
  /// Removes an effect by ID, calling OnRemove.
  /// </summary>
  /// <param name="effectId">The effect ID to remove.</param>
  /// <returns>True if removed, false if not found.</returns>
  public bool RemoveEffect(Guid effectId)
  {
    var effect = this.FirstOrDefault(e => e.Id == effectId);
    if (effect == null)
      return false;

    effect.Behavior.OnRemove(effect, Character);
    Remove(effect);
    return true;
  }

  /// <summary>
  /// Removes all effects of a specific type.
  /// </summary>
  /// <param name="effectType">The effect type to remove.</param>
  public void RemoveEffectsByType(EffectType effectType)
  {
    var toRemove = this.Where(e => e.EffectType == effectType).ToList();
    foreach (var effect in toRemove)
    {
      effect.Behavior.OnRemove(effect, Character);
      Remove(effect);
    }
  }

  #endregion

  #region Lifecycle

  /// <summary>
  /// Processes all effects for end of round.
  /// Increments elapsed time, calls OnTick, and handles expiration.
  /// </summary>
  public void EndOfRound()
  {
    var toExpire = new List<EffectRecord>();
    var toRemoveEarly = new List<EffectRecord>();

    foreach (var effect in this.Where(e => e.IsActive))
    {
      // Increment elapsed rounds
      effect.ElapsedRounds++;

      // Call OnTick
      var tickResult = effect.Behavior.OnTick(effect, Character);
      if (tickResult.ShouldExpireEarly)
      {
        toRemoveEarly.Add(effect);
        continue;
      }

      // Check for natural expiration
      if (effect.IsExpired)
      {
        toExpire.Add(effect);
      }
    }

    // Handle early expirations (treated as removes)
    foreach (var effect in toRemoveEarly)
    {
      effect.Behavior.OnRemove(effect, Character);
      Remove(effect);
    }

    // Handle natural expirations
    foreach (var effect in toExpire)
    {
      effect.Behavior.OnExpire(effect, Character);
      Remove(effect);
    }
  }

  #endregion

  #region Modifier Aggregation

  /// <summary>
  /// Gets the total modifier to apply to an attribute from all active effects.
  /// </summary>
  /// <param name="attributeName">The attribute name.</param>
  /// <param name="baseValue">The base attribute value.</param>
  /// <returns>The total modifier value.</returns>
  public int GetAttributeModifier(string attributeName, int baseValue)
  {
    return ((IEnumerable<EffectRecord>)this)
      .Where(e => e.IsActive)
      .SelectMany(e => e.GetAttributeModifiers(attributeName, baseValue))
      .Sum(m => m.Value);
  }

  /// <summary>
  /// Gets all attribute modifiers from active effects (for breakdown display).
  /// </summary>
  /// <param name="attributeName">The attribute name.</param>
  /// <param name="baseValue">The base attribute value.</param>
  /// <returns>All modifiers affecting this attribute.</returns>
  public IEnumerable<EffectModifier> GetAttributeModifiers(string attributeName, int baseValue)
  {
    return ((IEnumerable<EffectRecord>)this)
      .Where(e => e.IsActive)
      .SelectMany(e => e.GetAttributeModifiers(attributeName, baseValue));
  }

  /// <summary>
  /// Gets the total modifier to apply to an ability score from all active effects.
  /// </summary>
  /// <param name="skillName">The skill name.</param>
  /// <param name="attributeName">The attribute name.</param>
  /// <param name="currentAS">The current ability score before effect modifiers.</param>
  /// <returns>The total modifier value.</returns>
  public int GetAbilityScoreModifier(string skillName, string attributeName, int currentAS)
  {
    return ((IEnumerable<EffectRecord>)this)
      .Where(e => e.IsActive)
      .SelectMany(e => e.GetAbilityScoreModifiers(skillName, attributeName, currentAS))
      .Sum(m => m.Value);
  }

  /// <summary>
  /// Gets all ability score modifiers from active effects (for breakdown display).
  /// </summary>
  /// <param name="skillName">The skill name.</param>
  /// <param name="attributeName">The attribute name.</param>
  /// <param name="currentAS">The current ability score before effect modifiers.</param>
  /// <returns>All modifiers affecting this ability score.</returns>
  public IEnumerable<EffectModifier> GetAbilityScoreModifiers(string skillName, string attributeName, int currentAS)
  {
    return ((IEnumerable<EffectRecord>)this)
      .Where(e => e.IsActive)
      .SelectMany(e => e.GetAbilityScoreModifiers(skillName, attributeName, currentAS));
  }

  /// <summary>
  /// Gets the total modifier to apply to a success value from all active effects.
  /// </summary>
  /// <param name="actionType">The action type.</param>
  /// <param name="currentSV">The current success value before effect modifiers.</param>
  /// <returns>The total modifier value.</returns>
  public int GetSuccessValueModifier(string actionType, int currentSV)
  {
    return ((IEnumerable<EffectRecord>)this)
      .Where(e => e.IsActive)
      .SelectMany(e => e.GetSuccessValueModifiers(actionType, currentSV))
      .Sum(m => m.Value);
  }

  /// <summary>
  /// Gets all success value modifiers from active effects (for breakdown display).
  /// </summary>
  /// <param name="actionType">The action type.</param>
  /// <param name="currentSV">The current success value before effect modifiers.</param>
  /// <returns>All modifiers affecting this success value.</returns>
  public IEnumerable<EffectModifier> GetSuccessValueModifiers(string actionType, int currentSV)
  {
    return ((IEnumerable<EffectRecord>)this)
      .Where(e => e.IsActive)
      .SelectMany(e => e.GetSuccessValueModifiers(actionType, currentSV));
  }

  #endregion

  #region Wound Helpers

  /// <summary>
  /// Converts damage to wounds and applies them to random locations.
  /// </summary>
  /// <param name="damageValue">The damage value.</param>
  /// <param name="effectPortal">Portal for creating wound effects.</param>
  public void TakeDamage(DamageValue damageValue, IChildDataPortal<EffectRecord> effectPortal)
  {
    var dmg = damageValue.GetModifiedDamage(Character.DamageClass);
    int woundCount = 0;

    if (dmg >= 7 && dmg <= 9)
      woundCount = 1;
    else if (dmg >= 10 && dmg <= 14)
      woundCount = 2;
    else if (dmg >= 15 && dmg <= 19)
      woundCount = 3;
    else if (dmg > 19)
      woundCount = dmg / 5;

    for (int i = 0; i < woundCount; i++)
    {
      var location = GetRandomLocation();
      WoundBehavior.TakeWound(Character, location, effectPortal);
    }
  }

  /// <summary>
  /// Gets the wound state for a specific body location.
  /// </summary>
  /// <param name="location">The body location.</param>
  /// <returns>The wound state, or null if no wounds at that location.</returns>
  public WoundState? GetWoundState(string location)
  {
    return WoundBehavior.GetLocationState(Character, location);
  }

  /// <summary>
  /// Gets all wound states across all body locations.
  /// </summary>
  /// <returns>Enumerable of location/state pairs.</returns>
  public IEnumerable<(string Location, WoundState State)> GetAllWoundStates()
  {
    return WoundBehavior.GetAllWoundStates(Character);
  }

  /// <summary>
  /// Gets the total number of wounds across all locations.
  /// </summary>
  public int TotalWoundCount => GetAllWoundStates().Sum(w => w.State.TotalWounds);

  /// <summary>
  /// Rolls a random body location for wound placement.
  /// </summary>
  /// <returns>The body location name.</returns>
  public static string GetRandomLocation()
  {
    var location = Dice.Roll(1, 12);
    if (location == 12)
    {
      location = Dice.Roll(1, 12);
      if (location == 12)
        return "Head";
      else
        return GetRandomLocation();
    }
    else if (location > 9)
    {
      return "RightLeg";
    }
    else if (location > 7)
    {
      return "LeftLeg";
    }
    else if (location > 5)
    {
      return "RightArm";
    }
    else if (location > 3)
    {
      return "LeftArm";
    }
    else
    {
      return "Torso";
    }
  }

  #endregion

  #region Poison Helpers

  /// <summary>
  /// Applies a poison to the character.
  /// </summary>
  /// <param name="poisonState">The poison configuration.</param>
  /// <param name="effectPortal">Portal for creating the effect.</param>
  public void ApplyPoison(PoisonState poisonState, IChildDataPortal<EffectRecord> effectPortal)
  {
    PoisonBehavior.ApplyPoison(Character, poisonState, effectPortal);
  }

  /// <summary>
  /// Checks if the character is currently poisoned.
  /// </summary>
  public bool IsPoisoned => PoisonBehavior.IsPoisoned(Character);

  /// <summary>
  /// Gets all active poison states.
  /// </summary>
  public IEnumerable<(string Name, PoisonState State)> GetActivePoisons()
  {
    return PoisonBehavior.GetActivePoisons(Character);
  }

  /// <summary>
  /// Gets the total AS penalty from all active poisons.
  /// </summary>
  public int TotalPoisonPenalty => PoisonBehavior.GetTotalPoisonPenalty(Character);

  #endregion

  #region Query Helpers

  /// <summary>
  /// Gets effects at a specific body location.
  /// </summary>
  /// <param name="location">The body location.</param>
  /// <returns>Effects at that location.</returns>
  public IEnumerable<EffectRecord> GetEffectsAtLocation(string location)
  {
    return this.Where(e => e.Location == location);
  }

  /// <summary>
  /// Gets effects of a specific type.
  /// </summary>
  /// <param name="effectType">The effect type.</param>
  /// <returns>Effects of that type.</returns>
  public IEnumerable<EffectRecord> GetEffectsByType(EffectType effectType)
  {
    return this.Where(e => e.EffectType == effectType);
  }

  /// <summary>
  /// Checks if the character has any effect of a specific type.
  /// </summary>
  /// <param name="effectType">The effect type.</param>
  /// <returns>True if any active effect of that type exists.</returns>
  public bool HasEffect(EffectType effectType)
  {
    return this.Any(e => e.EffectType == effectType && e.IsActive);
  }

  /// <summary>
  /// Checks if the character has an effect with a specific name.
  /// </summary>
  /// <param name="effectName">The effect name.</param>
  /// <returns>True if any active effect with that name exists.</returns>
  public bool HasEffect(string effectName)
  {
    return this.Any(e => e.Name == effectName && e.IsActive);
  }

  #endregion

  #region Data Access

  [CreateChild]
  private void Create()
  {
    // Empty list on create
  }

  [FetchChild]
  private void Fetch(List<CharacterEffect>? effects, [Inject] IChildDataPortal<EffectRecord> effectPortal)
  {
    if (effects == null) return;
    using (LoadListMode)
    {
      foreach (var item in effects)
        Add(effectPortal.FetchChild(item));
    }
  }

  #endregion
}
