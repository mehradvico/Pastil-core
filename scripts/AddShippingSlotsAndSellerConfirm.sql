BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005080318_AddShippingSlotsAndSellerConfirm'
)
BEGIN
    ALTER TABLE [Shipments] ADD [DeliveryCode] nvarchar(10) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005080318_AddShippingSlotsAndSellerConfirm'
)
BEGIN
    ALTER TABLE [Shipments] ADD [PickupDeadlineUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005080318_AddShippingSlotsAndSellerConfirm'
)
BEGIN
    ALTER TABLE [Shipments] ADD [SellerConfirmDeadlineUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005080318_AddShippingSlotsAndSellerConfirm'
)
BEGIN
    ALTER TABLE [Shipments] ADD [SellerConfirmedAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005080318_AddShippingSlotsAndSellerConfirm'
)
BEGIN
    ALTER TABLE [Shipments] ADD [SellerReminderSentAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005080318_AddShippingSlotsAndSellerConfirm'
)
BEGIN
    ALTER TABLE [Shipments] ADD [SlotEndUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005080318_AddShippingSlotsAndSellerConfirm'
)
BEGIN
    ALTER TABLE [Shipments] ADD [SlotStartUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005080318_AddShippingSlotsAndSellerConfirm'
)
BEGIN
    ALTER TABLE [ProductOrderStores] ADD [ShippingSlotDate] date NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005080318_AddShippingSlotsAndSellerConfirm'
)
BEGIN
    ALTER TABLE [ProductOrderStores] ADD [ShippingSlotId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005080318_AddShippingSlotsAndSellerConfirm'
)
BEGIN
    ALTER TABLE [CartStores] ADD [ShippingSlotDate] date NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005080318_AddShippingSlotsAndSellerConfirm'
)
BEGIN
    ALTER TABLE [CartStores] ADD [ShippingSlotId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005080318_AddShippingSlotsAndSellerConfirm'
)
BEGIN
    CREATE TABLE [ShippingSlots] (
        [Id] bigint NOT NULL IDENTITY,
        [DayOfWeek] int NOT NULL,
        [StartTime] time NOT NULL,
        [EndTime] time NOT NULL,
        [Capacity] int NOT NULL,
        [Active] bit NOT NULL,
        [Deleted] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_ShippingSlots] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005080318_AddShippingSlotsAndSellerConfirm'
)
BEGIN
    CREATE INDEX [IX_Shipments_Status_SellerConfirmDeadlineUtc] ON [Shipments] ([Status], [SellerConfirmDeadlineUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005080318_AddShippingSlotsAndSellerConfirm'
)
BEGIN
    CREATE INDEX [IX_ProductOrderStores_ShippingSlotId_ShippingSlotDate] ON [ProductOrderStores] ([ShippingSlotId], [ShippingSlotDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005080318_AddShippingSlotsAndSellerConfirm'
)
BEGIN
    CREATE INDEX [IX_ShippingSlots_DayOfWeek_StartTime_EndTime] ON [ShippingSlots] ([DayOfWeek], [StartTime], [EndTime]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005080318_AddShippingSlotsAndSellerConfirm'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261005080318_AddShippingSlotsAndSellerConfirm', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005080448_SeedShippingSlotsAndSellerPushTypes'
)
BEGIN
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 94 AND Label <> N'PushShipmentAwaitingSeller')
        THROW 51000, 'PushType ID 94 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushShipmentAwaitingSeller' AND Id <> 94)
        THROW 51000, 'PushShipmentAwaitingSeller is already assigned to another ID.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 95 AND Label <> N'PushShipmentSellerReminder')
        THROW 51000, 'PushType ID 95 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushShipmentSellerReminder' AND Id <> 95)
        THROW 51000, 'PushShipmentSellerReminder is already assigned to another ID.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 96 AND Label <> N'PushShipmentSellerExpiredUser')
        THROW 51000, 'PushType ID 96 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushShipmentSellerExpiredUser' AND Id <> 96)
        THROW 51000, 'PushShipmentSellerExpiredUser is already assigned to another ID.', 1;

    SET IDENTITY_INSERT PushTypes ON;

    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 94)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (94, N'سفارش ارسال با میاره منتظر تأیید فروشنده', N'PushShipmentAwaitingSeller');
    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 95)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (95, N'یادآوری تأیید آماده‌سازی به فروشنده', N'PushShipmentSellerReminder');
    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 96)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (96, N'فروشنده سفارش را در مهلت تأیید نکرد (به مشتری)', N'PushShipmentSellerExpiredUser');

    SET IDENTITY_INSERT PushTypes OFF;

    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 94)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (94, N'سفارش جدید؛ تأیید آماده‌سازی', N'سفارش {0} با بازه‌ی تحویل {2} ثبت شد. تا ساعت {1} آماده‌سازی را تأیید کنید.', N'/sellerProfile/orders', NULL, N'shipment-awaiting-seller', 1);
    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 95)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (95, N'مهلت تأیید رو به پایان است', N'یادآوری: سفارش {0} هنوز تأیید نشده است. تا ساعت {1} مهلت دارید، وگرنه سفارش لغو می‌شود.', N'/sellerProfile/orders', NULL, N'shipment-seller-reminder', 1);
    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 96)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (96, N'سفارش شما در انتظار پیگیری است', N'فروشنده سفارش {0} را در مهلت مقرر تأیید نکرد. پشتیبانی پاستیل لغو سفارش و بازگشت مبلغ را پیگیری می‌کند.', N'/orders', NULL, N'shipment-seller-expired', 1);

    INSERT INTO PushSettings (PushPatternId, IsEnabled)
    SELECT pattern.Id, 1
    FROM PushPatterns pattern
    WHERE pattern.PushTypeId IN (94, 95, 96)
      AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);

    -- بازه‌های پیش‌فرض تحویل (هر ۷ روز هفته، به وقت تهران): ۹–۱۳، ۱۳–۱۷، ۱۷–۲۱ با ظرفیت ۱۰.
    -- فقط وقتی جدول خالی است؛ بعداً از پنل ادمین (ارسال > بازه‌های تحویل) ویرایش می‌شود.
    IF NOT EXISTS (SELECT 1 FROM ShippingSlots)
        INSERT INTO ShippingSlots (DayOfWeek, StartTime, EndTime, Capacity, Active, Deleted, CreatedAtUtc)
        SELECT d.DayOfWeek, w.StartTime, w.EndTime, 10, 1, 0, SYSUTCDATETIME()
        FROM (VALUES (0), (1), (2), (3), (4), (5), (6)) AS d(DayOfWeek)
        CROSS JOIN (VALUES ('09:00', '13:00'), ('13:00', '17:00'), ('17:00', '21:00')) AS w(StartTime, EndTime);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005080448_SeedShippingSlotsAndSellerPushTypes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261005080448_SeedShippingSlotsAndSellerPushTypes', N'9.0.0');
END;

COMMIT;
GO

