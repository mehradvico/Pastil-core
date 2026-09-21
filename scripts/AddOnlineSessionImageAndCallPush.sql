BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920224122_AddOnlineSessionMessageImage'
)
BEGIN
    ALTER TABLE [OnlineSessionMessages] ADD [ImageThumbnailUrl] nvarchar(2048) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920224122_AddOnlineSessionMessageImage'
)
BEGIN
    ALTER TABLE [OnlineSessionMessages] ADD [ImageUrl] nvarchar(2048) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920224122_AddOnlineSessionMessageImage'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260920224122_AddOnlineSessionMessageImage', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920230744_AddOnlineSessionCallPush'
)
BEGIN

    DECLARE @PushTypeId BIGINT = 68;
    DECLARE @PushTypeLabel NVARCHAR(200) = N'PushOnlineSessionCallStarted';

    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = @PushTypeId AND Label <> @PushTypeLabel)
        THROW 51000, 'PushType ID 68 is already assigned to another label.', 1;

    IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = @PushTypeLabel AND Id <> @PushTypeId)
        THROW 51000, 'PushOnlineSessionCallStarted label is already assigned to another ID.', 1;

    SET IDENTITY_INSERT PushTypes ON;

    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = @PushTypeId)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (@PushTypeId, N'شروع تماس درون‌برنامه‌ای جلسه‌ی آنلاین', @PushTypeLabel);

    SET IDENTITY_INSERT PushTypes OFF;

    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = @PushTypeId)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (@PushTypeId, N'تماس درون‌برنامه‌ای', N'{0} با شما تماس می‌گیرد. برای پاسخ ضربه بزنید.', N'/call/session/{1}', NULL, N'online-session-call', 1);

    INSERT INTO PushSettings (PushPatternId, IsEnabled)
    SELECT p.Id, 1 FROM PushPatterns p
    WHERE p.PushTypeId = @PushTypeId AND NOT EXISTS (SELECT 1 FROM PushSettings x WHERE x.PushPatternId = p.Id);

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920230744_AddOnlineSessionCallPush'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260920230744_AddOnlineSessionCallPush', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921071237_AddOnlineSessionVideoCallPush'
)
BEGIN

    DECLARE @PushTypeId BIGINT = 69;
    DECLARE @PushTypeLabel NVARCHAR(200) = N'PushOnlineSessionVideoCallStarted';

    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = @PushTypeId AND Label <> @PushTypeLabel)
        THROW 51000, 'PushType ID 69 is already assigned to another label.', 1;

    IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = @PushTypeLabel AND Id <> @PushTypeId)
        THROW 51000, 'PushOnlineSessionVideoCallStarted label is already assigned to another ID.', 1;

    SET IDENTITY_INSERT PushTypes ON;

    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = @PushTypeId)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (@PushTypeId, N'شروع تماس تصویری جلسه‌ی آنلاین', @PushTypeLabel);

    SET IDENTITY_INSERT PushTypes OFF;

    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = @PushTypeId)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (@PushTypeId, N'تماس تصویری', N'{0} با شما تماس تصویری می‌گیرد. برای پاسخ ضربه بزنید.', N'/call/session/{1}', NULL, N'online-session-video-call', 1);

    INSERT INTO PushSettings (PushPatternId, IsEnabled)
    SELECT p.Id, 1 FROM PushPatterns p
    WHERE p.PushTypeId = @PushTypeId AND NOT EXISTS (SELECT 1 FROM PushSettings x WHERE x.PushPatternId = p.Id);

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921071237_AddOnlineSessionVideoCallPush'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260921071237_AddOnlineSessionVideoCallPush', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921075230_AddOnlineSessionPhoneCallPush'
)
BEGIN

    DECLARE @PushTypeId BIGINT = 70;
    DECLARE @PushTypeLabel NVARCHAR(200) = N'PushOnlineSessionPhoneCallStarted';

    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = @PushTypeId AND Label <> @PushTypeLabel)
        THROW 51000, 'PushType ID 70 is already assigned to another label.', 1;

    IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = @PushTypeLabel AND Id <> @PushTypeId)
        THROW 51000, 'PushOnlineSessionPhoneCallStarted label is already assigned to another ID.', 1;

    SET IDENTITY_INSERT PushTypes ON;

    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = @PushTypeId)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (@PushTypeId, N'شروع تماس تلفنی جلسه‌ی آنلاین', @PushTypeLabel);

    SET IDENTITY_INSERT PushTypes OFF;

    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = @PushTypeId)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (@PushTypeId, N'تماس تلفنی', N'{0} در حال تماس تلفنی با شماست. لطفاً تماس را پاسخ دهید.', N'/', NULL, N'online-session-phone-call', 1);

    INSERT INTO PushSettings (PushPatternId, IsEnabled)
    SELECT p.Id, 1 FROM PushPatterns p
    WHERE p.PushTypeId = @PushTypeId AND NOT EXISTS (SELECT 1 FROM PushSettings x WHERE x.PushPatternId = p.Id);

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921075230_AddOnlineSessionPhoneCallPush'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260921075230_AddOnlineSessionPhoneCallPush', N'9.0.0');
END;

COMMIT;
GO

