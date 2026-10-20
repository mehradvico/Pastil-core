BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020103441_AddCoachOnline'
)
BEGIN
    ALTER TABLE [CoachReserves] ADD [CallEndDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020103441_AddCoachOnline'
)
BEGIN
    ALTER TABLE [CoachReserves] ADD [CallStartDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020103441_AddCoachOnline'
)
BEGIN
    CREATE TABLE [CoachReserveMessages] (
        [Id] bigint NOT NULL IDENTITY,
        [CoachReserveId] bigint NOT NULL,
        [SenderUserId] bigint NULL,
        [CoachReserveMessageTypeId] bigint NOT NULL,
        [ReplyToMessageId] bigint NULL,
        [Content] nvarchar(max) NULL,
        [DeliveredDate] datetime2 NULL,
        [ReadDate] datetime2 NULL,
        [Deleted] bit NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        CONSTRAINT [PK_CoachReserveMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CoachReserveMessages_CoachReserveMessages_ReplyToMessageId] FOREIGN KEY ([ReplyToMessageId]) REFERENCES [CoachReserveMessages] ([Id]),
        CONSTRAINT [FK_CoachReserveMessages_CoachReserves_CoachReserveId] FOREIGN KEY ([CoachReserveId]) REFERENCES [CoachReserves] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachReserveMessages_Codes_CoachReserveMessageTypeId] FOREIGN KEY ([CoachReserveMessageTypeId]) REFERENCES [Codes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachReserveMessages_Users_SenderUserId] FOREIGN KEY ([SenderUserId]) REFERENCES [Users] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020103441_AddCoachOnline'
)
BEGIN
    CREATE TABLE [CoachReserveMessageAttachments] (
        [Id] bigint NOT NULL IDENTITY,
        [CoachReserveMessageId] bigint NOT NULL,
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
        CONSTRAINT [PK_CoachReserveMessageAttachments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CoachReserveMessageAttachments_CoachReserveMessages_CoachReserveMessageId] FOREIGN KEY ([CoachReserveMessageId]) REFERENCES [CoachReserveMessages] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020103441_AddCoachOnline'
)
BEGIN
    CREATE TABLE [CoachReserveMessageReactions] (
        [Id] bigint NOT NULL IDENTITY,
        [CoachReserveMessageId] bigint NOT NULL,
        [ReactorUserId] bigint NOT NULL,
        [Reaction] nvarchar(max) NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_CoachReserveMessageReactions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CoachReserveMessageReactions_CoachReserveMessages_CoachReserveMessageId] FOREIGN KEY ([CoachReserveMessageId]) REFERENCES [CoachReserveMessages] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoachReserveMessageReactions_Users_ReactorUserId] FOREIGN KEY ([ReactorUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020103441_AddCoachOnline'
)
BEGIN
    CREATE INDEX [IX_CoachReserveMessageAttachments_CoachReserveMessageId] ON [CoachReserveMessageAttachments] ([CoachReserveMessageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020103441_AddCoachOnline'
)
BEGIN
    CREATE INDEX [IX_CoachReserveMessageReactions_CoachReserveMessageId] ON [CoachReserveMessageReactions] ([CoachReserveMessageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020103441_AddCoachOnline'
)
BEGIN
    CREATE INDEX [IX_CoachReserveMessageReactions_ReactorUserId] ON [CoachReserveMessageReactions] ([ReactorUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020103441_AddCoachOnline'
)
BEGIN
    CREATE INDEX [IX_CoachReserveMessages_CoachReserveId] ON [CoachReserveMessages] ([CoachReserveId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020103441_AddCoachOnline'
)
BEGIN
    CREATE INDEX [IX_CoachReserveMessages_CoachReserveMessageTypeId] ON [CoachReserveMessages] ([CoachReserveMessageTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020103441_AddCoachOnline'
)
BEGIN
    CREATE INDEX [IX_CoachReserveMessages_ReplyToMessageId] ON [CoachReserveMessages] ([ReplyToMessageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020103441_AddCoachOnline'
)
BEGIN
    CREATE INDEX [IX_CoachReserveMessages_SenderUserId] ON [CoachReserveMessages] ([SenderUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020103441_AddCoachOnline'
)
BEGIN
    DECLARE @Types TABLE (Id bigint, Name nvarchar(200), Label nvarchar(200), Title nvarchar(200), Body nvarchar(500), Url nvarchar(200), Tag nvarchar(200));
    INSERT INTO @Types (Id, Name, Label, Title, Body, Url, Tag) VALUES
        (165, N'پیام جدید چت رزرو مربی', N'PushCoachReserveNewMessage', N'پیام جدید در چت مربی', N'{0}: {1}', N'/companion-chat/{2}', N'coach-reserve-new-message'),
        (166, N'شروع تماس درون‌برنامه‌ای توسط مربی', N'PushCoachInAppCallStarted', N'تماس درون‌برنامه‌ای', N'{0} با شما تماس می‌گیرد. برای پاسخ ضربه بزنید.', N'/call/{1}', N'coach-in-app-call-started'),
        (167, N'یادآوری ۱۰ دقیقه مانده به خدمت آنلاین مربی', N'PushCoachOnlineReserveReminderBefore', N'۱۰ دقیقه تا خدمت آنلاین', N'۱۰ دقیقه دیگر باید با {0} درباره‌ی «{1}» از طریق «{2}» ارتباط برقرار کنید.', N'/operator', N'coach-online-reserve-reminder-before'),
        (168, N'یادآوری سر زمان خدمت آنلاین مربی', N'PushCoachOnlineReserveReminderAtTime', N'زمان خدمت آنلاین رسید', N'همین الان باید با {0} درباره‌ی «{1}» از طریق «{2}» ارتباط برقرار کنید.', N'/operator', N'coach-online-reserve-reminder-attime');

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
    WHERE pattern.PushTypeId IN (165, 166, 167, 168)
      AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020103441_AddCoachOnline'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261020103441_AddCoachOnline', N'9.0.0');
END;

COMMIT;
GO

