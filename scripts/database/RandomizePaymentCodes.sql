SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @Codes TABLE
(
    PaymentId bigint NOT NULL PRIMARY KEY,
    PaymentCode nvarchar(40) NOT NULL UNIQUE
);

DECLARE @PaymentId bigint;
DECLARE @PaymentCode nvarchar(40);

DECLARE payment_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT Id
    FROM dbo.Payments
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

        IF NOT EXISTS (SELECT 1 FROM @Codes WHERE PaymentCode = @Candidate)
           AND NOT EXISTS (SELECT 1 FROM dbo.Payments WHERE PaymentCode = @Candidate)
        BEGIN
            SET @PaymentCode = @Candidate;
        END;
    END;

    INSERT INTO @Codes (PaymentId, PaymentCode)
    VALUES (@PaymentId, @PaymentCode);

    FETCH NEXT FROM payment_cursor INTO @PaymentId;
END;

CLOSE payment_cursor;
DEALLOCATE payment_cursor;

UPDATE payment
SET payment.PaymentCode = codes.PaymentCode
FROM dbo.Payments AS payment
INNER JOIN @Codes AS codes ON codes.PaymentId = payment.Id;

COMMIT TRANSACTION;

SELECT Id, PaymentCode, CreateDate
FROM dbo.Payments
ORDER BY Id DESC;
GO
