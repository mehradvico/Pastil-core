BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010083500_AddExpressShipping'
)
BEGIN
    ALTER TABLE [Stores] ADD [ExpressShippingEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010083500_AddExpressShipping'
)
BEGIN
    ALTER TABLE [ShippingQuotes] ADD [ExpressSurcharge] float NOT NULL DEFAULT 0.0E0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010083500_AddExpressShipping'
)
BEGIN
    ALTER TABLE [ShippingQuotes] ADD [IsExpress] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010083500_AddExpressShipping'
)
BEGIN
    ALTER TABLE [Shipments] ADD [BoostAmount] float NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010083500_AddExpressShipping'
)
BEGIN
    ALTER TABLE [Shipments] ADD [BoostLevel] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010083500_AddExpressShipping'
)
BEGIN
    ALTER TABLE [Shipments] ADD [ExpressSurcharge] float NOT NULL DEFAULT 0.0E0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010083500_AddExpressShipping'
)
BEGIN
    ALTER TABLE [Shipments] ADD [IsExpress] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010083500_AddExpressShipping'
)
BEGIN
    ALTER TABLE [ProductOrderStores] ADD [ShippingIsExpress] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010083500_AddExpressShipping'
)
BEGIN
    ALTER TABLE [CartStores] ADD [ShippingIsExpress] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010083500_AddExpressShipping'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261010083500_AddExpressShipping', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010083557_SeedExpressShippingPush'
)
BEGIN
    DECLARE @Types TABLE (Id bigint, Name nvarchar(200), Label nvarchar(200), Title nvarchar(200), Body nvarchar(500), Url nvarchar(200), Tag nvarchar(200));
    INSERT INTO @Types (Id, Name, Label, Title, Body, Url, Tag) VALUES
        (121, N'ارسال اکسپرس: همین الان پیک بگیرید (به فروشگاه)', N'PushShipmentExpressAwaitingSeller', N'سفارش اکسپرس', N'سفارش اکسپرس {0} ثبت شد؛ همین الان کالا را آماده و «درخواست پیک» را بزنید (تا ساعت {1}).', N'/sellerProfile/orders', N'shipment-express-seller'),
        (122, N'ارسال اکسپرس: سفارش ثبت شد، ۲ تا ۳ ساعت کاری (به مشتری)', N'PushOrderExpressUser', N'سفارش اکسپرس شما ثبت شد', N'سفارش اکسپرس {0} ثبت شد؛ ظرف {1} تحویل می‌شود.', N'/orders/{0}', N'order-express-user');

    IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Id = s.Id WHERE t.Label <> s.Label)
        THROW 51000, 'An express shipping push type ID is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Label = s.Label WHERE t.Id <> s.Id)
        THROW 51000, 'An express shipping push label is already assigned to another ID.', 1;

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
    WHERE pattern.PushTypeId IN (121, 122)
      AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010083557_SeedExpressShippingPush'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261010083557_SeedExpressShippingPush', N'9.0.0');
END;

COMMIT;
GO

