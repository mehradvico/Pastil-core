/*
  Seed-ConsultationPackages.sql
  -----------------------------------------------------------------------------
  داده‌ی اولیه‌ی «پکیج مشاوره آنلاین» (طراحی: backend/Docs/ONLINE_CONSULTATION_PACKAGES_FA.md §۹)

  پیش‌نیاز: مایگریشن AddConsultationPackages اجرا شده باشد (جدول ConsultationPackages).
  اجرا: idempotent است؛ چندبار اجرا کردنش چیزی را دوباره نمی‌سازد و هیچ ردیف موجودی را تغییر نمی‌دهد.
  قبل از اجرا روی سرور اصلی بکاپ بگیرید (scripts/Backup-Database.ps1).

  برای هر «کلینیک فعال» (Companions: Active=1, Approved=1, Deleted=0):
    ۱) اگر CompanionAssistance برای خدمت کاتالوگ ۱۵ (مشاوره آنلاین) ندارد، ساخته می‌شود (Active=1, Approved=1, کارمزد ۰٪).
    (تغییر ۱۴۰۵/۰۷/۰۳) پکیج‌ها دیگر ماتریس ثابت ۸ خانه‌ای نیستند: هر کلینیک خودش پکیج‌های نام‌دار (نام، کانال، مدت از
    فهرست ثابت، قیمت، تصویر) می‌سازد؛ پس این اسکریپت دیگر پکیج پیش‌فرض نمی‌سازد و فقط خدمت ۱۵ را برای کلینیک‌ها فراهم می‌کند.
  کانال‌ها (OnlineSessionChannelEnum): ۱ چت، ۲ تماس درون‌برنامه‌ای، ۳ تماس تصویری، ۴ تماس تلفنی.

  اگر خدمت کاتالوگ ۱۵ وجود نداشته باشد اسکریپت بدون تغییر متوقف می‌شود.
*/
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
-- هر خطا کل تراکنش را برمی‌گرداند (بدون اعمال نیمه‌کاره)
SET XACT_ABORT ON;

DECLARE @AssistanceId BIGINT = 15;

IF NOT EXISTS (SELECT 1 FROM Assistances WHERE Id = @AssistanceId AND Deleted = 0)
BEGIN
    PRINT N'خدمت کاتالوگ ۱۵ (مشاوره آنلاین) پیدا نشد؛ هیچ تغییری اعمال نشد.';
    RETURN;
END;

-- نوع همراه پیش‌فرض «کلینیک» (Codes.Id=40, CompanionType_Clinic) برای کلینیک‌هایی که هنوز هیچ خدمتی ندارند
IF NOT EXISTS (SELECT 1 FROM Codes WHERE Id = 40)
BEGIN
    PRINT N'کد نوع همراه ۴۰ (CompanionType_Clinic) در جدول Codes نیست؛ هیچ تغییری اعمال نشد.';
    RETURN;
END;

BEGIN TRANSACTION;

-- ۱) خدمت «مشاوره آنلاین» برای کلینیک‌هایی که ندارند
DECLARE @CreatedAssistances INT = 0;

INSERT INTO CompanionAssistances (CompanionId, AssistanceId, IsSinglePackage, Active, CompanionTypeId, Approved, Deleted, CommissionPercent)
SELECT c.Id,
       @AssistanceId,
       0,
       1,
       -- نوع همراه: همان نوعی که کلینیک در سایر خدماتش دارد، وگرنه «کلینیک» (کد ۴۰)
       COALESCE((SELECT TOP (1) ca.CompanionTypeId FROM CompanionAssistances ca WHERE ca.CompanionId = c.Id AND ca.Deleted = 0 ORDER BY ca.Id), 40),
       1,
       0,
       0
FROM Companions c
WHERE c.Active = 1 AND c.Approved = 1 AND c.Deleted = 0
  AND NOT EXISTS (SELECT 1 FROM CompanionAssistances ca
                  WHERE ca.CompanionId = c.Id AND ca.AssistanceId = @AssistanceId AND ca.Deleted = 0);
SET @CreatedAssistances = @@ROWCOUNT;

-- ۲) پکیج پیش‌فرض ساخته نمی‌شود؛ کلینیک پکیج‌های نام‌دار خودش را از پنل نماینده تعریف می‌کند.


COMMIT TRANSACTION;

-- گزارش
SELECT
    (SELECT COUNT(*) FROM Companions WHERE Active = 1 AND Approved = 1 AND Deleted = 0) AS ActiveClinics,
    @CreatedAssistances AS CreatedConsultationAssistances,
    (SELECT COUNT(*) FROM ConsultationPackages WHERE Deleted = 0) AS TotalPackages;
