-- نقش/تخصص اعضای تیم کلینیک/مربی/آرایشگاه (VenueStaffExpertise، ۲۰۲۶-۱۰-۲۸). تکرارپذیر: فقط موارد ناموجود ساخته می‌شوند.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH('GroomerUsers','ExpertiseId') IS NULL
    EXEC(N'ALTER TABLE [GroomerUsers] ADD [ExpertiseId] bigint NULL');

IF COL_LENGTH('CoachUsers','ExpertiseId') IS NULL
    EXEC(N'ALTER TABLE [CoachUsers] ADD [ExpertiseId] bigint NULL');

IF COL_LENGTH('ClinicUsers','ExpertiseId') IS NULL
    EXEC(N'ALTER TABLE [ClinicUsers] ADD [ExpertiseId] bigint NULL');

IF OBJECT_ID(N'[ClinicUserExpertises]', N'U') IS NULL
    EXEC(N'CREATE TABLE [ClinicUserExpertises] (
    [Id] bigint NOT NULL IDENTITY,
    [ClinicUserId] bigint NOT NULL,
    [ExpertiseId] bigint NOT NULL,
    CONSTRAINT [PK_ClinicUserExpertises] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ClinicUserExpertises_ClinicUsers_ClinicUserId] FOREIGN KEY ([ClinicUserId]) REFERENCES [ClinicUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ClinicUserExpertises_Expertises_ExpertiseId] FOREIGN KEY ([ExpertiseId]) REFERENCES [Expertises] ([Id]) ON DELETE NO ACTION
)');

IF OBJECT_ID(N'[CoachUserExpertises]', N'U') IS NULL
    EXEC(N'CREATE TABLE [CoachUserExpertises] (
    [Id] bigint NOT NULL IDENTITY,
    [CoachUserId] bigint NOT NULL,
    [ExpertiseId] bigint NOT NULL,
    CONSTRAINT [PK_CoachUserExpertises] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CoachUserExpertises_CoachUsers_CoachUserId] FOREIGN KEY ([CoachUserId]) REFERENCES [CoachUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CoachUserExpertises_Expertises_ExpertiseId] FOREIGN KEY ([ExpertiseId]) REFERENCES [Expertises] ([Id]) ON DELETE NO ACTION
)');

IF OBJECT_ID(N'[GroomerUserExpertises]', N'U') IS NULL
    EXEC(N'CREATE TABLE [GroomerUserExpertises] (
    [Id] bigint NOT NULL IDENTITY,
    [GroomerUserId] bigint NOT NULL,
    [ExpertiseId] bigint NOT NULL,
    CONSTRAINT [PK_GroomerUserExpertises] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GroomerUserExpertises_Expertises_ExpertiseId] FOREIGN KEY ([ExpertiseId]) REFERENCES [Expertises] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_GroomerUserExpertises_GroomerUsers_GroomerUserId] FOREIGN KEY ([GroomerUserId]) REFERENCES [GroomerUsers] ([Id]) ON DELETE NO ACTION
)');

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_GroomerUsers_ExpertiseId' AND object_id = OBJECT_ID(N'[GroomerUsers]'))
    EXEC(N'CREATE INDEX [IX_GroomerUsers_ExpertiseId] ON [GroomerUsers] ([ExpertiseId])');

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CoachUsers_ExpertiseId' AND object_id = OBJECT_ID(N'[CoachUsers]'))
    EXEC(N'CREATE INDEX [IX_CoachUsers_ExpertiseId] ON [CoachUsers] ([ExpertiseId])');

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ClinicUsers_ExpertiseId' AND object_id = OBJECT_ID(N'[ClinicUsers]'))
    EXEC(N'CREATE INDEX [IX_ClinicUsers_ExpertiseId] ON [ClinicUsers] ([ExpertiseId])');

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ClinicUserExpertises_ClinicUserId_ExpertiseId' AND object_id = OBJECT_ID(N'[ClinicUserExpertises]'))
    EXEC(N'CREATE UNIQUE INDEX [IX_ClinicUserExpertises_ClinicUserId_ExpertiseId] ON [ClinicUserExpertises] ([ClinicUserId], [ExpertiseId])');

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ClinicUserExpertises_ExpertiseId' AND object_id = OBJECT_ID(N'[ClinicUserExpertises]'))
    EXEC(N'CREATE INDEX [IX_ClinicUserExpertises_ExpertiseId] ON [ClinicUserExpertises] ([ExpertiseId])');

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CoachUserExpertises_CoachUserId_ExpertiseId' AND object_id = OBJECT_ID(N'[CoachUserExpertises]'))
    EXEC(N'CREATE UNIQUE INDEX [IX_CoachUserExpertises_CoachUserId_ExpertiseId] ON [CoachUserExpertises] ([CoachUserId], [ExpertiseId])');

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CoachUserExpertises_ExpertiseId' AND object_id = OBJECT_ID(N'[CoachUserExpertises]'))
    EXEC(N'CREATE INDEX [IX_CoachUserExpertises_ExpertiseId] ON [CoachUserExpertises] ([ExpertiseId])');

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_GroomerUserExpertises_ExpertiseId' AND object_id = OBJECT_ID(N'[GroomerUserExpertises]'))
    EXEC(N'CREATE INDEX [IX_GroomerUserExpertises_ExpertiseId] ON [GroomerUserExpertises] ([ExpertiseId])');

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_GroomerUserExpertises_GroomerUserId_ExpertiseId' AND object_id = OBJECT_ID(N'[GroomerUserExpertises]'))
    EXEC(N'CREATE UNIQUE INDEX [IX_GroomerUserExpertises_GroomerUserId_ExpertiseId] ON [GroomerUserExpertises] ([GroomerUserId], [ExpertiseId])');

IF OBJECT_ID(N'[FK_ClinicUsers_Expertises_ExpertiseId]') IS NULL
    EXEC(N'ALTER TABLE [ClinicUsers] ADD CONSTRAINT [FK_ClinicUsers_Expertises_ExpertiseId] FOREIGN KEY ([ExpertiseId]) REFERENCES [Expertises] ([Id]) ON DELETE NO ACTION');

IF OBJECT_ID(N'[FK_CoachUsers_Expertises_ExpertiseId]') IS NULL
    EXEC(N'ALTER TABLE [CoachUsers] ADD CONSTRAINT [FK_CoachUsers_Expertises_ExpertiseId] FOREIGN KEY ([ExpertiseId]) REFERENCES [Expertises] ([Id]) ON DELETE NO ACTION');

IF OBJECT_ID(N'[FK_GroomerUsers_Expertises_ExpertiseId]') IS NULL
    EXEC(N'ALTER TABLE [GroomerUsers] ADD CONSTRAINT [FK_GroomerUsers_Expertises_ExpertiseId] FOREIGN KEY ([ExpertiseId]) REFERENCES [Expertises] ([Id]) ON DELETE NO ACTION');

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261028064413_VenueStaffExpertise')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261028064413_VenueStaffExpertise', N'9.0.0');

COMMIT;
GO
