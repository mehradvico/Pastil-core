-- تنظیم پارامترهای سراسری SQL Server production (هاست ۱۶ هسته / ۵۱ گیگ رم، اندازه‌گیری ۲۰۲۶-۱۰-۳۱).
-- هر سه مقدار پیش‌فرض نصب بودند. اجرا بدون downtime است (RECONFIGURE فوری اعمال می‌شود).
-- اگر روی همین هاست سرویس دیگری (کانتینر/برنامه) رم می‌گیرد، max server memory را به همان اندازه کم کنید.
--
-- برگشت به حالت قبل:
--   max server memory (MB) = 2147483647, max degree of parallelism = 0, cost threshold for parallelism = 5

-- وضعیت فعلی
SELECT name, value_in_use
FROM sys.configurations
WHERE name IN ('max server memory (MB)', 'max degree of parallelism', 'cost threshold for parallelism');

EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;

-- دیتابیس روی SQL مدیریت‌شده‌ی لیارا است؛ ۵۱ گیگ/۱۶ هسته ممکن است سخت‌افزار کل نود باشد نه پلن ما.
-- این عدد را ۸۰٪ رم پلن لیارا بگذارید (مثلاً پلن ۸ گیگ → 6554). ۴۵۰۵۶ فقط اگر واقعاً ۵۱ گیگ مال این سرور است.
EXEC sp_configure 'max server memory (MB)', 45056;
-- با ۱۶ هسته، هر کوئری حداکثر ۸ هسته بگیرد
EXEC sp_configure 'max degree of parallelism', 8;
-- کوئری‌های سبک OLTP موازی نشوند (wait بالای CXSYNC_PORT)
EXEC sp_configure 'cost threshold for parallelism', 50;
RECONFIGURE;

-- وضعیت بعد از تغییر
SELECT name, value_in_use
FROM sys.configurations
WHERE name IN ('max server memory (MB)', 'max degree of parallelism', 'cost threshold for parallelism');
