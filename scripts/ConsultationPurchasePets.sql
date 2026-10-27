BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027072529_ConsultationPurchasePets'
)
BEGIN
    CREATE TABLE [ConsultationPurchasePets] (
        [Id] bigint NOT NULL IDENTITY,
        [ConsultationPurchaseId] bigint NOT NULL,
        [UserPetId] bigint NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        CONSTRAINT [PK_ConsultationPurchasePets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ConsultationPurchasePets_ConsultationPurchases_ConsultationPurchaseId] FOREIGN KEY ([ConsultationPurchaseId]) REFERENCES [ConsultationPurchases] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ConsultationPurchasePets_UserPets_UserPetId] FOREIGN KEY ([UserPetId]) REFERENCES [UserPets] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027072529_ConsultationPurchasePets'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ConsultationPurchasePets_ConsultationPurchaseId_UserPetId] ON [ConsultationPurchasePets] ([ConsultationPurchaseId], [UserPetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027072529_ConsultationPurchasePets'
)
BEGIN
    CREATE INDEX [IX_ConsultationPurchasePets_UserPetId] ON [ConsultationPurchasePets] ([UserPetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027072529_ConsultationPurchasePets'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261027072529_ConsultationPurchasePets', N'9.0.0');
END;

COMMIT;
GO

