SET XACT_ABORT ON;
BEGIN TRY
BEGIN TRANSACTION;

DECLARE @Routes TABLE
(
    [Label] nvarchar(150) NOT NULL PRIMARY KEY,
    [NavigationTemplate] nvarchar(500) NOT NULL
);

INSERT INTO @Routes ([Label], [NavigationTemplate])
VALUES
(N'Sms.Sent', N'/admin/notice'),
(N'ProductOrder.Registered', N'/admin/productorder/detail-{orderId}'),
(N'CompanionReserve.Registered', N'/admin/companionreserve/detail-{referenceId}'),
(N'CompanionReserve.Updated', N'/admin/companionreserve/detail-{referenceId}'),
(N'PansionReserve.Registered', N'/admin/pansionreserve/detail-{referenceId}'),
(N'Pansion.Submitted', N'/admin/pansion?companionId={companionId}'),
(N'Companion.Submitted', N'/admin/companion'),
(N'Companion.Updated', N'/admin/companion'),
(N'CompanionAssistance.Submitted', N'/admin/companion-assistance?companionId={companionId}'),
(N'CompanionAssistance.Updated', N'/admin/companion-assistance?companionId={companionId}'),
(N'CompanionAssistancePackage.Submitted', N'/admin/companion-assistance-package?companionAssistanceId={companionAssistanceId}'),
(N'CompanionAssistancePackage.Updated', N'/admin/companion-assistance-package?companionAssistanceId={companionAssistanceId}'),
(N'CompanionAssistanceReport.Submitted', N'/admin/companion-assistance-report?companionAssistanceId={companionAssistanceId}'),
(N'CompanionReport.Submitted', N'/admin/companion-report?companionId={companionId}'),
(N'CompanionUser.Submitted', N'/admin/companion-user?companionId={companionId}'),
(N'Driver.Submitted', N'/admin/driver?driverId={referenceId}'),
(N'Driver.Updated', N'/admin/driver?driverId={referenceId}'),
(N'UserBankCard.Submitted', N'/admin/userbankcard?userBankCardId={referenceId}'),
(N'UserBankCard.Updated', N'/admin/userbankcard?userBankCardId={referenceId}'),
(N'Trip.DriverRequested', N'/admin/notice'),
(N'Trip.DriverSelectionRequired', N'/admin/notice'),
(N'Trip.CancelledByUser', N'/admin/notice');

IF EXISTS
(
    SELECT 1
    FROM @Routes route
    LEFT JOIN [dbo].[NoticeTypes] noticeType ON noticeType.[Label] = route.[Label]
    WHERE noticeType.[Id] IS NULL
)
    THROW 50001, 'One or more NoticeTypes were not found. No route was updated.', 1;

UPDATE noticeType
SET noticeType.[NavigationTemplate] = route.[NavigationTemplate]
FROM [dbo].[NoticeTypes] noticeType
INNER JOIN @Routes route ON route.[Label] = noticeType.[Label];

UPDATE notice
SET notice.[NavigationUrl] = CASE noticeType.[Label]
    WHEN N'Sms.Sent' THEN N'/admin/notice'
    WHEN N'Companion.Submitted' THEN N'/admin/companion'
    WHEN N'Companion.Updated' THEN N'/admin/companion'
    WHEN N'Trip.DriverRequested' THEN N'/admin/notice'
    WHEN N'Trip.DriverSelectionRequired' THEN N'/admin/notice'
    WHEN N'Trip.CancelledByUser' THEN N'/admin/notice'
END
FROM [dbo].[Notices] notice
INNER JOIN [dbo].[NoticeTypes] noticeType ON noticeType.[Id] = notice.[NoticeTypeId]
WHERE noticeType.[Label] IN (N'Sms.Sent', N'Companion.Submitted', N'Companion.Updated', N'Trip.DriverRequested', N'Trip.DriverSelectionRequired', N'Trip.CancelledByUser');

UPDATE notice
SET notice.[NavigationUrl] = CASE noticeType.[Label]
    WHEN N'ProductOrder.Registered' THEN CONCAT(N'/admin/productorder/detail-', notice.[ReferenceId])
    WHEN N'CompanionReserve.Registered' THEN CONCAT(N'/admin/companionreserve/detail-', notice.[ReferenceId])
    WHEN N'CompanionReserve.Updated' THEN CONCAT(N'/admin/companionreserve/detail-', notice.[ReferenceId])
    WHEN N'PansionReserve.Registered' THEN CONCAT(N'/admin/pansionreserve/detail-', notice.[ReferenceId])
    WHEN N'Driver.Submitted' THEN CONCAT(N'/admin/driver?driverId=', notice.[ReferenceId])
    WHEN N'Driver.Updated' THEN CONCAT(N'/admin/driver?driverId=', notice.[ReferenceId])
    WHEN N'UserBankCard.Submitted' THEN CONCAT(N'/admin/userbankcard?userBankCardId=', notice.[ReferenceId])
    WHEN N'UserBankCard.Updated' THEN CONCAT(N'/admin/userbankcard?userBankCardId=', notice.[ReferenceId])
END
FROM [dbo].[Notices] notice
INNER JOIN [dbo].[NoticeTypes] noticeType ON noticeType.[Id] = notice.[NoticeTypeId]
WHERE notice.[ReferenceId] IS NOT NULL
AND noticeType.[Label] IN (N'ProductOrder.Registered', N'CompanionReserve.Registered', N'CompanionReserve.Updated', N'PansionReserve.Registered', N'Driver.Submitted', N'Driver.Updated', N'UserBankCard.Submitted', N'UserBankCard.Updated');

UPDATE notice
SET notice.[NavigationUrl] = CONCAT(N'/admin/pansion?companionId=', pansion.[CompanionId])
FROM [dbo].[Notices] notice
INNER JOIN [dbo].[NoticeTypes] noticeType ON noticeType.[Id] = notice.[NoticeTypeId]
INNER JOIN [dbo].[Pansions] pansion ON pansion.[Id] = notice.[ReferenceId]
WHERE noticeType.[Label] = N'Pansion.Submitted';

UPDATE notice
SET notice.[NavigationUrl] = CONCAT(N'/admin/companion-assistance?companionId=', assistance.[CompanionId])
FROM [dbo].[Notices] notice
INNER JOIN [dbo].[NoticeTypes] noticeType ON noticeType.[Id] = notice.[NoticeTypeId]
INNER JOIN [dbo].[CompanionAssistances] assistance ON assistance.[Id] = notice.[ReferenceId]
WHERE noticeType.[Label] IN (N'CompanionAssistance.Submitted', N'CompanionAssistance.Updated');

UPDATE notice
SET notice.[NavigationUrl] = CONCAT(N'/admin/companion-assistance-package?companionAssistanceId=', assistancePackage.[CompanionAssistanceId])
FROM [dbo].[Notices] notice
INNER JOIN [dbo].[NoticeTypes] noticeType ON noticeType.[Id] = notice.[NoticeTypeId]
INNER JOIN [dbo].[CompanionAssistancePackages] assistancePackage ON assistancePackage.[Id] = notice.[ReferenceId]
WHERE noticeType.[Label] IN (N'CompanionAssistancePackage.Submitted', N'CompanionAssistancePackage.Updated');

UPDATE notice
SET notice.[NavigationUrl] = CONCAT(N'/admin/companion-assistance-report?companionAssistanceId=', report.[CompanionAssistanceId])
FROM [dbo].[Notices] notice
INNER JOIN [dbo].[NoticeTypes] noticeType ON noticeType.[Id] = notice.[NoticeTypeId]
INNER JOIN [dbo].[CompanionAssistanceReports] report ON report.[Id] = notice.[ReferenceId]
WHERE noticeType.[Label] = N'CompanionAssistanceReport.Submitted';

UPDATE notice
SET notice.[NavigationUrl] = CONCAT(N'/admin/companion-report?companionId=', report.[CompanionId])
FROM [dbo].[Notices] notice
INNER JOIN [dbo].[NoticeTypes] noticeType ON noticeType.[Id] = notice.[NoticeTypeId]
INNER JOIN [dbo].[CompanionReports] report ON report.[Id] = notice.[ReferenceId]
WHERE noticeType.[Label] = N'CompanionReport.Submitted';

UPDATE notice
SET notice.[NavigationUrl] = CONCAT(N'/admin/companion-user?companionId=', companionUser.[CompanionId])
FROM [dbo].[Notices] notice
INNER JOIN [dbo].[NoticeTypes] noticeType ON noticeType.[Id] = notice.[NoticeTypeId]
INNER JOIN [dbo].[CompanionUsers] companionUser ON companionUser.[Id] = notice.[ReferenceId]
WHERE noticeType.[Label] = N'CompanionUser.Submitted';

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT noticeType.[Id], noticeType.[Label], noticeType.[NavigationTemplate]
FROM [dbo].[NoticeTypes] noticeType
INNER JOIN @Routes route ON route.[Label] = noticeType.[Label]
ORDER BY noticeType.[Label];
