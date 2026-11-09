-- مرحله‌ی دوم ادغام هپت (۲۰۲۶-۱۱-۰۹): مرکز اصلی ردیف ۱۱ است (لوگو و نمایش در سایت).
-- داده‌های واقعی ردیف ۲۲ (آدرس، نقشه، ساعت، خدمات، قیمت پت‌ها، گالری) به ۱۱ منتقل و ۲۲ حذف نرم می‌شود.
-- پیش‌فرض @Commit = 0 است: اجرا، نمایش و ROLLBACK.
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Commit bit = 0;
DECLARE @MainId bigint = 11;
DECLARE @SourceId bigint = 22;

BEGIN TRANSACTION;

IF (SELECT COUNT(*) FROM Pansions WHERE Id IN (@MainId, @SourceId) AND Deleted = 0) <> 2
    THROW 50101, N'هر دو ردیف باید موجود و حذف‌نشده باشند.', 1;

IF (SELECT COUNT(DISTINCT ProviderId) FROM Pansions WHERE Id IN (@MainId, @SourceId)) <> 1
    THROW 50102, N'مالک دو ردیف یکسان نیست.', 1;

IF EXISTS (SELECT 1 FROM PansionReserves WHERE PansionId IN (@MainId, @SourceId))
   OR EXISTS (SELECT 1 FROM PansionComments WHERE PansionId IN (@MainId, @SourceId))
   OR EXISTS (SELECT 1 FROM StoryItems WHERE PansionId IN (@MainId, @SourceId))
    THROW 50103, N'یکی از ردیف‌ها رزرو، نظر یا استوری دارد.', 1;

IF EXISTS (SELECT 1 FROM PansionPets WHERE PansionId = @MainId AND Deleted = 0)
   OR EXISTS (SELECT 1 FROM PansionPictures WHERE PansionId = @MainId AND Deleted = 0)
    THROW 50104, N'مرکز اصلی از قبل پت یا گالری دارد.', 1;

UPDATE main
SET main.Name = N'پانسیون هپت',
    main.IsDaycare = src.IsDaycare,
    main.StateId = src.StateId,
    main.CityId = src.CityId,
    main.AddressValue = src.AddressValue,
    main.Location = src.Location,
    main.OpenHour = src.OpenHour,
    main.CloseHour = src.CloseHour,
    main.PansionPrice = src.PansionPrice,
    main.SchoolPrice = src.SchoolPrice,
    main.DailyCommissionPercent = src.DailyCommissionPercent,
    main.HourlyCommissionPercent = src.HourlyCommissionPercent,
    main.Discription = COALESCE(NULLIF(LTRIM(RTRIM(src.Discription)), N''), main.Discription),
    main.Regulations = COALESCE(NULLIF(LTRIM(RTRIM(src.Regulations)), N''), main.Regulations)
FROM Pansions main
CROSS JOIN Pansions src
WHERE main.Id = @MainId AND src.Id = @SourceId;

UPDATE PansionPets SET PansionId = @MainId WHERE PansionId = @SourceId AND Deleted = 0;
UPDATE PansionPictures SET PansionId = @MainId WHERE PansionId = @SourceId AND Deleted = 0;

-- عکس اصلی ردیف ۲۲ هم در گالری می‌ماند؛ لوگوی ردیف ۱۱ عکس اصلی است.
INSERT INTO PansionPictures (PansionId, PictureId, Label, Deleted)
SELECT @MainId, s.PictureId, NULL, 0
FROM Pansions s
WHERE s.Id = @SourceId AND s.PictureId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM PansionPictures x WHERE x.PansionId = @MainId AND x.PictureId = s.PictureId AND x.Deleted = 0);

UPDATE Pansions SET Deleted = 1, DeleteDate = GETDATE(), Active = 0, ShowToSite = 0 WHERE Id = @SourceId;

SELECT Id, Name, IsDaycare, Active, Approve, ShowToSite, Deleted, PansionPrice, SchoolPrice, OpenHour, CloseHour, PictureId, AddressValue
FROM Pansions WHERE Id IN (@MainId, @SourceId);

SELECT pp.Id, pet.Name AS Pet, pp.PetSize, pp.PansionPrice, pp.SchoolPrice
FROM PansionPets pp JOIN Pets pet ON pet.Id = pp.PetId
WHERE pp.PansionId = @MainId AND pp.Deleted = 0;

SELECT COUNT(*) AS GalleryPictures FROM PansionPictures WHERE PansionId = @MainId AND Deleted = 0;

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
