-- seed-companion-working-hours.sql
-- تعریف ساعات کاری استاندارد برای همه‌ی مراکز (Companions): شنبه تا پنج‌شنبه،
-- هر روز از ۰۸:۰۰ تا ۲۰:۰۰ در بازه‌های ۲ ساعته (۶ بازه در روز)، جمعه عمداً خالی.
--
-- WeekDayId تأیید شده از دیتابیس زنده: ۱=شنبه، ۲=یکشنبه، ۳=دوشنبه، ۴=سه‌شنبه،
-- ۵=چهارشنبه، ۶=پنج‌شنبه، ۷=جمعه (که در این اسکریپت عمداً درج نمی‌شود).
--
-- توجه مهم: این اسکریپت ابتدا تمام ردیف‌های موجود CompanionTimes را برای همه‌ی
-- مراکز پاک می‌کند و سپس الگوی استاندارد بالا را از نو درج می‌کند (idempotent -
-- اجرای دوباره‌اش نتیجه تکراری یا اضافه ایجاد نمی‌کند). اگر مرکز/نماینده‌ای از قبل
-- ساعات کاری دستی و متفاوت ثبت کرده، آن ساعات با اجرای این اسکریپت از بین می‌رود.

BEGIN TRAN;

DELETE FROM dbo.CompanionTimes
WHERE CompanionId IN (SELECT Id FROM dbo.Companions);

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

INSERT INTO dbo.CompanionTimes (StartTime, EndTime, Active, Deleted, WeekDayId, CompanionId)
SELECT s.StartTime, s.EndTime, 1, 0, w.WeekDayId, c.Id
FROM dbo.Companions c
CROSS JOIN @WeekDays w
CROSS JOIN @Slots s;

COMMIT TRAN;
