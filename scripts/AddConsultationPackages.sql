BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921082038_AddConsultationPackages'
)
BEGIN
    ALTER TABLE [OnlineSessions] ADD [ConsultationPurchaseId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921082038_AddConsultationPackages'
)
BEGIN
    ALTER TABLE [OnlineSessions] ADD [ExpireDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921082038_AddConsultationPackages'
)
BEGIN
    CREATE TABLE [ConsultationPackages] (
        [Id] bigint NOT NULL IDENTITY,
        [CompanionId] bigint NOT NULL,
        [ChannelId] int NOT NULL,
        [DurationMinutes] int NOT NULL,
        [Price] float NOT NULL,
        [Active] bit NOT NULL,
        [Deleted] bit NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [UpdateDate] datetime2 NULL,
        CONSTRAINT [PK_ConsultationPackages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ConsultationPackages_Companions_CompanionId] FOREIGN KEY ([CompanionId]) REFERENCES [Companions] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921082038_AddConsultationPackages'
)
BEGIN
    CREATE TABLE [ConsultationPurchases] (
        [Id] bigint NOT NULL IDENTITY,
        [PurchaseCode] nvarchar(40) NULL,
        [UserId] bigint NOT NULL,
        [ConsultationPackageId] bigint NOT NULL,
        [CompanionId] bigint NOT NULL,
        [ChannelId] int NOT NULL,
        [DurationMinutes] int NOT NULL,
        [Price] float NOT NULL,
        [Status] int NOT NULL,
        [FromWallet] bit NOT NULL,
        [WalletPrice] float NOT NULL,
        [PaymentPrice] float NOT NULL,
        [RebateId] bigint NULL,
        [RebatePrice] float NOT NULL,
        [Discount] float NOT NULL,
        [PaymentId] bigint NULL,
        [PaidDate] datetime2 NULL,
        [CompanionShare] float NOT NULL,
        [SiteShare] float NOT NULL,
        [StartDeadline] datetime2 NULL,
        [AgentUserId] bigint NULL,
        [OnlineSessionId] bigint NULL,
        [StartDate] datetime2 NULL,
        [ExpireDate] datetime2 NULL,
        [CancelDate] datetime2 NULL,
        [CancelReason] nvarchar(500) NULL,
        [RefundDate] datetime2 NULL,
        [CreateDate] datetime2 NOT NULL,
        CONSTRAINT [PK_ConsultationPurchases] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ConsultationPurchases_Companions_CompanionId] FOREIGN KEY ([CompanionId]) REFERENCES [Companions] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ConsultationPurchases_ConsultationPackages_ConsultationPackageId] FOREIGN KEY ([ConsultationPackageId]) REFERENCES [ConsultationPackages] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ConsultationPurchases_OnlineSessions_OnlineSessionId] FOREIGN KEY ([OnlineSessionId]) REFERENCES [OnlineSessions] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ConsultationPurchases_Rebate_RebateId] FOREIGN KEY ([RebateId]) REFERENCES [Rebate] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ConsultationPurchases_Users_AgentUserId] FOREIGN KEY ([AgentUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ConsultationPurchases_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921082038_AddConsultationPackages'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ConsultationPackages_CompanionId_ChannelId_DurationMinutes] ON [ConsultationPackages] ([CompanionId], [ChannelId], [DurationMinutes]) WHERE [Deleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921082038_AddConsultationPackages'
)
BEGIN
    CREATE INDEX [IX_ConsultationPurchases_AgentUserId] ON [ConsultationPurchases] ([AgentUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921082038_AddConsultationPackages'
)
BEGIN
    CREATE INDEX [IX_ConsultationPurchases_CompanionId_Status] ON [ConsultationPurchases] ([CompanionId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921082038_AddConsultationPackages'
)
BEGIN
    CREATE INDEX [IX_ConsultationPurchases_ConsultationPackageId] ON [ConsultationPurchases] ([ConsultationPackageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921082038_AddConsultationPackages'
)
BEGIN
    CREATE INDEX [IX_ConsultationPurchases_OnlineSessionId] ON [ConsultationPurchases] ([OnlineSessionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921082038_AddConsultationPackages'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ConsultationPurchases_PurchaseCode] ON [ConsultationPurchases] ([PurchaseCode]) WHERE [PurchaseCode] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921082038_AddConsultationPackages'
)
BEGIN
    CREATE INDEX [IX_ConsultationPurchases_RebateId] ON [ConsultationPurchases] ([RebateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921082038_AddConsultationPackages'
)
BEGIN
    CREATE INDEX [IX_ConsultationPurchases_Status_ExpireDate] ON [ConsultationPurchases] ([Status], [ExpireDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921082038_AddConsultationPackages'
)
BEGIN
    CREATE INDEX [IX_ConsultationPurchases_Status_StartDeadline] ON [ConsultationPurchases] ([Status], [StartDeadline]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921082038_AddConsultationPackages'
)
BEGIN
    CREATE INDEX [IX_ConsultationPurchases_UserId_Status] ON [ConsultationPurchases] ([UserId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921082038_AddConsultationPackages'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260921082038_AddConsultationPackages', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921083448_AddConsultationPurchasePayment'
)
BEGIN
    ALTER TABLE [Wallets] ADD [ConsultationPurchaseId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921083448_AddConsultationPurchasePayment'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Wallets_ConsultationPurchaseId] ON [Wallets] ([ConsultationPurchaseId]) WHERE [ConsultationPurchaseId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921083448_AddConsultationPurchasePayment'
)
BEGIN
    ALTER TABLE [Wallets] ADD CONSTRAINT [FK_Wallets_ConsultationPurchases_ConsultationPurchaseId] FOREIGN KEY ([ConsultationPurchaseId]) REFERENCES [ConsultationPurchases] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921083448_AddConsultationPurchasePayment'
)
BEGIN

    DECLARE @PaymentTypeGroupId BIGINT = (SELECT TOP (1) CodeGroupId FROM Codes WHERE Label = 'PaymentType_PansionReserve');
    IF @PaymentTypeGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'PaymentType_ConsultationPurchase')
    BEGIN
        DECLARE @NextPaymentPriority INT = (SELECT ISNULL(MAX(Priority), 0) + 1 FROM Codes WHERE CodeGroupId = @PaymentTypeGroupId);
        INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
        VALUES ('PaymentType_ConsultationPurchase', 'consultation-purchase', @PaymentTypeGroupId, @NextPaymentPriority, 1, N'پرداخت خرید مشاوره آنلاین');
    END

    DECLARE @RebateTypeGroupId BIGINT = (SELECT TOP (1) CodeGroupId FROM Codes WHERE Label = 'RebateType_PansionReserve');
    IF @RebateTypeGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'RebateType_ConsultationPurchase')
    BEGIN
        DECLARE @NextRebatePriority INT = (SELECT ISNULL(MAX(Priority), 0) + 1 FROM Codes WHERE CodeGroupId = @RebateTypeGroupId);
        INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
        VALUES ('RebateType_ConsultationPurchase', 'consultation-purchase', @RebateTypeGroupId, @NextRebatePriority, 1, N'تخفیف خرید مشاوره آنلاین');
    END

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921083448_AddConsultationPurchasePayment'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260921083448_AddConsultationPurchasePayment', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921085525_AddConsultationPushTypes'
)
BEGIN

    DECLARE @PushTypes TABLE (Id BIGINT NOT NULL, Name NVARCHAR(200) NOT NULL, Label NVARCHAR(200) NOT NULL, Title NVARCHAR(200) NOT NULL, Body NVARCHAR(500) NOT NULL, Url NVARCHAR(500) NOT NULL, Tag NVARCHAR(100) NOT NULL);

    INSERT INTO @PushTypes (Id, Name, Label, Title, Body, Url, Tag)
    VALUES
        (71, N'خرید مشاوره آنلاین (اعلان به نماینده)', N'PushConsultationPurchasedAgent', N'رزرو مشاوره جدید', N'{0} یک مشاوره‌ی {2} ({3} دقیقه‌ای) خریده است؛ بیا ارتباط را برقرار کن.', N'/consultations/manage', N'consultation-purchased-agent'),
        (72, N'خرید مشاوره آنلاین (اعلان به کاربر)', N'PushConsultationPurchasedUser', N'مشاوره‌ی شما ثبت شد', N'مشاوره‌ی شما در {0} ثبت شد؛ منتظر شروع نماینده باشید.', N'/consultations', N'consultation-purchased-user'),
        (73, N'نزدیک پایان پنجره‌ی مشاوره', N'PushConsultationEndingSoon', N'پایان نزدیک مشاوره', N'۵ دقیقه‌ی دیگر پنجره‌ی مشاوره با {0} تمام می‌شود.', N'{2}', N'consultation-ending-soon'),
        (74, N'بازپرداخت مشاوره‌ی شروع‌نشده', N'PushConsultationExpiredRefund', N'بازپرداخت مشاوره', N'مشاوره‌ی شما در {0} شروع نشد؛ مبلغ به کیف پول شما برگشت.', N'/consultations', N'consultation-expired-refund');

    IF EXISTS (SELECT 1 FROM @PushTypes s INNER JOIN PushTypes t ON t.Id = s.Id WHERE t.Label <> s.Label)
        THROW 51000, 'A consultation PushType ID is already assigned to another label.', 1;

    IF EXISTS (SELECT 1 FROM @PushTypes s INNER JOIN PushTypes t ON t.Label = s.Label WHERE t.Id <> s.Id)
        THROW 51000, 'A consultation PushType label is already assigned to another ID.', 1;

    SET IDENTITY_INSERT PushTypes ON;

    INSERT INTO PushTypes (Id, Name, Label)
    SELECT s.Id, s.Name, s.Label FROM @PushTypes s WHERE NOT EXISTS (SELECT 1 FROM PushTypes t WHERE t.Id = s.Id);

    SET IDENTITY_INSERT PushTypes OFF;

    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
    SELECT s.Id, s.Title, s.Body, s.Url, NULL, s.Tag, 1 FROM @PushTypes s
    WHERE NOT EXISTS (SELECT 1 FROM PushPatterns p WHERE p.PushTypeId = s.Id);

    INSERT INTO PushSettings (PushPatternId, IsEnabled)
    SELECT p.Id, 1 FROM PushPatterns p
    WHERE p.PushTypeId IN (71, 72, 73, 74) AND NOT EXISTS (SELECT 1 FROM PushSettings x WHERE x.PushPatternId = p.Id);

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921085525_AddConsultationPushTypes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260921085525_AddConsultationPushTypes', N'9.0.0');
END;

COMMIT;
GO

