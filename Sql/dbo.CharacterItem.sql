-- CharacterItem table: Actual item instances owned by characters
-- These are created from ItemTemplates and can have instance-specific data

CREATE TABLE CharacterItem (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    ItemTemplateId INT NOT NULL,
    OwnerCharacterId INT NOT NULL,
    ContainerItemId UNIQUEIDENTIFIER NULL,   -- FK to CharacterItem.Id if inside a container
    EquippedSlot INT NULL,                   -- Equipment slot enum value
    IsEquipped BIT NOT NULL DEFAULT 0,
    StackSize INT NOT NULL DEFAULT 1,
    CurrentDurability INT NULL,
    CustomName NVARCHAR(100) NULL,
    CustomProperties NVARCHAR(MAX) NULL,     -- JSON for additional properties
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_CharacterItem_ItemTemplate FOREIGN KEY (ItemTemplateId) 
        REFERENCES ItemTemplate(Id),
    CONSTRAINT FK_CharacterItem_Character FOREIGN KEY (OwnerCharacterId) 
        REFERENCES Character(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CharacterItem_Container FOREIGN KEY (ContainerItemId) 
        REFERENCES CharacterItem(Id)
);

CREATE INDEX IX_CharacterItem_OwnerCharacterId ON CharacterItem(OwnerCharacterId);
CREATE INDEX IX_CharacterItem_ContainerItemId ON CharacterItem(ContainerItemId);
CREATE INDEX IX_CharacterItem_IsEquipped ON CharacterItem(IsEquipped);
CREATE INDEX IX_CharacterItem_ItemTemplateId ON CharacterItem(ItemTemplateId);
