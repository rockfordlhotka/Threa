using System;
using System.Threading.Tasks;
using GameMechanics.Actions;
using GameMechanics.Effects;
using GameMechanics.Magic.Resolvers;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Magic;

/// <summary>
/// Service for casting spells. Handles mana validation, skill checks,
/// and delegates to spell type resolvers for effect application.
/// </summary>
public class SpellCastingService
{
    private readonly ManaManager _manaManager;
    private readonly ISpellDefinitionDal _spellDal;
    private readonly EffectManager _effectManager;
    private readonly IDiceRoller _diceRoller;
    private readonly SpellResolverFactory _resolverFactory;

    public SpellCastingService(
        ManaManager manaManager,
        ISpellDefinitionDal spellDal,
        EffectManager effectManager,
        IDiceRoller diceRoller,
        ILocationEffectDal? locationEffectDal = null)
    {
        _manaManager = manaManager;
        _spellDal = spellDal;
        _effectManager = effectManager;
        _diceRoller = diceRoller;
        _resolverFactory = new SpellResolverFactory(effectManager, locationEffectDal);
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

        // Validate spell type requirements using the resolver
        var resolver = _resolverFactory.GetResolver(spell.SpellType);
        var typeValidation = resolver.ValidateRequest(request, spell);
        if (!typeValidation.IsValid)
        {
            result.IsValid = false;
            result.Errors.AddRange(typeValidation.Errors);
        }

        result.SpellDefinition = spell;
        result.TotalManaCost = totalManaCost;
        return result;
    }

    /// <summary>
    /// Casts a spell using the appropriate resolver for the spell type.
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

        // Calculate spell skill check (caster rolls once)
        int skillBonus = SkillCost.GetBonus(request.SpellSkillLevel);
        int abilityScore = skillBonus + request.AttributeBonus + request.BoostBonus;
        int roll = _diceRoller.Roll4dFPlus();
        int casterAV = abilityScore + roll;

        // Create resolution context
        var context = new SpellResolutionContext
        {
            Request = request,
            Spell = spell,
            CasterAV = casterAV,
            Roll = roll,
            ManaCost = totalManaCost
        };

        // Get the appropriate resolver and resolve the spell
        var resolver = _resolverFactory.GetResolver(spell.SpellType);
        var resolution = await resolver.ResolveAsync(context);

        // Build final result
        var result = new SpellCastResult
        {
            Success = resolution.Success,
            AV = casterAV,
            Roll = roll,
            ManaSpent = totalManaCost,
            SpellDefinition = spell,
            TargetResults = resolution.TargetResults,
            AffectedLocation = resolution.AffectedLocation,
            ResultDescription = resolution.ResultDescription,
            FailureReason = resolution.ErrorMessage
        };

        // Populate legacy AppliedEffects from TargetResults
        foreach (var targetResult in resolution.TargetResults)
        {
            if (targetResult.AppliedEffect != null)
            {
                result.AppliedEffects.Add(targetResult.AppliedEffect);
            }

            // For single-target spells, populate TV/SV from the target result
            if (spell.SpellType == SpellType.Targeted || spell.SpellType == SpellType.SelfBuff)
            {
                result.TV = targetResult.TV;
                result.SV = targetResult.SV;
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the resolver factory for custom resolver registration.
    /// </summary>
    public SpellResolverFactory ResolverFactory => _resolverFactory;
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
    /// For area effect spells: list of target character IDs.
    /// </summary>
    public System.Collections.Generic.List<int>? TargetCharacterIds { get; set; }

    /// <summary>
    /// For area effect spells: defense values per target.
    /// Key = character ID, Value = defense value.
    /// </summary>
    public System.Collections.Generic.Dictionary<int, int>? TargetDefenseValues { get; set; }

    /// <summary>
    /// Campaign ID for environmental spells (to track location effects).
    /// </summary>
    public int? CampaignId { get; set; }

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
    /// The Target Value (resistance). For multi-target spells, this is 0.
    /// </summary>
    public int TV { get; set; }

    /// <summary>
    /// The Success Value (AV - TV). For multi-target spells, check TargetResults.
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
    /// Effects applied by this spell (legacy, use TargetResults for new code).
    /// </summary>
    public System.Collections.Generic.List<CharacterEffect> AppliedEffects { get; set; } = new();

    /// <summary>
    /// Individual results for each target (for area effect and targeted spells).
    /// </summary>
    public System.Collections.Generic.List<Resolvers.SpellTargetResult> TargetResults { get; set; } = new();

    /// <summary>
    /// For environmental spells, the location where the effect was placed.
    /// </summary>
    public SpellLocation? AffectedLocation { get; set; }

    /// <summary>
    /// Reason for failure if the spell failed.
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Description of the overall result.
    /// </summary>
    public string? ResultDescription { get; set; }

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
