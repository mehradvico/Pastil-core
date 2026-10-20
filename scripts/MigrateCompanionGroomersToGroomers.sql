-- انتقال آرایشگاه‌های فعلی (Companion با نوع «Grooming») به جدول‌های جدید Groomer*.
-- پیش‌نیاز: اسکریپت AddGroomers.sql (مایگریشن AddGroomers) اجرا شده باشد.
--
-- ایمنی:
--  * هیچ ردیف قدیمی پاک یا تغییر داده نمی‌شود؛ فقط ردیف‌های جدید ساخته می‌شود. رزروها، پرداخت‌ها و تسویه‌های قبلی روی Companion باقی می‌مانند.
--  * Idempotent: هر Companion با Groomers.LegacyCompanionId فقط یک‌بار منتقل می‌شود؛ دوباره اجرا کردن چیز تازه‌ای نمی‌سازد.
--  * همه‌چیز در یک تراکنش است. اول با @Commit = 0 اجرا کنید (ROLLBACK)، خروجی شمارش‌ها را ببینید؛ بعد @Commit = 1.
--
-- نکته‌ها:
--  * شناسه‌ی نوع «Grooming» = 45 (CompanionTypeEnum.CompanionType_Grooming). اگر «Barber» (43) هم آرایشگاه حیوان است، 43 را به @TypeIds اضافه کنید.
--  * اگر یک Companion همزمان نوع دیگری (مثلاً کلینیک) هم داشته باشد، کپی آرایشگاهی‌اش ساخته می‌شود و خودِ Companion (بخش کلینیک) سر جایش می‌ماند.
--  * نظرات (CompanionComments) کپی می‌شوند؛ لایک‌های هر نظر (CommentLikes) کپی نمی‌شوند، فقط شمارنده‌ها (LikeCount/DisLikeCount) می‌ماند.
--  * رزروهای قدیمی (CompanionReserves) به GroomerReserve منتقل نمی‌شوند (تاریخچه روی Companion می‌ماند).

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON; -- لازم برای ایندکس‌های فیلتری (Groomers)
SET XACT_ABORT ON;

DECLARE @Commit bit = 0;   -- 0 = فقط بررسی (ROLLBACK)، 1 = ذخیره

DECLARE @TypeIds TABLE (Id bigint PRIMARY KEY);
INSERT INTO @TypeIds (Id) VALUES (45);

BEGIN TRANSACTION;

-- ۱) Companionهای آرایشگاهی که هنوز منتقل نشده‌اند
SELECT c.Id AS CompanionId
INTO #NewGroomers
FROM Companions c
WHERE c.Deleted = 0
  AND EXISTS (SELECT 1 FROM CompanionTypes ct WHERE ct.CompanionId = c.Id AND ct.Deleted = 0 AND ct.TypeId IN (SELECT Id FROM @TypeIds))
  AND NOT EXISTS (SELECT 1 FROM Groomers g WHERE g.LegacyCompanionId = c.Id);

-- ۲) Groomers (Id خودکار؛ نگاشت قدیمی ← جدید با LegacyCompanionId)
INSERT INTO Groomers
    (Name, Summary, Description, ProviderId, OwnerId, LegacyCompanionId, IsPersonal, ReferralCode,
     PictureId, BackgroundPictureId, IconId, Phone, AddressValue, StateId, CityId, NeighborhoodId, Location,
     Active, ShowToSite, Approved, ActivationValue, Suggested, Deleted, DeleteDate, DeletedByUserId, CreateDate,
     CommentCount, RateAvg, RateCount, SearchKey, GoldAccountDate, SilverAccountDate,
     SeoH1, SeoMinDescription, SeoDescription, SeoTitle, SeoPictureAlt, SeoUrlText, SeoNoIndex, SeoNoFollow, SeoCanonical,
     Regulations, EstablishedYear, LicenseNumber, LicenseApproved, InstagramUrl, WebsiteUrl,
     ServiceAtHome, ServiceInSalon, PickupAvailable, PickupFee, HasParking, HasWaitingRoom, HasCamera,
     MaxConcurrentPets, StaffCount, PortfolioCount, PortfolioLikeCount, CommissionPercent)
SELECT
    c.Name, c.Summary, c.Description, c.ProviderId, c.OwnerId, c.Id, c.IsPersonal, c.ReferralCode,
    c.PictureId, c.BackgroundPictureId, c.IconId, c.Phone, c.AddressValue, ci.StateId, c.CityId, c.NeighborhoodId, c.Location,
    c.Active, c.ShowToSite, c.Approved, c.ActivationValue, 0, 0, NULL, NULL, SYSDATETIME(),
    c.CommentCount, c.RateAvg, c.RateCount, c.SearchKey, c.GoldAccountDate, c.SilverAccountDate,
    c.SeoH1, c.SeoMinDescription, c.SeoDescription, c.SeoTitle, c.SeoPictureAlt, c.SeoUrlText, c.SeoNoIndex, c.SeoNoFollow, c.SeoCanonical,
    NULL, NULL, NULL, 0, NULL, NULL,
    0, 1, 0, 0, 0, 0, 0,
    0,
    (SELECT COUNT(*) FROM CompanionUsers cu WHERE cu.CompanionId = c.Id AND cu.Deleted = 0),
    0, 0,
    -- کمیسیون پیش‌فرض: میانگین‌گیری نمی‌کنیم؛ هر خدمت درصد خودش را می‌گیرد (پایین). ۰ یعنی «از خدمت بخوان».
    0
FROM Companions c
JOIN #NewGroomers n ON n.CompanionId = c.Id
LEFT JOIN Cities ci ON ci.Id = c.CityId;

SELECT g.LegacyCompanionId AS CompanionId, g.Id AS GroomerId
INTO #GroomerMap
FROM Groomers g
JOIN #NewGroomers n ON n.CompanionId = g.LegacyCompanionId;

-- ۳) خدمات (CompanionAssistance → GroomerService) با نگاشت شناسه
CREATE TABLE #ServiceMap (OldId bigint PRIMARY KEY, NewId bigint NOT NULL);

MERGE INTO GroomerServices AS target
USING (
    SELECT ca.Id AS OldId, gm.GroomerId, ca.AssistanceId, ca.Active, ca.Approved, ca.ActivationValue, ca.Deleted, ca.CommissionPercent
    FROM CompanionAssistances ca
    JOIN #GroomerMap gm ON gm.CompanionId = ca.CompanionId
) AS src ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (GroomerId, AssistanceId, Active, Approved, ActivationValue, Deleted, CommissionPercent, DurationMinutes, Description)
    VALUES (src.GroomerId, src.AssistanceId, src.Active, src.Approved, src.ActivationValue, src.Deleted, src.CommissionPercent, 0, NULL)
OUTPUT src.OldId, inserted.Id INTO #ServiceMap (OldId, NewId);

-- ۴) بسته‌های قیمتی
CREATE TABLE #PackageMap (OldId bigint PRIMARY KEY, NewId bigint NOT NULL);

MERGE INTO GroomerServicePackages AS target
USING (
    SELECT p.Id AS OldId, sm.NewId AS GroomerServiceId, p.Name, p.Price, p.PrePaymentPrice, p.PictureId, p.Active, p.ActivationValue,
           p.Discription, p.PetSize, p.Deleted
    FROM CompanionAssistancePackages p
    JOIN #ServiceMap sm ON sm.OldId = p.CompanionAssistanceId
) AS src ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (GroomerServiceId, Name, Price, PrePaymentPrice, PictureId, Active, ActivationValue, Discription, PetSize, Deleted)
    VALUES (src.GroomerServiceId, src.Name, src.Price, src.PrePaymentPrice, src.PictureId, src.Active, src.ActivationValue, src.Discription, src.PetSize, src.Deleted)
OUTPUT src.OldId, inserted.Id INTO #PackageMap (OldId, NewId);

INSERT INTO GroomerServicePackagePictures (GroomerServicePackageId, PictureId, Label, Deleted)
SELECT pm.NewId, pp.PictureId, NULL, 0
FROM CompanionAssistancePackagePictures pp
JOIN #PackageMap pm ON pm.OldId = pp.CompanionAssistancePackageId;

-- ۵) گرومرهای مجاز هر خدمت
INSERT INTO GroomerServiceUsers (GroomerServiceId, UserId, Active, ActivationValue, Deleted)
SELECT sm.NewId, au.UserId, au.Active, au.ActivationValue, au.Deleted
FROM CompanionAssistanceUsers au
JOIN #ServiceMap sm ON sm.OldId = au.CompanionAssistanceId;

-- ۶) پرسنل، ساعت کاری، محدوده‌ی پوشش، گونه‌ها
INSERT INTO GroomerUsers (GroomerId, UserId, UserAccept, Active, ActivationValue, Deleted)
SELECT gm.GroomerId, cu.UserId, cu.UserAccept, cu.Active, cu.ActivationValue, cu.Deleted
FROM CompanionUsers cu
JOIN #GroomerMap gm ON gm.CompanionId = cu.CompanionId;

INSERT INTO GroomerTimes (GroomerId, WeekDayId, StartTime, EndTime, Capacity, Active, Deleted)
SELECT gm.GroomerId, t.WeekDayId, t.StartTime, t.EndTime, t.Capacity, t.Active, t.Deleted
FROM CompanionTimes t
JOIN #GroomerMap gm ON gm.CompanionId = t.CompanionId;

INSERT INTO GroomerZones (GroomerId, StateId, CityId, NeighborhoodId, Deleted)
SELECT gm.GroomerId, z.StateId, z.CityId, z.NeighborhoodId, z.Deleted
FROM CompanionZones z
JOIN #GroomerMap gm ON gm.CompanionId = z.CompanionId;

INSERT INTO GroomerPets (GroomerId, PetId, Deleted)
SELECT gm.GroomerId, p.PetId, p.Deleted
FROM CompanionPets p
JOIN #GroomerMap gm ON gm.CompanionId = p.CompanionId;

-- اگر آرایشگاه خدمات در محل داشته باشد (منطقه‌ی پوشش دارد)، پرچم ServiceAtHome روشن شود
UPDATE g SET g.ServiceAtHome = 1
FROM Groomers g
JOIN #GroomerMap gm ON gm.GroomerId = g.Id
WHERE EXISTS (SELECT 1 FROM GroomerZones z WHERE z.GroomerId = g.Id AND z.Deleted = 0);

-- ۷) نظرات (TPT: Comments + GroomerComments)
CREATE TABLE #CommentMap (OldId bigint PRIMARY KEY, NewId bigint NOT NULL, GroomerId bigint NOT NULL);

MERGE INTO Comments AS target
USING (
    SELECT cm.Id AS OldId, gm.GroomerId, cm.Text, cm.Rate, cm.StatusId, cm.CreateDate, cm.Answer, cm.UserId, cm.LikeCount, cm.DisLikeCount
    FROM CompanionComments cc
    JOIN Comments cm ON cm.Id = cc.Id
    JOIN #GroomerMap gm ON gm.CompanionId = cc.CompanionId
) AS src ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (Text, Rate, StatusId, CreateDate, Answer, UserId, LikeCount, DisLikeCount)
    VALUES (src.Text, src.Rate, src.StatusId, src.CreateDate, src.Answer, src.UserId, src.LikeCount, src.DisLikeCount)
OUTPUT src.OldId, inserted.Id, src.GroomerId INTO #CommentMap (OldId, NewId, GroomerId);

INSERT INTO GroomerComments (Id, GroomerId)
SELECT NewId, GroomerId FROM #CommentMap;

-- ۸) گزارش
SELECT 'Groomers' AS TableName, COUNT(*) AS Created FROM #GroomerMap
UNION ALL SELECT 'GroomerServices', COUNT(*) FROM #ServiceMap
UNION ALL SELECT 'GroomerServicePackages', COUNT(*) FROM #PackageMap
UNION ALL SELECT 'GroomerComments', COUNT(*) FROM #CommentMap;

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

DROP TABLE IF EXISTS #NewGroomers, #GroomerMap, #ServiceMap, #PackageMap, #CommentMap;
