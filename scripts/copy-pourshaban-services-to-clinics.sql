-- copy-pourshaban-services-to-clinics.sql
-- کپی «خدمات»، «پکیج‌ها» و «ساعت کاری» کلینیک پورشعبان به کلینیک‌های مقصد (Companion Id = 10018، 10017، 6، 5).
--
-- چه چیزی کپی می‌شود (فقط داده‌ی «کاتالوگ خدمت»، بدون هیچ رزرو/سفارش/پرداخت):
--   ۱. CompanionAssistances            خدمت‌هایی که کلینیک مبدأ ارائه می‌دهد (به‌ازای هر مقصد یک ردیف)
--   ۲. CompanionAssistancePackages     پکیج‌های هر خدمت (نام، قیمت، پیش‌پرداخت، تصویر، سایز پت، توضیحات)
--   ۳. CompanionAssistancePackageOnlineSelections  روش‌های مشاوره‌ی آنلاین هر پکیج (اگر مبدأ داشته باشد)
--   ۴. CompanionTimes                  ساعت کاری در سطح کلینیک (روز هفته، شروع/پایان، ظرفیت نوبت هم‌زمان) —
--                                      همین جدول است که رزرو (CompanionReserveService) برای انتخاب بازه استفاده می‌کند
--   ۵. CompanionAssistanceTimes        ساعت کاریِ اختصاصیِ هر خدمت (اگر مبدأ برای خدمتی جداگانه تعریف کرده باشد)
--
-- چه چیزی کپی «نمی‌شود»:
--   • پرسنل (CompanionAssistanceUsers)، گزارش‌ها، گالری تصاویر پکیج (CompanionAssistancePackagePictures)
--
-- ویژگی‌ها:
--   • idempotent: دوبار اجرا = بار دوم هیچ ردیف تکراری نمی‌سازد
--       - خدمتی که مقصد از قبل (غیرحذف‌شده) با همان AssistanceId دارد دوباره ساخته نمی‌شود، ولی پکیج‌های ناقصش کامل می‌شود
--       - پکیجی که با همان نام + PetSize در همان خدمتِ مقصد هست دوباره ساخته نمی‌شود
--       - بازه‌ی ساعتی که مقصد با همان روز/شروع/پایان دارد دوباره ساخته نمی‌شود
--   • همه‌چیز داخل یک تراکنش. پیش‌فرض @Commit = 0 یعنی «فقط شبیه‌سازی» و ROLLBACK می‌شود؛
--     خروجی شمارش‌ها را ببینید، بعد @Commit را 1 کنید و دوباره اجرا کنید.
--   • اگر مبدأ پیدا نشود/چندتا باشد یا یکی از مقصدها وجود نداشته باشد، بدون تغییر متوقف می‌شود.
--
-- ⚠️ مفروضات (قبل از COMMIT بررسی کنید):
--   ۱. مقادیر Active / Approved / CommissionPercent / CompanionTypeId هر خدمت عیناً از مبدأ کپی می‌شود.
--      اگر کمیسیون یا نوع (CompanionTypeId) کلینیک‌های مقصد فرق دارد، بعد از اجرا از پنل اصلاح کنید.
--   ۲. PictureId پکیج‌ها به همان تصویرِ مشترکِ مبدأ اشاره می‌کند (کپی فایل نمی‌شود).
--   ۳. قیمت‌ها همان قیمت مبدأ است.
--   ۴. «کلینیک پورشعبان» با جست‌وجوی نام پیدا می‌شود؛ اگر نام متفاوت ثبت شده، @SourceCompanionId را دستی بگذارید.
--   ۵. ساعت کاری: اگر مقصد از قبل بازه‌هایی دارد (مثلاً الگوی استانداردِ seed-companion-working-hours.sql)،
--      پیش‌فرض (@ReplaceExistingTimes = 0) بازه‌های پورشعبان را «به‌علاوه‌ی» آن‌ها می‌افزاید که ممکن است بازه‌های
--      هم‌پوشان بسازد. اگر می‌خواهید ساعت کاری مقصد دقیقاً مثل پورشعبان شود، @ReplaceExistingTimes = 1 بگذارید؛
--      آن‌وقت بازه‌های فعالِ قبلیِ مقصد «حذف نرم» می‌شوند (Deleted = 1؛ چیزی واقعاً پاک نمی‌شود و رزروهای قبلی سالم می‌مانند).

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Commit BIT = 0;                         -- 0 = شبیه‌سازی (ROLLBACK)، 1 = ثبت نهایی
DECLARE @SourceCompanionId BIGINT = NULL;        -- NULL = پیدا کردن خودکار با نام «پورشعبان»؛ یا Id را دستی بگذارید
DECLARE @ReplaceExistingTimes BIT = 0;           -- 0 = بازه‌های پورشعبان به موجودها اضافه شود؛ 1 = ساعت کاری فعلیِ مقصد حذف نرم و جایگزین شود

DECLARE @Targets TABLE (Id BIGINT PRIMARY KEY);
INSERT INTO @Targets (Id) VALUES (10018), (10017), (6), (5);

-- ---------- ۱) یافتن کلینیک مبدأ ----------
IF @SourceCompanionId IS NULL
BEGIN
    DECLARE @Candidates TABLE (Id BIGINT, Name NVARCHAR(MAX));
    INSERT INTO @Candidates (Id, Name)
    SELECT c.Id, c.Name
    FROM dbo.Companions c
    WHERE c.Deleted = 0
      AND (REPLACE(REPLACE(REPLACE(c.Name, N'ي', N'ی'), N'ك', N'ک'), NCHAR(8204), N'') LIKE N'%پورشعبان%'
        OR REPLACE(REPLACE(REPLACE(c.Name, N'ي', N'ی'), N'ك', N'ک'), N' ', N'') LIKE N'%پورشعبان%');

    PRINT N'--- کاندیداهای کلینیک مبدأ:';
    SELECT Id, Name FROM @Candidates;

    IF (SELECT COUNT(*) FROM @Candidates) <> 1
        THROW 50001, N'کلینیک مبدأ (پورشعبان) دقیقاً یکی پیدا نشد؛ @SourceCompanionId را دستی مقداردهی کنید.', 1;

    SELECT @SourceCompanionId = Id FROM @Candidates;
END

IF NOT EXISTS (SELECT 1 FROM dbo.Companions WHERE Id = @SourceCompanionId AND Deleted = 0)
    THROW 50002, N'کلینیک مبدأ وجود ندارد یا حذف شده است.', 1;

-- ---------- ۲) اعتبارسنجی مقصدها ----------
IF EXISTS (SELECT 1 FROM @Targets WHERE Id = @SourceCompanionId)
    THROW 50003, N'کلینیک مبدأ نباید جزو مقصدها باشد.', 1;

DECLARE @MissingTargets NVARCHAR(200) =
    (SELECT STRING_AGG(CAST(t.Id AS NVARCHAR(20)), N', ')
     FROM @Targets t
     WHERE NOT EXISTS (SELECT 1 FROM dbo.Companions c WHERE c.Id = t.Id AND c.Deleted = 0));
IF @MissingTargets IS NOT NULL
BEGIN
    DECLARE @Msg NVARCHAR(400) = N'کلینیک(های) مقصد وجود ندارند یا حذف شده‌اند: ' + @MissingTargets;
    THROW 50004, @Msg, 1;
END

PRINT N'--- مبدأ و مقصدها:';
SELECT c.Id, c.Name, CASE WHEN c.Id = @SourceCompanionId THEN N'مبدأ' ELSE N'مقصد' END AS Role
FROM dbo.Companions c
WHERE c.Id = @SourceCompanionId OR c.Id IN (SELECT Id FROM @Targets)
ORDER BY CASE WHEN c.Id = @SourceCompanionId THEN 0 ELSE 1 END, c.Id;

PRINT N'--- خدمت‌ها و تعداد پکیج‌های مبدأ:';
SELECT ca.Id AS AssistanceRowId, ca.AssistanceId, a.Name AS AssistanceName, ca.Active, ca.Approved, ca.CommissionPercent,
       (SELECT COUNT(*) FROM dbo.CompanionAssistancePackages p WHERE p.CompanionAssistanceId = ca.Id AND p.Deleted = 0) AS PackageCount
FROM dbo.CompanionAssistances ca
LEFT JOIN dbo.Assistances a ON a.Id = ca.AssistanceId
WHERE ca.CompanionId = @SourceCompanionId AND ca.Deleted = 0;

PRINT N'--- ساعت کاری مبدأ (CompanionTimes) و ساعت کاری فعلی هر مقصد:';
SELECT (SELECT COUNT(*) FROM dbo.CompanionTimes WHERE CompanionId = @SourceCompanionId AND Deleted = 0) AS SourceCompanionTimes,
       (SELECT COUNT(*) FROM dbo.CompanionAssistanceTimes at2
          JOIN dbo.CompanionAssistances ca2 ON ca2.Id = at2.CompanionAssistanceId
          WHERE ca2.CompanionId = @SourceCompanionId AND at2.Deleted = 0) AS SourceAssistanceTimes;
SELECT t.Id AS TargetCompanionId,
       (SELECT COUNT(*) FROM dbo.CompanionTimes ct WHERE ct.CompanionId = t.Id AND ct.Deleted = 0) AS ExistingCompanionTimes
FROM @Targets t
ORDER BY t.Id;

IF NOT EXISTS (SELECT 1 FROM dbo.CompanionAssistances WHERE CompanionId = @SourceCompanionId AND Deleted = 0)
    THROW 50005, N'کلینیک مبدأ هیچ خدمت فعالی (غیرحذف‌شده) ندارد؛ چیزی برای کپی نیست.', 1;

BEGIN TRANSACTION;

-- ---------- ۳) خدمت‌ها ----------
INSERT INTO dbo.CompanionAssistances
    (CompanionId, AssistanceId, IsSinglePackage, Active, ActivationValue, CompanionTypeId, Approved, Deleted, CommissionPercent)
SELECT t.Id, s.AssistanceId, s.IsSinglePackage, s.Active, s.ActivationValue, s.CompanionTypeId, s.Approved, 0, s.CommissionPercent
FROM dbo.CompanionAssistances s
CROSS JOIN @Targets t
WHERE s.CompanionId = @SourceCompanionId
  AND s.Deleted = 0
  AND NOT EXISTS (SELECT 1 FROM dbo.CompanionAssistances x
                  WHERE x.CompanionId = t.Id AND x.AssistanceId = s.AssistanceId AND x.Deleted = 0);
DECLARE @AssistancesInserted INT = @@ROWCOUNT;

-- نگاشت «خدمت مبدأ → خدمت مقصد» (اگر مقصد چند ردیف هم‌خدمت دارد، کوچک‌ترین Id)
SELECT s.Id AS SrcAssistanceRowId, t.Id AS TargetCompanionId, MIN(x.Id) AS TargetAssistanceRowId
INTO #AssistanceMap
FROM dbo.CompanionAssistances s
CROSS JOIN @Targets t
JOIN dbo.CompanionAssistances x
  ON x.CompanionId = t.Id AND x.AssistanceId = s.AssistanceId AND x.Deleted = 0
WHERE s.CompanionId = @SourceCompanionId AND s.Deleted = 0
GROUP BY s.Id, t.Id;

-- ---------- ۴) پکیج‌ها ----------
INSERT INTO dbo.CompanionAssistancePackages
    (Name, Price, PrePaymentPrice, PictureId, Active, ActivationValue, CompanionAssistanceId, Discription, PetSize, Deleted)
SELECT p.Name, p.Price, p.PrePaymentPrice, p.PictureId, p.Active, p.ActivationValue, m.TargetAssistanceRowId, p.Discription, p.PetSize, 0
FROM dbo.CompanionAssistancePackages p
JOIN #AssistanceMap m ON m.SrcAssistanceRowId = p.CompanionAssistanceId
WHERE p.Deleted = 0
  AND NOT EXISTS (SELECT 1 FROM dbo.CompanionAssistancePackages x
                  WHERE x.CompanionAssistanceId = m.TargetAssistanceRowId
                    AND x.Deleted = 0
                    AND x.Name = p.Name
                    AND ISNULL(x.PetSize, N'') = ISNULL(p.PetSize, N''));
DECLARE @PackagesInserted INT = @@ROWCOUNT;

-- نگاشت «پکیج مبدأ → پکیج مقصد» (با نام + PetSize داخل همان خدمتِ مقصد)
SELECT p.Id AS SrcPackageId, tp.TargetPackageId
INTO #PackageMap
FROM dbo.CompanionAssistancePackages p
JOIN #AssistanceMap m ON m.SrcAssistanceRowId = p.CompanionAssistanceId
CROSS APPLY (SELECT MIN(x.Id) AS TargetPackageId
             FROM dbo.CompanionAssistancePackages x
             WHERE x.CompanionAssistanceId = m.TargetAssistanceRowId
               AND x.Deleted = 0
               AND x.Name = p.Name
               AND ISNULL(x.PetSize, N'') = ISNULL(p.PetSize, N'')) tp
WHERE p.Deleted = 0 AND tp.TargetPackageId IS NOT NULL;

-- ---------- ۵) روش‌های مشاوره‌ی آنلاین پکیج‌ها ----------
INSERT INTO dbo.CompanionAssistancePackageOnlineSelections
    (CompanionAssistancePackageId, CompanionAssistancePackageOnlineId, Price, Active, ActivationValue, Deleted)
SELECT pm.TargetPackageId, o.CompanionAssistancePackageOnlineId, o.Price, o.Active, o.ActivationValue, 0
FROM dbo.CompanionAssistancePackageOnlineSelections o
JOIN #PackageMap pm ON pm.SrcPackageId = o.CompanionAssistancePackageId
WHERE o.Deleted = 0
  AND NOT EXISTS (SELECT 1 FROM dbo.CompanionAssistancePackageOnlineSelections x
                  WHERE x.CompanionAssistancePackageId = pm.TargetPackageId
                    AND x.CompanionAssistancePackageOnlineId = o.CompanionAssistancePackageOnlineId
                    AND x.Deleted = 0);
DECLARE @OnlineSelectionsInserted INT = @@ROWCOUNT;

-- ---------- ۶) ساعت کاری کلینیک (CompanionTimes) ----------
DECLARE @CompanionTimesReplaced INT = 0;
IF @ReplaceExistingTimes = 1
BEGIN
    UPDATE dbo.CompanionTimes
    SET Deleted = 1
    WHERE CompanionId IN (SELECT Id FROM @Targets) AND Deleted = 0;
    SET @CompanionTimesReplaced = @@ROWCOUNT;
END

INSERT INTO dbo.CompanionTimes (StartTime, EndTime, Capacity, Active, Deleted, WeekDayId, CompanionId)
SELECT s.StartTime, s.EndTime, s.Capacity, s.Active, 0, s.WeekDayId, t.Id
FROM dbo.CompanionTimes s
CROSS JOIN @Targets t
WHERE s.CompanionId = @SourceCompanionId
  AND s.Deleted = 0
  AND NOT EXISTS (SELECT 1 FROM dbo.CompanionTimes x
                  WHERE x.CompanionId = t.Id AND x.Deleted = 0
                    AND x.WeekDayId = s.WeekDayId AND x.StartTime = s.StartTime AND x.EndTime = s.EndTime);
DECLARE @CompanionTimesInserted INT = @@ROWCOUNT;

-- ---------- ۷) ساعت کاریِ اختصاصی هر خدمت (CompanionAssistanceTimes) ----------
IF @ReplaceExistingTimes = 1
BEGIN
    UPDATE dbo.CompanionAssistanceTimes
    SET Deleted = 1
    WHERE Deleted = 0
      AND CompanionAssistanceId IN (SELECT TargetAssistanceRowId FROM #AssistanceMap);
END

INSERT INTO dbo.CompanionAssistanceTimes (StartTime, EndTime, Active, Deleted, WeekDayId, CompanionAssistanceId)
SELECT s.StartTime, s.EndTime, s.Active, 0, s.WeekDayId, m.TargetAssistanceRowId
FROM dbo.CompanionAssistanceTimes s
JOIN #AssistanceMap m ON m.SrcAssistanceRowId = s.CompanionAssistanceId
WHERE s.Deleted = 0
  AND NOT EXISTS (SELECT 1 FROM dbo.CompanionAssistanceTimes x
                  WHERE x.CompanionAssistanceId = m.TargetAssistanceRowId AND x.Deleted = 0
                    AND x.WeekDayId = s.WeekDayId AND x.StartTime = s.StartTime AND x.EndTime = s.EndTime);
DECLARE @AssistanceTimesInserted INT = @@ROWCOUNT;

-- ---------- ۸) گزارش ----------
PRINT N'--- نتیجه:';
SELECT @AssistancesInserted AS AssistancesInserted,
       @PackagesInserted AS PackagesInserted,
       @OnlineSelectionsInserted AS OnlineSelectionsInserted,
       @CompanionTimesReplaced AS CompanionTimesSoftDeleted,
       @CompanionTimesInserted AS CompanionTimesInserted,
       @AssistanceTimesInserted AS AssistanceTimesInserted;

PRINT N'--- خلاصه‌ی هر مقصد بعد از کپی (خدمت‌ها / پکیج‌های فعال / بازه‌های ساعت کاری):';
SELECT t.Id AS TargetCompanionId, c.Name,
       (SELECT COUNT(*) FROM dbo.CompanionTimes ct WHERE ct.CompanionId = t.Id AND ct.Deleted = 0) AS WorkingHourSlots,
       (SELECT COUNT(*) FROM dbo.CompanionAssistances ca WHERE ca.CompanionId = t.Id AND ca.Deleted = 0) AS Assistances,
       (SELECT COUNT(*) FROM dbo.CompanionAssistancePackages p
          JOIN dbo.CompanionAssistances ca ON ca.Id = p.CompanionAssistanceId
          WHERE ca.CompanionId = t.Id AND ca.Deleted = 0 AND p.Deleted = 0) AS Packages
FROM @Targets t
JOIN dbo.Companions c ON c.Id = t.Id
ORDER BY t.Id;

DROP TABLE #PackageMap;
DROP TABLE #AssistanceMap;

IF @Commit = 1
BEGIN
    COMMIT TRANSACTION;
    PRINT N'✅ COMMIT شد.';
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT N'ℹ️ شبیه‌سازی بود و ROLLBACK شد. اگر اعداد درست بود، @Commit را 1 کنید و دوباره اجرا کنید.';
END
