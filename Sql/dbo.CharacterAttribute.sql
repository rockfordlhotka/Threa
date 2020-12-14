CREATE TABLE [dbo].[CharacterAttribute]
(
  [CharacterId] INT NOT NULL, 
  [AttributeId] INT NOT NULL
)
GO

CREATE CLUSTERED INDEX [CharacterAttributeKey] ON [dbo].[CharacterAttribute]
(
	[CharacterId] ASC
)
GO