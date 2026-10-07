BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006070433_SeedTripOngoingPush'
)
BEGIN
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 111 AND Label <> N'PushTripOngoing')
        THROW 51000, 'PushType ID 111 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushTripOngoing' AND Id <> 111)
        THROW 51000, 'PushTripOngoing is already assigned to another ID.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 112 AND Label <> N'PushTripOngoingEnd')
        THROW 51000, 'PushType ID 112 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushTripOngoingEnd' AND Id <> 112)
        THROW 51000, 'PushTripOngoingEnd is already assigned to another ID.', 1;

    SET IDENTITY_INSERT PushTypes ON;

    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 111)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (111, N'سفر پت‌رسان در جریان است - اعلان ماندگار (به مسافر)', N'PushTripOngoing');
    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 112)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (112, N'پایان سفر پت‌رسان - بستن اعلان ماندگار (به مسافر)', N'PushTripOngoingEnd');

    SET IDENTITY_INSERT PushTypes OFF;

    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 111)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (111, N'سفر پت‌رسان در جریان است', N'{0} در مسیر مقصد است. زمان تقریبی رسیدن به مقصد {1}', N'/trip', NULL, N'trip-ongoing', 1);
    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 112)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (112, N'سفر پت‌رسان', N'سفر {0} به پایان رسید.', N'/trip', NULL, N'trip-ongoing', 1);

    INSERT INTO PushSettings (PushPatternId, IsEnabled)
    SELECT pattern.Id, 1
    FROM PushPatterns pattern
    WHERE pattern.PushTypeId IN (111, 112)
      AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006070433_SeedTripOngoingPush'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261006070433_SeedTripOngoingPush', N'9.0.0');
END;

COMMIT;
GO

