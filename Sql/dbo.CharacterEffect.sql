-- Character Effects table
-- Active effect instances on characters
CREATE TABLE dbo.CharacterEffect (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CharacterId INT NOT NULL,
    EffectDefinitionId INT NOT NULL,
    CurrentStacks INT NOT NULL DEFAULT 1,
    StartTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    EndTime DATETIME2 NULL,
    RoundsRemaining INT NULL,  -- OBSOLETE: Use epoch-based expiration
    RoundsUntilTick INT NULL,
    CreatedAtEpochSeconds BIGINT NULL,  -- Game time when effect was created
    ExpiresAtEpochSeconds BIGINT NULL,  -- Game time when effect expires (NULL = permanent)
    SourceEntityId UNIQUEIDENTIFIER NULL,
    SourceItemId UNIQUEIDENTIFIER NULL,
    WoundLocation NVARCHAR(50) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    Notes NVARCHAR(MAX) NULL,
    CustomProperties NVARCHAR(MAX) NULL, -- JSON
    CONSTRAINT FK_CharacterEffect_Character FOREIGN KEY (CharacterId)
        REFERENCES dbo.Character(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CharacterEffect_EffectDefinition FOREIGN KEY (EffectDefinitionId)
        REFERENCES dbo.EffectDefinition(Id)
);

CREATE INDEX IX_CharacterEffect_CharacterId ON dbo.CharacterEffect(CharacterId);
CREATE INDEX IX_CharacterEffect_EffectDefinitionId ON dbo.CharacterEffect(EffectDefinitionId);
CREATE INDEX IX_CharacterEffect_IsActive ON dbo.CharacterEffect(IsActive);
CREATE INDEX IX_CharacterEffect_EndTime ON dbo.CharacterEffect(EndTime) WHERE EndTime IS NOT NULL;
CREATE INDEX IX_CharacterEffect_ExpiresAtEpochSeconds ON dbo.CharacterEffect(ExpiresAtEpochSeconds) WHERE ExpiresAtEpochSeconds IS NOT NULL;
