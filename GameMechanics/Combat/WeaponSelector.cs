using System.Collections.Generic;
using System.Linq;
using GameMechanics.Items;
using Threa.Dal.Dto;

namespace GameMechanics.Combat;

/// <summary>
/// Filters equipped weapons by combat mode (melee/ranged).
/// Per CONTEXT.md: "Combat mode automatically filters to show only equipped weapons valid for that mode"
/// </summary>
public static class WeaponSelector
{
    /// <summary>
    /// Gets melee weapons from equipped items.
    /// Melee = weapon in MainHand/OffHand/TwoHand with no Range property.
    /// </summary>
    public static IEnumerable<EquippedItemInfo> GetMeleeWeapons(
        IEnumerable<EquippedItemInfo> equippedItems)
    {
        return equippedItems.Where(i =>
            i.Template.ItemType == ItemType.Weapon &&
            IsWeaponSlot(i.Item.EquippedSlot) &&
            !i.Template.Range.HasValue);
    }

    /// <summary>
    /// Gets ranged weapons from equipped items.
    /// Ranged = weapon in MainHand/OffHand/TwoHand with Range property.
    /// </summary>
    public static IEnumerable<EquippedItemInfo> GetRangedWeapons(
        IEnumerable<EquippedItemInfo> equippedItems)
    {
        return equippedItems.Where(i =>
            i.Template.ItemType == ItemType.Weapon &&
            IsWeaponSlot(i.Item.EquippedSlot) &&
            i.Template.Range.HasValue);
    }

    private static bool IsWeaponSlot(EquipmentSlot slot) =>
        slot == EquipmentSlot.MainHand ||
        slot == EquipmentSlot.OffHand ||
        slot == EquipmentSlot.TwoHand;
}
