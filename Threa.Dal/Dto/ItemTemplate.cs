using System.Collections.Generic;

namespace Threa.Dal.Dto;

/// <summary>
/// Template/blueprint for items in the game world.
/// ItemTemplates define the properties that all instances of an item share.
/// </summary>
public class ItemTemplate
{
    /// <summary>
    /// Unique identifier for the item template.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Display name of the item.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Full description of the item.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Short description for inventory lists.
    /// </summary>
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>
    /// The category of item.
    /// </summary>
    public ItemType ItemType { get; set; }

    /// <summary>
    /// The type of weapon (if this is a weapon).
    /// </summary>
    public WeaponType WeaponType { get; set; }

    /// <summary>
    /// The slot(s) where this item can be equipped.
    /// </summary>
    public EquipmentSlot EquipmentSlot { get; set; }

    /// <summary>
    /// Weight of the item in pounds.
    /// </summary>
    public decimal Weight { get; set; }

    /// <summary>
    /// Volume of the item in cubic feet.
    /// </summary>
    public decimal Volume { get; set; }

    /// <summary>
    /// Base value of the item in copper pieces.
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Whether multiple items can stack in a single inventory slot.
    /// </summary>
    public bool IsStackable { get; set; }

    /// <summary>
    /// Maximum number of items that can be stacked.
    /// </summary>
    public int MaxStackSize { get; set; } = 1;

    /// <summary>
    /// Whether this item can contain other items.
    /// </summary>
    public bool IsContainer { get; set; }

    /// <summary>
    /// Maximum weight the container can hold (if IsContainer).
    /// </summary>
    public decimal? ContainerMaxWeight { get; set; }

    /// <summary>
    /// Maximum volume the container can hold (if IsContainer).
    /// </summary>
    public decimal? ContainerMaxVolume { get; set; }

    /// <summary>
    /// Comma-separated list of ItemTypes allowed in this container (if IsContainer).
    /// Null means any type is allowed.
    /// </summary>
    public string? ContainerAllowedTypes { get; set; }

    /// <summary>
    /// Weight reduction factor for magical containers (1.0 = normal, 0.1 = 90% reduction).
    /// </summary>
    public decimal ContainerWeightReduction { get; set; } = 1.0m;

    /// <summary>
    /// Whether this item has durability that can degrade.
    /// </summary>
    public bool HasDurability { get; set; }

    /// <summary>
    /// Maximum durability value (if HasDurability).
    /// </summary>
    public int? MaxDurability { get; set; }

    /// <summary>
    /// Rarity of the item.
    /// </summary>
    public ItemRarity Rarity { get; set; } = ItemRarity.Common;

    /// <summary>
    /// Whether this template is available for use.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Related skill name (for weapons, the skill used to attack).
    /// </summary>
    public string? RelatedSkill { get; set; }

    /// <summary>
    /// Minimum skill level required to use this item effectively.
    /// </summary>
    public int MinSkillLevel { get; set; }

    /// <summary>
    /// Damage class for weapons (1-4).
    /// </summary>
    public int DamageClass { get; set; }

    /// <summary>
    /// Damage type (Cutting, Piercing, Blunt).
    /// </summary>
    public string? DamageType { get; set; }

    /// <summary>
    /// Modifier to Success Value for weapons.
    /// </summary>
    public int SVModifier { get; set; }

    /// <summary>
    /// Modifier to Attack Value for weapons.
    /// </summary>
    public int AVModifier { get; set; }

    /// <summary>
    /// Modifier to Dodge for armor.
    /// </summary>
    public int DodgeModifier { get; set; }

    /// <summary>
    /// Range in feet for ranged weapons.
    /// </summary>
    public int? Range { get; set; }

    /// <summary>
    /// Absorption values per damage type for armor (JSON).
    /// </summary>
    public string? ArmorAbsorption { get; set; }

    /// <summary>
    /// Additional properties stored as JSON.
    /// </summary>
    public string? CustomProperties { get; set; }

    /// <summary>
    /// Skill bonuses provided by this item when equipped.
    /// </summary>
    public List<ItemSkillBonus> SkillBonuses { get; set; } = [];

    /// <summary>
    /// Attribute modifiers provided by this item when equipped.
    /// </summary>
    public List<ItemAttributeModifier> AttributeModifiers { get; set; } = [];
}
