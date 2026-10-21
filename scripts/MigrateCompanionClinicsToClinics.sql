-- انتقال کلینیک‌های فعلی (Companion با نوع «کلینیک/آزمایشگاه/پرستار») به جدول‌های جدید Clinic*.
-- پیش‌نیاز: اسکریپت AddClinics.sql (مایگریشن AddClinics) اجرا شده باشد.
--
-- ایمنی:
--  * هیچ ردیف قدیمی پاک یا تغییر داده نمی‌شود؛ فقط ردیف‌های جدید ساخته می‌شود. رزروها، پرداخت‌ها و تسویه‌های قبلی روی Companion باقی می‌مانند.
--  * Idempotent: هر Companion با Clinics.LegacyCompanionId فقط یک‌بار منتقل می‌شود؛ دوباره اجرا کردن چیز تازه‌ای نمی‌سازد.
--  * همه‌چیز در یک تراکنش است. اول با @Commit = 0 اجرا کنید (ROLLBACK)، خروجی شمارش‌ها را ببینید؛ بعد @Commit = 1.
--
-- نکته‌ها:
--  * شناسه‌ی نوع‌ها: کلینیک 40، آزمایشگاه 42، پرستار 44 (CompanionTypeEnum). Companion شخصی (IsPersonal = 1) منتقل نمی‌شود.
--  * خدمت‌های گرومینگ (Assistances.IsGrooming = 1) منتقل نمی‌شوند (مال آرایشگاه‌اند). قبل از اجرا خدمات گرومینگ را در پنل علامت بزنید.
--  * «نحوه‌ی ارائه» هر بسته: ردیف‌های CompanionAssistancePackageTypes عیناً کپی می‌شود. بسته‌ی بدون ردیف، برای هر حالتِ خدمت
--    (CodeCompanionAssistance: 37 آنلاین، 38 حضوری، 39 در محل) با قیمت پایه‌ی خودش یک ردیف می‌گیرد تا رفتار قبلی عیناً حفظ شود.
--  * پرچم‌های کلینیک: ServiceOnline/ServiceInPerson/ServiceAtHome از حالت‌های خدمت‌ها (و منطقه‌ی پوشش) به‌دست می‌آید؛ اگر هیچ حالتی ثبت نشده، حضوری روشن می‌شود.
--  * مدت خدمت (DurationMinutes) در Companion وجود ندارد؛ صفر می‌ماند و سیستم 60 دقیقه فرض می‌کند. ساعت کاری از CompanionTimes است؛ اگر کلینیکی
--    CompanionTimes نداشت از CompanionAssistanceTimes (ظرفیت 1) ساخته می‌شود.
--  * نظرات (CompanionComments) کپی می‌شوند؛ لایک‌های هر نظر (CommentLikes) کپی نمی‌شوند. رزروهای قدیمی به ClinicReserve منتقل نمی‌شوند.

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON; -- لازم برای ایندکس‌های فیلتری (Clinics)
SET XACT_ABORT ON;

DECLARE @Commit bit = 0;   -- 0 = فقط بررسی (ROLLBACK)، 1 = ذخیره

DECLARE @TypeIds TABLE (Id bigint PRIMARY KEY);
INSERT INTO @TypeIds (Id) VALUES (40), (42), (44);

-- اختیاری: Companionهایی که نوع کلینیک/آزمایشگاه/پرستار ندارند ولی باید کلینیک شوند (مثلاً «بیمارستان دامپزشکی آوینا» = 10018).
-- شخصی‌بودن برای این‌ها مانع نیست و اگر نوعی نداشته باشند نوع پیش‌فرض کلینیک (40) می‌گیرند.
DECLARE @CompanionIds TABLE (Id bigint PRIMARY KEY);
-- INSERT INTO @CompanionIds (Id) VALUES (10018);

BEGIN TRANSACTION;

-- ۱) Companionهای کلینیکی که هنوز منتقل نشده‌اند
SELECT c.Id AS CompanionId
INTO #NewClinics
FROM Companions c
WHERE c.Deleted = 0
  AND ((c.IsPersonal = 0
        AND EXISTS (SELECT 1 FROM CompanionTypes ct WHERE ct.CompanionId = c.Id AND ct.Deleted = 0 AND ct.TypeId IN (SELECT Id FROM @TypeIds)))
       OR c.Id IN (SELECT Id FROM @CompanionIds))
  AND NOT EXISTS (SELECT 1 FROM Clinics k WHERE k.LegacyCompanionId = c.Id);

-- ۲) Clinics (Id خودکار؛ نگاشت قدیمی ← جدید با LegacyCompanionId)
INSERT INTO Clinics
    (Name, Summary, Description, ProviderId, OwnerId, LegacyCompanionId, ReferralCode,
     PictureId, BackgroundPictureId, IconId, Phone, AddressValue, StateId, CityId, NeighborhoodId, Location,
     Active, ShowToSite, Approved, ActivationValue, Suggested, Deleted, DeleteDate, DeletedByUserId, CreateDate,
     CommentCount, RateAvg, RateCount, SearchKey, GoldAccountDate, SilverAccountDate,
     SeoH1, SeoMinDescription, SeoDescription, SeoTitle, SeoPictureAlt, SeoUrlText, SeoNoIndex, SeoNoFollow, SeoCanonical,
     Regulations, EstablishedYear, LicenseNumber, LicenseApproved, InstagramUrl, WebsiteUrl,
     ServiceAtHome, ServiceInPerson, ServiceOnline, HasParking, HasWaitingRoom, HasCamera,
     MaxConcurrentPets, StaffCount, CommissionPercent)
SELECT
    c.Name, c.Summary, c.Description, c.ProviderId, c.OwnerId, c.Id, c.ReferralCode,
    c.PictureId, c.BackgroundPictureId, c.IconId, c.Phone, c.AddressValue, ci.StateId, c.CityId, c.NeighborhoodId, c.Location,
    c.Active, c.ShowToSite, c.Approved, c.ActivationValue, 0, 0, NULL, NULL, SYSDATETIME(),
    c.CommentCount, c.RateAvg, c.RateCount, c.SearchKey, c.GoldAccountDate, c.SilverAccountDate,
    c.SeoH1, c.SeoMinDescription, c.SeoDescription, c.SeoTitle, c.SeoPictureAlt, c.SeoUrlText, c.SeoNoIndex, c.SeoNoFollow, c.SeoCanonical,
    NULL, NULL, NULL, 0, NULL, NULL,
    0, 0, 0, 0, 0, 0,
    0,
    (SELECT COUNT(*) FROM CompanionUsers cu WHERE cu.CompanionId = c.Id AND cu.Deleted = 0),
    0
FROM Companions c
JOIN #NewClinics n ON n.CompanionId = c.Id
LEFT JOIN Cities ci ON ci.Id = c.CityId;

SELECT k.LegacyCompanionId AS CompanionId, k.Id AS ClinicId
INTO #ClinicMap
FROM Clinics k
JOIN #NewClinics n ON n.CompanionId = k.LegacyCompanionId;

-- ۲.۱) انواع کلینیک
INSERT INTO ClinicTypes (ClinicId, TypeId, Deleted)
SELECT cm.ClinicId, ct.TypeId, 0
FROM CompanionTypes ct
JOIN #ClinicMap cm ON cm.CompanionId = ct.CompanionId
WHERE ct.Deleted = 0 AND ct.TypeId IN (SELECT Id FROM @TypeIds);

-- کلینیکی که هیچ نوعی نگرفت (فقط با @CompanionIds ممکن است): نوع پیش‌فرض کلینیک
INSERT INTO ClinicTypes (ClinicId, TypeId, Deleted)
SELECT cm.ClinicId, 40, 0
FROM #ClinicMap cm
WHERE NOT EXISTS (SELECT 1 FROM ClinicTypes x WHERE x.ClinicId = cm.ClinicId);

-- ۳) خدمات (CompanionAssistance → ClinicService) با نگاشت شناسه؛ خدمت گرومینگ منتقل نمی‌شود
CREATE TABLE #ServiceMap (OldId bigint PRIMARY KEY, NewId bigint NOT NULL);

MERGE INTO ClinicServices AS target
USING (
    SELECT ca.Id AS OldId, cm.ClinicId, ca.AssistanceId, ca.Active, ca.Approved, ca.ActivationValue, ca.Deleted, ca.CommissionPercent
    FROM CompanionAssistances ca
    JOIN #ClinicMap cm ON cm.CompanionId = ca.CompanionId
    JOIN Assistances a ON a.Id = ca.AssistanceId
    WHERE a.IsGrooming = 0
) AS src ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (ClinicId, AssistanceId, Active, Approved, ActivationValue, Deleted, CommissionPercent, DurationMinutes, Description)
    VALUES (src.ClinicId, src.AssistanceId, src.Active, src.Approved, src.ActivationValue, src.Deleted, src.CommissionPercent, 0, NULL)
OUTPUT src.OldId, inserted.Id INTO #ServiceMap (OldId, NewId);

-- ۴) بسته‌های قیمتی
CREATE TABLE #PackageMap (OldId bigint PRIMARY KEY, NewId bigint NOT NULL);

MERGE INTO ClinicServicePackages AS target
USING (
    SELECT p.Id AS OldId, sm.NewId AS ClinicServiceId, p.Name, p.Price, p.PrePaymentPrice, p.PictureId, p.Active, p.ActivationValue,
           p.Discription, p.PetSize, p.Deleted
    FROM CompanionAssistancePackages p
    JOIN #ServiceMap sm ON sm.OldId = p.CompanionAssistanceId
) AS src ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (ClinicServiceId, Name, Price, PrePaymentPrice, PictureId, Active, ActivationValue, Discription, PetSize, Deleted)
    VALUES (src.ClinicServiceId, src.Name, src.Price, src.PrePaymentPrice, src.PictureId, src.Active, src.ActivationValue, src.Discription, src.PetSize, src.Deleted)
OUTPUT src.OldId, inserted.Id INTO #PackageMap (OldId, NewId);

INSERT INTO ClinicServicePackagePictures (ClinicServicePackageId, PictureId, Label, Deleted)
SELECT pm.NewId, pp.PictureId, NULL, 0
FROM CompanionAssistancePackagePictures pp
JOIN #PackageMap pm ON pm.OldId = pp.CompanionAssistancePackageId;

-- ۴.۱) نحوه‌ی ارائه‌ی بسته‌ها: ردیف‌های موجود عیناً کپی می‌شود
INSERT INTO ClinicServicePackageTypes (ClinicServicePackageId, Mode, Price, PrePaymentPrice, Deleted)
SELECT pm.NewId, CAST(pt.CompanionAssistanceTypeId AS int), pt.Price, pt.PrePaymentPrice, 0
FROM CompanionAssistancePackageTypes pt
JOIN #PackageMap pm ON pm.OldId = pt.CompanionAssistancePackageId
WHERE pt.Deleted = 0;

-- بسته‌ی بدون ردیف: برای هر حالتِ خدمت با قیمت پایه (رفتار قدیمی عیناً حفظ می‌شود)
INSERT INTO ClinicServicePackageTypes (ClinicServicePackageId, Mode, Price, PrePaymentPrice, Deleted)
SELECT pm.NewId, CAST(code.CodesId AS int), p.Price, p.PrePaymentPrice, 0
FROM #PackageMap pm
JOIN CompanionAssistancePackages p ON p.Id = pm.OldId
JOIN CodeCompanionAssistance code ON code.CompanionAssistancesId = p.CompanionAssistanceId
WHERE code.CodesId IN (37, 38, 39)
  AND NOT EXISTS (SELECT 1 FROM CompanionAssistancePackageTypes pt WHERE pt.CompanionAssistancePackageId = p.Id AND pt.Deleted = 0);

-- ۵) پزشک/پرسنلِ مجاز هر خدمت
INSERT INTO ClinicServiceUsers (ClinicServiceId, UserId, Active, ActivationValue, Deleted)
SELECT sm.NewId, au.UserId, au.Active, au.ActivationValue, au.Deleted
FROM CompanionAssistanceUsers au
JOIN #ServiceMap sm ON sm.OldId = au.CompanionAssistanceId;

-- ۶) پرسنل، ساعت کاری، محدوده‌ی پوشش، گونه‌ها
INSERT INTO ClinicUsers (ClinicId, UserId, UserAccept, InviteDate, Active, ActivationValue, Deleted)
SELECT cm.ClinicId, cu.UserId, cu.UserAccept, NULL, cu.Active, cu.ActivationValue, cu.Deleted
FROM CompanionUsers cu
JOIN #ClinicMap cm ON cm.CompanionId = cu.CompanionId;

INSERT INTO ClinicTimes (ClinicId, WeekDayId, StartTime, EndTime, Capacity, Active, Deleted)
SELECT cm.ClinicId, t.WeekDayId, t.StartTime, t.EndTime, t.Capacity, t.Active, t.Deleted
FROM CompanionTimes t
JOIN #ClinicMap cm ON cm.CompanionId = t.CompanionId;

-- کلینیکی که ساعت کاری سطح مکان نداشت: از ساعت‌های سطح خدمت (ظرفیت 1)
INSERT INTO ClinicTimes (ClinicId, WeekDayId, StartTime, EndTime, Capacity, Active, Deleted)
SELECT DISTINCT cm.ClinicId, at.WeekDayId, at.StartTime, at.EndTime, 1, at.Active, 0
FROM CompanionAssistanceTimes at
JOIN CompanionAssistances ca ON ca.Id = at.CompanionAssistanceId
JOIN #ClinicMap cm ON cm.CompanionId = ca.CompanionId
WHERE at.Deleted = 0 AND at.Active = 1
  AND NOT EXISTS (SELECT 1 FROM ClinicTimes x WHERE x.ClinicId = cm.ClinicId);

INSERT INTO ClinicZones (ClinicId, StateId, CityId, NeighborhoodId, Deleted)
SELECT cm.ClinicId, z.StateId, z.CityId, z.NeighborhoodId, z.Deleted
FROM CompanionZones z
JOIN #ClinicMap cm ON cm.CompanionId = z.CompanionId;

INSERT INTO ClinicPets (ClinicId, PetId, Deleted)
SELECT cm.ClinicId, p.PetId, p.Deleted
FROM CompanionPets p
JOIN #ClinicMap cm ON cm.CompanionId = p.CompanionId;

-- پرچم‌های نحوه‌ی ارائه از حالت‌های خدمت‌ها (و منطقه‌ی پوشش)
UPDATE k SET
    k.ServiceOnline = CASE WHEN EXISTS (SELECT 1 FROM ClinicServicePackageTypes t JOIN ClinicServicePackages p ON p.Id = t.ClinicServicePackageId JOIN ClinicServices s ON s.Id = p.ClinicServiceId WHERE s.ClinicId = k.Id AND t.Mode = 37 AND t.Deleted = 0) THEN 1 ELSE 0 END,
    k.ServiceInPerson = CASE WHEN EXISTS (SELECT 1 FROM ClinicServicePackageTypes t JOIN ClinicServicePackages p ON p.Id = t.ClinicServicePackageId JOIN ClinicServices s ON s.Id = p.ClinicServiceId WHERE s.ClinicId = k.Id AND t.Mode = 38 AND t.Deleted = 0) THEN 1 ELSE 0 END,
    k.ServiceAtHome = CASE WHEN EXISTS (SELECT 1 FROM ClinicServicePackageTypes t JOIN ClinicServicePackages p ON p.Id = t.ClinicServicePackageId JOIN ClinicServices s ON s.Id = p.ClinicServiceId WHERE s.ClinicId = k.Id AND t.Mode = 39 AND t.Deleted = 0)
                                OR EXISTS (SELECT 1 FROM ClinicZones z WHERE z.ClinicId = k.Id AND z.Deleted = 0) THEN 1 ELSE 0 END
FROM Clinics k
JOIN #ClinicMap cm ON cm.ClinicId = k.Id;

-- اگر هیچ حالتی ثبت نشده بود، حضوری روشن شود تا کلینیک بی‌رزرو نماند
UPDATE k SET k.ServiceInPerson = 1
FROM Clinics k
JOIN #ClinicMap cm ON cm.ClinicId = k.Id
WHERE k.ServiceOnline = 0 AND k.ServiceInPerson = 0 AND k.ServiceAtHome = 0;

-- ۷) نظرات (TPT: Comments + ClinicComments)
CREATE TABLE #CommentMap (OldId bigint PRIMARY KEY, NewId bigint NOT NULL, ClinicId bigint NOT NULL);

MERGE INTO Comments AS target
USING (
    SELECT cm2.Id AS OldId, cm.ClinicId, cm2.Text, cm2.Rate, cm2.StatusId, cm2.CreateDate, cm2.Answer, cm2.UserId, cm2.LikeCount, cm2.DisLikeCount
    FROM CompanionComments cc
    JOIN Comments cm2 ON cm2.Id = cc.Id
    JOIN #ClinicMap cm ON cm.CompanionId = cc.CompanionId
) AS src ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (Text, Rate, StatusId, CreateDate, Answer, UserId, LikeCount, DisLikeCount)
    VALUES (src.Text, src.Rate, src.StatusId, src.CreateDate, src.Answer, src.UserId, src.LikeCount, src.DisLikeCount)
OUTPUT src.OldId, inserted.Id, src.ClinicId INTO #CommentMap (OldId, NewId, ClinicId);

INSERT INTO ClinicComments (Id, ClinicId)
SELECT NewId, ClinicId FROM #CommentMap;

-- ۸) گزارش
SELECT 'Clinics' AS TableName, COUNT(*) AS Created FROM #ClinicMap
UNION ALL SELECT 'ClinicServices', COUNT(*) FROM #ServiceMap
UNION ALL SELECT 'ClinicServicePackages', COUNT(*) FROM #PackageMap
UNION ALL SELECT 'ClinicComments', COUNT(*) FROM #CommentMap;

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

DROP TABLE IF EXISTS #NewClinics, #ClinicMap, #ServiceMap, #PackageMap, #CommentMap;
