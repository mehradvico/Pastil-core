SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF EXISTS (SELECT 1 FROM dbo.PushTypes WHERE Id = 48 AND Label <> N'PushProductStockAvailable')
    THROW 51011, N'شناسه PushType شماره ۴۸ قبلاً برای اعلان دیگری استفاده شده است.', 1;

IF EXISTS (SELECT 1 FROM dbo.PushTypes WHERE Label = N'PushProductStockAvailable' AND Id <> 48)
    THROW 51012, N'PushProductStockAvailable با شناسه دیگری ثبت شده است.', 1;

SET IDENTITY_INSERT dbo.PushTypes ON;

IF NOT EXISTS (SELECT 1 FROM dbo.PushTypes WHERE Id = 48)
    INSERT INTO dbo.PushTypes (Id, Name, Label)
    VALUES (48, N'موجودشدن کالای مورد انتظار', N'PushProductStockAvailable');

SET IDENTITY_INSERT dbo.PushTypes OFF;

IF NOT EXISTS (SELECT 1 FROM dbo.PushPatterns WHERE PushTypeId = 48)
    INSERT INTO dbo.PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
    VALUES
    (48, N'Push_Title', N'PushProductStockAvailable', N'/product/{4}', NULL, N'product-stock-available', 1);
ELSE
    UPDATE dbo.PushPatterns
    SET Title = N'Push_Title',
        Body = N'PushProductStockAvailable',
        Url = N'/product/{4}',
        Tag = N'product-stock-available',
        IsActive = 1
    WHERE PushTypeId = 48;

INSERT INTO dbo.PushSettings (PushPatternId, IsEnabled)
SELECT pattern.Id, 1
FROM dbo.PushPatterns AS pattern
WHERE pattern.PushTypeId = 48
  AND NOT EXISTS
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
WHERE pushType.Id = 48;
GO
