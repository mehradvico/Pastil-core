using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConsultationNamedPackagesAndAccounting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ConsultationPackages_CompanionId_ChannelId_DurationMinutes",
                table: "ConsultationPackages");

            migrationBuilder.AddColumn<long>(
                name: "ConsultationPurchaseId",
                table: "SettlementCompanions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CallEndDate",
                table: "OnlineSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CallSeconds",
                table: "OnlineSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CallStartDate",
                table: "OnlineSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PackageName",
                table: "ConsultationPurchases",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Permitted",
                table: "ConsultationPurchases",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ConsultationPackages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ConsultationPackages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PictureId",
                table: "ConsultationPackages",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "ConsultationPackages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SettlementCompanions_ConsultationPurchaseId",
                table: "SettlementCompanions",
                column: "ConsultationPurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPackages_CompanionId_ChannelId",
                table: "ConsultationPackages",
                columns: new[] { "CompanionId", "ChannelId" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPackages_PictureId",
                table: "ConsultationPackages",
                column: "PictureId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultationPackages_Pictures_PictureId",
                table: "ConsultationPackages",
                column: "PictureId",
                principalTable: "Pictures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SettlementCompanions_ConsultationPurchases_ConsultationPurchaseId",
                table: "SettlementCompanions",
                column: "ConsultationPurchaseId",
                principalTable: "ConsultationPurchases",
                principalColumn: "Id");

            // پکیج‌های قبلی (ماتریس کانال × مدت) نام پیش‌فرض می‌گیرند تا نام خالی نمایش داده نشود؛ مثلاً «چت ۳۰ دقیقه‌ای»
            migrationBuilder.Sql(@"
UPDATE ConsultationPackages
SET Name =
    CASE ChannelId
        WHEN 1 THEN N'چت'
        WHEN 2 THEN N'تماس درون‌برنامه'
        WHEN 3 THEN N'تماس تصویری'
        WHEN 4 THEN N'تماس تلفنی'
        ELSE N'مشاوره'
    END
    + N' ' +
    CASE DurationMinutes
        WHEN 30 THEN N'۳۰'
        WHEN 60 THEN N'۶۰'
        ELSE CAST(DurationMinutes AS nvarchar(10))
    END
    + N' دقیقه‌ای'
WHERE Name IS NULL;");

            // خریدهای قبلی نام پکیجِ همان لحظه را (بر اساس پکیج فعلی) می‌گیرند
            migrationBuilder.Sql(@"
UPDATE p
SET p.PackageName = cp.Name
FROM ConsultationPurchases p
INNER JOIN ConsultationPackages cp ON cp.Id = p.ConsultationPackageId
WHERE p.PackageName IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsultationPackages_Pictures_PictureId",
                table: "ConsultationPackages");

            migrationBuilder.DropForeignKey(
                name: "FK_SettlementCompanions_ConsultationPurchases_ConsultationPurchaseId",
                table: "SettlementCompanions");

            migrationBuilder.DropIndex(
                name: "IX_SettlementCompanions_ConsultationPurchaseId",
                table: "SettlementCompanions");

            migrationBuilder.DropIndex(
                name: "IX_ConsultationPackages_CompanionId_ChannelId",
                table: "ConsultationPackages");

            migrationBuilder.DropIndex(
                name: "IX_ConsultationPackages_PictureId",
                table: "ConsultationPackages");

            migrationBuilder.DropColumn(
                name: "ConsultationPurchaseId",
                table: "SettlementCompanions");

            migrationBuilder.DropColumn(
                name: "CallEndDate",
                table: "OnlineSessions");

            migrationBuilder.DropColumn(
                name: "CallSeconds",
                table: "OnlineSessions");

            migrationBuilder.DropColumn(
                name: "CallStartDate",
                table: "OnlineSessions");

            migrationBuilder.DropColumn(
                name: "PackageName",
                table: "ConsultationPurchases");

            migrationBuilder.DropColumn(
                name: "Permitted",
                table: "ConsultationPurchases");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ConsultationPackages");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ConsultationPackages");

            migrationBuilder.DropColumn(
                name: "PictureId",
                table: "ConsultationPackages");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "ConsultationPackages");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPackages_CompanionId_ChannelId_DurationMinutes",
                table: "ConsultationPackages",
                columns: new[] { "CompanionId", "ChannelId", "DurationMinutes" },
                unique: true,
                filter: "[Deleted] = 0");
        }
    }
}
