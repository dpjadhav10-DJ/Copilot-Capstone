SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF DB_ID(N'MusafirCafe') IS NULL
BEGIN
    CREATE DATABASE MusafirCafe;
END;
GO

USE MusafirCafe;
GO

IF OBJECT_ID(N'dbo.CafeStory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CafeStory
    (
        CafeStoryId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_CafeStory PRIMARY KEY,
        StoryText nvarchar(max) NOT NULL,
        IsActive bit NOT NULL CONSTRAINT DF_CafeStory_IsActive DEFAULT (0),
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_CafeStory_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt datetime2(0) NOT NULL CONSTRAINT DF_CafeStory_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_CafeStory_StoryText_NotEmpty CHECK (LEN(LTRIM(RTRIM(StoryText))) > 0)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_CafeStory_Active' AND object_id = OBJECT_ID(N'dbo.CafeStory'))
BEGIN
    CREATE UNIQUE INDEX UX_CafeStory_Active
        ON dbo.CafeStory (IsActive)
        WHERE IsActive = 1;
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.CafeStory WHERE IsActive = 1)
BEGIN
    INSERT INTO dbo.CafeStory (StoryText, IsActive)
    VALUES
    (N'This iconic place is dedicated to the "Musafir" (Traveller at heart) within yourself and we encourage you to discuss stories at your heart while enjoying every sip of delicious coffee we serve.

We would love to witness your success and celebrations and we promise to be an encouragement in your lows by listening to your heart over a up of coffee.

We would love to be your a coffee companion...wishing you a memorable coffee today...', 1);
END;
GO

IF OBJECT_ID(N'dbo.MenuItem', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MenuItem
    (
        MenuItemId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_MenuItem PRIMARY KEY,
        ItemName nvarchar(120) NOT NULL,
        Portion nvarchar(20) NOT NULL,
        Price decimal(10,2) NOT NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_MenuItem_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_MenuItem_ItemName_NotEmpty CHECK (LEN(LTRIM(RTRIM(ItemName))) > 0),
        CONSTRAINT CK_MenuItem_Portion CHECK (Portion IN (N'Half', N'Full', N'NA')),
        CONSTRAINT CK_MenuItem_Price_NonNegative CHECK (Price >= 0)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MenuItem_CreatedAt' AND object_id = OBJECT_ID(N'dbo.MenuItem'))
BEGIN
    CREATE INDEX IX_MenuItem_CreatedAt ON dbo.MenuItem (CreatedAt DESC, MenuItemId DESC);
END;
GO

INSERT INTO dbo.MenuItem (ItemName, Portion, Price)
SELECT seed.ItemName, seed.Portion, seed.Price
FROM (VALUES
    (N'Regular Coffee', N'Half', 15.00),
    (N'Regular Coffee', N'Full', 25.00),
    (N'Regular Tea', N'Half', 12.00),
    (N'Regular Tea', N'Full', 20.00),
    (N'Veg Club Sandwitch', N'NA', 50.00),
    (N'French Fries - Regular', N'Half', 60.00),
    (N'French Fries - Regular', N'Full', 90.00),
    (N'Bun Butter', N'NA', 40.00),
    (N'Regular Maggie', N'NA', 50.00),
    (N'Veg Maggie', N'NA', 60.00)
) AS seed(ItemName, Portion, Price)
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.MenuItem existing
    WHERE existing.ItemName = seed.ItemName
      AND existing.Portion = seed.Portion
      AND existing.Price = seed.Price
);
GO
