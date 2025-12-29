-- ItemTemplate table: Blueprints for all items in the game world
-- These are the definitions that instances are created from

CREATE TABLE ItemTemplate (
    Id INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(1000) NULL,
    ShortDescription NVARCHAR(100) NULL,
    ItemType INT NOT NULL,           -- 0=Misc, 1=Weapon, 2=Armor, 3=Container, etc.
    WeaponType INT NULL,             -- 0=None, 1=Sword, 2=Axe, etc.
    EquipmentSlot INT NULL,          -- Equipment slot enum value
    Weight DECIMAL(10,4) NOT NULL DEFAULT 0,
    Volume DECIMAL(10,4) NOT NULL DEFAULT 0,
    Value INT NOT NULL DEFAULT 0,    -- Base value in copper pieces
    IsStackable BIT NOT NULL DEFAULT 0,
    MaxStackSize INT NOT NULL DEFAULT 1,
    IsContainer BIT NOT NULL DEFAULT 0,
    ContainerMaxWeight DECIMAL(10,4) NULL,
    ContainerMaxVolume DECIMAL(10,4) NULL,
    ContainerAllowedTypes NVARCHAR(200) NULL,
    ContainerWeightReduction DECIMAL(5,4) NOT NULL DEFAULT 1.0,
    HasDurability BIT NOT NULL DEFAULT 0,
    MaxDurability INT NULL,
    Rarity INT NOT NULL DEFAULT 0,   -- 0=Common, 1=Uncommon, 2=Rare, 3=Epic, 4=Legendary
    IsActive BIT NOT NULL DEFAULT 1,
    RelatedSkill NVARCHAR(50) NULL,
    MinSkillLevel INT NOT NULL DEFAULT 0,
    DamageClass INT NOT NULL DEFAULT 0,
    DamageType NVARCHAR(20) NULL,
    SVModifier INT NOT NULL DEFAULT 0,
    AVModifier INT NOT NULL DEFAULT 0,
    DodgeModifier INT NOT NULL DEFAULT 0,
    Range INT NULL,
    ArmorAbsorption NVARCHAR(500) NULL,  -- JSON for damage type absorption
    CustomProperties NVARCHAR(MAX) NULL,  -- JSON for additional properties
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy INT NULL
);

CREATE INDEX IX_ItemTemplate_ItemType ON ItemTemplate(ItemType);
CREATE INDEX IX_ItemTemplate_Name ON ItemTemplate(Name);
CREATE INDEX IX_ItemTemplate_IsActive ON ItemTemplate(IsActive);
