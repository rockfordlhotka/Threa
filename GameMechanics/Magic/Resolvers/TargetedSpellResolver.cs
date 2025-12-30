using System.Threading.Tasks;
using GameMechanics.Actions;
using GameMechanics.Effects;
using Threa.Dal.Dto;

namespace GameMechanics.Magic.Resolvers;

/// <summary>
/// Resolver for targeted spells (single target).
/// Handles resistance checks and effect/damage application to a single target.
/// </summary>
public class TargetedSpellResolver : ISpellResolver
{
    private readonly EffectManager _effectManager;

    public SpellType SpellType => SpellType.Targeted;

    public TargetedSpellResolver(EffectManager effectManager)
    {
        _effectManager = effectManager;
    }

    public SpellTypeValidation ValidateRequest(SpellCastRequest request, SpellDefinition spell)
    {
        if (!request.TargetCharacterId.HasValue && !request.TargetItemId.HasValue)
        {
            return SpellTypeValidation.Invalid("Targeted spell requires a target character or item.");
        }

        return SpellTypeValidation.Valid();
    }

    public async Task<SpellResolutionResult> ResolveAsync(SpellResolutionContext context)
    {
        var request = context.Request;
        var spell = context.Spell;

        // Calculate TV based on resistance type
        int tv = CalculateTargetValue(spell, request);
        int sv = context.CasterAV - tv;

        var targetResult = new SpellTargetResult
        {
            TargetCharacterId = request.TargetCharacterId ?? 0,
            AV = context.CasterAV,
            TV = tv,
            SV = sv,
            Success = sv >= 0
        };

        if (targetResult.Success)
        {
            // Apply effect or damage based on spell
            await ApplySpellEffectAsync(spell, request, targetResult, sv);
            targetResult.ResultDescription = GetSuccessDescription(spell, sv);
        }
        else
        {
            targetResult.ResultDescription = GetFailureDescription(spell, sv);
        }

        return new SpellResolutionResult
        {
            Success = targetResult.Success,
            TargetResults = { targetResult },
            ResultDescription = targetResult.ResultDescription
        };
    }

    private static int CalculateTargetValue(SpellDefinition spell, SpellCastRequest request)
    {
        return spell.ResistanceType switch
        {
            SpellResistanceType.None => 0,
            SpellResistanceType.Fixed => spell.FixedResistanceTV ?? 8,
            SpellResistanceType.Willpower => request.TargetDefenseValue ?? 8,
            SpellResistanceType.Opposed => request.TargetDefenseValue ?? 8,
            _ => 8
        };
    }

    private async Task ApplySpellEffectAsync(
        SpellDefinition spell,
        SpellCastRequest request,
        SpellTargetResult result,
        int sv)
    {
        // Check if this is a damage spell (use RVS damage table)
        if (IsDamageSpell(spell))
        {
            var damageResult = ResultTables.GetResult(sv, ResultTableType.CombatDamage);
            result.DamageDealt = damageResult.EffectValue;
            result.ResultDescription = $"Spell hits for {damageResult.Label} ({damageResult.EffectValue} damage).";
        }

        // Apply persistent effect if defined
        if (spell.EffectDefinitionId.HasValue && request.TargetCharacterId.HasValue)
        {
            var effect = await _effectManager.ApplyEffectAsync(
                request.TargetCharacterId.Value,
                spell.EffectDefinitionId.Value.ToString(),
                spell.DefaultDuration);

            result.AppliedEffect = effect;
        }
    }

    private static bool IsDamageSpell(SpellDefinition spell)
    {
        // Simple heuristic - spells with "bolt", "shard", "strike" are damage spells
        // A more robust approach would add a SpellCategory or IsDamageSpell flag to SpellDefinition
        var skillId = spell.SkillId.ToLowerInvariant();
        return skillId.Contains("bolt") ||
               skillId.Contains("shard") ||
               skillId.Contains("strike") ||
               skillId.Contains("blast");
    }

    private static string GetSuccessDescription(SpellDefinition spell, int sv)
    {
        var strength = sv switch
        {
            >= 6 => "devastating",
            >= 4 => "powerful",
            >= 2 => "solid",
            _ => "glancing"
        };

        return $"A {strength} {spell.SkillId} strikes the target.";
    }

    private static string GetFailureDescription(SpellDefinition spell, int sv)
    {
        return sv switch
        {
            >= -2 => "The target narrowly resists the spell.",
            >= -4 => "The target shrugs off the magical attack.",
            _ => "The spell completely fails to affect the target."
        };
    }
}
