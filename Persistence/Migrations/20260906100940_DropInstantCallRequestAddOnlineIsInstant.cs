using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropInstantCallRequestAddOnlineIsInstant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanionInstantCallRequests");

            migrationBuilder.AddColumn<bool>(
                name: "IsInstant",
                table: "CompanionAssistancePackageOnlines",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // گزینه‌های «فوری» کاتالوگ خدمات آنلاین (چت/تماس‌داخل‌برنامه/تماس‌شخصی/ویدیوکال فوری با مربی) با نام مشخص می‌شوند،
            // نه با شناسه ثابت - چون شناسه‌ها ممکن است بین محیط‌ها فرق کند.
            migrationBuilder.Sql("""
                UPDATE CompanionAssistancePackageOnlines
                SET IsInstant = 1
                WHERE Name IN (
                    N'چت فوری با مربی',
                    N'تماس فوری داخل برنامه با مربی',
                    N'تماس فوری با شماره شخصی مربی',
                    N'ویدیو کال فوری با مربی'
                );
                """);

            // پاکسازی PushType مربوط به فیچر قبلی «درخواست تماس فوری مستقل» که با معماری رزرو واقعی جایگزین شد.
            migrationBuilder.Sql("""
                DELETE notification
                FROM PushNotifications notification
                INNER JOIN PushPatterns pattern ON pattern.Id = notification.PushPatternId
                WHERE pattern.PushTypeId = 62;

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId = 62;

                DELETE FROM PushPatterns WHERE PushTypeId = 62;
                DELETE FROM PushTypes
                WHERE Id = 62
                  AND Label = N'PushInstantCallRequestCompanion';
                """);

            // یادآورهای رزرو آنلاین (زمان‌بندی‌شده و فوری) از این پس شناسه‌ی رزرو را در token4 حمل می‌کنند تا
            // نماینده با زدن پوش مستقیم به جزئیات همان رزرو (و دکمه‌ی تماس) هدایت شود.
            migrationBuilder.Sql("""
                UPDATE PushPatterns SET Url = N'/operator?reserveId={3}' WHERE PushTypeId IN (60, 61);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE PushPatterns SET Url = N'/operator' WHERE PushTypeId IN (60, 61);
                """);

            migrationBuilder.Sql("""
                SET IDENTITY_INSERT PushTypes ON;

                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 62)
                    INSERT INTO PushTypes (Id, Name, Label)
                    VALUES (62, N'درخواست تماس فوری برای نماینده', N'PushInstantCallRequestCompanion');

                SET IDENTITY_INSERT PushTypes OFF;

                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 62)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES
                    (
                        62,
                        N'درخواست تماس فوری',
                        N'کاربر {0} برای «{1}» درخواست تماس فوری داد. برای مشاهده و تماس ضربه بزنید.',
                        N'/operator?instantCallRequestId={2}',
                        NULL,
                        N'instant-call-request-companion',
                        1
                    );

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId = 62
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM PushSettings setting
                      WHERE setting.PushPatternId = pattern.Id
                  );
                """);

            migrationBuilder.DropColumn(
                name: "IsInstant",
                table: "CompanionAssistancePackageOnlines");

            migrationBuilder.CreateTable(
                name: "CompanionInstantCallRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookerId = table.Column<long>(type: "bigint", nullable: false),
                    CompanionAssistancePackageOnlineSelectionId = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionInstantCallRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionInstantCallRequests_CompanionAssistancePackageOnlineSelections_CompanionAssistancePackageOnlineSelectionId",
                        column: x => x.CompanionAssistancePackageOnlineSelectionId,
                        principalTable: "CompanionAssistancePackageOnlineSelections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionInstantCallRequests_Users_BookerId",
                        column: x => x.BookerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanionInstantCallRequests_BookerId",
                table: "CompanionInstantCallRequests",
                column: "BookerId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionInstantCallRequests_CompanionAssistancePackageOnlineSelectionId",
                table: "CompanionInstantCallRequests",
                column: "CompanionAssistancePackageOnlineSelectionId");
        }
    }
}
