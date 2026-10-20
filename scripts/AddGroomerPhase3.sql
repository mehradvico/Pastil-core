BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019120720_AddGroomerPhase3'
)
BEGIN
    ALTER TABLE [GroomerUsers] ADD [InviteDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019120720_AddGroomerPhase3'
)
BEGIN
    ALTER TABLE [GroomerReserves] ADD [LastRescheduleDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019120720_AddGroomerPhase3'
)
BEGIN
    ALTER TABLE [GroomerReserves] ADD [OriginalDoDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019120720_AddGroomerPhase3'
)
BEGIN
    ALTER TABLE [GroomerReserves] ADD [RescheduleCount] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019120720_AddGroomerPhase3'
)
BEGIN
    CREATE TABLE [GroomerReserveItems] (
        [Id] bigint NOT NULL IDENTITY,
        [GroomerReserveId] bigint NOT NULL,
        [GroomerServiceId] bigint NOT NULL,
        [GroomerServicePackageId] bigint NOT NULL,
        [PackagePrice] float NOT NULL,
        [OnlineAmount] float NOT NULL,
        [DurationMinutes] int NOT NULL,
        [CommissionPercent] decimal(5,2) NOT NULL,
        [ProviderShare] float NOT NULL,
        [SiteShare] float NOT NULL,
        CONSTRAINT [PK_GroomerReserveItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GroomerReserveItems_GroomerReserves_GroomerReserveId] FOREIGN KEY ([GroomerReserveId]) REFERENCES [GroomerReserves] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerReserveItems_GroomerServicePackages_GroomerServicePackageId] FOREIGN KEY ([GroomerServicePackageId]) REFERENCES [GroomerServicePackages] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GroomerReserveItems_GroomerServices_GroomerServiceId] FOREIGN KEY ([GroomerServiceId]) REFERENCES [GroomerServices] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019120720_AddGroomerPhase3'
)
BEGIN
    CREATE INDEX [IX_GroomerReserveItems_GroomerReserveId] ON [GroomerReserveItems] ([GroomerReserveId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019120720_AddGroomerPhase3'
)
BEGIN
    CREATE INDEX [IX_GroomerReserveItems_GroomerServiceId] ON [GroomerReserveItems] ([GroomerServiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019120720_AddGroomerPhase3'
)
BEGIN
    CREATE INDEX [IX_GroomerReserveItems_GroomerServicePackageId] ON [GroomerReserveItems] ([GroomerServicePackageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019120720_AddGroomerPhase3'
)
BEGIN
    INSERT INTO GroomerReserveItems (GroomerReserveId, GroomerServiceId, GroomerServicePackageId, PackagePrice, OnlineAmount, DurationMinutes, CommissionPercent, ProviderShare, SiteShare)
    SELECT r.Id, r.GroomerServiceId, r.GroomerServicePackageId, r.PackagePrice, r.OnlineAmount, r.DurationMinutes, r.CommissionPercent, r.ProviderShare, r.SiteShare
    FROM GroomerReserves r
    WHERE NOT EXISTS (SELECT 1 FROM GroomerReserveItems i WHERE i.GroomerReserveId = r.Id);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019120720_AddGroomerPhase3'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'ScoreTransactionType_GroomerReserve')
    BEGIN
        DECLARE @ScoreGroupId bigint = (SELECT TOP 1 CodeGroupId FROM Codes WHERE Label = 'ScoreTransactionType_PansionReserve');
        IF @ScoreGroupId IS NOT NULL
            INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
            VALUES ('ScoreTransactionType_GroomerReserve', 'groomer-reserve', @ScoreGroupId,
                    (SELECT ISNULL(MAX(Priority), 0) + 1 FROM Codes WHERE CodeGroupId = @ScoreGroupId), 1, N'رزرو آرایشگاه');
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019120720_AddGroomerPhase3'
)
BEGIN
    DECLARE @Types TABLE (Id bigint, Name nvarchar(200), Label nvarchar(200), Title nvarchar(200), Body nvarchar(500), Url nvarchar(200), Tag nvarchar(200));
    INSERT INTO @Types (Id, Name, Label, Title, Body, Url, Tag) VALUES
        (131, N'آرایشگاه تأیید شد (به مالک)', N'PushGroomerApproved', N'آرایشگاه شما تأیید شد', N'آرایشگاه {0} تأیید شد و اکنون در پاستیل نمایش داده می‌شود.', N'/companionProfile', N'groomer-approved'),
        (132, N'آرایشگاه رد شد (به مالک)', N'PushGroomerRejected', N'آرایشگاه شما رد شد', N'آرایشگاه {0} رد شد. دلیل: {1}', N'/companionProfile', N'groomer-rejected'),
        (133, N'تغییر زمان رزرو آرایشگاه (به کاربر)', N'PushGroomerReserveRescheduled', N'زمان رزرو شما تغییر کرد', N'زمان رزرو شما در {0} به {1} ساعت {2} تغییر کرد.', N'/reserve', N'groomer-reserve-rescheduled'),
        (134, N'تغییر زمان رزرو آرایشگاه (به آرایشگاه)', N'PushGroomerReserveRescheduledForGroomer', N'زمان رزرو تغییر کرد', N'زمان رزرو {0} به {1} ساعت {2} تغییر کرد.', N'/companionProfile/groomerReserve', N'groomer-reserve-rescheduled-groomer'),
        (135, N'دعوت به پرسنل آرایشگاه (به کاربر)', N'PushGroomerStaffInvite', N'دعوت به همکاری', N'شما به پرسنل {0} دعوت شده‌اید. برای پذیرش وارد اپ شوید.', N'/', N'groomer-staff-invite'),
        (136, N'لغو رزرو آرایشگاه توسط پشتیبانی (به کاربر)', N'PushGroomerReserveCancelledByAdmin', N'رزرو شما لغو شد', N'رزرو شما در {0} توسط پشتیبانی لغو شد. دلیل: {1}', N'/reserve', N'groomer-reserve-cancelled-admin');

    IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Id = s.Id WHERE t.Label <> s.Label)
        THROW 51000, 'A groomer push type ID is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Label = s.Label WHERE t.Id <> s.Id)
        THROW 51000, 'A groomer push label is already assigned to another ID.', 1;

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
    WHERE pattern.PushTypeId IN (131, 132, 133, 134, 135, 136)
      AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019120720_AddGroomerPhase3'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261019120720_AddGroomerPhase3', N'9.0.0');
END;

COMMIT;
GO

