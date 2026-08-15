using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPastilMatchPushNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttemptCount",
                table: "PushNotifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextAttemptDate",
                table: "PushNotifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PushNotifications_IsSend_Status_NextAttemptDate_SendDate",
                table: "PushNotifications",
                columns: new[] { "IsSend", "Status", "NextAttemptDate", "SendDate" });

            migrationBuilder.Sql(@"
DECLARE @PastilMatchPushTypes TABLE
(
    Id bigint NOT NULL,
    Name nvarchar(200) NOT NULL,
    Label nvarchar(200) NOT NULL,
    Body nvarchar(200) NOT NULL,
    Url nvarchar(300) NOT NULL,
    Tag nvarchar(200) NOT NULL
);

INSERT INTO @PastilMatchPushTypes (Id, Name, Label, Body, Url, Tag)
VALUES
    (27, N'دریافت درخواست پاستیل مچ', N'PushPastilMatchRequestReceived', N'PushPastilMatchRequestReceived', N'/pastil-match/requests', N'pastil-match-request-received'),
    (28, N'پذیرش درخواست پاستیل مچ', N'PushPastilMatchRequestAccepted', N'PushPastilMatchRequestAccepted', N'/pastil-match/chats', N'pastil-match-request-accepted'),
    (29, N'رد درخواست پاستیل مچ', N'PushPastilMatchRequestRejected', N'PushPastilMatchRequestRejected', N'/pastil-match/requests', N'pastil-match-request-rejected'),
    (30, N'پیام جدید پاستیل مچ', N'PushPastilMatchNewMessage', N'PushPastilMatchNewMessage', N'/pastil-match/chats', N'pastil-match-new-message'),
    (31, N'پسندیدن پروفایل پاستیل مچ', N'PushPastilMatchProfileLiked', N'PushPastilMatchProfileLiked', N'/pastil-match/profile', N'pastil-match-profile-liked'),
    (32, N'پایان ارتباط پاستیل مچ', N'PushPastilMatchClosed', N'PushPastilMatchClosed', N'/pastil-match', N'pastil-match-closed'),
    (33, N'تأیید پروفایل پاستیل مچ', N'PushPastilMatchVerificationApproved', N'PushPastilMatchVerificationApproved', N'/pastil-match/profile', N'pastil-match-verification-approved'),
    (34, N'رد تأیید پروفایل پاستیل مچ', N'PushPastilMatchVerificationRejected', N'PushPastilMatchVerificationRejected', N'/pastil-match/profile', N'pastil-match-verification-rejected'),
    (35, N'واکنش به پیام پاستیل مچ', N'PushPastilMatchMessageReaction', N'PushPastilMatchMessageReaction', N'/pastil-match/chats', N'pastil-match-message-reaction'),
    (36, N'لغو درخواست پاستیل مچ', N'PushPastilMatchRequestCancelled', N'PushPastilMatchRequestCancelled', N'/pastil-match/requests', N'pastil-match-request-cancelled');

IF EXISTS
(
    SELECT 1
    FROM @PastilMatchPushTypes source
    INNER JOIN PushTypes target ON target.Id = source.Id
    WHERE target.Label <> source.Label
)
    THROW 51000, 'A PastilMatch PushType ID is already assigned to another label.', 1;

IF EXISTS
(
    SELECT 1
    FROM @PastilMatchPushTypes source
    INNER JOIN PushTypes target ON target.Label = source.Label
    WHERE target.Id <> source.Id
)
    THROW 51000, 'A PastilMatch PushType label is already assigned to another ID.', 1;

SET IDENTITY_INSERT PushTypes ON;

INSERT INTO PushTypes (Id, Name, Label)
SELECT source.Id, source.Name, source.Label
FROM @PastilMatchPushTypes source
WHERE NOT EXISTS (SELECT 1 FROM PushTypes target WHERE target.Id = source.Id);

SET IDENTITY_INSERT PushTypes OFF;

INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
SELECT source.Id, N'Push_Title', source.Body, source.Url, NULL, source.Tag, 1
FROM @PastilMatchPushTypes source
WHERE NOT EXISTS
(
    SELECT 1
    FROM PushPatterns pattern
    WHERE pattern.PushTypeId = source.Id
);

INSERT INTO PushSettings (PushPatternId, IsEnabled)
SELECT pattern.Id, 1
FROM PushPatterns pattern
WHERE pattern.PushTypeId BETWEEN 27 AND 36
  AND NOT EXISTS
  (
      SELECT 1
      FROM PushSettings setting
      WHERE setting.PushPatternId = pattern.Id
  );
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE notification
FROM PushNotifications notification
INNER JOIN PushPatterns pattern ON pattern.Id = notification.PushPatternId
WHERE pattern.PushTypeId BETWEEN 27 AND 36;

DELETE setting
FROM PushSettings setting
INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
WHERE pattern.PushTypeId BETWEEN 27 AND 36;

DELETE FROM PushPatterns WHERE PushTypeId BETWEEN 27 AND 36;
DELETE FROM PushTypes WHERE Id BETWEEN 27 AND 36;
");

            migrationBuilder.DropIndex(
                name: "IX_PushNotifications_IsSend_Status_NextAttemptDate_SendDate",
                table: "PushNotifications");

            migrationBuilder.DropColumn(
                name: "AttemptCount",
                table: "PushNotifications");

            migrationBuilder.DropColumn(
                name: "NextAttemptDate",
                table: "PushNotifications");
        }
    }
}
