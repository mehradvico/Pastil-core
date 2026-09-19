BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919084100_AddMissingProduct'
)
BEGIN
    CREATE TABLE [MissingProducts] (
        [Id] bigint NOT NULL IDENTITY,
        [StoreId] bigint NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [NormalizedName] nvarchar(250) NOT NULL,
        [Brand] nvarchar(100) NULL,
        [PackageSize] nvarchar(100) NULL,
        [Description] nvarchar(2000) NULL,
        [Price] bigint NULL,
        [Quantity] int NULL,
        [PictureId] bigint NULL,
        [Status] int NOT NULL,
        [RejectionReason] nvarchar(500) NULL,
        [Source] nvarchar(30) NULL,
        [CreateDate] datetime2 NOT NULL,
        [UpdateDate] datetime2 NOT NULL,
        [SubmittedDate] datetime2 NULL,
        [ReviewedDate] datetime2 NULL,
        [ReviewedByUserId] bigint NULL,
        [ProductId] bigint NULL,
        [ProductItemId] bigint NULL,
        CONSTRAINT [PK_MissingProducts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MissingProducts_Pictures_PictureId] FOREIGN KEY ([PictureId]) REFERENCES [Pictures] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MissingProducts_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MissingProducts_Stores_StoreId] FOREIGN KEY ([StoreId]) REFERENCES [Stores] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919084100_AddMissingProduct'
)
BEGIN
    CREATE INDEX [IX_MissingProducts_PictureId] ON [MissingProducts] ([PictureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919084100_AddMissingProduct'
)
BEGIN
    CREATE INDEX [IX_MissingProducts_ProductId] ON [MissingProducts] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919084100_AddMissingProduct'
)
BEGIN
    CREATE INDEX [IX_MissingProducts_Status_Id] ON [MissingProducts] ([Status], [Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919084100_AddMissingProduct'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MissingProducts_StoreId_NormalizedName] ON [MissingProducts] ([StoreId], [NormalizedName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919084100_AddMissingProduct'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260919084100_AddMissingProduct', N'9.0.0');
END;

COMMIT;
GO

