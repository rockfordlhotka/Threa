using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace GameMechanics.Magic.Resolvers;

/// <summary>
/// Interface for spell-type-specific resolution logic.
/// Each spell type (Targeted, SelfBuff, AreaEffect, Environmental)
/// has its own resolver that handles targeting, resistance, and effect application.
/// </summary>
public interface ISpellResolver
{
    /// <summary>
    /// Gets the spell type this resolver handles.
    /// </summary>
    SpellType SpellType { get; }

    /// <summary>
    /// Resolves a spell cast, handling targeting, resistance checks, and effect application.
    /// </summary>
    /// <param name="context">The spell resolution context with all needed data.</param>
    /// <returns>The result of the spell resolution.</returns>
    Task<SpellResolutionResult> ResolveAsync(SpellResolutionContext context);

    /// <summary>
    /// Validates that the request has the required data for this spell type.
    /// </summary>
    /// <param name="request">The spell cast request.</param>
    /// <param name="spell">The spell definition.</param>
    /// <returns>Validation result.</returns>
    SpellTypeValidation ValidateRequest(SpellCastRequest request, SpellDefinition spell);
}

/// <summary>
/// Context passed to spell resolvers containing all data needed for resolution.
/// </summary>
public class SpellResolutionContext
{
    /// <summary>
    /// The original spell cast request.
    /// </summary>
    public required SpellCastRequest Request { get; init; }

    /// <summary>
    /// The spell being cast.
    /// </summary>
    public required SpellDefinition Spell { get; init; }

    /// <summary>
    /// The caster's Attack Value (skill bonus + attribute + roll).
    /// </summary>
    public int CasterAV { get; init; }

    /// <summary>
    /// The dice roll result.
    /// </summary>
    public int Roll { get; init; }

    /// <summary>
    /// Total mana being spent on this cast.
    /// </summary>
    public int ManaCost { get; init; }
}

/// <summary>
/// Result of spell resolution from a resolver.
/// </summary>
public class SpellResolutionResult
{
    /// <summary>
    /// Whether the spell had any successful effect.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Individual results for each target affected.
    /// </summary>
    public System.Collections.Generic.List<SpellTargetResult> TargetResults { get; set; } = new();

    /// <summary>
    /// For environmental spells, the location where the effect was placed.
    /// </summary>
    public SpellLocation? AffectedLocation { get; set; }

    /// <summary>
    /// Description of the overall result for narrative purposes.
    /// </summary>
    public string? ResultDescription { get; set; }

    /// <summary>
    /// Any error that occurred during resolution.
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result for an individual target of a spell.
/// </summary>
public class SpellTargetResult
{
    /// <summary>
    /// The target character ID.
    /// </summary>
    public int TargetCharacterId { get; set; }

    /// <summary>
    /// Whether the spell affected this target.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The caster's AV for this target.
    /// </summary>
    public int AV { get; set; }

    /// <summary>
    /// The target's resistance/defense value.
    /// </summary>
    public int TV { get; set; }

    /// <summary>
    /// Success Value (AV - TV).
    /// </summary>
    public int SV { get; set; }

    /// <summary>
    /// Effect applied to this target, if any.
    /// </summary>
    public CharacterEffect? AppliedEffect { get; set; }

    /// <summary>
    /// Damage dealt to this target, if any.
    /// </summary>
    public int? DamageDealt { get; set; }

    /// <summary>
    /// Description of the result for this target.
    /// </summary>
    public string? ResultDescription { get; set; }
}

/// <summary>
/// Validation result for spell type requirements.
/// </summary>
public class SpellTypeValidation
{
    public bool IsValid { get; set; } = true;
    public System.Collections.Generic.List<string> Errors { get; set; } = new();

    public static SpellTypeValidation Valid() => new() { IsValid = true };

    public static SpellTypeValidation Invalid(string error) => new()
    {
        IsValid = false,
        Errors = new() { error }
    };
}
