using System;
using System.Threading.Tasks;
using GameMechanics.Actions;
using GameMechanics.Effects;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Magic;

/// <summary>
/// Service for casting spells. Handles mana validation, skill checks,
/// and effect application.
/// </summary>
public class SpellCastingService
{
    private readonly ManaManager _manaManager;
    private readonly ISpellDefinitionDal _spellDal;
    private readonly EffectManager _effectManager;
    private readonly IDiceRoller _diceRoller;

    public SpellCastingService(
        ManaManager manaManager,
        ISpellDefinitionDal spellDal,
        EffectManager effectManager,
        IDiceRoller diceRoller)
    {
        _manaManager = manaManager;
        _spellDal = spellDal;
        _effectManager = effectManager;
        _diceRoller = diceRoller;
    }

    /// <summary>
    /// Validates whether a spell can be cast.
    /// </summary>
    /// <param name="request">The spell cast request.</param>
    /// <returns>Validation result with any errors.</returns>
    public async Task<SpellCastValidation> ValidateSpellCastAsync(SpellCastRequest request)
    {
        var result = new SpellCastValidation { IsValid = true };

        // Get spell definition
        var spell = await _spellDal.GetSpellBySkillIdAsync(request.SpellSkillId);
        if (spell == null)
        {
            result.IsValid = false;
            result.Errors.Add($"Unknown spell: {request.SpellSkillId}");
            return result;
        }

        // Check mana
        int totalManaCost = spell.ManaCost + request.BoostMana;
        bool hasMana = await _manaManager.HasSufficientManaAsync(
            request.CasterId, spell.MagicSchool, totalManaCost);
        
        if (!hasMana)
        {
            result.IsValid = false;
            result.Errors.Add($"Insufficient {spell.MagicSchool} mana. Need {totalManaCost}.");
        }

        // Check target requirements
        if (spell.SpellType == SpellType.Targeted && !request.TargetCharacterId.HasValue && !request.TargetItemId.HasValue)
        {
            result.IsValid = false;
            result.Errors.Add("Targeted spell requires a target.");
        }

        if (spell.SpellType == SpellType.Environmental && string.IsNullOrEmpty(request.TargetLocation))
        {
            result.IsValid = false;
            result.Errors.Add("Environmental spell requires a target location.");
        }

        result.SpellDefinition = spell;
        result.TotalManaCost = totalManaCost;
        return result;
    }

    /// <summary>
    /// Casts a spell.
    /// </summary>
    /// <param name="request">The spell cast request.</param>
    /// <returns>Result of the spell cast attempt.</returns>
    public async Task<SpellCastResult> CastSpellAsync(SpellCastRequest request)
    {
        // Validate first
        var validation = await ValidateSpellCastAsync(request);
        if (!validation.IsValid)
        {
            return new SpellCastResult
            {
                Success = false,
                FailureReason = string.Join("; ", validation.Errors)
            };
        }

        var spell = validation.SpellDefinition!;
        int totalManaCost = validation.TotalManaCost;

        // Spend mana
        var spendResult = await _manaManager.SpendManaAsync(
            request.CasterId, spell.MagicSchool, totalManaCost);
        
        if (!spendResult.Success)
        {
            return new SpellCastResult
            {
                Success = false,
                FailureReason = spendResult.ErrorMessage
            };
        }

        // Calculate spell skill check
        int skillBonus = SkillCost.GetBonus(request.SpellSkillLevel);
        int abilityScore = skillBonus + request.AttributeBonus + request.BoostBonus;
        int roll = _diceRoller.Roll4dFPlus();
        int av = abilityScore + roll;

        // Determine TV based on resistance type
        int tv = await CalculateTargetValueAsync(spell, request);
        int sv = av - tv;

        var result = new SpellCastResult
        {
            AV = av,
            TV = tv,
            SV = sv,
            Roll = roll,
            ManaSpent = totalManaCost,
            SpellDefinition = spell
        };

        // Check if spell succeeds
        if (sv >= 0)
        {
            result.Success = true;
            result.SuccessLevel = ResultTables.GetResult(sv, ResultTableType.SpellEffect);

            // Apply effect if spell has one
            if (spell.EffectDefinitionId.HasValue)
            {
                await ApplySpellEffectAsync(spell, request, result);
            }
        }
        else
        {
            result.Success = false;
            result.FailureReason = GetFailureDescription(sv);
        }

        return result;
    }

    /// <summary>
    /// Calculates the Target Value for a spell based on its resistance type.
    /// </summary>
    private async Task<int> CalculateTargetValueAsync(SpellDefinition spell, SpellCastRequest request)
    {
        return spell.ResistanceType switch
        {
            SpellResistanceType.None => 0, // Self-buffs auto-succeed
            SpellResistanceType.Fixed => spell.FixedResistanceTV ?? 8,
            SpellResistanceType.Willpower => await GetTargetWillpowerAsync(request),
            SpellResistanceType.Opposed => await GetOpposedSkillValueAsync(spell, request),
            _ => 8 // Default TV
        };
    }

    /// <summary>
    /// Gets the target's Willpower-based resistance value.
    /// </summary>
    private Task<int> GetTargetWillpowerAsync(SpellCastRequest request)
    {
        // For now, use provided target defense value
        // Full implementation would look up target's WIL attribute
        return Task.FromResult(request.TargetDefenseValue ?? 8);
    }

    /// <summary>
    /// Gets the target's opposed skill value.
    /// </summary>
    private Task<int> GetOpposedSkillValueAsync(SpellDefinition spell, SpellCastRequest request)
    {
        // For now, use provided target defense value
        // Full implementation would roll target's resistance skill
        return Task.FromResult(request.TargetDefenseValue ?? 8);
    }

    /// <summary>
    /// Applies the spell's effect to the target(s).
    /// </summary>
    private async Task ApplySpellEffectAsync(SpellDefinition spell, SpellCastRequest request, SpellCastResult result)
    {
        int? targetId = null;

        switch (spell.SpellType)
        {
            case SpellType.SelfBuff:
                targetId = request.CasterId;
                break;
            case SpellType.Targeted:
                targetId = request.TargetCharacterId;
                break;
            case SpellType.AreaEffect:
                // Area effects need special handling - apply to multiple targets
                // For Phase 1, just apply to the primary target
                targetId = request.TargetCharacterId;
                break;
            case SpellType.Environmental:
                // Environmental effects apply to locations, not characters
                // Defer full implementation to Phase 2
                break;
        }

        if (targetId.HasValue && spell.EffectDefinitionId.HasValue)
        {
            var effect = await _effectManager.ApplyEffectAsync(
                targetId.Value,
                spell.EffectDefinitionId.Value.ToString(),
                spell.DefaultDuration,
                sourceEntityId: Guid.NewGuid() // Would be caster's entity ID
            );
            result.AppliedEffects.Add(effect);
        }
    }

    /// <summary>
    /// Gets a description of why a spell failed based on SV.
    /// </summary>
    private static string GetFailureDescription(int sv)
    {
        return sv switch
        {
            >= -2 => "Spell fizzles - target resists.",
            >= -4 => "Spell fails to take hold.",
            >= -6 => "Spell backfires slightly - minor fatigue.",
            >= -8 => "Significant spell failure - lose additional mana.",
            _ => "Critical spell failure - possible backlash."
        };
    }
}

/// <summary>
/// Request to cast a spell.
/// </summary>
public class SpellCastRequest
{
    /// <summary>
    /// The caster's character ID.
    /// </summary>
    public int CasterId { get; set; }

    /// <summary>
    /// The skill ID of the spell being cast.
    /// </summary>
    public string SpellSkillId { get; set; } = string.Empty;

    /// <summary>
    /// The caster's level in the spell skill.
    /// </summary>
    public int SpellSkillLevel { get; set; }

    /// <summary>
    /// The caster's relevant attribute bonus (INT, WIL, etc.)
    /// </summary>
    public int AttributeBonus { get; set; }

    /// <summary>
    /// Target character ID for targeted spells.
    /// </summary>
    public int? TargetCharacterId { get; set; }

    /// <summary>
    /// Target item ID for item-targeted spells.
    /// </summary>
    public Guid? TargetItemId { get; set; }

    /// <summary>
    /// Target location for environmental spells.
    /// </summary>
    public string? TargetLocation { get; set; }

    /// <summary>
    /// Target's defense value (for opposed/willpower resistance).
    /// </summary>
    public int? TargetDefenseValue { get; set; }

    /// <summary>
    /// Extra mana spent for boosted effects.
    /// </summary>
    public int BoostMana { get; set; }

    /// <summary>
    /// Bonus from AP/FAT boosts.
    /// </summary>
    public int BoostBonus { get; set; }
}

/// <summary>
/// Result of a spell cast attempt.
/// </summary>
public class SpellCastResult
{
    /// <summary>
    /// Whether the spell succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The Attack Value (skill + roll).
    /// </summary>
    public int AV { get; set; }

    /// <summary>
    /// The Target Value (resistance).
    /// </summary>
    public int TV { get; set; }

    /// <summary>
    /// The Success Value (AV - TV).
    /// </summary>
    public int SV { get; set; }

    /// <summary>
    /// The dice roll result.
    /// </summary>
    public int Roll { get; set; }

    /// <summary>
    /// Total mana spent on this cast.
    /// </summary>
    public int ManaSpent { get; set; }

    /// <summary>
    /// Effects applied by this spell.
    /// </summary>
    public System.Collections.Generic.List<CharacterEffect> AppliedEffects { get; set; } = new();

    /// <summary>
    /// Reason for failure if the spell failed.
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// The spell definition that was cast.
    /// </summary>
    public SpellDefinition? SpellDefinition { get; set; }

    /// <summary>
    /// Interpretation of the success level.
    /// </summary>
    public ResultInterpretation? SuccessLevel { get; set; }
}

/// <summary>
/// Validation result for a spell cast request.
/// </summary>
public class SpellCastValidation
{
    public bool IsValid { get; set; }
    public System.Collections.Generic.List<string> Errors { get; set; } = new();
    public SpellDefinition? SpellDefinition { get; set; }
    public int TotalManaCost { get; set; }
}
