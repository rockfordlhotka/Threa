using System.Collections.Generic;

namespace GameMechanics.Magic.Effects;

/// <summary>
/// Spell effect for healing spells like Minor Heal and Restore Vitality.
/// Healing uses an inverse damage table - higher SV = more healing.
/// Pump: Each pump point adds +1 to SV before healing lookup.
/// </summary>
public class HealingSpellEffect : ISpellEffect
{
    /// <inheritdoc/>
    public IEnumerable<string> HandledSpellIds => ["minor-heal", "restore-vitality", "major-heal", "regenerate"];

    /// <inheritdoc/>
    public SpellEffectResult Resolve(SpellEffectContext context)
    {
        // Healing spells can target self or others
        int targetId;
        if (context.TargetCharacterId.HasValue)
        {
            targetId = context.TargetCharacterId.Value;
        }
        else if (context.Spell.Range == 0) // Self-targeting
        {
            targetId = context.CasterId;
        }
        else
        {
            return SpellEffectResult.Failure("Healing spell requires a target.");
        }

        // Apply pump bonus to SV
        var effectiveSV = context.SV + context.TotalPumpValue;

        // Determine healing type based on spell
        var healingType = GetHealingType(context.Spell.SkillId);
        var healing = GetHealing(effectiveSV, healingType);

        var healingApplied = new SpellHealingApplied
        {
            CharacterId = targetId,
            FatigueHealed = healing.FatigueHealed,
            VitalityHealed = healing.VitalityHealed,
            Description = BuildDescription(context, effectiveSV, healing)
        };

        var narrative = BuildNarrative(context, effectiveSV, healing, targetId == context.CasterId);

        return new SpellEffectResult
        {
            Success = true,
            Description = healingApplied.Description,
            NarrativeText = narrative,
            HealingApplied = [healingApplied]
        };
    }

    /// <summary>
    /// Healing lookup table.
    /// Base healing starts at SV 0 and scales up.
    /// </summary>
    public static HealingResult GetHealing(int sv, HealingType healingType)
    {
        // Even failed spells provide minimal healing (the magic still flows)
        if (sv < 0)
        {
            return healingType switch
            {
                HealingType.Fatigue => new HealingResult { FatigueHealed = 1, Description = "Minimal healing: 1 FAT" },
                HealingType.Vitality => new HealingResult { VitalityHealed = 1, Description = "Minimal healing: 1 VIT" },
                _ => new HealingResult { FatigueHealed = 1, Description = "Minimal healing: 1 FAT" }
            };
        }

        return healingType switch
        {
            HealingType.Fatigue => GetFatigueHealing(sv),
            HealingType.Vitality => GetVitalityHealing(sv),
            HealingType.Both => GetCombinedHealing(sv),
            _ => GetFatigueHealing(sv)
        };
    }

    private static HealingResult GetFatigueHealing(int sv) => sv switch
    {
        0 => new HealingResult { FatigueHealed = 2, Description = "Light healing: 2 FAT" },
        1 => new HealingResult { FatigueHealed = 3, Description = "Minor healing: 3 FAT" },
        2 => new HealingResult { FatigueHealed = 4, Description = "Moderate healing: 4 FAT" },
        3 => new HealingResult { FatigueHealed = 5, Description = "Good healing: 5 FAT" },
        4 => new HealingResult { FatigueHealed = 6, Description = "Strong healing: 6 FAT" },
        5 => new HealingResult { FatigueHealed = 7, Description = "Powerful healing: 7 FAT" },
        6 => new HealingResult { FatigueHealed = 8, Description = "Major healing: 8 FAT" },
        _ => new HealingResult { FatigueHealed = 8 + (sv - 6), Description = $"Exceptional healing: {8 + (sv - 6)} FAT" }
    };

    private static HealingResult GetVitalityHealing(int sv) => sv switch
    {
        0 => new HealingResult { VitalityHealed = 1, Description = "Minor restoration: 1 VIT" },
        1 => new HealingResult { VitalityHealed = 1, Description = "Light restoration: 1 VIT" },
        2 => new HealingResult { VitalityHealed = 2, Description = "Moderate restoration: 2 VIT" },
        3 => new HealingResult { VitalityHealed = 2, Description = "Good restoration: 2 VIT" },
        4 => new HealingResult { VitalityHealed = 3, Description = "Strong restoration: 3 VIT" },
        5 => new HealingResult { VitalityHealed = 3, Description = "Powerful restoration: 3 VIT" },
        6 => new HealingResult { VitalityHealed = 4, Description = "Major restoration: 4 VIT" },
        _ => new HealingResult { VitalityHealed = 4 + (sv - 6) / 2, Description = $"Exceptional restoration: {4 + (sv - 6) / 2} VIT" }
    };

    private static HealingResult GetCombinedHealing(int sv)
    {
        var fatigue = GetFatigueHealing(sv);
        var vitality = GetVitalityHealing(sv);
        return new HealingResult
        {
            FatigueHealed = fatigue.FatigueHealed,
            VitalityHealed = vitality.VitalityHealed,
            Description = $"Full healing: {fatigue.FatigueHealed} FAT, {vitality.VitalityHealed} VIT"
        };
    }

    private static HealingType GetHealingType(string spellId) => spellId switch
    {
        "minor-heal" => HealingType.Fatigue,
        "restore-vitality" => HealingType.Vitality,
        "major-heal" => HealingType.Both,
        "regenerate" => HealingType.Both,
        _ => HealingType.Fatigue
    };

    private static string BuildDescription(SpellEffectContext context, int effectiveSV, HealingResult healing)
    {
        var pumpText = context.TotalPumpValue > 0
            ? $" (pumped +{context.TotalPumpValue})"
            : "";

        var healingParts = new List<string>();
        if (healing.FatigueHealed > 0)
            healingParts.Add($"{healing.FatigueHealed} FAT");
        if (healing.VitalityHealed > 0)
            healingParts.Add($"{healing.VitalityHealed} VIT");

        return $"{context.Spell.SkillId} SV {effectiveSV}{pumpText}: heals {string.Join(", ", healingParts)}";
    }

    private static string BuildNarrative(SpellEffectContext context, int effectiveSV, HealingResult healing, bool isSelf)
    {
        var spellName = GetSpellDisplayName(context.Spell.SkillId);
        var targetText = isSelf ? "the caster" : "the target";

        if (effectiveSV >= 6)
        {
            return $"A brilliant surge of healing energy washes over {targetText} as the {spellName} takes effect, " +
                   $"restoring {FormatHealing(healing)}.";
        }

        if (effectiveSV >= 3)
        {
            return $"Warm healing light envelops {targetText} from the {spellName}, " +
                   $"restoring {FormatHealing(healing)}.";
        }

        return $"The {spellName} provides {targetText} with gentle healing, " +
               $"restoring {FormatHealing(healing)}.";
    }

    private static string FormatHealing(HealingResult healing)
    {
        var parts = new List<string>();
        if (healing.FatigueHealed > 0)
            parts.Add($"{healing.FatigueHealed} fatigue");
        if (healing.VitalityHealed > 0)
            parts.Add($"{healing.VitalityHealed} vitality");
        return string.Join(" and ", parts);
    }

    private static string GetSpellDisplayName(string spellId) => spellId switch
    {
        "minor-heal" => "Minor Heal",
        "restore-vitality" => "Restore Vitality",
        "major-heal" => "Major Heal",
        "regenerate" => "Regenerate",
        _ => spellId
    };
}

/// <summary>
/// Type of healing a spell provides.
/// </summary>
public enum HealingType
{
    Fatigue,
    Vitality,
    Both
}

/// <summary>
/// Result from a healing lookup.
/// </summary>
public class HealingResult
{
    public int FatigueHealed { get; init; }
    public int VitalityHealed { get; init; }
    public string Description { get; init; } = string.Empty;
}
