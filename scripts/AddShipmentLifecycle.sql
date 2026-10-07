BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005083446_AddShipmentLifecycleColumns'
)
BEGIN
    ALTER TABLE [Shipments] ADD [CourierRetryCount] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005083446_AddShipmentLifecycleColumns'
)
BEGIN
    ALTER TABLE [Shipments] ADD [DisputeReportedAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005083446_AddShipmentLifecycleColumns'
)
BEGIN
    ALTER TABLE [Shipments] ADD [LateNotifiedAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005083446_AddShipmentLifecycleColumns'
)
BEGIN
    ALTER TABLE [Shipments] ADD [ShippedNotifiedAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005083446_AddShipmentLifecycleColumns'
)
BEGIN
    ALTER TABLE [ProductOrders] ADD [PostShippedNotifiedAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005083446_AddShipmentLifecycleColumns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261005083446_AddShipmentLifecycleColumns', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005083519_SeedShipmentLifecyclePushTypes'
)
BEGIN
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 97 AND Label <> N'PushShipmentCourierCanceled')
        THROW 51000, 'PushType ID 97 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushShipmentCourierCanceled' AND Id <> 97)
        THROW 51000, 'PushShipmentCourierCanceled is already assigned to another ID.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 98 AND Label <> N'PushShipmentReturning')
        THROW 51000, 'PushType ID 98 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushShipmentReturning' AND Id <> 98)
        THROW 51000, 'PushShipmentReturning is already assigned to another ID.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 99 AND Label <> N'PushShipmentDelayedUser')
        THROW 51000, 'PushType ID 99 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushShipmentDelayedUser' AND Id <> 99)
        THROW 51000, 'PushShipmentDelayedUser is already assigned to another ID.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 100 AND Label <> N'PushShipmentShippedUser')
        THROW 51000, 'PushType ID 100 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushShipmentShippedUser' AND Id <> 100)
        THROW 51000, 'PushShipmentShippedUser is already assigned to another ID.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 101 AND Label <> N'PushShipmentLateAdmin')
        THROW 51000, 'PushType ID 101 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushShipmentLateAdmin' AND Id <> 101)
        THROW 51000, 'PushShipmentLateAdmin is already assigned to another ID.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 102 AND Label <> N'PushShipmentNotReceivedAdmin')
        THROW 51000, 'PushType ID 102 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushShipmentNotReceivedAdmin' AND Id <> 102)
        THROW 51000, 'PushShipmentNotReceivedAdmin is already assigned to another ID.', 1;

    SET IDENTITY_INSERT PushTypes ON;

    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 97)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (97, N'پیک میاره لغو شد (به فروشنده)', N'PushShipmentCourierCanceled');
    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 98)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (98, N'کالا تحویل مشتری نشد و برمی‌گردد (به فروشنده)', N'PushShipmentReturning');
    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 99)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (99, N'تأخیر تحویل نسبت به بازه (به مشتری)', N'PushShipmentDelayedUser');
    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 100)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (100, N'سفارش میاره تحویل پیک شد؛ کد تحویل (به مشتری)', N'PushShipmentShippedUser');
    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 101)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (101, N'تحویل از بازه عقب افتاده (به ادمین)', N'PushShipmentLateAdmin');
    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 102)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (102, N'مشتری بعد از تحویل میاره تحویل نگرفتم زد (به ادمین)', N'PushShipmentNotReceivedAdmin');

    SET IDENTITY_INSERT PushTypes OFF;

    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 97)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (97, N'پیک میاره لغو شد', N'پیک میاره برای سفارش {0} لغو شد. تا ساعت {1} دوباره آماده‌سازی را تأیید کنید.', N'/sellerProfile/orders', NULL, N'shipment-courier-canceled', 1);
    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 98)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (98, N'سفارش تحویل داده نشد', N'سفارش {0} به مشتری تحویل داده نشد و پیک آن را برمی‌گرداند. با پشتیبانی هماهنگ کنید.', N'/sellerProfile/orders', NULL, N'shipment-returning', 1);
    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 99)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (99, N'تأخیر در تحویل سفارش', N'تحویل سفارش {0} کمی از بازه‌ی انتخابی شما عقب افتاده است. در حال پیگیری هستیم.', N'/orders', NULL, N'shipment-delayed', 1);
    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 100)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (100, N'فروشگاه پاستیل', N'کد تحویل شما: {0} ، برای سفارش {1}', N'/orders', NULL, N'shipment-shipped', 1);
    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 101)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (101, N'تأخیر در تحویل سفارش', N'تحویل سفارش {0} از بازه‌ی انتخابی مشتری عقب افتاده است.', N'/', NULL, N'shipment-late-admin', 1);
    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 102)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (102, N'مشتری تحویل نگرفته است', N'مشتری سفارش {0} می‌گوید کالا را تحویل نگرفته. تا ساعت {1} فرصت ثبت مشکل در میاره دارید.', N'/', NULL, N'shipment-not-received-admin', 1);

    INSERT INTO PushSettings (PushPatternId, IsEnabled)
    SELECT pattern.Id, 1
    FROM PushPatterns pattern
    WHERE pattern.PushTypeId IN (97, 98, 99, 100, 101, 102)
      AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005083519_SeedShipmentLifecyclePushTypes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261005083519_SeedShipmentLifecyclePushTypes', N'9.0.0');
END;

COMMIT;
GO

