-- برای هر فروشگاه فعال (Active=1, Deleted=0)، چند روش ارسال پایه (پیک، پست، تحویل حضوری) ثبت می‌کنه.
-- idempotent است — اگه یه فروشگاه از قبل همون نوع ارسال رو داشته باشه، دوباره براش نمی‌سازه (روی
-- StoreId+DeliveryTypeId چک می‌کنه، چون خودِ جدول محدودیت یکتایی روی این جفت نداره).
--
-- قیمت‌ها فرضی/پیش‌فرض‌ان — بعداً از پنل (مدیریت فروشگاه‌ها > روش‌های ارسال) قابل ویرایش‌ان.

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @Courier bigint = 88; -- DeliveryType_Courier — پیک
DECLARE @Post    bigint = 89; -- DeliveryType_Post — پست
DECLARE @InStore bigint = 91; -- DeliveryType_InStore — تحویل حضوری از فروشگاه

IF NOT EXISTS (SELECT 1 FROM dbo.Codes WHERE Id IN (@Courier, @Post, @InStore))
    THROW 51040, N'کدهای نوع ارسال (DeliveryType) پیدا نشدن — قبل از اجرای این اسکریپت مطمئن شو گروه DeliveryType seed شده.', 1;

DECLARE @Methods TABLE
(
    DeliveryTypeId bigint NOT NULL,
    BasePrice float NOT NULL,
    MinPriceForFree float NOT NULL,
    MaxDays int NOT NULL
);

INSERT INTO @Methods (DeliveryTypeId, BasePrice, MinPriceForFree, MaxDays)
VALUES
    (@Courier, 45000, 1500000, 1),  -- پیک: ۴۵٬۰۰۰ تومان، رایگان بالای ۱٫۵ میلیون، حداکثر ۱ روز
    (@Post,    35000, 1000000, 4),  -- پست: ۳۵٬۰۰۰ تومان، رایگان بالای ۱ میلیون، حداکثر ۴ روز
    (@InStore, 0,     0,       0);  -- تحویل حضوری: رایگان، همون روز

INSERT INTO dbo.Deliveries
(
    DeliveryTypeId, BasePrice, MinPriceForFree, MinCountForFree, MaxDays,
    CityId, StateId, Active, Deleted, AfterRent, ShippingProvider,
    LivePricing, AllowPrepaid, AllowReceiverPay, StoreId
)
SELECT
    m.DeliveryTypeId, m.BasePrice, m.MinPriceForFree, 0, m.MaxDays,
    NULL, NULL, 1, 0, 0, 0 /*ShippingProviderEnum.None*/,
    0, 1, 0, s.Id
FROM dbo.Stores AS s
CROSS JOIN @Methods AS m
WHERE s.Active = 1 AND s.Deleted = 0
  AND NOT EXISTS
  (
      SELECT 1 FROM dbo.Deliveries AS d
      WHERE d.StoreId = s.Id AND d.DeliveryTypeId = m.DeliveryTypeId AND d.Deleted = 0
  );

COMMIT TRANSACTION;

SELECT
    st.Id AS StoreId, st.Name AS StoreName,
    d.Id AS DeliveryId, c.Name AS DeliveryTypeName, d.BasePrice, d.MinPriceForFree, d.MaxDays, d.Active
FROM dbo.Deliveries AS d
INNER JOIN dbo.Stores AS st ON st.Id = d.StoreId
INNER JOIN dbo.Codes AS c ON c.Id = d.DeliveryTypeId
WHERE d.DeliveryTypeId IN (88, 89, 91)
ORDER BY st.Id, d.DeliveryTypeId;
GO
