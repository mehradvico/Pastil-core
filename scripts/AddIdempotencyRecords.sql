BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261021090000_GroupExpertisesByProviderRole'
)
BEGIN
    ALTER TABLE [Expertises] ADD [ProviderRoleId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261021090000_GroupExpertisesByProviderRole'
)
BEGIN

    UPDATE e SET ProviderRoleId = r.Id
    FROM dbo.Expertises e
    CROSS JOIN dbo.ProviderRoles r
    WHERE r.Label = N'Clinic' AND r.Deleted = 0
      AND e.ProviderRoleId IS NULL
      AND e.Name IN (N'دامپزشک عمومی', N'جراح دندان پزشکی', N'جراح عقیم سازی', N'فیزیوتراپی');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261021090000_GroupExpertisesByProviderRole'
)
BEGIN
    CREATE INDEX [IX_Expertises_ProviderRoleId] ON [Expertises] ([ProviderRoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261021090000_GroupExpertisesByProviderRole'
)
BEGIN
    ALTER TABLE [Expertises] ADD CONSTRAINT [FK_Expertises_ProviderRoles_ProviderRoleId] FOREIGN KEY ([ProviderRoleId]) REFERENCES [ProviderRoles] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261021090000_GroupExpertisesByProviderRole'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261021090000_GroupExpertisesByProviderRole', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022060709_AddIdempotencyRecords'
)
BEGIN
    CREATE TABLE [IdempotencyRecords] (
        [Id] bigint NOT NULL IDENTITY,
        [Key] nvarchar(300) NOT NULL,
        [UserId] bigint NOT NULL,
        [Completed] bit NOT NULL,
        [ResponseJson] nvarchar(max) NULL,
        [CreateDate] datetime2 NOT NULL,
        [ExpireDate] datetime2 NOT NULL,
        CONSTRAINT [PK_IdempotencyRecords] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022060709_AddIdempotencyRecords'
)
BEGIN
    CREATE INDEX [IX_IdempotencyRecords_ExpireDate] ON [IdempotencyRecords] ([ExpireDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022060709_AddIdempotencyRecords'
)
BEGIN
    CREATE UNIQUE INDEX [IX_IdempotencyRecords_Key] ON [IdempotencyRecords] ([Key]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022060709_AddIdempotencyRecords'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261022060709_AddIdempotencyRecords', N'9.0.0');
END;

COMMIT;
GO

