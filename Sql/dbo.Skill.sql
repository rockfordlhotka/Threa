CREATE TABLE [dbo].[Skill]
(
  [Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Category] VARCHAR(MAX) NOT NULL, 
    [Name] VARCHAR(MAX) NOT NULL, 
    [IsSpecialized] BIT,
    [IsMagic] BIT,
    [IsTheology] BIT,
    [IsPsionic] BIT,
    [Trained] INT NOT NULL,
    [Untrained] INT NOT NULL,
    [PrimaryAttribute] VARCHAR(20) NOT NULL,
    [SecondaryAttribute] VARCHAR(20),
    [TertiaryAttribute] VARCHAR(20),
    [ImageUrl] VARCHAR(MAX)
)
