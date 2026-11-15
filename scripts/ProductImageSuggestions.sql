-- تصویر پیشنهادیِ فروشگاه‌ها برای محصولات کاتالوگ: جدول ProductImageSuggestions + ردیف اعلان ادمین (زنگوله‌ی پنل).
-- معادل migration «20261115120000_ProductImageSuggestions». چندبار اجرا شدن بی‌خطر است.
-- بدون جدول، Api (نسخه‌ی جدید) روی endpointهای ProductImageSuggestion خطا می‌دهد؛ پیش از دیپلوی Api اجرا شود.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261115120000_ProductImageSuggestions')
BEGIN
    IF OBJECT_ID(N'ProductImageSuggestions', N'U') IS NULL
    BEGIN
        CREATE TABLE [ProductImageSuggestions] (
            [Id] bigint NOT NULL IDENTITY,
            [ProductId] bigint NOT NULL,
            [StoreId] bigint NOT NULL,
            [PictureId] bigint NOT NULL,
            [Status] int NOT NULL,
            [RejectionReason] nvarchar(500) NULL,
            [CreateDate] datetime2 NOT NULL,
            [ReviewedDate] datetime2 NULL,
            [ReviewedByUserId] bigint NULL,
            [ReplacedPictureId] bigint NULL,
            CONSTRAINT [PK_ProductImageSuggestions] PRIMARY KEY ([Id]),
            CONSTRAINT [FK_ProductImageSuggestions_Pictures_PictureId] FOREIGN KEY ([PictureId]) REFERENCES [Pictures] ([Id]),
            CONSTRAINT [FK_ProductImageSuggestions_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]),
            CONSTRAINT [FK_ProductImageSuggestions_Stores_StoreId] FOREIGN KEY ([StoreId]) REFERENCES [Stores] ([Id])
        );
        CREATE INDEX [IX_ProductImageSuggestions_PictureId] ON [ProductImageSuggestions] ([PictureId]);
        CREATE INDEX [IX_ProductImageSuggestions_ProductId] ON [ProductImageSuggestions] ([ProductId]);
        CREATE INDEX [IX_ProductImageSuggestions_Status_Id] ON [ProductImageSuggestions] ([Status], [Id]);
        CREATE INDEX [IX_ProductImageSuggestions_StoreId_ProductId_Status] ON [ProductImageSuggestions] ([StoreId], [ProductId], [Status]);
    END;

    IF NOT EXISTS (SELECT 1 FROM NoticeTypes WHERE Label = N'ProductImageSuggestion.Submitted')
        INSERT INTO NoticeTypes (Label, Title, Name, NavigationTemplate, Importance, IsActive)
        VALUES (N'ProductImageSuggestion.Submitted', N'تصویر پیشنهادی جدید برای محصول',
                N'فروشگاه «{storeName}» برای محصول «{productName}» تصویر پیشنهاد داد.',
                N'/admin/productimagesuggestion', 2, 1);

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20261115120000_ProductImageSuggestions', N'9.0.0');
END;

COMMIT TRANSACTION;

SELECT Label, Title, Importance, IsActive FROM NoticeTypes WHERE Label = N'ProductImageSuggestion.Submitted';
