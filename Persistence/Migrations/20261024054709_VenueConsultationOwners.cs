using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VenueConsultationOwners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "CompanionId",
                table: "ConsultationPurchases",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "ClinicId",
                table: "ConsultationPurchases",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CoachId",
                table: "ConsultationPurchases",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "CompanionId",
                table: "ConsultationPackages",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "ClinicId",
                table: "ConsultationPackages",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CoachId",
                table: "ConsultationPackages",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "CompanionId",
                table: "ConsultationAvailabilities",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "ClinicId",
                table: "ConsultationAvailabilities",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CoachId",
                table: "ConsultationAvailabilities",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ConsultationOnline",
                table: "CoachUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsultationOnlineExpiresAt",
                table: "CoachUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ConsultationOnline",
                table: "ClinicUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsultationOnlineExpiresAt",
                table: "ClinicUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SettlementConsultations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettlementId = table.Column<long>(type: "bigint", nullable: false),
                    ConsultationPurchaseId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementConsultations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SettlementConsultations_ConsultationPurchases_ConsultationPurchaseId",
                        column: x => x.ConsultationPurchaseId,
                        principalTable: "ConsultationPurchases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SettlementConsultations_Settlements_SettlementId",
                        column: x => x.SettlementId,
                        principalTable: "Settlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPurchases_ClinicId_ScheduledStart",
                table: "ConsultationPurchases",
                columns: new[] { "ClinicId", "ScheduledStart" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPurchases_ClinicId_Status",
                table: "ConsultationPurchases",
                columns: new[] { "ClinicId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPurchases_CoachId_ScheduledStart",
                table: "ConsultationPurchases",
                columns: new[] { "CoachId", "ScheduledStart" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPurchases_CoachId_Status",
                table: "ConsultationPurchases",
                columns: new[] { "CoachId", "Status" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ConsultationPurchases_Owner",
                table: "ConsultationPurchases",
                sql: "(CASE WHEN [CompanionId] IS NULL THEN 0 ELSE 1 END) + (CASE WHEN [ClinicId] IS NULL THEN 0 ELSE 1 END) + (CASE WHEN [CoachId] IS NULL THEN 0 ELSE 1 END) = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPackages_ClinicId_ChannelId",
                table: "ConsultationPackages",
                columns: new[] { "ClinicId", "ChannelId" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPackages_CoachId_ChannelId",
                table: "ConsultationPackages",
                columns: new[] { "CoachId", "ChannelId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ConsultationPackages_Owner",
                table: "ConsultationPackages",
                sql: "(CASE WHEN [CompanionId] IS NULL THEN 0 ELSE 1 END) + (CASE WHEN [ClinicId] IS NULL THEN 0 ELSE 1 END) + (CASE WHEN [CoachId] IS NULL THEN 0 ELSE 1 END) = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationAvailabilities_ClinicId_WeekDayId",
                table: "ConsultationAvailabilities",
                columns: new[] { "ClinicId", "WeekDayId" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationAvailabilities_CoachId_WeekDayId",
                table: "ConsultationAvailabilities",
                columns: new[] { "CoachId", "WeekDayId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ConsultationAvailabilities_Owner",
                table: "ConsultationAvailabilities",
                sql: "(CASE WHEN [CompanionId] IS NULL THEN 0 ELSE 1 END) + (CASE WHEN [ClinicId] IS NULL THEN 0 ELSE 1 END) + (CASE WHEN [CoachId] IS NULL THEN 0 ELSE 1 END) = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementConsultations_ConsultationPurchaseId",
                table: "SettlementConsultations",
                column: "ConsultationPurchaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SettlementConsultations_SettlementId",
                table: "SettlementConsultations",
                column: "SettlementId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultationAvailabilities_Clinics_ClinicId",
                table: "ConsultationAvailabilities",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultationAvailabilities_Coaches_CoachId",
                table: "ConsultationAvailabilities",
                column: "CoachId",
                principalTable: "Coaches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultationPackages_Clinics_ClinicId",
                table: "ConsultationPackages",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultationPackages_Coaches_CoachId",
                table: "ConsultationPackages",
                column: "CoachId",
                principalTable: "Coaches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultationPurchases_Clinics_ClinicId",
                table: "ConsultationPurchases",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultationPurchases_Coaches_CoachId",
                table: "ConsultationPurchases",
                column: "CoachId",
                principalTable: "Coaches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsultationAvailabilities_Clinics_ClinicId",
                table: "ConsultationAvailabilities");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsultationAvailabilities_Coaches_CoachId",
                table: "ConsultationAvailabilities");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsultationPackages_Clinics_ClinicId",
                table: "ConsultationPackages");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsultationPackages_Coaches_CoachId",
                table: "ConsultationPackages");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsultationPurchases_Clinics_ClinicId",
                table: "ConsultationPurchases");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsultationPurchases_Coaches_CoachId",
                table: "ConsultationPurchases");

            migrationBuilder.DropTable(
                name: "SettlementConsultations");

            migrationBuilder.DropIndex(
                name: "IX_ConsultationPurchases_ClinicId_ScheduledStart",
                table: "ConsultationPurchases");

            migrationBuilder.DropIndex(
                name: "IX_ConsultationPurchases_ClinicId_Status",
                table: "ConsultationPurchases");

            migrationBuilder.DropIndex(
                name: "IX_ConsultationPurchases_CoachId_ScheduledStart",
                table: "ConsultationPurchases");

            migrationBuilder.DropIndex(
                name: "IX_ConsultationPurchases_CoachId_Status",
                table: "ConsultationPurchases");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ConsultationPurchases_Owner",
                table: "ConsultationPurchases");

            migrationBuilder.DropIndex(
                name: "IX_ConsultationPackages_ClinicId_ChannelId",
                table: "ConsultationPackages");

            migrationBuilder.DropIndex(
                name: "IX_ConsultationPackages_CoachId_ChannelId",
                table: "ConsultationPackages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ConsultationPackages_Owner",
                table: "ConsultationPackages");

            migrationBuilder.DropIndex(
                name: "IX_ConsultationAvailabilities_ClinicId_WeekDayId",
                table: "ConsultationAvailabilities");

            migrationBuilder.DropIndex(
                name: "IX_ConsultationAvailabilities_CoachId_WeekDayId",
                table: "ConsultationAvailabilities");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ConsultationAvailabilities_Owner",
                table: "ConsultationAvailabilities");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "ConsultationPurchases");

            migrationBuilder.DropColumn(
                name: "CoachId",
                table: "ConsultationPurchases");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "ConsultationPackages");

            migrationBuilder.DropColumn(
                name: "CoachId",
                table: "ConsultationPackages");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "ConsultationAvailabilities");

            migrationBuilder.DropColumn(
                name: "CoachId",
                table: "ConsultationAvailabilities");

            migrationBuilder.DropColumn(
                name: "ConsultationOnline",
                table: "CoachUsers");

            migrationBuilder.DropColumn(
                name: "ConsultationOnlineExpiresAt",
                table: "CoachUsers");

            migrationBuilder.DropColumn(
                name: "ConsultationOnline",
                table: "ClinicUsers");

            migrationBuilder.DropColumn(
                name: "ConsultationOnlineExpiresAt",
                table: "ClinicUsers");

            migrationBuilder.AlterColumn<long>(
                name: "CompanionId",
                table: "ConsultationPurchases",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "CompanionId",
                table: "ConsultationPackages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "CompanionId",
                table: "ConsultationAvailabilities",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
