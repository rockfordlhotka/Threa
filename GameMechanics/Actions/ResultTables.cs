using System.Collections.Generic;
using Threa.Dal.Dto;

namespace GameMechanics.Actions;

/// <summary>
/// Result table entries that map Success Value (SV) to outcomes.
/// </summary>
public static class ResultTables
{
    /// <summary>
    /// Gets the result interpretation for a given SV and result table type.
    /// </summary>
    public static ResultInterpretation GetResult(int sv, ResultTableType tableType)
    {
        return tableType switch
        {
            ResultTableType.General => GetGeneralResult(sv),
            ResultTableType.CombatDamage => GetCombatDamageResult(sv),
            ResultTableType.Defense => GetDefenseResult(sv),
            ResultTableType.Social => GetSocialResult(sv),
            ResultTableType.Perception => GetPerceptionResult(sv),
            ResultTableType.Crafting => GetCraftingResult(sv),
            ResultTableType.Healing => GetHealingResult(sv),
            ResultTableType.Movement => GetMovementResult(sv),
            ResultTableType.ManaRecovery => GetManaRecoveryResult(sv),
            _ => GetGeneralResult(sv)
        };
    }

    private static ResultInterpretation GetGeneralResult(int sv)
    {
        if (sv < 0)
        {
            return sv switch
            {
                >= -2 => new ResultInterpretation(false, "Minor Failure", "Task not completed, no complications."),
                >= -4 => new ResultInterpretation(false, "Failure", "Clear lack of success."),
                >= -6 => new ResultInterpretation(false, "Bad Failure", "Complications may arise."),
                >= -8 => new ResultInterpretation(false, "Severe Failure", "Negative consequences occur."),
                _ => new ResultInterpretation(false, "Critical Failure", "Serious problems result.")
            };
        }
        else
        {
            return sv switch
            {
                <= 1 => new ResultInterpretation(true, "Marginal Success", "Barely achieved the goal."),
                <= 3 => new ResultInterpretation(true, "Standard Success", "Competent result."),
                <= 5 => new ResultInterpretation(true, "Good Success", "Above average result."),
                <= 7 => new ResultInterpretation(true, "Excellent Success", "Impressive result."),
                _ => new ResultInterpretation(true, "Outstanding Success", "Exceptional result.")
            };
        }
    }

    private static ResultInterpretation GetCombatDamageResult(int sv)
    {
        if (sv < 0)
        {
            return sv switch
            {
                >= -2 => new ResultInterpretation(false, "Miss", "Attack fails to connect."),
                >= -4 => new ResultInterpretation(false, "Clear Miss", "Attack misses by a wide margin."),
                >= -6 => new ResultInterpretation(false, "Bad Miss", "Possible overextension or stumble."),
                >= -8 => new ResultInterpretation(false, "Fumble", "May drop weapon or fall prone."),
                _ => new ResultInterpretation(false, "Critical Fumble", "Serious mishap occurs.")
            };
        }
        else
        {
            // SV determines damage dice - see DamageValue class for actual calculation
            string damageDice = sv switch
            {
                0 => "1d2",
                1 => "1d3",
                2 => "1d6",
                3 => "1d8",
                4 => "1d10",
                5 => "1d12",
                6 => "1d6+1d8",
                7 => "2d8",
                8 => "2d10",
                9 => "2d12",
                10 => "3d10",
                11 => "3d12",
                >= 12 and <= 14 => "4d10",
                >= 15 and <= 16 => "1d6 (class+1)",
                >= 17 and <= 18 => "1d8 (class+1)",
                _ => "1d10 (class+1)"
            };

            return new ResultInterpretation(true, "Hit", $"Deal {damageDice} damage.")
            {
                DamageMultiplier = 1,
                EffectValue = sv
            };
        }
    }

    private static ResultInterpretation GetDefenseResult(int sv)
    {
        if (sv < 0)
        {
            return new ResultInterpretation(false, "Defense Failed", "The attack hits.")
            {
                EffectValue = -sv // Pass the margin of failure for damage calculation
            };
        }
        else
        {
            return sv switch
            {
                <= 1 => new ResultInterpretation(true, "Barely Dodged", "Attack narrowly avoided."),
                <= 3 => new ResultInterpretation(true, "Dodged", "Attack cleanly avoided."),
                <= 5 => new ResultInterpretation(true, "Evasion", "Graceful avoidance, possible counter opportunity."),
                _ => new ResultInterpretation(true, "Perfect Defense", "Masterful defense, counter opportunity.")
            };
        }
    }

    private static ResultInterpretation GetSocialResult(int sv)
    {
        if (sv < 0)
        {
            return sv switch
            {
                >= -2 => new ResultInterpretation(false, "Unconvinced", "Target not swayed but not hostile."),
                >= -4 => new ResultInterpretation(false, "Rejected", "Target firmly disagrees."),
                >= -6 => new ResultInterpretation(false, "Offended", "Target becomes less cooperative."),
                _ => new ResultInterpretation(false, "Hostile", "Target becomes actively opposed.")
            };
        }
        else
        {
            return sv switch
            {
                <= 1 => new ResultInterpretation(true, "Slightly Influenced", "Target gives minimal cooperation."),
                <= 3 => new ResultInterpretation(true, "Convinced", "Target agrees or cooperates."),
                <= 5 => new ResultInterpretation(true, "Won Over", "Target is favorably disposed."),
                <= 7 => new ResultInterpretation(true, "Impressed", "Target becomes an ally or supporter."),
                _ => new ResultInterpretation(true, "Charmed", "Target is enthusiastically cooperative.")
            };
        }
    }

    private static ResultInterpretation GetPerceptionResult(int sv)
    {
        if (sv < 0)
        {
            return sv switch
            {
                >= -2 => new ResultInterpretation(false, "Nothing Noticed", "Fail to perceive anything useful."),
                >= -4 => new ResultInterpretation(false, "Missed", "Clearly miss important details."),
                _ => new ResultInterpretation(false, "Oblivious", "Completely unaware, may be surprised.")
            };
        }
        else
        {
            return sv switch
            {
                <= 1 => new ResultInterpretation(true, "Glimpse", "Notice something is there."),
                <= 3 => new ResultInterpretation(true, "Noticed", "Clear awareness of the target."),
                <= 5 => new ResultInterpretation(true, "Detailed", "Notice specific details."),
                <= 7 => new ResultInterpretation(true, "Thorough", "Complete understanding of the scene."),
                _ => new ResultInterpretation(true, "Perfect Awareness", "Notice hidden details others would miss.")
            };
        }
    }

    private static ResultInterpretation GetCraftingResult(int sv)
    {
        if (sv < 0)
        {
            return sv switch
            {
                >= -2 => new ResultInterpretation(false, "Flawed", "Item has minor defects, -1 quality."),
                >= -4 => new ResultInterpretation(false, "Poor", "Item has significant issues, -2 quality."),
                >= -6 => new ResultInterpretation(false, "Failed", "Item is unusable, materials may be salvaged."),
                _ => new ResultInterpretation(false, "Ruined", "Item and materials are destroyed.")
            };
        }
        else
        {
            return sv switch
            {
                <= 1 => new ResultInterpretation(true, "Serviceable", "Basic item, no bonuses.")
                {
                    EffectValue = 0
                },
                <= 3 => new ResultInterpretation(true, "Standard", "Good quality item.")
                {
                    EffectValue = 0
                },
                <= 5 => new ResultInterpretation(true, "Fine", "Superior quality, +1 bonus.")
                {
                    EffectValue = 1
                },
                <= 7 => new ResultInterpretation(true, "Excellent", "Exceptional quality, +2 bonus.")
                {
                    EffectValue = 2
                },
                _ => new ResultInterpretation(true, "Masterwork", "Mastercraft quality, +3 bonus.")
                {
                    EffectValue = 3
                }
            };
        }
    }

    private static ResultInterpretation GetHealingResult(int sv)
    {
        if (sv < 0)
        {
            return sv switch
            {
                >= -2 => new ResultInterpretation(false, "No Effect", "Healing attempt fails."),
                >= -4 => new ResultInterpretation(false, "Wasted", "Materials used with no benefit."),
                _ => new ResultInterpretation(false, "Harmful", "Treatment causes 1 FAT damage.")
                {
                    EffectValue = -1
                }
            };
        }
        else
        {
            int healAmount = sv switch
            {
                <= 1 => 1,
                <= 3 => 2,
                <= 5 => 4,
                <= 7 => 6,
                _ => 8
            };

            return new ResultInterpretation(true, $"Healed {healAmount}", $"Restore {healAmount} FAT or VIT.")
            {
                EffectValue = healAmount
            };
        }
    }

    private static ResultInterpretation GetMovementResult(int sv)
    {
        // Movement result table maps SV to range achieved
        // Base range is determined by movement type (Sprint=3, FullRound=5)
        // SV modifies the achieved range
        // EffectValue = range modifier (added to base range, clamped to 0)
        
        if (sv < 0)
        {
            return sv switch
            {
                >= -2 => new ResultInterpretation(false, "Slowed", "Movement impeded, achieve partial distance.")
                {
                    EffectValue = -1 // Reduce range by 1
                },
                >= -4 => new ResultInterpretation(false, "Stumbled", "Lost footing, minimal movement.")
                {
                    EffectValue = -2 // Reduce range by 2
                },
                >= -6 => new ResultInterpretation(false, "Stopped", "Failed to move effectively.")
                {
                    EffectValue = -3 // Reduce range by 3
                },
                >= -8 => new ResultInterpretation(false, "Fell", "Fell down, no movement, must recover.")
                {
                    EffectValue = -99 // No movement, prone
                },
                _ => new ResultInterpretation(false, "Mishap", "Serious fall or collision, possible injury.")
                {
                    EffectValue = -99 // No movement, possible damage
                }
            };
        }
        else
        {
            return sv switch
            {
                <= 1 => new ResultInterpretation(true, "Moved", "Achieved base movement distance.")
                {
                    EffectValue = 0 // Base range
                },
                <= 3 => new ResultInterpretation(true, "Quick", "Efficient movement, full distance.")
                {
                    EffectValue = 0 // Base range (no bonus for safety)
                },
                <= 5 => new ResultInterpretation(true, "Swift", "Excellent movement, bonus distance possible.")
                {
                    EffectValue = 1 // +1 range bonus
                },
                <= 7 => new ResultInterpretation(true, "Burst", "Exceptional speed burst.")
                {
                    EffectValue = 1 // +1 range bonus
                },
                _ => new ResultInterpretation(true, "Blazing", "Maximum possible speed achieved.")
                {
                    EffectValue = 2 // +2 range bonus
                }
            };
        }
    }

    private static ResultInterpretation GetManaRecoveryResult(int sv)
    {
        // Mana recovery result table maps SV to mana recovered.
        // TV is 6 by default. Each mana recovered takes 1 minute.
        // EffectValue = mana recovered
        
        if (sv < 0)
        {
            return sv switch
            {
                >= -2 => new ResultInterpretation(false, "No Recovery", "Failed to focus, no mana recovered.")
                {
                    EffectValue = 0
                },
                >= -4 => new ResultInterpretation(false, "Distracted", "Concentration broken, no mana recovered.")
                {
                    EffectValue = 0
                },
                >= -6 => new ResultInterpretation(false, "Exhausted", "Recovery attempt drains 1 FAT.")
                {
                    EffectValue = -1 // Lose 1 FAT
                },
                _ => new ResultInterpretation(false, "Backlash", "Magical backlash, lose 1 mana from pool.")
                {
                    EffectValue = -2 // Lose 1 mana
                }
            };
        }
        else
        {
            int manaRecovered = sv switch
            {
                <= 1 => 1,
                <= 3 => 2,
                <= 5 => 3,
                <= 7 => 4,
                _ => 5
            };

            return new ResultInterpretation(true, $"Recovered {manaRecovered}", $"Recover {manaRecovered} mana (1 minute per mana).")
            {
                EffectValue = manaRecovered
            };
        }
    }
}

/// <summary>
/// Interpretation of an action result based on SV and table type.
/// </summary>
public class ResultInterpretation
{
    /// <summary>
    /// Whether the action succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Short description of the result (e.g., "Good Success", "Miss").
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Longer description of what happens.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Numeric effect value (damage, healing, quality bonus, etc.)
    /// </summary>
    public int EffectValue { get; set; }

    /// <summary>
    /// Damage multiplier for combat (usually 1).
    /// </summary>
    public int DamageMultiplier { get; set; } = 0;

    public ResultInterpretation(bool isSuccess, string label, string description)
    {
        IsSuccess = isSuccess;
        Label = label;
        Description = description;
    }

    public override string ToString() => $"{Label}: {Description}";
}
