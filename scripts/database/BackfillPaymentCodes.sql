SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Payments', N'U') IS NULL
        THROW 51001, N'جدول Payments پیدا نشد.', 1;

    IF COL_LENGTH(N'dbo.Payments', N'PaymentCode') IS NULL
        THROW 51002, N'ستون PaymentCode وجود ندارد؛ ابتدا ساختار ستون را ایجاد کنید.', 1;

    DECLARE @UpdatedPaymentCount bigint = 0;
    DECLARE @PaymentId bigint;
    DECLARE @PaymentCode nvarchar(40);

    DECLARE payment_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT Id
        FROM dbo.Payments
        WHERE PaymentCode IS NULL
        ORDER BY Id;

    OPEN payment_cursor;
    FETCH NEXT FROM payment_cursor INTO @PaymentId;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @PaymentCode = NULL;

        WHILE @PaymentCode IS NULL
        BEGIN
            DECLARE @Candidate nvarchar(40) =
                N'P' +
                CONVERT(nvarchar(1), ABS(CHECKSUM(NEWID()) % 10)) +
                LEFT(CONVERT(varchar(64), CRYPT_GEN_RANDOM(32), 2), 22);

            IF NOT EXISTS (SELECT 1 FROM dbo.Payments WHERE PaymentCode = @Candidate)
                SET @PaymentCode = @Candidate;
        END;

        UPDATE dbo.Payments
        SET PaymentCode = @PaymentCode
        WHERE Id = @PaymentId;

        SET @UpdatedPaymentCount += 1;
        FETCH NEXT FROM payment_cursor INTO @PaymentId;
    END;

    CLOSE payment_cursor;
    DEALLOCATE payment_cursor;

    IF EXISTS (SELECT 1 FROM dbo.Payments WHERE PaymentCode IS NULL)
        THROW 51003, N'برای بعضی از پرداخت‌ها PaymentCode ساخته نشد.', 1;

    COMMIT TRANSACTION;

    SELECT @UpdatedPaymentCount AS UpdatedPaymentCount;
END TRY
BEGIN CATCH
    IF CURSOR_STATUS('local', 'payment_cursor') >= 0
        CLOSE payment_cursor;
    IF CURSOR_STATUS('local', 'payment_cursor') > -3
        DEALLOCATE payment_cursor;
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
