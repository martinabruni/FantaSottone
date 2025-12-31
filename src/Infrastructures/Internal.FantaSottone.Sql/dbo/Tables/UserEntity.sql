CREATE TABLE [dbo].[UserEntity] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [Email]     NVARCHAR (255) NOT NULL,
    [CreatedAt] DATETIME2 (3)  DEFAULT (sysutcdatetime()) NOT NULL,
    [UpdatedAt] DATETIME2 (3)  DEFAULT (sysutcdatetime()) NOT NULL,
    [Password]  NVARCHAR (30)     NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_UserEntity_Email]
    ON [dbo].[UserEntity]([Email] ASC);

