using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPetResanService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PetResanServiceScheduleId",
                table: "Trips",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PetResanServices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    UserPetId = table.Column<long>(type: "bigint", nullable: false),
                    Origin = table.Column<Point>(type: "geography", nullable: true),
                    Destination = table.Column<Point>(type: "geography", nullable: true),
                    FromAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalWeeks = table.Column<int>(type: "int", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PricePerOccurrence = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetResanServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PetResanServices_UserPets_UserPetId",
                        column: x => x.UserPetId,
                        principalTable: "UserPets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PetResanServices_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PetResanServiceSchedules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PetResanServiceId = table.Column<long>(type: "bigint", nullable: false),
                    WeekDayId = table.Column<long>(type: "bigint", nullable: false),
                    Time = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetResanServiceSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PetResanServiceSchedules_PetResanServices_PetResanServiceId",
                        column: x => x.PetResanServiceId,
                        principalTable: "PetResanServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PetResanServiceSchedules_WeekDays_WeekDayId",
                        column: x => x.WeekDayId,
                        principalTable: "WeekDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trips_PetResanServiceScheduleId",
                table: "Trips",
                column: "PetResanServiceScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_PetResanServices_UserId",
                table: "PetResanServices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PetResanServices_UserPetId",
                table: "PetResanServices",
                column: "UserPetId");

            migrationBuilder.CreateIndex(
                name: "IX_PetResanServiceSchedules_PetResanServiceId",
                table: "PetResanServiceSchedules",
                column: "PetResanServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PetResanServiceSchedules_WeekDayId",
                table: "PetResanServiceSchedules",
                column: "WeekDayId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_PetResanServiceSchedules_PetResanServiceScheduleId",
                table: "Trips",
                column: "PetResanServiceScheduleId",
                principalTable: "PetResanServiceSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trips_PetResanServiceSchedules_PetResanServiceScheduleId",
                table: "Trips");

            migrationBuilder.DropTable(
                name: "PetResanServiceSchedules");

            migrationBuilder.DropTable(
                name: "PetResanServices");

            migrationBuilder.DropIndex(
                name: "IX_Trips_PetResanServiceScheduleId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "PetResanServiceScheduleId",
                table: "Trips");
        }
    }
}
