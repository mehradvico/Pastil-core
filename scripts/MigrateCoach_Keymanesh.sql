-- انتقال مربی‌های فعلی (Companion) به جدول‌های جدید Coach*.
-- پیش‌نیاز: اسکریپت AddCoaches.sql (مایگریشن AddCoaches) اجرا شده باشد.
--
-- مربی در Companion نوع مخصوص ندارد، پس انتخاب با شناسه‌ی خود شماست:
--   @CompanionIds : شناسه‌ی Companionهایی که باید مربی شوند (مثلاً قادرپناه و کی‌منش)
--   @TypeIds      : اختیاری؛ همه‌ی Companionهایی که این نوع‌ها را دارند (خالی = استفاده نشود)
-- برای پیدا کردن شناسه‌ها:
--   SELECT c.Id, c.Name, c.IsPersonal, c.ProviderId FROM Companions c WHERE c.Deleted = 0 AND c.Name LIKE N'%قادرپناه%'
--   SELECT ct.TypeId, cd.Label, cd.Name, COUNT(*) FROM CompanionTypes ct JOIN Codes cd ON cd.Id = ct.TypeId WHERE ct.Deleted = 0 GROUP BY ct.TypeId, cd.Label, cd.Name
--
-- ایمنی:
--  * هیچ ردیف قدیمی پاک یا تغییر داده نمی‌شود (جز گزینه‌ی اختیاری @DeactivateLegacyServices). رزروها، پرداخت‌ها و تسویه‌های قدیمی روی Companion می‌مانند.
--  * مدرسه (School) و دوره‌ها اصلاً دخیل نیستند و دست نمی‌خورند.
--  * Idempotent: هر Companion با Coaches.LegacyCompanionId فقط یک‌بار منتقل می‌شود.
--  * همه‌چیز در یک تراکنش است. اول با @Commit = 0 اجرا کنید (ROLLBACK)، شمارش‌ها را ببینید؛ بعد 1.
--  * Companion شخصی (IsPersonal = 1) هم قابل انتقال است (مربی‌ها معمولاً شخصی‌اند).
--  * خدمت‌های گرومینگ (Assistances.IsGrooming = 1) منتقل نمی‌شوند.
--  * نحوه‌ی ارائه‌ی هر بسته (آنلاین 37، حضوری 38، در محل 39) مثل مربی منتقل می‌شود؛ بسته‌ی بدون ردیف، با قیمت پایه برای هر حالت خدمت.
--  * @DeactivateLegacyServices = 1: خدمت‌های منتقل‌شده در Companion غیرفعال می‌شوند تا دو بار رزرو نشوند (Companion خودش برای پانسیون/مدرسه می‌ماند).
--  * نظرات کپی می‌شوند؛ لایک نظرها نه.

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON; -- لازم برای ایندکس‌های فیلتری (Coaches)
SET XACT_ABORT ON;

DECLARE @Commit bit = 0;                    -- 0 = فقط بررسی (ROLLBACK)، 1 = ذخیره
DECLARE @DeactivateLegacyServices bit = 1;  -- خدمت‌های Companionِ منتقل‌شده غیرفعال شوند؟

DECLARE @CompanionIds TABLE (Id bigint PRIMARY KEY);
INSERT INTO @CompanionIds (Id) VALUES (11);   -- مهدی کی‌منش

-- 1 = همه‌ی Companionهایی که نوعشان در جدول Codes برچسب CompanionType_Coach دارد (همان که وب‌اپ برای «مربیان» می‌خواند)
DECLARE @IncludeCoachTypeLabel bit = 0;

DECLARE @TypeIds TABLE (Id bigint PRIMARY KEY);
-- INSERT INTO @TypeIds (Id) VALUES (41);                -- ← اختیاری

IF NOT EXISTS (SELECT 1 FROM @CompanionIds) AND NOT EXISTS (SELECT 1 FROM @TypeIds) AND @IncludeCoachTypeLabel = 0
BEGIN
    PRINT N'هیچ شناسه‌ای تعریف نشده؛ هیچ تغییری اعمال نشد. @CompanionIds یا @TypeIds را پر کنید.';
    RETURN;
END

BEGIN TRANSACTION;

-- ۱) Companionهایی که هنوز منتقل نشده‌اند
SELECT c.Id AS CompanionId
INTO #NewCoaches
FROM Companions c
WHERE c.Deleted = 0
  AND (c.Id IN (SELECT Id FROM @CompanionIds)
       OR EXISTS (SELECT 1 FROM CompanionTypes ct WHERE ct.CompanionId = c.Id AND ct.Deleted = 0 AND ct.TypeId IN (SELECT Id FROM @TypeIds))
       OR (@IncludeCoachTypeLabel = 1 AND EXISTS (SELECT 1 FROM CompanionTypes ct JOIN Codes cd ON cd.Id = ct.TypeId WHERE ct.CompanionId = c.Id AND ct.Deleted = 0 AND cd.Label = N'CompanionType_Coach')))
  AND NOT EXISTS (SELECT 1 FROM Coaches k WHERE k.LegacyCompanionId = c.Id);

-- ۲) Coaches (Id خودکار؛ نگاشت قدیمی ← جدید با LegacyCompanionId)
INSERT INTO Coaches
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
JOIN #NewCoaches n ON n.CompanionId = c.Id
LEFT JOIN Cities ci ON ci.Id = c.CityId;

SELECT k.LegacyCompanionId AS CompanionId, k.Id AS CoachId
INTO #CoachMap
FROM Coaches k
JOIN #NewCoaches n ON n.CompanionId = k.LegacyCompanionId;

-- ۳) خدمات (CompanionAssistance → CoachService) با نگاشت شناسه؛ خدمت گرومینگ منتقل نمی‌شود
CREATE TABLE #ServiceMap (OldId bigint PRIMARY KEY, NewId bigint NOT NULL);

MERGE INTO CoachServices AS target
USING (
    SELECT ca.Id AS OldId, cm.CoachId, ca.AssistanceId, ca.Active, ca.Approved, ca.ActivationValue, ca.Deleted, ca.CommissionPercent
    FROM CompanionAssistances ca
    JOIN #CoachMap cm ON cm.CompanionId = ca.CompanionId
    JOIN Assistances a ON a.Id = ca.AssistanceId
    WHERE a.IsGrooming = 0
) AS src ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (CoachId, AssistanceId, Active, Approved, ActivationValue, Deleted, CommissionPercent, DurationMinutes, Description)
    VALUES (src.CoachId, src.AssistanceId, src.Active, src.Approved, src.ActivationValue, src.Deleted, src.CommissionPercent, 0, NULL)
OUTPUT src.OldId, inserted.Id INTO #ServiceMap (OldId, NewId);

-- ۴) بسته‌های قیمتی
CREATE TABLE #PackageMap (OldId bigint PRIMARY KEY, NewId bigint NOT NULL);

MERGE INTO CoachServicePackages AS target
USING (
    SELECT p.Id AS OldId, sm.NewId AS CoachServiceId, p.Name, p.Price, p.PrePaymentPrice, p.PictureId, p.Active, p.ActivationValue,
           p.Discription, p.PetSize, p.Deleted
    FROM CompanionAssistancePackages p
    JOIN #ServiceMap sm ON sm.OldId = p.CompanionAssistanceId
) AS src ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (CoachServiceId, Name, Price, PrePaymentPrice, PictureId, Active, ActivationValue, Discription, PetSize, Deleted)
    VALUES (src.CoachServiceId, src.Name, src.Price, src.PrePaymentPrice, src.PictureId, src.Active, src.ActivationValue, src.Discription, src.PetSize, src.Deleted)
OUTPUT src.OldId, inserted.Id INTO #PackageMap (OldId, NewId);

INSERT INTO CoachServicePackagePictures (CoachServicePackageId, PictureId, Label, Deleted)
SELECT pm.NewId, pp.PictureId, NULL, 0
FROM CompanionAssistancePackagePictures pp
JOIN #PackageMap pm ON pm.OldId = pp.CompanionAssistancePackageId;

-- ۴.۱) نحوه‌ی ارائه‌ی بسته‌ها: ردیف‌های موجود عیناً کپی می‌شود
INSERT INTO CoachServicePackageTypes (CoachServicePackageId, Mode, Price, PrePaymentPrice, Deleted)
SELECT pm.NewId, CAST(pt.CompanionAssistanceTypeId AS int), pt.Price, pt.PrePaymentPrice, 0
FROM CompanionAssistancePackageTypes pt
JOIN #PackageMap pm ON pm.OldId = pt.CompanionAssistancePackageId
WHERE pt.Deleted = 0;

-- بسته‌ی بدون ردیف: برای هر حالتِ خدمت با قیمت پایه (رفتار قدیمی عیناً حفظ می‌شود)
INSERT INTO CoachServicePackageTypes (CoachServicePackageId, Mode, Price, PrePaymentPrice, Deleted)
SELECT pm.NewId, CAST(code.CodesId AS int), p.Price, p.PrePaymentPrice, 0
FROM #PackageMap pm
JOIN CompanionAssistancePackages p ON p.Id = pm.OldId
JOIN CodeCompanionAssistance code ON code.CompanionAssistancesId = p.CompanionAssistanceId
WHERE code.CodesId IN (37, 38, 39)
  AND NOT EXISTS (SELECT 1 FROM CompanionAssistancePackageTypes pt WHERE pt.CompanionAssistancePackageId = p.Id AND pt.Deleted = 0);

-- ۵) پزشک/پرسنلِ مجاز هر خدمت
INSERT INTO CoachServiceUsers (CoachServiceId, UserId, Active, ActivationValue, Deleted)
SELECT sm.NewId, au.UserId, au.Active, au.ActivationValue, au.Deleted
FROM CompanionAssistanceUsers au
JOIN #ServiceMap sm ON sm.OldId = au.CompanionAssistanceId;

-- ۶) پرسنل، ساعت کاری، محدوده‌ی پوشش، گونه‌ها
INSERT INTO CoachUsers (CoachId, UserId, UserAccept, InviteDate, Active, ActivationValue, Deleted)
SELECT cm.CoachId, cu.UserId, cu.UserAccept, NULL, cu.Active, cu.ActivationValue, cu.Deleted
FROM CompanionUsers cu
JOIN #CoachMap cm ON cm.CompanionId = cu.CompanionId;

INSERT INTO CoachTimes (CoachId, WeekDayId, StartTime, EndTime, Capacity, Active, Deleted)
SELECT cm.CoachId, t.WeekDayId, t.StartTime, t.EndTime, t.Capacity, t.Active, t.Deleted
FROM CompanionTimes t
JOIN #CoachMap cm ON cm.CompanionId = t.CompanionId;

-- مربیی که ساعت کاری سطح مکان نداشت: از ساعت‌های سطح خدمت (ظرفیت 1)
INSERT INTO CoachTimes (CoachId, WeekDayId, StartTime, EndTime, Capacity, Active, Deleted)
SELECT DISTINCT cm.CoachId, at.WeekDayId, at.StartTime, at.EndTime, 1, at.Active, 0
FROM CompanionAssistanceTimes at
JOIN CompanionAssistances ca ON ca.Id = at.CompanionAssistanceId
JOIN #CoachMap cm ON cm.CompanionId = ca.CompanionId
WHERE at.Deleted = 0 AND at.Active = 1
  AND NOT EXISTS (SELECT 1 FROM CoachTimes x WHERE x.CoachId = cm.CoachId);

INSERT INTO CoachZones (CoachId, StateId, CityId, NeighborhoodId, Deleted)
SELECT cm.CoachId, z.StateId, z.CityId, z.NeighborhoodId, z.Deleted
FROM CompanionZones z
JOIN #CoachMap cm ON cm.CompanionId = z.CompanionId;

INSERT INTO CoachPets (CoachId, PetId, Deleted)
SELECT cm.CoachId, p.PetId, p.Deleted
FROM CompanionPets p
JOIN #CoachMap cm ON cm.CompanionId = p.CompanionId;

-- پرچم‌های نحوه‌ی ارائه از حالت‌های خدمت‌ها (و منطقه‌ی پوشش)
UPDATE k SET
    k.ServiceOnline = CASE WHEN EXISTS (SELECT 1 FROM CoachServicePackageTypes t JOIN CoachServicePackages p ON p.Id = t.CoachServicePackageId JOIN CoachServices s ON s.Id = p.CoachServiceId WHERE s.CoachId = k.Id AND t.Mode = 37 AND t.Deleted = 0) THEN 1 ELSE 0 END,
    k.ServiceInPerson = CASE WHEN EXISTS (SELECT 1 FROM CoachServicePackageTypes t JOIN CoachServicePackages p ON p.Id = t.CoachServicePackageId JOIN CoachServices s ON s.Id = p.CoachServiceId WHERE s.CoachId = k.Id AND t.Mode = 38 AND t.Deleted = 0) THEN 1 ELSE 0 END,
    k.ServiceAtHome = CASE WHEN EXISTS (SELECT 1 FROM CoachServicePackageTypes t JOIN CoachServicePackages p ON p.Id = t.CoachServicePackageId JOIN CoachServices s ON s.Id = p.CoachServiceId WHERE s.CoachId = k.Id AND t.Mode = 39 AND t.Deleted = 0)
                                OR EXISTS (SELECT 1 FROM CoachZones z WHERE z.CoachId = k.Id AND z.Deleted = 0) THEN 1 ELSE 0 END
FROM Coaches k
JOIN #CoachMap cm ON cm.CoachId = k.Id;

-- اگر هیچ حالتی ثبت نشده بود، حضوری روشن شود تا مربی بی‌رزرو نماند
UPDATE k SET k.ServiceInPerson = 1
FROM Coaches k
JOIN #CoachMap cm ON cm.CoachId = k.Id
WHERE k.ServiceOnline = 0 AND k.ServiceInPerson = 0 AND k.ServiceAtHome = 0;

-- ۷) نظرات (TPT: Comments + CoachComments)
CREATE TABLE #CommentMap (OldId bigint PRIMARY KEY, NewId bigint NOT NULL, CoachId bigint NOT NULL);

MERGE INTO Comments AS target
USING (
    SELECT cm2.Id AS OldId, cm.CoachId, cm2.Text, cm2.Rate, cm2.StatusId, cm2.CreateDate, cm2.Answer, cm2.UserId, cm2.LikeCount, cm2.DisLikeCount
    FROM CompanionComments cc
    JOIN Comments cm2 ON cm2.Id = cc.Id
    JOIN #CoachMap cm ON cm.CompanionId = cc.CompanionId
) AS src ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (Text, Rate, StatusId, CreateDate, Answer, UserId, LikeCount, DisLikeCount)
    VALUES (src.Text, src.Rate, src.StatusId, src.CreateDate, src.Answer, src.UserId, src.LikeCount, src.DisLikeCount)
OUTPUT src.OldId, inserted.Id, src.CoachId INTO #CommentMap (OldId, NewId, CoachId);

INSERT INTO CoachComments (Id, CoachId)
SELECT NewId, CoachId FROM #CommentMap;

-- ۷.۱) غیرفعال‌کردن خدمت‌های منتقل‌شده در Companion (اختیاری)
IF @DeactivateLegacyServices = 1
    UPDATE ca SET ca.Active = 0
    FROM CompanionAssistances ca
    JOIN #CoachMap cm ON cm.CompanionId = ca.CompanionId
    JOIN Assistances a ON a.Id = ca.AssistanceId
    WHERE a.IsGrooming = 0 AND ca.Active = 1;

-- ۸) گزارش
SELECT 'Coaches' AS TableName, COUNT(*) AS Created FROM #CoachMap
UNION ALL SELECT 'CoachServices', COUNT(*) FROM #ServiceMap
UNION ALL SELECT 'CoachServicePackages', COUNT(*) FROM #PackageMap
UNION ALL SELECT 'CoachComments', COUNT(*) FROM #CommentMap;

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

DROP TABLE IF EXISTS #NewCoaches, #CoachMap, #ServiceMap, #PackageMap, #CommentMap;
