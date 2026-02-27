using System;
using System.Collections.Generic;

namespace Threa.Dal.Dto;

/// <summary>
/// Represents an actual instance of an item owned by a character.
/// Items are created from ItemTemplates and can have instance-specific data.
/// </summary>
public class CharacterItem
{
    /// <summary>
    /// Unique identifier for this item instance.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The template this item is based on.
    /// </summary>
    public int ItemTemplateId { get; set; }

    /// <summary>
    /// The character who owns this item.
    /// </summary>
    public int OwnerCharacterId { get; set; }

    /// <summary>
    /// If this item is inside a container, the ID of the container item.
    /// </summary>
    public Guid? ContainerItemId { get; set; }

    /// <summary>
    /// The primary slot where this item is equipped (legacy single-slot field, kept for backward compatibility).
    /// Canonical slot data is in <see cref="EquippedSlots"/>.
    /// </summary>
    public EquipmentSlot EquippedSlot { get; set; } = EquipmentSlot.None;

    /// <summary>
    /// All slots currently occupied by this item (canonical).
    /// When empty and IsEquipped, the DAL normalizes this to [EquippedSlot].
    /// </summary>
    public List<EquipmentSlot> EquippedSlots { get; set; } = [];

    /// <summary>
    /// Whether this item is currently equipped.
    /// </summary>
    public bool IsEquipped { get; set; }

    /// <summary>
    /// Number of items in this stack (for stackable items).
    /// </summary>
    public int StackSize { get; set; } = 1;

    /// <summary>
    /// Current durability (if the template has durability).
    /// </summary>
    public int? CurrentDurability { get; set; }

    /// <summary>
    /// Custom name given to this specific item (e.g., "Grandfather's Sword").
    /// </summary>
    public string? CustomName { get; set; }

    /// <summary>
    /// When this item instance was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional properties stored as JSON.
    /// </summary>
    public string? CustomProperties { get; set; }

    /// <summary>
    /// Reference to the item template (populated when loading).
    /// </summary>
    public ItemTemplate? Template { get; set; }
}
