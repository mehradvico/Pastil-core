using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTripDriverBroadcastPush : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    (N'Trip.DriverCanceled', N'انصراف راننده از سفر', N'راننده سفر پت‌رسان را بعد از پذیرفتن لغو کرد؛ سفر دوباره به همه‌ی راننده‌ها نمایش داده شد.', N'/admin/trip');

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
                DECLARE @TripPushTypes TABLE
                (
                    Id bigint NOT NULL,
                    Name nvarchar(200) NOT NULL,
                    Label nvarchar(200) NOT NULL,
                    Url nvarchar(300) NOT NULL,
                    Tag nvarchar(200) NOT NULL
                );

                INSERT INTO @TripPushTypes (Id, Name, Label, Url, Tag)
                VALUES
                    (54, N'سفر پت‌رسان جدید در دسترس', N'PushTripRequestAvailable', N'/driverProfile/suggestedTrips', N'trip-request-available'),
                    (55, N'انصراف راننده از سفر', N'PushTripDriverCanceled', N'/trip', N'trip-driver-canceled');

                IF EXISTS
                (
                    SELECT 1
                    FROM @TripPushTypes source
                    INNER JOIN PushTypes target ON target.Id = source.Id
                    WHERE target.Label <> source.Label
                )
                    THROW 51000, 'A trip push type ID is already assigned to another label.', 1;

                IF EXISTS
                (
                    SELECT 1
                    FROM @TripPushTypes source
                    INNER JOIN PushTypes target ON target.Label = source.Label
                    WHERE target.Id <> source.Id
                )
                    THROW 51000, 'A trip push type label is already assigned to another ID.', 1;

                SET IDENTITY_INSERT PushTypes ON;

                INSERT INTO PushTypes (Id, Name, Label)
                SELECT source.Id, source.Name, source.Label
                FROM @TripPushTypes source
                WHERE NOT EXISTS (SELECT 1 FROM PushTypes target WHERE target.Id = source.Id);

                SET IDENTITY_INSERT PushTypes OFF;

                INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                SELECT source.Id, N'Push_Title', source.Label, source.Url, NULL, source.Tag, 1
                FROM @TripPushTypes source
                WHERE NOT EXISTS
                (
                    SELECT 1 FROM PushPatterns pattern WHERE pattern.PushTypeId = source.Id
                );

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId BETWEEN 54 AND 55
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
                WHERE pattern.PushTypeId BETWEEN 54 AND 55;

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId BETWEEN 54 AND 55;

                DELETE FROM PushPatterns WHERE PushTypeId BETWEEN 54 AND 55;
                DELETE FROM PushTypes WHERE Id BETWEEN 54 AND 55;
                """);
        }
    }
}
