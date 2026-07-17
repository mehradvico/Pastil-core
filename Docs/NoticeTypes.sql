SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @NoticeTypes TABLE
(
    [Label] nvarchar(150) NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Name] nvarchar(1000) NOT NULL,
    [Importance] tinyint NOT NULL,
    [NavigationTemplate] nvarchar(500) NOT NULL,
    [IsActive] bit NOT NULL
);

INSERT INTO @NoticeTypes ([Label], [Title], [Name], [Importance], [NavigationTemplate], [IsActive])
VALUES
(N'Sms.Sent', N'ارسال پیامک', N'پیامک {messageType} برای شماره {mobile} در تاریخ {sendDate} ارسال شد.', 3, N'/admin/messages?type={messageType}', 1),
(N'ProductOrder.Registered', N'سفارش جدید', N'{userName} سفارش {orderId} را ثبت کرد؛ لطفاً پیگیری نمایید.', 3, N'/admin/product-orders/{orderId}', 1),
(N'CompanionReserve.Registered', N'رزرو جدید همراه', N'{userName} سرویس {serviceName} را از {companionName} رزرو کرد؛ لطفاً پیگیری نمایید.', 3, N'/admin/companion-reserves/{referenceId}', 1),
(N'CompanionReserve.Updated', N'ویرایش رزرو همراه', N'رزرو همراه با شناسه {referenceId} ویرایش شد.', 1, N'/admin/companion-reserves/{referenceId}', 1),
(N'PansionReserve.Registered', N'رزرو جدید پانسیون', N'{userName} در {pansionName} برای تاریخ {reserveDate} رزرو ثبت کرد؛ لطفاً پیگیری نمایید.', 3, N'/admin/pansion-reserves/{referenceId}', 1),
(N'Pansion.Submitted', N'پانسیون در انتظار بررسی', N'پانسیون {pansionName} ثبت شد و نیاز به بررسی و تأیید دارد.', 2, N'/admin/pansions/{referenceId}', 1),
(N'Companion.Submitted', N'همراه در انتظار بررسی', N'همراه {companionName} ثبت شد و نیاز به بررسی و تأیید دارد.', 2, N'/admin/companions/{referenceId}', 1),
(N'Companion.Updated', N'ویرایش اطلاعات همراه', N'اطلاعات همراه {companionName} ویرایش شد و نیاز به بررسی دارد.', 2, N'/admin/companions/{referenceId}', 1),
(N'CompanionAssistance.Submitted', N'خدمت همراه در انتظار بررسی', N'خدمت همراه با شناسه {referenceId} ثبت شد و نیاز به بررسی و فعال‌سازی دارد.', 2, N'/admin/companion-assistances/{referenceId}', 1),
(N'CompanionAssistance.Updated', N'ویرایش خدمت همراه', N'خدمت همراه با شناسه {referenceId} ویرایش شد و نیاز به بررسی دارد.', 2, N'/admin/companion-assistances/{referenceId}', 1),
(N'CompanionAssistancePackage.Submitted', N'پکیج خدمت در انتظار بررسی', N'پکیج خدمت با شناسه {referenceId} ثبت شد و نیاز به بررسی و فعال‌سازی دارد.', 2, N'/admin/companion-assistance-packages/{referenceId}', 1),
(N'CompanionAssistancePackage.Updated', N'ویرایش پکیج خدمت', N'پکیج خدمت با شناسه {referenceId} ویرایش شد و نیاز به بررسی دارد.', 2, N'/admin/companion-assistance-packages/{referenceId}', 1),
(N'CompanionAssistanceReport.Submitted', N'گزارش خدمت همراه', N'گزارش خدمت همراه با شناسه {referenceId} ثبت شد؛ لطفاً بررسی نمایید.', 2, N'/admin/companion-assistance-reports/{referenceId}', 1),
(N'CompanionReport.Submitted', N'گزارش همراه', N'گزارش همراه با شناسه {referenceId} ثبت شد؛ لطفاً بررسی نمایید.', 2, N'/admin/companion-reports/{referenceId}', 1),
(N'CompanionUser.Submitted', N'عضو جدید مجموعه همراه', N'عضو مجموعه همراه با شناسه {referenceId} ثبت شد و نیاز به بررسی دارد.', 2, N'/admin/companion-users/{referenceId}', 1),
(N'Driver.Submitted', N'راننده در انتظار بررسی', N'راننده {driverName} ثبت شد و نیاز به بررسی و تأیید دارد.', 2, N'/admin/drivers/{referenceId}', 1),
(N'Driver.Updated', N'ویرایش اطلاعات راننده', N'اطلاعات راننده {driverName} ویرایش شد و نیاز به بررسی دارد.', 2, N'/admin/drivers/{referenceId}', 1),
(N'UserBankCard.Submitted', N'کارت بانکی در انتظار بررسی', N'کارت بانکی با شناسه {referenceId} ثبت شد و نیاز به تأیید دارد.', 2, N'/admin/user-bank-cards/{referenceId}', 1),
(N'UserBankCard.Updated', N'ویرایش کارت بانکی', N'کارت بانکی با شناسه {referenceId} ویرایش شد و نیاز به تأیید مجدد دارد.', 2, N'/admin/user-bank-cards/{referenceId}', 1),
(N'Trip.DriverRequested', N'درخواست راننده', N'برای سفر با شناسه {referenceId} درخواست راننده ثبت شد؛ لطفاً پیگیری نمایید.', 3, N'/admin/trips/{referenceId}', 1),
(N'Trip.DriverSelectionRequired', N'انتخاب راننده', N'راننده سفر با شناسه {referenceId} درخواست را رد کرد؛ راننده دیگری انتخاب نمایید.', 2, N'/admin/trips/{referenceId}', 1),
(N'Trip.CancelledByUser', N'لغو سفر توسط کاربر', N'سفر با شناسه {referenceId} توسط کاربر لغو شد؛ لطفاً پیگیری نمایید.', 3, N'/admin/trips/{referenceId}', 1);

UPDATE target
SET target.[Title] = source.[Title],
    target.[Name] = source.[Name],
    target.[Importance] = source.[Importance],
    target.[NavigationTemplate] = source.[NavigationTemplate],
    target.[IsActive] = source.[IsActive]
FROM [dbo].[NoticeTypes] target
INNER JOIN @NoticeTypes source ON source.[Label] = target.[Label];

INSERT INTO [dbo].[NoticeTypes] ([Label], [Title], [Name], [Importance], [NavigationTemplate], [IsActive])
SELECT source.[Label], source.[Title], source.[Name], source.[Importance], source.[NavigationTemplate], source.[IsActive]
FROM @NoticeTypes source
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[NoticeTypes] target WITH (UPDLOCK, HOLDLOCK) WHERE target.[Label] = source.[Label]);

COMMIT TRANSACTION;

SELECT [Id], [Label], [Title], [Name], [Importance], [NavigationTemplate], [IsActive]
FROM [dbo].[NoticeTypes]
WHERE [Label] IN (SELECT [Label] FROM @NoticeTypes)
ORDER BY [Importance] DESC, [Label];
