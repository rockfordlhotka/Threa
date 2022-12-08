IF OBJECT_ID('dbo.[Character]', 'U') IS NOT NULL
 DROP TABLE dbo.[Character];
GO

CREATE TABLE [dbo].[Character]
(
  [Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [PlayerId] INT NOT NULL,
    [Name] NVARCHAR(MAX) NOT NULL, 
    [TrueName] NVARCHAR(MAX), 
    [Aliases] NVARCHAR(MAX), 
    [Species] NVARCHAR(MAX), 
    [DamageClass] INT, 
    [Height] NVARCHAR(15), 
    [Weight] NVARCHAR(15), 
    [Notes] NVARCHAR(MAX), 
    [SkinDescription] NVARCHAR(MAX), 
    [HairDescription] NVARCHAR(MAX), 
    [Description] NVARCHAR(MAX), 
    [Birthdate] BIGINT, 
    [XPTotal] FLOAT, 
    [XPBanked] FLOAT, 
    [IsPlayable] BIT NOT NULL,
    [ActionPointMax] INT,
    [ActionPointRecovery] INT,
    [ActionPointAvailable] INT,
    [IsPassedOut] BIT NOT NULL, 
    [VitValue] INT,
    [VitBaseValue] INT,
    [VitPendingHealing] INT,
    [VitPendingDamage] INT,
    [FatValue] INT,
    [FatBaseValue] INT,
    [FatPendingHealing] INT,
    [FatPendingDamage] INT,
    [ImageUrl] NVARCHAR(MAX) NOT NULL
)
GO
