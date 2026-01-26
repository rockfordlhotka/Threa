using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using GameMechanics.Combat;
using Threa.Dal.Dto;

namespace GameMechanics.Items;

/// <summary>
/// Creates ArmorInfo objects from equipped armor items.
/// </summary>
public static class ArmorInfoFactory
{
    /// <summary>
    /// Creates an ArmorInfo from an equipped armor item.
    /// </summary>
    public static ArmorInfo CreateArmorInfo(EquippedItemInfo item)
    {
        // Parse ArmorAbsorption JSON
        var absorption = new Dictionary<DamageType, int>();
        if (!string.IsNullOrEmpty(item.Template.ArmorAbsorption))
        {
            try
            {
                // ArmorAbsorption is stored as JSON like: {"Cutting":5,"Piercing":3}
                var parsed = JsonSerializer.Deserialize<Dictionary<string, int>>(item.Template.ArmorAbsorption);
                if (parsed != null)
                {
                    foreach (var kvp in parsed)
                    {
                        if (Enum.TryParse<DamageType>(kvp.Key, true, out var damageType))
                        {
                            absorption[damageType] = kvp.Value;
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // Invalid JSON - use empty absorption
            }
        }

        return new ArmorInfo
        {
            ItemId = item.Item.Id.ToString(),
            Name = item.Template.Name,
            CoveredLocations = EquipmentLocationMapper.GetCoveredLocations(item.Item.EquippedSlot),
            DamageClass = item.Template.DamageClass,
            Absorption = absorption,
            CurrentDurability = item.Item.CurrentDurability ?? item.Template.MaxDurability ?? 100,
            MaxDurability = item.Template.MaxDurability ?? 100,
            LayerOrder = GetLayerOrder(item.Item.EquippedSlot)
        };
    }

    /// <summary>
    /// Gets all armor pieces from equipped items.
    /// </summary>
    public static List<ArmorInfo> GetArmorPieces(IEnumerable<EquippedItemInfo> equippedItems)
    {
        return equippedItems
            .Where(i => i.Template.ItemType == ItemType.Armor)
            .Select(CreateArmorInfo)
            .ToList();
    }

    /// <summary>
    /// Gets armor pieces that cover a specific hit location.
    /// </summary>
    public static List<ArmorInfo> GetArmorForLocation(
        IEnumerable<EquippedItemInfo> equippedItems,
        HitLocation location)
    {
        return equippedItems
            .Where(i => i.Template.ItemType == ItemType.Armor)
            .Where(i => EquipmentLocationMapper.GetCoveredLocations(i.Item.EquippedSlot)
                .Contains(location))
            .Select(CreateArmorInfo)
            .OrderBy(a => a.LayerOrder)
            .ToList();
    }

    private static int GetLayerOrder(EquipmentSlot slot) => slot switch
    {
        // Outer layers (absorb first)
        EquipmentSlot.Back => 1,
        EquipmentSlot.Shoulders => 2,
        EquipmentSlot.Chest => 3,
        // Inner layers
        _ => 10
    };
}
