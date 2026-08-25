using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPetRasanTripProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CompanionReserveId",
                table: "Trips",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReturnLeg",
                table: "Trips",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OwnerRidesAlong",
                table: "Trips",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ProgressStageId",
                table: "Trips",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProgressUpdateDate",
                table: "Trips",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledDepartureAt",
                table: "Trips",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ScheduledDispatched",
                table: "Trips",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ScheduledLeadMinutes",
                table: "Trips",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TripPets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TripId = table.Column<long>(type: "bigint", nullable: false),
                    UserPetId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripPets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripPets_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TripPets_UserPets_UserPetId",
                        column: x => x.UserPetId,
                        principalTable: "UserPets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trips_CompanionReserveId",
                table: "Trips",
                column: "CompanionReserveId");

            migrationBuilder.CreateIndex(
                name: "IX_TripPets_TripId",
                table: "TripPets",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_TripPets_UserPetId",
                table: "TripPets",
                column: "UserPetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_CompanionReserves_CompanionReserveId",
                table: "Trips",
                column: "CompanionReserveId",
                principalTable: "CompanionReserves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                    (N'Trip.PetPickedUp', N'تحویل پت توسط راننده', N'راننده پت را برای سفر پت‌رسان تحویل گرفت.', N'/admin/trip'),
                    (N'Trip.ArrivedDestination', N'رسیدن به مقصد', N'سفر پت‌رسان به مقصد رسید.', N'/admin/trip');

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
                    (49, N'رسیدن راننده به مبدا', N'PushTripArrivedOrigin', N'/trip', N'trip-arrived-origin'),
                    (50, N'تحویل پت توسط راننده', N'PushTripPetPickedUp', N'/trip', N'trip-pet-picked-up'),
                    (51, N'رسیدن به مقصد', N'PushTripArrivedDestination', N'/trip', N'trip-arrived-destination'),
                    (52, N'تکمیل سفر پت‌رسان', N'PushTripCompleted', N'/trip', N'trip-completed'),
                    (53, N'لغو سفر پت‌رسان', N'PushTripCanceled', N'/trip', N'trip-canceled');

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
                WHERE pattern.PushTypeId BETWEEN 49 AND 53
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
                WHERE pattern.PushTypeId BETWEEN 49 AND 53;

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId BETWEEN 49 AND 53;

                DELETE FROM PushPatterns WHERE PushTypeId BETWEEN 49 AND 53;
                DELETE FROM PushTypes WHERE Id BETWEEN 49 AND 53;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_CompanionReserves_CompanionReserveId",
                table: "Trips");

            migrationBuilder.DropTable(
                name: "TripPets");

            migrationBuilder.DropIndex(
                name: "IX_Trips_CompanionReserveId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "CompanionReserveId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "IsReturnLeg",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "OwnerRidesAlong",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "ProgressStageId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "ProgressUpdateDate",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "ScheduledDepartureAt",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "ScheduledDispatched",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "ScheduledLeadMinutes",
                table: "Trips");
        }
    }
}
