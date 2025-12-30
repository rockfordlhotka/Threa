using System;
using System.Threading.Tasks;
using GameMechanics.Effects;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Magic.Resolvers;

/// <summary>
/// Resolver for environmental spells (location-based persistent effects).
/// These spells create effects attached to narrative locations rather than characters.
/// </summary>
public class EnvironmentalSpellResolver : ISpellResolver
{
    private readonly EffectManager _effectManager;
    private readonly ILocationEffectDal? _locationEffectDal;

    public SpellType SpellType => SpellType.Environmental;

    public EnvironmentalSpellResolver(EffectManager effectManager, ILocationEffectDal? locationEffectDal = null)
    {
        _effectManager = effectManager;
        _locationEffectDal = locationEffectDal;
    }

    public SpellTypeValidation ValidateRequest(SpellCastRequest request, SpellDefinition spell)
    {
        if (string.IsNullOrWhiteSpace(request.TargetLocation))
        {
            return SpellTypeValidation.Invalid("Environmental spell requires a target location name.");
        }

        return SpellTypeValidation.Valid();
    }

    public async Task<SpellResolutionResult> ResolveAsync(SpellResolutionContext context)
    {
        var request = context.Request;
        var spell = context.Spell;

        // Environmental spells always succeed at creating the effect
        // The SV determines the strength/duration/size of the effect
        int sv = context.CasterAV; // No resistance for creating environmental effects

        // Create or find the location
        var location = new SpellLocation
        {
            Id = Guid.NewGuid(),
            Name = request.TargetLocation!,
            Description = $"Location affected by {spell.SkillId}",
            CampaignId = request.CampaignId,
            CreatedAt = DateTime.UtcNow
        };

        // Create the location effect
        var locationEffect = new LocationEffect
        {
            Id = Guid.NewGuid(),
            LocationId = location.Id,
            SpellSkillId = spell.SkillId,
            CasterId = request.CasterId,
            EffectDefinitionId = spell.EffectDefinitionId,
            RoundsRemaining = CalculateDuration(spell, sv),
            CastSV = sv,
            Description = GetEffectDescription(spell, sv),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Persist if DAL is available
        if (_locationEffectDal != null)
        {
            await _locationEffectDal.CreateLocationAsync(location);
            await _locationEffectDal.CreateLocationEffectAsync(locationEffect);
        }

        var result = new SpellResolutionResult
        {
            Success = true,
            AffectedLocation = location,
            ResultDescription = GetCastDescription(spell, location.Name, sv, locationEffect.RoundsRemaining)
        };

        // If there are any characters already at the location, they might be affected
        if (request.TargetCharacterIds != null && request.TargetCharacterIds.Count > 0)
        {
            await ApplyImmediateEffectsAsync(spell, request, context.CasterAV, result);
        }

        return result;
    }

    private static int CalculateDuration(SpellDefinition spell, int sv)
    {
        // Base duration from spell definition, potentially extended by high SV
        int baseDuration = spell.DefaultDuration ?? 5;
        
        // Bonus rounds based on SV
        int bonusRounds = sv switch
        {
            >= 6 => 3,
            >= 4 => 2,
            >= 2 => 1,
            _ => 0
        };

        return baseDuration + bonusRounds;
    }

    private static string GetEffectDescription(SpellDefinition spell, int sv)
    {
        var intensity = sv switch
        {
            >= 6 => "raging",
            >= 4 => "powerful",
            >= 2 => "strong",
            >= 0 => "moderate",
            _ => "weak"
        };

        return $"A {intensity} {spell.SkillId} effect fills the area.";
    }

    private static string GetCastDescription(SpellDefinition spell, string locationName, int sv, int? duration)
    {
        var durationStr = duration.HasValue ? $" for {duration} rounds" : "";
        var intensity = sv >= 4 ? "powerful " : sv >= 2 ? "" : "modest ";
        
        return $"A {intensity}{spell.SkillId} manifests at {locationName}{durationStr}.";
    }

    private async Task ApplyImmediateEffectsAsync(
        SpellDefinition spell,
        SpellCastRequest request,
        int casterAV,
        SpellResolutionResult result)
    {
        var targetDefenses = request.TargetDefenseValues ?? new System.Collections.Generic.Dictionary<int, int>();

        foreach (int targetId in request.TargetCharacterIds!)
        {
            int tv = targetDefenses.TryGetValue(targetId, out var defense) ? defense : 8;
            
            // For environmental effects, resistance is typically fixed or willpower-based
            if (spell.ResistanceType == SpellResistanceType.Fixed)
            {
                tv = spell.FixedResistanceTV ?? 8;
            }

            int sv = casterAV - tv;

            var targetResult = new SpellTargetResult
            {
                TargetCharacterId = targetId,
                AV = casterAV,
                TV = tv,
                SV = sv,
                Success = sv >= 0
            };

            if (targetResult.Success && spell.EffectDefinitionId.HasValue)
            {
                var effect = await _effectManager.ApplyEffectAsync(
                    targetId,
                    spell.EffectDefinitionId.Value.ToString(),
                    spell.DefaultDuration);

                targetResult.AppliedEffect = effect;
                targetResult.ResultDescription = "Caught in the environmental effect.";
            }
            else if (!targetResult.Success)
            {
                targetResult.ResultDescription = "Resists the environmental effect.";
            }

            result.TargetResults.Add(targetResult);
        }
    }
}
