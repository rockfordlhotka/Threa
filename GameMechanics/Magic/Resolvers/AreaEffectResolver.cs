using System.Collections.Generic;
using System.Threading.Tasks;
using GameMechanics.Actions;
using GameMechanics.Effects;
using Threa.Dal.Dto;

namespace GameMechanics.Magic.Resolvers;

/// <summary>
/// Resolver for area effect spells (multiple targets).
/// The caster rolls once, but each target defends individually.
/// </summary>
public class AreaEffectResolver : ISpellResolver
{
    private readonly EffectManager _effectManager;

    public SpellType SpellType => SpellType.AreaEffect;

    public AreaEffectResolver(EffectManager effectManager)
    {
        _effectManager = effectManager;
    }

    public SpellTypeValidation ValidateRequest(SpellCastRequest request, SpellDefinition spell)
    {
        if (request.TargetCharacterIds == null || request.TargetCharacterIds.Count == 0)
        {
            return SpellTypeValidation.Invalid("Area effect spell requires at least one target.");
        }

        return SpellTypeValidation.Valid();
    }

    public async Task<SpellResolutionResult> ResolveAsync(SpellResolutionContext context)
    {
        var request = context.Request;
        var spell = context.Spell;

        var result = new SpellResolutionResult
        {
            Success = false,
            TargetResults = new List<SpellTargetResult>()
        };

        // Get target defense values (passed in request)
        var targetDefenses = request.TargetDefenseValues ?? new Dictionary<int, int>();

        int successCount = 0;
        int totalDamage = 0;

        // Each target defends individually against the same caster AV
        foreach (int targetId in request.TargetCharacterIds ?? new List<int>())
        {
            // Get this target's defense value, default to 8 if not specified
            int tv = targetDefenses.TryGetValue(targetId, out var defense) ? defense : 8;
            
            // Apply any resistance type modifiers
            tv = ApplyResistanceType(spell, tv, request);

            int sv = context.CasterAV - tv;

            var targetResult = new SpellTargetResult
            {
                TargetCharacterId = targetId,
                AV = context.CasterAV,
                TV = tv,
                SV = sv,
                Success = sv >= 0
            };

            if (targetResult.Success)
            {
                successCount++;
                await ApplySpellEffectAsync(spell, targetId, targetResult, sv);
                totalDamage += targetResult.DamageDealt ?? 0;
            }
            else
            {
                targetResult.ResultDescription = GetFailureDescription(sv);
            }

            result.TargetResults.Add(targetResult);
        }

        result.Success = successCount > 0;
        result.ResultDescription = GetOverallDescription(spell, successCount, result.TargetResults.Count, totalDamage);

        return result;
    }

    private static int ApplyResistanceType(SpellDefinition spell, int baseTV, SpellCastRequest request)
    {
        // For area effects, resistance type can modify how we interpret the TV
        return spell.ResistanceType switch
        {
            SpellResistanceType.None => 0, // Beneficial area spells (Mass Heal)
            SpellResistanceType.Fixed => spell.FixedResistanceTV ?? 8,
            _ => baseTV // Use individual target defenses
        };
    }

    private async Task ApplySpellEffectAsync(
        SpellDefinition spell,
        int targetId,
        SpellTargetResult result,
        int sv)
    {
        // Check if this is a damage spell
        if (IsDamageSpell(spell))
        {
            var damageResult = ResultTables.GetResult(sv, ResultTableType.CombatDamage);
            result.DamageDealt = damageResult.EffectValue;
            result.ResultDescription = $"Hit for {damageResult.EffectValue} damage.";
        }

        // Apply persistent effect if defined
        if (spell.EffectDefinitionId.HasValue)
        {
            var effect = await _effectManager.ApplyEffectAsync(
                targetId,
                spell.EffectDefinitionId.Value.ToString(),
                spell.DefaultDuration);

            result.AppliedEffect = effect;
        }

        if (result.ResultDescription == null)
        {
            result.ResultDescription = GetSuccessDescription(sv);
        }
    }

    private static bool IsDamageSpell(SpellDefinition spell)
    {
        var skillId = spell.SkillId.ToLowerInvariant();
        return skillId.Contains("ball") ||
               skillId.Contains("blast") ||
               skillId.Contains("storm") ||
               skillId.Contains("nova") ||
               skillId.Contains("burst");
    }

    private static string GetSuccessDescription(int sv)
    {
        return sv switch
        {
            >= 6 => "Critically affected!",
            >= 4 => "Strongly affected.",
            >= 2 => "Solidly affected.",
            _ => "Affected."
        };
    }

    private static string GetFailureDescription(int sv)
    {
        return sv switch
        {
            >= -2 => "Narrowly resists.",
            _ => "Completely resists."
        };
    }

    private static string GetOverallDescription(SpellDefinition spell, int successCount, int totalTargets, int totalDamage)
    {
        if (successCount == 0)
        {
            return $"{spell.SkillId} affects no targets - all resist!";
        }

        if (successCount == totalTargets)
        {
            var damageStr = totalDamage > 0 ? $" for {totalDamage} total damage" : "";
            return $"{spell.SkillId} affects all {totalTargets} targets{damageStr}!";
        }

        var dmgStr = totalDamage > 0 ? $" ({totalDamage} total damage)" : "";
        return $"{spell.SkillId} affects {successCount} of {totalTargets} targets{dmgStr}.";
    }
}
