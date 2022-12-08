IF OBJECT_ID('dbo.[Skill]', 'U') IS NOT NULL
 DROP TABLE dbo.[Skill];
GO

CREATE TABLE [dbo].[Skill]
(
  [Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Category] NVARCHAR(MAX) NOT NULL, 
    [Name] NVARCHAR(MAX) NOT NULL, 
    [IsSpecialized] BIT NOT NULL,
    [IsMagic] BIT NOT NULL,
    [IsTheology] BIT NOT NULL,
    [IsPsionic] BIT NOT NULL,
    [Trained] INT NOT NULL,
    [Untrained] INT NOT NULL,
    [PrimaryAttribute] NVARCHAR(20) NOT NULL,
    [SecondaryAttribute] NVARCHAR(20),
    [TertiaryAttribute] NVARCHAR(20),
    [ImageUrl] NVARCHAR(MAX)
)
GO
