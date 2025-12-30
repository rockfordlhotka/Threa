using System;
using System.Collections.Generic;
using System.Linq;
using Threa.Dal.Dto;

namespace GameMechanics.Magic.Effects;

/// <summary>
/// Interface for spell effect classes that resolve the specific mechanics
/// of each spell or spell category.
/// </summary>
public interface ISpellEffect
{
    /// <summary>
    /// Gets the spell skill IDs that this effect class handles.
    /// A single effect class can handle multiple related spells.
    /// </summary>
    IEnumerable<string> HandledSpellIds { get; }

    /// <summary>
    /// Resolves the spell effect after a successful cast.
    /// </summary>
    /// <param name="context">The context containing spell, SV, caster, targets, etc.</param>
    /// <returns>The result of applying the spell effect.</returns>
    SpellEffectResult Resolve(SpellEffectContext context);
}

/// <summary>
/// Context passed to spell effect resolvers containing all information
/// needed to resolve the spell's effects.
/// </summary>
public class SpellEffectContext
{
    /// <summary>
    /// The spell being cast.
    /// </summary>
    public SpellDefinition Spell { get; init; } = null!;

    /// <summary>
    /// The Success Value from the casting check (AV - TV).
    /// Higher SV generally means stronger effect.
    /// </summary>
    public int SV { get; init; }

    /// <summary>
    /// The Attack Value (caster's roll).
    /// </summary>
    public int AV { get; init; }

    /// <summary>
    /// The caster's character ID.
    /// </summary>
    public int CasterId { get; init; }

    /// <summary>
    /// The caster's relevant skill level for this spell.
    /// </summary>
    public int CasterSkillLevel { get; init; }

    /// <summary>
    /// Single target character ID (for targeted spells).
    /// </summary>
    public int? TargetCharacterId { get; init; }

    /// <summary>
    /// Multiple target character IDs (for area effect spells).
    /// </summary>
    public List<int>? TargetCharacterIds { get; init; }

    /// <summary>
    /// Per-target Success Values for area effect spells.
    /// Key = character ID, Value = SV for that target.
    /// </summary>
    public Dictionary<int, int>? TargetSVs { get; init; }

    /// <summary>
    /// Target location name (for environmental spells).
    /// </summary>
    public string? TargetLocation { get; init; }

    /// <summary>
    /// Target item ID (for item-affecting spells).
    /// </summary>
    public Guid? TargetItemId { get; init; }

    /// <summary>
    /// Campaign ID for location tracking.
    /// </summary>
    public int? CampaignId { get; init; }

    /// <summary>
    /// Extra FAT pumped into the spell for increased power/duration.
    /// </summary>
    public int PumpedFatigue { get; init; }

    /// <summary>
    /// Extra mana pumped into the spell for increased power/duration.
    /// Dictionary of MagicSchool to amount.
    /// </summary>
    public Dictionary<MagicSchool, int>? PumpedMana { get; init; }

    /// <summary>
    /// Gets the total pump value (FAT + all mana combined).
    /// </summary>
    public int TotalPumpValue => PumpedFatigue + (PumpedMana?.Values.Sum() ?? 0);
}

/// <summary>
/// Result from applying a spell effect, containing all information
/// needed to update game state and communicate to the UI.
/// </summary>
public class SpellEffectResult
{
    /// <summary>
    /// Whether the spell effect was successfully applied.
    /// </summary>
    public bool Success { get; init; } = true;

    /// <summary>
    /// Human-readable description of what happened.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Detailed narrative text for the UI.
    /// </summary>
    public string? NarrativeText { get; init; }

    /// <summary>
    /// Error message if the spell effect failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Effects that were applied to characters.
    /// </summary>
    public List<AppliedEffect> AppliedEffects { get; init; } = [];

    /// <summary>
    /// Damage dealt to targets.
    /// </summary>
    public List<SpellDamageDealt> DamageDealt { get; init; } = [];

    /// <summary>
    /// Healing applied to targets.
    /// </summary>
    public List<SpellHealingApplied> HealingApplied { get; init; } = [];

    /// <summary>
    /// Conditions removed from targets.
    /// </summary>
    public List<ConditionRemoved> ConditionsRemoved { get; init; } = [];

    /// <summary>
    /// Location effect created (for environmental spells).
    /// </summary>
    public LocationEffect? CreatedLocationEffect { get; init; }

    /// <summary>
    /// Location created or used.
    /// </summary>
    public SpellLocation? AffectedLocation { get; init; }

    /// <summary>
    /// Per-target results for area effect spells.
    /// </summary>
    public List<TargetEffectResult> TargetResults { get; init; } = [];

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    public static SpellEffectResult Failure(string error) => new()
    {
        Success = false,
        ErrorMessage = error,
        Description = $"Spell failed: {error}"
    };
}

/// <summary>
/// An effect that was applied to a character.
/// </summary>
public class AppliedEffect
{
    public int CharacterId { get; init; }
    public int EffectDefinitionId { get; init; }
    public string EffectName { get; init; } = string.Empty;
    public int DurationRounds { get; init; }
    public int Stacks { get; init; } = 1;
}

/// <summary>
/// Damage dealt by a spell to a target.
/// </summary>
public class SpellDamageDealt
{
    public int CharacterId { get; init; }
    public int FatigueDamage { get; init; }
    public int VitalityDamage { get; init; }
    public bool CausedWound { get; init; }
    public string DamageType { get; init; } = "Magic";
    public string Description { get; init; } = string.Empty;
}

/// <summary>
/// Healing applied by a spell to a target.
/// </summary>
public class SpellHealingApplied
{
    public int CharacterId { get; init; }
    public int FatigueHealed { get; init; }
    public int VitalityHealed { get; init; }
    public string Description { get; init; } = string.Empty;
}

/// <summary>
/// A condition that was removed from a character.
/// </summary>
public class ConditionRemoved
{
    public int CharacterId { get; init; }
    public int EffectId { get; init; }
    public string EffectName { get; init; } = string.Empty;
}

/// <summary>
/// Result for a single target in an area effect spell.
/// </summary>
public class TargetEffectResult
{
    public int CharacterId { get; init; }
    public bool Success { get; init; }
    public int SV { get; init; }
    public string Description { get; init; } = string.Empty;
    public SpellDamageDealt? Damage { get; init; }
    public SpellHealingApplied? Healing { get; init; }
    public AppliedEffect? AppliedEffect { get; init; }
}
