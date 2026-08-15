using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerApplicationWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalValue",
                table: "Pansions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalValue",
                table: "Stores",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "Stores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Existing active stores were already reviewed before this workflow existed.
            migrationBuilder.Sql("UPDATE [Stores] SET [Approved] = 1 WHERE [Active] = 1;");

            migrationBuilder.Sql(
                """
                SET NOCOUNT ON;

                DECLARE @NoticeTypes TABLE
                (
                    Label nvarchar(150) NOT NULL,
                    Title nvarchar(200) NOT NULL,
                    Name nvarchar(1000) NOT NULL,
                    NavigationTemplate nvarchar(500) NOT NULL
                );

                INSERT INTO @NoticeTypes (Label, Title, Name, NavigationTemplate)
                VALUES
                    (N'Driver.Submitted', N'درخواست رانندگی جدید', N'یک درخواست رانندگی جدید ثبت شده است.', N'/admin/driver'),
                    (N'Driver.Updated', N'ارسال مجدد درخواست رانندگی', N'یک درخواست رانندگی برای بررسی مجدد ارسال شده است.', N'/admin/driver'),
                    (N'Companion.Submitted', N'درخواست نمایندگی جدید', N'یک درخواست نمایندگی جدید ثبت شده است.', N'/admin/companion'),
                    (N'Companion.Updated', N'ارسال مجدد درخواست نمایندگی', N'یک درخواست نمایندگی برای بررسی مجدد ارسال شده است.', N'/admin/companion'),
                    (N'Pansion.Submitted', N'درخواست پانسیون جدید', N'یک درخواست پانسیون جدید ثبت شده است.', N'/admin/pansion'),
                    (N'Pansion.Updated', N'ارسال مجدد درخواست پانسیون', N'یک درخواست پانسیون برای بررسی مجدد ارسال شده است.', N'/admin/pansion'),
                    (N'Store.Submitted', N'درخواست فروشگاه جدید', N'یک درخواست فروشگاه جدید ثبت شده است.', N'/admin/store'),
                    (N'Store.Updated', N'ارسال مجدد درخواست فروشگاه', N'یک درخواست فروشگاه برای بررسی مجدد ارسال شده است.', N'/admin/store');

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
                WHERE NOT EXISTS
                (
                    SELECT 1 FROM NoticeTypes target WHERE target.Label = source.Label
                );
                """);

            migrationBuilder.Sql(
                """
                DECLARE @PartnerPushTypes TABLE
                (
                    Id bigint NOT NULL,
                    Name nvarchar(200) NOT NULL,
                    Label nvarchar(200) NOT NULL,
                    Url nvarchar(300) NOT NULL,
                    Tag nvarchar(200) NOT NULL
                );

                INSERT INTO @PartnerPushTypes (Id, Name, Label, Url, Tag)
                VALUES
                    (37, N'تأیید درخواست نمایندگی', N'PushCompanionRequestApproved', N'/profile/companion-request', N'companion-request-approved'),
                    (38, N'رد درخواست نمایندگی', N'PushCompanionRequestRejected', N'/profile/companion-request', N'companion-request-rejected'),
                    (39, N'تأیید درخواست رانندگی', N'PushDriverRequestApproved', N'/profile/driver-request', N'driver-request-approved'),
                    (40, N'رد درخواست رانندگی', N'PushDriverRequestRejected', N'/profile/driver-request', N'driver-request-rejected'),
                    (41, N'تأیید درخواست فروشگاه', N'PushStoreRequestApproved', N'/profile/store-request', N'store-request-approved'),
                    (42, N'رد درخواست فروشگاه', N'PushStoreRequestRejected', N'/profile/store-request', N'store-request-rejected'),
                    (43, N'تأیید درخواست پانسیون', N'PushPansionRequestApproved', N'/companion/pansion', N'pansion-request-approved'),
                    (44, N'رد درخواست پانسیون', N'PushPansionRequestRejected', N'/companion/pansion', N'pansion-request-rejected');

                IF EXISTS
                (
                    SELECT 1
                    FROM @PartnerPushTypes source
                    INNER JOIN PushTypes target ON target.Id = source.Id
                    WHERE target.Label <> source.Label
                )
                    THROW 51000, 'A partner request PushType ID is already assigned to another label.', 1;

                IF EXISTS
                (
                    SELECT 1
                    FROM @PartnerPushTypes source
                    INNER JOIN PushTypes target ON target.Label = source.Label
                    WHERE target.Id <> source.Id
                )
                    THROW 51000, 'A partner request PushType label is already assigned to another ID.', 1;

                SET IDENTITY_INSERT PushTypes ON;

                INSERT INTO PushTypes (Id, Name, Label)
                SELECT source.Id, source.Name, source.Label
                FROM @PartnerPushTypes source
                WHERE NOT EXISTS (SELECT 1 FROM PushTypes target WHERE target.Id = source.Id);

                SET IDENTITY_INSERT PushTypes OFF;

                INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                SELECT source.Id, N'Push_Title', source.Label, source.Url, NULL, source.Tag, 1
                FROM @PartnerPushTypes source
                WHERE NOT EXISTS
                (
                    SELECT 1 FROM PushPatterns pattern WHERE pattern.PushTypeId = source.Id
                );

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId BETWEEN 37 AND 44
                  AND NOT EXISTS
                  (
                      SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE notification
                FROM PushNotifications notification
                INNER JOIN PushPatterns pattern ON pattern.Id = notification.PushPatternId
                WHERE pattern.PushTypeId BETWEEN 37 AND 44;

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId BETWEEN 37 AND 44;

                DELETE FROM PushPatterns WHERE PushTypeId BETWEEN 37 AND 44;
                DELETE FROM PushTypes WHERE Id BETWEEN 37 AND 44;
                """);

            migrationBuilder.DropColumn(
                name: "ApprovalValue",
                table: "Pansions");

            migrationBuilder.DropColumn(
                name: "ApprovalValue",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "Approved",
                table: "Stores");
        }
    }
}
