-- ItemSkillBonus table: Skill bonuses provided by items
-- Links ItemTemplates to the skills they boost

CREATE TABLE ItemSkillBonus (
    Id INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    ItemTemplateId INT NOT NULL,
    SkillName NVARCHAR(50) NOT NULL,
    BonusType INT NOT NULL,          -- 0=FlatBonus, 1=PercentageBonus, 2=CooldownReduction
    BonusValue DECIMAL(10,4) NOT NULL,
    Condition NVARCHAR(100) NULL,    -- Optional condition (e.g., "daytime", "night")
    CONSTRAINT FK_ItemSkillBonus_ItemTemplate FOREIGN KEY (ItemTemplateId) 
        REFERENCES ItemTemplate(Id) ON DELETE CASCADE
);

CREATE INDEX IX_ItemSkillBonus_ItemTemplateId ON ItemSkillBonus(ItemTemplateId);
