-- اهمیت اعلان‌های ادمین + نوع‌های جدید (۲۰۲۶-۱۱-۱۵). فقط داده؛ بدون تغییر جدول. چندبار اجرا شدن بی‌خطر است.
-- پوش سیستمی پنل فقط برای «مهم» (2) و «بحرانی» (3) می‌رود؛ «عادی» (1) فقط توی پنل (toast + صدا) نشان داده می‌شود.
--   عادی   : ثبت‌نام کاربر، پیامک، ثبت پروفایل پت‌میچ، مراحل جزئی سفر (برداشتن پت / رسیدن به مقصد)
--   مهم    : ثبت سفارش، ثبت/تغییر رزرو (همه‌ی انواع)، پیام تیکت، درخواست/لغو سفر
--   بحرانی : هر چیزی که نیاز به تأیید/رد/بررسی ادمین دارد (ثبت/ویرایش ارائه‌دهنده، مرکز، خدمت، بسته، راننده، کارت بانکی،
--            نظرها، تأیید هویت و گزارش پت‌میچ، گزارش‌ها، انتخاب راننده، پیگیری میکروچیپ)
-- اعلان‌های Sms.Sent دیگر ساخته نمی‌شوند و از پنل پنهان‌اند؛ ردیف‌شان فقط عادی می‌ماند.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- ۱) نوع‌های جدید
DECLARE @New TABLE (Label nvarchar(150) NOT NULL, Title nvarchar(200) NOT NULL, Name nvarchar(1000) NOT NULL, NavigationTemplate nvarchar(500) NOT NULL);
INSERT INTO @New (Label, Title, Name, NavigationTemplate) VALUES
 (N'Ticket.MessageSubmitted', N'پیام جدید در تیکت', N'کاربر در تیکت «{ticketName}» پیام ارسال کرد.', N'/admin/ticket/detail-{referenceId}'),
 (N'ClinicReserve.Registered', N'رزرو جدید کلینیک', N'{userName} یک رزرو برای «{venueName}» در تاریخ {reserveDate} ثبت کرد.', N'/admin/clinicreserve'),
 (N'CoachReserve.Registered', N'رزرو جدید مربی', N'{userName} یک رزرو برای «{venueName}» در تاریخ {reserveDate} ثبت کرد.', N'/admin/coachreserve'),
 (N'GroomerReserve.Registered', N'رزرو جدید آرایشگاه', N'{userName} یک رزرو برای «{venueName}» در تاریخ {reserveDate} ثبت کرد.', N'/admin/groomerreserve');

INSERT INTO NoticeTypes (Label, Title, Name, NavigationTemplate, Importance, IsActive)
SELECT n.Label, n.Title, n.Name, n.NavigationTemplate, 2, 1
FROM @New n WHERE NOT EXISTS (SELECT 1 FROM NoticeTypes t WHERE t.Label = n.Label);

-- ۲) اهمیت
UPDATE NoticeTypes SET Importance = 1 WHERE Label IN (
 N'User.Registered', N'Sms.Sent', N'PastilMatch.ProfileSubmitted', N'Trip.PetPickedUp', N'Trip.ArrivedDestination');

UPDATE NoticeTypes SET Importance = 2 WHERE Label IN (
 N'ProductOrder.Registered', N'CompanionReserve.Registered', N'CompanionReserve.Updated', N'PansionReserve.Registered',
 N'ClinicReserve.Registered', N'CoachReserve.Registered', N'GroomerReserve.Registered', N'GroomerReserve.Updated',
 N'Ticket.MessageSubmitted', N'Trip.DriverRequested', N'Trip.CancelledByUser', N'Trip.DriverCanceled');

UPDATE NoticeTypes SET Importance = 3 WHERE Label IN (
 N'Companion.Submitted', N'Companion.Updated', N'CompanionUser.Submitted',
 N'CompanionAssistance.Submitted', N'CompanionAssistance.Updated',
 N'CompanionAssistancePackage.Submitted', N'CompanionAssistancePackage.Updated',
 N'CompanionAssistanceReport.Submitted', N'CompanionReport.Submitted',
 N'Companion.CommentSubmitted', N'CompanionReserve.CommentSubmitted', N'Pansion.CommentSubmitted',
 N'Driver.Submitted', N'Driver.Updated', N'Pansion.Submitted', N'Pansion.Updated', N'Store.Submitted', N'Store.Updated',
 N'UserBankCard.Submitted', N'UserBankCard.Updated',
 N'Clinic.Submitted', N'Clinic.Updated', N'Coach.Submitted', N'Coach.Updated', N'Groomer.Submitted', N'Groomer.Updated',
 N'School.Submitted', N'VenueService.Submitted', N'ConsultationPackage.Submitted', N'ConsultationPackage.Updated',
 N'PastilMatch.VerificationRequested', N'PastilMatch.ReportSubmitted', N'PetMicrochip.FollowUpRequested',
 N'Trip.DriverSelectionRequired', N'Trip.NoDriverFound');

SELECT Importance, COUNT(*) AS Types FROM NoticeTypes GROUP BY Importance ORDER BY Importance;
SELECT Label, Importance FROM NoticeTypes WHERE Label NOT IN (
 N'User.Registered', N'Sms.Sent', N'PastilMatch.ProfileSubmitted', N'Trip.PetPickedUp', N'Trip.ArrivedDestination') AND Importance = 1;
COMMIT TRANSACTION;
