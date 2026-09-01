-- فقط خواندنیه — هیچ داده‌ای رو تغییر نمی‌ده. برای هر ساعت از شبانه‌روز (۰ تا ۲۳) بررسی می‌کنه آیا یک
-- ردیف PriceCalculation فعال (Deleted=0) پوششش می‌ده یا نه. هر ساعتی که HasCoverage=0 داشته باشه،
-- یعنی یک سفرِ رزرویی (پت‌رسان، حالت دو) که حرکتش دقیقاً توی همون ساعت برنامه‌ریزی بشه، قیمتش صفر
-- محاسبه می‌شه و کلاً ثبت نمی‌شه — همون علتِ گزارش‌شده‌ی «قیمت نمیاد و درخواست ثبت نمی‌شه».

WITH Hours AS
(
    SELECT 0 AS Hour
    UNION ALL
    SELECT Hour + 1 FROM Hours WHERE Hour < 23
)
SELECT
    h.Hour,
    CASE WHEN pc.Id IS NULL THEN 0 ELSE 1 END AS HasCoverage,
    pc.Id AS PriceCalculationId,
    pc.FromTime,
    pc.ToTime,
    pc.Price
FROM Hours AS h
OUTER APPLY
(
    SELECT TOP 1 p.Id, p.FromTime, p.ToTime, p.Price
    FROM dbo.PriceCalculations AS p
    WHERE p.Deleted = 0 AND p.FromTime <= h.Hour AND p.ToTime >= h.Hour
) AS pc
ORDER BY h.Hour
OPTION (MAXRECURSION 24);
GO
