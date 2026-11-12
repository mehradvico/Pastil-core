-- پت‌رسانِ همراه رزرو (۲۰۲۶-۱۱-۱۱): Trips.CoachReserveId + Trips.PaidWithReserve — معادل migration ReservePetResanBundle
-- چند بار اجرا شدن امن است. ترتیب: اول این اسکریپت، بعد دیپلوی Api و Payment با هم.
-- پیش‌نیاز: ستون Trips.GroomerReserveId (migration AddGroomerReserveToTrip) — اگر نبود همین‌جا ساخته می‌شود.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
BEGIN TRAN;

IF COL_LENGTH('Trips', 'GroomerReserveId') IS NULL
BEGIN
    ALTER TABLE [Trips] ADD [GroomerReserveId] bigint NULL;
    EXEC(N'CREATE INDEX [IX_Trips_GroomerReserveId] ON [Trips] ([GroomerReserveId]);');
    EXEC(N'ALTER TABLE [Trips] ADD CONSTRAINT [FK_Trips_GroomerReserves_GroomerReserveId] FOREIGN KEY ([GroomerReserveId]) REFERENCES [GroomerReserves] ([Id]);');
END;
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261110090000_AddGroomerReserveToTrip')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20261110090000_AddGroomerReserveToTrip', N'9.0.0');

IF COL_LENGTH('Trips', 'CoachReserveId') IS NULL
BEGIN
    ALTER TABLE [Trips] ADD [CoachReserveId] bigint NULL;
    EXEC(N'CREATE INDEX [IX_Trips_CoachReserveId] ON [Trips] ([CoachReserveId]);');
    EXEC(N'ALTER TABLE [Trips] ADD CONSTRAINT [FK_Trips_CoachReserves_CoachReserveId] FOREIGN KEY ([CoachReserveId]) REFERENCES [CoachReserves] ([Id]);');
END;
IF COL_LENGTH('Trips', 'PaidWithReserve') IS NULL
    ALTER TABLE [Trips] ADD [PaidWithReserve] bit NOT NULL CONSTRAINT [DF_Trips_PaidWithReserve] DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261111090000_ReservePetResanBundle')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20261111090000_ReservePetResanBundle', N'9.0.0');

COMMIT;
GO
-- بررسی:
-- SELECT COL_LENGTH('Trips','CoachReserveId') AS CoachReserveId, COL_LENGTH('Trips','PaidWithReserve') AS PaidWithReserve, COL_LENGTH('Trips','GroomerReserveId') AS GroomerReserveId;
