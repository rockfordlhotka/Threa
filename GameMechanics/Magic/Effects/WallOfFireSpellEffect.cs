using System;
using System.Collections.Generic;
using System.Linq;
using Threa.Dal.Dto;

namespace GameMechanics.Magic.Effects;

/// <summary>
/// Spell effect for Wall of Fire and similar environmental damage spells.
/// Creates a persistent damaging effect at a location.
/// Affects any characters in the area each round.
/// Pump: Each pump point adds +1 to damage SV and 2 rounds to duration.
/// </summary>
public class WallOfFireSpellEffect : ISpellEffect
{
    /// <summary>
    /// Base duration in rounds.
    /// </summary>
    public const int BaseDurationRounds = 10;

    /// <inheritdoc/>
    public IEnumerable<string> HandledSpellIds => ["wall-of-fire", "fire-storm", "inferno"];

    /// <inheritdoc/>
    public SpellEffectResult Resolve(SpellEffectContext context)
    {
        if (string.IsNullOrEmpty(context.TargetLocation))
        {
            return SpellEffectResult.Failure("Wall of Fire requires a target location.");
        }

        // Calculate duration: base + SV bonus + pump bonus
        // Each SV above 0 adds 2 rounds
        // Each pump point adds 2 rounds
        var svBonus = Math.Max(0, context.SV) * 2;
        var pumpDurationBonus = context.TotalPumpValue * 2;
        var totalDuration = BaseDurationRounds + svBonus + pumpDurationBonus;

        // Use spell's default duration if specified
        if (context.Spell.DefaultDuration.HasValue)
        {
            totalDuration = context.Spell.DefaultDuration.Value + svBonus + pumpDurationBonus;
        }

        // Calculate the damage SV that will be used each round
        // Base damage is caster's SV, pump adds to it
        var effectDamageSV = context.SV + context.TotalPumpValue;

        // Create the location
        var location = new SpellLocation
        {
            Id = Guid.NewGuid(),
            Name = context.TargetLocation,
            Description = $"Engulfed by {GetSpellDisplayName(context.Spell.SkillId)}",
            CampaignId = context.CampaignId ?? 0,
            CreatedAt = DateTime.UtcNow
        };

        // Create the location effect
        var locationEffect = new LocationEffect
        {
            Id = Guid.NewGuid(),
            LocationId = location.Id,
            SpellSkillId = context.Spell.SkillId,
            CasterId = context.CasterId,
            RoundsRemaining = totalDuration,
            CastSV = effectDamageSV, // Store the effective damage SV
            IsActive = true
        };

        // If there are characters already at the location, damage them immediately
        var damageDealt = new List<SpellDamageDealt>();
        var targetResults = new List<TargetEffectResult>();

        if (context.TargetCharacterIds?.Count > 0)
        {
            foreach (var characterId in context.TargetCharacterIds)
            {
                // Get the per-target SV if available, otherwise use the spell SV
                var targetSV = context.TargetSVs?.TryGetValue(characterId, out var sv) == true
                    ? sv
                    : context.SV;

                var effectiveTargetSV = targetSV + context.TotalPumpValue;
                var damage = EnergyDamageSpellEffect.GetEnergyDamage(effectiveTargetSV);

                var spellDamage = new SpellDamageDealt
                {
                    CharacterId = characterId,
                    FatigueDamage = damage.FatigueDamage,
                    VitalityDamage = damage.VitalityDamage,
                    CausedWound = damage.CausesWound,
                    DamageType = "Fire",
                    Description = $"Fire damage SV {effectiveTargetSV}: {damage.FatigueDamage} FAT, {damage.VitalityDamage} VIT"
                };

                damageDealt.Add(spellDamage);

                targetResults.Add(new TargetEffectResult
                {
                    CharacterId = characterId,
                    Success = effectiveTargetSV >= 0,
                    SV = effectiveTargetSV,
                    Description = spellDamage.Description,
                    Damage = spellDamage
                });
            }
        }

        var description = BuildDescription(context, totalDuration, effectDamageSV, damageDealt.Count);
        var narrative = BuildNarrative(context, totalDuration, effectDamageSV, damageDealt);

        return new SpellEffectResult
        {
            Success = true,
            Description = description,
            NarrativeText = narrative,
            AffectedLocation = location,
            CreatedLocationEffect = locationEffect,
            DamageDealt = damageDealt,
            TargetResults = targetResults
        };
    }

    /// <summary>
    /// Calculates damage for a character entering or remaining in the wall of fire.
    /// Called each round for characters in the affected area.
    /// </summary>
    /// <param name="castSV">The stored CastSV from the LocationEffect.</param>
    /// <param name="characterDefenseBonus">Any defensive bonus the character has (armor, resistance, etc.).</param>
    /// <returns>The damage result.</returns>
    public static EnergyDamageResult CalculateRoundDamage(int castSV, int characterDefenseBonus = 0)
    {
        var effectiveSV = castSV - characterDefenseBonus;
        return EnergyDamageSpellEffect.GetEnergyDamage(effectiveSV);
    }

    private static string BuildDescription(SpellEffectContext context, int duration, int damageSV, int targetsHit)
    {
        var pumpText = context.TotalPumpValue > 0
            ? $" (pumped +{context.TotalPumpValue})"
            : "";

        var durationText = FormatDuration(duration);
        var targetsText = targetsHit > 0 ? $", {targetsHit} targets caught in flames" : "";

        return $"{context.Spell.SkillId} at {context.TargetLocation}{pumpText}: SV {damageSV} fire damage each round, {durationText}{targetsText}";
    }

    private static string BuildNarrative(SpellEffectContext context, int duration, int damageSV, List<SpellDamageDealt> damageDealt)
    {
        var spellName = GetSpellDisplayName(context.Spell.SkillId);
        var durationText = FormatDuration(duration);
        var intensity = GetIntensity(damageSV);

        var narrative = context.Spell.SkillId switch
        {
            "fire-storm" =>
                $"A raging storm of fire erupts at {context.TargetLocation}, " +
                $"creating a {intensity} inferno that will burn for {durationText}.",

            "inferno" =>
                $"An all-consuming inferno blazes to life at {context.TargetLocation}, " +
                $"its {intensity} flames lasting for {durationText}.",

            _ =>
                $"A wall of {intensity} flames springs up at {context.TargetLocation}, " +
                $"burning fiercely for {durationText}."
        };

        if (damageDealt.Count > 0)
        {
            var totalFat = damageDealt.Sum(d => d.FatigueDamage);
            var totalVit = damageDealt.Sum(d => d.VitalityDamage);
            var wounds = damageDealt.Count(d => d.CausedWound);

            narrative += $" {damageDealt.Count} creature(s) are caught in the flames, " +
                        $"suffering {totalFat} total fatigue and {totalVit} total vitality damage";

            if (wounds > 0)
            {
                narrative += $", with {wounds} suffering burns";
            }
            narrative += "!";
        }

        return narrative;
    }

    private static string GetIntensity(int sv) => sv switch
    {
        < 0 => "flickering",
        0 or 1 => "moderate",
        2 or 3 => "intense",
        4 or 5 => "fierce",
        6 or 7 => "roaring",
        _ => "devastating"
    };

    private static string FormatDuration(int rounds)
    {
        if (rounds >= 1200) // 1 hour
        {
            var hours = rounds / 1200;
            return hours == 1 ? "1 hour" : $"{hours} hours";
        }
        if (rounds >= 20) // 1 minute
        {
            var minutes = rounds / 20;
            return minutes == 1 ? "1 minute" : $"{minutes} minutes";
        }
        return rounds == 1 ? "1 round" : $"{rounds} rounds";
    }

    private static string GetSpellDisplayName(string spellId) => spellId switch
    {
        "wall-of-fire" => "Wall of Fire",
        "fire-storm" => "Fire Storm",
        "inferno" => "Inferno",
        _ => spellId
    };
}
