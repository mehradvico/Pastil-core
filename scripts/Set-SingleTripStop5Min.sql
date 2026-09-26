-- تعرفه‌ی انتظار پت‌رسان: فقط یک ردیف «۵ دقیقه انتظار» (قیمت هر ۵ دقیقه). قیمت را در @Price بگذارید (تومان).
-- ردیف‌های قدیمی حذف منطقی می‌شوند (سفرهای قدیمی که به آنها ارجاع دارند سالم می‌مانند).
DECLARE @Price FLOAT = 15000;

BEGIN TRAN;

DECLARE @KeepId BIGINT = (SELECT TOP 1 Id FROM TripStops WHERE Deleted = 0 ORDER BY Id);

IF @KeepId IS NULL
BEGIN
    INSERT INTO TripStops (Name, Price, Active, Deleted) VALUES (N'۵ دقیقه انتظار', @Price, 1, 0);
END
ELSE
BEGIN
    UPDATE TripStops SET Name = N'۵ دقیقه انتظار', Price = @Price, Active = 1 WHERE Id = @KeepId;
    UPDATE TripStops SET Deleted = 1, Active = 0 WHERE Deleted = 0 AND Id <> @KeepId;
END

SELECT Id, Name, Price, Active, Deleted FROM TripStops WHERE Deleted = 0;

COMMIT;
