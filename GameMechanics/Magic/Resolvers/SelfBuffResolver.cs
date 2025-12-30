using System.Threading.Tasks;
using GameMechanics.Effects;
using Threa.Dal.Dto;

namespace GameMechanics.Magic.Resolvers;

/// <summary>
/// Resolver for self-buff spells.
/// Self-buff spells always succeed (no resistance) and apply effects to the caster.
/// </summary>
public class SelfBuffResolver : ISpellResolver
{
    private readonly EffectManager _effectManager;

    public SpellType SpellType => SpellType.SelfBuff;

    public SelfBuffResolver(EffectManager effectManager)
    {
        _effectManager = effectManager;
    }

    public SpellTypeValidation ValidateRequest(SpellCastRequest request, SpellDefinition spell)
    {
        // Self-buff spells don't require any target - they always target the caster
        return SpellTypeValidation.Valid();
    }

    public async Task<SpellResolutionResult> ResolveAsync(SpellResolutionContext context)
    {
        var request = context.Request;
        var spell = context.Spell;

        // Self-buffs always succeed - no resistance check needed
        // The SV determines effect strength for scaling buffs
        int sv = context.CasterAV; // No TV subtraction for self-buffs

        var targetResult = new SpellTargetResult
        {
            TargetCharacterId = request.CasterId,
            Success = true,
            AV = context.CasterAV,
            TV = 0, // No resistance
            SV = sv,
            ResultDescription = GetBuffDescription(spell, sv)
        };

        // Apply the effect if the spell has one defined
        if (spell.EffectDefinitionId.HasValue)
        {
            var effect = await _effectManager.ApplyEffectAsync(
                request.CasterId,
                spell.EffectDefinitionId.Value.ToString(),
                spell.DefaultDuration);
            
            targetResult.AppliedEffect = effect;
        }

        return new SpellResolutionResult
        {
            Success = true,
            TargetResults = { targetResult },
            ResultDescription = $"{spell.EffectDescription ?? spell.SkillId} successfully cast on self."
        };
    }

    private static string GetBuffDescription(SpellDefinition spell, int sv)
    {
        // SV can influence buff description for narrative purposes
        var strength = sv switch
        {
            >= 6 => "exceptionally powerful",
            >= 4 => "strong",
            >= 2 => "solid",
            >= 0 => "adequate",
            _ => "weak but functional"
        };

        return $"A {strength} {spell.SkillId} effect takes hold.";
    }
}
