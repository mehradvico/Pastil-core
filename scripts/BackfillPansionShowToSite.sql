-- فهرست و جزئیات عمومی پانسیون حالا فقط پانسیونِ دارای ShowToSite = 1 را نشان می‌دهد (علاوه بر Active و Approve).
-- برای اینکه با دیپلوی، پانسیون‌های فعلی ناگهان از سایت ناپدید نشوند، قبل از دیپلوی Api این اسکریپت را اجرا کنید:
-- هر پانسیونِ فعال، تأییدشده و حذف‌نشده که الان در سایت دیده می‌شود، ShowToSite = 1 می‌گیرد.

-- پیش‌نمایش
SELECT ShowToSite, Active, Approve, COUNT(*) AS Cnt FROM Pansions WHERE Deleted = 0 GROUP BY ShowToSite, Active, Approve;

-- اعمال (بعد از بررسی، کامنت را بردارید)
-- UPDATE Pansions SET ShowToSite = 1 WHERE Deleted = 0 AND Active = 1 AND Approve = 1 AND ShowToSite = 0;
