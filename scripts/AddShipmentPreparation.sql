BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005085339_AddStorePreparationAndShipmentReady'
)
BEGIN
    ALTER TABLE [Stores] ADD [MaxPreparationMinutes] int NOT NULL DEFAULT 120;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005085339_AddStorePreparationAndShipmentReady'
)
BEGIN
    ALTER TABLE [Shipments] ADD [ReadyAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005085339_AddStorePreparationAndShipmentReady'
)
BEGIN
    ALTER TABLE [Shipments] ADD [ReadyDeadlineUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005085339_AddStorePreparationAndShipmentReady'
)
BEGIN
    ALTER TABLE [Shipments] ADD [ReadyReminderSentAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005085339_AddStorePreparationAndShipmentReady'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261005085339_AddStorePreparationAndShipmentReady', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005085426_SeedShipmentReadyPush'
)
BEGIN
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 103 AND Label <> N'PushShipmentReadyReminder')
        THROW 51000, 'PushType ID 103 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushShipmentReadyReminder' AND Id <> 103)
        THROW 51000, 'PushShipmentReadyReminder is already assigned to another ID.', 1;

    SET IDENTITY_INSERT PushTypes ON;

    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 103)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (103, N'یادآوری «آماده تحویل به پیک» به فروشنده', N'PushShipmentReadyReminder');

    SET IDENTITY_INSERT PushTypes OFF;

    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 103)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (103, N'مهلت آماده‌سازی رو به پایان است', N'سفارش {0} باید تا ساعت {1} «آماده تحویل به پیک» شود، وگرنه لغو می‌شود.', N'/sellerProfile/orders', NULL, N'shipment-ready-reminder', 1);

    INSERT INTO PushSettings (PushPatternId, IsEnabled)
    SELECT pattern.Id, 1
    FROM PushPatterns pattern
    WHERE pattern.PushTypeId = 103
      AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);

    -- متن پوش «پیک لغو شد» عوض شد: حالا فروشنده «آماده تحویل به پیک» را دوباره می‌زند (نه تأیید سفارش)
    UPDATE PushPatterns
    SET Body = N'پیک میاره برای سفارش {0} لغو شد. تا ساعت {1} دوباره «آماده تحویل به پیک» را بزنید.'
    WHERE PushTypeId = 97;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005085426_SeedShipmentReadyPush'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261005085426_SeedShipmentReadyPush', N'9.0.0');
END;

COMMIT;
GO

