-- 2026-11-15: مخاطب پوش گروهی به تفکیک نقش ارائه‌دهنده (پت‌شاپ، کلینیک، مربی، آرایشگر، پانسیون، مدرسه).
-- بک‌اند مخاطب را از Label می‌خواند (PushBroadcastService.ResolveAudienceAsync)؛ Label ها دست نمی‌خورند، فقط نام نمایشی و ردیف‌های تازه.
-- چندبار اجرا شدن بی‌خطر است. تا Api جدید دیپلوی نشود، ارسال به مخاطب‌های تازه خطای «داده نامعتبر» می‌دهد (چیزی ارسال نمی‌شود).
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @group bigint = (SELECT TOP 1 CodeGroupId FROM Codes WHERE Label = N'PushMessageType_All');

-- نام نمایشی مخاطب‌های موجود (فروشگاه = پت‌شاپ، پانسیون)
UPDATE Codes SET Name = N'ارائه‌دهندگان: پت‌شاپ‌ها' WHERE Label = N'PushMessageType_Store';
UPDATE Codes SET Name = N'ارائه‌دهندگان: پانسیون‌ها' WHERE Label = N'PushMessageType_Pansion';

IF NOT EXISTS (SELECT 1 FROM Codes WHERE Label = N'PushMessageType_Clinic')
    INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name) VALUES (N'PushMessageType_Clinic', N'PushMessageType_Clinic', @group, 8, 1, N'ارائه‌دهندگان: کلینیک‌ها');
IF NOT EXISTS (SELECT 1 FROM Codes WHERE Label = N'PushMessageType_Coach')
    INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name) VALUES (N'PushMessageType_Coach', N'PushMessageType_Coach', @group, 9, 1, N'ارائه‌دهندگان: مربیان');
IF NOT EXISTS (SELECT 1 FROM Codes WHERE Label = N'PushMessageType_Groomer')
    INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name) VALUES (N'PushMessageType_Groomer', N'PushMessageType_Groomer', @group, 10, 1, N'ارائه‌دهندگان: آرایشگران');
IF NOT EXISTS (SELECT 1 FROM Codes WHERE Label = N'PushMessageType_School')
    INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name) VALUES (N'PushMessageType_School', N'PushMessageType_School', @group, 11, 1, N'ارائه‌دهندگان: مدارس');

SELECT Id, Name, Label, Priority FROM Codes WHERE CodeGroupId = @group ORDER BY Priority;
COMMIT TRANSACTION;
