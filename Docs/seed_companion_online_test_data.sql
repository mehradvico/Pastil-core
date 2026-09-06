-- داده‌ی تست برای محمد قادرپناه (Companion #7، خدمت "مربی سگ" با CompanionAssistanceId = 49)
-- چون این نماینده فعلاً هیچ پکیجی نداشت، دو پکیج نمونه می‌سازیم و هرکدوم رو به هر ۴ گزینه‌ی
-- خدمات آنلاین (چت/تماس پاستیلی/تماس/ویدیو کال) با قیمت متفاوت و از قبل تأییدشده (Active=1) وصل می‌کنیم
-- تا بشه کل مسیر رزرو رو بدون نیاز به تأیید دستی ادمین تست کرد.

DECLARE @CompanionAssistanceId BIGINT = 49;

DECLARE @Packages TABLE (Id BIGINT, Name NVARCHAR(200));

INSERT INTO CompanionAssistancePackages
    (Name, Price, PrePaymentPrice, PictureId, Active, ActivationValue, CompanionAssistanceId, Discription, Deleted)
OUTPUT inserted.Id, inserted.Name INTO @Packages (Id, Name)
VALUES
    (N'پکیج پایه آموزش سگ', 500000, 100000, NULL, 1, NULL, @CompanionAssistanceId, N'یک جلسه مشاوره و آموزش پایه.', 0),
    (N'پکیج پیشرفته آموزش سگ', 1200000, 300000, NULL, 1, NULL, @CompanionAssistanceId, N'دوره‌ی کامل آموزش رفتاری سگ.', 0);

-- شناسه‌های کاتالوگ پایه‌ی خدمات آنلاین: ۱=چت با مربی، ۲=تماس پاستیلی با مربی، ۳=تماس با مربی، ۴=ویدیو کال پاستیلی با مربی
INSERT INTO CompanionAssistancePackageOnlineSelections
    (CompanionAssistancePackageId, CompanionAssistancePackageOnlineId, Price, Active, ActivationValue, Deleted)
SELECT p.Id, o.OnlineId, o.Price, 1, NULL, 0
FROM @Packages p
CROSS JOIN (VALUES (1, 30000), (2, 50000), (3, 70000), (4, 150000)) AS o(OnlineId, Price);

SELECT * FROM @Packages;
