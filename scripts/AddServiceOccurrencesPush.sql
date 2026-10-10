BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006121717_SeedServiceOccurrencesPush'
)
BEGIN
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 113 AND Label <> N'PushTripServiceOccurrencesAvailable')
        THROW 51000, 'PushType ID 113 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushTripServiceOccurrencesAvailable' AND Id <> 113)
        THROW 51000, 'PushTripServiceOccurrencesAvailable is already assigned to another ID.', 1;

    SET IDENTITY_INSERT PushTypes ON;

    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 113)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (113, N'نوبت‌های جدید سرویس هفتگی پت‌رسان (پوش تجمیعی به رانندگان)', N'PushTripServiceOccurrencesAvailable');

    SET IDENTITY_INSERT PushTypes OFF;

    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 113)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (113, N'سفر جدید سرویس هفتگی', N'{0} سفر جدید سرویس هفتگی برای فردا در دسترس است', N'/driverProfile/suggestedTrips', NULL, N'trip-service-occurrences', 1);

    INSERT INTO PushSettings (PushPatternId, IsEnabled)
    SELECT pattern.Id, 1
    FROM PushPatterns pattern
    WHERE pattern.PushTypeId = 113
      AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006121717_SeedServiceOccurrencesPush'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261006121717_SeedServiceOccurrencesPush', N'9.0.0');
END;

COMMIT;
GO

