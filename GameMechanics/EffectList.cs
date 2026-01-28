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

  #region Item Effect Management

  /// <summary>
  /// Gets all effects that originated from a specific item.
  /// </summary>
  /// <param name="itemId">The CharacterItem ID.</param>
  /// <returns>Effects from that item.</returns>
  public IEnumerable<EffectRecord> GetEffectsFromItem(Guid itemId)
  {
    return this.Where(e => e.SourceItemId == itemId);
  }

  /// <summary>
  /// Gets all effects that are from items with a specific trigger type.
  /// </summary>
  /// <param name="trigger">The trigger type to filter by.</param>
  /// <returns>Effects with that trigger.</returns>
  public IEnumerable<EffectRecord> GetEffectsByTrigger(ItemEffectTrigger trigger)
  {
    return this.Where(e => e.ItemEffectTrigger == trigger);
  }

  /// <summary>
  /// Removes all effects from a specific item, typically when unequipping or dropping.
  /// </summary>
  /// <param name="itemId">The CharacterItem ID.</param>
  /// <param name="trigger">Optional: only remove effects with this trigger type.</param>
  /// <returns>True if any effects were removed.</returns>
  public bool RemoveEffectsFromItem(Guid itemId, ItemEffectTrigger? trigger = null)
  {
    var toRemove = this
      .Where(e => e.SourceItemId == itemId)
      .Where(e => trigger == null || e.ItemEffectTrigger == trigger)
      .ToList();

    foreach (var effect in toRemove)
    {
      effect.Behavior.OnRemove(effect, Character);
      Remove(effect);
    }

    return toRemove.Count > 0;
  }

  /// <summary>
  /// Removes effects triggered by equipping when an item is unequipped.
  /// Does not remove possession-based effects.
  /// </summary>
  /// <param name="itemId">The CharacterItem ID.</param>
  /// <returns>True if any effects were removed.</returns>
  public bool RemoveEquipEffects(Guid itemId)
  {
    return RemoveEffectsFromItem(itemId, ItemEffectTrigger.WhileEquipped);
  }

  /// <summary>
  /// Removes effects triggered by possession when an item is dropped or transferred.
  /// Also removes equip effects since the item is leaving inventory entirely.
  /// </summary>
  /// <param name="itemId">The CharacterItem ID.</param>
  /// <returns>True if any effects were removed.</returns>
  public bool RemovePossessionEffects(Guid itemId)
  {
    // When dropping, remove both possession AND equip effects
    var toRemove = this
      .Where(e => e.SourceItemId == itemId)
      .Where(e => e.ItemEffectTrigger == ItemEffectTrigger.WhilePossessed 
               || e.ItemEffectTrigger == ItemEffectTrigger.WhileEquipped
               || e.ItemEffectTrigger == ItemEffectTrigger.OnPickup)
      .ToList();

    foreach (var effect in toRemove)
    {
      effect.Behavior.OnRemove(effect, Character);
      Remove(effect);
    }

    return toRemove.Count > 0;
  }

  /// <summary>
  /// Checks if any effect from a specific item is blocking unequip (cursed equip effect).
  /// </summary>
  /// <param name="itemId">The CharacterItem ID.</param>
  /// <returns>True if there's an active curse preventing unequip.</returns>
  public bool IsItemBlockingUnequip(Guid itemId)
  {
    return this.Any(e => e.SourceItemId == itemId && e.IsBlockingUnequip);
  }

  /// <summary>
  /// Checks if any effect from a specific item is blocking drop (cursed possession effect).
  /// </summary>
  /// <param name="itemId">The CharacterItem ID.</param>
  /// <returns>True if there's an active curse preventing drop.</returns>
  public bool IsItemBlockingDrop(Guid itemId)
  {
    return this.Any(e => e.SourceItemId == itemId && e.IsBlockingDrop);
  }

  /// <summary>
  /// Checks if any effect from a specific item is blocking any removal (unequip or drop).
  /// </summary>
  /// <param name="itemId">The CharacterItem ID.</param>
  /// <returns>True if there's any active curse on this item.</returns>
  public bool IsItemCursed(Guid itemId)
  {
    return this.Any(e => e.SourceItemId == itemId && e.IsBlockingItemRemoval);
  }

  /// <summary>
  /// Determines if an item can be unequipped (not blocked by curses).
  /// </summary>
  /// <param name="itemId">The CharacterItem ID.</param>
  /// <returns>True if the item can be unequipped.</returns>
  public bool CanUnequipItem(Guid itemId)
  {
    return !IsItemBlockingUnequip(itemId);
  }

  /// <summary>
  /// Determines if an item can be dropped or transferred (not blocked by curses).
  /// </summary>
  /// <param name="itemId">The CharacterItem ID.</param>
  /// <returns>True if the item can be dropped.</returns>
  public bool CanDropItem(Guid itemId)
  {
    return !IsItemBlockingDrop(itemId);
  }

  /// <summary>
  /// Gets cursed effects from a specific item.
  /// </summary>
  /// <param name="itemId">The CharacterItem ID.</param>
  /// <returns>Cursed effects from that item.</returns>
  public IEnumerable<EffectRecord> GetCursedEffectsFromItem(Guid itemId)
  {
    return this.Where(e => e.SourceItemId == itemId && e.IsCursed && e.IsActive);
  }

  /// <summary>
  /// Gets all currently active item-based effects.
  /// </summary>
  /// <returns>All effects that originated from items.</returns>
  public IEnumerable<EffectRecord> GetAllItemEffects()
  {
    return this.Where(e => e.IsFromItem);
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

  /// <summary>
  /// Processes a time skip (calendar time advancement) for all active effects.
  /// Advances elapsed rounds by the specified amount and expires effects that have reached their duration.
  /// </summary>
  /// <param name="roundsPassed">Number of rounds that passed during the time skip.</param>
  public void ProcessTimeSkip(int roundsPassed)
  {
    var toExpire = new List<EffectRecord>();

    foreach (var effect in this.Where(e => e.IsActive))
    {
      // Advance elapsed rounds by the full time skip amount
      effect.ElapsedRounds += roundsPassed;

      // Check for natural expiration
      if (effect.IsExpired)
      {
        toExpire.Add(effect);
      }
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

  #region Spell Buff Helpers

  /// <summary>
  /// Applies a spell buff to the character.
  /// </summary>
  /// <param name="buffState">The buff configuration.</param>
  /// <param name="effectPortal">Portal for creating the effect.</param>
  /// <returns>True if the buff was applied, false if it failed (e.g., already active).</returns>
  public bool ApplySpellBuff(SpellBuffState buffState, IChildDataPortal<EffectRecord> effectPortal)
  {
    return SpellBuffBehavior.ApplyBuff(Character, buffState, effectPortal);
  }

  /// <summary>
  /// Checks if a specific buff is active.
  /// </summary>
  /// <param name="buffName">The name of the buff.</param>
  /// <returns>True if the buff is active.</returns>
  public bool HasBuff(string buffName)
  {
    return SpellBuffBehavior.HasBuff(Character, buffName);
  }

  /// <summary>
  /// Gets all active spell buffs.
  /// </summary>
  public IEnumerable<(string Name, SpellBuffState State)> GetActiveBuffs()
  {
    return SpellBuffBehavior.GetActiveBuffs(Character);
  }

  /// <summary>
  /// Attempts to dispel a buff.
  /// </summary>
  /// <param name="buffName">The name of the buff to dispel.</param>
  /// <param name="dispelPower">The power of the dispel attempt.</param>
  /// <returns>True if the buff was dispelled.</returns>
  public bool TryDispelBuff(string buffName, int dispelPower)
  {
    return SpellBuffBehavior.TryDispel(Character, buffName, dispelPower);
  }

  #endregion

  #region Drug Helpers

  /// <summary>
  /// Applies a drug to the character.
  /// </summary>
  /// <param name="drugState">The drug configuration.</param>
  /// <param name="effectPortal">Portal for creating the effect.</param>
  /// <returns>Tuple of (success, message) indicating result and any warnings.</returns>
  public (bool Success, string Message) ApplyDrug(DrugState drugState, IChildDataPortal<EffectRecord> effectPortal)
  {
    return DrugBehavior.ApplyDrug(Character, drugState, effectPortal);
  }

  /// <summary>
  /// Checks if the character is under the influence of any drug.
  /// </summary>
  public bool IsUnderInfluence => DrugBehavior.IsUnderInfluence(Character);

  /// <summary>
  /// Checks if the character has overdosed on any drug.
  /// </summary>
  public bool HasOverdosed => DrugBehavior.HasOverdosed(Character);

  /// <summary>
  /// Gets all active drugs.
  /// </summary>
  public IEnumerable<(string Name, DrugState State)> GetActiveDrugs()
  {
    return DrugBehavior.GetActiveDrugs(Character);
  }

  /// <summary>
  /// Attempts to neutralize a drug with an antidote.
  /// </summary>
  /// <param name="drugName">The name of the drug to neutralize.</param>
  /// <param name="antidotePower">The power/quality of the antidote.</param>
  /// <returns>True if the drug was neutralized.</returns>
  public bool TryNeutralizeDrug(string drugName, int antidotePower)
  {
    return DrugBehavior.TryNeutralize(Character, drugName, antidotePower);
  }

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
