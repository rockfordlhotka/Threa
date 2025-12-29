using System.Collections.Generic;

namespace GameMechanics.Actions;

/// <summary>
/// Encapsulates the Ability Score (AS) calculation for an action,
/// including base value and all modifiers.
/// </summary>
public class AbilityScore
{
    /// <summary>
    /// The base attribute value used (or average if multiple attributes).
    /// </summary>
    public int AttributeValue { get; set; }

    /// <summary>
    /// The skill level (0 if untrained).
    /// </summary>
    public int SkillLevel { get; set; }

    /// <summary>
    /// The standard offset (-5 per design spec).
    /// </summary>
    public const int StandardOffset = -5;

    /// <summary>
    /// The base AS before modifiers: Attribute + SkillLevel - 5.
    /// </summary>
    public int BaseAS => AttributeValue + SkillLevel + StandardOffset;

    /// <summary>
    /// All modifiers affecting this ability score.
    /// </summary>
    public ModifierStack Modifiers { get; } = new();

    /// <summary>
    /// The final AS after all modifiers.
    /// </summary>
    public int FinalAS => BaseAS + Modifiers.Total;

    /// <summary>
    /// Name of the skill being used.
    /// </summary>
    public string SkillName { get; set; } = string.Empty;

    /// <summary>
    /// Name(s) of the attribute(s) being used.
    /// </summary>
    public string AttributeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets a formatted breakdown for display.
    /// </summary>
    public string GetBreakdown()
    {
        var lines = new List<string>
        {
            $"{AttributeName}: {AttributeValue}",
            $"{SkillName} Level: {SkillLevel}",
            $"Base Offset: {StandardOffset}",
            $"Base AS: {BaseAS}"
        };

        if (Modifiers.Modifiers.Count > 0)
        {
            lines.Add("---");
            foreach (var mod in Modifiers.Modifiers)
            {
                lines.Add(mod.ToString());
            }
            lines.Add("---");
            lines.Add($"Final AS: {FinalAS}");
        }

        return string.Join("\n", lines);
    }

    /// <summary>
    /// Adds a modifier to this ability score.
    /// </summary>
    public void AddModifier(ModifierSource source, string description, int value)
    {
        Modifiers.Add(source, description, value);
    }

    /// <summary>
    /// Adds the multiple action penalty (-1 AS for non-first actions).
    /// </summary>
    public void ApplyMultipleActionPenalty()
    {
        Modifiers.Add(ModifierSource.MultipleAction, "Multiple Action", -1);
    }

    /// <summary>
    /// Adds wound penalties (-2 AS per wound).
    /// </summary>
    public void ApplyWoundPenalties(int woundCount)
    {
        if (woundCount > 0)
        {
            Modifiers.Add(ModifierSource.Wound, $"Wounds (×{woundCount})", woundCount * -2);
        }
    }

    /// <summary>
    /// Adds boost bonus (+1 AS per AP/FAT spent).
    /// </summary>
    public void ApplyBoostBonus(int boostAmount)
    {
        if (boostAmount > 0)
        {
            Modifiers.Add(ModifierSource.Boost, $"Boost (×{boostAmount})", boostAmount);
        }
    }

    /// <summary>
    /// Adds aim bonus (+2 AS for ranged attacks after aiming).
    /// </summary>
    public void ApplyAimBonus()
    {
        Modifiers.Add(ModifierSource.Aim, "Aimed", 2);
    }
}
