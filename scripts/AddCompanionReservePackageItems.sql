BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004105852_AddCompanionReservePackageItems'
)
BEGIN
    CREATE TABLE [CompanionReservePackageItems] (
        [Id] bigint NOT NULL IDENTITY,
        [CompanionReserveId] bigint NOT NULL,
        [CompanionAssistancePackageId] bigint NOT NULL,
        [PackageName] nvarchar(300) NULL,
        [PetCount] int NOT NULL,
        [Price] float NOT NULL,
        [PrePaymentPrice] float NOT NULL,
        [Status] int NOT NULL,
        [StatusReason] nvarchar(1000) NULL,
        [StatusChangedDate] datetime2 NULL,
        [StatusChangedByUserId] bigint NULL,
        [StatusChangedByAdmin] bit NOT NULL,
        [RefundAmount] float NOT NULL,
        [RefundDate] datetime2 NULL,
        [AddedAfterPayment] bit NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        CONSTRAINT [PK_CompanionReservePackageItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CompanionReservePackageItems_CompanionAssistancePackages_CompanionAssistancePackageId] FOREIGN KEY ([CompanionAssistancePackageId]) REFERENCES [CompanionAssistancePackages] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CompanionReservePackageItems_CompanionReserves_CompanionReserveId] FOREIGN KEY ([CompanionReserveId]) REFERENCES [CompanionReserves] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CompanionReservePackageItems_Users_StatusChangedByUserId] FOREIGN KEY ([StatusChangedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004105852_AddCompanionReservePackageItems'
)
BEGIN
    CREATE INDEX [IX_CompanionReservePackageItems_CompanionAssistancePackageId] ON [CompanionReservePackageItems] ([CompanionAssistancePackageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004105852_AddCompanionReservePackageItems'
)
BEGIN
    CREATE INDEX [IX_CompanionReservePackageItems_CompanionReserveId_CompanionAssistancePackageId] ON [CompanionReservePackageItems] ([CompanionReserveId], [CompanionAssistancePackageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004105852_AddCompanionReservePackageItems'
)
BEGIN
    CREATE INDEX [IX_CompanionReservePackageItems_StatusChangedByUserId] ON [CompanionReservePackageItems] ([StatusChangedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004105852_AddCompanionReservePackageItems'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261004105852_AddCompanionReservePackageItems', N'9.0.0');
END;

COMMIT;
GO

