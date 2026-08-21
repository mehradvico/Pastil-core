BEGIN TRANSACTION;
SET NOCOUNT ON;

DECLARE @Routes TABLE
(
    Label nvarchar(200) NOT NULL PRIMARY KEY,
    Url nvarchar(500) NOT NULL
);

INSERT INTO @Routes (Label, Url)
VALUES
    (N'PushSignUpUser', N'/profile'),
    (N'PushSignUpAdmin', N'/profile'),
    (N'PushRegisterOrderUser', N'/orders/{1}'),
    (N'PushProccessOrderUser', N'/orders/{1}'),
    (N'PushSentOrderUser', N'/orders/{1}'),
    (N'PushRegisterOrderStore', N'/sellerProfile/orders'),
    (N'PushRegisterOrderAdmin', N'/'),
    (N'PushSentOrderAdmin', N'/'),
    (N'PushRegisterReserveUser', N'/reserve'),
    (N'PushCompleteReserveUser', N'/reserve'),
    (N'PushCancelReserveUser', N'/reserve'),
    (N'PushRegisterReserveCompanion', N'/companionProfile/companionReserve'),
    (N'PushCancelReserveCompanion', N'/companionProfile/companionReserve'),
    (N'PushRegisterReserveAdmin', N'/'),
    (N'PushCompleteReserveAdmin', N'/'),
    (N'PushCancelReserveAdmin', N'/'),
    (N'PushRegisterPansionUser', N'/reserve'),
    (N'PushCompletePansionUser', N'/reserve'),
    (N'PushRegisterPansionCompanion', N'/companionProfile/pansionReserve'),
    (N'PushRegisterPansionAdmin', N'/'),
    (N'PushCompletePansionAdmin', N'/'),
    (N'PushMemoryReminder', N'/memories'),
    (N'PushReminderOneWeekBefore', N'/reminder'),
    (N'PushReminderOneDayBefore', N'/reminder'),
    (N'PushReminderOneDayAfter', N'/reminder'),
    (N'PushPastilMatchRequestReceived', N'/match'),
    (N'PushPastilMatchRequestAccepted', N'/match/{2}?profileId={3}'),
    (N'PushPastilMatchRequestRejected', N'/match'),
    (N'PushPastilMatchNewMessage', N'/match/{2}?profileId={3}'),
    (N'PushPastilMatchProfileLiked', N'/match'),
    (N'PushPastilMatchClosed', N'/match'),
    (N'PushPastilMatchVerificationApproved', N'/match'),
    (N'PushPastilMatchVerificationRejected', N'/match'),
    (N'PushPastilMatchMessageReaction', N'/match/{2}?profileId={3}'),
    (N'PushPastilMatchRequestCancelled', N'/match'),
    (N'PushCompanionRequestApproved', N'/companionProfile'),
    (N'PushCompanionRequestRejected', N'/agencyRequest'),
    (N'PushDriverRequestApproved', N'/driverProfile'),
    (N'PushDriverRequestRejected', N'/driverRequest'),
    (N'PushStoreRequestApproved', N'/sellerProfile'),
    (N'PushStoreRequestRejected', N'/sellerProfile'),
    (N'PushPansionRequestApproved', N'/companionProfile/pansion'),
    (N'PushPansionRequestRejected', N'/companionProfile/pansion'),
    (N'PushCompanionReserveAssigned', N'/operator');

UPDATE pattern
SET pattern.Url = route.Url
FROM PushPatterns pattern
INNER JOIN PushTypes pushType ON pushType.Id = pattern.PushTypeId
INNER JOIN @Routes route ON route.Label = pushType.Label;

DECLARE @SignInPushTypes TABLE (Id bigint NOT NULL PRIMARY KEY);

INSERT INTO @SignInPushTypes (Id)
SELECT Id
FROM PushTypes
WHERE Label = N'PushSignInUser';

DELETE notification
FROM PushNotifications notification
INNER JOIN PushPatterns pattern ON pattern.Id = notification.PushPatternId
INNER JOIN @SignInPushTypes pushType ON pushType.Id = pattern.PushTypeId;

DELETE setting
FROM PushSettings setting
INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
INNER JOIN @SignInPushTypes pushType ON pushType.Id = pattern.PushTypeId;

DELETE pattern
FROM PushPatterns pattern
INNER JOIN @SignInPushTypes pushType ON pushType.Id = pattern.PushTypeId;

DELETE pushType
FROM PushTypes pushType
INNER JOIN @SignInPushTypes removed ON removed.Id = pushType.Id;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260821085935_FixPushNotificationRoutesAndRemoveSignInPush', N'9.0.0');

COMMIT;
GO

