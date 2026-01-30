using System;
using System.Collections.Generic;
using GameMechanics.Time;
using Threa.Dal.Dto;

namespace GameMechanics.Magic.Effects;

/// <summary>
/// Spell effect for area light spells like Illuminate Area.
/// Creates a persistent light at a location that illuminates the area.
/// Pump: Each pump point adds 1 round to duration.
/// </summary>
public class AreaLightSpellEffect : ISpellEffect
{
    /// <summary>
    /// Default light radius in meters.
    /// </summary>
    public const int DefaultLightRadius = 10;

    /// <summary>
    /// Base duration in rounds.
    /// </summary>
    public const int BaseDurationRounds = 60; // 3 minutes

    /// <inheritdoc/>
    public IEnumerable<string> HandledSpellIds => ["illuminate-area", "daylight", "dancing-lights"];

    /// <inheritdoc/>
    public SpellEffectResult Resolve(SpellEffectContext context)
    {
        if (string.IsNullOrEmpty(context.TargetLocation))
        {
            return SpellEffectResult.Failure("Area light spell requires a target location.");
        }

        // Calculate duration: base + SV bonus + pump bonus
        // Each SV above 0 adds 10 rounds (30 seconds)
        // Each pump point adds 20 rounds (1 minute)
        var svBonus = Math.Max(0, context.SV) * 10;
        var pumpBonus = context.TotalPumpValue * 20;
        var totalDuration = BaseDurationRounds + svBonus + pumpBonus;

        // Use spell's default duration if specified
        if (context.Spell.DefaultDuration.HasValue)
        {
            totalDuration = context.Spell.DefaultDuration.Value + svBonus + pumpBonus;
        }

        // Calculate light intensity based on SV
        var lightIntensity = GetLightIntensity(context.SV);
        var lightRadius = GetLightRadius(context.Spell.SkillId, context.SV);

        // Create the location
        var location = new SpellLocation
        {
            Id = Guid.NewGuid(),
            Name = context.TargetLocation,
            Description = $"Illuminated by {GetSpellDisplayName(context.Spell.SkillId)}",
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
            CastSV = context.SV,
            IsActive = true
        };

        var description = BuildDescription(context, totalDuration, lightRadius, lightIntensity);
        var narrative = BuildNarrative(context, totalDuration, lightRadius, lightIntensity);

        return new SpellEffectResult
        {
            Success = true,
            Description = description,
            NarrativeText = narrative,
            AffectedLocation = location,
            CreatedLocationEffect = locationEffect
        };
    }

    private static string GetLightIntensity(int sv) => sv switch
    {
        < 0 => "dim",
        0 or 1 => "moderate",
        2 or 3 => "bright",
        4 or 5 => "brilliant",
        _ => "blinding"
    };

    private static int GetLightRadius(string spellId, int sv)
    {
        var baseRadius = spellId switch
        {
            "daylight" => 20,
            "dancing-lights" => 5,
            _ => DefaultLightRadius
        };

        // SV adds to radius: +1 meter per SV above 0
        return baseRadius + Math.Max(0, sv);
    }

    private static string BuildDescription(SpellEffectContext context, int duration, int radius, string intensity)
    {
        var pumpText = context.TotalPumpValue > 0
            ? $" (pumped +{context.TotalPumpValue})"
            : "";

        var durationText = FormatDuration(duration);
        return $"{context.Spell.SkillId} at {context.TargetLocation}{pumpText}: {intensity} light, {radius}m radius, {durationText}";
    }

    private static string BuildNarrative(SpellEffectContext context, int duration, int radius, string intensity)
    {
        var spellName = GetSpellDisplayName(context.Spell.SkillId);
        var durationText = FormatDuration(duration);

        return context.Spell.SkillId switch
        {
            "dancing-lights" =>
                $"Several orbs of {intensity} light spring into existence, " +
                $"dancing through the air at {context.TargetLocation}, illuminating a {radius} meter area for {durationText}.",

            "daylight" =>
                $"A sphere of {intensity} daylight erupts at {context.TargetLocation}, " +
                $"banishing all shadows within {radius} meters for {durationText}.",

            _ =>
                $"A {intensity} magical light appears at {context.TargetLocation}, " +
                $"illuminating a {radius} meter area for {durationText}."
        };
    }

    private static readonly IGameTimeFormatService TimeFormat = new DefaultGameTimeFormatService();

    private static string FormatDuration(int rounds) => TimeFormat.FormatRounds(rounds);

    private static string GetSpellDisplayName(string spellId) => spellId switch
    {
        "illuminate-area" => "Illuminate Area",
        "daylight" => "Daylight",
        "dancing-lights" => "Dancing Lights",
        _ => spellId
    };
}
