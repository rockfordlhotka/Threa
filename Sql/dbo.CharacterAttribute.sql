IF OBJECT_ID('dbo.[CharacterAttribute]', 'U') IS NOT NULL
 DROP TABLE dbo.[CharacterAttribute];
GO

CREATE TABLE [dbo].[CharacterAttribute]
(
    [CharacterId] INT NOT NULL, 
    [AttributeId] INT NOT NULL,
    [BaseValue] INT NOT NULL,
    [Value] INT NOT NULL
);
GO

CREATE CLUSTERED INDEX [CharacterAttributeKey] ON [dbo].[CharacterAttribute]
(
	[CharacterId] ASC
)
GO