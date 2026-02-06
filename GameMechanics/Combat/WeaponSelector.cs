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
    /// Melee = weapon in MainHand/OffHand/TwoHand with no Range property AND not flagged as ranged in CustomProperties.
    /// Also includes implant weapons that are melee.
    /// </summary>
    public static IEnumerable<EquippedItemInfo> GetMeleeWeapons(
        IEnumerable<EquippedItemInfo> equippedItems)
    {
        return equippedItems.Where(i =>
            (i.Template.ItemType == ItemType.Weapon && IsWeaponSlot(i.Item.EquippedSlot) && !IsRangedWeapon(i)) ||
            (i.Template.ItemType == ItemType.Implant && IsImplantWeaponSlot(i.Item.EquippedSlot) &&
             i.Template.WeaponType != WeaponType.None && !IsRangedWeapon(i)));
    }

    /// <summary>
    /// Gets ranged weapons from equipped items.
    /// Ranged = weapon in MainHand/OffHand/TwoHand with Range property OR RangedWeaponProperties.IsRangedWeapon == true.
    /// Also includes implant weapons that are ranged.
    /// </summary>
    public static IEnumerable<EquippedItemInfo> GetRangedWeapons(
        IEnumerable<EquippedItemInfo> equippedItems)
    {
        return equippedItems.Where(i =>
            (i.Template.ItemType == ItemType.Weapon && IsWeaponSlot(i.Item.EquippedSlot) && IsRangedWeapon(i)) ||
            (i.Template.ItemType == ItemType.Implant && IsImplantWeaponSlot(i.Item.EquippedSlot) &&
             i.Template.WeaponType != WeaponType.None && IsRangedWeapon(i)));
    }

    /// <summary>
    /// Checks if a weapon is ranged by looking at both Range property and RangedWeaponProperties in CustomProperties.
    /// </summary>
    private static bool IsRangedWeapon(EquippedItemInfo item)
    {
        // Check simple Range property
        if (item.Template.Range.HasValue)
            return true;

        // Check advanced RangedWeaponProperties in CustomProperties JSON
        var rangedProps = RangedWeaponProperties.FromJson(item.Template.CustomProperties);
        return rangedProps?.IsRangedWeapon == true;
    }

    private static bool IsWeaponSlot(EquipmentSlot slot) =>
        slot == EquipmentSlot.MainHand ||
        slot == EquipmentSlot.OffHand ||
        slot == EquipmentSlot.TwoHand;

    private static bool IsImplantWeaponSlot(EquipmentSlot slot) =>
        slot.IsImplant();
}
