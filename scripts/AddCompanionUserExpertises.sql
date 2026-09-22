BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921213400_AddCompanionUserExpertises'
)
BEGIN
    CREATE TABLE [CompanionUserExpertises] (
        [Id] bigint NOT NULL IDENTITY,
        [CompanionUserId] bigint NOT NULL,
        [ExpertiseId] bigint NOT NULL,
        CONSTRAINT [PK_CompanionUserExpertises] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CompanionUserExpertises_CompanionUsers_CompanionUserId] FOREIGN KEY ([CompanionUserId]) REFERENCES [CompanionUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CompanionUserExpertises_Expertises_ExpertiseId] FOREIGN KEY ([ExpertiseId]) REFERENCES [Expertises] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921213400_AddCompanionUserExpertises'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CompanionUserExpertises_CompanionUserId_ExpertiseId] ON [CompanionUserExpertises] ([CompanionUserId], [ExpertiseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921213400_AddCompanionUserExpertises'
)
BEGIN
    CREATE INDEX [IX_CompanionUserExpertises_ExpertiseId] ON [CompanionUserExpertises] ([ExpertiseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921213400_AddCompanionUserExpertises'
)
BEGIN

    INSERT INTO CompanionUserExpertises (CompanionUserId, ExpertiseId)
    SELECT cu.Id, cu.ExpertiseId
    FROM CompanionUsers cu
    WHERE cu.ExpertiseId IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM CompanionUserExpertises x WHERE x.CompanionUserId = cu.Id AND x.ExpertiseId = cu.ExpertiseId);

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921213400_AddCompanionUserExpertises'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260921213400_AddCompanionUserExpertises', N'9.0.0');
END;

COMMIT;
GO

