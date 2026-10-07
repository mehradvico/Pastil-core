BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004105942_SeedCompanionReservePackagePushTypes'
)
BEGIN
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 91 AND Label <> N'PushCompanionReservePackageApproved')
        THROW 51000, 'PushType ID 91 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 92 AND Label <> N'PushCompanionReservePackageCancelled')
        THROW 51000, 'PushType ID 92 is already assigned to another label.', 1;

    SET IDENTITY_INSERT PushTypes ON;
    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 91)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (91, N'تأیید پکیج رزرو توسط نماینده (به کاربر)', N'PushCompanionReservePackageApproved');
    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 92)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (92, N'لغو پکیج رزرو و بازگشت مبلغ به کیف پول (به کاربر)', N'PushCompanionReservePackageCancelled');
    SET IDENTITY_INSERT PushTypes OFF;

    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 91)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (91, N'پکیج رزرو تأیید شد', N'نماینده پکیج «{0}» رزرو شما را تأیید کرد.', N'/reserve/{2}', NULL, N'companion-reserve-package-approved', 1);
    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 92)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (92, N'پکیج رزرو لغو شد', N'پکیج «{0}» رزرو شما لغو شد و {3} تومان به کیف پول شما برگشت داده شد.', N'/reserve/{2}', NULL, N'companion-reserve-package-cancelled', 1);

    INSERT INTO PushSettings (PushPatternId, IsEnabled)
    SELECT pattern.Id, 1 FROM PushPatterns pattern
    WHERE pattern.PushTypeId IN (91, 92)
      AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004105942_SeedCompanionReservePackagePushTypes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261004105942_SeedCompanionReservePackagePushTypes', N'9.0.0');
END;

COMMIT;
GO

