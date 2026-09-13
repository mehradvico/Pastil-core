-- fix-yousefpopo-packages-petsize.sql
-- ترمیم فیلد PetSize پکیج‌های آرایشگاه «یوسف پو پو» (CompanionAssistanceId = 54).
--
-- چرا لازم شد: کوئری insert-yousefpopo-grooming-packages.sql مقدار PetSize رو درست
-- ثبت کرده بود، ولی فرم پکیج توی پنل ادمین (قبل از فیکسی که همین الان زدم) موقع
-- ذخیره، کل رکورد رو بدون فیلد PetSize دوباره می‌نوشت. پس وقتی برای هر پکیج از همون
-- فرم پنل عکس آپلود و ذخیره کردید، PetSize هر ۲۹ پکیج به‌طور ناخواسته null شده.
-- این کوئری فقط همون مقدار اصلی PetSize رو برمی‌گردونه؛ به Name، Price، PictureId
-- یا هیچ فیلد دیگه‌ای دست نمی‌زنه (عکس‌هایی که آپلود کردید دست‌نخورده می‌مونن).

DECLARE @CompanionAssistanceId BIGINT = 54;

UPDATE dbo.CompanionAssistancePackages SET PetSize = N'Small'  WHERE CompanionAssistanceId = @CompanionAssistanceId AND Name = N'آرایش مدل‌دار نژاد کوچک';
UPDATE dbo.CompanionAssistancePackages SET PetSize = N'Medium' WHERE CompanionAssistanceId = @CompanionAssistanceId AND Name = N'آرایش مدل‌دار نژاد متوسط';
UPDATE dbo.CompanionAssistancePackages SET PetSize = N'Large'  WHERE CompanionAssistanceId = @CompanionAssistanceId AND Name = N'آرایش مدل‌دار نژاد بزرگ';

UPDATE dbo.CompanionAssistancePackages SET PetSize = N'Small'  WHERE CompanionAssistanceId = @CompanionAssistanceId AND Name = N'آرایش ساده نژاد کوچک';
UPDATE dbo.CompanionAssistancePackages SET PetSize = N'Medium' WHERE CompanionAssistanceId = @CompanionAssistanceId AND Name = N'آرایش ساده نژاد متوسط';
UPDATE dbo.CompanionAssistancePackages SET PetSize = N'Large'  WHERE CompanionAssistanceId = @CompanionAssistanceId AND Name = N'آرایش ساده نژاد بزرگ';

UPDATE dbo.CompanionAssistancePackages SET PetSize = N'Small'  WHERE CompanionAssistanceId = @CompanionAssistanceId AND Name = N'کوتاهی و کامل نژاد کوچک';
UPDATE dbo.CompanionAssistancePackages SET PetSize = N'Medium' WHERE CompanionAssistanceId = @CompanionAssistanceId AND Name = N'کوتاهی و کامل نژاد متوسط';
UPDATE dbo.CompanionAssistancePackages SET PetSize = N'Large'  WHERE CompanionAssistanceId = @CompanionAssistanceId AND Name = N'کوتاهی و کامل نژاد بزرگ';

UPDATE dbo.CompanionAssistancePackages SET PetSize = N'Small'  WHERE CompanionAssistanceId = @CompanionAssistanceId AND Name = N'شستشو و آرایش نژاد کوچک با آمدورفت عمو یوسف';
UPDATE dbo.CompanionAssistancePackages SET PetSize = N'Medium' WHERE CompanionAssistanceId = @CompanionAssistanceId AND Name = N'آرایش نژاد متوسط با آمدورفت عمو یوسف';
UPDATE dbo.CompanionAssistancePackages SET PetSize = N'Large'  WHERE CompanionAssistanceId = @CompanionAssistanceId AND Name = N'آرایش نژاد بزرگ با آمدورفت عمو یوسف';

UPDATE dbo.CompanionAssistancePackages SET PetSize = N'Small'  WHERE CompanionAssistanceId = @CompanionAssistanceId AND Name = N'شستشوی VIP تخصصی نژاد کوچک';
UPDATE dbo.CompanionAssistancePackages SET PetSize = N'Medium' WHERE CompanionAssistanceId = @CompanionAssistanceId AND Name = N'شستشوی VIP تخصصی نژاد متوسط';
UPDATE dbo.CompanionAssistancePackages SET PetSize = N'Large'  WHERE CompanionAssistanceId = @CompanionAssistanceId AND Name = N'شستشوی VIP تخصصی نژاد بزرگ';

-- بقیه‌ی پکیج‌ها (گربه، خرگوش، خدمات جانبی و جزئی) از اول هم PetSize نداشتن؛ کاری باهاشون نداریم.

-- بررسی سریع نتیجه پس از اجرا:
-- SELECT Id, Name, PetSize, PictureId FROM dbo.CompanionAssistancePackages WHERE CompanionAssistanceId = 54 ORDER BY Id;
