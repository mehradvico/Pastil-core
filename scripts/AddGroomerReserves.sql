BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    DECLARE @PaymentTypeGroupId BIGINT = (SELECT TOP (1) CodeGroupId FROM Codes WHERE Label = 'PaymentType_PansionReserve');
    IF @PaymentTypeGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'PaymentType_GroomerReserve')
    BEGIN
        INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
        VALUES ('PaymentType_GroomerReserve', 'groomer-reserve', @PaymentTypeGroupId,
                (SELECT ISNULL(MAX(Priority), 0) + 1 FROM Codes WHERE CodeGroupId = @PaymentTypeGroupId), 1, N'پرداخت رزرو آرایشگاه');
    END

    DECLARE @RebateTypeGroupId BIGINT = (SELECT TOP (1) CodeGroupId FROM Codes WHERE Label = 'RebateType_PansionReserve');
    IF @RebateTypeGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'RebateType_GroomerReserve')
    BEGIN
        INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
        VALUES ('RebateType_GroomerReserve', 'groomer-reserve', @RebateTypeGroupId,
                (SELECT ISNULL(MAX(Priority), 0) + 1 FROM Codes WHERE CodeGroupId = @RebateTypeGroupId), 1, N'تخفیف رزرو آرایشگاه');
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    DECLARE @Types TABLE (Id bigint, Name nvarchar(200), Label nvarchar(200), Title nvarchar(200), Body nvarchar(500), Url nvarchar(200), Tag nvarchar(200));
    INSERT INTO @Types (Id, Name, Label, Title, Body, Url, Tag) VALUES
        (126, N'رزرو آرایشگاه منتظر تأیید (به آرایشگاه)', N'PushGroomerReserveApprovalRequired', N'رزرو جدید منتظر تأیید', N'رزرو جدید {1} در {0} منتظر تأیید شماست.', N'/companionProfile/groomerReserve', N'groomer-reserve-approval'),
        (127, N'رزرو آرایشگاه تأیید شد (به کاربر)', N'PushGroomerReserveApproved', N'رزرو شما تأیید شد', N'رزرو {1} در {0} توسط آرایشگاه تأیید شد.', N'/reserve', N'groomer-reserve-approved'),
        (128, N'رزرو آرایشگاه رد شد (به کاربر)', N'PushGroomerReserveRejected', N'رزرو شما رد شد', N'رزرو شما در {0} رد شد و مبلغ به کیف پول شما برگشت. دلیل: {1}', N'/reserve', N'groomer-reserve-rejected'),
        (129, N'آرایشگاه به رزرو پاسخ نداد، رزرو لغو شد (به کاربر)', N'PushGroomerReserveExpired', N'رزرو شما لغو شد', N'آرایشگاه {0} در مهلت مقرر به رزرو شما پاسخ نداد؛ رزرو لغو و مبلغ به کیف پول شما برگشت.', N'/reserve', N'groomer-reserve-expired'),
        (130, N'رزرو آرایشگاه توسط کاربر لغو شد (به آرایشگاه)', N'PushGroomerReserveCancelledByBooker', N'رزرو لغو شد', N'رزرو {0} در تاریخ {1} توسط مشتری لغو شد.', N'/companionProfile/groomerReserve', N'groomer-reserve-cancelled');

    IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Id = s.Id WHERE t.Label <> s.Label)
        THROW 51000, 'A groomer reserve push type ID is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Label = s.Label WHERE t.Id <> s.Id)
        THROW 51000, 'A groomer reserve push label is already assigned to another ID.', 1;

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
    WHERE pattern.PushTypeId IN (126, 127, 128, 129, 130)
      AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    ALTER TABLE [Wallets] ADD [GroomerReserveId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    ALTER TABLE [GroomerComments] ADD [GroomerReserveId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    ALTER TABLE [GroomerComments] ADD [IsReserved] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    CREATE TABLE [GroomerReserves] (
        [Id] bigint NOT NULL IDENTITY,
        [ReserveCode] nvarchar(40) NULL,
        [GroomerId] bigint NOT NULL,
        [GroomerServiceId] bigint NOT NULL,
        [GroomerServicePackageId] bigint NOT NULL,
        [BookerId] bigint NOT NULL,
        [UserPetId] bigint NOT NULL,
        [AssigneeUserId] bigint NULL,
        [GroomerTimeId] bigint NULL,
        [DoDate] datetime2 NOT NULL,
        [DurationMinutes] int NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [IsHomeService] bit NOT NULL,
        [AddressId] bigint NULL,
        [PickupRequested] bit NOT NULL,
        [BookerNote] nvarchar(500) NULL,
        [PackagePrice] float NOT NULL,
        [PickupFee] float NOT NULL,
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
        [OwnerDecision] int NOT NULL,
        [OwnerDecisionDate] datetime2 NULL,
        [OwnerDecisionReason] nvarchar(max) NULL,
        [OwnerApprovalDeadline] datetime2 NULL,
        CONSTRAINT [PK_GroomerReserves] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_GroomerReserve_Dates] CHECK ([EndDate] >= [DoDate]),
        CONSTRAINT [CK_GroomerReserve_Prices] CHECK ([Price] >= 0 AND [OnlineAmount] >= 0 AND [PaymentPrice] >= 0 AND [WalletPrice] >= 0 AND [RebatePrice] >= 0),
        CONSTRAINT [FK_GroomerReserves_Addresses_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [Addresses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerReserves_GroomerServicePackages_GroomerServicePackageId] FOREIGN KEY ([GroomerServicePackageId]) REFERENCES [GroomerServicePackages] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerReserves_GroomerServices_GroomerServiceId] FOREIGN KEY ([GroomerServiceId]) REFERENCES [GroomerServices] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerReserves_GroomerTimes_GroomerTimeId] FOREIGN KEY ([GroomerTimeId]) REFERENCES [GroomerTimes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerReserves_Groomers_GroomerId] FOREIGN KEY ([GroomerId]) REFERENCES [Groomers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerReserves_Rebate_RebateId] FOREIGN KEY ([RebateId]) REFERENCES [Rebate] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerReserves_UserPets_UserPetId] FOREIGN KEY ([UserPetId]) REFERENCES [UserPets] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerReserves_Users_AssigneeUserId] FOREIGN KEY ([AssigneeUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerReserves_Users_BookerId] FOREIGN KEY ([BookerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    CREATE TABLE [SettlementGroomers] (
        [Id] bigint NOT NULL IDENTITY,
        [SettlementId] bigint NOT NULL,
        [GroomerReserveId] bigint NOT NULL,
        CONSTRAINT [PK_SettlementGroomers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SettlementGroomers_GroomerReserves_GroomerReserveId] FOREIGN KEY ([GroomerReserveId]) REFERENCES [GroomerReserves] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SettlementGroomers_Settlements_SettlementId] FOREIGN KEY ([SettlementId]) REFERENCES [Settlements] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Wallets_GroomerReserveId] ON [Wallets] ([GroomerReserveId]) WHERE [GroomerReserveId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_GroomerComments_GroomerReserveId] ON [GroomerComments] ([GroomerReserveId]) WHERE [GroomerReserveId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    CREATE INDEX [IX_GroomerReserves_AddressId] ON [GroomerReserves] ([AddressId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    CREATE INDEX [IX_GroomerReserves_AssigneeUserId] ON [GroomerReserves] ([AssigneeUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    CREATE INDEX [IX_GroomerReserves_BookerId_CreateDate] ON [GroomerReserves] ([BookerId], [CreateDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    CREATE INDEX [IX_GroomerReserves_GroomerId_DoDate_EndDate] ON [GroomerReserves] ([GroomerId], [DoDate], [EndDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    CREATE INDEX [IX_GroomerReserves_GroomerServiceId] ON [GroomerReserves] ([GroomerServiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    CREATE INDEX [IX_GroomerReserves_GroomerServicePackageId] ON [GroomerReserves] ([GroomerServicePackageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    CREATE INDEX [IX_GroomerReserves_GroomerTimeId] ON [GroomerReserves] ([GroomerTimeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    CREATE INDEX [IX_GroomerReserves_OwnerDecision_OwnerApprovalDeadline] ON [GroomerReserves] ([OwnerDecision], [OwnerApprovalDeadline]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    CREATE INDEX [IX_GroomerReserves_RebateId] ON [GroomerReserves] ([RebateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_GroomerReserves_ReserveCode] ON [GroomerReserves] ([ReserveCode]) WHERE [ReserveCode] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    CREATE INDEX [IX_GroomerReserves_UserPetId] ON [GroomerReserves] ([UserPetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SettlementGroomers_GroomerReserveId] ON [SettlementGroomers] ([GroomerReserveId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    CREATE INDEX [IX_SettlementGroomers_SettlementId] ON [SettlementGroomers] ([SettlementId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    ALTER TABLE [GroomerComments] ADD CONSTRAINT [FK_GroomerComments_GroomerReserves_GroomerReserveId] FOREIGN KEY ([GroomerReserveId]) REFERENCES [GroomerReserves] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    ALTER TABLE [Wallets] ADD CONSTRAINT [FK_Wallets_GroomerReserves_GroomerReserveId] FOREIGN KEY ([GroomerReserveId]) REFERENCES [GroomerReserves] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111501_AddGroomerReserves'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261019111501_AddGroomerReserves', N'9.0.0');
END;

COMMIT;
GO

