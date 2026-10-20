BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    ALTER TABLE [Wallets] ADD [ClinicReserveId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE TABLE [Clinics] (
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
        CONSTRAINT [PK_Clinics] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Clinics_Cities_CityId] FOREIGN KEY ([CityId]) REFERENCES [Cities] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Clinics_Neighborhoods_NeighborhoodId] FOREIGN KEY ([NeighborhoodId]) REFERENCES [Neighborhoods] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Clinics_Pictures_BackgroundPictureId] FOREIGN KEY ([BackgroundPictureId]) REFERENCES [Pictures] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Clinics_Pictures_IconId] FOREIGN KEY ([IconId]) REFERENCES [Pictures] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Clinics_Pictures_PictureId] FOREIGN KEY ([PictureId]) REFERENCES [Pictures] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Clinics_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Clinics_States_StateId] FOREIGN KEY ([StateId]) REFERENCES [States] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Clinics_Users_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE TABLE [ClinicPets] (
        [Id] bigint NOT NULL IDENTITY,
        [ClinicId] bigint NOT NULL,
        [PetId] bigint NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_ClinicPets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClinicPets_Clinics_ClinicId] FOREIGN KEY ([ClinicId]) REFERENCES [Clinics] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicPets_Pets_PetId] FOREIGN KEY ([PetId]) REFERENCES [Pets] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE TABLE [ClinicPictures] (
        [Id] bigint NOT NULL IDENTITY,
        [ClinicId] bigint NOT NULL,
        [PictureId] bigint NOT NULL,
        [Label] nvarchar(max) NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_ClinicPictures] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClinicPictures_Clinics_ClinicId] FOREIGN KEY ([ClinicId]) REFERENCES [Clinics] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicPictures_Pictures_PictureId] FOREIGN KEY ([PictureId]) REFERENCES [Pictures] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE TABLE [ClinicServices] (
        [Id] bigint NOT NULL IDENTITY,
        [ClinicId] bigint NOT NULL,
        [AssistanceId] bigint NOT NULL,
        [Active] bit NOT NULL,
        [Approved] bit NOT NULL,
        [ActivationValue] nvarchar(max) NULL,
        [Deleted] bit NOT NULL,
        [CommissionPercent] decimal(5,2) NOT NULL,
        [DurationMinutes] int NOT NULL,
        [Description] nvarchar(max) NULL,
        CONSTRAINT [PK_ClinicServices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClinicServices_Assistances_AssistanceId] FOREIGN KEY ([AssistanceId]) REFERENCES [Assistances] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicServices_Clinics_ClinicId] FOREIGN KEY ([ClinicId]) REFERENCES [Clinics] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE TABLE [ClinicTimes] (
        [Id] bigint NOT NULL IDENTITY,
        [ClinicId] bigint NOT NULL,
        [WeekDayId] bigint NOT NULL,
        [StartTime] nvarchar(10) NULL,
        [EndTime] nvarchar(10) NULL,
        [Capacity] int NOT NULL,
        [Active] bit NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_ClinicTimes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClinicTimes_Clinics_ClinicId] FOREIGN KEY ([ClinicId]) REFERENCES [Clinics] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicTimes_WeekDays_WeekDayId] FOREIGN KEY ([WeekDayId]) REFERENCES [WeekDays] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE TABLE [ClinicTypes] (
        [Id] bigint NOT NULL IDENTITY,
        [ClinicId] bigint NOT NULL,
        [TypeId] bigint NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_ClinicTypes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClinicTypes_Clinics_ClinicId] FOREIGN KEY ([ClinicId]) REFERENCES [Clinics] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicTypes_Codes_TypeId] FOREIGN KEY ([TypeId]) REFERENCES [Codes] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE TABLE [ClinicUsers] (
        [Id] bigint NOT NULL IDENTITY,
        [ClinicId] bigint NOT NULL,
        [UserId] bigint NOT NULL,
        [UserAccept] bit NULL,
        [InviteDate] datetime2 NULL,
        [Active] bit NOT NULL,
        [ActivationValue] nvarchar(max) NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_ClinicUsers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClinicUsers_Clinics_ClinicId] FOREIGN KEY ([ClinicId]) REFERENCES [Clinics] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicUsers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE TABLE [ClinicZones] (
        [Id] bigint NOT NULL IDENTITY,
        [ClinicId] bigint NOT NULL,
        [StateId] bigint NOT NULL,
        [CityId] bigint NOT NULL,
        [NeighborhoodId] bigint NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_ClinicZones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClinicZones_Cities_CityId] FOREIGN KEY ([CityId]) REFERENCES [Cities] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicZones_Clinics_ClinicId] FOREIGN KEY ([ClinicId]) REFERENCES [Clinics] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicZones_Neighborhoods_NeighborhoodId] FOREIGN KEY ([NeighborhoodId]) REFERENCES [Neighborhoods] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicZones_States_StateId] FOREIGN KEY ([StateId]) REFERENCES [States] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE TABLE [ClinicServicePackages] (
        [Id] bigint NOT NULL IDENTITY,
        [ClinicServiceId] bigint NOT NULL,
        [Price] float NOT NULL,
        [PrePaymentPrice] float NOT NULL,
        [PictureId] bigint NULL,
        [Active] bit NOT NULL,
        [ActivationValue] nvarchar(max) NULL,
        [Discription] nvarchar(max) NULL,
        [PetSize] nvarchar(20) NULL,
        [Deleted] bit NOT NULL,
        [Name] nvarchar(max) NULL,
        CONSTRAINT [PK_ClinicServicePackages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClinicServicePackages_ClinicServices_ClinicServiceId] FOREIGN KEY ([ClinicServiceId]) REFERENCES [ClinicServices] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicServicePackages_Pictures_PictureId] FOREIGN KEY ([PictureId]) REFERENCES [Pictures] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE TABLE [ClinicServiceUsers] (
        [Id] bigint NOT NULL IDENTITY,
        [ClinicServiceId] bigint NOT NULL,
        [UserId] bigint NOT NULL,
        [Active] bit NOT NULL,
        [ActivationValue] nvarchar(max) NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_ClinicServiceUsers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClinicServiceUsers_ClinicServices_ClinicServiceId] FOREIGN KEY ([ClinicServiceId]) REFERENCES [ClinicServices] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicServiceUsers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE TABLE [ClinicReserves] (
        [Id] bigint NOT NULL IDENTITY,
        [ReserveCode] nvarchar(40) NULL,
        [ClinicId] bigint NOT NULL,
        [ClinicServiceId] bigint NOT NULL,
        [ClinicServicePackageId] bigint NOT NULL,
        [BookerId] bigint NOT NULL,
        [UserPetId] bigint NOT NULL,
        [AssigneeUserId] bigint NULL,
        [ClinicTimeId] bigint NULL,
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
        CONSTRAINT [PK_ClinicReserves] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ClinicReserve_Dates] CHECK ([EndDate] >= [DoDate]),
        CONSTRAINT [CK_ClinicReserve_Prices] CHECK ([Price] >= 0 AND [OnlineAmount] >= 0 AND [PaymentPrice] >= 0 AND [WalletPrice] >= 0 AND [RebatePrice] >= 0),
        CONSTRAINT [FK_ClinicReserves_Addresses_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [Addresses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicReserves_ClinicServicePackages_ClinicServicePackageId] FOREIGN KEY ([ClinicServicePackageId]) REFERENCES [ClinicServicePackages] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicReserves_ClinicServices_ClinicServiceId] FOREIGN KEY ([ClinicServiceId]) REFERENCES [ClinicServices] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicReserves_ClinicTimes_ClinicTimeId] FOREIGN KEY ([ClinicTimeId]) REFERENCES [ClinicTimes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicReserves_Clinics_ClinicId] FOREIGN KEY ([ClinicId]) REFERENCES [Clinics] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicReserves_Rebate_RebateId] FOREIGN KEY ([RebateId]) REFERENCES [Rebate] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicReserves_UserPets_UserPetId] FOREIGN KEY ([UserPetId]) REFERENCES [UserPets] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicReserves_Users_AssigneeUserId] FOREIGN KEY ([AssigneeUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicReserves_Users_BookerId] FOREIGN KEY ([BookerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE TABLE [ClinicServicePackagePictures] (
        [Id] bigint NOT NULL IDENTITY,
        [ClinicServicePackageId] bigint NOT NULL,
        [PictureId] bigint NOT NULL,
        [Label] nvarchar(max) NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_ClinicServicePackagePictures] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClinicServicePackagePictures_ClinicServicePackages_ClinicServicePackageId] FOREIGN KEY ([ClinicServicePackageId]) REFERENCES [ClinicServicePackages] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicServicePackagePictures_Pictures_PictureId] FOREIGN KEY ([PictureId]) REFERENCES [Pictures] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE TABLE [ClinicServicePackageTypes] (
        [Id] bigint NOT NULL IDENTITY,
        [ClinicServicePackageId] bigint NOT NULL,
        [Mode] int NOT NULL,
        [Price] float NOT NULL,
        [PrePaymentPrice] float NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_ClinicServicePackageTypes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClinicServicePackageTypes_ClinicServicePackages_ClinicServicePackageId] FOREIGN KEY ([ClinicServicePackageId]) REFERENCES [ClinicServicePackages] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE TABLE [ClinicComments] (
        [Id] bigint NOT NULL,
        [ClinicId] bigint NOT NULL,
        [ClinicReserveId] bigint NULL,
        [IsReserved] bit NOT NULL,
        CONSTRAINT [PK_ClinicComments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClinicComments_ClinicReserves_ClinicReserveId] FOREIGN KEY ([ClinicReserveId]) REFERENCES [ClinicReserves] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicComments_Clinics_ClinicId] FOREIGN KEY ([ClinicId]) REFERENCES [Clinics] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicComments_Comments_Id] FOREIGN KEY ([Id]) REFERENCES [Comments] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE TABLE [ClinicReserveItems] (
        [Id] bigint NOT NULL IDENTITY,
        [ClinicReserveId] bigint NOT NULL,
        [ClinicServiceId] bigint NOT NULL,
        [ClinicServicePackageId] bigint NOT NULL,
        [PackagePrice] float NOT NULL,
        [OnlineAmount] float NOT NULL,
        [DurationMinutes] int NOT NULL,
        [CommissionPercent] decimal(5,2) NOT NULL,
        [ProviderShare] float NOT NULL,
        [SiteShare] float NOT NULL,
        CONSTRAINT [PK_ClinicReserveItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClinicReserveItems_ClinicReserves_ClinicReserveId] FOREIGN KEY ([ClinicReserveId]) REFERENCES [ClinicReserves] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicReserveItems_ClinicServicePackages_ClinicServicePackageId] FOREIGN KEY ([ClinicServicePackageId]) REFERENCES [ClinicServicePackages] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicReserveItems_ClinicServices_ClinicServiceId] FOREIGN KEY ([ClinicServiceId]) REFERENCES [ClinicServices] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE TABLE [SettlementClinics] (
        [Id] bigint NOT NULL IDENTITY,
        [SettlementId] bigint NOT NULL,
        [ClinicReserveId] bigint NOT NULL,
        CONSTRAINT [PK_SettlementClinics] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SettlementClinics_ClinicReserves_ClinicReserveId] FOREIGN KEY ([ClinicReserveId]) REFERENCES [ClinicReserves] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SettlementClinics_Settlements_SettlementId] FOREIGN KEY ([SettlementId]) REFERENCES [Settlements] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Wallets_ClinicReserveId] ON [Wallets] ([ClinicReserveId]) WHERE [ClinicReserveId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicComments_ClinicId] ON [ClinicComments] ([ClinicId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ClinicComments_ClinicReserveId] ON [ClinicComments] ([ClinicReserveId]) WHERE [ClinicReserveId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicPets_ClinicId_PetId] ON [ClinicPets] ([ClinicId], [PetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicPets_PetId] ON [ClinicPets] ([PetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicPictures_ClinicId] ON [ClinicPictures] ([ClinicId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicPictures_PictureId] ON [ClinicPictures] ([PictureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicReserveItems_ClinicReserveId] ON [ClinicReserveItems] ([ClinicReserveId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicReserveItems_ClinicServiceId] ON [ClinicReserveItems] ([ClinicServiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicReserveItems_ClinicServicePackageId] ON [ClinicReserveItems] ([ClinicServicePackageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicReserves_AddressId] ON [ClinicReserves] ([AddressId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicReserves_AssigneeUserId] ON [ClinicReserves] ([AssigneeUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicReserves_BookerId_CreateDate] ON [ClinicReserves] ([BookerId], [CreateDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicReserves_ClinicId_DoDate_EndDate] ON [ClinicReserves] ([ClinicId], [DoDate], [EndDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicReserves_ClinicServiceId] ON [ClinicReserves] ([ClinicServiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicReserves_ClinicServicePackageId] ON [ClinicReserves] ([ClinicServicePackageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicReserves_ClinicTimeId] ON [ClinicReserves] ([ClinicTimeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicReserves_OwnerDecision_OwnerApprovalDeadline] ON [ClinicReserves] ([OwnerDecision], [OwnerApprovalDeadline]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicReserves_RebateId] ON [ClinicReserves] ([RebateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ClinicReserves_ReserveCode] ON [ClinicReserves] ([ReserveCode]) WHERE [ReserveCode] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicReserves_UserPetId] ON [ClinicReserves] ([UserPetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_Clinics_BackgroundPictureId] ON [Clinics] ([BackgroundPictureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_Clinics_CityId_Active_Approved_Deleted] ON [Clinics] ([CityId], [Active], [Approved], [Deleted]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_Clinics_IconId] ON [Clinics] ([IconId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Clinics_LegacyCompanionId] ON [Clinics] ([LegacyCompanionId]) WHERE [LegacyCompanionId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_Clinics_NeighborhoodId] ON [Clinics] ([NeighborhoodId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_Clinics_OwnerId] ON [Clinics] ([OwnerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_Clinics_PictureId] ON [Clinics] ([PictureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_Clinics_ProviderId_Deleted] ON [Clinics] ([ProviderId], [Deleted]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Clinics_ReferralCode] ON [Clinics] ([ReferralCode]) WHERE [ReferralCode] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_Clinics_StateId] ON [Clinics] ([StateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicServicePackagePictures_ClinicServicePackageId] ON [ClinicServicePackagePictures] ([ClinicServicePackageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicServicePackagePictures_PictureId] ON [ClinicServicePackagePictures] ([PictureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicServicePackages_ClinicServiceId] ON [ClinicServicePackages] ([ClinicServiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicServicePackages_PictureId] ON [ClinicServicePackages] ([PictureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicServicePackageTypes_ClinicServicePackageId_Mode] ON [ClinicServicePackageTypes] ([ClinicServicePackageId], [Mode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicServices_AssistanceId] ON [ClinicServices] ([AssistanceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicServices_ClinicId_AssistanceId] ON [ClinicServices] ([ClinicId], [AssistanceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicServiceUsers_ClinicServiceId_UserId] ON [ClinicServiceUsers] ([ClinicServiceId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicServiceUsers_UserId] ON [ClinicServiceUsers] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicTimes_ClinicId_WeekDayId] ON [ClinicTimes] ([ClinicId], [WeekDayId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicTimes_WeekDayId] ON [ClinicTimes] ([WeekDayId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicTypes_ClinicId_TypeId] ON [ClinicTypes] ([ClinicId], [TypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicTypes_TypeId] ON [ClinicTypes] ([TypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicUsers_ClinicId_UserId] ON [ClinicUsers] ([ClinicId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicUsers_UserId] ON [ClinicUsers] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicZones_CityId] ON [ClinicZones] ([CityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicZones_ClinicId_CityId] ON [ClinicZones] ([ClinicId], [CityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicZones_NeighborhoodId] ON [ClinicZones] ([NeighborhoodId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_ClinicZones_StateId] ON [ClinicZones] ([StateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SettlementClinics_ClinicReserveId] ON [SettlementClinics] ([ClinicReserveId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    CREATE INDEX [IX_SettlementClinics_SettlementId] ON [SettlementClinics] ([SettlementId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    ALTER TABLE [Wallets] ADD CONSTRAINT [FK_Wallets_ClinicReserves_ClinicReserveId] FOREIGN KEY ([ClinicReserveId]) REFERENCES [ClinicReserves] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    DECLARE @PaymentTypeGroupId BIGINT = (SELECT TOP (1) CodeGroupId FROM Codes WHERE Label = 'PaymentType_PansionReserve');
    IF @PaymentTypeGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'PaymentType_ClinicReserve')
        INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
        VALUES ('PaymentType_ClinicReserve', 'clinic-reserve', @PaymentTypeGroupId,
                (SELECT ISNULL(MAX(Priority), 0) + 1 FROM Codes WHERE CodeGroupId = @PaymentTypeGroupId), 1, N'پرداخت رزرو کلینیک');

    DECLARE @RebateTypeGroupId BIGINT = (SELECT TOP (1) CodeGroupId FROM Codes WHERE Label = 'RebateType_PansionReserve');
    IF @RebateTypeGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'RebateType_ClinicReserve')
        INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
        VALUES ('RebateType_ClinicReserve', 'clinic-reserve', @RebateTypeGroupId,
                (SELECT ISNULL(MAX(Priority), 0) + 1 FROM Codes WHERE CodeGroupId = @RebateTypeGroupId), 1, N'تخفیف رزرو کلینیک');

    DECLARE @ScoreGroupId BIGINT = (SELECT TOP (1) CodeGroupId FROM Codes WHERE Label = 'ScoreTransactionType_PansionReserve');
    IF @ScoreGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'ScoreTransactionType_ClinicReserve')
        INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
        VALUES ('ScoreTransactionType_ClinicReserve', 'clinic-reserve', @ScoreGroupId,
                (SELECT ISNULL(MAX(Priority), 0) + 1 FROM Codes WHERE CodeGroupId = @ScoreGroupId), 1, N'رزرو کلینیک');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    DECLARE @Types TABLE (Id bigint, Name nvarchar(200), Label nvarchar(200), Title nvarchar(200), Body nvarchar(500), Url nvarchar(200), Tag nvarchar(200));
    INSERT INTO @Types (Id, Name, Label, Title, Body, Url, Tag) VALUES
        (137, N'رزرو کلینیک منتظر تأیید (به کلینیک)', N'PushClinicReserveApprovalRequired', N'رزرو جدید منتظر تأیید', N'رزرو جدید {1} در {0} منتظر تأیید شماست.', N'/companionProfile/clinicReserve', N'clinic-reserve-approval'),
        (138, N'رزرو کلینیک تأیید شد (به کاربر)', N'PushClinicReserveApproved', N'رزرو شما تأیید شد', N'رزرو {1} در {0} توسط کلینیک تأیید شد.', N'/reserve', N'clinic-reserve-approved'),
        (139, N'رزرو کلینیک رد شد (به کاربر)', N'PushClinicReserveRejected', N'رزرو شما رد شد', N'رزرو شما در {0} رد شد و مبلغ به کیف پول شما برگشت. دلیل: {1}', N'/reserve', N'clinic-reserve-rejected'),
        (140, N'کلینیک به رزرو پاسخ نداد، رزرو لغو شد (به کاربر)', N'PushClinicReserveExpired', N'رزرو شما لغو شد', N'کلینیک {0} در مهلت مقرر به رزرو شما پاسخ نداد؛ رزرو لغو و مبلغ به کیف پول شما برگشت.', N'/reserve', N'clinic-reserve-expired'),
        (141, N'رزرو کلینیک توسط کاربر لغو شد (به کلینیک)', N'PushClinicReserveCancelledByBooker', N'رزرو لغو شد', N'رزرو {0} در تاریخ {1} توسط مشتری لغو شد.', N'/companionProfile/clinicReserve', N'clinic-reserve-cancelled'),
        (142, N'کلینیک تأیید شد (به مالک)', N'PushClinicApproved', N'کلینیک شما تأیید شد', N'کلینیک {0} تأیید شد و اکنون در پاستیل نمایش داده می‌شود.', N'/companionProfile', N'clinic-approved'),
        (143, N'کلینیک رد شد (به مالک)', N'PushClinicRejected', N'کلینیک شما رد شد', N'کلینیک {0} رد شد. دلیل: {1}', N'/companionProfile', N'clinic-rejected'),
        (144, N'تغییر زمان رزرو کلینیک (به کاربر)', N'PushClinicReserveRescheduled', N'زمان رزرو شما تغییر کرد', N'زمان رزرو شما در {0} به {1} ساعت {2} تغییر کرد.', N'/reserve', N'clinic-reserve-rescheduled'),
        (145, N'تغییر زمان رزرو کلینیک (به کلینیک)', N'PushClinicReserveRescheduledForClinic', N'زمان رزرو تغییر کرد', N'زمان رزرو {0} به {1} ساعت {2} تغییر کرد.', N'/companionProfile/clinicReserve', N'clinic-reserve-rescheduled-clinic'),
        (146, N'دعوت به پرسنل کلینیک (به کاربر)', N'PushClinicStaffInvite', N'دعوت به همکاری', N'شما به پرسنل {0} دعوت شده‌اید. برای پذیرش وارد اپ شوید.', N'/', N'clinic-staff-invite'),
        (147, N'لغو رزرو کلینیک توسط پشتیبانی (به کاربر)', N'PushClinicReserveCancelledByAdmin', N'رزرو شما لغو شد', N'رزرو شما در {0} توسط پشتیبانی لغو شد. دلیل: {1}', N'/reserve', N'clinic-reserve-cancelled-admin');

    IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Id = s.Id WHERE t.Label <> s.Label)
        THROW 51000, 'A clinic push type ID is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Label = s.Label WHERE t.Id <> s.Id)
        THROW 51000, 'A clinic push label is already assigned to another ID.', 1;

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
    WHERE pattern.PushTypeId IN (137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147)
      AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020060757_AddClinics'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261020060757_AddClinics', N'9.0.0');
END;

COMMIT;
GO

