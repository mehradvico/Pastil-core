BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    ALTER TABLE [Trips] ADD [ClinicReserveId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    ALTER TABLE [OnlinePrescriptions] ADD [ClinicReserveId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    ALTER TABLE [ClinicReserves] ADD [CallEndDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    ALTER TABLE [ClinicReserves] ADD [CallStartDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    ALTER TABLE [ClinicReserveItems] ADD [AddedAfterPayment] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    ALTER TABLE [ClinicReserveItems] ADD [CreateDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    ALTER TABLE [ClinicReserveItems] ADD [PackageName] nvarchar(300) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    ALTER TABLE [ClinicReserveItems] ADD [RefundAmount] float NOT NULL DEFAULT 0.0E0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    ALTER TABLE [ClinicReserveItems] ADD [RefundDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    ALTER TABLE [ClinicReserveItems] ADD [Status] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    ALTER TABLE [ClinicReserveItems] ADD [StatusChangedByAdmin] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    ALTER TABLE [ClinicReserveItems] ADD [StatusChangedByUserId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    ALTER TABLE [ClinicReserveItems] ADD [StatusChangedDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    ALTER TABLE [ClinicReserveItems] ADD [StatusReason] nvarchar(1000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    CREATE TABLE [ClinicReserveMessages] (
        [Id] bigint NOT NULL IDENTITY,
        [ClinicReserveId] bigint NOT NULL,
        [SenderUserId] bigint NULL,
        [ClinicReserveMessageTypeId] bigint NOT NULL,
        [ReplyToMessageId] bigint NULL,
        [Content] nvarchar(max) NULL,
        [DeliveredDate] datetime2 NULL,
        [ReadDate] datetime2 NULL,
        [Deleted] bit NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        CONSTRAINT [PK_ClinicReserveMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClinicReserveMessages_ClinicReserveMessages_ReplyToMessageId] FOREIGN KEY ([ReplyToMessageId]) REFERENCES [ClinicReserveMessages] ([Id]),
        CONSTRAINT [FK_ClinicReserveMessages_ClinicReserves_ClinicReserveId] FOREIGN KEY ([ClinicReserveId]) REFERENCES [ClinicReserves] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicReserveMessages_Codes_ClinicReserveMessageTypeId] FOREIGN KEY ([ClinicReserveMessageTypeId]) REFERENCES [Codes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicReserveMessages_Users_SenderUserId] FOREIGN KEY ([SenderUserId]) REFERENCES [Users] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    CREATE TABLE [ClinicReserveMessageAttachments] (
        [Id] bigint NOT NULL IDENTITY,
        [ClinicReserveMessageId] bigint NOT NULL,
        [Url] nvarchar(max) NULL,
        [ThumbnailUrl] nvarchar(max) NULL,
        [FileName] nvarchar(max) NULL,
        [ContentType] nvarchar(max) NULL,
        [FileSize] bigint NOT NULL,
        [Duration] int NULL,
        [Width] int NULL,
        [Height] int NULL,
        [Order] int NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_ClinicReserveMessageAttachments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClinicReserveMessageAttachments_ClinicReserveMessages_ClinicReserveMessageId] FOREIGN KEY ([ClinicReserveMessageId]) REFERENCES [ClinicReserveMessages] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    CREATE TABLE [ClinicReserveMessageReactions] (
        [Id] bigint NOT NULL IDENTITY,
        [ClinicReserveMessageId] bigint NOT NULL,
        [ReactorUserId] bigint NOT NULL,
        [Reaction] nvarchar(max) NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_ClinicReserveMessageReactions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClinicReserveMessageReactions_ClinicReserveMessages_ClinicReserveMessageId] FOREIGN KEY ([ClinicReserveMessageId]) REFERENCES [ClinicReserveMessages] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClinicReserveMessageReactions_Users_ReactorUserId] FOREIGN KEY ([ReactorUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    CREATE INDEX [IX_Trips_ClinicReserveId] ON [Trips] ([ClinicReserveId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_OnlinePrescriptions_ClinicReserveId] ON [OnlinePrescriptions] ([ClinicReserveId]) WHERE [ClinicReserveId] IS NOT NULL AND [Deleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    CREATE INDEX [IX_ClinicReserveMessageAttachments_ClinicReserveMessageId] ON [ClinicReserveMessageAttachments] ([ClinicReserveMessageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    CREATE INDEX [IX_ClinicReserveMessageReactions_ClinicReserveMessageId] ON [ClinicReserveMessageReactions] ([ClinicReserveMessageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    CREATE INDEX [IX_ClinicReserveMessageReactions_ReactorUserId] ON [ClinicReserveMessageReactions] ([ReactorUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    CREATE INDEX [IX_ClinicReserveMessages_ClinicReserveId] ON [ClinicReserveMessages] ([ClinicReserveId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    CREATE INDEX [IX_ClinicReserveMessages_ClinicReserveMessageTypeId] ON [ClinicReserveMessages] ([ClinicReserveMessageTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    CREATE INDEX [IX_ClinicReserveMessages_ReplyToMessageId] ON [ClinicReserveMessages] ([ReplyToMessageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    CREATE INDEX [IX_ClinicReserveMessages_SenderUserId] ON [ClinicReserveMessages] ([SenderUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    ALTER TABLE [OnlinePrescriptions] ADD CONSTRAINT [FK_OnlinePrescriptions_ClinicReserves_ClinicReserveId] FOREIGN KEY ([ClinicReserveId]) REFERENCES [ClinicReserves] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    ALTER TABLE [Trips] ADD CONSTRAINT [FK_Trips_ClinicReserves_ClinicReserveId] FOREIGN KEY ([ClinicReserveId]) REFERENCES [ClinicReserves] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    DECLARE @Types TABLE (Id bigint, Name nvarchar(200), Label nvarchar(200), Title nvarchar(200), Body nvarchar(500), Url nvarchar(200), Tag nvarchar(200));
    INSERT INTO @Types (Id, Name, Label, Title, Body, Url, Tag) VALUES
        (148, N'تأیید بسته‌ی رزرو کلینیک (به کاربر)', N'PushClinicReserveItemApproved', N'بسته‌ی شما تأیید شد', N'کلینیک بسته «{0}» رزرو شما را تأیید کرد.', N'/reserve', N'clinic-reserve-item-approved'),
        (149, N'لغو بسته‌ی رزرو کلینیک (به کاربر)', N'PushClinicReserveItemCancelled', N'بسته‌ی رزرو لغو شد', N'بسته «{0}» رزرو شما لغو شد و {3} تومان به کیف پول شما برگشت داده شد.', N'/reserve', N'clinic-reserve-item-cancelled'),
        (150, N'پیام جدید چت رزرو کلینیک', N'PushClinicReserveNewMessage', N'پیام جدید در چت کلینیک', N'{0}: {1}', N'/companion-chat/{2}', N'clinic-reserve-new-message'),
        (151, N'شروع تماس درون‌برنامه‌ای توسط کلینیک', N'PushClinicInAppCallStarted', N'تماس درون‌برنامه‌ای', N'{0} با شما تماس می‌گیرد. برای پاسخ ضربه بزنید.', N'/call/{1}', N'clinic-in-app-call-started'),
        (152, N'یادآوری ۱۰ دقیقه مانده به خدمت آنلاین کلینیک', N'PushClinicOnlineReserveReminderBefore', N'۱۰ دقیقه تا خدمت آنلاین', N'۱۰ دقیقه دیگر باید با {0} درباره‌ی «{1}» از طریق «{2}» ارتباط برقرار کنید.', N'/operator', N'clinic-online-reserve-reminder-before'),
        (153, N'یادآوری سر زمان خدمت آنلاین کلینیک', N'PushClinicOnlineReserveReminderAtTime', N'زمان خدمت آنلاین رسید', N'همین الان باید با {0} درباره‌ی «{1}» از طریق «{2}» ارتباط برقرار کنید.', N'/operator', N'clinic-online-reserve-reminder-attime');

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
    WHERE pattern.PushTypeId IN (148, 149, 150, 151, 152, 153)
      AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020070912_AddClinicReserveLinks'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261020070912_AddClinicReserveLinks', N'9.0.0');
END;

COMMIT;
GO

