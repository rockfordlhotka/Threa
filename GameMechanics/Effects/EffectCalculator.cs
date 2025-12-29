using System;
using System.Collections.Generic;
using System.Linq;
using Threa.Dal.Dto;

namespace GameMechanics.Effects;

/// <summary>
/// Calculates total modifiers from effects for attributes, skills, combat values, etc.
/// This class is stateless and can be used for pure calculations.
/// </summary>
public class EffectCalculator
{
    /// <summary>
    /// Gets the total attribute modifier from all effects.
    /// </summary>
    /// <param name="characterEffects">Active effects on the character.</param>
    /// <param name="itemEffects">Active effects on equipped items.</param>
    /// <param name="attributeName">The attribute name (STR, DEX, etc.).</param>
    public int GetTotalAttributeModifier(
        IEnumerable<CharacterEffect> characterEffects,
        IEnumerable<ItemEffect> itemEffects,
        string attributeName)
    {
        int total = 0;

        foreach (var effect in characterEffects.Where(e => e.Definition != null))
        {
            var impacts = effect.Definition!.Impacts
                .Where(i => i.ImpactType == EffectImpactType.AttributeModifier &&
                           (i.Target.Equals(attributeName, StringComparison.OrdinalIgnoreCase) ||
                            i.Target.Equals("All", StringComparison.OrdinalIgnoreCase)));

            foreach (var impact in impacts)
            {
                total += GetImpactValue(impact, effect.CurrentStacks);
            }
        }

        foreach (var effect in itemEffects.Where(e => e.Definition != null))
        {
            var impacts = effect.Definition!.Impacts
                .Where(i => i.ImpactType == EffectImpactType.AttributeModifier &&
                           (i.Target.Equals(attributeName, StringComparison.OrdinalIgnoreCase) ||
                            i.Target.Equals("All", StringComparison.OrdinalIgnoreCase)));

            foreach (var impact in impacts)
            {
                total += GetImpactValue(impact, effect.CurrentStacks);
            }
        }

        return total;
    }

    /// <summary>
    /// Gets the total skill modifier from all effects.
    /// </summary>
    public int GetTotalSkillModifier(
        IEnumerable<CharacterEffect> characterEffects,
        IEnumerable<ItemEffect> itemEffects,
        string skillName)
    {
        int total = 0;

        foreach (var effect in characterEffects.Where(e => e.Definition != null))
        {
            var impacts = effect.Definition!.Impacts
                .Where(i => i.ImpactType == EffectImpactType.SkillModifier &&
                           (i.Target.Equals(skillName, StringComparison.OrdinalIgnoreCase) ||
                            i.Target.Equals("All", StringComparison.OrdinalIgnoreCase) ||
                            i.Target.Equals("Physical", StringComparison.OrdinalIgnoreCase) ||
                            i.Target.Equals("Mental", StringComparison.OrdinalIgnoreCase)));

            foreach (var impact in impacts)
            {
                total += GetImpactValue(impact, effect.CurrentStacks);
            }
        }

        foreach (var effect in itemEffects.Where(e => e.Definition != null))
        {
            var impacts = effect.Definition!.Impacts
                .Where(i => i.ImpactType == EffectImpactType.SkillModifier &&
                           (i.Target.Equals(skillName, StringComparison.OrdinalIgnoreCase) ||
                            i.Target.Equals("All", StringComparison.OrdinalIgnoreCase) ||
                            i.Target.Equals("RelatedSkill", StringComparison.OrdinalIgnoreCase)));

            foreach (var impact in impacts)
            {
                total += GetImpactValue(impact, effect.CurrentStacks);
            }
        }

        return total;
    }

    /// <summary>
    /// Gets the total AS (Ability Score) modifier from all effects.
    /// </summary>
    /// <param name="targetType">Optional filter: "All", "Physical", "Mental", or specific skill.</param>
    public int GetTotalASModifier(
        IEnumerable<CharacterEffect> characterEffects,
        IEnumerable<ItemEffect> itemEffects,
        string? targetType = null)
    {
        int total = 0;

        foreach (var effect in characterEffects.Where(e => e.Definition != null))
        {
            var impacts = effect.Definition!.Impacts
                .Where(i => i.ImpactType == EffectImpactType.ASModifier &&
                           MatchesTarget(i.Target, targetType));

            foreach (var impact in impacts)
            {
                total += GetImpactValue(impact, effect.CurrentStacks);
            }
        }

        foreach (var effect in itemEffects.Where(e => e.Definition != null))
        {
            var impacts = effect.Definition!.Impacts
                .Where(i => i.ImpactType == EffectImpactType.ASModifier &&
                           MatchesTarget(i.Target, targetType));

            foreach (var impact in impacts)
            {
                total += GetImpactValue(impact, effect.CurrentStacks);
            }
        }

        return total;
    }

    /// <summary>
    /// Gets the total AV (Attack Value) modifier from all effects.
    /// </summary>
    public int GetTotalAVModifier(
        IEnumerable<CharacterEffect> characterEffects,
        IEnumerable<ItemEffect> itemEffects)
    {
        int total = 0;

        foreach (var effect in characterEffects.Where(e => e.Definition != null))
        {
            var impacts = effect.Definition!.Impacts
                .Where(i => i.ImpactType == EffectImpactType.AVModifier);

            foreach (var impact in impacts)
            {
                total += GetImpactValue(impact, effect.CurrentStacks);
            }
        }

        foreach (var effect in itemEffects.Where(e => e.Definition != null))
        {
            var impacts = effect.Definition!.Impacts
                .Where(i => i.ImpactType == EffectImpactType.AVModifier);

            foreach (var impact in impacts)
            {
                total += GetImpactValue(impact, effect.CurrentStacks);
            }
        }

        return total;
    }

    /// <summary>
    /// Gets the total TV (Target Value) modifier from all effects.
    /// Positive = harder to hit, Negative = easier to hit.
    /// </summary>
    public int GetTotalTVModifier(
        IEnumerable<CharacterEffect> characterEffects,
        IEnumerable<ItemEffect> itemEffects)
    {
        int total = 0;

        foreach (var effect in characterEffects.Where(e => e.Definition != null))
        {
            var impacts = effect.Definition!.Impacts
                .Where(i => i.ImpactType == EffectImpactType.TVModifier);

            foreach (var impact in impacts)
            {
                // Only count "Self" modifiers (affects the character's own TV)
                if (impact.Target.Equals("Self", StringComparison.OrdinalIgnoreCase) ||
                    impact.Target.Equals("All", StringComparison.OrdinalIgnoreCase))
                {
                    total += GetImpactValue(impact, effect.CurrentStacks);
                }
            }
        }

        foreach (var effect in itemEffects.Where(e => e.Definition != null))
        {
            var impacts = effect.Definition!.Impacts
                .Where(i => i.ImpactType == EffectImpactType.TVModifier);

            foreach (var impact in impacts)
            {
                total += GetImpactValue(impact, effect.CurrentStacks);
            }
        }

        return total;
    }

    /// <summary>
    /// Gets the total SV (Success Value) modifier from all effects.
    /// </summary>
    public int GetTotalSVModifier(
        IEnumerable<CharacterEffect> characterEffects,
        IEnumerable<ItemEffect> itemEffects)
    {
        int total = 0;

        foreach (var effect in characterEffects.Where(e => e.Definition != null))
        {
            var impacts = effect.Definition!.Impacts
                .Where(i => i.ImpactType == EffectImpactType.SVModifier);

            foreach (var impact in impacts)
            {
                total += GetImpactValue(impact, effect.CurrentStacks);
            }
        }

        foreach (var effect in itemEffects.Where(e => e.Definition != null))
        {
            var impacts = effect.Definition!.Impacts
                .Where(i => i.ImpactType == EffectImpactType.SVModifier);

            foreach (var impact in impacts)
            {
                total += GetImpactValue(impact, effect.CurrentStacks);
            }
        }

        return total;
    }

    /// <summary>
    /// Gets the total movement modifier from all effects.
    /// </summary>
    public int GetTotalMovementModifier(
        IEnumerable<CharacterEffect> characterEffects,
        IEnumerable<ItemEffect> itemEffects)
    {
        int total = 0;

        foreach (var effect in characterEffects.Where(e => e.Definition != null))
        {
            var impacts = effect.Definition!.Impacts
                .Where(i => i.ImpactType == EffectImpactType.MovementModifier);

            foreach (var impact in impacts)
            {
                total += GetImpactValue(impact, effect.CurrentStacks);
            }
        }

        foreach (var effect in itemEffects.Where(e => e.Definition != null))
        {
            var impacts = effect.Definition!.Impacts
                .Where(i => i.ImpactType == EffectImpactType.MovementModifier);

            foreach (var impact in impacts)
            {
                total += GetImpactValue(impact, effect.CurrentStacks);
            }
        }

        return total;
    }

    /// <summary>
    /// Gets damage over time values that should be applied this tick.
    /// Only returns damage for effects that are ready to tick.
    /// </summary>
    /// <returns>Tuple of (FAT damage, VIT damage).</returns>
    public (int fatDamage, int vitDamage) GetDamageOverTime(IEnumerable<CharacterEffect> characterEffects)
    {
        int fatDamage = 0;
        int vitDamage = 0;

        foreach (var effect in characterEffects.Where(e => e.Definition != null))
        {
            // Only apply damage if RoundsUntilTick is 0 or 1 (will tick this round)
            if (effect.RoundsUntilTick == null || effect.RoundsUntilTick > 1)
                continue;

            var dotImpacts = effect.Definition!.Impacts
                .Where(i => i.ImpactType == EffectImpactType.DamageOverTime);

            foreach (var impact in dotImpacts)
            {
                int damage = GetImpactValue(impact, effect.CurrentStacks);
                
                if (impact.Target.Equals("FAT", StringComparison.OrdinalIgnoreCase))
                    fatDamage += damage;
                else if (impact.Target.Equals("VIT", StringComparison.OrdinalIgnoreCase))
                    vitDamage += damage;
            }
        }

        return (fatDamage, vitDamage);
    }

    /// <summary>
    /// Checks if a character has a special ability from effects.
    /// </summary>
    public bool HasSpecialAbility(
        IEnumerable<CharacterEffect> characterEffects,
        IEnumerable<ItemEffect> itemEffects,
        string abilityName)
    {
        foreach (var effect in characterEffects.Where(e => e.Definition != null))
        {
            var hasAbility = effect.Definition!.Impacts
                .Any(i => i.ImpactType == EffectImpactType.SpecialAbility &&
                         i.Target.Equals(abilityName, StringComparison.OrdinalIgnoreCase));

            if (hasAbility)
                return true;
        }

        foreach (var effect in itemEffects.Where(e => e.Definition != null))
        {
            var hasAbility = effect.Definition!.Impacts
                .Any(i => i.ImpactType == EffectImpactType.SpecialAbility &&
                         i.Target.Equals(abilityName, StringComparison.OrdinalIgnoreCase));

            if (hasAbility)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Gets all special abilities granted by effects.
    /// </summary>
    public Dictionary<string, decimal> GetSpecialAbilities(
        IEnumerable<CharacterEffect> characterEffects,
        IEnumerable<ItemEffect> itemEffects)
    {
        var abilities = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        foreach (var effect in characterEffects.Where(e => e.Definition != null))
        {
            var specialImpacts = effect.Definition!.Impacts
                .Where(i => i.ImpactType == EffectImpactType.SpecialAbility);

            foreach (var impact in specialImpacts)
            {
                abilities[impact.Target] = impact.Value;
            }
        }

        foreach (var effect in itemEffects.Where(e => e.Definition != null))
        {
            var specialImpacts = effect.Definition!.Impacts
                .Where(i => i.ImpactType == EffectImpactType.SpecialAbility);

            foreach (var impact in specialImpacts)
            {
                abilities[impact.Target] = impact.Value;
            }
        }

        return abilities;
    }

    /// <summary>
    /// Checks if an action would break any effects with break conditions.
    /// </summary>
    public List<CharacterEffect> GetEffectsBrokenByAction(
        IEnumerable<CharacterEffect> characterEffects,
        string actionType)
    {
        var brokenEffects = new List<CharacterEffect>();

        foreach (var effect in characterEffects.Where(e => e.Definition?.BreakConditions != null))
        {
            var breakConditions = effect.Definition!.BreakConditions!
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (breakConditions.Any(c => c.Equals(actionType, StringComparison.OrdinalIgnoreCase)))
            {
                brokenEffects.Add(effect);
            }
        }

        return brokenEffects;
    }

    #region Private Helpers

    private int GetImpactValue(EffectImpact impact, int stacks)
    {
        // For stackable effects, multiply value by stacks
        return (int)(impact.Value * stacks);
    }

    private bool MatchesTarget(string impactTarget, string? requestedTarget)
    {
        // If no filter, accept all targets
        if (string.IsNullOrEmpty(requestedTarget))
            return true;

        // "All" impacts always match
        if (impactTarget.Equals("All", StringComparison.OrdinalIgnoreCase))
            return true;

        // Exact match
        if (impactTarget.Equals(requestedTarget, StringComparison.OrdinalIgnoreCase))
            return true;

        // If requesting "Physical" or "Mental" category
        if (requestedTarget.Equals("Physical", StringComparison.OrdinalIgnoreCase) &&
            impactTarget.Equals("Physical", StringComparison.OrdinalIgnoreCase))
            return true;

        if (requestedTarget.Equals("Mental", StringComparison.OrdinalIgnoreCase) &&
            impactTarget.Equals("Mental", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    #endregion
}
