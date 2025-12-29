using System.Collections.Generic;

namespace GameMechanics.Actions;

/// <summary>
/// Represents the cost to perform an action.
/// </summary>
public class ActionCost
{
    /// <summary>
    /// Action Points spent.
    /// </summary>
    public int AP { get; set; }

    /// <summary>
    /// Fatigue spent.
    /// </summary>
    public int FAT { get; set; }

    /// <summary>
    /// Mana spent (for spells).
    /// </summary>
    public int Mana { get; set; }

    /// <summary>
    /// Additional AP/FAT spent for boosts.
    /// </summary>
    public int BoostAP { get; set; }

    /// <summary>
    /// Additional FAT spent for boosts.
    /// </summary>
    public int BoostFAT { get; set; }

    /// <summary>
    /// Total AP cost including boosts.
    /// </summary>
    public int TotalAP => AP + BoostAP;

    /// <summary>
    /// Total FAT cost including boosts.
    /// </summary>
    public int TotalFAT => FAT + BoostFAT;

    /// <summary>
    /// Total boost bonus (+1 per AP/FAT spent on boosts).
    /// </summary>
    public int BoostBonus => BoostAP + BoostFAT;

    /// <summary>
    /// Whether this is a free action (no AP/FAT cost).
    /// </summary>
    public bool IsFreeAction => AP == 0 && FAT == 0;

    /// <summary>
    /// Creates a standard action cost (1 AP + 1 FAT).
    /// </summary>
    public static ActionCost Standard() => new() { AP = 1, FAT = 1 };

    /// <summary>
    /// Creates a fatigue-free action cost (2 AP + 0 FAT).
    /// </summary>
    public static ActionCost FatigueFree() => new() { AP = 2, FAT = 0 };

    /// <summary>
    /// Creates a free action cost (0 AP + 0 FAT).
    /// </summary>
    public static ActionCost Free() => new() { AP = 0, FAT = 0 };

    /// <summary>
    /// Creates a spell action cost with mana.
    /// </summary>
    public static ActionCost Spell(int manaCost) => new() { AP = 1, FAT = 1, Mana = manaCost };

    /// <summary>
    /// Adds AP boost to the cost.
    /// </summary>
    public ActionCost WithAPBoost(int amount)
    {
        BoostAP += amount;
        return this;
    }

    /// <summary>
    /// Adds FAT boost to the cost.
    /// </summary>
    public ActionCost WithFATBoost(int amount)
    {
        BoostFAT += amount;
        return this;
    }

    public override string ToString()
    {
        var parts = new List<string>();
        if (TotalAP > 0) parts.Add($"{TotalAP} AP");
        if (TotalFAT > 0) parts.Add($"{TotalFAT} FAT");
        if (Mana > 0) parts.Add($"{Mana} Mana");
        if (BoostBonus > 0) parts.Add($"+{BoostBonus} AS from boosts");
        return parts.Count > 0 ? string.Join(", ", parts) : "Free";
    }
}
