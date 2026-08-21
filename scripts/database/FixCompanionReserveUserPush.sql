SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

UPDATE pattern
SET pattern.Body = N'PushRegisterReserveUser'
FROM dbo.PushPatterns AS pattern
INNER JOIN dbo.PushTypes AS pushType
    ON pushType.Id = pattern.PushTypeId
WHERE pushType.Label = N'PushRegisterReserveUser';

IF @@ROWCOUNT = 0
BEGIN
    THROW 51001, N'الگوی اعلان PushRegisterReserveUser در دیتابیس پیدا نشد.', 1;
END;

COMMIT TRANSACTION;

SELECT
    pattern.Id,
    pushType.Label,
    pattern.Title,
    pattern.Body,
    pattern.IsActive
FROM dbo.PushPatterns AS pattern
INNER JOIN dbo.PushTypes AS pushType
    ON pushType.Id = pattern.PushTypeId
WHERE pushType.Label = N'PushRegisterReserveUser';
GO
