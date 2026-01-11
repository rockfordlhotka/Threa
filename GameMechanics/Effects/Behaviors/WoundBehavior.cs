using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Threa.Dal.Dto;

namespace GameMechanics.Effects.Behaviors;

/// <summary>
/// State data stored in BehaviorState for wound effects.
/// </summary>
public class WoundState
{
  /// <summary>
  /// Number of light wounds at this location.
  /// Light wounds are healed serious wounds that still cause some impairment.
  /// </summary>
  public int LightWounds { get; set; }

  /// <summary>
  /// Number of serious wounds at this location.
  /// Serious wounds cause significant impairment and periodic damage.
  /// </summary>
  public int SeriousWounds { get; set; }

  /// <summary>
  /// Maximum wounds this location can sustain before being destroyed.
  /// </summary>
  public int MaxWounds { get; set; }

  /// <summary>
  /// Whether this body part is crippled (serious wounds >= max).
  /// </summary>
  public bool IsCrippled { get; set; }

  /// <summary>
  /// Whether this body part is destroyed (serious wounds > max).
  /// </summary>
  public bool IsDestroyed { get; set; }

  /// <summary>
  /// Rounds remaining until next periodic damage application.
  /// Resets to 20 after each application.
  /// </summary>
  public int RoundsToDamage { get; set; }

  /// <summary>
  /// Total wounds (light + serious) at this location.
  /// </summary>
  public int TotalWounds => LightWounds + SeriousWounds;

  /// <summary>
  /// Whether this location is disabled (total wounds >= max - 1).
  /// </summary>
  public bool IsDisabled => TotalWounds >= MaxWounds - 1;

  /// <summary>
  /// Serializes this state to JSON for storage in BehaviorState.
  /// </summary>
  public string Serialize() => JsonSerializer.Serialize(this);

  /// <summary>
  /// Deserializes wound state from BehaviorState JSON.
  /// </summary>
  public static WoundState Deserialize(string? json)
  {
    if (string.IsNullOrEmpty(json))
      return new WoundState();
    return JsonSerializer.Deserialize<WoundState>(json) ?? new WoundState();
  }

  /// <summary>
  /// Gets the max wounds for a body location.
  /// </summary>
  public static int GetMaxWoundsForLocation(string location) => location switch
  {
    "Head" => 2,
    "Torso" => 4,
    "LeftArm" or "RightArm" or "LeftLeg" or "RightLeg" => 2,
    _ => 2
  };
}

/// <summary>
/// Behavior implementation for wound effects.
/// Wounds are location-based injuries that cause global AS penalties
/// and periodic FAT/VIT damage.
/// </summary>
public class WoundBehavior : IEffectBehavior
{
  public EffectType EffectType => EffectType.Wound;

  /// <summary>
  /// The global AS penalty per wound (-2 per design spec).
  /// </summary>
  public const int PenaltyPerWound = -2;

  /// <summary>
  /// Rounds between periodic damage applications.
  /// </summary>
  public const int DamageIntervalRounds = 20;

  public EffectAddResult OnAdding(EffectRecord effect, CharacterEdit character)
  {
    // Check if there's already a wound effect at this location
    var existingWound = character.Effects
      .Where(e => e.EffectType == EffectType.Wound && e.Location == effect.Location)
      .FirstOrDefault();

    if (existingWound != null)
    {
      // Wounds at the same location stack - add to existing
      var existingState = WoundState.Deserialize(existingWound.BehaviorState);
      var newState = WoundState.Deserialize(effect.BehaviorState);

      existingState.SeriousWounds += newState.SeriousWounds;
      existingState.LightWounds += newState.LightWounds;

      // Update crippled/destroyed status
      if (existingState.SeriousWounds >= existingState.MaxWounds)
        existingState.IsCrippled = true;
      if (existingState.SeriousWounds > existingState.MaxWounds)
        existingState.IsDestroyed = true;

      // Start or continue the damage countdown
      if (existingState.RoundsToDamage == 0 && existingState.TotalWounds > 0)
        existingState.RoundsToDamage = DamageIntervalRounds;

      existingWound.BehaviorState = existingState.Serialize();

      return EffectAddResult.Reject($"Added wounds to existing {effect.Location} wound effect");
    }

    // New wound location - ensure state is initialized
    var state = WoundState.Deserialize(effect.BehaviorState);
    if (state.MaxWounds == 0)
      state.MaxWounds = WoundState.GetMaxWoundsForLocation(effect.Location ?? "Torso");
    if (state.RoundsToDamage == 0 && state.TotalWounds > 0)
      state.RoundsToDamage = DamageIntervalRounds;

    // Update crippled/destroyed status
    if (state.SeriousWounds >= state.MaxWounds)
      state.IsCrippled = true;
    if (state.SeriousWounds > state.MaxWounds)
      state.IsDestroyed = true;

    effect.BehaviorState = state.Serialize();

    return EffectAddResult.AddNormally();
  }

  public void OnApply(EffectRecord effect, CharacterEdit character)
  {
    // Wounds don't have immediate effects beyond what OnAdding sets up
  }

  public EffectTickResult OnTick(EffectRecord effect, CharacterEdit character)
  {
    var state = WoundState.Deserialize(effect.BehaviorState);

    if (state.TotalWounds == 0)
      return EffectTickResult.Continue();

    if (state.RoundsToDamage > 0)
    {
      state.RoundsToDamage--;

      if (state.RoundsToDamage == 0)
      {
        // Apply periodic damage from wounds
        // Serious wounds deal VIT damage and 2x FAT damage
        // Light wounds deal FAT damage only
        character.Vitality.PendingDamage += state.SeriousWounds;
        character.Fatigue.PendingDamage += state.SeriousWounds * 2;
        character.Fatigue.PendingDamage += state.LightWounds;

        // Reset countdown
        state.RoundsToDamage = DamageIntervalRounds;
      }

      effect.BehaviorState = state.Serialize();
    }

    return EffectTickResult.Continue();
  }

  public void OnExpire(EffectRecord effect, CharacterEdit character)
  {
    // Wounds don't expire naturally - they must be healed
  }

  public void OnRemove(EffectRecord effect, CharacterEdit character)
  {
    // No special cleanup needed when wound is removed (healed)
  }

  public IEnumerable<EffectModifier> GetAttributeModifiers(EffectRecord effect, string attributeName, int baseValue)
  {
    // Wounds don't directly modify attributes
    return [];
  }

  public IEnumerable<EffectModifier> GetAbilityScoreModifiers(EffectRecord effect, string skillName, string attributeName, int currentAS)
  {
    var state = WoundState.Deserialize(effect.BehaviorState);

    if (state.TotalWounds == 0)
      return [];

    // Each wound applies a -2 penalty to ALL ability scores
    var totalPenalty = state.TotalWounds * PenaltyPerWound;

    return
    [
      new EffectModifier
      {
        Description = $"Wound: {effect.Location} ({state.TotalWounds})",
        Value = totalPenalty
      }
    ];
  }

  public IEnumerable<EffectModifier> GetSuccessValueModifiers(EffectRecord effect, string actionType, int currentSV)
  {
    // Wounds don't modify SV directly
    return [];
  }

  /// <summary>
  /// Applies a wound to a location, adding to existing wound effect or creating new one.
  /// </summary>
  public static void TakeWound(CharacterEdit character, string location, Csla.IChildDataPortal<EffectRecord> effectPortal)
  {
    var existingWound = character.Effects
      .Where(e => e.EffectType == EffectType.Wound && e.Location == location)
      .FirstOrDefault();

    if (existingWound != null)
    {
      var state = WoundState.Deserialize(existingWound.BehaviorState);
      state.SeriousWounds++;

      if (state.RoundsToDamage == 0)
        state.RoundsToDamage = DamageIntervalRounds;
      if (state.SeriousWounds >= state.MaxWounds)
        state.IsCrippled = true;
      if (state.SeriousWounds > state.MaxWounds)
        state.IsDestroyed = true;

      existingWound.BehaviorState = state.Serialize();
    }
    else
    {
      var state = new WoundState
      {
        SeriousWounds = 1,
        MaxWounds = WoundState.GetMaxWoundsForLocation(location),
        RoundsToDamage = DamageIntervalRounds
      };

      var newWound = effectPortal.CreateChild(
        EffectType.Wound,
        $"Wound: {location}",
        location,
        null, // Wounds don't expire by duration
        state.Serialize());

      character.Effects.Add(newWound);
    }
  }

  /// <summary>
  /// Heals a wound at a location (serious -> light -> removed).
  /// </summary>
  public static void HealWound(CharacterEdit character, string location)
  {
    var wound = character.Effects
      .Where(e => e.EffectType == EffectType.Wound && e.Location == location)
      .FirstOrDefault();

    if (wound == null)
      return;

    var state = WoundState.Deserialize(wound.BehaviorState);

    if (state.SeriousWounds > 0)
    {
      state.SeriousWounds--;
      state.LightWounds++;
    }
    else if (state.LightWounds > 0)
    {
      state.LightWounds--;
    }

    if (state.TotalWounds == 0)
    {
      // Fully healed - remove the wound effect
      character.Effects.Remove(wound);
    }
    else
    {
      // Still wounded - update state
      if (state.SeriousWounds < state.MaxWounds)
        state.IsCrippled = false;
      state.IsDestroyed = false;
      wound.BehaviorState = state.Serialize();
    }
  }

  /// <summary>
  /// Gets the wound state for a specific location, or null if no wounds.
  /// </summary>
  public static WoundState? GetLocationState(CharacterEdit character, string location)
  {
    var wound = character.Effects
      .Where(e => e.EffectType == EffectType.Wound && e.Location == location)
      .FirstOrDefault();

    if (wound == null)
      return null;

    return WoundState.Deserialize(wound.BehaviorState);
  }

  /// <summary>
  /// Gets all wound states across all locations.
  /// </summary>
  public static IEnumerable<(string Location, WoundState State)> GetAllWoundStates(CharacterEdit character)
  {
    return character.Effects
      .Where(e => e.EffectType == EffectType.Wound && !string.IsNullOrEmpty(e.Location))
      .Select(e => (e.Location!, WoundState.Deserialize(e.BehaviorState)));
  }
}
