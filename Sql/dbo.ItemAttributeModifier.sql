-- ItemAttributeModifier table: Attribute modifiers provided by items
-- Links ItemTemplates to the attributes they modify

CREATE TABLE ItemAttributeModifier (
    Id INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    ItemTemplateId INT NOT NULL,
    AttributeName NVARCHAR(10) NOT NULL,  -- STR, DEX, END, INT, ITT, WIL, PHY
    ModifierType INT NOT NULL,             -- 0=FlatBonus, 1=PercentageBonus
    ModifierValue DECIMAL(10,4) NOT NULL,
    Condition NVARCHAR(100) NULL,          -- Optional condition
    CONSTRAINT FK_ItemAttributeModifier_ItemTemplate FOREIGN KEY (ItemTemplateId) 
        REFERENCES ItemTemplate(Id) ON DELETE CASCADE
);

CREATE INDEX IX_ItemAttributeModifier_ItemTemplateId ON ItemAttributeModifier(ItemTemplateId);
