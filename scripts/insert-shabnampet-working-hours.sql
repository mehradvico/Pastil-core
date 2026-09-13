-- insert-shabnampet-working-hours.sql
-- تعریف ساعات کاری «شبنم پت» (Companion Id = 10019) - شنبه تا پنج‌شنبه، هر روز از
-- ۰۸:۰۰ تا ۲۰:۰۰ در بازه‌های ۲ ساعته (۶ بازه در روز)، جمعه عمداً خالی؛ همون الگویی که
-- برای بقیه‌ی مراکز با seed-companion-working-hours.sql تعریف شده - با Capacity = 3
-- برای هر بازه (تعداد نوبت هم‌زمان قابل رزرو در آن بازه).
--
-- تأیید شده زنده در پنل (/admin/companion-time?companionId=10019): این مرکز فعلاً
-- هیچ ساعت کاری‌ای ثبت نکرده، پس این INSERT خالص است و چیزی رو بازنویسی/حذف نمی‌کنه.
--
-- WeekDayId: ۱=شنبه، ۲=یکشنبه، ۳=دوشنبه، ۴=سه‌شنبه، ۵=چهارشنبه، ۶=پنج‌شنبه، ۷=جمعه.

DECLARE @CompanionId BIGINT = 10019;

DECLARE @Slots TABLE (StartTime NVARCHAR(5), EndTime NVARCHAR(5));
INSERT INTO @Slots (StartTime, EndTime) VALUES
('08:00','10:00'),
('10:00','12:00'),
('12:00','14:00'),
('14:00','16:00'),
('16:00','18:00'),
('18:00','20:00');

DECLARE @WeekDays TABLE (WeekDayId BIGINT);
INSERT INTO @WeekDays (WeekDayId) VALUES (1),(2),(3),(4),(5),(6); -- شنبه..پنج‌شنبه؛ جمعه(۷) عمداً حذف شده

INSERT INTO dbo.CompanionTimes (StartTime, EndTime, Capacity, Active, Deleted, WeekDayId, CompanionId)
SELECT s.StartTime, s.EndTime, 3, 1, 0, w.WeekDayId, @CompanionId
FROM @WeekDays w
CROSS JOIN @Slots s;
