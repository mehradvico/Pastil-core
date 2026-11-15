-- پشتِ گواهینامه و پشتِ کارت خودرو راننده (Drivers.CertificateBackPictureId / VehicleCardBackPictureId، هر دو nullable).
-- چندبار اجرا شدن بی‌خطر است. بدون ستون‌ها، Api (نسخه‌ی جدید) روی جدول Drivers خطا می‌دهد؛ پیش از دیپلوی Api اجرا شود.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261115090000_DriverBackPictures')
BEGIN
    IF COL_LENGTH(N'Drivers', N'CertificateBackPictureId') IS NULL
        ALTER TABLE [Drivers] ADD [CertificateBackPictureId] bigint NULL;
    IF COL_LENGTH(N'Drivers', N'VehicleCardBackPictureId') IS NULL
        ALTER TABLE [Drivers] ADD [VehicleCardBackPictureId] bigint NULL;
END;
GO
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261115090000_DriverBackPictures')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Drivers_CertificateBackPictureId' AND object_id = OBJECT_ID(N'Drivers'))
        CREATE INDEX [IX_Drivers_CertificateBackPictureId] ON [Drivers] ([CertificateBackPictureId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Drivers_VehicleCardBackPictureId' AND object_id = OBJECT_ID(N'Drivers'))
        CREATE INDEX [IX_Drivers_VehicleCardBackPictureId] ON [Drivers] ([VehicleCardBackPictureId]);
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Drivers_Pictures_CertificateBackPictureId')
        ALTER TABLE [Drivers] ADD CONSTRAINT [FK_Drivers_Pictures_CertificateBackPictureId] FOREIGN KEY ([CertificateBackPictureId]) REFERENCES [Pictures] ([Id]);
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Drivers_Pictures_VehicleCardBackPictureId')
        ALTER TABLE [Drivers] ADD CONSTRAINT [FK_Drivers_Pictures_VehicleCardBackPictureId] FOREIGN KEY ([VehicleCardBackPictureId]) REFERENCES [Pictures] ([Id]);
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20261115090000_DriverBackPictures', N'9.0.0');
END;
COMMIT TRANSACTION;
