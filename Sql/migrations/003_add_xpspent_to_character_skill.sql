-- Add XPSpent column to CharacterSkill table to track XP allocation during character creation
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CharacterSkill') AND name = 'XPSpent')
BEGIN
    ALTER TABLE dbo.CharacterSkill
    ADD XPSpent INT NOT NULL DEFAULT 0;
END
GO
