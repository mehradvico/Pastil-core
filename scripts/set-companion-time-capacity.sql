-- set-companion-time-capacity.sql
-- ظرفیت (تعداد نوبت هم‌زمان) تمام بازه‌های ساعت کاری‌ای که قبلاً با اسکریپت
-- seed-companion-working-hours.sql برای همه‌ی مراکز ثبت شده بود را روی ۳ می‌گذارد.
-- (توجه: چون خود ردیف‌ها از قبل وجود دارند، این یک UPDATE است نه INSERT.)

UPDATE dbo.CompanionTimes
SET Capacity = 3
WHERE Deleted = 0;
