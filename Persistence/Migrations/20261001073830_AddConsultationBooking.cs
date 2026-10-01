using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConsultationBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledEnd",
                table: "ConsultationPurchases",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledStart",
                table: "ConsultationPurchases",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Bookable",
                table: "ConsultationPackages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ConsultationAvailabilities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanionId = table.Column<long>(type: "bigint", nullable: false),
                    WeekDayId = table.Column<int>(type: "int", nullable: false),
                    StartMinute = table.Column<int>(type: "int", nullable: false),
                    EndMinute = table.Column<int>(type: "int", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsultationAvailabilities_Companions_CompanionId",
                        column: x => x.CompanionId,
                        principalTable: "Companions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPurchases_CompanionId_ScheduledStart",
                table: "ConsultationPurchases",
                columns: new[] { "CompanionId", "ScheduledStart" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationAvailabilities_CompanionId_WeekDayId",
                table: "ConsultationAvailabilities",
                columns: new[] { "CompanionId", "WeekDayId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsultationAvailabilities");

            migrationBuilder.DropIndex(
                name: "IX_ConsultationPurchases_CompanionId_ScheduledStart",
                table: "ConsultationPurchases");

            migrationBuilder.DropColumn(
                name: "ScheduledEnd",
                table: "ConsultationPurchases");

            migrationBuilder.DropColumn(
                name: "ScheduledStart",
                table: "ConsultationPurchases");

            migrationBuilder.DropColumn(
                name: "Bookable",
                table: "ConsultationPackages");
        }
    }
}
