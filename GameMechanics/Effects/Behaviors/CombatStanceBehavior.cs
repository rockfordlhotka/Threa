using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Threa.Dal.Dto;

namespace GameMechanics.Effects.Behaviors;

/// <summary>
/// Combat stance state stored in BehaviorState.
/// </summary>
public class CombatStanceState
{
    /// <summary>
    /// Type of combat stance (e.g., "Parry", "Defensive", "Aggressive").
    /// </summary>
    public string StanceType { get; set; } = "Parry";

    /// <summary>
    /// The skill name used for this stance (e.g., weapon skill for parry).
    /// </summary>
    public string SkillName { get; set; } = "";

    /// <summary>
    /// The AS value for the stance skill at time of entering.
    /// </summary>
    public int SkillAS { get; set; }

    /// <summary>
    /// Number of free defenses used this round (for tracking).
    /// </summary>
    public int DefensesThisRound { get; set; }

    public string Serialize() => JsonSerializer.Serialize(this);

    public static CombatStanceState Deserialize(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return new CombatStanceState();
        try
        {
            return JsonSerializer.Deserialize<CombatStanceState>(json) ?? new CombatStanceState();
        }
        catch
        {
            return new CombatStanceState();
        }
    }
}

/// <summary>
/// Behavior for combat stances like Parry Mode.
///
/// Per COMBAT_SYSTEM.md:
/// - Parry mode costs 1 AP + 1 FAT (or 2 AP) to enter
/// - While in parry mode, parry defenses are FREE
/// - Only works against melee attacks (not ranged)
/// - Parry mode ends when the character takes any non-parry action
/// </summary>
public class CombatStanceBehavior : IEffectBehavior
{
    public const string ParryStance = "Parry";
    public const string ParryModeName = "Parry Mode";
    public const string DodgeFocusStance = "DodgeFocus";
    public const string DodgeFocusName = "Dodge Focus";
    public const string BlockWithShieldStance = "BlockWithShield";
    public const string BlockWithShieldName = "Block with Shield";

    public EffectType EffectType => EffectType.CombatStance;

    public EffectAddResult OnAdding(EffectRecord effect, CharacterEdit character)
    {
        // Only one stance can be active at a time -- replace ANY existing CombatStance
        var existingStance = character.Effects.GetEffectsByType(EffectType.CombatStance)
            .FirstOrDefault();

        if (existingStance != null)
        {
            return EffectAddResult.Replace(existingStance.Id);
        }

        return EffectAddResult.AddNormally();
    }

    public void OnApply(EffectRecord effect, CharacterEdit character)
    {
        // Initialize state if not set
        if (string.IsNullOrEmpty(effect.BehaviorState))
        {
            var state = new CombatStanceState();
            effect.BehaviorState = state.Serialize();
        }
    }

    public EffectTickResult OnTick(EffectRecord effect, CharacterEdit character)
    {
        // Reset defenses count at end of round
        var state = CombatStanceState.Deserialize(effect.BehaviorState);
        state.DefensesThisRound = 0;
        effect.BehaviorState = state.Serialize();

        // Combat stances persist until explicitly ended or broken by action
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
        return [];
    }

    public IEnumerable<EffectModifier> GetAbilityScoreModifiers(EffectRecord effect, string skillName, string attributeName, int currentAS)
    {
        // Combat stances don't modify AS directly
        return [];
    }

    public IEnumerable<EffectModifier> GetSuccessValueModifiers(EffectRecord effect, string actionType, int currentSV)
    {
        return [];
    }

    /// <summary>
    /// Creates the initial BehaviorState for a Parry Mode effect.
    /// </summary>
    /// <param name="weaponSkillName">The weapon skill being used for parry.</param>
    /// <param name="weaponSkillAS">The weapon skill's AS value.</param>
    /// <returns>Serialized behavior state.</returns>
    public static string CreateParryModeState(string weaponSkillName, int weaponSkillAS)
    {
        var state = new CombatStanceState
        {
            StanceType = ParryStance,
            SkillName = weaponSkillName,
            SkillAS = weaponSkillAS,
            DefensesThisRound = 0
        };
        return state.Serialize();
    }

    /// <summary>
    /// Gets the description for a parry mode effect.
    /// </summary>
    public static string GetParryModeDescription(string weaponSkillName)
    {
        return $"Parrying with {weaponSkillName}. Free parry defenses against melee. Ends on non-parry action.";
    }

    /// <summary>
    /// Checks if the character is currently in parry mode.
    /// </summary>
    public static bool IsInParryMode(CharacterEdit character)
    {
        return character.Effects.GetEffectsByType(EffectType.CombatStance)
            .Any(e => e.Name == ParryModeName && e.IsActive);
    }

    /// <summary>
    /// Gets the parry mode effect if active.
    /// </summary>
    public static EffectRecord? GetParryModeEffect(CharacterEdit character)
    {
        return character.Effects.GetEffectsByType(EffectType.CombatStance)
            .FirstOrDefault(e => e.Name == ParryModeName && e.IsActive);
    }

    /// <summary>
    /// Ends parry mode for a character (when they take a non-parry action).
    /// </summary>
    public static void EndParryMode(CharacterEdit character)
    {
        var parryEffect = GetParryModeEffect(character);
        if (parryEffect != null)
        {
            character.Effects.RemoveEffect(parryEffect.Id);
        }
    }

    /// <summary>
    /// Gets the weapon skill AS for parry defense.
    /// </summary>
    public static int GetParrySkillAS(CharacterEdit character)
    {
        var parryEffect = GetParryModeEffect(character);
        if (parryEffect == null) return 0;

        var state = CombatStanceState.Deserialize(parryEffect.BehaviorState);
        return state.SkillAS;
    }

    /// <summary>
    /// Gets the weapon skill name for parry defense.
    /// </summary>
    public static string GetParrySkillName(CharacterEdit character)
    {
        var parryEffect = GetParryModeEffect(character);
        if (parryEffect == null) return "";

        var state = CombatStanceState.Deserialize(parryEffect.BehaviorState);
        return state.SkillName;
    }

    /// <summary>
    /// Checks if the character is in Dodge Focus stance.
    /// </summary>
    public static bool IsInDodgeFocus(CharacterEdit character)
    {
        return character.Effects.GetEffectsByType(EffectType.CombatStance)
            .Any(e => e.Name == DodgeFocusName && e.IsActive);
    }

    /// <summary>
    /// Checks if the character is in Block with Shield stance.
    /// </summary>
    public static bool IsInBlockWithShield(CharacterEdit character)
    {
        return character.Effects.GetEffectsByType(EffectType.CombatStance)
            .Any(e => e.Name == BlockWithShieldName && e.IsActive);
    }

    /// <summary>
    /// Gets the name of the active CombatStance effect, or null if none (Normal stance).
    /// </summary>
    public static string? GetActiveStanceName(CharacterEdit character)
    {
        var activeStance = character.Effects.GetEffectsByType(EffectType.CombatStance)
            .FirstOrDefault(e => e.IsActive);
        return activeStance?.Name;
    }

    /// <summary>
    /// Removes any active CombatStance effect (switches to Normal stance).
    /// </summary>
    public static void ClearStance(CharacterEdit character)
    {
        var stances = character.Effects.GetEffectsByType(EffectType.CombatStance).ToList();
        foreach (var stance in stances)
        {
            character.Effects.RemoveEffect(stance.Id);
        }
    }

    /// <summary>
    /// Creates serialized CombatStanceState for Dodge Focus stance.
    /// </summary>
    public static string CreateDodgeFocusState()
    {
        var state = new CombatStanceState
        {
            StanceType = DodgeFocusStance,
            SkillName = "",
            SkillAS = 0,
            DefensesThisRound = 0
        };
        return state.Serialize();
    }

    /// <summary>
    /// Creates serialized CombatStanceState for Block with Shield stance.
    /// </summary>
    public static string CreateBlockWithShieldState()
    {
        var state = new CombatStanceState
        {
            StanceType = BlockWithShieldStance,
            SkillName = "",
            SkillAS = 0,
            DefensesThisRound = 0
        };
        return state.Serialize();
    }

    /// <summary>
    /// Gets the description for a Dodge Focus effect.
    /// </summary>
    public static string GetDodgeFocusDescription()
    {
        return "Dodge Focus: Pre-selects dodge defense when defending.";
    }

    /// <summary>
    /// Gets the description for a Block with Shield effect.
    /// </summary>
    public static string GetBlockWithShieldDescription()
    {
        return "Block with Shield: Pre-selects shield block defense when defending.";
    }
}
