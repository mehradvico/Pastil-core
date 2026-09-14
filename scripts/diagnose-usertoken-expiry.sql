-- فقط خواندنی — هیچ تغییری در داده ایجاد نمی‌کند
-- هدف: مقایسه‌ی TokenExp واقعاً ذخیره‌شده در دیتابیس با زمان واقعی سرور دیتابیس (SYSUTCDATETIME)
-- برای کاربر UserId = 4 (همان کاربری که برای تست استفاده شد)

SELECT TOP 5
    Id,
    UserId,
    CreateDate,
    TokenExp,
    RefreshTokenExp,
    Deleted,
    RotatedFromTokenId,
    SYSUTCDATETIME() AS DbServerUtcNow,
    DATEDIFF(MINUTE, SYSUTCDATETIME(), TokenExp) AS MinutesUntilExpiryFromDbPerspective,
    DATEDIFF(MINUTE, CreateDate, TokenExp) AS TokenLifetimeMinutesAsStored
FROM UserTokens
WHERE UserId = 4
ORDER BY Id DESC;
