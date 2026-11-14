-- روش‌های ارسال با قیمت فروشگاه: پست، تیپاکس، ماهکس، الوپیک (۲۰۲۶-۱۱-۱۳)
-- پست/تیپاکس/ماهکس/الوپیک را فروشگاه خودش قیمت می‌گذارد و خودش «پس‌کرایه» بودن را تعیین می‌کند؛ باید در فهرست انواع ارسال فعال باشند.
-- این اسکریپت فقط داده‌ی Codes را درست می‌کند (ماهکس نبود؛ تیپاکس و الوپیک غیرفعال بودند). چند بار اجرا هم امن است. هیچ تغییر ساختاری ندارد.
-- ترتیب: اول این اسکریپت، بعد دیپلوی Api.

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET LOCK_TIMEOUT 15000;

BEGIN TRAN;

DECLARE @GroupId BIGINT = (SELECT TOP (1) CodeGroupId FROM Codes WHERE Label = 'DeliveryType_Post' ORDER BY Id);

IF @GroupId IS NULL
BEGIN
    RAISERROR(N'گروه کد DeliveryType پیدا نشد.', 16, 1);
    ROLLBACK TRAN;
    RETURN;
END

IF NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'DeliveryType_Mahex')
    INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
    VALUES ('DeliveryType_Mahex', 'mahex', @GroupId, 8, 1, N'ماهکس');

UPDATE Codes SET Active = 1 WHERE Label IN ('DeliveryType_Post', 'DeliveryType_Tipax', 'DeliveryType_AloPeyk', 'DeliveryType_Mahex');

COMMIT TRAN;

SELECT Id, Name, Label, Priority, Active FROM Codes WHERE Label LIKE 'DeliveryType[_]%' ORDER BY Priority, Id;
