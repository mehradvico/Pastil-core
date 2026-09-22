BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922110326_AddConsultationPurchaseReview'
)
BEGIN
    ALTER TABLE [ConsultationPurchases] ADD [Rate] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922110326_AddConsultationPurchaseReview'
)
BEGIN
    ALTER TABLE [ConsultationPurchases] ADD [ReviewText] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922110326_AddConsultationPurchaseReview'
)
BEGIN
    ALTER TABLE [ConsultationPurchases] ADD [ReviewedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922110326_AddConsultationPurchaseReview'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260922110326_AddConsultationPurchaseReview', N'9.0.0');
END;

COMMIT;
GO

