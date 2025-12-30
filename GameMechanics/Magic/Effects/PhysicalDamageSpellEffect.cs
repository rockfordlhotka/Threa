using System.Collections.Generic;
using GameMechanics.Combat;

namespace GameMechanics.Magic.Effects;

/// <summary>
/// Spell effect for physical damage spells like Mystic Punch.
/// Uses the same damage table as melee combat.
/// Pump: Each pump point adds +1 to SV before damage lookup.
/// </summary>
public class PhysicalDamageSpellEffect : ISpellEffect
{
    /// <inheritdoc/>
    public IEnumerable<string> HandledSpellIds => ["mystic-punch", "force-strike", "telekinetic-slam"];

    /// <inheritdoc/>
    public SpellEffectResult Resolve(SpellEffectContext context)
    {
        if (context.TargetCharacterId == null)
        {
            return SpellEffectResult.Failure("Physical damage spell requires a target.");
        }

        var targetId = context.TargetCharacterId.Value;

        // Apply pump bonus to SV
        var effectiveSV = context.SV + context.TotalPumpValue;

        // Use the melee damage table
        var damageResult = CombatResultTables.GetDamage(effectiveSV);

        var damageDealt = new SpellDamageDealt
        {
            CharacterId = targetId,
            FatigueDamage = damageResult.FatigueDamage,
            VitalityDamage = damageResult.VitalityDamage,
            CausedWound = damageResult.CausesWound,
            DamageType = "Physical (Magic)",
            Description = BuildDescription(context, effectiveSV, damageResult)
        };

        var narrative = BuildNarrative(context, effectiveSV, damageResult);

        return new SpellEffectResult
        {
            Success = true,
            Description = damageDealt.Description,
            NarrativeText = narrative,
            DamageDealt = [damageDealt]
        };
    }

    private static string BuildDescription(SpellEffectContext context, int effectiveSV, DamageResult damage)
    {
        var pumpText = context.TotalPumpValue > 0
            ? $" (pumped +{context.TotalPumpValue})"
            : "";

        if (damage.FatigueDamage == 0 && damage.VitalityDamage == 0)
        {
            return $"{context.Spell.SkillId} SV {effectiveSV}{pumpText}: Miss";
        }

        var woundText = damage.CausesWound ? ", causes wound" : "";
        return $"{context.Spell.SkillId} SV {effectiveSV}{pumpText}: {damage.FatigueDamage} FAT, {damage.VitalityDamage} VIT{woundText}";
    }

    private static string BuildNarrative(SpellEffectContext context, int effectiveSV, DamageResult damage)
    {
        var spellName = GetSpellDisplayName(context.Spell.SkillId);

        if (effectiveSV < 0)
        {
            return $"The {spellName} misses its target entirely.";
        }

        if (damage.CausesWound)
        {
            return $"The {spellName} connects with devastating force, " +
                   $"dealing {damage.FatigueDamage} fatigue and {damage.VitalityDamage} vitality damage, leaving a wound!";
        }

        if (damage.VitalityDamage > 0)
        {
            return $"The {spellName} strikes hard, " +
                   $"dealing {damage.FatigueDamage} fatigue and {damage.VitalityDamage} vitality damage.";
        }

        return $"The {spellName} connects, dealing {damage.FatigueDamage} fatigue damage.";
    }

    private static string GetSpellDisplayName(string spellId) => spellId switch
    {
        "mystic-punch" => "Mystic Punch",
        "force-strike" => "Force Strike",
        "telekinetic-slam" => "Telekinetic Slam",
        _ => spellId
    };
}
