BEGIN TRANSACTION;

DECLARE @PaymentTypeGroupId BIGINT =
(
    SELECT TOP (1) CodeGroupId
    FROM Codes
    WHERE Label = 'PaymentType_PansionReserve'
);

IF @PaymentTypeGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'PaymentType_PastilAI')
BEGIN
    DECLARE @NextPriority INT =
    (
        SELECT ISNULL(MAX(Priority), 0) + 1
        FROM Codes
        WHERE CodeGroupId = @PaymentTypeGroupId
    );

    INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
    VALUES ('PaymentType_PastilAI', 'pastil-ai', @PaymentTypeGroupId, @NextPriority, 1, N'پرداخت اشتراک پاستیل‌ای‌آی');
END


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260923083941_SeedPastilAiPaymentTypeCode', N'9.0.0');

COMMIT;
GO

