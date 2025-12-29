using System.Collections.Generic;
using GameMechanics.Actions;
using Threa.Dal.Dto;

namespace GameMechanics.Effects;

/// <summary>
/// Extension methods to convert effect modifiers to the Actions system's ModifierStack.
/// </summary>
public static class EffectModifierExtensions
{
    /// <summary>
    /// Adds effect modifiers to a ModifierStack for action resolution.
    /// </summary>
    /// <param name="modifiers">The modifier stack to add to.</param>
    /// <param name="characterEffects">Active character effects.</param>
    /// <param name="itemEffects">Active equipped item effects.</param>
    /// <param name="calculator">The effect calculator.</param>
    public static void AddEffectModifiers(
        this ModifierStack modifiers,
        IEnumerable<CharacterEffect> characterEffects,
        IEnumerable<ItemEffect> itemEffects,
        EffectCalculator calculator)
    {
        // Add AS modifiers
        int asModifier = calculator.GetTotalASModifier(characterEffects, itemEffects);
        if (asModifier != 0)
        {
            modifiers.Add(ModifierSource.Effect, "Effect Modifiers", asModifier);
        }
    }

    /// <summary>
    /// Creates AsModifier entries for each active effect for detailed breakdown.
    /// </summary>
    public static List<AsModifier> GetDetailedEffectModifiers(
        IEnumerable<CharacterEffect> characterEffects,
        IEnumerable<ItemEffect> itemEffects,
        EffectImpactType impactType)
    {
        var modifiers = new List<AsModifier>();

        foreach (var effect in characterEffects)
        {
            if (effect.Definition == null) continue;

            foreach (var impact in effect.Definition.Impacts)
            {
                if (impact.ImpactType == impactType)
                {
                    int value = (int)(impact.Value * effect.CurrentStacks);
                    modifiers.Add(new AsModifier(
                        ModifierSource.Effect,
                        effect.Definition.Name,
                        value));
                }
            }
        }

        foreach (var effect in itemEffects)
        {
            if (effect.Definition == null) continue;

            string itemName = effect.Item?.CustomName ?? effect.Item?.Template?.Name ?? "Item";

            foreach (var impact in effect.Definition.Impacts)
            {
                if (impact.ImpactType == impactType)
                {
                    int value = (int)(impact.Value * effect.CurrentStacks);
                    modifiers.Add(new AsModifier(
                        ModifierSource.Effect,
                        $"{effect.Definition.Name} ({itemName})",
                        value));
                }
            }
        }

        return modifiers;
    }

    /// <summary>
    /// Gets a list of all active condition names on a character.
    /// </summary>
    public static List<string> GetActiveConditions(IEnumerable<CharacterEffect> characterEffects)
    {
        var conditions = new List<string>();

        foreach (var effect in characterEffects)
        {
            if (effect.Definition?.EffectType == EffectType.Condition)
            {
                conditions.Add(effect.Definition.Name);
            }
        }

        return conditions;
    }

    /// <summary>
    /// Checks if a character is prevented from taking actions due to conditions.
    /// </summary>
    public static bool IsActionPrevented(
        IEnumerable<CharacterEffect> characterEffects,
        IEnumerable<ItemEffect> itemEffects,
        EffectCalculator calculator)
    {
        return calculator.HasSpecialAbility(characterEffects, itemEffects, "Actions") &&
               calculator.GetSpecialAbilities(characterEffects, itemEffects)
                   .TryGetValue("Actions", out var value) && value == 0;
    }

    /// <summary>
    /// Checks if a character is invisible.
    /// </summary>
    public static bool IsInvisible(
        IEnumerable<CharacterEffect> characterEffects,
        IEnumerable<ItemEffect> itemEffects,
        EffectCalculator calculator)
    {
        return calculator.HasSpecialAbility(characterEffects, itemEffects, "Visibility") &&
               calculator.GetSpecialAbilities(characterEffects, itemEffects)
                   .TryGetValue("Visibility", out var value) && value == 0;
    }
}
