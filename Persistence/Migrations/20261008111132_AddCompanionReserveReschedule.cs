using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanionReserveReschedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanionReserveReschedules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanionReserveId = table.Column<long>(type: "bigint", nullable: false),
                    OldDoDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OldCompanionTimeId = table.Column<long>(type: "bigint", nullable: true),
                    NewDoDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NewCompanionTimeId = table.Column<long>(type: "bigint", nullable: true),
                    OldStartAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NewStartAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActorKind = table.Column<int>(type: "int", nullable: false),
                    ActorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LinkedTripId = table.Column<long>(type: "bigint", nullable: true),
                    UserNotified = table.Column<bool>(type: "bit", nullable: false),
                    DriverNotified = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionReserveReschedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionReserveReschedules_CompanionReserves_CompanionReserveId",
                        column: x => x.CompanionReserveId,
                        principalTable: "CompanionReserves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserveReschedules_CompanionReserveId",
                table: "CompanionReserveReschedules",
                column: "CompanionReserveId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanionReserveReschedules");
        }
    }
}
