BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    ALTER TABLE [Wallets] ADD [CoachReserveId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE TABLE [Coaches] (
        [Id] bigint NOT NULL IDENTITY,
        [ProviderId] bigint NULL,
        [OwnerId] bigint NOT NULL,
        [LegacyCompanionId] bigint NULL,
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
        [Regulations] nvarchar(max) NULL,
        [EstablishedYear] int NULL,
        [LicenseNumber] nvarchar(100) NULL,
        [LicenseApproved] bit NOT NULL,
        [InstagramUrl] nvarchar(300) NULL,
        [WebsiteUrl] nvarchar(300) NULL,
        [ServiceAtHome] bit NOT NULL,
        [ServiceInPerson] bit NOT NULL,
        [ServiceOnline] bit NOT NULL,
        [HasParking] bit NOT NULL,
        [HasWaitingRoom] bit NOT NULL,
        [HasCamera] bit NOT NULL,
        [MaxConcurrentPets] int NOT NULL,
        [StaffCount] int NOT NULL,
        [CommissionPercent] decimal(5,2) NOT NULL,
        [Name] nvarchar(200) NULL,
        [Summary] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [SeoH1] nvarchar(max) NULL,
        [SeoMinDescription] nvarchar(max) NULL,
        [SeoDescription] nvarchar(max) NULL,
        [SeoTitle] nvarchar(max) NULL,
        [SeoPictureAlt] nvarchar(max) NULL,
        [SeoUrlText] nvarchar(max) NULL,
        [SeoNoIndex] bit NOT NULL,
        [SeoNoFollow] bit NOT NULL,
        [SeoCanonical] nvarchar(max) NULL,
        CONSTRAINT [PK_Coaches] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Coaches_Cities_CityId] FOREIGN KEY ([CityId]) REFERENCES [Cities] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Coaches_Neighborhoods_NeighborhoodId] FOREIGN KEY ([NeighborhoodId]) REFERENCES [Neighborhoods] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Coaches_Pictures_BackgroundPictureId] FOREIGN KEY ([BackgroundPictureId]) REFERENCES [Pictures] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Coaches_Pictures_IconId] FOREIGN KEY ([IconId]) REFERENCES [Pictures] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Coaches_Pictures_PictureId] FOREIGN KEY ([PictureId]) REFERENCES [Pictures] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Coaches_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Coaches_States_StateId] FOREIGN KEY ([StateId]) REFERENCES [States] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Coaches_Users_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE TABLE [CoachPets] (
        [Id] bigint NOT NULL IDENTITY,
        [CoachId] bigint NOT NULL,
        [PetId] bigint NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_CoachPets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CoachPets_Coaches_CoachId] FOREIGN KEY ([CoachId]) REFERENCES [Coaches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachPets_Pets_PetId] FOREIGN KEY ([PetId]) REFERENCES [Pets] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE TABLE [CoachPictures] (
        [Id] bigint NOT NULL IDENTITY,
        [CoachId] bigint NOT NULL,
        [PictureId] bigint NOT NULL,
        [Label] nvarchar(max) NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_CoachPictures] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CoachPictures_Coaches_CoachId] FOREIGN KEY ([CoachId]) REFERENCES [Coaches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachPictures_Pictures_PictureId] FOREIGN KEY ([PictureId]) REFERENCES [Pictures] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE TABLE [CoachServices] (
        [Id] bigint NOT NULL IDENTITY,
        [CoachId] bigint NOT NULL,
        [AssistanceId] bigint NOT NULL,
        [Active] bit NOT NULL,
        [Approved] bit NOT NULL,
        [ActivationValue] nvarchar(max) NULL,
        [Deleted] bit NOT NULL,
        [CommissionPercent] decimal(5,2) NOT NULL,
        [DurationMinutes] int NOT NULL,
        [Description] nvarchar(max) NULL,
        CONSTRAINT [PK_CoachServices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CoachServices_Assistances_AssistanceId] FOREIGN KEY ([AssistanceId]) REFERENCES [Assistances] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachServices_Coaches_CoachId] FOREIGN KEY ([CoachId]) REFERENCES [Coaches] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE TABLE [CoachTimes] (
        [Id] bigint NOT NULL IDENTITY,
        [CoachId] bigint NOT NULL,
        [WeekDayId] bigint NOT NULL,
        [StartTime] nvarchar(10) NULL,
        [EndTime] nvarchar(10) NULL,
        [Capacity] int NOT NULL,
        [Active] bit NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_CoachTimes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CoachTimes_Coaches_CoachId] FOREIGN KEY ([CoachId]) REFERENCES [Coaches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachTimes_WeekDays_WeekDayId] FOREIGN KEY ([WeekDayId]) REFERENCES [WeekDays] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE TABLE [CoachUsers] (
        [Id] bigint NOT NULL IDENTITY,
        [CoachId] bigint NOT NULL,
        [UserId] bigint NOT NULL,
        [UserAccept] bit NULL,
        [InviteDate] datetime2 NULL,
        [Active] bit NOT NULL,
        [ActivationValue] nvarchar(max) NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_CoachUsers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CoachUsers_Coaches_CoachId] FOREIGN KEY ([CoachId]) REFERENCES [Coaches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachUsers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE TABLE [CoachZones] (
        [Id] bigint NOT NULL IDENTITY,
        [CoachId] bigint NOT NULL,
        [StateId] bigint NOT NULL,
        [CityId] bigint NOT NULL,
        [NeighborhoodId] bigint NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_CoachZones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CoachZones_Cities_CityId] FOREIGN KEY ([CityId]) REFERENCES [Cities] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachZones_Coaches_CoachId] FOREIGN KEY ([CoachId]) REFERENCES [Coaches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachZones_Neighborhoods_NeighborhoodId] FOREIGN KEY ([NeighborhoodId]) REFERENCES [Neighborhoods] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachZones_States_StateId] FOREIGN KEY ([StateId]) REFERENCES [States] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE TABLE [CoachServicePackages] (
        [Id] bigint NOT NULL IDENTITY,
        [CoachServiceId] bigint NOT NULL,
        [Price] float NOT NULL,
        [PrePaymentPrice] float NOT NULL,
        [PictureId] bigint NULL,
        [Active] bit NOT NULL,
        [ActivationValue] nvarchar(max) NULL,
        [Discription] nvarchar(max) NULL,
        [PetSize] nvarchar(20) NULL,
        [Deleted] bit NOT NULL,
        [Name] nvarchar(max) NULL,
        CONSTRAINT [PK_CoachServicePackages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CoachServicePackages_CoachServices_CoachServiceId] FOREIGN KEY ([CoachServiceId]) REFERENCES [CoachServices] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachServicePackages_Pictures_PictureId] FOREIGN KEY ([PictureId]) REFERENCES [Pictures] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE TABLE [CoachServiceUsers] (
        [Id] bigint NOT NULL IDENTITY,
        [CoachServiceId] bigint NOT NULL,
        [UserId] bigint NOT NULL,
        [Active] bit NOT NULL,
        [ActivationValue] nvarchar(max) NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_CoachServiceUsers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CoachServiceUsers_CoachServices_CoachServiceId] FOREIGN KEY ([CoachServiceId]) REFERENCES [CoachServices] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachServiceUsers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE TABLE [CoachReserves] (
        [Id] bigint NOT NULL IDENTITY,
        [ReserveCode] nvarchar(40) NULL,
        [CoachId] bigint NOT NULL,
        [CoachServiceId] bigint NOT NULL,
        [CoachServicePackageId] bigint NOT NULL,
        [BookerId] bigint NOT NULL,
        [UserPetId] bigint NOT NULL,
        [AssigneeUserId] bigint NULL,
        [CoachTimeId] bigint NULL,
        [DoDate] datetime2 NOT NULL,
        [DurationMinutes] int NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [Mode] int NOT NULL,
        [AddressId] bigint NULL,
        [BookerNote] nvarchar(500) NULL,
        [PackagePrice] float NOT NULL,
        [Price] float NOT NULL,
        [OnlineAmount] float NOT NULL,
        [FromWallet] bit NOT NULL,
        [WalletPrice] float NOT NULL,
        [RebateId] bigint NULL,
        [RebatePrice] float NOT NULL,
        [PaymentPrice] float NOT NULL,
        [CommissionPercent] decimal(5,2) NOT NULL,
        [ProviderShare] float NOT NULL,
        [SiteShare] float NOT NULL,
        [Permitted] bit NOT NULL,
        [Status] int NOT NULL,
        [IsReserved] bit NOT NULL,
        [IsCancel] bit NOT NULL,
        [CancelDetail] nvarchar(max) NULL,
        [CancelDate] datetime2 NULL,
        [CancelledByBooker] bit NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [PaidDate] datetime2 NULL,
        [DoneDate] datetime2 NULL,
        [RescheduleCount] int NOT NULL,
        [OriginalDoDate] datetime2 NULL,
        [LastRescheduleDate] datetime2 NULL,
        [OwnerDecision] int NOT NULL,
        [OwnerDecisionDate] datetime2 NULL,
        [OwnerDecisionReason] nvarchar(max) NULL,
        [OwnerApprovalDeadline] datetime2 NULL,
        CONSTRAINT [PK_CoachReserves] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_CoachReserve_Dates] CHECK ([EndDate] >= [DoDate]),
        CONSTRAINT [CK_CoachReserve_Prices] CHECK ([Price] >= 0 AND [OnlineAmount] >= 0 AND [PaymentPrice] >= 0 AND [WalletPrice] >= 0 AND [RebatePrice] >= 0),
        CONSTRAINT [FK_CoachReserves_Addresses_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [Addresses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachReserves_CoachServicePackages_CoachServicePackageId] FOREIGN KEY ([CoachServicePackageId]) REFERENCES [CoachServicePackages] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachReserves_CoachServices_CoachServiceId] FOREIGN KEY ([CoachServiceId]) REFERENCES [CoachServices] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachReserves_CoachTimes_CoachTimeId] FOREIGN KEY ([CoachTimeId]) REFERENCES [CoachTimes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachReserves_Coaches_CoachId] FOREIGN KEY ([CoachId]) REFERENCES [Coaches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachReserves_Rebate_RebateId] FOREIGN KEY ([RebateId]) REFERENCES [Rebate] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachReserves_UserPets_UserPetId] FOREIGN KEY ([UserPetId]) REFERENCES [UserPets] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachReserves_Users_AssigneeUserId] FOREIGN KEY ([AssigneeUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachReserves_Users_BookerId] FOREIGN KEY ([BookerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE TABLE [CoachServicePackagePictures] (
        [Id] bigint NOT NULL IDENTITY,
        [CoachServicePackageId] bigint NOT NULL,
        [PictureId] bigint NOT NULL,
        [Label] nvarchar(max) NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_CoachServicePackagePictures] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CoachServicePackagePictures_CoachServicePackages_CoachServicePackageId] FOREIGN KEY ([CoachServicePackageId]) REFERENCES [CoachServicePackages] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachServicePackagePictures_Pictures_PictureId] FOREIGN KEY ([PictureId]) REFERENCES [Pictures] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE TABLE [CoachServicePackageTypes] (
        [Id] bigint NOT NULL IDENTITY,
        [CoachServicePackageId] bigint NOT NULL,
        [Mode] int NOT NULL,
        [Price] float NOT NULL,
        [PrePaymentPrice] float NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_CoachServicePackageTypes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CoachServicePackageTypes_CoachServicePackages_CoachServicePackageId] FOREIGN KEY ([CoachServicePackageId]) REFERENCES [CoachServicePackages] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE TABLE [CoachComments] (
        [Id] bigint NOT NULL,
        [CoachId] bigint NOT NULL,
        [CoachReserveId] bigint NULL,
        [IsReserved] bit NOT NULL,
        CONSTRAINT [PK_CoachComments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CoachComments_CoachReserves_CoachReserveId] FOREIGN KEY ([CoachReserveId]) REFERENCES [CoachReserves] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachComments_Coaches_CoachId] FOREIGN KEY ([CoachId]) REFERENCES [Coaches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachComments_Comments_Id] FOREIGN KEY ([Id]) REFERENCES [Comments] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE TABLE [CoachReserveItems] (
        [Id] bigint NOT NULL IDENTITY,
        [CoachReserveId] bigint NOT NULL,
        [CoachServiceId] bigint NOT NULL,
        [CoachServicePackageId] bigint NOT NULL,
        [PackagePrice] float NOT NULL,
        [OnlineAmount] float NOT NULL,
        [DurationMinutes] int NOT NULL,
        [CommissionPercent] decimal(5,2) NOT NULL,
        [ProviderShare] float NOT NULL,
        [SiteShare] float NOT NULL,
        [PackageName] nvarchar(300) NULL,
        [CreateDate] datetime2 NOT NULL,
        CONSTRAINT [PK_CoachReserveItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CoachReserveItems_CoachReserves_CoachReserveId] FOREIGN KEY ([CoachReserveId]) REFERENCES [CoachReserves] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachReserveItems_CoachServicePackages_CoachServicePackageId] FOREIGN KEY ([CoachServicePackageId]) REFERENCES [CoachServicePackages] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachReserveItems_CoachServices_CoachServiceId] FOREIGN KEY ([CoachServiceId]) REFERENCES [CoachServices] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE TABLE [SettlementCoaches] (
        [Id] bigint NOT NULL IDENTITY,
        [SettlementId] bigint NOT NULL,
        [CoachReserveId] bigint NOT NULL,
        CONSTRAINT [PK_SettlementCoaches] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SettlementCoaches_CoachReserves_CoachReserveId] FOREIGN KEY ([CoachReserveId]) REFERENCES [CoachReserves] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SettlementCoaches_Settlements_SettlementId] FOREIGN KEY ([SettlementId]) REFERENCES [Settlements] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Wallets_CoachReserveId] ON [Wallets] ([CoachReserveId]) WHERE [CoachReserveId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachComments_CoachId] ON [CoachComments] ([CoachId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_CoachComments_CoachReserveId] ON [CoachComments] ([CoachReserveId]) WHERE [CoachReserveId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_Coaches_BackgroundPictureId] ON [Coaches] ([BackgroundPictureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_Coaches_CityId_Active_Approved_Deleted] ON [Coaches] ([CityId], [Active], [Approved], [Deleted]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_Coaches_IconId] ON [Coaches] ([IconId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Coaches_LegacyCompanionId] ON [Coaches] ([LegacyCompanionId]) WHERE [LegacyCompanionId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_Coaches_NeighborhoodId] ON [Coaches] ([NeighborhoodId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_Coaches_OwnerId] ON [Coaches] ([OwnerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_Coaches_PictureId] ON [Coaches] ([PictureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_Coaches_ProviderId_Deleted] ON [Coaches] ([ProviderId], [Deleted]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Coaches_ReferralCode] ON [Coaches] ([ReferralCode]) WHERE [ReferralCode] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_Coaches_StateId] ON [Coaches] ([StateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachPets_CoachId_PetId] ON [CoachPets] ([CoachId], [PetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachPets_PetId] ON [CoachPets] ([PetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachPictures_CoachId] ON [CoachPictures] ([CoachId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachPictures_PictureId] ON [CoachPictures] ([PictureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachReserveItems_CoachReserveId] ON [CoachReserveItems] ([CoachReserveId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachReserveItems_CoachServiceId] ON [CoachReserveItems] ([CoachServiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachReserveItems_CoachServicePackageId] ON [CoachReserveItems] ([CoachServicePackageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachReserves_AddressId] ON [CoachReserves] ([AddressId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachReserves_AssigneeUserId] ON [CoachReserves] ([AssigneeUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachReserves_BookerId_CreateDate] ON [CoachReserves] ([BookerId], [CreateDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachReserves_CoachId_DoDate_EndDate] ON [CoachReserves] ([CoachId], [DoDate], [EndDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachReserves_CoachServiceId] ON [CoachReserves] ([CoachServiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachReserves_CoachServicePackageId] ON [CoachReserves] ([CoachServicePackageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachReserves_CoachTimeId] ON [CoachReserves] ([CoachTimeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachReserves_OwnerDecision_OwnerApprovalDeadline] ON [CoachReserves] ([OwnerDecision], [OwnerApprovalDeadline]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachReserves_RebateId] ON [CoachReserves] ([RebateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_CoachReserves_ReserveCode] ON [CoachReserves] ([ReserveCode]) WHERE [ReserveCode] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachReserves_UserPetId] ON [CoachReserves] ([UserPetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachServicePackagePictures_CoachServicePackageId] ON [CoachServicePackagePictures] ([CoachServicePackageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachServicePackagePictures_PictureId] ON [CoachServicePackagePictures] ([PictureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachServicePackages_CoachServiceId] ON [CoachServicePackages] ([CoachServiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachServicePackages_PictureId] ON [CoachServicePackages] ([PictureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachServicePackageTypes_CoachServicePackageId_Mode] ON [CoachServicePackageTypes] ([CoachServicePackageId], [Mode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachServices_AssistanceId] ON [CoachServices] ([AssistanceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachServices_CoachId_AssistanceId] ON [CoachServices] ([CoachId], [AssistanceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachServiceUsers_CoachServiceId_UserId] ON [CoachServiceUsers] ([CoachServiceId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachServiceUsers_UserId] ON [CoachServiceUsers] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachTimes_CoachId_WeekDayId] ON [CoachTimes] ([CoachId], [WeekDayId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachTimes_WeekDayId] ON [CoachTimes] ([WeekDayId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachUsers_CoachId_UserId] ON [CoachUsers] ([CoachId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachUsers_UserId] ON [CoachUsers] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachZones_CityId] ON [CoachZones] ([CityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachZones_CoachId_CityId] ON [CoachZones] ([CoachId], [CityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachZones_NeighborhoodId] ON [CoachZones] ([NeighborhoodId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_CoachZones_StateId] ON [CoachZones] ([StateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SettlementCoaches_CoachReserveId] ON [SettlementCoaches] ([CoachReserveId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    CREATE INDEX [IX_SettlementCoaches_SettlementId] ON [SettlementCoaches] ([SettlementId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    ALTER TABLE [Wallets] ADD CONSTRAINT [FK_Wallets_CoachReserves_CoachReserveId] FOREIGN KEY ([CoachReserveId]) REFERENCES [CoachReserves] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    DECLARE @PaymentTypeGroupId BIGINT = (SELECT TOP (1) CodeGroupId FROM Codes WHERE Label = 'PaymentType_PansionReserve');
    IF @PaymentTypeGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'PaymentType_CoachReserve')
        INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
        VALUES ('PaymentType_CoachReserve', 'coach-reserve', @PaymentTypeGroupId,
                (SELECT ISNULL(MAX(Priority), 0) + 1 FROM Codes WHERE CodeGroupId = @PaymentTypeGroupId), 1, N'پرداخت رزرو مربی');

    DECLARE @RebateTypeGroupId BIGINT = (SELECT TOP (1) CodeGroupId FROM Codes WHERE Label = 'RebateType_PansionReserve');
    IF @RebateTypeGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'RebateType_CoachReserve')
        INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
        VALUES ('RebateType_CoachReserve', 'coach-reserve', @RebateTypeGroupId,
                (SELECT ISNULL(MAX(Priority), 0) + 1 FROM Codes WHERE CodeGroupId = @RebateTypeGroupId), 1, N'تخفیف رزرو مربی');

    DECLARE @ScoreGroupId BIGINT = (SELECT TOP (1) CodeGroupId FROM Codes WHERE Label = 'ScoreTransactionType_PansionReserve');
    IF @ScoreGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'ScoreTransactionType_CoachReserve')
        INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
        VALUES ('ScoreTransactionType_CoachReserve', 'coach-reserve', @ScoreGroupId,
                (SELECT ISNULL(MAX(Priority), 0) + 1 FROM Codes WHERE CodeGroupId = @ScoreGroupId), 1, N'رزرو مربی');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    DECLARE @Types TABLE (Id bigint, Name nvarchar(200), Label nvarchar(200), Title nvarchar(200), Body nvarchar(500), Url nvarchar(200), Tag nvarchar(200));
    INSERT INTO @Types (Id, Name, Label, Title, Body, Url, Tag) VALUES
        (154, N'رزرو مربی منتظر تأیید (به مربی)', N'PushCoachReserveApprovalRequired', N'رزرو جدید منتظر تأیید', N'رزرو جدید {1} در {0} منتظر تأیید شماست.', N'/companionProfile/coachReserve', N'coach-reserve-approval'),
        (155, N'رزرو مربی تأیید شد (به کاربر)', N'PushCoachReserveApproved', N'رزرو شما تأیید شد', N'رزرو {1} در {0} توسط مربی تأیید شد.', N'/reserve', N'coach-reserve-approved'),
        (156, N'رزرو مربی رد شد (به کاربر)', N'PushCoachReserveRejected', N'رزرو شما رد شد', N'رزرو شما در {0} رد شد و مبلغ به کیف پول شما برگشت. دلیل: {1}', N'/reserve', N'coach-reserve-rejected'),
        (157, N'مربی به رزرو پاسخ نداد، رزرو لغو شد (به کاربر)', N'PushCoachReserveExpired', N'رزرو شما لغو شد', N'مربی {0} در مهلت مقرر به رزرو شما پاسخ نداد؛ رزرو لغو و مبلغ به کیف پول شما برگشت.', N'/reserve', N'coach-reserve-expired'),
        (158, N'رزرو مربی توسط کاربر لغو شد (به مربی)', N'PushCoachReserveCancelledByBooker', N'رزرو لغو شد', N'رزرو {0} در تاریخ {1} توسط مشتری لغو شد.', N'/companionProfile/coachReserve', N'coach-reserve-cancelled'),
        (159, N'مربی تأیید شد (به مالک)', N'PushCoachApproved', N'مربی شما تأیید شد', N'مربی {0} تأیید شد و اکنون در پاستیل نمایش داده می‌شود.', N'/companionProfile', N'coach-approved'),
        (160, N'مربی رد شد (به مالک)', N'PushCoachRejected', N'مربی شما رد شد', N'مربی {0} رد شد. دلیل: {1}', N'/companionProfile', N'coach-rejected'),
        (161, N'تغییر زمان رزرو مربی (به کاربر)', N'PushCoachReserveRescheduled', N'زمان رزرو شما تغییر کرد', N'زمان رزرو شما در {0} به {1} ساعت {2} تغییر کرد.', N'/reserve', N'coach-reserve-rescheduled'),
        (162, N'تغییر زمان رزرو مربی (به مربی)', N'PushCoachReserveRescheduledForCoach', N'زمان رزرو تغییر کرد', N'زمان رزرو {0} به {1} ساعت {2} تغییر کرد.', N'/companionProfile/coachReserve', N'coach-reserve-rescheduled-coach'),
        (163, N'دعوت به پرسنل مربی (به کاربر)', N'PushCoachStaffInvite', N'دعوت به همکاری', N'شما به پرسنل {0} دعوت شده‌اید. برای پذیرش وارد اپ شوید.', N'/', N'coach-staff-invite'),
        (164, N'لغو رزرو مربی توسط پشتیبانی (به کاربر)', N'PushCoachReserveCancelledByAdmin', N'رزرو شما لغو شد', N'رزرو شما در {0} توسط پشتیبانی لغو شد. دلیل: {1}', N'/reserve', N'coach-reserve-cancelled-admin');

    IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Id = s.Id WHERE t.Label <> s.Label)
        THROW 51000, 'A coach push type ID is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Label = s.Label WHERE t.Id <> s.Id)
        THROW 51000, 'A coach push label is already assigned to another ID.', 1;

    SET IDENTITY_INSERT PushTypes ON;
    INSERT INTO PushTypes (Id, Name, Label)
    SELECT s.Id, s.Name, s.Label FROM @Types s
    WHERE NOT EXISTS (SELECT 1 FROM PushTypes t WHERE t.Id = s.Id);
    SET IDENTITY_INSERT PushTypes OFF;

    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
    SELECT s.Id, s.Title, s.Body, s.Url, NULL, s.Tag, 1 FROM @Types s
    WHERE NOT EXISTS (SELECT 1 FROM PushPatterns p WHERE p.PushTypeId = s.Id);

    INSERT INTO PushSettings (PushPatternId, IsEnabled)
    SELECT pattern.Id, 1
    FROM PushPatterns pattern
    WHERE pattern.PushTypeId IN (154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164)
      AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020080540_AddCoaches'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261020080540_AddCoaches', N'9.0.0');
END;

COMMIT;
GO

