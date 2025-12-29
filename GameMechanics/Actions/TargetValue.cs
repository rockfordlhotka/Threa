using System.Collections.Generic;
using Threa.Dal.Dto;

namespace GameMechanics.Actions;

/// <summary>
/// Represents the Target Value (TV) for an action, including
/// how it was determined and its final value.
/// </summary>
public class TargetValue
{
    /// <summary>
    /// How this TV was determined.
    /// </summary>
    public TargetValueType Type { get; set; }

    /// <summary>
    /// The base TV before modifiers.
    /// For Fixed: the difficulty value.
    /// For Opposed: the opponent's AS.
    /// For Passive: the opponent's AS - 1.
    /// </summary>
    public int BaseTV { get; set; }

    /// <summary>
    /// The dice roll (for opposed TV only, 0 for fixed/passive).
    /// </summary>
    public int DiceRoll { get; set; }

    /// <summary>
    /// Modifiers to TV (range, cover, movement, etc. for ranged attacks).
    /// </summary>
    public ModifierStack Modifiers { get; } = new();

    /// <summary>
    /// The final TV value.
    /// </summary>
    public int FinalTV => BaseTV + DiceRoll + Modifiers.Total;

    /// <summary>
    /// Description of the target or difficulty.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Creates a fixed TV from a difficulty level.
    /// </summary>
    public static TargetValue Fixed(int difficulty, string description = "")
    {
        return new TargetValue
        {
            Type = TargetValueType.Fixed,
            BaseTV = difficulty,
            Description = description
        };
    }

    /// <summary>
    /// Creates a fixed TV from a standard difficulty category.
    /// </summary>
    public static TargetValue FromDifficulty(DifficultyLevel difficulty)
    {
        return new TargetValue
        {
            Type = TargetValueType.Fixed,
            BaseTV = (int)difficulty,
            Description = difficulty.ToString()
        };
    }

    /// <summary>
    /// Creates an opposed TV from an opponent's skill roll.
    /// </summary>
    public static TargetValue Opposed(int opponentAS, int diceRoll, string opponentDescription = "Opponent")
    {
        return new TargetValue
        {
            Type = TargetValueType.Opposed,
            BaseTV = opponentAS,
            DiceRoll = diceRoll,
            Description = opponentDescription
        };
    }

    /// <summary>
    /// Creates a passive TV from an opponent's AS (no roll, -1 penalty).
    /// </summary>
    public static TargetValue Passive(int opponentAS, string opponentDescription = "Opponent")
    {
        return new TargetValue
        {
            Type = TargetValueType.Passive,
            BaseTV = opponentAS - 1,
            DiceRoll = 0,
            Description = $"{opponentDescription} (Passive)"
        };
    }

    /// <summary>
    /// Adds a range modifier (for ranged attacks).
    /// </summary>
    public void AddRangeModifier(RangeCategory range)
    {
        int modifier = range switch
        {
            RangeCategory.Short => 0,  // Base TV 6
            RangeCategory.Medium => 2,  // Base TV 8
            RangeCategory.Long => 4,    // Base TV 10
            RangeCategory.Extreme => 6, // Base TV 12
            _ => 0
        };
        if (modifier != 0)
            Modifiers.Add(ModifierSource.Environment, range.ToString() + " Range", modifier);
    }

    public string GetBreakdown()
    {
        var lines = new List<string>
        {
            $"Type: {Type}",
            $"{Description}: {BaseTV}"
        };

        if (DiceRoll != 0)
            lines.Add($"Roll: {(DiceRoll >= 0 ? "+" : "")}{DiceRoll}");

        if (Modifiers.Modifiers.Count > 0)
        {
            foreach (var mod in Modifiers.Modifiers)
            {
                lines.Add(mod.ToString());
            }
        }

        lines.Add($"Final TV: {FinalTV}");
        return string.Join("\n", lines);
    }
}

/// <summary>
/// Standard difficulty levels and their TV values.
/// </summary>
public enum DifficultyLevel
{
    Trivial = 2,
    Easy = 4,
    Routine = 6,
    Moderate = 8,
    Challenging = 10,
    Hard = 12,
    VeryHard = 14,
    Extreme = 16,
    Impossible = 18
}

/// <summary>
/// Range categories for ranged attacks.
/// </summary>
public enum RangeCategory
{
    /// <summary>Short range - Base TV 6</summary>
    Short = 0,
    /// <summary>Medium range - Base TV 8</summary>
    Medium = 1,
    /// <summary>Long range - Base TV 10</summary>
    Long = 2,
    /// <summary>Extreme range - Base TV 12</summary>
    Extreme = 3
}
