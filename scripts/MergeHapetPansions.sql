-- ادغام یک‌باره‌ی ۶ ردیف پانسیون هپت (شناسه‌های ۲۰ تا ۲۵) در یک مرکز «پانسیون هپت» (۲۰۲۶-۱۱-۰۹).
-- ردیف ۲۲ می‌ماند: هر دو خدمت (IsDaycare = NULL) و قیمت هر پت/سایز روی PansionPets. بقیه حذف نرم می‌شوند.
-- پیش‌نیاز: PansionPetRates.sql. پیش‌فرض @Commit = 0 است: همه‌چیز اجرا و نمایش داده می‌شود و بعد ROLLBACK.
-- اگر ردیفی رزرو، نظر یا استوری داشته باشد یا داده‌ها با فرض‌ها نخواند، اسکریپت بدون هیچ تغییری متوقف می‌شود.
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Commit bit = 0;
DECLARE @KeepId bigint = 22;
DECLARE @CenterName nvarchar(200) = N'پانسیون هپت';

DECLARE @Map TABLE (Id bigint PRIMARY KEY, PetKind varchar(3) NOT NULL, PetSize nvarchar(16) NULL);
INSERT INTO @Map VALUES (20, 'dog', N'Small'), (21, 'dog', N'Small'), (22, 'dog', N'Large'), (23, 'dog', N'Large'), (24, 'cat', NULL), (25, 'cat', NULL);

-- وضعیت فعلی (برای برگرداندن دستی هم کافی است)
SELECT p.Id, p.Name, p.IsDaycare, p.IsInfectious, p.Active, p.Approve, p.Deleted, p.PansionPrice, p.SchoolPrice,
       p.DailyCommissionPercent, p.HourlyCommissionPercent, p.ProviderId, p.CompanionId, p.PictureId,
       (SELECT COUNT(*) FROM PansionReserves r WHERE r.PansionId = p.Id) AS Reserves,
       (SELECT COUNT(*) FROM PansionComments c WHERE c.PansionId = p.Id) AS Comments,
       (SELECT COUNT(*) FROM StoryItems s WHERE s.PansionId = p.Id) AS Stories,
       (SELECT COUNT(*) FROM PansionPets x WHERE x.PansionId = p.Id AND x.Deleted = 0) AS Pets,
       p.AddressValue
FROM Pansions p
JOIN @Map m ON m.Id = p.Id
ORDER BY p.Id DESC;

BEGIN TRANSACTION;

IF (SELECT COUNT(*) FROM Pansions p JOIN @Map m ON m.Id = p.Id WHERE p.Deleted = 0) <> 6
    THROW 50001, N'همه‌ی ۶ ردیف باید موجود و حذف‌نشده باشند.', 1;

IF (SELECT COUNT(*) FROM (SELECT DISTINCT ISNULL(p.ProviderId, -1) AS P, ISNULL(p.CompanionId, -1) AS C FROM Pansions p JOIN @Map m ON m.Id = p.Id) o) <> 1
    THROW 50002, N'مالک ۶ ردیف یکسان نیست.', 1;

IF EXISTS (
    SELECT 1 FROM Pansions p JOIN @Map m ON m.Id = p.Id
    CROSS APPLY (SELECT REPLACE(REPLACE(p.Name, NCHAR(1603), NCHAR(1705)), NCHAR(1610), NCHAR(1740)) AS N) n
    WHERE (m.PetKind = 'cat' AND n.N NOT LIKE N'%گربه%')
       OR (m.PetSize = N'Large' AND n.N NOT LIKE N'%بزرگ%')
       OR (m.PetSize = N'Small' AND n.N NOT LIKE N'%کوچک%'))
    THROW 50003, N'نام ردیف‌ها با نقشه‌ی پت و سایز نمی‌خواند.', 1;

IF EXISTS (SELECT 1 FROM Pansions p JOIN @Map m ON m.Id = p.Id WHERE p.IsInfectious = 1 OR p.IsDaycare IS NULL)
    THROW 50004, N'ردیف عفونی یا بدون نوع وجود دارد.', 1;

IF NOT EXISTS (SELECT 1 FROM Pansions WHERE Id = @KeepId AND Active = 1 AND Approve = 1)
    THROW 50005, N'ردیف اصلی باید فعال و تأییدشده باشد.', 1;

IF EXISTS (SELECT 1 FROM PansionReserves WHERE PansionId IN (SELECT Id FROM @Map WHERE Id <> @KeepId))
   OR EXISTS (SELECT 1 FROM PansionComments WHERE PansionId IN (SELECT Id FROM @Map WHERE Id <> @KeepId))
   OR EXISTS (SELECT 1 FROM StoryItems WHERE PansionId IN (SELECT Id FROM @Map WHERE Id <> @KeepId))
    THROW 50006, N'ردیف‌های حذف‌شونده رزرو، نظر یا استوری دارند.', 1;

DECLARE @DogId bigint, @CatId bigint;
IF (SELECT COUNT(*) FROM Pets WHERE Deleted = 0 AND Name LIKE N'%سگ%') <> 1
   OR (SELECT COUNT(*) FROM Pets WHERE Deleted = 0 AND Name LIKE N'%گربه%') <> 1
    THROW 50007, N'سگ یا گربه در جدول Pets یکتا نیست.', 1;
SELECT @DogId = Id FROM Pets WHERE Deleted = 0 AND Name LIKE N'%سگ%';
SELECT @CatId = Id FROM Pets WHERE Deleted = 0 AND Name LIKE N'%گربه%';

DECLARE @Rates TABLE (PetKind varchar(3), PetSize nvarchar(16), NightMin float, NightMax float, HourMin float, HourMax float);
INSERT INTO @Rates
SELECT m.PetKind, m.PetSize,
       MIN(CASE WHEN p.IsDaycare = 0 THEN NULLIF(p.PansionPrice, 0) END), MAX(CASE WHEN p.IsDaycare = 0 THEN NULLIF(p.PansionPrice, 0) END),
       MIN(CASE WHEN p.IsDaycare = 1 THEN NULLIF(p.SchoolPrice, 0) END), MAX(CASE WHEN p.IsDaycare = 1 THEN NULLIF(p.SchoolPrice, 0) END)
FROM @Map m JOIN Pansions p ON p.Id = m.Id
GROUP BY m.PetKind, m.PetSize;

IF EXISTS (SELECT 1 FROM @Rates WHERE NightMin <> NightMax OR HourMin <> HourMax)
    THROW 50008, N'دو ردیف یک پت و یک خدمت، قیمت متفاوت دارند.', 1;

-- قیمت پایه برای حالتی است که پت/سایز، قیمت همان خدمت را ندارد.
DECLARE @BaseNight float = (SELECT MIN(NightMin) FROM @Rates);
DECLARE @BaseHour float = (SELECT MIN(HourMin) FROM @Rates);
IF @BaseNight IS NULL OR @BaseHour IS NULL
    THROW 50009, N'قیمت پانسیون یا مهد در ردیف‌ها پیدا نشد.', 1;

IF (SELECT COUNT(DISTINCT NULLIF(p.DailyCommissionPercent, 0)) FROM Pansions p JOIN @Map m ON m.Id = p.Id WHERE p.IsDaycare = 0) > 1
   OR (SELECT COUNT(DISTINCT NULLIF(p.HourlyCommissionPercent, 0)) FROM Pansions p JOIN @Map m ON m.Id = p.Id WHERE p.IsDaycare = 1) > 1
    THROW 50010, N'کمیسیون ردیف‌ها یکسان نیست.', 1;

DECLARE @Daily decimal(18, 2) = (SELECT MAX(p.DailyCommissionPercent) FROM Pansions p JOIN @Map m ON m.Id = p.Id WHERE p.IsDaycare = 0);
DECLARE @Hourly decimal(18, 2) = (SELECT MAX(p.HourlyCommissionPercent) FROM Pansions p JOIN @Map m ON m.Id = p.Id WHERE p.IsDaycare = 1);
DECLARE @KeepPicture bigint = (SELECT PictureId FROM Pansions WHERE Id = @KeepId);

-- گالری: عکس اصلی و گالری ردیف‌های دیگر به مرکز می‌آید.
INSERT INTO PansionPictures (PansionId, PictureId, Label, Deleted)
SELECT @KeepId, s.PictureId, NULL, 0
FROM (
    SELECT PictureId FROM PansionPictures WHERE PansionId IN (SELECT Id FROM @Map WHERE Id <> @KeepId) AND Deleted = 0
    UNION
    SELECT PictureId FROM Pansions WHERE Id IN (SELECT Id FROM @Map WHERE Id <> @KeepId) AND PictureId IS NOT NULL
) s
WHERE s.PictureId <> ISNULL(@KeepPicture, 0)
  AND NOT EXISTS (SELECT 1 FROM PansionPictures x WHERE x.PansionId = @KeepId AND x.PictureId = s.PictureId AND x.Deleted = 0);

UPDATE PansionPets SET Deleted = 1 WHERE PansionId = @KeepId AND Deleted = 0;

INSERT INTO PansionPets (PansionId, PetId, PetSize, PansionPrice, SchoolPrice, Deleted)
SELECT @KeepId, CASE r.PetKind WHEN 'dog' THEN @DogId ELSE @CatId END, r.PetSize, r.NightMin, r.HourMin, 0
FROM @Rates r;

UPDATE Pansions
SET Name = @CenterName, IsDaycare = NULL, PansionPrice = @BaseNight, SchoolPrice = @BaseHour,
    DailyCommissionPercent = @Daily, HourlyCommissionPercent = @Hourly
WHERE Id = @KeepId;

UPDATE Pansions
SET Deleted = 1, DeleteDate = GETDATE(), Active = 0, ShowToSite = 0
WHERE Id IN (SELECT Id FROM @Map WHERE Id <> @KeepId);

-- نتیجه
SELECT Id, Name, IsDaycare, Active, Approve, Deleted, PansionPrice, SchoolPrice, DailyCommissionPercent, HourlyCommissionPercent
FROM Pansions WHERE Id IN (SELECT Id FROM @Map) ORDER BY Id DESC;

SELECT pp.Id, pet.Name AS Pet, pp.PetSize, pp.PansionPrice, pp.SchoolPrice
FROM PansionPets pp JOIN Pets pet ON pet.Id = pp.PetId
WHERE pp.PansionId = @KeepId AND pp.Deleted = 0;

SELECT COUNT(*) AS GalleryPictures FROM PansionPictures WHERE PansionId = @KeepId AND Deleted = 0;

IF @Commit = 1
BEGIN
    COMMIT;
    SELECT N'COMMITTED' AS Result;
END
ELSE
BEGIN
    ROLLBACK;
    SELECT N'PREVIEW ONLY: ROLLED BACK' AS Result;
END
