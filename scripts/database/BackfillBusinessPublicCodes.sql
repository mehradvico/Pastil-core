SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF COL_LENGTH(N'dbo.CompanionReserves', N'ReserveCode') IS NULL
        THROW 51001, N'ستون CompanionReserves.ReserveCode وجود ندارد؛ ابتدا migration ساختاری را اجرا کنید.', 1;
    IF COL_LENGTH(N'dbo.PansionReserves', N'ReserveCode') IS NULL
        THROW 51002, N'ستون PansionReserves.ReserveCode وجود ندارد؛ ابتدا migration ساختاری را اجرا کنید.', 1;
    IF COL_LENGTH(N'dbo.ProductOrders', N'OrderCode') IS NULL
        THROW 51003, N'ستون ProductOrders.OrderCode وجود ندارد؛ ابتدا migration ساختاری را اجرا کنید.', 1;
    IF OBJECT_ID(N'dbo.BusinessCodeSequence', N'SO') IS NULL
        THROW 51004, N'Sequence مربوط به کدهای عمومی وجود ندارد؛ ابتدا migration ساختاری را اجرا کنید.', 1;

    DECLARE @CurrentSequenceValue bigint = ISNULL(
        (SELECT CONVERT(bigint, current_value)
         FROM sys.sequences
         WHERE object_id = OBJECT_ID(N'dbo.BusinessCodeSequence')),
        0);

    CREATE TABLE #BusinessCodes
    (
        EntityType char(3) NOT NULL,
        NumericId bigint NULL,
        StringId nvarchar(450) NULL,
        CreatedAt datetime2 NOT NULL,
        SequenceNumber bigint NOT NULL,
        PRIMARY KEY (EntityType, SequenceNumber)
    );

    ;WITH MissingCodes AS
    (
        SELECT N'RSV' AS EntityType, Id AS NumericId, CONVERT(nvarchar(450), NULL) AS StringId, CreateDate AS CreatedAt
        FROM dbo.CompanionReserves WHERE ReserveCode IS NULL
        UNION ALL
        SELECT N'PAN', Id, NULL, CreateDate
        FROM dbo.PansionReserves WHERE ReserveCode IS NULL
        UNION ALL
        SELECT N'ORD', NULL, Id, CreateDate
        FROM dbo.ProductOrders WHERE OrderCode IS NULL
    )
    INSERT INTO #BusinessCodes (EntityType, NumericId, StringId, CreatedAt, SequenceNumber)
    SELECT EntityType, NumericId, StringId, CreatedAt,
           @CurrentSequenceValue + ROW_NUMBER() OVER (
               ORDER BY CreatedAt, EntityType, ISNULL(CONVERT(nvarchar(450), NumericId), StringId))
    FROM MissingCodes;

    UPDATE reserve
    SET ReserveCode = code.EntityType + N'-' + dateParts.JalaliDate + N'-' + dateParts.TimePart + N'-' + parts.Suffix
    FROM dbo.CompanionReserves AS reserve
    INNER JOIN #BusinessCodes AS code ON code.EntityType = N'RSV' AND code.NumericId = reserve.Id
    CROSS APPLY (SELECT
        TRANSLATE(TRANSLATE(FORMAT(code.CreatedAt, N'yyyyMMdd', N'fa-IR'), N'۰۱۲۳۴۵۶۷۸۹', N'0123456789'), N'٠١٢٣٤٥٦٧٨٩', N'0123456789') AS JalaliDate,
        REPLACE(CONVERT(char(5), code.CreatedAt, 108), N':', N'') AS TimePart) AS dateParts
    CROSS APPLY (SELECT CONVERT(nvarchar(4), 1000 + (
        (((code.SequenceNumber - 1) % 9000) * 7919)
        + (((CONVERT(bigint, dateParts.JalaliDate + dateParts.TimePart) * 3571) + 1877) % 9000)
    ) % 9000) AS Suffix) AS parts;

    UPDATE reserve
    SET ReserveCode = code.EntityType + N'-' + dateParts.JalaliDate + N'-' + dateParts.TimePart + N'-' + parts.Suffix
    FROM dbo.PansionReserves AS reserve
    INNER JOIN #BusinessCodes AS code ON code.EntityType = N'PAN' AND code.NumericId = reserve.Id
    CROSS APPLY (SELECT
        TRANSLATE(TRANSLATE(FORMAT(code.CreatedAt, N'yyyyMMdd', N'fa-IR'), N'۰۱۲۳۴۵۶۷۸۹', N'0123456789'), N'٠١٢٣٤٥٦٧٨٩', N'0123456789') AS JalaliDate,
        REPLACE(CONVERT(char(5), code.CreatedAt, 108), N':', N'') AS TimePart) AS dateParts
    CROSS APPLY (SELECT CONVERT(nvarchar(4), 1000 + (
        (((code.SequenceNumber - 1) % 9000) * 7919)
        + (((CONVERT(bigint, dateParts.JalaliDate + dateParts.TimePart) * 3571) + 1877) % 9000)
    ) % 9000) AS Suffix) AS parts;

    UPDATE productOrder
    SET OrderCode = code.EntityType + N'-' + dateParts.JalaliDate + N'-' + dateParts.TimePart + N'-' + parts.Suffix
    FROM dbo.ProductOrders AS productOrder
    INNER JOIN #BusinessCodes AS code ON code.EntityType = N'ORD' AND code.StringId = productOrder.Id
    CROSS APPLY (SELECT
        TRANSLATE(TRANSLATE(FORMAT(code.CreatedAt, N'yyyyMMdd', N'fa-IR'), N'۰۱۲۳۴۵۶۷۸۹', N'0123456789'), N'٠١٢٣٤٥٦٧٨٩', N'0123456789') AS JalaliDate,
        REPLACE(CONVERT(char(5), code.CreatedAt, 108), N':', N'') AS TimePart) AS dateParts
    CROSS APPLY (SELECT CONVERT(nvarchar(4), 1000 + (
        (((code.SequenceNumber - 1) % 9000) * 7919)
        + (((CONVERT(bigint, dateParts.JalaliDate + dateParts.TimePart) * 3571) + 1877) % 9000)
    ) % 9000) AS Suffix) AS parts;

    DECLARE @UpdatedCount bigint = (SELECT COUNT_BIG(1) FROM #BusinessCodes);
    DECLARE @NextSequenceValue bigint = @CurrentSequenceValue + @UpdatedCount + 1;
    DECLARE @RestartSql nvarchar(200) = N'ALTER SEQUENCE dbo.BusinessCodeSequence RESTART WITH '
        + CONVERT(nvarchar(20), @NextSequenceValue) + N';';
    EXEC sys.sp_executesql @RestartSql;

    IF EXISTS (SELECT 1 FROM dbo.CompanionReserves WHERE ReserveCode IS NULL)
        THROW 51005, N'برای بعضی رزروهای همراه کد ساخته نشد.', 1;
    IF EXISTS (SELECT 1 FROM dbo.PansionReserves WHERE ReserveCode IS NULL)
        THROW 51006, N'برای بعضی رزروهای پانسیون کد ساخته نشد.', 1;
    IF EXISTS (SELECT 1 FROM dbo.ProductOrders WHERE OrderCode IS NULL)
        THROW 51007, N'برای بعضی سفارش‌ها کد ساخته نشد.', 1;

    COMMIT TRANSACTION;
    SELECT @UpdatedCount AS UpdatedRecordCount, @NextSequenceValue AS NextSequenceValue;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
