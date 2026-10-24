-- برچسب DeviceName توکن‌های وب‌اپ/موبایل از «zand» به «vico» (کد جدید هم از این به بعد vico می‌نویسد).
-- هیچ منطقی به مقدار zand وابسته نیست (فقط نشست‌های panel:* بررسی می‌شوند)؛ اجرا چندباره بی‌خطر است.
BEGIN TRANSACTION;

SELECT COUNT(*) AS ToRename FROM [UserTokens] WHERE [DeviceName] = N'zand';

UPDATE [UserTokens] SET [DeviceName] = N'vico' WHERE [DeviceName] = N'zand';

COMMIT;
