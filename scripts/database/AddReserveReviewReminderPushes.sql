SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF EXISTS (SELECT 1 FROM dbo.PushTypes WHERE Id IN (46, 47) AND Label NOT IN (N'PushCompanionReserveReviewReminder', N'PushPansionReserveReviewReminder'))
    THROW 51001, N'شناسه‌های PushType 46 یا 47 قبلاً برای اعلان دیگری استفاده شده‌اند.', 1;

IF EXISTS (SELECT 1 FROM dbo.PushTypes WHERE Label = N'PushCompanionReserveReviewReminder' AND Id <> 46)
    THROW 51002, N'PushCompanionReserveReviewReminder با شناسه دیگری ثبت شده است.', 1;

IF EXISTS (SELECT 1 FROM dbo.PushTypes WHERE Label = N'PushPansionReserveReviewReminder' AND Id <> 47)
    THROW 51003, N'PushPansionReserveReviewReminder با شناسه دیگری ثبت شده است.', 1;

SET IDENTITY_INSERT dbo.PushTypes ON;

IF NOT EXISTS (SELECT 1 FROM dbo.PushTypes WHERE Id = 46)
    INSERT INTO dbo.PushTypes (Id, Name, Label)
    VALUES (46, N'یادآوری ثبت نظر رزرو خدمات', N'PushCompanionReserveReviewReminder');

IF NOT EXISTS (SELECT 1 FROM dbo.PushTypes WHERE Id = 47)
    INSERT INTO dbo.PushTypes (Id, Name, Label)
    VALUES (47, N'یادآوری ثبت نظر رزرو پانسیون', N'PushPansionReserveReviewReminder');

SET IDENTITY_INSERT dbo.PushTypes OFF;

DECLARE @Patterns TABLE
(
    PushTypeId bigint NOT NULL PRIMARY KEY,
    Body nvarchar(max) NOT NULL,
    Url nvarchar(500) NOT NULL,
    Tag nvarchar(200) NOT NULL
);

INSERT INTO @Patterns (PushTypeId, Body, Url, Tag)
VALUES
    (46, N'PushCompanionReserveReviewReminder', N'/reserve?reviewType=companion&reserveId={3}', N'companion-reserve-review'),
    (47, N'PushPansionReserveReviewReminder', N'/reserve?reviewType=pansion&reserveId={2}', N'pansion-reserve-review');

INSERT INTO dbo.PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
SELECT source.PushTypeId, N'Push_Title', source.Body, source.Url, NULL, source.Tag, 1
FROM @Patterns AS source
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.PushPatterns AS target
    WHERE target.PushTypeId = source.PushTypeId
);

UPDATE pattern
SET
    pattern.Title = N'Push_Title',
    pattern.Body = source.Body,
    pattern.Url = source.Url,
    pattern.Tag = source.Tag,
    pattern.IsActive = 1
FROM dbo.PushPatterns AS pattern
INNER JOIN @Patterns AS source ON source.PushTypeId = pattern.PushTypeId;

INSERT INTO dbo.PushSettings (PushPatternId, IsEnabled)
SELECT pattern.Id, 1
FROM dbo.PushPatterns AS pattern
INNER JOIN @Patterns AS source ON source.PushTypeId = pattern.PushTypeId
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.PushSettings AS setting
    WHERE setting.PushPatternId = pattern.Id
);

COMMIT TRANSACTION;

SELECT pushType.Id, pushType.Label, pattern.Body, pattern.Url, pattern.IsActive, setting.IsEnabled
FROM dbo.PushTypes AS pushType
INNER JOIN dbo.PushPatterns AS pattern ON pattern.PushTypeId = pushType.Id
LEFT JOIN dbo.PushSettings AS setting ON setting.PushPatternId = pattern.Id
WHERE pushType.Id IN (46, 47)
ORDER BY pushType.Id;
GO
