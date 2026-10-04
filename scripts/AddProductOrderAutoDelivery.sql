BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004063923_AddProductOrderUserDelivery'
)
BEGIN
    ALTER TABLE [ProductOrders] ADD [UserReceiveNote] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004063923_AddProductOrderUserDelivery'
)
BEGIN
    ALTER TABLE [ProductOrders] ADD [UserReceived] bit NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004063923_AddProductOrderUserDelivery'
)
BEGIN
    ALTER TABLE [ProductOrders] ADD [UserReceivedDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004063923_AddProductOrderUserDelivery'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261004063923_AddProductOrderUserDelivery', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004065555_AddProductOrderAutoDelivery'
)
BEGIN
    ALTER TABLE [ProductOrders] ADD [SentDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004065555_AddProductOrderAutoDelivery'
)
BEGIN
    ALTER TABLE [ProductOrders] ADD [UserReceivedAuto] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004065555_AddProductOrderAutoDelivery'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261004065555_AddProductOrderAutoDelivery', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004065640_SeedProductOrderAutoDeliveryPushTypes'
)
BEGIN
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id IN (88, 89) AND Label NOT IN (N'PushProductOrderAutoDeliveryWarning', N'PushProductOrderAutoDelivered'))
        THROW 51000, 'PushType IDs 88-89 are already assigned to other labels.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Label IN (N'PushProductOrderAutoDeliveryWarning', N'PushProductOrderAutoDelivered') AND Id NOT IN (88, 89))
        THROW 51000, 'Product order auto-delivery push labels are already assigned to other IDs.', 1;

    SET IDENTITY_INSERT PushTypes ON;

    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 88)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (88, N'هشدار تأیید خودکار تحویل سفارش (۲ روز قبل)', N'PushProductOrderAutoDeliveryWarning');
    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 89)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (89, N'تأیید خودکار تحویل سفارش بعد از ۷ روز', N'PushProductOrderAutoDelivered');

    SET IDENTITY_INSERT PushTypes OFF;

    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 88)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (88, N'تأیید تحویل سفارش', N'سفارش {2} را تحویل گرفته‌اید؟ اگر تا {3} در «سفارش‌های من» پاسخ ندهید، به‌صورت خودکار «تحویل گرفته شد» ثبت می‌شود.', N'/orders', NULL, N'product-order-auto-delivery-warning', 1);
    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 89)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (89, N'سفارش شما تحویل‌شده ثبت شد', N'۷ روز از ارسال سفارش {2} گذشت و پاسخی ثبت نشد؛ سفارش به‌صورت خودکار «تحویل داده شد» ثبت شد. اگر بسته را دریافت نکرده‌اید، با پشتیبانی تماس بگیرید.', N'/orders', NULL, N'product-order-auto-delivered', 1);

    INSERT INTO PushSettings (PushPatternId, IsEnabled)
    SELECT pattern.Id, 1
    FROM PushPatterns pattern
    WHERE pattern.PushTypeId IN (88, 89)
      AND NOT EXISTS
      (
          SELECT 1
          FROM PushSettings setting
          WHERE setting.PushPatternId = pattern.Id
      );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004065640_SeedProductOrderAutoDeliveryPushTypes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261004065640_SeedProductOrderAutoDeliveryPushTypes', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004070259_SeedProductOrderNotReceivedStorePushType'
)
BEGIN
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 90 AND Label <> N'PushProductOrderNotReceivedStore')
        THROW 51000, 'PushType ID 90 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushProductOrderNotReceivedStore' AND Id <> 90)
        THROW 51000, 'PushProductOrderNotReceivedStore is already assigned to another ID.', 1;

    SET IDENTITY_INSERT PushTypes ON;

    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 90)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (90, N'مشتری سفارش را تحویل نگرفته (اعلان به فروشگاه)', N'PushProductOrderNotReceivedStore');

    SET IDENTITY_INSERT PushTypes OFF;

    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 90)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (90, N'مشتری سفارش را تحویل نگرفته', N'مشتری ({0}) سفارش {2} را تحویل نگرفته است. لطفاً با مشتری تماس بگیرید و پیگیری کنید.', N'/sellerProfile/orders', NULL, N'product-order-not-received-store', 1);

    INSERT INTO PushSettings (PushPatternId, IsEnabled)
    SELECT pattern.Id, 1
    FROM PushPatterns pattern
    WHERE pattern.PushTypeId = 90
      AND NOT EXISTS
      (
          SELECT 1
          FROM PushSettings setting
          WHERE setting.PushPatternId = pattern.Id
      );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004070259_SeedProductOrderNotReceivedStorePushType'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261004070259_SeedProductOrderNotReceivedStorePushType', N'9.0.0');
END;

COMMIT;
GO

