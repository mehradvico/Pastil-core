-- بازمحاسبه‌ی SystemActive و قیمت کالاهای فروشگاه (۲۰۲۶-۱۱-۱۳)
-- علت: وقتی ادمین یک فروشگاه را «تأیید» می‌کرد، Api procedure قیمت را برای کالاهای آن فروشگاه اجرا نمی‌کرد؛ پس SystemActive
-- همان ۰ مرحله‌ی قبل از تأیید می‌ماند و کالاهای فروشگاه در صفحه‌ی کالا (لیست فروشندگان) دیده نمی‌شدند ولی در صفحه‌ی خود فروشگاه بودند.
-- (در کد اصلاح شد: UpdateApprovalAsync حالا خودش procedure را صدا می‌زند؛ این اسکریپت فقط داده‌ی قدیمی را درست می‌کند.)
-- بی‌خطر است: همان procedure که Api هر بار صدا می‌زند (UpdateProductItemDiscount → UpdateProductPrice). چند بار اجرا هم امن است.
-- وضعیت دیتابیس اصلی (۲۰۲۶-۱۱-۱۳): فقط فروشگاه ۱۴ («پت شاپ فرشته») ۲۶ آیتم کهنه دارد.

SET NOCOUNT ON;
SET LOCK_TIMEOUT 15000;

-- ۱) قبل: آیتم‌هایی که طبق قاعده‌ی procedure باید SystemActive=1 باشند ولی ۰اند، به‌تفکیک فروشگاه
SELECT pi.StoreId, COUNT(*) AS StaleItems
FROM ProductItems pi
JOIN Products p ON p.Id = pi.ProductId
JOIN Stores s ON s.Id = pi.StoreId
JOIN Codes c ON c.Id = p.StatusId
WHERE pi.Deleted = 0 AND pi.Active = 1 AND pi.Price > 0 AND pi.BasePrice > 0 AND pi.Quantity > 0
  AND p.Active = 1 AND p.Deleted = 0
  AND c.Label NOT IN ('ProductStatus_Draft', 'ProductStatus_NotSell', 'ProductStatus_Soon', 'ProductStatus_Soo')
  AND s.Active = 1 AND s.Deleted = 0 AND pi.SystemActive = 0
GROUP BY pi.StoreId ORDER BY pi.StoreId;

-- ۲) بازمحاسبه برای فروشگاه‌های لیست بالا (اینجا فقط فروشگاه ۱۴؛ برای فروشگاه دیگر شناسه را عوض کنید یا خط را تکرار کنید)
EXEC dbo.UpdateProductItemDiscount @FilterType = N'Store', @FilterIds = N'14';

-- ۳) بعد: باید ردیفی برنگردد
SELECT pi.StoreId, COUNT(*) AS StaleItems
FROM ProductItems pi
JOIN Products p ON p.Id = pi.ProductId
JOIN Stores s ON s.Id = pi.StoreId
JOIN Codes c ON c.Id = p.StatusId
WHERE pi.Deleted = 0 AND pi.Active = 1 AND pi.Price > 0 AND pi.BasePrice > 0 AND pi.Quantity > 0
  AND p.Active = 1 AND p.Deleted = 0
  AND c.Label NOT IN ('ProductStatus_Draft', 'ProductStatus_NotSell', 'ProductStatus_Soon', 'ProductStatus_Soo')
  AND s.Active = 1 AND s.Deleted = 0 AND pi.SystemActive = 0
GROUP BY pi.StoreId;

-- نمونه: کالای ۲۵۹۱۳ باید حالا از هر دو فروشگاه (۱۴ و ۱۵) SystemActive=1 داشته باشد
-- SELECT Id, StoreId, Price, Quantity, SystemActive FROM ProductItems WHERE ProductId = 25913 AND Deleted = 0 ORDER BY StoreId;
