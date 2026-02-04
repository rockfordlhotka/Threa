using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Threa.Dal.Dto;

namespace Threa.Dal.Sqlite;

/// <summary>
/// Seeds test data into a SQLite database for unit testing.
/// Migrates all seed data from the former MockDb implementation.
/// </summary>
public class TestDataSeeder
{
    /// <summary>
    /// Seeds all test data into the database.
    /// </summary>
    public async Task SeedAllAsync(IServiceProvider services)
    {
        await SeedPlayersAsync(services);
        // Note: Species are hardcoded in SpeciesDal, no need to seed
        await SeedSkillsAsync(services);
        // Note: Magic schools are hardcoded in MagicSchoolDal, no need to seed
        await SeedSpellDefinitionsAsync(services);
        await SeedItemTemplatesAsync(services);
        await SeedEffectDefinitionsAsync(services);
        await SeedEffectTemplatesAsync(services);
        await SeedTablesAsync(services);
        await SeedCharactersAsync(services);
        await SeedCharacterItemsAsync(services);
    }

    private async Task SeedPlayersAsync(IServiceProvider services)
    {
        var dal = services.GetRequiredService<IPlayerDal>();
        var players = new[]
        {
            new Player { Id = 42, Name = "Rocky", Email = "rocky@lhotka.net" },
            new Player { Id = 43, Name = "Illiante", Email = "illiante@yahoo.com" }
        };

        foreach (var player in players)
        {
            await dal.SavePlayerAsync(player);
        }
    }

    private async Task SeedSpeciesAsync(IServiceProvider services)
    {
        var dal = services.GetRequiredService<ISpeciesDal>();
        var species = new[]
        {
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
        };

        foreach (var s in species)
        {
            await dal.SaveSpeciesAsync(s);
        }
    }

    private async Task SeedSkillsAsync(IServiceProvider services)
    {
        var dal = services.GetRequiredService<ISkillDal>();
        var skills = CreateSkills();

        foreach (var skill in skills)
        {
            await dal.SaveSkillAsync(skill);
        }
    }

    private async Task SeedMagicSchoolsAsync(IServiceProvider services)
    {
        var dal = services.GetRequiredService<IMagicSchoolDal>();
        var schools = CreateMagicSchools();

        foreach (var school in schools)
        {
            await dal.SaveSchoolAsync(school);
        }
    }

    private async Task SeedSpellDefinitionsAsync(IServiceProvider services)
    {
        var dal = services.GetRequiredService<ISpellDefinitionDal>();
        var spells = CreateSpellDefinitions();

        foreach (var spell in spells)
        {
            await dal.SaveSpellAsync(spell);
        }
    }

    private async Task SeedItemTemplatesAsync(IServiceProvider services)
    {
        var dal = services.GetRequiredService<IItemTemplateDal>();
        var templates = CreateItemTemplates();

        foreach (var template in templates)
        {
            await dal.SaveTemplateAsync(template);
        }
    }

    private async Task SeedEffectDefinitionsAsync(IServiceProvider services)
    {
        var dal = services.GetRequiredService<IEffectDefinitionDal>();
        var definitions = CreateEffectDefinitions();

        foreach (var def in definitions)
        {
            await dal.SaveDefinitionAsync(def);
        }
    }

    private async Task SeedEffectTemplatesAsync(IServiceProvider services)
    {
        var dal = services.GetRequiredService<IEffectTemplateDal>();
        var templates = CreateEffectTemplates();

        foreach (var template in templates)
        {
            await dal.SaveTemplateAsync(template);
        }
    }

    private async Task SeedTablesAsync(IServiceProvider services)
    {
        var dal = services.GetRequiredService<ITableDal>();

        var table = new GameTable
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Name = "Test Adventure",
            GameMasterId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-7),
            LastActivityAt = DateTime.UtcNow.AddHours(-1),
            Status = TableStatus.Active,
            CurrentRound = 5,
            IsInCombat = false
        };

        await dal.SaveTableAsync(table);

        var tableCharacter = new TableCharacter
        {
            TableId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            CharacterId = 1,
            PlayerId = 1,
            JoinedAt = DateTime.UtcNow.AddDays(-7),
            ConnectionStatus = ConnectionStatus.Connected,
            LastActivity = DateTime.UtcNow.AddHours(-1)
        };

        await dal.AddCharacterToTableAsync(tableCharacter);
    }

    private async Task SeedCharactersAsync(IServiceProvider services)
    {
        var dal = services.GetRequiredService<ICharacterDal>();

        var character = new Character
        {
            Id = 1,
            PlayerId = 42,
            Name = "Illiante",
            Species = "Human",
            AttributeList =
            [
                new CharacterAttribute { Name = "STR", BaseValue = 10 },
                new CharacterAttribute { Name = "DEX", BaseValue = 10 },
                new CharacterAttribute { Name = "END", BaseValue = 10 },
                new CharacterAttribute { Name = "INT", BaseValue = 10 },
                new CharacterAttribute { Name = "ITT", BaseValue = 10 },
                new CharacterAttribute { Name = "WIL", BaseValue = 10 },
                new CharacterAttribute { Name = "PHY", BaseValue = 10 },
                new CharacterAttribute { Name = "SOC", BaseValue = 10 }
            ],
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
        };

        await dal.SaveCharacterAsync(character);
    }

    private async Task SeedCharacterItemsAsync(IServiceProvider services)
    {
        var dal = services.GetRequiredService<ICharacterItemDal>();
        var items = CreateCharacterItems();

        foreach (var item in items)
        {
            await dal.AddItemAsync(item);
        }
    }

    #region Data Creation Methods

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
                Id = "hand-to-hand",
                Name = "Hand-to-Hand",
                Category = SkillCategory.Combat,
                IsSpecialized = true,
                Untrained = 6,
                Trained = 3,
                PrimaryAttribute = "STR"
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

    private static List<MagicSchoolDefinition> CreateMagicSchools()
    {
        return
        [
            new MagicSchoolDefinition
            {
                Id = "fire",
                Name = "Fire",
                Description = "Fire magic channels the raw power of flame and heat.",
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
                Description = "Water magic encompasses the fluid nature of water and ice.",
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
                Description = "Light magic harnesses the power of illumination and truth.",
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
                Description = "Life magic taps into the vital force that animates all living things.",
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

    private static List<SpellDefinition> CreateSpellDefinitions()
    {
        return
        [
            new SpellDefinition
            {
                SkillId = "fire-bolt",
                MagicSchool = MagicSchool.Fire,
                SpellType = SpellType.Targeted,
                ManaCost = 1,
                Range = 2,
                ResistanceType = SpellResistanceType.Fixed,
                FixedResistanceTV = 8,
                EffectDescription = "Hurls a bolt of fire at the target, dealing fire damage."
            },
            new SpellDefinition
            {
                SkillId = "illuminate",
                MagicSchool = MagicSchool.Light,
                SpellType = SpellType.SelfBuff,
                ManaCost = 1,
                Range = 0,
                DefaultDuration = 60,
                ResistanceType = SpellResistanceType.None,
                EffectDescription = "Creates a bright light around the caster."
            },
            new SpellDefinition
            {
                SkillId = "heal",
                MagicSchool = MagicSchool.Life,
                SpellType = SpellType.Targeted,
                ManaCost = 1,
                Range = 1,
                ResistanceType = SpellResistanceType.None,
                EffectDescription = "Heals minor wounds, restoring FAT."
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
                Description = "A magical longsword that glows faintly blue.",
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
                CustomProperties = "{\"isRangedWeapon\":true,\"ammoType\":\"Arrow\",\"capacity\":1,\"rangeShort\":20,\"rangeMedium\":40,\"rangeLong\":80,\"rangeExtreme\":120}"
            },

            // === VIRTUAL WEAPONS (Unarmed Combat) ===
            new ItemTemplate
            {
                Id = 15,
                Name = "Punch",
                Description = "A quick strike with the fist. Favors speed over power.",
                ShortDescription = "Unarmed punch attack",
                ItemType = ItemType.Weapon,
                WeaponType = WeaponType.Unarmed,
                EquipmentSlot = EquipmentSlot.None,
                Weight = 0m,
                Volume = 0m,
                Value = 0,
                IsVirtual = true,
                RelatedSkill = "Hand-to-Hand",
                MinSkillLevel = 0,
                DamageClass = 1,
                DamageType = "Bludgeoning",
                SVModifier = 2,
                AVModifier = 0,
                Rarity = ItemRarity.Common,
                Tags = "unarmed,virtual"
            },
            new ItemTemplate
            {
                Id = 16,
                Name = "Kick",
                Description = "A powerful strike with the leg. Trades accuracy for damage.",
                ShortDescription = "Unarmed kick attack",
                ItemType = ItemType.Weapon,
                WeaponType = WeaponType.Unarmed,
                EquipmentSlot = EquipmentSlot.None,
                Weight = 0m,
                Volume = 0m,
                Value = 0,
                IsVirtual = true,
                RelatedSkill = "Hand-to-Hand",
                MinSkillLevel = 0,
                DamageClass = 1,
                DamageType = "Bludgeoning",
                SVModifier = 4,
                AVModifier = -1,
                Rarity = ItemRarity.Common,
                Tags = "unarmed,virtual"
            },

            // === AMMUNITION ===
            new ItemTemplate
            {
                Id = 101,
                Name = "9mm FMJ Rounds",
                Description = "Standard 9mm full metal jacket ammunition.",
                ShortDescription = "9mm ammo",
                ItemType = ItemType.Ammunition,
                Weight = 0.01m,
                Volume = 0.001m,
                Value = 1,
                IsStackable = true,
                MaxStackSize = 100,
                Rarity = ItemRarity.Common,
                Tags = "ammunition",
                CustomProperties = "{\"ammoType\":\"9mm\",\"damageModifier\":0,\"isLooseAmmo\":true}"
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
                CustomProperties = "{\"ammoType\":\"Arrow\",\"damageModifier\":0,\"isLooseAmmo\":true}"
            },
            new ItemTemplate
            {
                Id = 100,
                Name = "9mm Pistol",
                Description = "A reliable semi-automatic pistol chambered in 9mm.",
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
                CustomProperties = "{\"isRangedWeapon\":true,\"ammoType\":\"9mm\",\"capacity\":15,\"rangeShort\":10,\"rangeMedium\":25,\"rangeLong\":50,\"rangeExtreme\":100}"
            },

            // === ARMOR ===
            new ItemTemplate
            {
                Id = 20,
                Name = "Leather Armor",
                Description = "Light armor made of hardened leather.",
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

            // === JEWELRY WITH BONUSES ===
            new ItemTemplate
            {
                Id = 40,
                Name = "Ring of Strength",
                Description = "A magical ring set with a red gem.",
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

            // === TREASURE ===
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

            // === FOOD ===
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
            }
        ];
    }

    private static List<CharacterItem> CreateCharacterItems()
    {
        return
        [
            new CharacterItem
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                ItemTemplateId = 10,
                OwnerCharacterId = 1,
                IsEquipped = true,
                EquippedSlot = EquipmentSlot.MainHand,
                CurrentDurability = 100
            },
            new CharacterItem
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                ItemTemplateId = 20,
                OwnerCharacterId = 1,
                IsEquipped = true,
                EquippedSlot = EquipmentSlot.Chest,
                CurrentDurability = 80
            },
            new CharacterItem
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                ItemTemplateId = 30,
                OwnerCharacterId = 1,
                IsEquipped = true,
                EquippedSlot = EquipmentSlot.Back
            },
            new CharacterItem
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                ItemTemplateId = 50,
                OwnerCharacterId = 1,
                ContainerItemId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                StackSize = 3
            },
            new CharacterItem
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                ItemTemplateId = 80,
                OwnerCharacterId = 1,
                ContainerItemId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                StackSize = 5
            },
            new CharacterItem
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                ItemTemplateId = 71,
                OwnerCharacterId = 1,
                ContainerItemId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                StackSize = 25
            },
            new CharacterItem
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                ItemTemplateId = 100,
                OwnerCharacterId = 1,
                IsEquipped = false,
                CurrentDurability = 100,
                CustomProperties = "{\"loadedAmmo\":10,\"loadedAmmoType\":\"9mm\",\"loadedAmmoDamageModifier\":0}"
            },
            new CharacterItem
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                ItemTemplateId = 103,
                OwnerCharacterId = 1,
                ContainerItemId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                CustomProperties = "{\"ammoType\":\"9mm\",\"capacity\":15,\"currentAmmo\":15,\"ammoDamageModifier\":0}"
            },
            new CharacterItem
            {
                Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                ItemTemplateId = 101,
                OwnerCharacterId = 1,
                ContainerItemId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                StackSize = 30
            },
            new CharacterItem
            {
                Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                ItemTemplateId = 14,
                OwnerCharacterId = 1,
                IsEquipped = false,
                CurrentDurability = 80
            },
            new CharacterItem
            {
                Id = Guid.Parse("bbbbbbbb-cccc-dddd-eeee-ffffffffffff"),
                ItemTemplateId = 104,
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
                IsActive = true,
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 1,
                        EffectDefinitionId = 1,
                        ImpactType = EffectImpactType.ASModifier,
                        Target = "All",
                        Value = -2
                    }
                ]
            },
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
                IsActive = true,
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 10,
                        EffectDefinitionId = 10,
                        ImpactType = EffectImpactType.SpecialAbility,
                        Target = "Actions",
                        Value = 0
                    }
                ]
            },
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
                IsActive = true,
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
            // Test effects for EffectsSystemTests
            new EffectDefinition
            {
                Id = 40,
                Name = "Weak Poison",
                Description = "A mild poison that weakens over time.",
                EffectType = EffectType.Poison,
                TargetType = EffectTargetType.Character,
                DurationType = DurationType.Rounds,
                DefaultDuration = 5,
                IsStackable = false,
                MaxStacks = 5,
                StackBehavior = StackBehavior.Intensify,
                CanBeRemoved = true,
                RemovalMethods = "Antidote,Time",
                IsActive = true,
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 40,
                        EffectDefinitionId = 40,
                        ImpactType = EffectImpactType.DamageOverTime,
                        Target = "Self",
                        Value = 1
                    }
                ]
            },
            new EffectDefinition
            {
                Id = 50,
                Name = "Battle Focus",
                Description = "Enhanced combat awareness and reflexes.",
                EffectType = EffectType.Buff,
                TargetType = EffectTargetType.Character,
                Source = "Skill",
                DurationType = DurationType.Rounds,
                DefaultDuration = 3,
                IsStackable = false,
                StackBehavior = StackBehavior.Replace,
                CanBeRemoved = true,
                IsActive = true,
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 50,
                        EffectDefinitionId = 50,
                        ImpactType = EffectImpactType.AVModifier,
                        Target = "All",
                        Value = 2
                    }
                ]
            },
            new EffectDefinition
            {
                Id = 60,
                Name = "Magic Shield",
                Description = "A protective magical barrier.",
                EffectType = EffectType.Buff,
                TargetType = EffectTargetType.Character,
                Source = "Spell",
                DurationType = DurationType.Minutes,
                DefaultDuration = 10,
                IsStackable = false,
                StackBehavior = StackBehavior.Replace,
                CanBeRemoved = true,
                RemovalMethods = "Dispel,Time",
                IsActive = true,
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 60,
                        EffectDefinitionId = 60,
                        ImpactType = EffectImpactType.TVModifier,
                        Target = "Self",
                        Value = 2
                    }
                ]
            },
            new EffectDefinition
            {
                Id = 70,
                Name = "Combat Drug",
                Description = "A stimulant that enhances combat ability but causes damage when it wears off.",
                EffectType = EffectType.ItemEffect,
                TargetType = EffectTargetType.Character,
                Source = "Item",
                DurationType = DurationType.Rounds,
                DefaultDuration = 5,
                IsStackable = false,
                StackBehavior = StackBehavior.Replace,
                CanBeRemoved = false,
                IsActive = true,
                Impacts =
                [
                    new EffectImpact
                    {
                        Id = 70,
                        EffectDefinitionId = 70,
                        ImpactType = EffectImpactType.ASModifier,
                        Target = "All",
                        Value = 2
                    },
                    new EffectImpact
                    {
                        Id = 71,
                        EffectDefinitionId = 70,
                        ImpactType = EffectImpactType.DamageOverTime,
                        Target = "Self",
                        Value = 3
                    }
                ]
            }
        ];
    }

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
            }
        ];
    }

    #endregion
}
