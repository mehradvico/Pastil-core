-- تبدیل یک «نمایندگی قدیمی» (Companion) به «پانسیون» (Pansion).
-- مثال: پاپ هاب هتل (Companion شناسه‌ی 10022).
--
-- ایمنی و رفتار:
--  * Companion پاک یا تغییر داده نمی‌شود؛ پانسیون جدید زیر همان Companion ساخته می‌شود (Pansion.CompanionId). مالک/ارائه‌دهنده، رزروها و تسویه از همین Companion می‌آیند.
--  * خدمت‌ها و بسته‌های Companion دست‌نخورده می‌مانند. اگر آن‌ها هم باید برود، جدا بررسی کنید.
--  * اگر Companion هیچ خدمتی (CompanionAssistance) و هیچ پانسیون/مدرسه‌ی حذف‌نشده‌ی دیگری نداشته باشد، بعد از Deploy کد جدید این Companion از لیست «نمایندگی‌ها» مخفی می‌شود و فقط پانسیون دیده می‌شود (نیازمند Deploy نسخه‌ی جدید Api).
--  * Idempotent: اگر برای این Companion پانسیونی با همین نام باشد، دوباره ساخته نمی‌شود.
--  * قیمت هر شب (PansionPrice)، ساعت‌ها و درصد کمیسیون صفر می‌ماند؛ در پنل (ویرایش پانسیون) تنظیم کنید. پیش‌فرض: پانسیون شبانه (IsDaycare = NULL)؛ برای مهد، مقدار را 1 کنید.
--  * پت‌های قابل‌قبول Companion (CompanionPets) به PansionPets کپی می‌شود.
--  * همه‌چیز در یک تراکنش است؛ اول با @Commit = 0 اجرا کنید (فقط بررسی/ROLLBACK).

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

DECLARE @CompanionId bigint = 10022;
DECLARE @Commit bit = 0;          -- 0 = فقط بررسی (ROLLBACK)، 1 = ذخیره
DECLARE @IsDaycare bit = NULL;    -- NULL/0 = پانسیون شبانه، 1 = مهد

BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM Companions WHERE Id = @CompanionId AND Deleted = 0)
BEGIN
    PRINT N'Companion پیدا نشد یا حذف شده است؛ هیچ تغییری اعمال نشد.';
    ROLLBACK TRANSACTION;
    RETURN;
END

IF EXISTS (SELECT 1 FROM Pansions p JOIN Companions c ON c.Id = p.CompanionId WHERE p.CompanionId = @CompanionId AND p.Deleted = 0 AND p.Name = c.Name)
BEGIN
    PRINT N'برای این Companion پانسیونی با همین نام از قبل هست؛ چیزی ساخته نشد.';
    ROLLBACK TRANSACTION;
    RETURN;
END

DECLARE @NewPansion TABLE (Id bigint);

INSERT INTO Pansions
    (Name, IsDaycare, CompanionId, Active, ShowToSite, Approve, ApprovalValue, StateId, CityId, Discription, AddressValue, Location,
     CommentCount, RateAvg, RateCount, PictureId, Suggested, Deleted, DeleteDate, DeletedByUserId,
     IsInfectious, PansionPrice, SchoolPrice, Regulations, OpenHour, CloseHour, DailyCommissionPercent, HourlyCommissionPercent)
OUTPUT inserted.Id INTO @NewPansion (Id)
SELECT
    c.Name, @IsDaycare, c.Id, c.Active, c.ShowToSite, c.Approved, c.ActivationValue, ci.StateId, c.CityId, c.Description, c.AddressValue, c.Location,
    c.CommentCount, c.RateAvg, c.RateCount, c.PictureId, 0, 0, NULL, NULL,
    0, 0, 0, NULL, NULL, NULL, 0, 0
FROM Companions c
LEFT JOIN Cities ci ON ci.Id = c.CityId
WHERE c.Id = @CompanionId;

INSERT INTO PansionPets (PansionId, PetId, Deleted)
SELECT n.Id, cp.PetId, cp.Deleted
FROM CompanionPets cp
CROSS JOIN @NewPansion n
WHERE cp.CompanionId = @CompanionId;

SELECT p.Id AS PansionId, p.Name, p.CompanionId, p.Active, p.Approve, p.StateId, p.CityId,
       (SELECT COUNT(*) FROM PansionPets pp WHERE pp.PansionId = p.Id) AS Pets,
       (SELECT COUNT(*) FROM CompanionAssistances ca WHERE ca.CompanionId = @CompanionId AND ca.Deleted = 0) AS LegacyServices
FROM Pansions p JOIN @NewPansion n ON n.Id = p.Id;

IF @Commit = 1
BEGIN
    COMMIT TRANSACTION;
    PRINT N'COMMIT انجام شد.';
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT N'ROLLBACK شد (فقط بررسی). برای ذخیره @Commit را 1 کنید.';
END
