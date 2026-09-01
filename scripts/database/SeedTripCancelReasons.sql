-- دلایل لغو سفر پت‌رسان — یک گروه برای راننده و یک گروه برای کاربر (جدا از هم، هرکدام در پنل ادمین
-- از مسیرهای عمومی Code/CodeGroup قابل مدیریت هستند: /admin/codegroup و /admin/code).
-- این اسکریپت idempotent است — اجرای چندباره‌اش خطا یا رکورد تکراری نمی‌سازد.

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @ExpectedGroups TABLE
(
    GroupLabel nvarchar(100) NOT NULL,
    GroupName  nvarchar(200) NOT NULL
);

INSERT INTO @ExpectedGroups (GroupLabel, GroupName)
VALUES
    (N'TripCancelReason_Driver', N'دلیل لغو سفر توسط راننده'),
    (N'TripCancelReason_User',   N'دلیل لغو سفر توسط کاربر');

MERGE INTO dbo.CodeGroups AS target
USING @ExpectedGroups AS source
ON target.Label = source.GroupLabel
WHEN MATCHED THEN
    UPDATE SET target.Name = source.GroupName
WHEN NOT MATCHED THEN
    INSERT (Name, Label)
    VALUES (source.GroupName, source.GroupLabel);

DECLARE @ExpectedCodes TABLE
(
    GroupLabel nvarchar(100) NOT NULL,
    CodeLabel  nvarchar(150) NOT NULL,
    CodeName   nvarchar(300) NOT NULL,
    Priority   int NOT NULL
);

INSERT INTO @ExpectedCodes (GroupLabel, CodeLabel, CodeName, Priority)
VALUES
    -- دلایل لغو توسط راننده
    (N'TripCancelReason_Driver', N'TripCancelReason_Driver_VehicleIssue',     N'مشکل فنی خودرو',                              60),
    (N'TripCancelReason_Driver', N'TripCancelReason_Driver_UserUnreachable',  N'عدم پاسخگویی کاربر',                          50),
    (N'TripCancelReason_Driver', N'TripCancelReason_Driver_TrafficDelay',     N'ترافیک و تأخیر زیاد',                         40),
    (N'TripCancelReason_Driver', N'TripCancelReason_Driver_PetMismatch',      N'عدم تطابق شرایط پت با امکانات خودرو',         30),
    (N'TripCancelReason_Driver', N'TripCancelReason_Driver_Other',            N'سایر موارد',                                  10),

    -- دلایل لغو توسط کاربر
    (N'TripCancelReason_User', N'TripCancelReason_User_ChangedMind',        N'منصرف شدم',                                     60),
    (N'TripCancelReason_User', N'TripCancelReason_User_FoundAlternative',   N'روش دیگری برای جابه‌جایی پیدا کردم',            50),
    (N'TripCancelReason_User', N'TripCancelReason_User_DriverTooLate',      N'تأخیر زیاد راننده',                             40),
    (N'TripCancelReason_User', N'TripCancelReason_User_WrongDetails',       N'اطلاعات سفر را اشتباه ثبت کردم',                30),
    (N'TripCancelReason_User', N'TripCancelReason_User_Other',              N'سایر موارد',                                    10);

MERGE INTO dbo.Codes AS target
USING
(
    SELECT
        cg.Id AS CodeGroupId,
        ec.CodeLabel,
        ec.CodeName,
        ec.Priority
    FROM @ExpectedCodes AS ec
    INNER JOIN dbo.CodeGroups AS cg ON cg.Label = ec.GroupLabel
) AS source
ON target.CodeGroupId = source.CodeGroupId AND target.Label = source.CodeLabel
WHEN MATCHED THEN
    UPDATE SET
        target.Name = source.CodeName,
        target.Priority = source.Priority,
        target.Active = 1
WHEN NOT MATCHED THEN
    INSERT (Name, Label, Value, CodeGroupId, Priority, Active)
    VALUES (source.CodeName, source.CodeLabel, source.CodeLabel, source.CodeGroupId, source.Priority, 1);

COMMIT TRANSACTION;

SELECT
    cg.Label AS GroupLabel,
    cg.Name  AS GroupName,
    c.Id,
    c.Label,
    c.Name,
    c.Priority,
    c.Active
FROM dbo.Codes AS c
INNER JOIN dbo.CodeGroups AS cg ON cg.Id = c.CodeGroupId
WHERE cg.Label IN (N'TripCancelReason_Driver', N'TripCancelReason_User')
ORDER BY cg.Label, c.Priority DESC, c.Id;
GO
