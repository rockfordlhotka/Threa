-- Item Effects table
-- Active effect instances on items
CREATE TABLE dbo.ItemEffect (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    ItemId UNIQUEIDENTIFIER NOT NULL,
    EffectDefinitionId INT NOT NULL,
    CurrentStacks INT NOT NULL DEFAULT 1,
    StartTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    EndTime DATETIME2 NULL,
    RoundsRemaining INT NULL,
    SourceEntityId UNIQUEIDENTIFIER NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    Notes NVARCHAR(MAX) NULL,
    CustomProperties NVARCHAR(MAX) NULL, -- JSON
    CONSTRAINT FK_ItemEffect_CharacterItem FOREIGN KEY (ItemId) 
        REFERENCES dbo.CharacterItem(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ItemEffect_EffectDefinition FOREIGN KEY (EffectDefinitionId) 
        REFERENCES dbo.EffectDefinition(Id)
);

CREATE INDEX IX_ItemEffect_ItemId ON dbo.ItemEffect(ItemId);
CREATE INDEX IX_ItemEffect_EffectDefinitionId ON dbo.ItemEffect(EffectDefinitionId);
CREATE INDEX IX_ItemEffect_IsActive ON dbo.ItemEffect(IsActive);
