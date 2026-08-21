using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixPushNotificationRoutesAndRemoveSignInPush : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
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
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                SET NOCOUNT ON;

                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 2 AND Label <> N'PushSignInUser')
                    THROW 51000, 'PushType ID 2 is already assigned to another label.', 1;

                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushSignInUser' AND Id <> 2)
                    THROW 51000, 'PushSignInUser is already assigned to another ID.', 1;

                SET IDENTITY_INSERT PushTypes ON;

                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 2)
                    INSERT INTO PushTypes (Id, Name, Label)
                    VALUES (2, N'ورود کاربر', N'PushSignInUser');

                SET IDENTITY_INSERT PushTypes OFF;

                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 2)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (2, N'Push_Title', N'PushSignInUser', N'/profile', NULL, N'user-sign-in', 1);

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId = 2
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM PushSettings setting
                      WHERE setting.PushPatternId = pattern.Id
                  );

                DECLARE @PreviousRoutes TABLE
                (
                    Label nvarchar(200) NOT NULL PRIMARY KEY,
                    Url nvarchar(500) NOT NULL
                );

                INSERT INTO @PreviousRoutes (Label, Url)
                VALUES
                    (N'PushPastilMatchRequestReceived', N'/pastil-match/requests'),
                    (N'PushPastilMatchRequestAccepted', N'/pastil-match/chats'),
                    (N'PushPastilMatchRequestRejected', N'/pastil-match/requests'),
                    (N'PushPastilMatchNewMessage', N'/pastil-match/chats'),
                    (N'PushPastilMatchProfileLiked', N'/pastil-match/profile'),
                    (N'PushPastilMatchClosed', N'/pastil-match'),
                    (N'PushPastilMatchVerificationApproved', N'/pastil-match/profile'),
                    (N'PushPastilMatchVerificationRejected', N'/pastil-match/profile'),
                    (N'PushPastilMatchMessageReaction', N'/pastil-match/chats'),
                    (N'PushPastilMatchRequestCancelled', N'/pastil-match/requests'),
                    (N'PushCompanionRequestApproved', N'/profile/companion-request'),
                    (N'PushCompanionRequestRejected', N'/profile/companion-request'),
                    (N'PushDriverRequestApproved', N'/profile/driver-request'),
                    (N'PushDriverRequestRejected', N'/profile/driver-request'),
                    (N'PushStoreRequestApproved', N'/profile/store-request'),
                    (N'PushStoreRequestRejected', N'/profile/store-request'),
                    (N'PushPansionRequestApproved', N'/companion/pansion'),
                    (N'PushPansionRequestRejected', N'/companion/pansion');

                UPDATE pattern
                SET pattern.Url = route.Url
                FROM PushPatterns pattern
                INNER JOIN PushTypes pushType ON pushType.Id = pattern.PushTypeId
                INNER JOIN @PreviousRoutes route ON route.Label = pushType.Label;
                """);
        }
    }
}
