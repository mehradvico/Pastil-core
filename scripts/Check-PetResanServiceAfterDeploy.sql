/*
  بررسی بعد از deploy: سرویس هفتگی پت‌رسان (اولین پرداخت توسط کاربر + پوش شارژ کیف پول)
  فقط SELECT است؛ هیچ داده‌ای را تغییر نمی‌دهد. بخش‌ها را به‌ترتیب اجرا کنید.
  وضعیت‌ها: 75 = درخواست‌شده، 76 = پذیرفته‌شده، 77 = لغو.
*/

-- ۱) مایگریشن seed پوش ۱۱۸ اعمال شده؟ (باید یک ردیف برگردد)
SELECT MigrationId
FROM __EFMigrationsHistory
WHERE MigrationId = N'20261008120000_SeedServiceWalletTopUpPush';

-- ۲) نوع، الگو و تنظیم پوش ۱۱۸ (باید هر سه ردیف باشند و IsEnabled/IsActive = 1)
SELECT t.Id AS PushTypeId, t.Label, p.Id AS PatternId, p.IsActive, p.Url, s.IsEnabled
FROM PushTypes t
LEFT JOIN PushPatterns p ON p.PushTypeId = t.Id
LEFT JOIN PushSettings s ON s.PushPatternId = p.Id
WHERE t.Id = 118;

-- ۳) نوبت‌های سرویس هفتگی که بعد از deploy ساخته شده‌اند.
--    @DeployTime را به زمان deploy (ساعت تهران) تغییر دهید.
--    انتظار: اولین نوبت هر سرویس TripStatusId = 75 و IsPaid = 0 (لغو نشده).
DECLARE @DeployTime datetime2 = '2026-10-08 12:00';

SELECT t.Id, t.TripStatusId, t.DriverId, t.IsOnline, t.IsPaid, t.FromWallet, t.WalletPrice, t.Price,
       t.TripStartDateTime, t.CreateDate, t.IsReturnLeg, t.CancelReasonDetail,
       t.PetResanServiceScheduleId, sch.PetResanServiceId
FROM Trips t
JOIN PetResanServiceSchedules sch ON sch.Id = t.PetResanServiceScheduleId
WHERE t.CreateDate >= @DeployTime
ORDER BY t.Id DESC;

-- ۴) هشدار: نوبت‌هایی که هنوز با دلیل «کیف پول» لغو می‌شوند.
--    برای سرویسی که هیچ سفر پرداخت‌شده‌ای ندارد نباید بعد از deploy ردیفی بیاید
--    (برای سرویس‌های دارای سفر پرداخت‌شده و کیف پول خالی، این ردیف‌ها عادی‌اند).
SELECT t.Id, t.CreateDate, t.TripStartDateTime, sch.PetResanServiceId,
       CASE WHEN EXISTS (SELECT 1 FROM Trips p
                         JOIN PetResanServiceSchedules ps ON ps.Id = p.PetResanServiceScheduleId
                         WHERE ps.PetResanServiceId = sch.PetResanServiceId AND p.IsPaid = 1)
            THEN N'دارای سفر پرداخت‌شده (کسر از کیف پول عادی است)'
            ELSE N'⚠ بدون سفر پرداخت‌شده - نباید لغو می‌شد' END AS Verdict
FROM Trips t
JOIN PetResanServiceSchedules sch ON sch.Id = t.PetResanServiceScheduleId
WHERE t.TripStatusId = 77
  AND t.CancelReasonDetail LIKE N'%کیف پول%'
  AND t.CreateDate >= @DeployTime
ORDER BY t.Id DESC;

-- ۵) نوبت‌هایی که الان باید در TripAvailable راننده‌ها دیده شوند
--    (همان شرط‌های GetAvailableTripsForDriverAsync؛ نوع خودرو برای سرویس خالی است)
SELECT t.Id, t.TripStartDateTime, t.IsPaid, t.CreateDate, sch.PetResanServiceId
FROM Trips t
JOIN PetResanServiceSchedules sch ON sch.Id = t.PetResanServiceScheduleId
WHERE t.TripStatusId = 75 AND t.DriverId IS NULL AND t.IsOnline = 1
ORDER BY t.TripStartDateTime;

-- ۶) نوبت‌های قبول‌شده‌ی پرداخت‌نشده: کاربر باید پرداخت کند (دکمه‌ی «پرداخت» در اپ).
SELECT t.Id, t.UserId, t.DriverId, t.TripStartDateTime, t.PaymentPrice, t.IsPaid
FROM Trips t
WHERE t.PetResanServiceScheduleId IS NOT NULL
  AND t.TripStatusId = 76 AND t.IsPaid = 0
ORDER BY t.TripStartDateTime;

-- ۷) پوش‌های «شارژ کیف پول» که ارسال شده‌اند (job ساعت ۱۸ یا لغوِ کمبود موجودی)
SELECT n.Id, n.UserId, n.Token1 AS ShortfallToman, n.CreateDate
FROM PushNotifications n
JOIN PushPatterns p ON p.Id = n.PushPatternId
WHERE p.PushTypeId = 118
ORDER BY n.Id DESC;
