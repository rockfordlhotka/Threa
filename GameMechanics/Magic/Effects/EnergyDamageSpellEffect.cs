using System.Collections.Generic;

namespace GameMechanics.Magic.Effects;

/// <summary>
/// Spell effect for energy damage spells like Fire Bolt and Ice Shard.
/// Uses a dedicated energy damage table (not the melee table).
/// Pump: Each pump point adds +1 to SV before damage lookup.
/// </summary>
public class EnergyDamageSpellEffect : ISpellEffect
{
    /// <inheritdoc/>
    public IEnumerable<string> HandledSpellIds => ["fire-bolt", "ice-shard", "lightning-bolt", "frost-ray"];

    /// <inheritdoc/>
    public SpellEffectResult Resolve(SpellEffectContext context)
    {
        if (context.TargetCharacterId == null)
        {
            return SpellEffectResult.Failure("Energy damage spell requires a target.");
        }

        var targetId = context.TargetCharacterId.Value;

        // Apply pump bonus to SV
        var effectiveSV = context.SV + context.TotalPumpValue;

        // Use the energy damage table
        var damageResult = GetEnergyDamage(effectiveSV);
        var damageType = GetDamageType(context.Spell.SkillId);

        var damageDealt = new SpellDamageDealt
        {
            CharacterId = targetId,
            FatigueDamage = damageResult.FatigueDamage,
            VitalityDamage = damageResult.VitalityDamage,
            CausedWound = damageResult.CausesWound,
            DamageType = damageType,
            Description = BuildDescription(context, effectiveSV, damageResult, damageType)
        };

        var narrative = BuildNarrative(context, effectiveSV, damageResult, damageType);

        return new SpellEffectResult
        {
            Success = true,
            Description = damageDealt.Description,
            NarrativeText = narrative,
            DamageDealt = [damageDealt]
        };
    }

    /// <summary>
    /// Energy damage lookup table.
    /// Energy spells deal more FAT than physical but less VIT until high SV.
    /// </summary>
    public static EnergyDamageResult GetEnergyDamage(int sv)
    {
        if (sv < 0)
        {
            return new EnergyDamageResult
            {
                FatigueDamage = 0,
                VitalityDamage = 0,
                CausesWound = false,
                Description = "Miss"
            };
        }

        return sv switch
        {
            0 => new EnergyDamageResult
            {
                FatigueDamage = 2,
                VitalityDamage = 0,
                CausesWound = false,
                Description = "Graze: 2 FAT"
            },
            1 => new EnergyDamageResult
            {
                FatigueDamage = 3,
                VitalityDamage = 0,
                CausesWound = false,
                Description = "Light hit: 3 FAT"
            },
            2 => new EnergyDamageResult
            {
                FatigueDamage = 4,
                VitalityDamage = 0,
                CausesWound = false,
                Description = "Solid hit: 4 FAT"
            },
            3 => new EnergyDamageResult
            {
                FatigueDamage = 5,
                VitalityDamage = 1,
                CausesWound = false,
                Description = "Good hit: 5 FAT, 1 VIT"
            },
            4 => new EnergyDamageResult
            {
                FatigueDamage = 6,
                VitalityDamage = 1,
                CausesWound = false,
                Description = "Strong hit: 6 FAT, 1 VIT"
            },
            5 => new EnergyDamageResult
            {
                FatigueDamage = 7,
                VitalityDamage = 2,
                CausesWound = false,
                Description = "Heavy hit: 7 FAT, 2 VIT"
            },
            6 => new EnergyDamageResult
            {
                FatigueDamage = 8,
                VitalityDamage = 3,
                CausesWound = true,
                Description = "Searing: 8 FAT, 3 VIT, wound"
            },
            7 => new EnergyDamageResult
            {
                FatigueDamage = 9,
                VitalityDamage = 4,
                CausesWound = true,
                Description = "Scorching: 9 FAT, 4 VIT, wound"
            },
            _ => new EnergyDamageResult
            {
                FatigueDamage = 9 + (sv - 7),
                VitalityDamage = 4 + (sv - 7),
                CausesWound = true,
                Description = $"Devastating: {9 + (sv - 7)} FAT, {4 + (sv - 7)} VIT, wound"
            }
        };
    }

    private static string GetDamageType(string spellId) => spellId switch
    {
        "fire-bolt" => "Fire",
        "ice-shard" or "frost-ray" => "Cold",
        "lightning-bolt" => "Lightning",
        _ => "Energy"
    };

    private static string BuildDescription(SpellEffectContext context, int effectiveSV, EnergyDamageResult damage, string damageType)
    {
        var pumpText = context.TotalPumpValue > 0
            ? $" (pumped +{context.TotalPumpValue})"
            : "";

        if (damage.FatigueDamage == 0 && damage.VitalityDamage == 0)
        {
            return $"{context.Spell.SkillId} SV {effectiveSV}{pumpText}: Miss";
        }

        var woundText = damage.CausesWound ? ", causes wound" : "";
        return $"{context.Spell.SkillId} ({damageType}) SV {effectiveSV}{pumpText}: {damage.FatigueDamage} FAT, {damage.VitalityDamage} VIT{woundText}";
    }

    private static string BuildNarrative(SpellEffectContext context, int effectiveSV, EnergyDamageResult damage, string damageType)
    {
        var spellName = GetSpellDisplayName(context.Spell.SkillId);
        var elementDesc = GetElementDescription(damageType);

        if (effectiveSV < 0)
        {
            return $"The {spellName} {elementDesc} harmlessly past the target.";
        }

        if (damage.CausesWound)
        {
            return $"The {spellName} {elementDesc} through the target with searing intensity, " +
                   $"dealing {damage.FatigueDamage} fatigue and {damage.VitalityDamage} vitality damage, leaving a {damageType.ToLower()} wound!";
        }

        if (damage.VitalityDamage > 0)
        {
            return $"The {spellName} {elementDesc} the target squarely, " +
                   $"dealing {damage.FatigueDamage} fatigue and {damage.VitalityDamage} vitality damage.";
        }

        return $"The {spellName} {elementDesc} the target, dealing {damage.FatigueDamage} fatigue damage.";
    }

    private static string GetSpellDisplayName(string spellId) => spellId switch
    {
        "fire-bolt" => "Fire Bolt",
        "ice-shard" => "Ice Shard",
        "lightning-bolt" => "Lightning Bolt",
        "frost-ray" => "Frost Ray",
        _ => spellId
    };

    private static string GetElementDescription(string damageType) => damageType switch
    {
        "Fire" => "burns",
        "Cold" => "freezes",
        "Lightning" => "shocks",
        _ => "strikes"
    };
}

/// <summary>
/// Result from an energy damage lookup.
/// </summary>
public class EnergyDamageResult
{
    public int FatigueDamage { get; init; }
    public int VitalityDamage { get; init; }
    public bool CausesWound { get; init; }
    public string Description { get; init; } = string.Empty;
}
