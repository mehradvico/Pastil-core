BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005100202_AddOrderReceiptAsked'
)
BEGIN
    ALTER TABLE [ProductOrders] ADD [ReceiptAskedAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005100202_AddOrderReceiptAsked'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261005100202_AddOrderReceiptAsked', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005100245_SeedOrderReceiptPush'
)
BEGIN
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 104 AND Label <> N'PushOrderAskReceived')
        THROW 51000, 'PushType ID 104 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushOrderAskReceived' AND Id <> 104)
        THROW 51000, 'PushOrderAskReceived is already assigned to another ID.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 105 AND Label <> N'PushOrderReceivedStore')
        THROW 51000, 'PushType ID 105 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushOrderReceivedStore' AND Id <> 105)
        THROW 51000, 'PushOrderReceivedStore is already assigned to another ID.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 106 AND Label <> N'PushOrderReceivedAdmin')
        THROW 51000, 'PushType ID 106 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushOrderReceivedAdmin' AND Id <> 106)
        THROW 51000, 'PushOrderReceivedAdmin is already assigned to another ID.', 1;

    SET IDENTITY_INSERT PushTypes ON;

    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 104)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (104, N'آیا سفارش را تحویل گرفتید؟ (به مشتری، راس پایان بازه)', N'PushOrderAskReceived');
    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 105)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (105, N'مشتری سفارش را تحویل گرفت (به فروشنده)', N'PushOrderReceivedStore');
    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 106)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (106, N'مشتری سفارش را تحویل گرفت (به ادمین)', N'PushOrderReceivedAdmin');

    SET IDENTITY_INSERT PushTypes OFF;

    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 104)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (104, N'فروشگاه پاستیل', N'آیا سفارش {0} را تحویل گرفتید؟', N'/orders/{0}', NULL, N'order-ask-received', 1);
    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 105)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (105, N'سفارش تحویل داده شد', N'سفارش {0} با موفقیت به کاربر تحویل داده شد.', N'/sellerProfile/orders', NULL, N'order-received-store', 1);
    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 106)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (106, N'سفارش تحویل داده شد', N'سفارش {0} با موفقیت به کاربر تحویل داده شد.', N'/', NULL, N'order-received-admin', 1);

    INSERT INTO PushSettings (PushPatternId, IsEnabled)
    SELECT pattern.Id, 1
    FROM PushPatterns pattern
    WHERE pattern.PushTypeId IN (104, 105, 106)
      AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);

    -- متن پوش ادمین «مشتری تحویل نگرفتم زد» عوض شد
    UPDATE PushPatterns
    SET Title = N'سفارش به کاربر نرسیده است', Body = N'سفارش {0} در ساعت مقرر به کاربر نرسیده است.'
    WHERE PushTypeId = 102;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005100245_SeedOrderReceiptPush'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261005100245_SeedOrderReceiptPush', N'9.0.0');
END;

COMMIT;
GO

