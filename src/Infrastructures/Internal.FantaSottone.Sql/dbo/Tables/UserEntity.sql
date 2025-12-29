CREATE TABLE [dbo].[UserEntity] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [Email]     NVARCHAR (100) NULL,
    [CreatedAt] DATETIME2 (3)  DEFAULT (sysutcdatetime()) NOT NULL,
    [UpdatedAt] DATETIME2 (3)  DEFAULT (sysutcdatetime()) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

