using System;
using System.Collections.Generic;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb;

public static class MockDb
{
    public static readonly List<Player> Players =
    [
      new Player { Id = 42, Name = "Rocky", Email = "rocky@lhotka.net" }
    ];

    public static readonly List<string> Images = [];

    /// <summary>
    /// Item template reference data - blueprints for all items in the game world.
    /// </summary>
    public static readonly List<ItemTemplate> ItemTemplates = CreateItemTemplates();

    /// <summary>
    /// Character item instances - actual items owned by characters.
    /// </summary>
    public static readonly List<CharacterItem> CharacterItems = CreateCharacterItems();

    private static List<ItemTemplate> CreateItemTemplates()
    {
        return
        [
            // === RAW MATERIALS ===
            new ItemTemplate
            {
                Id = 1,
                Name = "Iron Ore",
                Description = "Raw iron ore, suitable for smithing.",
                ShortDescription = "Raw iron for smithing",
                ItemType = ItemType.RawMaterial,
                Weight = 5m,
                Volume = 0.1m,
                Value = 2,
                IsStackable = true,
                MaxStackSize = 100,
                Rarity = ItemRarity.Common
            },
            new ItemTemplate
            {
                Id = 2,
                Name = "Leather Hide",
                Description = "An untanned animal hide.",
                ShortDescription = "Untanned hide",
                ItemType = ItemType.RawMaterial,
                Weight = 2m,
                Volume = 0.2m,
                Value = 3,
                IsStackable = true,
                MaxStackSize = 50,
                Rarity = ItemRarity.Common
            },

            // === WEAPONS ===
            new ItemTemplate
            {
                Id = 10,
                Name = "Longsword",
                Description = "A well-crafted longsword with a keen edge.",
                ShortDescription = "Standard longsword",
                ItemType = ItemType.Weapon,
                WeaponType = WeaponType.Sword,
                EquipmentSlot = EquipmentSlot.MainHand,
                Weight = 4m,
                Volume = 0.2m,
                Value = 160,
                HasDurability = true,
                MaxDurability = 100,
                RelatedSkill = "Swords",
                MinSkillLevel = 0,
                DamageClass = 2,
                DamageType = "Cutting",
                Rarity = ItemRarity.Common
            },
            new ItemTemplate
            {
                Id = 11,
                Name = "Enchanted Longsword",
                Description = "A magical longsword that glows faintly blue. It enhances the wielder's swordsmanship.",
                ShortDescription = "Magic sword +2",
                ItemType = ItemType.Weapon,
                WeaponType = WeaponType.Sword,
                EquipmentSlot = EquipmentSlot.MainHand,
                Weight = 4m,
                Volume = 0.2m,
                Value = 10000,
                HasDurability = true,
                MaxDurability = 150,
                RelatedSkill = "Swords",
                MinSkillLevel = 3,
                DamageClass = 2,
                DamageType = "Cutting",
                Rarity = ItemRarity.Rare,
                SkillBonuses =
                [
                    new ItemSkillBonus
                    {
                        Id = 1,
                        ItemTemplateId = 11,
                        SkillName = "Swords",
                        BonusType = BonusType.FlatBonus,
                        BonusValue = 2
                    }
                ]
            },
            new ItemTemplate
            {
                Id = 12,
                Name = "Battle Axe",
                Description = "A heavy battle axe capable of devastating blows.",
                ShortDescription = "Two-handed axe",
                ItemType = ItemType.Weapon,
                WeaponType = WeaponType.Axe,
                EquipmentSlot = EquipmentSlot.TwoHand,
                Weight = 8m,
                Volume = 0.4m,
                Value = 200,
                HasDurability = true,
                MaxDurability = 120,
                RelatedSkill = "Axes",
                MinSkillLevel = 2,
                DamageClass = 3,
                DamageType = "Cutting",
                Rarity = ItemRarity.Uncommon
            },
            new ItemTemplate
            {
                Id = 13,
                Name = "Dagger",
                Description = "A small, easily concealed blade.",
                ShortDescription = "Small blade",
                ItemType = ItemType.Weapon,
                WeaponType = WeaponType.Dagger,
                EquipmentSlot = EquipmentSlot.MainHand,
                Weight = 1m,
                Volume = 0.05m,
                Value = 40,
                HasDurability = true,
                MaxDurability = 60,
                RelatedSkill = "Daggers",
                MinSkillLevel = 0,
                DamageClass = 1,
                DamageType = "Piercing",
                Rarity = ItemRarity.Common
            },
            new ItemTemplate
            {
                Id = 14,
                Name = "Shortbow",
                Description = "A compact bow suitable for hunting or skirmishing.",
                ShortDescription = "Compact bow",
                ItemType = ItemType.Weapon,
                WeaponType = WeaponType.Bow,
                EquipmentSlot = EquipmentSlot.TwoHand,
                Weight = 2m,
                Volume = 0.3m,
                Value = 100,
                HasDurability = true,
                MaxDurability = 80,
                RelatedSkill = "Bows",
                MinSkillLevel = 0,
                DamageClass = 2,
                DamageType = "Piercing",
                Range = 100,
                Rarity = ItemRarity.Common
            },

            // === ARMOR ===
            new ItemTemplate
            {
                Id = 20,
                Name = "Leather Armor",
                Description = "Light armor made of hardened leather. Provides basic protection without restricting movement.",
                ShortDescription = "Light leather armor",
                ItemType = ItemType.Armor,
                EquipmentSlot = EquipmentSlot.Chest,
                Weight = 10m,
                Volume = 0.5m,
                Value = 100,
                HasDurability = true,
                MaxDurability = 80,
                DodgeModifier = -1,
                ArmorAbsorption = "{\"Cutting\": 2, \"Piercing\": 1, \"Blunt\": 1}",
                Rarity = ItemRarity.Common
            },
            new ItemTemplate
            {
                Id = 21,
                Name = "Chain Mail",
                Description = "Armor made of interlocking metal rings. Good protection but somewhat heavy.",
                ShortDescription = "Chain mail armor",
                ItemType = ItemType.Armor,
                EquipmentSlot = EquipmentSlot.Chest,
                Weight = 25m,
                Volume = 0.8m,
                Value = 400,
                HasDurability = true,
                MaxDurability = 150,
                DodgeModifier = -3,
                ArmorAbsorption = "{\"Cutting\": 4, \"Piercing\": 3, \"Blunt\": 2}",
                Rarity = ItemRarity.Uncommon
            },
            new ItemTemplate
            {
                Id = 22,
                Name = "Steel Helmet",
                Description = "A sturdy steel helmet that protects the head.",
                ShortDescription = "Steel helm",
                ItemType = ItemType.Armor,
                EquipmentSlot = EquipmentSlot.Head,
                Weight = 4m,
                Volume = 0.2m,
                Value = 80,
                HasDurability = true,
                MaxDurability = 100,
                DodgeModifier = 0,
                ArmorAbsorption = "{\"Cutting\": 3, \"Piercing\": 3, \"Blunt\": 2}",
                Rarity = ItemRarity.Common
            },
            new ItemTemplate
            {
                Id = 23,
                Name = "Wooden Shield",
                Description = "A basic wooden shield reinforced with iron bands.",
                ShortDescription = "Wooden shield",
                ItemType = ItemType.Armor,
                EquipmentSlot = EquipmentSlot.OffHand,
                Weight = 6m,
                Volume = 0.3m,
                Value = 100,
                HasDurability = true,
                MaxDurability = 60,
                Rarity = ItemRarity.Common
            },
            new ItemTemplate
            {
                Id = 24,
                Name = "Leather Boots",
                Description = "Sturdy leather boots suitable for travel.",
                ShortDescription = "Leather boots",
                ItemType = ItemType.Armor,
                EquipmentSlot = EquipmentSlot.FootLeft,
                Weight = 2m,
                Volume = 0.15m,
                Value = 40,
                HasDurability = true,
                MaxDurability = 60,
                Rarity = ItemRarity.Common
            },

            // === CONTAINERS ===
            new ItemTemplate
            {
                Id = 30,
                Name = "Large Backpack",
                Description = "A sturdy canvas backpack with multiple compartments.",
                ShortDescription = "Large backpack",
                ItemType = ItemType.Container,
                EquipmentSlot = EquipmentSlot.Back,
                Weight = 5m,
                Volume = 0.5m,
                Value = 50,
                IsContainer = true,
                ContainerMaxWeight = 50m,
                ContainerMaxVolume = 10m,
                Rarity = ItemRarity.Common
            },
            new ItemTemplate
            {
                Id = 31,
                Name = "Belt Pouch",
                Description = "A small leather pouch that attaches to a belt.",
                ShortDescription = "Small pouch",
                ItemType = ItemType.Container,
                EquipmentSlot = EquipmentSlot.Waist,
                Weight = 0.3m,
                Volume = 0.05m,
                Value = 10,
                IsContainer = true,
                ContainerMaxWeight = 3m,
                ContainerMaxVolume = 0.5m,
                Rarity = ItemRarity.Common
            },
            new ItemTemplate
            {
                Id = 32,
                Name = "Quiver",
                Description = "A leather quiver for arrows or bolts.",
                ShortDescription = "Arrow quiver",
                ItemType = ItemType.Container,
                EquipmentSlot = EquipmentSlot.Back,
                Weight = 1m,
                Volume = 0.1m,
                Value = 20,
                IsContainer = true,
                ContainerMaxWeight = 5m,
                ContainerMaxVolume = 0.5m,
                ContainerAllowedTypes = "Arrow,Bolt",
                Rarity = ItemRarity.Common
            },
            new ItemTemplate
            {
                Id = 33,
                Name = "Bag of Holding",
                Description = "A magical bag that reduces the weight of its contents. The interior is larger than it appears.",
                ShortDescription = "Magic bag",
                ItemType = ItemType.Container,
                Weight = 1m,
                Volume = 0.2m,
                Value = 25000,
                IsContainer = true,
                ContainerMaxWeight = 100m,
                ContainerMaxVolume = 20m,
                ContainerWeightReduction = 0.1m,
                Rarity = ItemRarity.Epic
            },

            // === JEWELRY WITH BONUSES ===
            new ItemTemplate
            {
                Id = 40,
                Name = "Ring of Strength",
                Description = "A magical ring set with a red gem. It enhances the wearer's physical power.",
                ShortDescription = "STR +1 ring",
                ItemType = ItemType.Jewelry,
                EquipmentSlot = EquipmentSlot.FingerLeft1,
                Weight = 0.05m,
                Volume = 0.001m,
                Value = 8000,
                Rarity = ItemRarity.Rare,
                AttributeModifiers =
                [
                    new ItemAttributeModifier
                    {
                        Id = 1,
                        ItemTemplateId = 40,
                        AttributeName = "STR",
                        ModifierType = BonusType.FlatBonus,
                        ModifierValue = 1
                    }
                ]
            },
            new ItemTemplate
            {
                Id = 41,
                Name = "Belt of Giants",
                Description = "A wide leather belt adorned with runes. It grants great strength to the wearer.",
                ShortDescription = "STR +3 belt",
                ItemType = ItemType.Jewelry,
                EquipmentSlot = EquipmentSlot.Waist,
                Weight = 1m,
                Volume = 0.1m,
                Value = 30000,
                Rarity = ItemRarity.Epic,
                AttributeModifiers =
                [
                    new ItemAttributeModifier
                    {
                        Id = 2,
                        ItemTemplateId = 41,
                        AttributeName = "STR",
                        ModifierType = BonusType.FlatBonus,
                        ModifierValue = 3
                    }
                ]
            },
            new ItemTemplate
            {
                Id = 42,
                Name = "Gauntlets of Strength",
                Description = "Magical metal gauntlets that enhance the wearer's grip and striking power.",
                ShortDescription = "STR +2 gauntlets",
                ItemType = ItemType.Armor,
                EquipmentSlot = EquipmentSlot.HandLeft,
                Weight = 2m,
                Volume = 0.1m,
                Value = 15000,
                HasDurability = true,
                MaxDurability = 100,
                Rarity = ItemRarity.Rare,
                AttributeModifiers =
                [
                    new ItemAttributeModifier
                    {
                        Id = 3,
                        ItemTemplateId = 42,
                        AttributeName = "STR",
                        ModifierType = BonusType.FlatBonus,
                        ModifierValue = 2
                    }
                ]
            },
            new ItemTemplate
            {
                Id = 43,
                Name = "Amulet of Protection",
                Description = "A silver amulet inscribed with protective runes. It enhances the wearer's resilience.",
                ShortDescription = "END +2 amulet",
                ItemType = ItemType.Jewelry,
                EquipmentSlot = EquipmentSlot.Neck,
                Weight = 0.1m,
                Volume = 0.01m,
                Value = 12000,
                Rarity = ItemRarity.Rare,
                AttributeModifiers =
                [
                    new ItemAttributeModifier
                    {
                        Id = 4,
                        ItemTemplateId = 43,
                        AttributeName = "END",
                        ModifierType = BonusType.FlatBonus,
                        ModifierValue = 2
                    }
                ]
            },

            // === CONSUMABLES ===
            new ItemTemplate
            {
                Id = 50,
                Name = "Health Potion",
                Description = "A red potion that restores vitality when consumed.",
                ShortDescription = "Restores 5 VIT",
                ItemType = ItemType.Consumable,
                Weight = 0.5m,
                Volume = 0.05m,
                Value = 60,
                IsStackable = true,
                MaxStackSize = 20,
                Rarity = ItemRarity.Common
            },
            new ItemTemplate
            {
                Id = 51,
                Name = "Stamina Potion",
                Description = "A green potion that restores fatigue when consumed.",
                ShortDescription = "Restores 5 FAT",
                ItemType = ItemType.Consumable,
                Weight = 0.5m,
                Volume = 0.05m,
                Value = 40,
                IsStackable = true,
                MaxStackSize = 20,
                Rarity = ItemRarity.Common
            },

            // === TOOLS ===
            new ItemTemplate
            {
                Id = 60,
                Name = "Torch",
                Description = "A wooden torch that provides light in dark areas.",
                ShortDescription = "Light source",
                ItemType = ItemType.Tool,
                Weight = 1m,
                Volume = 0.1m,
                Value = 3,
                IsStackable = true,
                MaxStackSize = 10,
                HasDurability = true,
                MaxDurability = 60,
                Rarity = ItemRarity.Common
            },
            new ItemTemplate
            {
                Id = 61,
                Name = "Lockpicks",
                Description = "A set of fine metal tools for picking locks.",
                ShortDescription = "Lock picking tools",
                ItemType = ItemType.Tool,
                Weight = 0.2m,
                Volume = 0.02m,
                Value = 30,
                HasDurability = true,
                MaxDurability = 20,
                RelatedSkill = "Lockpicking",
                Rarity = ItemRarity.Uncommon
            },
            new ItemTemplate
            {
                Id = 62,
                Name = "Rope (50 ft)",
                Description = "A sturdy hemp rope, 50 feet in length.",
                ShortDescription = "50ft rope",
                ItemType = ItemType.Tool,
                Weight = 10m,
                Volume = 2m,
                Value = 20,
                Rarity = ItemRarity.Common
            },

            // === TREASURE ===
            new ItemTemplate
            {
                Id = 70,
                Name = "Gold Coins",
                Description = "Standard gold coins used as currency.",
                ShortDescription = "Gold pieces",
                ItemType = ItemType.Treasure,
                Weight = 0.02m,
                Volume = 0.0001m,
                Value = 400,
                IsStackable = true,
                MaxStackSize = 1000,
                Rarity = ItemRarity.Common
            },
            new ItemTemplate
            {
                Id = 71,
                Name = "Silver Coins",
                Description = "Standard silver coins used as currency.",
                ShortDescription = "Silver pieces",
                ItemType = ItemType.Treasure,
                Weight = 0.02m,
                Volume = 0.0001m,
                Value = 20,
                IsStackable = true,
                MaxStackSize = 1000,
                Rarity = ItemRarity.Common
            },
            new ItemTemplate
            {
                Id = 72,
                Name = "Copper Coins",
                Description = "Standard copper coins used as currency.",
                ShortDescription = "Copper pieces",
                ItemType = ItemType.Treasure,
                Weight = 0.02m,
                Volume = 0.0001m,
                Value = 1,
                IsStackable = true,
                MaxStackSize = 1000,
                Rarity = ItemRarity.Common
            },
            new ItemTemplate
            {
                Id = 73,
                Name = "Small Gemstone",
                Description = "An uncut gemstone of modest value.",
                ShortDescription = "Uncut gem",
                ItemType = ItemType.Treasure,
                Weight = 0.1m,
                Volume = 0.01m,
                Value = 15,
                IsStackable = true,
                MaxStackSize = 50,
                Rarity = ItemRarity.Uncommon
            },

            // === FOOD & DRINK ===
            new ItemTemplate
            {
                Id = 80,
                Name = "Trail Rations",
                Description = "Dried meat, hardtack, and nuts. Enough for one day.",
                ShortDescription = "One day's food",
                ItemType = ItemType.Food,
                Weight = 1m,
                Volume = 0.1m,
                Value = 5,
                IsStackable = true,
                MaxStackSize = 30,
                Rarity = ItemRarity.Common
            },
            new ItemTemplate
            {
                Id = 81,
                Name = "Waterskin",
                Description = "A leather waterskin that holds enough water for a day.",
                ShortDescription = "Water container",
                ItemType = ItemType.Drink,
                Weight = 4m,
                Volume = 0.2m,
                Value = 10,
                Rarity = ItemRarity.Common
            },

            // === CLOTHING ===
            new ItemTemplate
            {
                Id = 90,
                Name = "Traveler's Cloak",
                Description = "A hooded cloak suitable for travel in various weather.",
                ShortDescription = "Hooded cloak",
                ItemType = ItemType.Clothing,
                EquipmentSlot = EquipmentSlot.Back,
                Weight = 2m,
                Volume = 0.3m,
                Value = 20,
                Rarity = ItemRarity.Common
            },
            new ItemTemplate
            {
                Id = 91,
                Name = "Common Tunic",
                Description = "A simple cloth tunic.",
                ShortDescription = "Basic tunic",
                ItemType = ItemType.Clothing,
                EquipmentSlot = EquipmentSlot.Chest,
                Weight = 0.5m,
                Volume = 0.1m,
                Value = 5,
                Rarity = ItemRarity.Common
            }
        ];
    }

    private static List<CharacterItem> CreateCharacterItems()
    {
        // Give the sample character (Illiante) some starting equipment
        return
        [
            new CharacterItem
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                ItemTemplateId = 10, // Longsword
                OwnerCharacterId = 1,
                IsEquipped = true,
                EquippedSlot = EquipmentSlot.MainHand,
                CurrentDurability = 100
            },
            new CharacterItem
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                ItemTemplateId = 20, // Leather Armor
                OwnerCharacterId = 1,
                IsEquipped = true,
                EquippedSlot = EquipmentSlot.Chest,
                CurrentDurability = 80
            },
            new CharacterItem
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                ItemTemplateId = 30, // Large Backpack
                OwnerCharacterId = 1,
                IsEquipped = true,
                EquippedSlot = EquipmentSlot.Back
            },
            new CharacterItem
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                ItemTemplateId = 50, // Health Potion
                OwnerCharacterId = 1,
                ContainerItemId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                StackSize = 3
            },
            new CharacterItem
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                ItemTemplateId = 80, // Trail Rations
                OwnerCharacterId = 1,
                ContainerItemId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                StackSize = 5
            },
            new CharacterItem
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                ItemTemplateId = 71, // Silver Coins
                OwnerCharacterId = 1,
                ContainerItemId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                StackSize = 25
            }
        ];
    }

    /// <summary>
    /// Species reference data with attribute modifiers per design spec.
    /// </summary>
    public static readonly List<Species> Species =
    [
        new Species
        {
            Id = "Human",
            Name = "Human",
            Description = "Humans are the baseline species with no attribute modifiers.",
            AttributeModifiers = []
        },
        new Species
        {
            Id = "Elf",
            Name = "Elf",
            Description = "Elves are intellectual and agile, but physically delicate.",
            AttributeModifiers =
            [
                new SpeciesAttributeModifier { AttributeName = "INT", Modifier = 1 },
                new SpeciesAttributeModifier { AttributeName = "STR", Modifier = -1 }
            ]
        },
        new Species
        {
            Id = "Dwarf",
            Name = "Dwarf",
            Description = "Dwarves are strong and resilient, but less agile.",
            AttributeModifiers =
            [
                new SpeciesAttributeModifier { AttributeName = "STR", Modifier = 1 },
                new SpeciesAttributeModifier { AttributeName = "DEX", Modifier = -1 }
            ]
        },
        new Species
        {
            Id = "Halfling",
            Name = "Halfling",
            Description = "Halflings are quick and perceptive, but physically weak.",
            AttributeModifiers =
            [
                new SpeciesAttributeModifier { AttributeName = "DEX", Modifier = 1 },
                new SpeciesAttributeModifier { AttributeName = "ITT", Modifier = 1 },
                new SpeciesAttributeModifier { AttributeName = "STR", Modifier = -2 }
            ]
        },
        new Species
        {
            Id = "Orc",
            Name = "Orc",
            Description = "Orcs are physically powerful and enduring, but less intelligent and social.",
            AttributeModifiers =
            [
                new SpeciesAttributeModifier { AttributeName = "STR", Modifier = 2 },
                new SpeciesAttributeModifier { AttributeName = "END", Modifier = 1 },
                new SpeciesAttributeModifier { AttributeName = "INT", Modifier = -1 },
                new SpeciesAttributeModifier { AttributeName = "PHY", Modifier = -1 }
            ]
        }
    ];

    public static readonly List<Character> Characters =
    [
      new Character
      {
        Id = 1,
        PlayerId = 42,
        Name = "Illiante",
        Species = "Human",
        AttributeList = new List<CharacterAttribute>
        {
            new() { Name = "STR", BaseValue = 10 },
            new() { Name = "DEX", BaseValue = 10 },
            new() { Name = "END", BaseValue = 10 },
            new() { Name = "INT", BaseValue = 10 },
            new() { Name = "ITT", BaseValue = 10 },
            new() { Name = "WIS", BaseValue = 10 },
            new() { Name = "PHY", BaseValue = 10 },
            new() { Name = "SOC", BaseValue = 10 },
            new() { Name = "PSY", BaseValue = 10 }
        },
        FatBaseValue = 10,
        FatValue = 10,
        VitBaseValue = 10,
        VitValue = 10,
        ActionPointRecovery = 1,
        ActionPointMax = 5,
        ActionPointAvailable = 5,
        DamageClass = 1,
        Description = "A human with a sword",
        HairDescription = "Long and black",
        Height = "5' 6\"",
        SkinDescription = "Fair",
        Weight = "150 lbs"
      }
    ];
}
