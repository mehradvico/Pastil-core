-- 2026-11-14: مخاطب پوش «نمایندگان» → «ارائه‌دهندگان» (فقط نام نمایشی؛ Label = PushMessageType_Companion دست نمی‌خورد چون بک‌اند مخاطب را از Label می‌خواند)
-- اجرا بعد از دیپلوی Api (کد جدید PushBroadcastService که ارائه‌دهنده‌ها را هم می‌گیرد).
BEGIN TRAN;

SELECT Id, Name, Label FROM Codes WHERE Label = N'PushMessageType_Companion';

UPDATE Codes SET Name = N'ارائه‌دهندگان' WHERE Label = N'PushMessageType_Companion';

SELECT Id, Name, Label FROM Codes WHERE Label = N'PushMessageType_Companion';

-- اگر خروجی درست بود:
COMMIT;
-- وگرنه: ROLLBACK;
