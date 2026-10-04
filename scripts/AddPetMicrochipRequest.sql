BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004080236_AddPetMicrochipRequest'
)
BEGIN
    CREATE TABLE [PetMicrochipRequests] (
        [Id] bigint NOT NULL IDENTITY,
        [MicrochipCode] nvarchar(20) NULL,
        [PublicToken] nvarchar(40) NULL,
        [UserId] bigint NULL,
        [ClientIp] nvarchar(64) NULL,
        [FoundUserPetId] bigint NULL,
        [MatchCount] int NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [FollowUpRequested] bit NOT NULL,
        [FollowUpDate] datetime2 NULL,
        [FullName] nvarchar(100) NULL,
        [Mobile] nvarchar(20) NULL,
        [Email] nvarchar(200) NULL,
        [Message] nvarchar(1000) NULL,
        [Status] int NOT NULL,
        [AdminNote] nvarchar(1000) NULL,
        [HandledByUserId] bigint NULL,
        [ClosedDate] datetime2 NULL,
        CONSTRAINT [PK_PetMicrochipRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PetMicrochipRequests_UserPets_FoundUserPetId] FOREIGN KEY ([FoundUserPetId]) REFERENCES [UserPets] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PetMicrochipRequests_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004080236_AddPetMicrochipRequest'
)
BEGIN
    CREATE INDEX [IX_PetMicrochipRequests_FoundUserPetId] ON [PetMicrochipRequests] ([FoundUserPetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004080236_AddPetMicrochipRequest'
)
BEGIN
    CREATE INDEX [IX_PetMicrochipRequests_MicrochipCode] ON [PetMicrochipRequests] ([MicrochipCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004080236_AddPetMicrochipRequest'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PetMicrochipRequests_PublicToken] ON [PetMicrochipRequests] ([PublicToken]) WHERE [PublicToken] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004080236_AddPetMicrochipRequest'
)
BEGIN
    CREATE INDEX [IX_PetMicrochipRequests_Status_CreateDate] ON [PetMicrochipRequests] ([Status], [CreateDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004080236_AddPetMicrochipRequest'
)
BEGIN
    CREATE INDEX [IX_PetMicrochipRequests_UserId] ON [PetMicrochipRequests] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004080236_AddPetMicrochipRequest'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261004080236_AddPetMicrochipRequest', N'9.0.0');
END;

COMMIT;
GO

