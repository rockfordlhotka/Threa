using System;
using System.Text;

namespace GameMechanics.Items;

/// <summary>
/// Breakdown of an attribute's total value showing base, item, and effect contributions.
/// </summary>
public class AttributeBonusBreakdown
{
    /// <summary>
    /// The attribute name (STR, DEX, etc.).
    /// </summary>
    public string AttributeName { get; set; } = string.Empty;

    /// <summary>
    /// The character's base value for this attribute.
    /// </summary>
    public int BaseValue { get; set; }

    /// <summary>
    /// Total bonus from equipped items.
    /// </summary>
    public int ItemBonus { get; set; }

    /// <summary>
    /// Total bonus from active effects (spells, wounds, etc.).
    /// </summary>
    public int EffectBonus { get; set; }

    /// <summary>
    /// Total effective value (BaseValue + ItemBonus + EffectBonus).
    /// </summary>
    public int Total => BaseValue + ItemBonus + EffectBonus;

    /// <summary>
    /// Formatted string showing breakdown of the attribute value.
    /// Example: "STR 12 (10 base + 2 items)" or "DEX 8 (10 base - 2 effects)"
    /// Only shows non-zero components.
    /// </summary>
    public string FormattedString
    {
        get
        {
            var sb = new StringBuilder();
            sb.Append($"{AttributeName} {Total} ({BaseValue} base");

            if (ItemBonus != 0)
            {
                if (ItemBonus > 0)
                    sb.Append($" + {ItemBonus} items");
                else
                    sb.Append($" - {Math.Abs(ItemBonus)} items");
            }

            if (EffectBonus != 0)
            {
                if (EffectBonus > 0)
                    sb.Append($" + {EffectBonus} effects");
                else
                    sb.Append($" - {Math.Abs(EffectBonus)} effects");
            }

            sb.Append(')');
            return sb.ToString();
        }
    }
}
