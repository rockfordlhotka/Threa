IF OBJECT_ID('dbo.[Attribute]', 'U') IS NOT NULL
 DROP TABLE dbo.[Attribute];
GO

CREATE TABLE [dbo].[Attribute]
(
  [Id] INT NOT NULL PRIMARY KEY IDENTITY, 
  [Name] NVARCHAR(MAX) NOT NULL,
  [ImageUrl] NVARCHAR(MAX)
)
GO

INSERT INTO [dbo].[Attribute] ([Name]) VALUES ('STR');
INSERT INTO [dbo].[Attribute] ([Name]) VALUES ('DEX');
INSERT INTO [dbo].[Attribute] ([Name]) VALUES ('END');
INSERT INTO [dbo].[Attribute] ([Name]) VALUES ('INT');
INSERT INTO [dbo].[Attribute] ([Name]) VALUES ('ITT');
INSERT INTO [dbo].[Attribute] ([Name]) VALUES ('WIL');
INSERT INTO [dbo].[Attribute] ([Name]) VALUES ('PHY');
INSERT INTO [dbo].[Attribute] ([Name]) VALUES ('SOC');
GO
