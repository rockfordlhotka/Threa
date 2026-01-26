using System;
using System.Collections.Generic;
using System.Linq;
using Threa.Dal.Dto;

namespace GameMechanics.Items;

/// <summary>
/// Calculates total bonuses from equipped items for attributes and skills.
/// This class is stateless and can be used for pure calculations.
/// </summary>
public class ItemBonusCalculator
{
    /// <summary>
    /// Calculates total attribute modifier from equipped items.
    /// Per CONTEXT.md: Flat bonuses only, all bonuses stack additively.
    /// </summary>
    /// <param name="equippedItems">The equipped items to calculate bonuses from.</param>
    /// <param name="attributeName">The attribute name (STR, DEX, etc.).</param>
    /// <returns>Total attribute bonus from all equipped items.</returns>
    public int GetAttributeBonus(
        IEnumerable<EquippedItemInfo> equippedItems,
        string attributeName)
    {
        if (equippedItems == null)
            return 0;

        int total = 0;

        foreach (var item in equippedItems)
        {
            // Skip items with null template
            if (item.Template == null)
                continue;

            // Filter attribute modifiers by name (case-insensitive) and FlatBonus type only
            var modifiers = item.Template.AttributeModifiers
                .Where(m => m.AttributeName.Equals(attributeName, StringComparison.OrdinalIgnoreCase) &&
                           m.ModifierType == BonusType.FlatBonus);

            foreach (var modifier in modifiers)
            {
                total += (int)modifier.ModifierValue;
            }
        }

        return total;
    }

    /// <summary>
    /// Calculates total skill bonus from equipped items.
    /// Per CONTEXT.md: Flat bonuses only, all bonuses stack additively.
    /// </summary>
    /// <param name="equippedItems">The equipped items to calculate bonuses from.</param>
    /// <param name="skillName">The skill name to get bonus for.</param>
    /// <returns>Total skill bonus from all equipped items.</returns>
    public int GetSkillBonus(
        IEnumerable<EquippedItemInfo> equippedItems,
        string skillName)
    {
        if (equippedItems == null)
            return 0;

        int total = 0;

        foreach (var item in equippedItems)
        {
            // Skip items with null template
            if (item.Template == null)
                continue;

            // Filter skill bonuses by name (case-insensitive) and FlatBonus type only
            var bonuses = item.Template.SkillBonuses
                .Where(b => b.SkillName.Equals(skillName, StringComparison.OrdinalIgnoreCase) &&
                           b.BonusType == BonusType.FlatBonus);

            foreach (var bonus in bonuses)
            {
                total += (int)bonus.BonusValue;
            }
        }

        return total;
    }

    /// <summary>
    /// Gets itemized breakdown of attribute bonuses from each item.
    /// </summary>
    /// <param name="equippedItems">The equipped items to get breakdown from.</param>
    /// <param name="attributeName">The attribute name (STR, DEX, etc.).</param>
    /// <returns>List of (ItemName, Bonus) tuples for each contributing item.</returns>
    public List<(string ItemName, int Bonus)> GetAttributeBonusBreakdown(
        IEnumerable<EquippedItemInfo> equippedItems,
        string attributeName)
    {
        var breakdown = new List<(string ItemName, int Bonus)>();

        if (equippedItems == null)
            return breakdown;

        foreach (var item in equippedItems)
        {
            // Skip items with null template
            if (item.Template == null)
                continue;

            // Sum all flat modifiers for this attribute from this item
            var itemBonus = item.Template.AttributeModifiers
                .Where(m => m.AttributeName.Equals(attributeName, StringComparison.OrdinalIgnoreCase) &&
                           m.ModifierType == BonusType.FlatBonus)
                .Sum(m => (int)m.ModifierValue);

            if (itemBonus != 0)
            {
                breakdown.Add((item.Template.Name, itemBonus));
            }
        }

        return breakdown;
    }

    /// <summary>
    /// Gets itemized breakdown of skill bonuses from each item.
    /// </summary>
    /// <param name="equippedItems">The equipped items to get breakdown from.</param>
    /// <param name="skillName">The skill name to get breakdown for.</param>
    /// <returns>List of (ItemName, Bonus) tuples for each contributing item.</returns>
    public List<(string ItemName, int Bonus)> GetSkillBonusBreakdown(
        IEnumerable<EquippedItemInfo> equippedItems,
        string skillName)
    {
        var breakdown = new List<(string ItemName, int Bonus)>();

        if (equippedItems == null)
            return breakdown;

        foreach (var item in equippedItems)
        {
            // Skip items with null template
            if (item.Template == null)
                continue;

            // Sum all flat bonuses for this skill from this item
            var itemBonus = item.Template.SkillBonuses
                .Where(b => b.SkillName.Equals(skillName, StringComparison.OrdinalIgnoreCase) &&
                           b.BonusType == BonusType.FlatBonus)
                .Sum(b => (int)b.BonusValue);

            if (itemBonus != 0)
            {
                breakdown.Add((item.Template.Name, itemBonus));
            }
        }

        return breakdown;
    }
}
