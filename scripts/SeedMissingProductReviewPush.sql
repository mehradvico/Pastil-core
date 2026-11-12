BEGIN TRANSACTION;
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261112090000_SeedMissingProductReviewPush')
BEGIN
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 173 AND Label <> N'PushMissingProductApproved')
        THROW 51000, 'PushType ID 173 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 174 AND Label <> N'PushMissingProductRejected')
        THROW 51000, 'PushType ID 174 is already assigned to another label.', 1;

    SET IDENTITY_INSERT PushTypes ON;
    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 173)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (173, N'تأیید درخواست افزودن به کاتالوگ (به فروشنده)', N'PushMissingProductApproved');
    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 174)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (174, N'رد درخواست افزودن به کاتالوگ (به فروشنده)', N'PushMissingProductRejected');
    SET IDENTITY_INSERT PushTypes OFF;

    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 173)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (173, N'محصول شما به کاتالوگ اضافه شد', N'«{0}» در کاتالوگ پاستیل ثبت شد. قیمت و موجودی آن را در فروشگاهتان وارد کنید.', N'/sellerProfile/sellerProducts', NULL, N'missing-product-approved', 1);
    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 174)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (174, N'درخواست کاتالوگ رد شد', N'درخواست «{0}» رد شد: {1}', N'/sellerProfile/sellerProducts', NULL, N'missing-product-rejected', 1);

    INSERT INTO PushSettings (PushPatternId, IsEnabled)
    SELECT pattern.Id, 1
    FROM PushPatterns pattern
    WHERE pattern.PushTypeId IN (173, 174)
      AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);


    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20261112090000_SeedMissingProductReviewPush', N'9.0.0');
END;
COMMIT;
