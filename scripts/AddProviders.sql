BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018120629_AddProviders'
)
BEGIN
    ALTER TABLE [Users] ADD [IsProvider] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018120629_AddProviders'
)
BEGIN
    ALTER TABLE [Stores] ADD [ProviderId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018120629_AddProviders'
)
BEGIN
    ALTER TABLE [Companions] ADD [ProviderId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018120629_AddProviders'
)
BEGIN
    CREATE TABLE [Providers] (
        [Id] bigint NOT NULL IDENTITY,
        [UserId] bigint NOT NULL,
        [Active] bit NOT NULL,
        [Approved] bit NOT NULL,
        [Deleted] bit NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [Note] nvarchar(max) NULL,
        CONSTRAINT [PK_Providers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Providers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018120629_AddProviders'
)
BEGIN
    CREATE TABLE [SubscriptionPlans] (
        [Id] bigint NOT NULL IDENTITY,
        [Level] int NOT NULL,
        [Active] bit NOT NULL,
        [Deleted] bit NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_SubscriptionPlans] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018120629_AddProviders'
)
BEGIN
    CREATE TABLE [ProviderCapabilities] (
        [Id] bigint NOT NULL IDENTITY,
        [ProviderId] bigint NOT NULL,
        [Type] int NOT NULL,
        [Approved] bit NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_ProviderCapabilities] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProviderCapabilities_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018120629_AddProviders'
)
BEGIN
    CREATE TABLE [ProviderSubscriptions] (
        [Id] bigint NOT NULL IDENTITY,
        [ProviderId] bigint NOT NULL,
        [PlanId] bigint NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [CreatedByUserId] bigint NULL,
        [CreateDate] datetime2 NOT NULL,
        [Note] nvarchar(500) NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_ProviderSubscriptions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProviderSubscriptions_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProviderSubscriptions_SubscriptionPlans_PlanId] FOREIGN KEY ([PlanId]) REFERENCES [SubscriptionPlans] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018120629_AddProviders'
)
BEGIN
    CREATE INDEX [IX_Stores_ProviderId] ON [Stores] ([ProviderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018120629_AddProviders'
)
BEGIN
    CREATE INDEX [IX_Companions_ProviderId] ON [Companions] ([ProviderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018120629_AddProviders'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ProviderCapabilities_ProviderId_Type] ON [ProviderCapabilities] ([ProviderId], [Type]) WHERE [Deleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018120629_AddProviders'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Providers_UserId] ON [Providers] ([UserId]) WHERE [Deleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018120629_AddProviders'
)
BEGIN
    CREATE INDEX [IX_ProviderSubscriptions_PlanId] ON [ProviderSubscriptions] ([PlanId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018120629_AddProviders'
)
BEGIN
    CREATE INDEX [IX_ProviderSubscriptions_ProviderId_EndDate] ON [ProviderSubscriptions] ([ProviderId], [EndDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018120629_AddProviders'
)
BEGIN
    CREATE INDEX [IX_SubscriptionPlans_Level] ON [SubscriptionPlans] ([Level]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018120629_AddProviders'
)
BEGIN
    ALTER TABLE [Companions] ADD CONSTRAINT [FK_Companions_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018120629_AddProviders'
)
BEGIN
    ALTER TABLE [Stores] ADD CONSTRAINT [FK_Stores_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018120629_AddProviders'
)
BEGIN

    INSERT INTO SubscriptionPlans (Name, Level, Active, Deleted) VALUES
     (N'نقره‌ای', 1, 1, 0), (N'طلایی', 2, 1, 0), (N'الماس', 3, 1, 0);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018120629_AddProviders'
)
BEGIN

    INSERT INTO Providers (UserId, Active, Approved, Deleted, CreateDate)
    SELECT o.UserId, 1, 1, 0, GETDATE()
    FROM (
        SELECT c.OwnerId AS UserId FROM Companions c WHERE c.Deleted = 0
        UNION
        SELECT MIN(su.UsersId) FROM StoreUser su JOIN Stores s ON s.Id = su.StoresId WHERE s.Deleted = 0 GROUP BY su.StoresId
    ) o
    JOIN Users u ON u.Id = o.UserId AND u.Deleted = 0;

    UPDATE u SET u.IsProvider = 1 FROM Users u JOIN Providers p ON p.UserId = u.Id AND p.Deleted = 0;

    UPDATE c SET c.ProviderId = p.Id FROM Companions c JOIN Providers p ON p.UserId = c.OwnerId AND p.Deleted = 0 WHERE c.Deleted = 0;

    UPDATE s SET s.ProviderId = p.Id
    FROM Stores s
    JOIN (SELECT StoresId, MIN(UsersId) AS OwnerUserId FROM StoreUser GROUP BY StoresId) so ON so.StoresId = s.Id
    JOIN Providers p ON p.UserId = so.OwnerUserId AND p.Deleted = 0
    WHERE s.Deleted = 0;

    -- تخصص‌ها: فروشنده (دارای فروشگاه) و پانسیون‌دار (دارای پانسیون)؛ تایید = تایید فروشگاه/پانسیون فعلی
    INSERT INTO ProviderCapabilities (ProviderId, Type, Approved, Deleted)
    SELECT s.ProviderId, 1, MAX(CAST(s.Approved AS int)), 0 FROM Stores s WHERE s.ProviderId IS NOT NULL AND s.Deleted = 0 GROUP BY s.ProviderId;
    INSERT INTO ProviderCapabilities (ProviderId, Type, Approved, Deleted)
    SELECT c.ProviderId, 5, MAX(CAST(pa.Approve AS int)), 0
    FROM Pansions pa JOIN Companions c ON c.Id = pa.CompanionId
    WHERE c.ProviderId IS NOT NULL AND pa.Deleted = 0 GROUP BY c.ProviderId;

    -- اشتراک قدیمی (طلایی/نقره‌ای روی Companion) → اشتراک مالک؛ بالاترین سطحِ هنوز-معتبر
    INSERT INTO ProviderSubscriptions (ProviderId, PlanId, StartDate, EndDate, CreatedByUserId, CreateDate, Note, Deleted)
    SELECT x.ProviderId, (SELECT TOP 1 Id FROM SubscriptionPlans WHERE Level = x.Level), GETDATE(), x.EndDate, NULL, GETDATE(), N'Migrated from legacy Companion account', 0
    FROM (
        SELECT c.ProviderId,
               CASE WHEN MAX(c.GoldAccountDate) > GETDATE() THEN 2 ELSE 1 END AS Level,
               CASE WHEN MAX(c.GoldAccountDate) > GETDATE() THEN MAX(c.GoldAccountDate) ELSE MAX(c.SilverAccountDate) END AS EndDate
        FROM Companions c
        WHERE c.ProviderId IS NOT NULL AND c.Deleted = 0
          AND (c.GoldAccountDate > GETDATE() OR c.SilverAccountDate > GETDATE())
        GROUP BY c.ProviderId
    ) x;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018120629_AddProviders'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261018120629_AddProviders', N'9.0.0');
END;

COMMIT;
GO


-- ============================================================
-- بعد از اجرا بازبینی کنید: فروشگاه‌های چندکاربره (صاحب = کوچک‌ترین UsersId انتخاب شده؛ اگر درست نیست، از پنل
-- «ارائه‌دهندگان ← مکان‌ها» فروشگاه را به ارائه‌دهنده‌ی درست وصل کنید).
-- SELECT s.Id, s.Name, COUNT(*) AS Users, MIN(su.UsersId) AS AssumedOwnerUserId
-- FROM Stores s JOIN StoreUser su ON su.StoresId = s.Id WHERE s.Deleted = 0
-- GROUP BY s.Id, s.Name HAVING COUNT(*) > 1;
-- ============================================================
