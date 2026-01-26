using System;
using Threa.Dal.Dto;

namespace GameMechanics.Items;

/// <summary>
/// Value object containing a character's equipped item and its template.
/// Used for bonus calculations and combat integration.
/// </summary>
public class EquippedItemInfo
{
    /// <summary>
    /// The character's item instance.
    /// </summary>
    public CharacterItem Item { get; }

    /// <summary>
    /// The item template with stat bonuses and properties.
    /// </summary>
    public ItemTemplate Template { get; }

    /// <summary>
    /// Creates a new EquippedItemInfo from a CharacterItem and its template.
    /// </summary>
    public EquippedItemInfo(CharacterItem item, ItemTemplate template)
    {
        Item = item ?? throw new ArgumentNullException(nameof(item));
        Template = template ?? throw new ArgumentNullException(nameof(template));
    }

    /// <summary>
    /// True if this item is a weapon (ItemType.Weapon).
    /// </summary>
    public bool IsWeapon => Template.ItemType == ItemType.Weapon;

    /// <summary>
    /// True if this item is armor (ItemType.Armor or ItemType.Shield).
    /// </summary>
    public bool IsArmor => Template.ItemType == ItemType.Armor || Template.ItemType == ItemType.Shield;

    /// <summary>
    /// True if this is a melee weapon (weapon with no Range defined).
    /// </summary>
    public bool IsMelee => Template.ItemType == ItemType.Weapon && !Template.Range.HasValue;

    /// <summary>
    /// True if this is a ranged weapon (weapon with Range defined).
    /// </summary>
    public bool IsRanged => Template.ItemType == ItemType.Weapon && Template.Range.HasValue;
}
