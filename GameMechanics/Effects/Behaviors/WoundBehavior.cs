using System;
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
  /// The original severity when the wound was created (Minor, Moderate, Severe, Critical).
  /// Used for half-life healing calculation.
  /// </summary>
  public string? OriginalSeverity { get; set; }

  /// <summary>
  /// Custom severity label set by GM (Minor, Moderate, Severe, Critical).
  /// This is the current severity, which may decrease over time as the wound heals.
  /// </summary>
  public string? Severity { get; set; }

  /// <summary>
  /// GM-provided description of the wound.
  /// </summary>
  public string? Description { get; set; }

  /// <summary>
  /// Custom AS penalty override (null = use severity-based calculation).
  /// When set, disables automatic severity-based penalty scaling.
  /// </summary>
  public int? CustomASPenalty { get; set; }

  /// <summary>
  /// Custom FAT damage per tick override (null = use severity-based calculation).
  /// When set, disables automatic severity-based damage scaling.
  /// </summary>
  public int? CustomFatDamagePerTick { get; set; }

  /// <summary>
  /// Custom VIT damage per tick override (null = use severity-based calculation).
  /// When set, disables automatic severity-based damage scaling.
  /// </summary>
  public int? CustomVitDamagePerTick { get; set; }

  /// <summary>
  /// Legacy: FAT damage per tick. Use CustomFatDamagePerTick instead.
  /// Kept for backwards compatibility with existing wounds.
  /// </summary>
  public int? FatDamagePerTick { get; set; }

  /// <summary>
  /// Legacy: VIT damage per tick. Use CustomVitDamagePerTick instead.
  /// Kept for backwards compatibility with existing wounds.
  /// </summary>
  public int? VitDamagePerTick { get; set; }

  /// <summary>
  /// Legacy: epoch-based expiry time. Use EffectRecord.ExpiresAtEpochSeconds instead.
  /// Kept for backwards compatibility with existing wounds.
  /// </summary>
  public long? ExpiryTimeSeconds { get; set; }

  /// <summary>
  /// Cached current severity (updated each tick based on healing progress).
  /// Used for half-life healing model calculations.
  /// </summary>
  public string? CurrentSeverity { get; set; }

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

  /// <summary>
  /// Gets suggested default values based on severity level.
  /// Returns (asPenalty, fatPerTick, vitPerTick).
  /// </summary>
  public static (int asPenalty, int fatPerTick, int vitPerTick) GetSeverityDefaults(string? severity) => severity switch
  {
    "Minor" => (-1, 1, 0),
    "Moderate" => (-2, 2, 1),
    "Severe" => (-4, 3, 2),
    "Critical" => (-6, 4, 3),
    _ => (-1, 1, 0)
  };

  /// <summary>
  /// Gets the default healing time in seconds for a severity level.
  /// Uses a half-life model where severity decreases as the wound heals.
  /// </summary>
  public static long GetDefaultHealingTimeSeconds(string? severity) => severity switch
  {
    "Minor" => 7 * 24 * 3600,      // 1 week
    "Moderate" => 14 * 24 * 3600,  // 2 weeks
    "Severe" => 28 * 24 * 3600,    // 4 weeks
    "Critical" => 56 * 24 * 3600,  // 8 weeks
    _ => 7 * 24 * 3600             // Default 1 week
  };

  /// <summary>
  /// Severity levels in order from least to most severe.
  /// </summary>
  public static readonly string[] SeverityLevels = ["Minor", "Moderate", "Severe", "Critical"];

  /// <summary>
  /// Gets the severity index (0=Minor, 1=Moderate, 2=Severe, 3=Critical).
  /// </summary>
  public static int GetSeverityIndex(string? severity) => severity switch
  {
    "Minor" => 0,
    "Moderate" => 1,
    "Severe" => 2,
    "Critical" => 3,
    _ => 0
  };

  /// <summary>
  /// Calculates the current severity based on healing progress using a half-life model.
  /// At 50% healed, drops one severity level. At 75%, drops two. At 87.5%, drops three.
  /// </summary>
  /// <param name="originalSeverity">The severity when the wound was created</param>
  /// <param name="healingProgress">Progress from 0.0 (just created) to 1.0 (fully healed)</param>
  /// <returns>The current severity level</returns>
  public static string GetCurrentSeverity(string? originalSeverity, double healingProgress)
  {
    if (string.IsNullOrEmpty(originalSeverity))
      return "Minor";

    int originalIndex = GetSeverityIndex(originalSeverity);

    // Half-life model: each severity level covers half the remaining time
    // 0-50%: original, 50-75%: -1, 75-87.5%: -2, 87.5-100%: -3
    int levelsToReduce = 0;
    if (healingProgress >= 0.5) levelsToReduce++;
    if (healingProgress >= 0.75) levelsToReduce++;
    if (healingProgress >= 0.875) levelsToReduce++;

    int currentIndex = Math.Max(0, originalIndex - levelsToReduce);
    return SeverityLevels[currentIndex];
  }

  /// <summary>
  /// Gets the effective current severity, considering healing progress.
  /// If CustomASPenalty is set, returns the stored Severity (no auto-degradation).
  /// </summary>
  public string GetEffectiveSeverity(double healingProgress)
  {
    // If custom values are set, don't auto-degrade
    if (CustomASPenalty.HasValue)
      return Severity ?? "Minor";

    // Use original severity for half-life calculation
    var original = OriginalSeverity ?? Severity ?? "Minor";
    return GetCurrentSeverity(original, healingProgress);
  }
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

    // Check for wound expiry (use EffectRecord's ExpiresAtEpochSeconds, falling back to legacy WoundState.ExpiryTimeSeconds)
    long? expiryTime = effect.ExpiresAtEpochSeconds ?? state.ExpiryTimeSeconds;
    if (expiryTime.HasValue && character.CurrentGameTimeSeconds >= expiryTime.Value)
    {
      // Wound has fully healed
      return EffectTickResult.ExpireEarly("Wound fully healed");
    }

    // Custom wounds with severity use severity-based tick rates
    bool isCustomWound = state.Severity != null;

    if (!isCustomWound && state.TotalWounds == 0)
      return EffectTickResult.Continue();

    // Update cached current severity based on healing progress (half-life model)
    if (isCustomWound && !state.CustomASPenalty.HasValue)
    {
      var progress = effect.GetDurationProgress(character.CurrentGameTimeSeconds) ?? 0.0;
      var originalSeverity = state.OriginalSeverity ?? state.Severity ?? "Minor";
      state.CurrentSeverity = WoundState.GetCurrentSeverity(originalSeverity, progress);
    }

    if (state.RoundsToDamage > 0)
    {
      state.RoundsToDamage--;

      if (state.RoundsToDamage == 0)
      {
        if (isCustomWound)
        {
          // Check for custom overrides first
          bool hasCustomDamage = state.CustomFatDamagePerTick.HasValue || state.CustomVitDamagePerTick.HasValue
                                || state.FatDamagePerTick.HasValue || state.VitDamagePerTick.HasValue;

          int fatDamage, vitDamage;

          if (hasCustomDamage)
          {
            // Use explicit custom damage rates (no auto-degradation)
            fatDamage = state.CustomFatDamagePerTick ?? state.FatDamagePerTick ?? 1;
            vitDamage = state.CustomVitDamagePerTick ?? state.VitDamagePerTick ?? 0;
          }
          else
          {
            // Use cached current severity for damage calculation
            var currentSeverity = state.CurrentSeverity ?? state.Severity ?? "Minor";
            var defaults = WoundState.GetSeverityDefaults(currentSeverity);
            fatDamage = defaults.fatPerTick;
            vitDamage = defaults.vitPerTick;
          }

          character.Fatigue.PendingDamage += fatDamage;
          character.Vitality.PendingDamage += vitDamage;
        }
        else
        {
          // Apply periodic damage from wounds using legacy calculation
          // Serious wounds deal VIT damage and 2x FAT damage
          // Light wounds deal FAT damage only
          character.Vitality.PendingDamage += state.SeriousWounds;
          character.Fatigue.PendingDamage += state.SeriousWounds * 2;
          character.Fatigue.PendingDamage += state.LightWounds;
        }

        // Reset countdown
        state.RoundsToDamage = DamageIntervalRounds;
      }

      effect.BehaviorState = state.Serialize();
    }
    else
    {
      // Even if no damage this tick, save updated CurrentSeverity
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

    // Custom wounds with severity use severity-based AS penalty
    bool isCustomWound = state.Severity != null;

    if (!isCustomWound && state.TotalWounds == 0)
      return [];

    int totalPenalty;
    string description;

    if (isCustomWound)
    {
      if (state.CustomASPenalty.HasValue)
      {
        // Use explicit custom AS penalty (no auto-degradation)
        totalPenalty = state.CustomASPenalty.Value;
        description = $"Wound: {effect.Location} ({state.Severity})";
      }
      else
      {
        // Use cached current severity (updated each tick via half-life model)
        var currentSeverity = state.CurrentSeverity ?? state.Severity ?? "Minor";
        totalPenalty = WoundState.GetSeverityDefaults(currentSeverity).asPenalty;

        // Show both original and current severity if different
        var originalSeverity = state.OriginalSeverity ?? state.Severity;
        if (currentSeverity != originalSeverity)
          description = $"Wound: {effect.Location} ({currentSeverity}, was {originalSeverity})";
        else
          description = $"Wound: {effect.Location} ({currentSeverity})";
      }
    }
    else
    {
      // Each wound applies a -2 penalty to ALL ability scores
      totalPenalty = state.TotalWounds * PenaltyPerWound;
      description = $"Wound: {effect.Location} ({state.TotalWounds})";
    }

    return
    [
      new EffectModifier
      {
        Description = description,
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

  /// <summary>
  /// Creates a custom wound with GM-specified severity and damage rates.
  /// Wounds use a half-life healing model where severity decreases over time.
  /// </summary>
  /// <param name="character">The character to add the wound to</param>
  /// <param name="effectPortal">The effect portal for creating child objects</param>
  /// <param name="location">Body location (Head, Torso, LeftArm, RightArm, LeftLeg, RightLeg)</param>
  /// <param name="severity">Severity level (Minor, Moderate, Severe, Critical)</param>
  /// <param name="description">GM-provided description of the wound</param>
  /// <param name="asPenalty">Custom AS penalty override (null = use severity-based auto-degradation)</param>
  /// <param name="fatPerTick">Custom FAT damage per tick (null = use severity-based auto-degradation)</param>
  /// <param name="vitPerTick">Custom VIT damage per tick (null = use severity-based auto-degradation)</param>
  /// <param name="healingTimeSeconds">Time until fully healed in game seconds (null = use severity default, 0 = manual healing only)</param>
  /// <returns>The created EffectRecord</returns>
  public static EffectRecord CreateCustomWound(
    CharacterEdit character,
    Csla.IChildDataPortal<EffectRecord> effectPortal,
    string location,
    string severity,
    string? description,
    int? asPenalty = null,
    int? fatPerTick = null,
    int? vitPerTick = null,
    long? healingTimeSeconds = null)
  {
    // Determine actual healing time
    long? actualHealingTime = healingTimeSeconds;
    if (!healingTimeSeconds.HasValue)
    {
      // Use severity-based default healing time
      actualHealingTime = WoundState.GetDefaultHealingTimeSeconds(severity);
    }
    else if (healingTimeSeconds.Value == 0)
    {
      // Explicit 0 means manual healing only
      actualHealingTime = null;
    }

    // Only set custom overrides if values differ from severity defaults
    var defaults = WoundState.GetSeverityDefaults(severity);
    int? customAsPenalty = (asPenalty.HasValue && asPenalty.Value != defaults.asPenalty) ? asPenalty : null;
    int? customFatPerTick = (fatPerTick.HasValue && fatPerTick.Value != defaults.fatPerTick) ? fatPerTick : null;
    int? customVitPerTick = (vitPerTick.HasValue && vitPerTick.Value != defaults.vitPerTick) ? vitPerTick : null;

    var state = new WoundState
    {
      OriginalSeverity = severity,
      Severity = severity,
      Description = description,
      CustomASPenalty = customAsPenalty,
      CustomFatDamagePerTick = customFatPerTick,
      CustomVitDamagePerTick = customVitPerTick,
      MaxWounds = WoundState.GetMaxWoundsForLocation(location),
      RoundsToDamage = DamageIntervalRounds,
      SeriousWounds = 1 // Custom wounds count as 1 serious wound for location tracking
    };

    // Create wound with proper expiry time on the EffectRecord
    // Use CreateWithEpochTime for seconds-based duration
    var wound = effectPortal.CreateChild(
      EffectType.Wound,
      $"Wound: {location}",
      location,
      actualHealingTime, // Duration in seconds
      state.Serialize(),
      character.CurrentGameTimeSeconds); // Current game time for epoch calculation

    wound.Description = description;
    wound.Source = "GM";

    character.Effects.Add(wound);
    return wound;
  }
}
