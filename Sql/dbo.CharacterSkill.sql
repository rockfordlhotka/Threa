CREATE TABLE [dbo].[CharacterSkill]
(
  [CharacterId] INT NOT NULL, 
  [SkillId] INT NOT NULL,
  [Level] INT NOT NULL,
  [XPBanked] FLOAT NOT NULL
)
GO

CREATE CLUSTERED INDEX [ClusteredIndex-20201213-145744] ON [dbo].[CharacterSkill]
(
	[CharacterId] ASC
)
GO