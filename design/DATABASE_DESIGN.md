# Threa TTRPG Assistant - Database Design

## Overview

This document outlines the database schema design for the Threa TTRPG Character Sheet Assistant, focusing on the skill-based character progression system.

## Core Entities

### Users & Authentication

```sql
-- User accounts and authentication
Users
- Id (Primary Key)
- Username (Unique)
- Email 
- PasswordHash
- Created
- LastLogin
- IsActive
- Roles (Admin, Player)

-- Character profiles
Characters
- Id (Primary Key)
- UserId (Foreign Key -> Users.Id)
- Name (Unique per user)
- Species (Human, Elf, Dwarf, Halfling, Orc)
- Description
- Created
- LastModified
```

### Attributes System

```sql
-- Character attributes (the 7 core attributes)
CharacterAttributes
- Id (Primary Key)
- CharacterId (Foreign Key -> Characters.Id)
- AttributeName (STR, DEX, END, INT, ITT, WIL, PHY)
- BaseValue (rolled value including species modifiers)
- CurrentValue (may differ due to effects/conditions)
- UNIQUE(CharacterId, AttributeName)
```

### Skills System

```sql
-- Master list of all possible skills
SkillDefinitions
- Id (Primary Key)
- Name (e.g., "Physicality", "Fire Bolt", "Swords")
- SkillType (AttributeSkill, WeaponSkill, SpellSkill, ManaRecoverySkill, CraftingSkill)
- Category (e.g., "Fire Magic", "Melee Weapons")
- Description
- BaseSkillRequired (Foreign Key -> SkillDefinitions.Id, nullable)
- RelatedAttribute (STR, DEX, END, INT, ITT, WIL, PHY)
- BaseCost (usage events for level 0→1)
- Multiplier (progression scaling factor)
- MinimumStartingLevel
- MaximumLevel
- IsStartingSkill (true for the 7 attribute skills)

-- Individual character's skill levels
CharacterSkills
- Id (Primary Key)
- CharacterId (Foreign Key -> Characters.Id)
- SkillDefinitionId (Foreign Key -> SkillDefinitions.Id)
- CurrentLevel (integer skill level)
- UsagePoints (accumulated usage toward next level)
- LastUsed
- UNIQUE(CharacterId, SkillDefinitionId)
```

### Health System

```sql
-- Character health tracking
CharacterHealth
- Id (Primary Key)
- CharacterId (Foreign Key -> Characters.Id)
- MaxFatigue (calculated from END)
- CurrentFatigue
- PendingFatigueDamage
- PendingFatigueHealing
- MaxVitality (calculated from STR + END)
- CurrentVitality
- PendingVitalityDamage
- PendingVitalityHealing
- LastUpdated
- UNIQUE(CharacterId)

-- Wound tracking
CharacterWounds
- Id (Primary Key)
- CharacterId (Foreign Key -> Characters.Id)
- Location (Head, Torso, LeftArm, RightArm, LeftLeg, RightLeg)
- WoundCount
- AcquiredAt
- Notes
```

### Magic & Mana System

```sql
-- Magic schools for mana management
MagicSchools
- Id (Primary Key)
- Name (Fire, Healing, Illusion, etc.)
- Description
- Color (for UI theming)

-- Link spells to their magic schools
SpellSchools
- SpellSkillId (Foreign Key -> SkillDefinitions.Id)
- MagicSchoolId (Foreign Key -> MagicSchools.Id)
- ManaCost
- PRIMARY KEY(SpellSkillId, MagicSchoolId)

-- Character mana pools per school
CharacterMana
- Id (Primary Key)
- CharacterId (Foreign Key -> Characters.Id)
- MagicSchoolId (Foreign Key -> MagicSchools.Id)
- CurrentMana
- MaximumMana
- RecoveryRate (mana per hour)
- LastRecoveryUpdate
- UNIQUE(CharacterId, MagicSchoolId)
```

### Effects System

```sql
-- Effect definitions (buffs, debuffs, conditions)
EffectDefinitions
- Id (Primary Key)
- Name (e.g., "Poison", "Strength Boost", "Paralysis")
- Description
- EffectType (Buff, Debuff, Condition, Disease)
- Duration (minutes, 0 = permanent until removed)
- IsStackable
- MaxStacks
- CreatedBy (Foreign Key -> Users.Id)
- IsActive

-- Effect impacts on character attributes and skills
EffectImpacts
- Id (Primary Key)
- EffectDefinitionId (Foreign Key -> EffectDefinitions.Id)
- ImpactType (SkillBonus, SkillPenalty, AttributeBonus, AttributePenalty, etc.)
- TargetSkillId (Foreign Key -> SkillDefinitions.Id, nullable)
- TargetAttribute (nullable: STR, DEX, END, INT, ITT, WIL, PHY)
- ImpactValue
- IsPercentage

-- Active effects on characters
CharacterEffects
- Id (Primary Key)
- CharacterId (Foreign Key -> Characters.Id)
- EffectDefinitionId (Foreign Key -> EffectDefinitions.Id)
- SourceType (Spell, Item, Environmental)
- SourceName
- StackCount
- StartTime
- EndTime (nullable for permanent effects)
- IsActive
```

### Items & Equipment

```sql
-- Template definitions for items (the "blueprint" for creating item instances)
ItemTemplates
- Id (Primary Key)
- Name
- Description
- ShortDescription
- ItemType (Weapon, Armor, Container, Consumable, Treasure, Key, Magic, Food, Drink, Tool, Miscellaneous)
- WeaponType (Sword, Axe, Mace, Polearm, Bow, Crossbow, Dagger, Staff, Wand) -- nullable
- ArmorSlot (Head, Neck, Shoulders, Chest, Back, Wrists, Hands, Waist, Legs, Feet, Fingers, MainHand, OffHand, TwoHand) -- nullable
- Weight (decimal, in pounds)
- Volume (decimal, in cubic feet)
- Value (in copper pieces)
- IsStackable
- MaxStackSize
- IsContainer
- ContainerMaxWeight -- nullable
- ContainerMaxVolume -- nullable
- ContainerAllowedTypes -- nullable
- ContainerWeightReduction (default 1.0) -- nullable
- HasDurability
- MaxDurability -- nullable
- Rarity (Common, Uncommon, Rare, Epic, Legendary)
- IsActive
- CreatedAt
- CreatedBy (Foreign Key -> Users.Id)
- CustomProperties (JSON)

-- Actual item instances owned by characters
Items
- Id (Primary Key, GUID)
- ItemTemplateId (Foreign Key -> ItemTemplates.Id)
- OwnerCharacterId (Foreign Key -> Characters.Id)
- ContainerItemId (Foreign Key -> Items.Id, nullable)
- EquippedSlot (ArmorSlot enum, nullable)
- StackSize
- CurrentDurability -- nullable
- IsEquipped
- CustomName -- nullable
- CreatedAt
- CustomProperties (JSON)

-- Item effects on skills
ItemSkillBonuses
- Id (Primary Key)
- ItemTemplateId (Foreign Key -> ItemTemplates.Id)
- SkillDefinitionId (Foreign Key -> SkillDefinitions.Id)
- BonusType (FlatBonus, PercentageBonus, CooldownReduction)
- BonusValue
- Condition -- nullable

-- Item effects on attributes
ItemAttributeModifiers
- Id (Primary Key)
- ItemTemplateId (Foreign Key -> ItemTemplates.Id)
- AttributeName (STR, DEX, END, INT, ITT, WIL, PHY)
- ModifierType (FlatBonus, PercentageBonus)
- ModifierValue
- Condition -- nullable

-- Character inventory capacity tracking
CharacterInventory
- CharacterId (Primary Key, Foreign Key -> Characters.Id)
- MaxWeight (based on Physicality)
- MaxVolume (based on Physicality)
- LastCalculatedAt
```

### Currency

```sql
-- Character currency tracking
CharacterCurrency
- CharacterId (Primary Key, Foreign Key -> Characters.Id)
- CopperCoins (integer)
- SilverCoins (integer)
- GoldCoins (integer)
- PlatinumCoins (integer)
```

## Indexes for Performance

```sql
-- Critical indexes for application performance
CREATE INDEX IX_Characters_UserId ON Characters(UserId);

CREATE INDEX IX_CharacterSkills_CharacterId ON CharacterSkills(CharacterId);
CREATE INDEX IX_CharacterSkills_SkillDefinitionId ON CharacterSkills(SkillDefinitionId);

CREATE INDEX IX_CharacterEffects_CharacterId ON CharacterEffects(CharacterId);
CREATE INDEX IX_CharacterEffects_IsActive ON CharacterEffects(IsActive);

CREATE INDEX IX_Items_OwnerCharacterId ON Items(OwnerCharacterId);
CREATE INDEX IX_Items_ContainerItemId ON Items(ContainerItemId);
CREATE INDEX IX_Items_IsEquipped ON Items(IsEquipped);

CREATE INDEX IX_ItemSkillBonuses_ItemTemplateId ON ItemSkillBonuses(ItemTemplateId);
CREATE INDEX IX_ItemAttributeModifiers_ItemTemplateId ON ItemAttributeModifiers(ItemTemplateId);
```

## Key Design Decisions

### Practice-Based Progression

- **No Traditional Levels**: Characters advance individual skills through use
- **Usage Tracking**: All skill usage is logged for progression calculations
- **Fractional Progress**: UsagePoints track progress toward next level

### Character Health System

- **Dual Health Pools**: Fatigue (stamina) and Vitality (life force)
- **Pending Damage**: Damage accumulates before being applied
- **Wound Tracking**: Long-term injuries tracked separately

### Flexible Skill System

- **Skill Definitions**: All skills are data-driven, not hard-coded
- **Skill Dependencies**: Advanced skills can require prerequisite skills
- **Starting Skills**: All characters begin with the 7 attribute skills

### Item System

- **Template Pattern**: ItemTemplates define blueprints, Items are instances
- **Container Support**: Items can contain other items (bags, backpacks)
- **Bonus Stacking**: Equipment bonuses stack and cascade through attributes

### Inventory Capacity

- Base carrying capacity uses exponential scaling: 50 lbs × (1.15 ^ (Physicality - 10))
- Base volume capacity uses exponential scaling: 10 cu.ft. × (1.15 ^ (Physicality - 10))
- Containers can increase effective capacity
- Magical containers can reduce effective weight of contained items

## Entity Framework Considerations

### Migrations Strategy

- Use EF Core migrations for database versioning
- Seed data for initial skill definitions
- Backup strategy for user data

### Relationships

- Configure cascade delete carefully to prevent accidental data loss
- Use navigation properties for efficient querying
- Consider lazy loading vs. explicit loading for performance

---

*This database design supports the core character management features while maintaining flexibility for future enhancements.*
