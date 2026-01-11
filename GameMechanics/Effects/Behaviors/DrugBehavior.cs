using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Threa.Dal.Dto;

namespace GameMechanics.Effects.Behaviors;

/// <summary>
/// Drug category for interaction checking.
/// </summary>
public enum DrugCategory
{
  /// <summary>
  /// No specific category.
  /// </summary>
  None,

  /// <summary>
  /// Stimulants increase alertness and energy.
  /// </summary>
  Stimulant,

  /// <summary>
  /// Sedatives calm and reduce activity.
  /// </summary>
  Sedative,

  /// <summary>
  /// Hallucinogens alter perception.
  /// </summary>
  Hallucinogen,

  /// <summary>
  /// Painkillers reduce pain sensation.
  /// </summary>
  Painkiller,

  /// <summary>
  /// Performance enhancers boost physical abilities.
  /// </summary>
  PerformanceEnhancer,

  /// <summary>
  /// Healing compounds restore health.
  /// </summary>
  Healing,

  /// <summary>
  /// Magical potions with arcane effects.
  /// </summary>
  MagicalPotion
}

/// <summary>
/// Defines the type of crash/withdrawal effect when a drug expires.
/// </summary>
public enum CrashType
{
  /// <summary>
  /// No crash effect.
  /// </summary>
  None,

  /// <summary>
  /// Instant damage on expiration.
  /// </summary>
  InstantDamage,

  /// <summary>
  /// Creates a lingering debuff effect.
  /// </summary>
  LingeringDebuff,

  /// <summary>
  /// Creates a light wound.
  /// </summary>
  Wound
}

/// <summary>
/// Configuration for crash/withdrawal effects.
/// </summary>
public class CrashEffect
{
  /// <summary>
  /// Type of crash effect.
  /// </summary>
  public CrashType Type { get; set; } = CrashType.None;

  /// <summary>
  /// Fatigue damage on crash (for InstantDamage or LingeringDebuff).
  /// </summary>
  public int FatigueDamage { get; set; }

  /// <summary>
  /// Vitality damage on crash (for InstantDamage or LingeringDebuff).
  /// </summary>
  public int VitalityDamage { get; set; }

  /// <summary>
  /// Duration of lingering debuff in rounds.
  /// </summary>
  public int DebuffDurationRounds { get; set; } = 60;

  /// <summary>
  /// AS penalty during crash debuff.
  /// </summary>
  public int ASPenalty { get; set; }

  /// <summary>
  /// Description of the crash effect.
  /// </summary>
  public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Configuration for overdose effects.
/// </summary>
public class OverdoseEffect
{
  /// <summary>
  /// Immediate fatigue damage from overdose.
  /// </summary>
  public int FatigueDamage { get; set; }

  /// <summary>
  /// Immediate vitality damage from overdose.
  /// </summary>
  public int VitalityDamage { get; set; }

  /// <summary>
  /// Whether overdose causes a wound.
  /// </summary>
  public bool CausesWound { get; set; }

  /// <summary>
  /// Duration of overdose debuff in rounds.
  /// </summary>
  public int DebuffDurationRounds { get; set; } = 100;

  /// <summary>
  /// AS penalty during overdose.
  /// </summary>
  public int ASPenalty { get; set; } = -4;

  /// <summary>
  /// Description of overdose effects.
  /// </summary>
  public string Description { get; set; } = "Overdose";
}

/// <summary>
/// Defines a drug interaction rule.
/// </summary>
public class DrugInteraction
{
  /// <summary>
  /// Category that triggers this interaction (if any).
  /// </summary>
  public DrugCategory? IncompatibleCategory { get; set; }

  /// <summary>
  /// Specific drug name that triggers this interaction (if any).
  /// </summary>
  public string? IncompatibleDrugName { get; set; }

  /// <summary>
  /// Severity of the adverse reaction (1-10 scale).
  /// </summary>
  public int Severity { get; set; } = 5;

  /// <summary>
  /// Description of what happens.
  /// </summary>
  public string Description { get; set; } = "Adverse reaction";
}

/// <summary>
/// State data stored in BehaviorState for drug effects.
/// </summary>
public class DrugState
{
  /// <summary>
  /// Display name of the drug.
  /// </summary>
  public string DrugName { get; set; } = "Drug";

  /// <summary>
  /// Description of the drug's effects.
  /// </summary>
  public string Description { get; set; } = string.Empty;

  /// <summary>
  /// Category of this drug (for interaction checking).
  /// </summary>
  public DrugCategory Category { get; set; } = DrugCategory.None;

  /// <summary>
  /// Additional tags for specific interaction checking.
  /// </summary>
  public List<string> Tags { get; set; } = [];

  /// <summary>
  /// The modifiers this drug applies (same structure as spell buffs).
  /// </summary>
  public List<BuffModifier> Modifiers { get; set; } = [];

  /// <summary>
  /// Total duration in rounds.
  /// </summary>
  public int TotalDurationRounds { get; set; }

  /// <summary>
  /// Rounds elapsed since drug was taken.
  /// </summary>
  public int ElapsedRounds { get; set; }

  /// <summary>
  /// Number of safe doses before overdose risk.
  /// </summary>
  public int SafeDoses { get; set; } = 1;

  /// <summary>
  /// Current number of active doses.
  /// </summary>
  public int CurrentDoses { get; set; } = 1;

  /// <summary>
  /// Configuration for what happens on overdose.
  /// </summary>
  public OverdoseEffect? Overdose { get; set; }

  /// <summary>
  /// Configuration for crash/withdrawal effects.
  /// </summary>
  public CrashEffect? Crash { get; set; }

  /// <summary>
  /// Interactions with other drugs/categories.
  /// </summary>
  public List<DrugInteraction> Interactions { get; set; } = [];

  /// <summary>
  /// Difficulty to remove with medicine/antidote (TV for skill check).
  /// </summary>
  public int RemovalDifficulty { get; set; } = 8;

  /// <summary>
  /// Whether this drug is currently in overdose state.
  /// </summary>
  public bool IsOverdosed { get; set; }

  /// <summary>
  /// Whether this drug is currently crashing.
  /// </summary>
  public bool IsCrashing { get; set; }

  /// <summary>
  /// Rounds until next healing tick (for healing drugs).
  /// </summary>
  public int RoundsUntilHealTick { get; set; }

  /// <summary>
  /// Gets whether the character is over the safe dose limit.
  /// </summary>
  public bool IsOverSafeLimit => CurrentDoses > SafeDoses;

  /// <summary>
  /// Serializes this state to JSON for storage in BehaviorState.
  /// </summary>
  public string Serialize() => JsonSerializer.Serialize(this);

  /// <summary>
  /// Deserializes drug state from BehaviorState JSON.
  /// </summary>
  public static DrugState Deserialize(string? json)
  {
    if (string.IsNullOrEmpty(json))
      return new DrugState();
    return JsonSerializer.Deserialize<DrugState>(json) ?? new DrugState();
  }

  #region Factory Methods

  /// <summary>
  /// Creates a simple stimulant drug.
  /// </summary>
  public static DrugState CreateStimulant(string name, int asBonus, int durationRounds)
  {
    return new DrugState
    {
      DrugName = name,
      Description = $"Stimulant: +{asBonus} AS, increased alertness",
      Category = DrugCategory.Stimulant,
      TotalDurationRounds = durationRounds,
      SafeDoses = 1,
      Modifiers =
      [
        new BuffModifier
        {
          Type = BuffModifierType.AbilityScoreGlobal,
          Target = "All",
          Value = asBonus
        }
      ],
      Crash = new CrashEffect
      {
        Type = CrashType.InstantDamage,
        FatigueDamage = asBonus * 2,
        ASPenalty = -asBonus,
        Description = "Stimulant crash"
      },
      Overdose = new OverdoseEffect
      {
        FatigueDamage = 5,
        VitalityDamage = 2,
        ASPenalty = -4,
        DebuffDurationRounds = 60,
        Description = "Stimulant overdose - heart racing"
      },
      Interactions =
      [
        new DrugInteraction
        {
          IncompatibleCategory = DrugCategory.Sedative,
          Severity = 7,
          Description = "Mixing stimulants and sedatives causes heart strain"
        }
      ]
    };
  }

  /// <summary>
  /// Creates a sedative/painkiller drug.
  /// </summary>
  public static DrugState CreateSedative(string name, int painReduction, int durationRounds)
  {
    return new DrugState
    {
      DrugName = name,
      Description = $"Sedative: reduces wound penalties by {painReduction}",
      Category = DrugCategory.Sedative,
      TotalDurationRounds = durationRounds,
      SafeDoses = 1,
      Modifiers =
      [
        new BuffModifier
        {
          Type = BuffModifierType.AbilityScoreGlobal,
          Target = "All",
          Value = painReduction // Counteracts wound penalties
        }
      ],
      Crash = new CrashEffect
      {
        Type = CrashType.LingeringDebuff,
        FatigueDamage = 3,
        ASPenalty = -2,
        DebuffDurationRounds = 30,
        Description = "Sedative wearing off - grogginess"
      },
      Overdose = new OverdoseEffect
      {
        FatigueDamage = 10,
        ASPenalty = -6,
        DebuffDurationRounds = 120,
        Description = "Sedative overdose - respiratory depression"
      },
      Interactions =
      [
        new DrugInteraction
        {
          IncompatibleCategory = DrugCategory.Stimulant,
          Severity = 7,
          Description = "Mixing sedatives and stimulants causes heart strain"
        },
        new DrugInteraction
        {
          IncompatibleCategory = DrugCategory.Sedative,
          Severity = 8,
          Description = "Stacking sedatives risks unconsciousness"
        }
      ]
    };
  }

  /// <summary>
  /// Creates a performance enhancer (combat drug).
  /// </summary>
  public static DrugState CreateCombatDrug(string name, int strBonus, int dexBonus, int durationRounds)
  {
    return new DrugState
    {
      DrugName = name,
      Description = $"Combat drug: +{strBonus} STR, +{dexBonus} DEX",
      Category = DrugCategory.PerformanceEnhancer,
      TotalDurationRounds = durationRounds,
      SafeDoses = 1,
      Modifiers =
      [
        new BuffModifier { Type = BuffModifierType.Attribute, Target = "STR", Value = strBonus },
        new BuffModifier { Type = BuffModifierType.Attribute, Target = "DEX", Value = dexBonus }
      ],
      Crash = new CrashEffect
      {
        Type = CrashType.Wound,
        FatigueDamage = 5,
        VitalityDamage = 3,
        Description = "Combat drug crash - muscle tears"
      },
      Overdose = new OverdoseEffect
      {
        FatigueDamage = 8,
        VitalityDamage = 5,
        CausesWound = true,
        ASPenalty = -5,
        DebuffDurationRounds = 100,
        Description = "Combat drug overdose - severe strain"
      }
    };
  }

  /// <summary>
  /// Creates a healing potion.
  /// </summary>
  public static DrugState CreateHealingPotion(string name, int healPerTick, int tickInterval, int durationRounds, bool healsFatigue = true, bool healsVitality = true)
  {
    var modifiers = new List<BuffModifier>();

    if (healsFatigue)
    {
      modifiers.Add(new BuffModifier
      {
        Type = BuffModifierType.HealingOverTime,
        Target = "FAT",
        Value = healPerTick,
        HealIntervalRounds = tickInterval
      });
    }

    if (healsVitality)
    {
      modifiers.Add(new BuffModifier
      {
        Type = BuffModifierType.HealingOverTime,
        Target = "VIT",
        Value = healPerTick,
        HealIntervalRounds = tickInterval
      });
    }

    return new DrugState
    {
      DrugName = name,
      Description = $"Healing potion: restores {healPerTick} health every {tickInterval} rounds",
      Category = DrugCategory.Healing,
      TotalDurationRounds = durationRounds,
      SafeDoses = 2, // Can safely take 2 healing potions
      RoundsUntilHealTick = tickInterval,
      Modifiers = modifiers,
      Overdose = new OverdoseEffect
      {
        FatigueDamage = 3,
        ASPenalty = -2,
        DebuffDurationRounds = 30,
        Description = "Healing potion overdose - nausea"
      }
    };
  }

  /// <summary>
  /// Creates a hallucinogenic drug.
  /// </summary>
  public static DrugState CreateHallucinogen(string name, int ittBonus, int awarenessBonus, int durationRounds)
  {
    return new DrugState
    {
      DrugName = name,
      Description = $"Hallucinogen: +{ittBonus} ITT, +{awarenessBonus} to Awareness",
      Category = DrugCategory.Hallucinogen,
      TotalDurationRounds = durationRounds,
      SafeDoses = 1,
      Modifiers =
      [
        new BuffModifier { Type = BuffModifierType.Attribute, Target = "ITT", Value = ittBonus },
        new BuffModifier { Type = BuffModifierType.AbilityScoreSkill, Target = "Awareness", Value = awarenessBonus }
      ],
      Crash = new CrashEffect
      {
        Type = CrashType.LingeringDebuff,
        FatigueDamage = 2,
        ASPenalty = -3,
        DebuffDurationRounds = 60,
        Description = "Coming down - confusion and fatigue"
      },
      Overdose = new OverdoseEffect
      {
        FatigueDamage = 4,
        VitalityDamage = 1,
        ASPenalty = -6,
        DebuffDurationRounds = 200,
        Description = "Bad trip - severe disorientation"
      },
      Interactions =
      [
        new DrugInteraction
        {
          IncompatibleCategory = DrugCategory.Hallucinogen,
          Severity = 9,
          Description = "Mixing hallucinogens causes dangerous psychotic episode"
        }
      ]
    };
  }

  #endregion
}

/// <summary>
/// State for the crash/withdrawal debuff effect.
/// </summary>
public class CrashDebuffState
{
  public string SourceDrugName { get; set; } = string.Empty;
  public int ASPenalty { get; set; }
  public int FatigueDamagePerTick { get; set; }
  public int TickIntervalRounds { get; set; } = 10;
  public int RoundsUntilNextTick { get; set; }
  public int TotalDurationRounds { get; set; }
  public int ElapsedRounds { get; set; }

  public string Serialize() => JsonSerializer.Serialize(this);

  public static CrashDebuffState Deserialize(string? json)
  {
    if (string.IsNullOrEmpty(json))
      return new CrashDebuffState();
    return JsonSerializer.Deserialize<CrashDebuffState>(json) ?? new CrashDebuffState();
  }
}

/// <summary>
/// State for the overdose debuff effect.
/// </summary>
public class OverdoseDebuffState
{
  public string SourceDrugName { get; set; } = string.Empty;
  public int ASPenalty { get; set; }
  public int FatigueDamagePerTick { get; set; }
  public int VitalityDamagePerTick { get; set; }
  public int TickIntervalRounds { get; set; } = 10;
  public int RoundsUntilNextTick { get; set; }
  public int TotalDurationRounds { get; set; }
  public int ElapsedRounds { get; set; }

  public string Serialize() => JsonSerializer.Serialize(this);

  public static OverdoseDebuffState Deserialize(string? json)
  {
    if (string.IsNullOrEmpty(json))
      return new OverdoseDebuffState();
    return JsonSerializer.Deserialize<OverdoseDebuffState>(json) ?? new OverdoseDebuffState();
  }
}

/// <summary>
/// State for adverse reaction debuff from drug interactions.
/// </summary>
public class AdverseReactionState
{
  public string Drug1Name { get; set; } = string.Empty;
  public string Drug2Name { get; set; } = string.Empty;
  public int Severity { get; set; }
  public int ASPenalty { get; set; }
  public int FatigueDamagePerTick { get; set; }
  public int VitalityDamagePerTick { get; set; }
  public int TickIntervalRounds { get; set; } = 5;
  public int RoundsUntilNextTick { get; set; }
  public int TotalDurationRounds { get; set; }
  public int ElapsedRounds { get; set; }
  public string Description { get; set; } = string.Empty;

  public string Serialize() => JsonSerializer.Serialize(this);

  public static AdverseReactionState Deserialize(string? json)
  {
    if (string.IsNullOrEmpty(json))
      return new AdverseReactionState();
    return JsonSerializer.Deserialize<AdverseReactionState>(json) ?? new AdverseReactionState();
  }
}

/// <summary>
/// Behavior implementation for drug effects.
/// Drugs have complex mechanics including overdose, crashes, and interactions.
/// They require medicine/antidotes to remove (not dispellable).
/// </summary>
public class DrugBehavior : IEffectBehavior
{
  public EffectType EffectType => EffectType.SpellEffect; // Using SpellEffect for drugs (could add Drug type later)

  public EffectAddResult OnAdding(EffectRecord effect, CharacterEdit character)
  {
    var newState = DrugState.Deserialize(effect.BehaviorState);

    // Check for drug interactions first
    var interactionResult = CheckInteractions(newState, character);
    if (interactionResult != null)
    {
      // Apply the new drug anyway, but also create an adverse reaction
      return EffectAddResult.AddWithSideEffects(
        $"Adverse reaction: {interactionResult.Description}",
        interactionResult);
    }

    // Check for existing dose of the same drug
    var existingDrug = FindExistingDrug(character, newState.DrugName);
    if (existingDrug != null)
    {
      var existingState = DrugState.Deserialize(existingDrug.BehaviorState);

      // Increase dose count
      existingState.CurrentDoses++;
      existingState.ElapsedRounds = 0; // Refresh duration

      // Check for overdose
      if (existingState.IsOverSafeLimit && !existingState.IsOverdosed)
      {
        existingState.IsOverdosed = true;
        existingDrug.BehaviorState = existingState.Serialize();

        // Return info about overdose - caller should create overdose debuff
        return EffectAddResult.Reject(
          $"Overdose! {existingState.Overdose?.Description ?? "Too much " + newState.DrugName}");
      }

      existingDrug.BehaviorState = existingState.Serialize();
      return EffectAddResult.Reject($"{newState.DrugName} dose increased to {existingState.CurrentDoses}");
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

  private DrugInteraction? CheckInteractions(DrugState newDrug, CharacterEdit character)
  {
    var activeDrugs = GetActiveDrugs(character).ToList();

    foreach (var (name, existingDrug) in activeDrugs)
    {
      // Check new drug's interactions against existing drugs
      foreach (var interaction in newDrug.Interactions)
      {
        if (interaction.IncompatibleCategory.HasValue &&
            interaction.IncompatibleCategory == existingDrug.Category)
        {
          return interaction;
        }

        if (!string.IsNullOrEmpty(interaction.IncompatibleDrugName) &&
            interaction.IncompatibleDrugName.Equals(existingDrug.DrugName, StringComparison.OrdinalIgnoreCase))
        {
          return interaction;
        }
      }

      // Also check existing drug's interactions against new drug
      foreach (var interaction in existingDrug.Interactions)
      {
        if (interaction.IncompatibleCategory.HasValue &&
            interaction.IncompatibleCategory == newDrug.Category)
        {
          return interaction;
        }

        if (!string.IsNullOrEmpty(interaction.IncompatibleDrugName) &&
            interaction.IncompatibleDrugName.Equals(newDrug.DrugName, StringComparison.OrdinalIgnoreCase))
        {
          return interaction;
        }
      }
    }

    return null;
  }

  public void OnApply(EffectRecord effect, CharacterEdit character)
  {
    // Drugs don't have immediate effects beyond modifiers
  }

  public EffectTickResult OnTick(EffectRecord effect, CharacterEdit character)
  {
    var state = DrugState.Deserialize(effect.BehaviorState);

    // Note: We don't increment ElapsedRounds here - EffectList.EndOfRound handles that
    // We also don't check for expiration here - let IsExpired handle natural expiration
    // so that OnExpire gets called (which triggers crash effects)

    // Handle healing over time
    var healingModifiers = state.Modifiers.Where(m => m.Type == BuffModifierType.HealingOverTime).ToList();
    if (healingModifiers.Count > 0)
    {
      state.RoundsUntilHealTick--;
      if (state.RoundsUntilHealTick <= 0)
      {
        ApplyHealing(healingModifiers, character);
        state.RoundsUntilHealTick = healingModifiers[0].HealIntervalRounds;
      }
      effect.BehaviorState = state.Serialize();
    }

    return EffectTickResult.Continue();
  }

  private void ApplyHealing(List<BuffModifier> healingModifiers, CharacterEdit character)
  {
    foreach (var mod in healingModifiers)
    {
      if (mod.Value <= 0) continue;

      if (mod.Target == "FAT")
      {
        character.Fatigue.PendingHealing += mod.Value;
      }
      else if (mod.Target == "VIT")
      {
        character.Vitality.PendingHealing += mod.Value;
      }
    }
  }

  public void OnExpire(EffectRecord effect, CharacterEdit character)
  {
    var state = DrugState.Deserialize(effect.BehaviorState);

    // Apply crash effects
    if (state.Crash != null && state.Crash.Type != CrashType.None)
    {
      ApplyCrashEffect(state, character);
    }
  }

  private void ApplyCrashEffect(DrugState state, CharacterEdit character)
  {
    var crash = state.Crash!;

    switch (crash.Type)
    {
      case CrashType.InstantDamage:
        character.Fatigue.PendingDamage += crash.FatigueDamage;
        character.Vitality.PendingDamage += crash.VitalityDamage;
        break;

      case CrashType.LingeringDebuff:
        // Note: We can't create a new effect here without the portal
        // The crash damage is applied immediately as a compromise
        // A proper implementation would need the effect portal passed in
        character.Fatigue.PendingDamage += crash.FatigueDamage;
        character.Vitality.PendingDamage += crash.VitalityDamage;
        // The AS penalty would need a separate crash debuff effect
        break;

      case CrashType.Wound:
        character.Fatigue.PendingDamage += crash.FatigueDamage;
        character.Vitality.PendingDamage += crash.VitalityDamage;
        // Wound would be created via VIT overflow mechanics
        break;
    }
  }

  public void OnRemove(EffectRecord effect, CharacterEdit character)
  {
    // Drug was neutralized with antidote - no crash effects
  }

  public IEnumerable<EffectModifier> GetAttributeModifiers(EffectRecord effect, string attributeName, int baseValue)
  {
    var state = DrugState.Deserialize(effect.BehaviorState);

    // Don't apply positive modifiers if overdosed
    if (state.IsOverdosed)
      yield break;

    var attributeModifiers = state.Modifiers
      .Where(m => m.Type == BuffModifierType.Attribute &&
                  m.Target.Equals(attributeName, StringComparison.OrdinalIgnoreCase));

    foreach (var mod in attributeModifiers)
    {
      if (mod.Value != 0)
      {
        yield return new EffectModifier
        {
          Description = state.DrugName,
          Value = mod.Value,
          TargetAttribute = attributeName
        };
      }
    }
  }

  public IEnumerable<EffectModifier> GetAbilityScoreModifiers(EffectRecord effect, string skillName, string attributeName, int currentAS)
  {
    var state = DrugState.Deserialize(effect.BehaviorState);

    // If overdosed, apply penalty instead of bonus
    if (state.IsOverdosed && state.Overdose != null)
    {
      yield return new EffectModifier
      {
        Description = $"{state.DrugName} (Overdose)",
        Value = state.Overdose.ASPenalty
      };
      yield break;
    }

    foreach (var mod in state.Modifiers)
    {
      if (mod.Value == 0) continue;

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
          Description = state.DrugName,
          Value = mod.Value,
          TargetSkill = skillName
        };
      }
    }
  }

  public IEnumerable<EffectModifier> GetSuccessValueModifiers(EffectRecord effect, string actionType, int currentSV)
  {
    var state = DrugState.Deserialize(effect.BehaviorState);

    if (state.IsOverdosed)
      yield break;

    foreach (var mod in state.Modifiers)
    {
      if (mod.Value == 0) continue;

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
          Description = state.DrugName,
          Value = mod.Value
        };
      }
    }
  }

  #region Static Helpers

  /// <summary>
  /// Applies a drug to a character.
  /// </summary>
  /// <returns>Tuple of (success, message) indicating if drug was applied and any warnings.</returns>
  public static (bool Success, string Message) ApplyDrug(
    CharacterEdit character,
    DrugState drugState,
    Csla.IChildDataPortal<EffectRecord> effectPortal)
  {
    var effect = effectPortal.CreateChild(
      EffectType.SpellEffect, // Using SpellEffect type for drugs
      drugState.DrugName,
      null,
      drugState.TotalDurationRounds,
      drugState.Serialize());

    // Mark as drug type via tag in the state
    drugState.Tags.Add("Drug");
    effect.BehaviorState = drugState.Serialize();

    var behavior = new DrugBehavior();
    var addResult = behavior.OnAdding(effect, character);

    switch (addResult.Outcome)
    {
      case EffectAddOutcome.Add:
        character.Effects.Add(effect);
        return (true, $"{drugState.DrugName} applied");

      case EffectAddOutcome.Reject:
        // Check if it was an overdose
        if (addResult.Message?.Contains("Overdose") == true)
        {
          // Create overdose debuff
          ApplyOverdoseDebuff(character, drugState, effectPortal);
        }
        return (false, addResult.Message ?? "Drug not applied");

      case EffectAddOutcome.AddWithSideEffects:
        // Add the drug
        character.Effects.Add(effect);
        // Create adverse reaction
        if (addResult.SideEffectData is DrugInteraction interaction)
        {
          ApplyAdverseReaction(character, drugState.DrugName, interaction, effectPortal);
        }
        return (true, $"{drugState.DrugName} applied with adverse reaction: {addResult.Message}");

      default:
        return (false, "Unknown result");
    }
  }

  /// <summary>
  /// Creates an overdose debuff effect.
  /// </summary>
  public static void ApplyOverdoseDebuff(
    CharacterEdit character,
    DrugState sourceDrug,
    Csla.IChildDataPortal<EffectRecord> effectPortal)
  {
    var overdose = sourceDrug.Overdose;
    if (overdose == null) return;

    // Apply immediate damage
    character.Fatigue.PendingDamage += overdose.FatigueDamage;
    character.Vitality.PendingDamage += overdose.VitalityDamage;

    // Create debuff state
    var debuffState = new OverdoseDebuffState
    {
      SourceDrugName = sourceDrug.DrugName,
      ASPenalty = overdose.ASPenalty,
      TotalDurationRounds = overdose.DebuffDurationRounds,
      TickIntervalRounds = 10,
      RoundsUntilNextTick = 10
    };

    var debuffEffect = effectPortal.CreateChild(
      EffectType.Debuff,
      $"{sourceDrug.DrugName} Overdose",
      null,
      overdose.DebuffDurationRounds,
      debuffState.Serialize());

    character.Effects.Add(debuffEffect);
  }

  /// <summary>
  /// Creates an adverse reaction debuff from drug interaction.
  /// </summary>
  public static void ApplyAdverseReaction(
    CharacterEdit character,
    string newDrugName,
    DrugInteraction interaction,
    Csla.IChildDataPortal<EffectRecord> effectPortal)
  {
    // Severity affects damage and duration
    var damage = interaction.Severity;
    var duration = interaction.Severity * 10;

    // Apply immediate damage based on severity
    character.Fatigue.PendingDamage += damage;
    if (interaction.Severity >= 7)
    {
      character.Vitality.PendingDamage += damage / 2;
    }

    // Create adverse reaction debuff
    var reactionState = new AdverseReactionState
    {
      Drug1Name = newDrugName,
      Description = interaction.Description,
      Severity = interaction.Severity,
      ASPenalty = -interaction.Severity / 2,
      TotalDurationRounds = duration,
      TickIntervalRounds = 5,
      RoundsUntilNextTick = 5,
      FatigueDamagePerTick = interaction.Severity >= 5 ? 1 : 0,
      VitalityDamagePerTick = interaction.Severity >= 8 ? 1 : 0
    };

    var reactionEffect = effectPortal.CreateChild(
      EffectType.Debuff,
      "Adverse Reaction",
      null,
      duration,
      reactionState.Serialize());

    character.Effects.Add(reactionEffect);
  }

  /// <summary>
  /// Finds an existing drug effect by name.
  /// </summary>
  private static EffectRecord? FindExistingDrug(CharacterEdit character, string drugName)
  {
    return character.Effects
      .Where(e => e.IsActive)
      .FirstOrDefault(e =>
      {
        var state = DrugState.Deserialize(e.BehaviorState);
        return state.DrugName.Equals(drugName, StringComparison.OrdinalIgnoreCase) &&
               state.Tags.Contains("Drug");
      });
  }

  /// <summary>
  /// Gets all active drug effects on a character.
  /// </summary>
  public static IEnumerable<(string Name, DrugState State)> GetActiveDrugs(CharacterEdit character)
  {
    return character.Effects
      .Where(e => e.IsActive)
      .Select(e => (e.Name, DrugState.Deserialize(e.BehaviorState)))
      .Where(x => x.Item2.Tags.Contains("Drug"));
  }

  /// <summary>
  /// Checks if a character is under the influence of any drug.
  /// </summary>
  public static bool IsUnderInfluence(CharacterEdit character)
  {
    return GetActiveDrugs(character).Any();
  }

  /// <summary>
  /// Checks if a character has overdosed on any drug.
  /// </summary>
  public static bool HasOverdosed(CharacterEdit character)
  {
    return GetActiveDrugs(character).Any(d => d.State.IsOverdosed);
  }

  /// <summary>
  /// Attempts to neutralize a drug with an antidote.
  /// </summary>
  /// <param name="character">The character.</param>
  /// <param name="drugName">The drug to neutralize.</param>
  /// <param name="antidotePower">The power/quality of the antidote.</param>
  /// <returns>True if the drug was neutralized.</returns>
  public static bool TryNeutralize(CharacterEdit character, string drugName, int antidotePower)
  {
    var drugEffect = FindExistingDrug(character, drugName);
    if (drugEffect == null)
      return false;

    var state = DrugState.Deserialize(drugEffect.BehaviorState);

    // Antidote must be strong enough
    if (antidotePower >= state.RemovalDifficulty)
    {
      // Remove without crash effects
      character.Effects.RemoveEffect(drugEffect.Id);
      return true;
    }

    return false;
  }

  #endregion
}
