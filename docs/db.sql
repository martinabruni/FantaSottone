/****** Object:  Database [sqldb-fantastn-webapp-dev]    Script Date: 12/28/2025 6:12:51 PM ******/
CREATE DATABASE [sqldb-fantastn-webapp-dev]  
GO
CREATE TABLE [dbo].[GameEntity](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[InitialScore] [int] NOT NULL,
	[Status] [tinyint] NOT NULL,
	[CreatorPlayerId] [int] NULL,
	[WinnerPlayerId] [int] NULL,
	[CreatedAt] [datetime2](3) NOT NULL,
	[UpdatedAt] [datetime2](3) NOT NULL,
 CONSTRAINT [PK_GameEntity] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PlayerEntity]    Script Date: 12/28/2025 6:12:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PlayerEntity](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GameId] [int] NULL,
	[IsCreator] [bit] NOT NULL,
	[CurrentScore] [int] NOT NULL,
	[CreatedAt] [datetime2](3) NOT NULL,
	[UpdatedAt] [datetime2](3) NOT NULL,
	[UserId] [int] NOT NULL,
 CONSTRAINT [PK_PlayerEntity] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RuleAssignmentEntity]    Script Date: 12/28/2025 6:12:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RuleAssignmentEntity](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RuleId] [int] NOT NULL,
	[GameId] [int] NOT NULL,
	[AssignedToPlayerId] [int] NOT NULL,
	[ScoreDeltaApplied] [int] NOT NULL,
	[AssignedAt] [datetime2](3) NOT NULL,
	[CreatedAt] [datetime2](3) NOT NULL,
	[UpdatedAt] [datetime2](3) NOT NULL,
 CONSTRAINT [PK_RuleAssignmentEntity] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RuleEntity]    Script Date: 12/28/2025 6:12:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RuleEntity](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GameId] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[RuleType] [tinyint] NOT NULL,
	[ScoreDelta] [int] NOT NULL,
	[CreatedAt] [datetime2](3) NOT NULL,
	[UpdatedAt] [datetime2](3) NOT NULL,
 CONSTRAINT [PK_RuleEntity] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserEntity]    Script Date: 12/28/2025 6:12:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserEntity](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](50) NOT NULL,
	[PasswordHash] [nvarchar](255) NOT NULL,
	[Email] [nvarchar](100) NULL,
	[CreatedAt] [datetime2](3) NOT NULL,
	[UpdatedAt] [datetime2](3) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [IX_GameEntity_Status]    Script Date: 12/28/2025 6:12:52 PM ******/
CREATE NONCLUSTERED INDEX [IX_GameEntity_Status] ON [dbo].[GameEntity]
(
	[Status] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PlayerEntity_GameId_CurrentScore]    Script Date: 12/28/2025 6:12:52 PM ******/
CREATE NONCLUSTERED INDEX [IX_PlayerEntity_GameId_CurrentScore] ON [dbo].[PlayerEntity]
(
	[GameId] ASC,
	[CurrentScore] DESC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_PlayerEntity_GameId_UserId]    Script Date: 12/28/2025 6:12:52 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_PlayerEntity_GameId_UserId] ON [dbo].[PlayerEntity]
(
	[GameId] ASC,
	[UserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RuleAssignmentEntity_GameId_AssignedAt]    Script Date: 12/28/2025 6:12:52 PM ******/
CREATE NONCLUSTERED INDEX [IX_RuleAssignmentEntity_GameId_AssignedAt] ON [dbo].[RuleAssignmentEntity]
(
	[GameId] ASC,
	[AssignedAt] DESC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RuleAssignmentEntity_GameId_RuleId]    Script Date: 12/28/2025 6:12:52 PM ******/
CREATE NONCLUSTERED INDEX [IX_RuleAssignmentEntity_GameId_RuleId] ON [dbo].[RuleAssignmentEntity]
(
	[GameId] ASC,
	[RuleId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_RuleAssignmentEntity_RuleId]    Script Date: 12/28/2025 6:12:52 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_RuleAssignmentEntity_RuleId] ON [dbo].[RuleAssignmentEntity]
(
	[RuleId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RuleEntity_GameId]    Script Date: 12/28/2025 6:12:52 PM ******/
CREATE NONCLUSTERED INDEX [IX_RuleEntity_GameId] ON [dbo].[RuleEntity]
(
	[GameId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_RuleEntity_GameId_Name]    Script Date: 12/28/2025 6:12:52 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_RuleEntity_GameId_Name] ON [dbo].[RuleEntity]
(
	[GameId] ASC,
	[Name] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_UserEntity_Username]    Script Date: 12/28/2025 6:12:52 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_UserEntity_Username] ON [dbo].[UserEntity]
(
	[Username] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[GameEntity] ADD  CONSTRAINT [DF_GameEntity_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[GameEntity] ADD  CONSTRAINT [DF_GameEntity_UpdatedAt]  DEFAULT (sysutcdatetime()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[PlayerEntity] ADD  CONSTRAINT [DF_PlayerEntity_IsCreator]  DEFAULT ((0)) FOR [IsCreator]
GO
ALTER TABLE [dbo].[PlayerEntity] ADD  CONSTRAINT [DF_PlayerEntity_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[PlayerEntity] ADD  CONSTRAINT [DF_PlayerEntity_UpdatedAt]  DEFAULT (sysutcdatetime()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[RuleAssignmentEntity] ADD  CONSTRAINT [DF_RuleAssignmentEntity_AssignedAt]  DEFAULT (sysutcdatetime()) FOR [AssignedAt]
GO
ALTER TABLE [dbo].[RuleAssignmentEntity] ADD  CONSTRAINT [DF_RuleAssignmentEntity_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[RuleAssignmentEntity] ADD  CONSTRAINT [DF_RuleAssignmentEntity_UpdatedAt]  DEFAULT (sysutcdatetime()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[RuleEntity] ADD  CONSTRAINT [DF_RuleEntity_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[RuleEntity] ADD  CONSTRAINT [DF_RuleEntity_UpdatedAt]  DEFAULT (sysutcdatetime()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[UserEntity] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[UserEntity] ADD  DEFAULT (sysutcdatetime()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[GameEntity]  WITH CHECK ADD  CONSTRAINT [FK_GameEntity_CreatorPlayer] FOREIGN KEY([CreatorPlayerId])
REFERENCES [dbo].[PlayerEntity] ([Id])
GO
ALTER TABLE [dbo].[GameEntity] CHECK CONSTRAINT [FK_GameEntity_CreatorPlayer]
GO
ALTER TABLE [dbo].[GameEntity]  WITH CHECK ADD  CONSTRAINT [FK_GameEntity_WinnerPlayer] FOREIGN KEY([WinnerPlayerId])
REFERENCES [dbo].[PlayerEntity] ([Id])
GO
ALTER TABLE [dbo].[GameEntity] CHECK CONSTRAINT [FK_GameEntity_WinnerPlayer]
GO
ALTER TABLE [dbo].[PlayerEntity]  WITH CHECK ADD  CONSTRAINT [FK_PlayerEntity_GameEntity] FOREIGN KEY([GameId])
REFERENCES [dbo].[GameEntity] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PlayerEntity] CHECK CONSTRAINT [FK_PlayerEntity_GameEntity]
GO
ALTER TABLE [dbo].[PlayerEntity]  WITH CHECK ADD  CONSTRAINT [FK_PlayerEntity_UserEntity] FOREIGN KEY([UserId])
REFERENCES [dbo].[UserEntity] ([Id])
GO
ALTER TABLE [dbo].[PlayerEntity] CHECK CONSTRAINT [FK_PlayerEntity_UserEntity]
GO
ALTER TABLE [dbo].[RuleAssignmentEntity]  WITH CHECK ADD  CONSTRAINT [FK_RuleAssignmentEntity_GameEntity] FOREIGN KEY([GameId])
REFERENCES [dbo].[GameEntity] ([Id])
GO
ALTER TABLE [dbo].[RuleAssignmentEntity] CHECK CONSTRAINT [FK_RuleAssignmentEntity_GameEntity]
GO
ALTER TABLE [dbo].[RuleAssignmentEntity]  WITH CHECK ADD  CONSTRAINT [FK_RuleAssignmentEntity_PlayerEntity] FOREIGN KEY([AssignedToPlayerId])
REFERENCES [dbo].[PlayerEntity] ([Id])
GO
ALTER TABLE [dbo].[RuleAssignmentEntity] CHECK CONSTRAINT [FK_RuleAssignmentEntity_PlayerEntity]
GO
ALTER TABLE [dbo].[RuleAssignmentEntity]  WITH CHECK ADD  CONSTRAINT [FK_RuleAssignmentEntity_RuleEntity] FOREIGN KEY([RuleId])
REFERENCES [dbo].[RuleEntity] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RuleAssignmentEntity] CHECK CONSTRAINT [FK_RuleAssignmentEntity_RuleEntity]
GO
ALTER TABLE [dbo].[RuleEntity]  WITH CHECK ADD  CONSTRAINT [FK_RuleEntity_GameEntity] FOREIGN KEY([GameId])
REFERENCES [dbo].[GameEntity] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RuleEntity] CHECK CONSTRAINT [FK_RuleEntity_GameEntity]
GO
ALTER TABLE [dbo].[GameEntity]  WITH CHECK ADD  CONSTRAINT [CK_GameEntity_Status] CHECK  (([Status]=(3) OR [Status]=(2) OR [Status]=(1)))
GO
ALTER TABLE [dbo].[GameEntity] CHECK CONSTRAINT [CK_GameEntity_Status]
GO
ALTER TABLE [dbo].[RuleEntity]  WITH CHECK ADD  CONSTRAINT [CK_RuleEntity_RuleType] CHECK  (([RuleType]=(2) OR [RuleType]=(1)))
GO
ALTER TABLE [dbo].[RuleEntity] CHECK CONSTRAINT [CK_RuleEntity_RuleType]
GO
ALTER DATABASE [sqldb-fantastn-webapp-dev] SET  READ_WRITE 
GO
