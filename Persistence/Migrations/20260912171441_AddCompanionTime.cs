using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanionTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CompanionTimeId",
                table: "CompanionReserves",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CompanionTimes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    WeekDayId = table.Column<long>(type: "bigint", nullable: false),
                    CompanionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionTimes_Companions_CompanionId",
                        column: x => x.CompanionId,
                        principalTable: "Companions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionTimes_WeekDays_WeekDayId",
                        column: x => x.WeekDayId,
                        principalTable: "WeekDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserves_CompanionTimeId",
                table: "CompanionReserves",
                column: "CompanionTimeId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionTimes_CompanionId",
                table: "CompanionTimes",
                column: "CompanionId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionTimes_WeekDayId",
                table: "CompanionTimes",
                column: "WeekDayId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanionReserves_CompanionTimes_CompanionTimeId",
                table: "CompanionReserves",
                column: "CompanionTimeId",
                principalTable: "CompanionTimes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanionReserves_CompanionTimes_CompanionTimeId",
                table: "CompanionReserves");

            migrationBuilder.DropTable(
                name: "CompanionTimes");

            migrationBuilder.DropIndex(
                name: "IX_CompanionReserves_CompanionTimeId",
                table: "CompanionReserves");

            migrationBuilder.DropColumn(
                name: "CompanionTimeId",
                table: "CompanionReserves");
        }
    }
}
