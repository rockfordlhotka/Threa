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

    /// <summary>
    /// Gets available unarmed (virtual) weapon templates based on the character's equipment state.
    /// Per design: Punches require empty hand (no MainHand/TwoHand weapon), kicks are always available.
    /// </summary>
    /// <param name="virtualWeaponTemplates">All virtual weapon templates from the database.</param>
    /// <param name="equippedItems">The character's currently equipped items.</param>
    /// <returns>Virtual weapon templates available for the character's current equipment state.</returns>
    public static IEnumerable<ItemTemplate> GetAvailableUnarmedWeapons(
        IEnumerable<ItemTemplate> virtualWeaponTemplates,
        IEnumerable<EquippedItemInfo> equippedItems)
    {
        var equipped = equippedItems.ToList();
        bool hasMainHandWeapon = equipped.Any(i =>
            i.Template.ItemType == ItemType.Weapon &&
            (i.Item.EquippedSlot == EquipmentSlot.MainHand || i.Item.EquippedSlot == EquipmentSlot.TwoHand));

        foreach (var template in virtualWeaponTemplates)
        {
            if (template.ItemType != ItemType.Weapon)
                continue;
            if (!template.IsVirtual && template.WeaponType != WeaponType.Unarmed)
                continue;
            yield return template;
        }
    }

    /// <summary>
    /// Checks whether any hand weapon slots (MainHand/OffHand/TwoHand) have a weapon equipped.
    /// </summary>
    public static bool HasMeleeWeaponEquipped(IEnumerable<EquippedItemInfo> equippedItems)
    {
        return GetMeleeWeapons(equippedItems).Any();
    }

    private static bool IsWeaponSlot(EquipmentSlot slot) =>
        slot == EquipmentSlot.MainHand ||
        slot == EquipmentSlot.OffHand ||
        slot == EquipmentSlot.TwoHand;

    private static bool IsImplantWeaponSlot(EquipmentSlot slot) =>
        slot.IsImplant();
}
