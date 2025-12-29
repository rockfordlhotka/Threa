-- Effect Definitions table
-- Templates/blueprints for all effects in the game
CREATE TABLE dbo.EffectDefinition (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    EffectType INT NOT NULL, -- EffectType enum
    TargetType INT NOT NULL DEFAULT 0, -- EffectTargetType enum
    Source NVARCHAR(100) NULL,
    IconName NVARCHAR(100) NULL,
    DurationType INT NOT NULL, -- DurationType enum
    DefaultDuration INT NOT NULL DEFAULT 0,
    IsStackable BIT NOT NULL DEFAULT 0,
    MaxStacks INT NOT NULL DEFAULT 1,
    StackBehavior INT NOT NULL DEFAULT 0, -- StackBehavior enum
    CanBeRemoved BIT NOT NULL DEFAULT 1,
    RemovalMethods NVARCHAR(500) NULL, -- Comma-separated list
    RemovalDifficulty INT NOT NULL DEFAULT 0,
    WoundLocation NVARCHAR(50) NULL,
    BreakConditions NVARCHAR(500) NULL, -- Comma-separated list
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_EffectDefinition_Name ON dbo.EffectDefinition(Name);
CREATE INDEX IX_EffectDefinition_EffectType ON dbo.EffectDefinition(EffectType);
CREATE INDEX IX_EffectDefinition_IsActive ON dbo.EffectDefinition(IsActive);

-- Effect Impacts table
-- Individual impacts/modifiers that an effect applies
CREATE TABLE dbo.EffectImpact (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    EffectDefinitionId INT NOT NULL,
    ImpactType INT NOT NULL, -- EffectImpactType enum
    Target NVARCHAR(100) NOT NULL, -- Attribute name, skill name, "All", etc.
    Value DECIMAL(10,2) NOT NULL,
    IsPercentage BIT NOT NULL DEFAULT 0,
    DamageInterval INT NULL, -- For DamageOverTime effects
    Condition NVARCHAR(200) NULL,
    CONSTRAINT FK_EffectImpact_EffectDefinition FOREIGN KEY (EffectDefinitionId) 
        REFERENCES dbo.EffectDefinition(Id) ON DELETE CASCADE
);

CREATE INDEX IX_EffectImpact_EffectDefinitionId ON dbo.EffectImpact(EffectDefinitionId);
