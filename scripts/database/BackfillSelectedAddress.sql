-- بعد از اعمال migration AddAddressIsSelected: برای هر کاربری که از قبل حداقل یک آدرس ذخیره‌شده داره
-- ولی هیچ‌کدوم منتخب نیست (چون ستون تازه با پیش‌فرض false اضافه شده)، قدیمی‌ترین آدرسش رو منتخب می‌کنه —
-- دقیقاً همون رفتاری که برای آدرس‌های تازه (اولین آدرس هر کاربر) از این به بعد خودکار اتفاق می‌افته.
-- idempotent است — روی کاربرهایی که از قبل یک آدرس منتخب دارن هیچ کاری نمی‌کنه.

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

;WITH OldestAddress AS
(
    SELECT a.Id, a.UserId,
           ROW_NUMBER() OVER (PARTITION BY a.UserId ORDER BY a.Id ASC) AS rn
    FROM dbo.Addresses AS a
    WHERE a.Deleted = 0
)
UPDATE dbo.Addresses
SET IsSelected = 1
FROM dbo.Addresses AS addr
INNER JOIN OldestAddress AS oa ON oa.Id = addr.Id AND oa.rn = 1
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Addresses AS existing
    WHERE existing.UserId = addr.UserId AND existing.Deleted = 0 AND existing.IsSelected = 1
);

;WITH OldestTripAddress AS
(
    SELECT ta.Id, ta.UserId,
           ROW_NUMBER() OVER (PARTITION BY ta.UserId ORDER BY ta.Id ASC) AS rn
    FROM dbo.TripAddresses AS ta
    WHERE ta.Deleted = 0
)
UPDATE dbo.TripAddresses
SET IsSelected = 1
FROM dbo.TripAddresses AS addr
INNER JOIN OldestTripAddress AS ota ON ota.Id = addr.Id AND ota.rn = 1
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.TripAddresses AS existing
    WHERE existing.UserId = addr.UserId AND existing.Deleted = 0 AND existing.IsSelected = 1
);

COMMIT TRANSACTION;

SELECT 'Addresses' AS TableName, UserId, COUNT(*) AS SelectedCount
FROM dbo.Addresses WHERE Deleted = 0 AND IsSelected = 1
GROUP BY UserId
HAVING COUNT(*) > 1;

SELECT 'TripAddresses' AS TableName, UserId, COUNT(*) AS SelectedCount
FROM dbo.TripAddresses WHERE Deleted = 0 AND IsSelected = 1
GROUP BY UserId
HAVING COUNT(*) > 1;
GO
