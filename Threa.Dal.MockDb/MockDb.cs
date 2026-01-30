using System;
using System.Collections.Generic;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb;

public static class MockDb
{
    public static readonly List<Player> Players =
    [
      new Player { Id = 42, Name = "Rocky", Email = "rocky@lhotka.net" },
      new Player { Id = 43, Name = "Illiante", Email = "illiante@yahoo.com" }
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

    /// <summary>
    /// Effect definition reference data - templates for all effects in the game.
    /// </summary>
    public static readonly List<EffectDefinition> EffectDefinitions = CreateEffectDefinitions();

    /// <summary>
    /// Active effects on characters.
    /// </summary>
    public static readonly List<CharacterEffect> CharacterEffects = [];

    /// <summary>
    /// Active effects on items.
    /// </summary>
    public static readonly List<ItemEffect> ItemEffects = [];

    /// <summary>
    /// Character mana pools per magic school.
    /// </summary>
    public static readonly List<CharacterMana> CharacterManaPools = [];

    /// <summary>
    /// Spell definitions - spell-specific metadata for spell skills.
    /// </summary>
    public static readonly List<SpellDefinition> SpellDefinitions = CreateSpellDefinitions();

    /// <summary>
    /// Spell locations for environmental effects.
    /// </summary>
    public static readonly List<SpellLocation> SpellLocations = [];

    /// <summary>
    /// Location-based spell effects.
    /// </summary>
    public static readonly List<LocationEffect> LocationEffects = [];

    /// <summary>
    /// Game tables (active sessions).
    /// </summary>
    public static readonly List<GameTable> Tables = CreateTables();

    /// <summary>
    /// Characters connected to tables.
    /// </summary>
    public static readonly List<TableCharacter> TableCharacters = CreateTableCharacters();

    /// <summary>
    /// NPCs at tables.
    /// </summary>
    public static readonly List<TableNpc> TableNpcs = [];

    /// <summary>
    /// Join requests from players wanting to join tables.
    /// </summary>
    public static readonly List<JoinRequest> JoinRequests = [];

    /// <summary>
    /// Skill definitions - all available skills in the game.
    /// </summary>
    public static readonly List<Skill> Skills = CreateSkills();

    /// <summary>
    /// Effect templates - reusable effect presets for the system-wide library.
    /// </summary>
    public static readonly List<EffectTemplateDto> EffectTemplates = CreateEffectTemplates();

    private static List<EffectTemplateDto> CreateEffectTemplates()
    {
        return
        [
            new EffectTemplateDto
            {
                Id = 1,
                Name = "Stunned",
                EffectType = EffectType.Condition,
                Description = "Unable to take actions, severely impaired.",
                IconName = "bi-lightning",
                Color = "#ffc107",
                DefaultDurationValue = 1,
                DurationType = DurationType.Rounds,
                StateJson = "{\"ASModifier\":-4,\"BehaviorTags\":[\"condition\",\"end-of-turn-remove\"]}",
                Tags = "combat,condition,debilitating",
                IsSystem = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new EffectTemplateDto
            {
                Id = 2,
                Name = "Blessed",
                EffectType = EffectType.Buff,
                Description = "Divine favor enhances all actions.",
                IconName = "bi-star-fill",
                Color = "#ffd700",
                DefaultDurationValue = 10,
                DurationType = DurationType.Rounds,
                StateJson = "{\"ASModifier\":2,\"BehaviorTags\":[\"modifier\",\"magic\"]}",
                Tags = "magic,buff,divine",
                IsSystem = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new EffectTemplateDto
            {
                Id = 3,
                Name = "Poisoned",
                EffectType = EffectType.Poison,
                Description = "Toxin causes ongoing damage.",
                IconName = "bi-droplet-fill",
                Color = "#28a745",
                DefaultDurationValue = 5,
                DurationType = DurationType.Rounds,
                StateJson = "{\"FatDamagePerTick\":2,\"VitDamagePerTick\":1,\"BehaviorTags\":[\"poison\",\"end-of-round-trigger\"]}",
                Tags = "poison,damage-over-time",
                IsSystem = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new EffectTemplateDto
            {
                Id = 4,
                Name = "Haste",
                EffectType = EffectType.Buff,
                Description = "Supernatural speed enhances reflexes.",
                IconName = "bi-speedometer2",
                Color = "#17a2b8",
                DefaultDurationValue = 3,
                DurationType = DurationType.Rounds,
                StateJson = "{\"AttributeModifiers\":{\"DEX\":2},\"BehaviorTags\":[\"modifier\",\"magic\"]}",
                Tags = "magic,buff,speed",
                IsSystem = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new EffectTemplateDto
            {
                Id = 5,
                Name = "Weakened",
                EffectType = EffectType.Debuff,
                Description = "Physical strength is sapped.",
                IconName = "bi-person-dash",
                Color = "#6c757d",
                DefaultDurationValue = 5,
                DurationType = DurationType.Rounds,
                StateJson = "{\"AttributeModifiers\":{\"STR\":-2},\"BehaviorTags\":[\"modifier\",\"debuff\"]}",
                Tags = "debuff,physical",
                IsSystem = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new EffectTemplateDto
            {
                Id = 6,
                Name = "Regenerating",
                EffectType = EffectType.Buff,
                Description = "Wounds heal over time.",
                IconName = "bi-heart-pulse",
                Color = "#dc3545",
                DefaultDurationValue = 10,
                DurationType = DurationType.Rounds,
                StateJson = "{\"FatHealingPerTick\":1,\"VitHealingPerTick\":1,\"BehaviorTags\":[\"healing\",\"end-of-round-trigger\"]}",
                Tags = "healing,regeneration,buff",
                IsSystem = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new EffectTemplateDto
            {
                Id = 7,
                Name = "Blinded",
                EffectType = EffectType.Condition,
                Description = "Cannot see, severely impacting combat and awareness.",
                IconName = "bi-eye-slash",
                Color = "#343a40",
                DefaultDurationValue = 3,
                DurationType = DurationType.Rounds,
                StateJson = "{\"ASModifier\":-6,\"BehaviorTags\":[\"condition\",\"sensory\"]}",
                Tags = "condition,sensory,debilitating",
                IsSystem = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            }
        ];
    }

    private static List<GameTable> CreateTables()
    {
        return
        [
            new GameTable
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Test Adventure",
                GameMasterId = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                LastActivityAt = DateTime.UtcNow.AddHours(-1),
                Status = TableStatus.Active,
                CurrentRound = 5,
                IsInCombat = false
            }
        ];
    }

    private static List<TableCharacter> CreateTableCharacters()
    {
        return
        [
            new TableCharacter
            {
                TableId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                CharacterId = 1, // First test character
                PlayerId = 1,
                JoinedAt = DateTime.UtcNow.AddDays(-7),
                ConnectionStatus = ConnectionStatus.Connected,
                LastActivity = DateTime.UtcNow.AddHours(-1)
            }
        ];
    }

    /// <summary>
    /// Magic school definitions - schools of magic available in the game world.
    /// </summary>
    public static readonly List<MagicSchoolDefinition> MagicSchools = CreateMagicSchools();

    private static List<MagicSchoolDefinition> CreateMagicSchools()
    {
        return
        [
            new MagicSchoolDefinition
            {
                Id = "fire",
                Name = "Fire",
                Description = "Fire magic channels the raw power of flame and heat. Practitioners can create fire, manipulate heat, and unleash devastating offensive spells. Fire magic is associated with passion, destruction, and transformation.",
                ShortDescription = "Offensive magic of flame and heat",
                ColorCode = "#FF4500",
                IsActive = true,
                IsCore = true,
                ManaSkillId = "fire-mana",
                DisplayOrder = 1,
                TypicalSpellTypes = "Offensive damage, area control, illumination through flame"
            },
            new MagicSchoolDefinition
            {
                Id = "water",
                Name = "Water",
                Description = "Water magic encompasses the fluid nature of water and ice. Practitioners can freeze, heal, purify, and manipulate the flow of water. Water magic embodies adaptability, healing, and cleansing.",
                ShortDescription = "Ice, healing, and purification",
                ColorCode = "#4169E1",
                IsActive = true,
                IsCore = true,
                ManaSkillId = "water-mana",
                DisplayOrder = 2,
                TypicalSpellTypes = "Healing, ice attacks, purification, water manipulation"
            },
            new MagicSchoolDefinition
            {
                Id = "light",
                Name = "Light",
                Description = "Light magic harnesses the power of illumination and truth. Practitioners can create light, reveal truth, protect against darkness, and banish evil. Light magic represents clarity, protection, and revelation.",
                ShortDescription = "Illumination, truth, and protection",
                ColorCode = "#FFD700",
                IsActive = true,
                IsCore = true,
                ManaSkillId = "light-mana",
                DisplayOrder = 3,
                TypicalSpellTypes = "Illumination, truth detection, protection, banishment"
            },
            new MagicSchoolDefinition
            {
                Id = "life",
                Name = "Life",
                Description = "Life magic taps into the vital force that animates all living things. Practitioners can heal wounds, promote growth, enhance vitality, and commune with nature. Life magic embodies healing, growth, and renewal.",
                ShortDescription = "Healing, growth, and vitality",
                ColorCode = "#32CD32",
                IsActive = true,
                IsCore = true,
                ManaSkillId = "life-mana",
                DisplayOrder = 4,
                TypicalSpellTypes = "Healing, growth enhancement, vitality buffs, nature communion"
            }
        ];
    }

    private static List<Skill> CreateSkills()
    {
        return
        [
            // === STANDARD ATTRIBUTE SKILLS ===
            new Skill
            {
                Id = "physicality",
                Name = "Physicality",
                Category = SkillCategory.Standard,
                IsSpecialized = false,
                Untrained = 3,
                Trained = 1,
                PrimaryAttribute = "STR"
            },
            new Skill
            {
                Id = "dodge",
                Name = "Dodge",
                Category = SkillCategory.Standard,
                IsSpecialized = false,
                Untrained = 6,
                Trained = 4,
                PrimaryAttribute = "DEX/ITT"
            },
            new Skill
            {
                Id = "drive",
                Name = "Drive",
                Category = SkillCategory.Standard,
                IsSpecialized = false,
                Untrained = 5,
                Trained = 3,
                PrimaryAttribute = "WIL/END"
            },
            new Skill
            {
                Id = "reasoning",
                Name = "Reasoning",
                Category = SkillCategory.Standard,
                IsSpecialized = false,
                Untrained = 5,
                Trained = 3,
                PrimaryAttribute = "INT"
            },
            new Skill
            {
                Id = "awareness",
                Name = "Awareness",
                Category = SkillCategory.Standard,
                IsSpecialized = false,
                Untrained = 5,
                Trained = 2,
                PrimaryAttribute = "ITT"
            },
            new Skill
            {
                Id = "focus",
                Name = "Focus",
                Category = SkillCategory.Standard,
                IsSpecialized = false,
                Untrained = 5,
                Trained = 2,
                PrimaryAttribute = "WIL"
            },
            new Skill
            {
                Id = "bearing",
                Name = "Bearing",
                Category = SkillCategory.Standard,
                IsSpecialized = false,
                Untrained = 4,
                Trained = 2,
                PrimaryAttribute = "SOC"
            },
            new Skill
            {
                Id = "influence",
                Name = "Influence",
                Category = SkillCategory.Standard,
                IsSpecialized = false,
                Untrained = 4,
                Trained = 2,
                PrimaryAttribute = "PHY"
            },

            // === WEAPON COMBAT SKILLS ===
            new Skill
            {
                Id = "swords",
                Name = "Swords",
                Category = SkillCategory.Combat,
                IsSpecialized = true,
                Untrained = 7,
                Trained = 4,
                PrimaryAttribute = "DEX"
            },
            new Skill
            {
                Id = "axes",
                Name = "Axes",
                Category = SkillCategory.Combat,
                IsSpecialized = true,
                Untrained = 7,
                Trained = 4,
                PrimaryAttribute = "STR"
            },
            new Skill
            {
                Id = "daggers",
                Name = "Daggers",
                Category = SkillCategory.Combat,
                IsSpecialized = true,
                Untrained = 6,
                Trained = 3,
                PrimaryAttribute = "DEX"
            },
            new Skill
            {
                Id = "bows",
                Name = "Bows",
                Category = SkillCategory.Combat,
                IsSpecialized = true,
                Untrained = 7,
                Trained = 4,
                PrimaryAttribute = "DEX"
            },
            new Skill
            {
                Id = "crossbows",
                Name = "Crossbows",
                Category = SkillCategory.Combat,
                IsSpecialized = true,
                Untrained = 6,
                Trained = 3,
                PrimaryAttribute = "DEX"
            },
            new Skill
            {
                Id = "pistols",
                Name = "Pistols",
                Category = SkillCategory.Combat,
                IsSpecialized = true,
                Untrained = 7,
                Trained = 4,
                PrimaryAttribute = "DEX"
            },
            new Skill
            {
                Id = "spears",
                Name = "Spears",
                Category = SkillCategory.Combat,
                IsSpecialized = true,
                Untrained = 6,
                Trained = 3,
                PrimaryAttribute = "DEX"
            },
            new Skill
            {
                Id = "polearms",
                Name = "Polearms",
                Category = SkillCategory.Combat,
                IsSpecialized = true,
                Untrained = 7,
                Trained = 4,
                PrimaryAttribute = "STR"
            },
            new Skill
            {
                Id = "unarmed-combat",
                Name = "Unarmed Combat",
                Category = SkillCategory.Combat,
                IsSpecialized = true,
                Untrained = 6,
                Trained = 3,
                PrimaryAttribute = "DEX"
            },

            // === MOVEMENT SKILLS ===
            new Skill
            {
                Id = "sprint",
                Name = "Sprint",
                Category = SkillCategory.Movement,
                IsSpecialized = false,
                Untrained = 5,
                Trained = 2,
                PrimaryAttribute = "DEX"
            },

            // === MAGIC: MANA SKILLS ===
            new Skill
            {
                Id = "fire-mana",
                Name = "Fire Mana",
                Category = SkillCategory.Mana,
                IsSpecialized = false,
                IsMagic = true,
                Untrained = 7,
                Trained = 4,
                PrimaryAttribute = "WIL"
            },
            new Skill
            {
                Id = "water-mana",
                Name = "Water Mana",
                Category = SkillCategory.Mana,
                IsSpecialized = false,
                IsMagic = true,
                Untrained = 7,
                Trained = 4,
                PrimaryAttribute = "WIL"
            },
            new Skill
            {
                Id = "light-mana",
                Name = "Light Mana",
                Category = SkillCategory.Mana,
                IsSpecialized = false,
                IsMagic = true,
                Untrained = 7,
                Trained = 4,
                PrimaryAttribute = "WIL"
            },
            new Skill
            {
                Id = "life-mana",
                Name = "Life Mana",
                Category = SkillCategory.Mana,
                IsSpecialized = false,
                IsMagic = true,
                Untrained = 7,
                Trained = 4,
                PrimaryAttribute = "WIL"
            },

            // === MAGIC: SPELL SKILLS ===
            new Skill
            {
                Id = "fire-bolt",
                Name = "Fire Bolt",
                Category = SkillCategory.Spell,
                IsSpecialized = false,
                IsMagic = true,
                Untrained = 8,
                Trained = 5,
                PrimaryAttribute = "INT"
            },
            new Skill
            {
                Id = "heal",
                Name = "Heal",
                Category = SkillCategory.Spell,
                IsSpecialized = false,
                IsMagic = true,
                Untrained = 8,
                Trained = 5,
                PrimaryAttribute = "WIL"
            },
            new Skill
            {
                Id = "illuminate",
                Name = "Illuminate",
                Category = SkillCategory.Spell,
                IsSpecialized = false,
                IsMagic = true,
                Untrained = 7,
                Trained = 4,
                PrimaryAttribute = "INT"
            }
        ];
    }

    private static List<SpellDefinition> CreateSpellDefinitions()
    {
        return
        [
            // === FIRE SPELLS ===
            new SpellDefinition
            {
                SkillId = "fire-bolt",
                MagicSchool = MagicSchool.Fire,
                SpellType = SpellType.Targeted,
                ManaCost = 1,
                Range = 2, // Short range
                ResistanceType = SpellResistanceType.Fixed,
                FixedResistanceTV = 8,
                EffectDescription = "Hurls a bolt of fire at the target, dealing fire damage."
            },
            new SpellDefinition
            {
                SkillId = "flame-shield",
                MagicSchool = MagicSchool.Fire,
                SpellType = SpellType.SelfBuff,
                ManaCost = 2,
                Range = 0, // Self
                DefaultDuration = 10, // 10 rounds
                ResistanceType = SpellResistanceType.None,
                EffectDescription = "Surrounds the caster with protective flames that damage attackers."
            },
            new SpellDefinition
            {
                SkillId = "wall-of-fire",
                MagicSchool = MagicSchool.Fire,
                SpellType = SpellType.Environmental,
                ManaCost = 3,
                Range = 2, // Short range
                AreaRadius = 3, // 3 meter radius
                DefaultDuration = 10, // 10 rounds base
                ResistanceType = SpellResistanceType.None, // Location-based, no resistance
                EffectDescription = "Creates a wall of fire at a location that burns anyone who enters or remains in the area."
            },

            // === WATER SPELLS ===
            new SpellDefinition
            {
                SkillId = "ice-shard",
                MagicSchool = MagicSchool.Water,
                SpellType = SpellType.Targeted,
                ManaCost = 1,
                Range = 2,
                ResistanceType = SpellResistanceType.Fixed,
                FixedResistanceTV = 8,
                EffectDescription = "Launches a shard of ice at the target."
            },
            new SpellDefinition
            {
                SkillId = "frost-armor",
                MagicSchool = MagicSchool.Water,
                SpellType = SpellType.SelfBuff,
                ManaCost = 2,
                Range = 0,
                DefaultDuration = 10,
                ResistanceType = SpellResistanceType.None,
                EffectDescription = "Encases the caster in protective ice armor."
            },

            // === LIGHT SPELLS ===
            new SpellDefinition
            {
                SkillId = "illuminate",
                MagicSchool = MagicSchool.Light,
                SpellType = SpellType.SelfBuff,
                ManaCost = 1,
                Range = 0,
                DefaultDuration = 60, // 60 rounds (3 minutes)
                ResistanceType = SpellResistanceType.None,
                EffectDescription = "Creates a bright light around the caster."
            },
            new SpellDefinition
            {
                SkillId = "illuminate-area",
                MagicSchool = MagicSchool.Light,
                SpellType = SpellType.AreaEffect,
                ManaCost = 2,
                Range = 2, // Short range
                AreaRadius = 10, // 10 meter radius
                DefaultDuration = 60, // 60 rounds (3 minutes)
                ResistanceType = SpellResistanceType.None,
                EffectDescription = "Creates a persistent magical light at a location, illuminating the area."
            },
            new SpellDefinition
            {
                SkillId = "blind",
                MagicSchool = MagicSchool.Light,
                SpellType = SpellType.Targeted,
                ManaCost = 2,
                Range = 1, // Touch/Short
                DefaultDuration = 3,
                ResistanceType = SpellResistanceType.Willpower,
                EffectDescription = "Blinds the target with a flash of brilliant light."
            },

            // === LIFE SPELLS ===
            new SpellDefinition
            {
                SkillId = "mystic-punch",
                MagicSchool = MagicSchool.Life,
                SpellType = SpellType.Targeted,
                ManaCost = 1,
                Range = 1, // Touch
                ResistanceType = SpellResistanceType.Fixed,
                FixedResistanceTV = 6,
                EffectDescription = "Channels magical force into a physical strike, dealing damage like a punch."
            },
            new SpellDefinition
            {
                SkillId = "minor-heal",
                MagicSchool = MagicSchool.Life,
                SpellType = SpellType.Targeted,
                ManaCost = 1,
                Range = 1, // Touch
                ResistanceType = SpellResistanceType.None,
                EffectDescription = "Heals minor wounds, restoring FAT."
            },
            new SpellDefinition
            {
                SkillId = "restore-vitality",
                MagicSchool = MagicSchool.Life,
                SpellType = SpellType.Targeted,
                ManaCost = 3,
                Range = 1,
                ResistanceType = SpellResistanceType.None,
                EffectDescription = "Restores vitality to the target."
            },
            new SpellDefinition
            {
                SkillId = "life-shield",
                MagicSchool = MagicSchool.Life,
                SpellType = SpellType.SelfBuff,
                ManaCost = 2,
                Range = 0,
                DefaultDuration = 10,
                ResistanceType = SpellResistanceType.None,
                EffectDescription = "Creates a protective aura that absorbs damage."
            }
        ];
    }

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
                Rarity = ItemRarity.Common,
                Tags = "material,crafting"
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
                Rarity = ItemRarity.Common,
                Tags = "material,crafting"
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
                Rarity = ItemRarity.Common,
                Tags = "melee,starter-gear"
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
                Tags = "melee,magical",
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
                Rarity = ItemRarity.Uncommon,
                Tags = "melee,starter-gear"
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
                Rarity = ItemRarity.Common,
                Tags = "melee,starter-gear"
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
                Rarity = ItemRarity.Common,
                Tags = "ranged,starter-gear",
                // Ranged weapon properties for shortbow
                CustomProperties = "{\"isRangedWeapon\":true,\"ammoType\":\"Arrow\",\"capacity\":1,\"rangeShort\":20,\"rangeMedium\":40,\"rangeLong\":80,\"rangeExtreme\":120,\"weaponModifier\":0,\"baseSVModifier\":0,\"fireModes\":[\"Single\"],\"reloadType\":\"Single\",\"reloadRoundsPerTick\":1,\"isDodgeable\":true}"
            },
            new ItemTemplate
            {
                Id = 15,
                Name = "War Spear",
                Description = "A long spear effective for both melee and thrown attacks.",
                ShortDescription = "Versatile spear",
                ItemType = ItemType.Weapon,
                WeaponType = WeaponType.Polearm,
                EquipmentSlot = EquipmentSlot.TwoHand,
                Weight = 4m,
                Volume = 0.3m,
                Value = 120,
                HasDurability = true,
                MaxDurability = 90,
                RelatedSkill = "Spears",
                MinSkillLevel = 0,
                DamageClass = 2,
                DamageType = "Piercing",
                Range = 10,
                Rarity = ItemRarity.Common,
                Tags = "melee,starter-gear"
            },
            new ItemTemplate
            {
                Id = 16,
                Name = "Longbow",
                Description = "A tall, powerful bow requiring considerable strength to draw. Excellent range and penetration.",
                ShortDescription = "Powerful longbow",
                ItemType = ItemType.Weapon,
                WeaponType = WeaponType.Bow,
                EquipmentSlot = EquipmentSlot.TwoHand,
                Weight = 3m,
                Volume = 0.4m,
                Value = 200,
                HasDurability = true,
                MaxDurability = 100,
                RelatedSkill = "Bows",
                MinSkillLevel = 2,
                DamageClass = 3,
                DamageType = "Piercing",
                Range = 150,
                Rarity = ItemRarity.Uncommon,
                Tags = "ranged,starter-gear",
                CustomProperties = "{\"isRangedWeapon\":true,\"ammoType\":\"Arrow\",\"capacity\":1,\"rangeShort\":30,\"rangeMedium\":60,\"rangeLong\":120,\"rangeExtreme\":180,\"weaponModifier\":0,\"baseSVModifier\":0,\"fireModes\":[\"Single\"],\"reloadType\":\"Single\",\"reloadRoundsPerTick\":1,\"isDodgeable\":true}"
            },
            new ItemTemplate
            {
                Id = 17,
                Name = "Heavy Crossbow",
                Description = "A powerful crossbow with a mechanical crank for drawing. Slow to reload but devastating.",
                ShortDescription = "Heavy crossbow",
                ItemType = ItemType.Weapon,
                WeaponType = WeaponType.Crossbow,
                EquipmentSlot = EquipmentSlot.TwoHand,
                Weight = 6m,
                Volume = 0.5m,
                Value = 300,
                HasDurability = true,
                MaxDurability = 120,
                RelatedSkill = "Crossbows",
                MinSkillLevel = 1,
                DamageClass = 4,
                DamageType = "Piercing",
                Range = 120,
                Rarity = ItemRarity.Uncommon,
                Tags = "ranged,starter-gear",
                CustomProperties = "{\"isRangedWeapon\":true,\"ammoType\":\"Bolt\",\"capacity\":1,\"rangeShort\":30,\"rangeMedium\":60,\"rangeLong\":100,\"rangeExtreme\":150,\"weaponModifier\":1,\"baseSVModifier\":1,\"fireModes\":[\"Single\"],\"reloadType\":\"Single\",\"reloadRoundsPerTick\":2,\"isDodgeable\":true}"
            },
            new ItemTemplate
            {
                Id = 18,
                Name = "Throwing Knives (Set of 6)",
                Description = "A balanced set of six throwing knives designed for quick, accurate throws.",
                ShortDescription = "6 throwing knives",
                ItemType = ItemType.Weapon,
                WeaponType = WeaponType.Thrown,
                EquipmentSlot = EquipmentSlot.MainHand,
                Weight = 0.6m,
                Volume = 0.03m,
                Value = 60,
                HasDurability = true,
                MaxDurability = 40,
                RelatedSkill = "Daggers",
                MinSkillLevel = 0,
                DamageClass = 1,
                DamageType = "Piercing",
                Range = 20,
                IsStackable = true,
                MaxStackSize = 6,
                Rarity = ItemRarity.Common,
                Tags = "ranged,starter-gear",
                CustomProperties = "{\"isRangedWeapon\":true,\"isDodgeable\":true,\"rangeShort\":5,\"rangeMedium\":10,\"rangeLong\":15,\"rangeExtreme\":25}"
            },

            // === RANGED WEAPONS (SCI-FI) ===
            new ItemTemplate
            {
                Id = 100,
                Name = "9mm Pistol",
                Description = "A reliable semi-automatic pistol chambered in 9mm. Standard sidearm for security forces.",
                ShortDescription = "9mm sidearm",
                ItemType = ItemType.Weapon,
                WeaponType = WeaponType.Pistol,
                EquipmentSlot = EquipmentSlot.MainHand,
                Weight = 1.2m,
                Volume = 0.05m,
                Value = 400,
                HasDurability = true,
                MaxDurability = 100,
                RelatedSkill = "Pistols",
                MinSkillLevel = 0,
                DamageClass = 2,
                DamageType = "Piercing",
                Range = 50,
                Rarity = ItemRarity.Common,
                Tags = "ranged,firearm,modern",
                // Ranged weapon properties: magazine-fed, single/burst fire modes
                CustomProperties = "{\"isRangedWeapon\":true,\"ammoType\":\"9mm\",\"capacity\":15,\"rangeShort\":10,\"rangeMedium\":25,\"rangeLong\":50,\"rangeExtreme\":100,\"weaponModifier\":1,\"baseSVModifier\":0,\"fireModes\":[\"Single\",\"Burst\"],\"burstSize\":3,\"reloadType\":\"Magazine\",\"reloadRoundsPerTick\":3,\"isDodgeable\":false}"
            },
            new ItemTemplate
            {
                Id = 105,
                Name = "Assault Rifle",
                Description = "A versatile automatic rifle chambered in 5.56mm. Standard military infantry weapon with multiple fire modes.",
                ShortDescription = "5.56mm assault rifle",
                ItemType = ItemType.Weapon,
                WeaponType = WeaponType.Rifle,
                EquipmentSlot = EquipmentSlot.TwoHand,
                Weight = 4.5m,
                Volume = 0.4m,
                Value = 1200,
                HasDurability = true,
                MaxDurability = 150,
                RelatedSkill = "Rifles",
                MinSkillLevel = 1,
                DamageClass = 3,
                DamageType = "Piercing",
                Range = 200,
                Rarity = ItemRarity.Uncommon,
                Tags = "ranged,firearm,modern",
                CustomProperties = "{\"isRangedWeapon\":true,\"ammoType\":\"5.56mm\",\"capacity\":30,\"rangeShort\":50,\"rangeMedium\":150,\"rangeLong\":300,\"rangeExtreme\":500,\"weaponModifier\":1,\"baseSVModifier\":0,\"fireModes\":[\"Single\",\"Burst\",\"Auto\"],\"burstSize\":3,\"sustainedBurstSize\":5,\"reloadType\":\"Magazine\",\"reloadRoundsPerTick\":3,\"isDodgeable\":false}"
            },
            new ItemTemplate
            {
                Id = 106,
                Name = "Combat Shotgun",
                Description = "A rugged pump-action shotgun designed for close quarters combat. Devastating at short range.",
                ShortDescription = "12 gauge shotgun",
                ItemType = ItemType.Weapon,
                WeaponType = WeaponType.Shotgun,
                EquipmentSlot = EquipmentSlot.TwoHand,
                Weight = 5m,
                Volume = 0.35m,
                Value = 800,
                HasDurability = true,
                MaxDurability = 120,
                RelatedSkill = "Shotguns",
                MinSkillLevel = 0,
                DamageClass = 4,
                DamageType = "Piercing",
                Range = 50,
                Rarity = ItemRarity.Common,
                Tags = "ranged,firearm,modern",
                CustomProperties = "{\"isRangedWeapon\":true,\"ammoType\":\"12gauge\",\"capacity\":8,\"rangeShort\":10,\"rangeMedium\":25,\"rangeLong\":40,\"rangeExtreme\":60,\"weaponModifier\":2,\"baseSVModifier\":1,\"fireModes\":[\"Single\"],\"reloadType\":\"Single\",\"reloadRoundsPerTick\":1,\"isDodgeable\":false,\"spreadPattern\":true}"
            },
            new ItemTemplate
            {
                Id = 107,
                Name = "Compact SMG",
                Description = "A lightweight submachine gun chambered in 9mm. High rate of fire with manageable recoil.",
                ShortDescription = "9mm SMG",
                ItemType = ItemType.Weapon,
                WeaponType = WeaponType.SMG,
                EquipmentSlot = EquipmentSlot.MainHand,
                Weight = 2.5m,
                Volume = 0.15m,
                Value = 900,
                HasDurability = true,
                MaxDurability = 100,
                RelatedSkill = "SMGs",
                MinSkillLevel = 0,
                DamageClass = 2,
                DamageType = "Piercing",
                Range = 75,
                Rarity = ItemRarity.Uncommon,
                Tags = "ranged,firearm,modern",
                CustomProperties = "{\"isRangedWeapon\":true,\"ammoType\":\"9mm\",\"capacity\":25,\"rangeShort\":15,\"rangeMedium\":35,\"rangeLong\":60,\"rangeExtreme\":100,\"weaponModifier\":1,\"baseSVModifier\":0,\"fireModes\":[\"Single\",\"Burst\",\"Auto\"],\"burstSize\":3,\"sustainedBurstSize\":4,\"reloadType\":\"Magazine\",\"reloadRoundsPerTick\":2,\"isDodgeable\":false}"
            },
            new ItemTemplate
            {
                Id = 108,
                Name = "Laser Pistol",
                Description = "A compact energy weapon that fires coherent light beams. Accurate and silent, powered by rechargeable cells.",
                ShortDescription = "Energy sidearm",
                ItemType = ItemType.Weapon,
                WeaponType = WeaponType.Pistol,
                EquipmentSlot = EquipmentSlot.MainHand,
                Weight = 0.8m,
                Volume = 0.04m,
                Value = 2500,
                HasDurability = true,
                MaxDurability = 80,
                RelatedSkill = "Pistols",
                MinSkillLevel = 1,
                DamageClass = 2,
                DamageType = "Energy",
                Range = 75,
                Rarity = ItemRarity.Rare,
                Tags = "ranged,firearm,modern",
                CustomProperties = "{\"isRangedWeapon\":true,\"ammoType\":\"PowerCell\",\"capacity\":20,\"rangeShort\":20,\"rangeMedium\":50,\"rangeLong\":75,\"rangeExtreme\":120,\"weaponModifier\":2,\"baseSVModifier\":0,\"fireModes\":[\"Single\"],\"reloadType\":\"Magazine\",\"reloadRoundsPerTick\":2,\"isDodgeable\":false,\"isEnergyWeapon\":true,\"noBulletDrop\":true}"
            },

            // === AMMUNITION ===
            new ItemTemplate
            {
                Id = 101,
                Name = "9mm FMJ Rounds",
                Description = "Standard 9mm full metal jacket ammunition. Reliable and accurate.",
                ShortDescription = "9mm ammo",
                ItemType = ItemType.Ammunition,
                Weight = 0.01m,
                Volume = 0.001m,
                Value = 1,
                IsStackable = true,
                MaxStackSize = 100,
                Rarity = ItemRarity.Common,
                Tags = "ammunition",
                // Ammunition properties
                CustomProperties = "{\"ammoType\":\"9mm\",\"damageModifier\":0,\"isLooseAmmo\":true}"
            },
            new ItemTemplate
            {
                Id = 102,
                Name = "9mm Hollow-Point Rounds",
                Description = "9mm hollow-point ammunition. Higher damage against unarmored targets.",
                ShortDescription = "9mm HP ammo",
                ItemType = ItemType.Ammunition,
                Weight = 0.01m,
                Volume = 0.001m,
                Value = 3,
                IsStackable = true,
                MaxStackSize = 100,
                Rarity = ItemRarity.Uncommon,
                Tags = "ammunition",
                // Ammunition properties with damage bonus
                CustomProperties = "{\"ammoType\":\"9mm\",\"damageModifier\":2,\"specialEffect\":\"Hollow-Point\",\"isLooseAmmo\":true}"
            },
            new ItemTemplate
            {
                Id = 103,
                Name = "9mm Magazine (15 rnd)",
                Description = "A standard 15-round magazine for 9mm pistols.",
                ShortDescription = "Pistol magazine",
                ItemType = ItemType.Container,
                Weight = 0.15m,
                Volume = 0.02m,
                Value = 25,
                IsContainer = true,
                ContainerMaxWeight = 0.5m,
                ContainerMaxVolume = 0.02m,
                ContainerAllowedTypes = "9mm",
                Rarity = ItemRarity.Common,
                Tags = "ammunition,container",
                // Magazine state - capacity and ammo type
                CustomProperties = "{\"ammoType\":\"9mm\",\"capacity\":15,\"currentAmmo\":0,\"ammoDamageModifier\":0}"
            },
            new ItemTemplate
            {
                Id = 104,
                Name = "Arrow",
                Description = "A standard wooden arrow with an iron tip.",
                ShortDescription = "Wooden arrow",
                ItemType = ItemType.Ammunition,
                Weight = 0.05m,
                Volume = 0.01m,
                Value = 1,
                IsStackable = true,
                MaxStackSize = 50,
                Rarity = ItemRarity.Common,
                Tags = "ammunition",
                // Arrow ammunition properties
                CustomProperties = "{\"ammoType\":\"Arrow\",\"damageModifier\":0,\"isLooseAmmo\":true}"
            },
            new ItemTemplate
            {
                Id = 109,
                Name = "Crossbow Bolt",
                Description = "A short, heavy bolt designed for crossbows. Greater penetration than arrows.",
                ShortDescription = "Crossbow bolt",
                ItemType = ItemType.Ammunition,
                Weight = 0.08m,
                Volume = 0.01m,
                Value = 2,
                IsStackable = true,
                MaxStackSize = 30,
                Rarity = ItemRarity.Common,
                Tags = "ammunition",
                CustomProperties = "{\"ammoType\":\"Bolt\",\"damageModifier\":0,\"isLooseAmmo\":true}"
            },
            new ItemTemplate
            {
                Id = 110,
                Name = "5.56mm FMJ Rounds",
                Description = "Standard 5.56mm NATO full metal jacket rifle ammunition.",
                ShortDescription = "5.56mm ammo",
                ItemType = ItemType.Ammunition,
                Weight = 0.012m,
                Volume = 0.002m,
                Value = 2,
                IsStackable = true,
                MaxStackSize = 100,
                Rarity = ItemRarity.Common,
                Tags = "ammunition",
                CustomProperties = "{\"ammoType\":\"5.56mm\",\"damageModifier\":0,\"isLooseAmmo\":true}"
            },
            new ItemTemplate
            {
                Id = 111,
                Name = "12 Gauge Buckshot",
                Description = "Standard 12 gauge buckshot shells. Effective spread pattern for close quarters.",
                ShortDescription = "Buckshot shells",
                ItemType = ItemType.Ammunition,
                Weight = 0.04m,
                Volume = 0.008m,
                Value = 3,
                IsStackable = true,
                MaxStackSize = 25,
                Rarity = ItemRarity.Common,
                Tags = "ammunition",
                CustomProperties = "{\"ammoType\":\"12gauge\",\"damageModifier\":0,\"spreadPattern\":true,\"isLooseAmmo\":true}"
            },
            new ItemTemplate
            {
                Id = 112,
                Name = "12 Gauge Slug",
                Description = "Solid 12 gauge slugs for greater range and penetration. Trades spread for accuracy.",
                ShortDescription = "Slug rounds",
                ItemType = ItemType.Ammunition,
                Weight = 0.05m,
                Volume = 0.008m,
                Value = 5,
                IsStackable = true,
                MaxStackSize = 25,
                Rarity = ItemRarity.Uncommon,
                Tags = "ammunition",
                CustomProperties = "{\"ammoType\":\"12gauge\",\"damageModifier\":1,\"rangeBonus\":1,\"spreadPattern\":false,\"isLooseAmmo\":true}"
            },
            new ItemTemplate
            {
                Id = 113,
                Name = "Power Cell",
                Description = "A rechargeable energy cell for laser and plasma weapons. Can be recharged at charging stations.",
                ShortDescription = "Energy cell",
                ItemType = ItemType.Ammunition,
                Weight = 0.1m,
                Volume = 0.02m,
                Value = 50,
                IsStackable = true,
                MaxStackSize = 10,
                Rarity = ItemRarity.Uncommon,
                Tags = "ammunition",
                CustomProperties = "{\"ammoType\":\"PowerCell\",\"damageModifier\":0,\"isRechargeable\":true,\"maxCharges\":20,\"rechargeTime\":240}"
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
                ArmorAbsorption = "{\"Cutting\": 2, \"Piercing\": 1, \"Bashing\": 1}",
                Rarity = ItemRarity.Common,
                Tags = "armor,starter-gear"
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
                ArmorAbsorption = "{\"Cutting\": 4, \"Piercing\": 3, \"Bashing\": 2}",
                Rarity = ItemRarity.Uncommon,
                Tags = "armor,starter-gear"
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
                ArmorAbsorption = "{\"Cutting\": 3, \"Piercing\": 3, \"Bashing\": 2}",
                Rarity = ItemRarity.Common,
                Tags = "armor,starter-gear"
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
                Rarity = ItemRarity.Common,
                Tags = "armor,starter-gear,shield"
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
                Rarity = ItemRarity.Common,
                Tags = "armor,starter-gear"
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
                Rarity = ItemRarity.Common,
                Tags = "container,utility"
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
                Rarity = ItemRarity.Common,
                Tags = "container,utility"
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
                Rarity = ItemRarity.Common,
                Tags = "container,utility"
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
                Rarity = ItemRarity.Epic,
                Tags = "container,magical"
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
                Tags = "jewelry,magical",
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
                Tags = "jewelry,magical",
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
                Tags = "armor,magical",
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
                Tags = "jewelry,magical",
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
                Rarity = ItemRarity.Common,
                Tags = "consumable"
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
                Rarity = ItemRarity.Common,
                Tags = "consumable"
            },
            new ItemTemplate
            {
                Id = 52,
                Name = "Frag Grenade",
                Description = "A military fragmentation grenade. Pull pin, throw, take cover. Explodes after a short delay.",
                ShortDescription = "Explosive grenade",
                ItemType = ItemType.Consumable,
                Weight = 0.5m,
                Volume = 0.05m,
                Value = 150,
                IsStackable = true,
                MaxStackSize = 5,
                DamageClass = 3,
                DamageType = "Explosive",
                Rarity = ItemRarity.Uncommon,
                Tags = "consumable",
                CustomProperties = "{\"isThrown\":true,\"range\":30,\"areaRadius\":5,\"fuseTime\":2,\"canBeCountered\":true}"
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
                Rarity = ItemRarity.Common,
                Tags = "tool,utility"
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
                Rarity = ItemRarity.Uncommon,
                Tags = "tool,utility"
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
                Rarity = ItemRarity.Common,
                Tags = "tool,utility"
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
                Rarity = ItemRarity.Common,
                Tags = "treasure,currency"
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
                Rarity = ItemRarity.Common,
                Tags = "treasure,currency"
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
                Rarity = ItemRarity.Common,
                Tags = "treasure,currency"
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
                Rarity = ItemRarity.Uncommon,
                Tags = "treasure"
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
                Rarity = ItemRarity.Common,
                Tags = "food,consumable"
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
                Rarity = ItemRarity.Common,
                Tags = "drink,consumable"
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
                Rarity = ItemRarity.Common,
                Tags = "clothing,starter-gear"
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
                Rarity = ItemRarity.Common,
                Tags = "clothing,starter-gear"
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
            },

            // === RANGED WEAPON EQUIPMENT ===
            new CharacterItem
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                ItemTemplateId = 100, // 9mm Pistol
                OwnerCharacterId = 1,
                IsEquipped = false,
                CurrentDurability = 100,
                // Loaded with 10 rounds of standard 9mm ammo
                CustomProperties = "{\"loadedAmmo\":10,\"loadedAmmoType\":\"9mm\",\"loadedAmmoDamageModifier\":0}"
            },
            new CharacterItem
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                ItemTemplateId = 103, // 9mm Magazine (15 rnd)
                OwnerCharacterId = 1,
                ContainerItemId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                // Magazine is loaded with 15 rounds of FMJ
                CustomProperties = "{\"ammoType\":\"9mm\",\"capacity\":15,\"currentAmmo\":15,\"ammoDamageModifier\":0}"
            },
            new CharacterItem
            {
                Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                ItemTemplateId = 101, // 9mm FMJ Rounds (loose)
                OwnerCharacterId = 1,
                ContainerItemId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                StackSize = 30
            },
            new CharacterItem
            {
                Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                ItemTemplateId = 14, // Shortbow
                OwnerCharacterId = 1,
                IsEquipped = false,
                CurrentDurability = 80
            },
            new CharacterItem
            {
                Id = Guid.Parse("bbbbbbbb-cccc-dddd-eeee-ffffffffffff"),
                ItemTemplateId = 104, // Arrows
                OwnerCharacterId = 1,
                ContainerItemId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                StackSize = 20
            }
        ];
    }

    private static List<EffectDefinition> CreateEffectDefinitions()
    {
        return
        [
            // === WOUNDS ===
            new EffectDefinition
            {
                Id = 1,
                Name = "Wound",
                Description = "A physical injury that causes ongoing pain and impairs abilities.",
                EffectType = EffectType.Wound,
                TargetType = EffectTargetType.Character,
                DurationType = DurationType.UntilRemoved,
                DefaultDuration = 0,
                IsStackable = true,
                MaxStacks = 4,
                StackBehavior = StackBehavior.Independent,
                CanBeRemoved = true,
                RemovalMethods = "Medicine,Spell,NaturalHealing",
                RemovalDifficulty = 8,
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 1,
                        EffectDefinitionId = 1,
                        ImpactType = EffectImpactType.ASModifier,
                        Target = "All",
                        Value = -2
                    },
                    new EffectImpact
                    {
                        Id = 2,
                        EffectDefinitionId = 1,
                        ImpactType = EffectImpactType.DamageOverTime,
                        Target = "FAT",
                        Value = 1,
                        DamageInterval = 2
                    }
                ]
            },

            // === CONDITIONS ===
            new EffectDefinition
            {
                Id = 10,
                Name = "Stunned",
                Description = "Unable to take actions. FAT recovery suspended.",
                EffectType = EffectType.Condition,
                TargetType = EffectTargetType.Character,
                DurationType = DurationType.Rounds,
                DefaultDuration = 1,
                IsStackable = false,
                StackBehavior = StackBehavior.Replace,
                CanBeRemoved = true,
                RemovalMethods = "Time,Healing",
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 10,
                        EffectDefinitionId = 10,
                        ImpactType = EffectImpactType.SpecialAbility,
                        Target = "Actions",
                        Value = 0 // 0 = disabled
                    },
                    new EffectImpact
                    {
                        Id = 11,
                        EffectDefinitionId = 10,
                        ImpactType = EffectImpactType.RecoveryModifier,
                        Target = "FAT",
                        Value = 0 // No recovery
                    }
                ]
            },
            new EffectDefinition
            {
                Id = 11,
                Name = "Unconscious",
                Description = "Completely unaware and unable to act.",
                EffectType = EffectType.Condition,
                TargetType = EffectTargetType.Character,
                DurationType = DurationType.UntilRemoved,
                DefaultDuration = 0,
                IsStackable = false,
                StackBehavior = StackBehavior.Replace,
                CanBeRemoved = true,
                RemovalMethods = "Time,Healing,Damage",
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 12,
                        EffectDefinitionId = 11,
                        ImpactType = EffectImpactType.SpecialAbility,
                        Target = "Actions",
                        Value = 0
                    }
                ]
            },
            new EffectDefinition
            {
                Id = 12,
                Name = "Prone",
                Description = "On the ground. Harder to hit with ranged, easier for melee.",
                EffectType = EffectType.Condition,
                TargetType = EffectTargetType.Character,
                DurationType = DurationType.UntilRemoved,
                DefaultDuration = 0,
                IsStackable = false,
                StackBehavior = StackBehavior.Replace,
                CanBeRemoved = true,
                RemovalMethods = "StandUp",
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 13,
                        EffectDefinitionId = 12,
                        ImpactType = EffectImpactType.TVModifier,
                        Target = "Ranged",
                        Value = 2 // Harder to hit with ranged
                    },
                    new EffectImpact
                    {
                        Id = 14,
                        EffectDefinitionId = 12,
                        ImpactType = EffectImpactType.TVModifier,
                        Target = "Melee",
                        Value = -2 // Easier to hit in melee
                    }
                ]
            },
            new EffectDefinition
            {
                Id = 13,
                Name = "Blinded",
                Description = "Cannot see. Severe penalties to visual actions.",
                EffectType = EffectType.Condition,
                TargetType = EffectTargetType.Character,
                DurationType = DurationType.UntilRemoved,
                DefaultDuration = 0,
                IsStackable = false,
                StackBehavior = StackBehavior.Extend,
                CanBeRemoved = true,
                RemovalMethods = "Spell,Medicine,Time",
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 15,
                        EffectDefinitionId = 13,
                        ImpactType = EffectImpactType.ASModifier,
                        Target = "Physical",
                        Value = -4
                    },
                    new EffectImpact
                    {
                        Id = 16,
                        EffectDefinitionId = 13,
                        ImpactType = EffectImpactType.TVModifier,
                        Target = "Self",
                        Value = -4 // Easier to hit
                    }
                ]
            },

            // === POISONS ===
            new EffectDefinition
            {
                Id = 20,
                Name = "Weak Poison",
                Description = "A mild poison causing fatigue damage over time.",
                EffectType = EffectType.Poison,
                TargetType = EffectTargetType.Character,
                DurationType = DurationType.Minutes,
                DefaultDuration = 10,
                IsStackable = true,
                MaxStacks = 3,
                StackBehavior = StackBehavior.Intensify,
                CanBeRemoved = true,
                RemovalMethods = "Medicine,Spell,Antidote",
                RemovalDifficulty = 6,
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 20,
                        EffectDefinitionId = 20,
                        ImpactType = EffectImpactType.DamageOverTime,
                        Target = "FAT",
                        Value = 1,
                        DamageInterval = 20 // Per minute (20 rounds)
                    },
                    new EffectImpact
                    {
                        Id = 21,
                        EffectDefinitionId = 20,
                        ImpactType = EffectImpactType.ASModifier,
                        Target = "All",
                        Value = -1
                    }
                ]
            },
            new EffectDefinition
            {
                Id = 21,
                Name = "Strong Poison",
                Description = "A potent poison causing vitality damage over time.",
                EffectType = EffectType.Poison,
                TargetType = EffectTargetType.Character,
                DurationType = DurationType.Minutes,
                DefaultDuration = 5,
                IsStackable = true,
                MaxStacks = 3,
                StackBehavior = StackBehavior.Intensify,
                CanBeRemoved = true,
                RemovalMethods = "Medicine,Spell,Antidote",
                RemovalDifficulty = 10,
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 22,
                        EffectDefinitionId = 21,
                        ImpactType = EffectImpactType.DamageOverTime,
                        Target = "VIT",
                        Value = 1,
                        DamageInterval = 10 // Every 30 seconds
                    },
                    new EffectImpact
                    {
                        Id = 23,
                        EffectDefinitionId = 21,
                        ImpactType = EffectImpactType.ASModifier,
                        Target = "All",
                        Value = -2
                    }
                ]
            },

            // === BUFFS ===
            new EffectDefinition
            {
                Id = 30,
                Name = "Strength Boost",
                Description = "Magical enhancement to physical strength.",
                EffectType = EffectType.Buff,
                TargetType = EffectTargetType.Character,
                Source = "Spell",
                DurationType = DurationType.Minutes,
                DefaultDuration = 10,
                IsStackable = false,
                StackBehavior = StackBehavior.Replace,
                CanBeRemoved = true,
                RemovalMethods = "Dispel,Time",
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 30,
                        EffectDefinitionId = 30,
                        ImpactType = EffectImpactType.AttributeModifier,
                        Target = "STR",
                        Value = 2
                    }
                ]
            },
            new EffectDefinition
            {
                Id = 31,
                Name = "Agility Boost",
                Description = "Magical enhancement to speed and reflexes.",
                EffectType = EffectType.Buff,
                TargetType = EffectTargetType.Character,
                Source = "Spell",
                DurationType = DurationType.Minutes,
                DefaultDuration = 10,
                IsStackable = false,
                StackBehavior = StackBehavior.Replace,
                CanBeRemoved = true,
                RemovalMethods = "Dispel,Time",
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 31,
                        EffectDefinitionId = 31,
                        ImpactType = EffectImpactType.AttributeModifier,
                        Target = "DEX",
                        Value = 2
                    }
                ]
            },
            new EffectDefinition
            {
                Id = 32,
                Name = "Battle Focus",
                Description = "Heightened combat awareness and precision.",
                EffectType = EffectType.Buff,
                TargetType = EffectTargetType.Character,
                Source = "Skill",
                DurationType = DurationType.Rounds,
                DefaultDuration = 10,
                IsStackable = false,
                StackBehavior = StackBehavior.Replace,
                CanBeRemoved = true,
                RemovalMethods = "Time",
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 32,
                        EffectDefinitionId = 32,
                        ImpactType = EffectImpactType.AVModifier,
                        Target = "All",
                        Value = 2
                    }
                ]
            },

            // === DEBUFFS ===
            new EffectDefinition
            {
                Id = 40,
                Name = "Weakened",
                Description = "Physical strength is diminished.",
                EffectType = EffectType.Debuff,
                TargetType = EffectTargetType.Character,
                DurationType = DurationType.Minutes,
                DefaultDuration = 5,
                IsStackable = false,
                StackBehavior = StackBehavior.Replace,
                CanBeRemoved = true,
                RemovalMethods = "Spell,Rest",
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 40,
                        EffectDefinitionId = 40,
                        ImpactType = EffectImpactType.AttributeModifier,
                        Target = "STR",
                        Value = -2
                    }
                ]
            },
            new EffectDefinition
            {
                Id = 41,
                Name = "Intoxicated",
                Description = "Impaired by alcohol or similar substances.",
                EffectType = EffectType.Debuff,
                TargetType = EffectTargetType.Character,
                DurationType = DurationType.Hours,
                DefaultDuration = 2,
                IsStackable = true,
                MaxStacks = 3,
                StackBehavior = StackBehavior.Intensify,
                CanBeRemoved = true,
                RemovalMethods = "Time,Medicine",
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 41,
                        EffectDefinitionId = 41,
                        ImpactType = EffectImpactType.ASModifier,
                        Target = "Physical",
                        Value = -2
                    },
                    new EffectImpact
                    {
                        Id = 42,
                        EffectDefinitionId = 41,
                        ImpactType = EffectImpactType.ASModifier,
                        Target = "Mental",
                        Value = -1
                    },
                    new EffectImpact
                    {
                        Id = 43,
                        EffectDefinitionId = 41,
                        ImpactType = EffectImpactType.AttributeModifier,
                        Target = "WIL",
                        Value = -2
                    }
                ]
            },

            // === SPELL EFFECTS ===
            new EffectDefinition
            {
                Id = 50,
                Name = "Invisibility",
                Description = "Rendered invisible to normal sight.",
                EffectType = EffectType.SpellEffect,
                TargetType = EffectTargetType.Character,
                Source = "Spell",
                DurationType = DurationType.Minutes,
                DefaultDuration = 5,
                IsStackable = false,
                StackBehavior = StackBehavior.Extend,
                CanBeRemoved = true,
                RemovalMethods = "Dispel,Detection,BreakAction",
                BreakConditions = "Attack,CastSpell,TakeDamage",
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 50,
                        EffectDefinitionId = 50,
                        ImpactType = EffectImpactType.SpecialAbility,
                        Target = "Visibility",
                        Value = 0 // 0 = Invisible
                    },
                    new EffectImpact
                    {
                        Id = 51,
                        EffectDefinitionId = 50,
                        ImpactType = EffectImpactType.TVModifier,
                        Target = "Self",
                        Value = 4 // Harder for enemies to hit
                    }
                ]
            },
            new EffectDefinition
            {
                Id = 51,
                Name = "Magic Shield",
                Description = "A protective magical barrier.",
                EffectType = EffectType.SpellEffect,
                TargetType = EffectTargetType.Character,
                Source = "Spell",
                DurationType = DurationType.Minutes,
                DefaultDuration = 10,
                IsStackable = false,
                StackBehavior = StackBehavior.Replace,
                CanBeRemoved = true,
                RemovalMethods = "Dispel,Time",
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 52,
                        EffectDefinitionId = 51,
                        ImpactType = EffectImpactType.TVModifier,
                        Target = "Self",
                        Value = 2
                    }
                ]
            },
            new EffectDefinition
            {
                Id = 52,
                Name = "Burning",
                Description = "Engulfed in magical flames causing ongoing damage.",
                EffectType = EffectType.SpellEffect,
                TargetType = EffectTargetType.Character,
                Source = "Spell",
                DurationType = DurationType.Rounds,
                DefaultDuration = 5,
                IsStackable = true,
                MaxStacks = 3,
                StackBehavior = StackBehavior.Intensify,
                CanBeRemoved = true,
                RemovalMethods = "Water,Spell,Roll",
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 53,
                        EffectDefinitionId = 52,
                        ImpactType = EffectImpactType.DamageOverTime,
                        Target = "FAT",
                        Value = 2,
                        DamageInterval = 1 // Every round
                    }
                ]
            },

            // === OBJECT EFFECTS ===
            new EffectDefinition
            {
                Id = 60,
                Name = "Magical Light",
                Description = "The item glows with magical light.",
                EffectType = EffectType.ObjectEffect,
                TargetType = EffectTargetType.Item,
                Source = "Spell",
                DurationType = DurationType.Hours,
                DefaultDuration = 4,
                IsStackable = false,
                StackBehavior = StackBehavior.Extend,
                CanBeRemoved = true,
                RemovalMethods = "Dispel,Time",
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 60,
                        EffectDefinitionId = 60,
                        ImpactType = EffectImpactType.SpecialAbility,
                        Target = "LightRadius",
                        Value = 10 // 10 meters
                    }
                ]
            },
            new EffectDefinition
            {
                Id = 61,
                Name = "Temporary Enchantment",
                Description = "A temporary magical enhancement to the item.",
                EffectType = EffectType.ObjectEffect,
                TargetType = EffectTargetType.Item,
                Source = "Spell",
                DurationType = DurationType.Hours,
                DefaultDuration = 1,
                IsStackable = false,
                StackBehavior = StackBehavior.Replace,
                CanBeRemoved = true,
                RemovalMethods = "Dispel,Time",
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 61,
                        EffectDefinitionId = 61,
                        ImpactType = EffectImpactType.SkillModifier,
                        Target = "RelatedSkill",
                        Value = 2
                    }
                ]
            },
            new EffectDefinition
            {
                Id = 62,
                Name = "Cursed",
                Description = "A malevolent curse affecting the item and its holder.",
                EffectType = EffectType.ObjectEffect,
                TargetType = EffectTargetType.Item,
                Source = "Curse",
                DurationType = DurationType.Permanent,
                DefaultDuration = 0,
                IsStackable = false,
                StackBehavior = StackBehavior.Replace,
                CanBeRemoved = true,
                RemovalMethods = "RemoveCurse,Dispel",
                RemovalDifficulty = 12,
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 62,
                        EffectDefinitionId = 62,
                        ImpactType = EffectImpactType.ASModifier,
                        Target = "All",
                        Value = -2
                    }
                ]
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
                new SpeciesAttributeModifier { AttributeName = "SOC", Modifier = -1 }
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
