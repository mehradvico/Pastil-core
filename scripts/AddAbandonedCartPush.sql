BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005071937_SeedAbandonedCartPushType'
)
BEGIN
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 93 AND Label <> N'PushAbandonedCart')
        THROW 51000, 'PushType ID 93 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushAbandonedCart' AND Id <> 93)
        THROW 51000, 'PushAbandonedCart is already assigned to another ID.', 1;

    SET IDENTITY_INSERT PushTypes ON;

    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 93)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (93, N'یادآوری سبد خرید رها شده', N'PushAbandonedCart');

    SET IDENTITY_INSERT PushTypes OFF;

    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 93)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (93, N'سبدت رو فراموش نکن!', N'{0} هنوز موجوده! قبل از اینکه تموم بشه سفارشت رو کامل کن.', N'/store/basket', NULL, N'abandoned-cart', 1);

    INSERT INTO PushSettings (PushPatternId, IsEnabled)
    SELECT pattern.Id, 1
    FROM PushPatterns pattern
    WHERE pattern.PushTypeId = 93
      AND NOT EXISTS
      (
          SELECT 1
          FROM PushSettings setting
          WHERE setting.PushPatternId = pattern.Id
      );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005071937_SeedAbandonedCartPushType'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261005071937_SeedAbandonedCartPushType', N'9.0.0');
END;

COMMIT;
GO

