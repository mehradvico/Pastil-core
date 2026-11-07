-- اعلان ادمین (زنگوله‌ی پنل) برای ثبت/ویرایش‌های ارائه‌دهنده که تا تأیید ادمین غیرفعال می‌مانند.
-- کد: Application/Services/Setting/NoticeSrv/VenueReviewNotifier.cs
-- فقط داده (بدون تغییر جدول)؛ چندبار اجرا شدن بی‌خطر است. بدون این ردیف‌ها اعلان ساخته نمی‌شود (فقط لاگ هشدار).
SET NOCOUNT ON;
BEGIN TRANSACTION;

DECLARE @NoticeTypes TABLE
(
    Label nvarchar(150) NOT NULL,
    Title nvarchar(200) NOT NULL,
    Name nvarchar(1000) NOT NULL,
    NavigationTemplate nvarchar(500) NOT NULL
);

INSERT INTO @NoticeTypes (Label, Title, Name, NavigationTemplate)
VALUES
    (N'Clinic.Submitted', N'درخواست کلینیک جدید', N'کلینیک «{venueName}» ثبت شد و منتظر تأیید است.', N'/admin/companion?kind=6'),
    (N'Clinic.Updated', N'ویرایش کلینیک — نیاز به تأیید', N'کلینیک «{venueName}» توسط ارائه‌دهنده ویرایش و غیرفعال شد؛ بررسی و فعال کنید.', N'/admin/companion?kind=6'),
    (N'Coach.Submitted', N'درخواست مربی جدید', N'مربی «{venueName}» ثبت شد و منتظر تأیید است.', N'/admin/companion?kind=7'),
    (N'Coach.Updated', N'ویرایش مربی — نیاز به تأیید', N'مربی «{venueName}» توسط ارائه‌دهنده ویرایش و غیرفعال شد؛ بررسی و فعال کنید.', N'/admin/companion?kind=7'),
    (N'Groomer.Submitted', N'درخواست آرایشگاه جدید', N'آرایشگاه «{venueName}» ثبت شد و منتظر تأیید است.', N'/admin/companion?kind=5'),
    (N'Groomer.Updated', N'ویرایش آرایشگاه — نیاز به تأیید', N'آرایشگاه «{venueName}» توسط ارائه‌دهنده ویرایش و غیرفعال شد؛ بررسی و فعال کنید.', N'/admin/companion?kind=5'),
    (N'School.Submitted', N'درخواست مدرسه جدید', N'مدرسه «{venueName}» ثبت شد و منتظر تأیید است.', N'/admin/companion?kind=4'),
    (N'VenueService.Submitted', N'خدمت جدید — نیاز به تأیید', N'ارائه‌دهنده یک خدمت جدید تعریف کرد؛ تا تأیید شما قابل رزرو نیست.', N'/admin/companion'),
    (N'ConsultationPackage.Submitted', N'بسته‌ی مشاوره‌ی جدید — نیاز به تأیید', N'بسته‌ی مشاوره‌ی «{packageName}» ثبت شد و منتظر تأیید است.', N'/admin/companion'),
    (N'ConsultationPackage.Updated', N'ویرایش بسته‌ی مشاوره — نیاز به تأیید', N'بسته‌ی مشاوره‌ی «{packageName}» ویرایش شد و تا تأیید دوباره نمایش داده نمی‌شود.', N'/admin/companion');

UPDATE target
SET target.Title = source.Title,
    target.Name = source.Name,
    target.NavigationTemplate = source.NavigationTemplate,
    target.Importance = 2,
    target.IsActive = 1
FROM NoticeTypes target
INNER JOIN @NoticeTypes source ON source.Label = target.Label;

INSERT INTO NoticeTypes (Label, Title, Name, NavigationTemplate, Importance, IsActive)
SELECT source.Label, source.Title, source.Name, source.NavigationTemplate, 2, 1
FROM @NoticeTypes source
WHERE NOT EXISTS (SELECT 1 FROM NoticeTypes target WHERE target.Label = source.Label);

COMMIT;

SELECT Label, Title, Importance, IsActive FROM NoticeTypes
WHERE Label IN (N'Clinic.Submitted', N'Clinic.Updated', N'Coach.Submitted', N'Coach.Updated', N'Groomer.Submitted',
                N'Groomer.Updated', N'School.Submitted', N'VenueService.Submitted',
                N'ConsultationPackage.Submitted', N'ConsultationPackage.Updated');
