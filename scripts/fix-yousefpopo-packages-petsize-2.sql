-- fix-yousefpopo-packages-petsize-2.sql
-- ترمیم دوم PetSize پکیج‌های آرایشگاه «یوسف پو پو» (CompanionAssistanceId = 54).
--
-- چرا دوباره لازم شد: کوئری قبلی (fix-yousefpopo-packages-petsize.sql) بر اساس نامِ
-- اصلی‌ای که خودم موقع INSERT گذاشته بودم، پکیج‌ها رو پیدا می‌کرد. ولی روی دیتابیس
-- زنده چک کردم و الان اسم ۳ تا سرویس «شستشو و آرایش + آمدورفت عمو یوسف» و ۳ تا
-- «کوتاهی و کامل» کمی متفاوت شده (مثلاً «کوتاهی و کامل نژاد بزرگ» شده «کوتاهی کامل
-- نژاد بزرگ»، یا پسوند «با آمدورفت عمو یوسف» از اسمشون افتاده) — به‌احتمال زیاد
-- هم‌زمان با ویرایش/آپلود عکس از پنل، اسم‌ها هم کمی اصلاح شده. چون کوئری قبلی دقیقاً
-- با همون نام قدیمی مچ می‌کرد، این ۶ ردیف رو اصلاً پیدا نکرد و PetSizeشون همچنان
-- null مونده بود (دقیقاً همون‌هایی که الان توی اپ برای هر سایز پتی نمایش داده می‌شن).
--
-- این‌بار به‌جای Name، مستقیم از روی Id واقعی‌ای که همین الان از API زنده گرفتم
-- (بدون هیچ ابهامی) آپدیت می‌کنم. به هیچ فیلد دیگه‌ای (Name، Price، PictureId) دست
-- نمی‌زنه.

UPDATE dbo.CompanionAssistancePackages SET PetSize = N'Small'  WHERE Id = 10029; -- شستشو و آرایش نژاد کوچک
UPDATE dbo.CompanionAssistancePackages SET PetSize = N'Medium' WHERE Id = 10030; -- شستشو و آرایش نژاد متوسط
UPDATE dbo.CompanionAssistancePackages SET PetSize = N'Large'  WHERE Id = 10031; -- شستشو و آرایش نژاد بزرگ

UPDATE dbo.CompanionAssistancePackages SET PetSize = N'Small'  WHERE Id = 10014; -- کوتاهی کامل نژاد کوچک
UPDATE dbo.CompanionAssistancePackages SET PetSize = N'Medium' WHERE Id = 10015; -- کوتاهی کامل نژاد متوسط
UPDATE dbo.CompanionAssistancePackages SET PetSize = N'Large'  WHERE Id = 10016; -- کوتاهی کامل نژاد بزرگ

-- بررسی سریع نتیجه پس از اجرا (باید همه‌ی ۱۲ ردیف سایزدار Small/Medium/Large داشته باشن):
-- SELECT Id, Name, PetSize, PictureId FROM dbo.CompanionAssistancePackages WHERE CompanionAssistanceId = 54 ORDER BY Id;
