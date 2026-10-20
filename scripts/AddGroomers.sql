BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE TABLE [Groomers] (
        [Id] bigint NOT NULL IDENTITY,
        [Name] nvarchar(200) NULL,
        [ProviderId] bigint NULL,
        [OwnerId] bigint NOT NULL,
        [LegacyCompanionId] bigint NULL,
        [IsPersonal] bit NOT NULL,
        [ReferralCode] nvarchar(10) NULL,
        [PictureId] bigint NULL,
        [BackgroundPictureId] bigint NULL,
        [IconId] bigint NULL,
        [Phone] nvarchar(40) NULL,
        [AddressValue] nvarchar(max) NULL,
        [StateId] bigint NULL,
        [CityId] bigint NOT NULL,
        [NeighborhoodId] bigint NULL,
        [Location] geography NULL,
        [Active] bit NOT NULL,
        [ShowToSite] bit NOT NULL,
        [Approved] bit NOT NULL,
        [ActivationValue] nvarchar(max) NULL,
        [Suggested] bit NOT NULL,
        [Deleted] bit NOT NULL,
        [DeleteDate] datetime2 NULL,
        [DeletedByUserId] bigint NULL,
        [CreateDate] datetime2 NOT NULL,
        [CommentCount] int NOT NULL,
        [RateAvg] float NOT NULL,
        [RateCount] int NOT NULL,
        [SearchKey] nvarchar(max) NULL,
        [GoldAccountDate] datetime2 NULL,
        [SilverAccountDate] datetime2 NULL,
        [Description] nvarchar(max) NULL,
        [Regulations] nvarchar(max) NULL,
        [EstablishedYear] int NULL,
        [LicenseNumber] nvarchar(100) NULL,
        [LicenseApproved] bit NOT NULL,
        [InstagramUrl] nvarchar(300) NULL,
        [WebsiteUrl] nvarchar(300) NULL,
        [ServiceAtHome] bit NOT NULL,
        [ServiceInSalon] bit NOT NULL,
        [PickupAvailable] bit NOT NULL,
        [PickupFee] float NOT NULL,
        [HasParking] bit NOT NULL,
        [HasWaitingRoom] bit NOT NULL,
        [HasCamera] bit NOT NULL,
        [MaxConcurrentPets] int NOT NULL,
        [StaffCount] int NOT NULL,
        [PortfolioCount] int NOT NULL,
        [PortfolioLikeCount] int NOT NULL,
        [CommissionPercent] decimal(5,2) NOT NULL,
        [Summary] nvarchar(max) NULL,
        [SeoH1] nvarchar(max) NULL,
        [SeoMinDescription] nvarchar(max) NULL,
        [SeoDescription] nvarchar(max) NULL,
        [SeoTitle] nvarchar(max) NULL,
        [SeoPictureAlt] nvarchar(max) NULL,
        [SeoUrlText] nvarchar(max) NULL,
        [SeoNoIndex] bit NOT NULL,
        [SeoNoFollow] bit NOT NULL,
        [SeoCanonical] nvarchar(max) NULL,
        CONSTRAINT [PK_Groomers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Groomers_Cities_CityId] FOREIGN KEY ([CityId]) REFERENCES [Cities] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Groomers_Neighborhoods_NeighborhoodId] FOREIGN KEY ([NeighborhoodId]) REFERENCES [Neighborhoods] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Groomers_Pictures_BackgroundPictureId] FOREIGN KEY ([BackgroundPictureId]) REFERENCES [Pictures] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Groomers_Pictures_IconId] FOREIGN KEY ([IconId]) REFERENCES [Pictures] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Groomers_Pictures_PictureId] FOREIGN KEY ([PictureId]) REFERENCES [Pictures] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Groomers_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Groomers_States_StateId] FOREIGN KEY ([StateId]) REFERENCES [States] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Groomers_Users_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE TABLE [GroomerComments] (
        [Id] bigint NOT NULL,
        [GroomerId] bigint NOT NULL,
        CONSTRAINT [PK_GroomerComments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GroomerComments_Comments_Id] FOREIGN KEY ([Id]) REFERENCES [Comments] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_GroomerComments_Groomers_GroomerId] FOREIGN KEY ([GroomerId]) REFERENCES [Groomers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE TABLE [GroomerPets] (
        [Id] bigint NOT NULL IDENTITY,
        [GroomerId] bigint NOT NULL,
        [PetId] bigint NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_GroomerPets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GroomerPets_Groomers_GroomerId] FOREIGN KEY ([GroomerId]) REFERENCES [Groomers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerPets_Pets_PetId] FOREIGN KEY ([PetId]) REFERENCES [Pets] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE TABLE [GroomerPictures] (
        [Id] bigint NOT NULL IDENTITY,
        [GroomerId] bigint NOT NULL,
        [PictureId] bigint NOT NULL,
        [Label] nvarchar(max) NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_GroomerPictures] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GroomerPictures_Groomers_GroomerId] FOREIGN KEY ([GroomerId]) REFERENCES [Groomers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerPictures_Pictures_PictureId] FOREIGN KEY ([PictureId]) REFERENCES [Pictures] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE TABLE [GroomerServices] (
        [Id] bigint NOT NULL IDENTITY,
        [GroomerId] bigint NOT NULL,
        [AssistanceId] bigint NOT NULL,
        [Active] bit NOT NULL,
        [Approved] bit NOT NULL,
        [ActivationValue] nvarchar(max) NULL,
        [Deleted] bit NOT NULL,
        [CommissionPercent] decimal(5,2) NOT NULL,
        [DurationMinutes] int NOT NULL,
        [Description] nvarchar(max) NULL,
        CONSTRAINT [PK_GroomerServices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GroomerServices_Assistances_AssistanceId] FOREIGN KEY ([AssistanceId]) REFERENCES [Assistances] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerServices_Groomers_GroomerId] FOREIGN KEY ([GroomerId]) REFERENCES [Groomers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE TABLE [GroomerTimes] (
        [Id] bigint NOT NULL IDENTITY,
        [GroomerId] bigint NOT NULL,
        [WeekDayId] bigint NOT NULL,
        [StartTime] nvarchar(10) NULL,
        [EndTime] nvarchar(10) NULL,
        [Capacity] int NOT NULL,
        [Active] bit NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_GroomerTimes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GroomerTimes_Groomers_GroomerId] FOREIGN KEY ([GroomerId]) REFERENCES [Groomers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerTimes_WeekDays_WeekDayId] FOREIGN KEY ([WeekDayId]) REFERENCES [WeekDays] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE TABLE [GroomerUsers] (
        [Id] bigint NOT NULL IDENTITY,
        [GroomerId] bigint NOT NULL,
        [UserId] bigint NOT NULL,
        [UserAccept] bit NULL,
        [Active] bit NOT NULL,
        [ActivationValue] nvarchar(max) NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_GroomerUsers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GroomerUsers_Groomers_GroomerId] FOREIGN KEY ([GroomerId]) REFERENCES [Groomers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerUsers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE TABLE [GroomerZones] (
        [Id] bigint NOT NULL IDENTITY,
        [GroomerId] bigint NOT NULL,
        [StateId] bigint NOT NULL,
        [CityId] bigint NOT NULL,
        [NeighborhoodId] bigint NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_GroomerZones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GroomerZones_Cities_CityId] FOREIGN KEY ([CityId]) REFERENCES [Cities] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerZones_Groomers_GroomerId] FOREIGN KEY ([GroomerId]) REFERENCES [Groomers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerZones_Neighborhoods_NeighborhoodId] FOREIGN KEY ([NeighborhoodId]) REFERENCES [Neighborhoods] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerZones_States_StateId] FOREIGN KEY ([StateId]) REFERENCES [States] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE TABLE [GroomerPortfolioPosts] (
        [Id] bigint NOT NULL IDENTITY,
        [GroomerId] bigint NOT NULL,
        [Caption] nvarchar(2000) NULL,
        [PetId] bigint NULL,
        [GroomerServiceId] bigint NULL,
        [Active] bit NOT NULL,
        [Approved] bit NOT NULL,
        [ApprovalValue] nvarchar(max) NULL,
        [Deleted] bit NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [LikeCount] int NOT NULL,
        [CommentCount] int NOT NULL,
        CONSTRAINT [PK_GroomerPortfolioPosts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GroomerPortfolioPosts_GroomerServices_GroomerServiceId] FOREIGN KEY ([GroomerServiceId]) REFERENCES [GroomerServices] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerPortfolioPosts_Groomers_GroomerId] FOREIGN KEY ([GroomerId]) REFERENCES [Groomers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerPortfolioPosts_Pets_PetId] FOREIGN KEY ([PetId]) REFERENCES [Pets] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE TABLE [GroomerServicePackages] (
        [Id] bigint NOT NULL IDENTITY,
        [GroomerServiceId] bigint NOT NULL,
        [Price] float NOT NULL,
        [PrePaymentPrice] float NOT NULL,
        [PictureId] bigint NULL,
        [Active] bit NOT NULL,
        [ActivationValue] nvarchar(max) NULL,
        [Discription] nvarchar(max) NULL,
        [PetSize] nvarchar(20) NULL,
        [Deleted] bit NOT NULL,
        [Name] nvarchar(max) NULL,
        CONSTRAINT [PK_GroomerServicePackages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GroomerServicePackages_GroomerServices_GroomerServiceId] FOREIGN KEY ([GroomerServiceId]) REFERENCES [GroomerServices] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerServicePackages_Pictures_PictureId] FOREIGN KEY ([PictureId]) REFERENCES [Pictures] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE TABLE [GroomerServiceUsers] (
        [Id] bigint NOT NULL IDENTITY,
        [GroomerServiceId] bigint NOT NULL,
        [UserId] bigint NOT NULL,
        [Active] bit NOT NULL,
        [ActivationValue] nvarchar(max) NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_GroomerServiceUsers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GroomerServiceUsers_GroomerServices_GroomerServiceId] FOREIGN KEY ([GroomerServiceId]) REFERENCES [GroomerServices] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerServiceUsers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE TABLE [GroomerPortfolioComments] (
        [Id] bigint NOT NULL,
        [PostId] bigint NOT NULL,
        [ParentId] bigint NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_GroomerPortfolioComments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GroomerPortfolioComments_Comments_Id] FOREIGN KEY ([Id]) REFERENCES [Comments] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_GroomerPortfolioComments_GroomerPortfolioComments_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [GroomerPortfolioComments] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerPortfolioComments_GroomerPortfolioPosts_PostId] FOREIGN KEY ([PostId]) REFERENCES [GroomerPortfolioPosts] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE TABLE [GroomerPortfolioLikes] (
        [Id] bigint NOT NULL IDENTITY,
        [PostId] bigint NOT NULL,
        [UserId] bigint NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        CONSTRAINT [PK_GroomerPortfolioLikes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GroomerPortfolioLikes_GroomerPortfolioPosts_PostId] FOREIGN KEY ([PostId]) REFERENCES [GroomerPortfolioPosts] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerPortfolioLikes_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE TABLE [GroomerPortfolioPictures] (
        [Id] bigint NOT NULL IDENTITY,
        [PostId] bigint NOT NULL,
        [PictureId] bigint NOT NULL,
        [Kind] int NOT NULL,
        [SortOrder] int NOT NULL,
        [Label] nvarchar(max) NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_GroomerPortfolioPictures] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GroomerPortfolioPictures_GroomerPortfolioPosts_PostId] FOREIGN KEY ([PostId]) REFERENCES [GroomerPortfolioPosts] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerPortfolioPictures_Pictures_PictureId] FOREIGN KEY ([PictureId]) REFERENCES [Pictures] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE TABLE [GroomerServicePackagePictures] (
        [Id] bigint NOT NULL IDENTITY,
        [GroomerServicePackageId] bigint NOT NULL,
        [PictureId] bigint NOT NULL,
        [Label] nvarchar(max) NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_GroomerServicePackagePictures] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GroomerServicePackagePictures_GroomerServicePackages_GroomerServicePackageId] FOREIGN KEY ([GroomerServicePackageId]) REFERENCES [GroomerServicePackages] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerServicePackagePictures_Pictures_PictureId] FOREIGN KEY ([PictureId]) REFERENCES [Pictures] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerComments_GroomerId] ON [GroomerComments] ([GroomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerPets_GroomerId_PetId] ON [GroomerPets] ([GroomerId], [PetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerPets_PetId] ON [GroomerPets] ([PetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerPictures_GroomerId] ON [GroomerPictures] ([GroomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerPictures_PictureId] ON [GroomerPictures] ([PictureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerPortfolioComments_ParentId] ON [GroomerPortfolioComments] ([ParentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerPortfolioComments_PostId_Deleted] ON [GroomerPortfolioComments] ([PostId], [Deleted]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE UNIQUE INDEX [IX_GroomerPortfolioLikes_PostId_UserId] ON [GroomerPortfolioLikes] ([PostId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerPortfolioLikes_UserId] ON [GroomerPortfolioLikes] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerPortfolioPictures_PictureId] ON [GroomerPortfolioPictures] ([PictureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerPortfolioPictures_PostId_SortOrder] ON [GroomerPortfolioPictures] ([PostId], [SortOrder]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerPortfolioPosts_GroomerId_Deleted_Approved_Active_Id] ON [GroomerPortfolioPosts] ([GroomerId], [Deleted], [Approved], [Active], [Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerPortfolioPosts_GroomerServiceId] ON [GroomerPortfolioPosts] ([GroomerServiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerPortfolioPosts_PetId] ON [GroomerPortfolioPosts] ([PetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_Groomers_BackgroundPictureId] ON [Groomers] ([BackgroundPictureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_Groomers_CityId_Active_Approved_Deleted] ON [Groomers] ([CityId], [Active], [Approved], [Deleted]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_Groomers_IconId] ON [Groomers] ([IconId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Groomers_LegacyCompanionId] ON [Groomers] ([LegacyCompanionId]) WHERE [LegacyCompanionId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_Groomers_NeighborhoodId] ON [Groomers] ([NeighborhoodId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_Groomers_OwnerId] ON [Groomers] ([OwnerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_Groomers_PictureId] ON [Groomers] ([PictureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_Groomers_ProviderId_Deleted] ON [Groomers] ([ProviderId], [Deleted]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Groomers_ReferralCode] ON [Groomers] ([ReferralCode]) WHERE [ReferralCode] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_Groomers_StateId] ON [Groomers] ([StateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerServicePackagePictures_GroomerServicePackageId] ON [GroomerServicePackagePictures] ([GroomerServicePackageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerServicePackagePictures_PictureId] ON [GroomerServicePackagePictures] ([PictureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerServicePackages_GroomerServiceId] ON [GroomerServicePackages] ([GroomerServiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerServicePackages_PictureId] ON [GroomerServicePackages] ([PictureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerServices_AssistanceId] ON [GroomerServices] ([AssistanceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerServices_GroomerId_AssistanceId] ON [GroomerServices] ([GroomerId], [AssistanceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerServiceUsers_GroomerServiceId_UserId] ON [GroomerServiceUsers] ([GroomerServiceId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerServiceUsers_UserId] ON [GroomerServiceUsers] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerTimes_GroomerId_WeekDayId] ON [GroomerTimes] ([GroomerId], [WeekDayId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerTimes_WeekDayId] ON [GroomerTimes] ([WeekDayId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerUsers_GroomerId_UserId] ON [GroomerUsers] ([GroomerId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerUsers_UserId] ON [GroomerUsers] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerZones_CityId] ON [GroomerZones] ([CityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerZones_GroomerId_CityId] ON [GroomerZones] ([GroomerId], [CityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerZones_NeighborhoodId] ON [GroomerZones] ([NeighborhoodId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    CREATE INDEX [IX_GroomerZones_StateId] ON [GroomerZones] ([StateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019085911_AddGroomers'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261019085911_AddGroomers', N'9.0.0');
END;

COMMIT;
GO

