using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanionReserveBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "BatchId",
                table: "CompanionReserves",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CompanionReserveBatches",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionReserveBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionReserveBatches_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserves_BatchId",
                table: "CompanionReserves",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserveBatches_UserId",
                table: "CompanionReserveBatches",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanionReserves_CompanionReserveBatches_BatchId",
                table: "CompanionReserves",
                column: "BatchId",
                principalTable: "CompanionReserveBatches",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanionReserves_CompanionReserveBatches_BatchId",
                table: "CompanionReserves");

            migrationBuilder.DropTable(
                name: "CompanionReserveBatches");

            migrationBuilder.DropIndex(
                name: "IX_CompanionReserves_BatchId",
                table: "CompanionReserves");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "CompanionReserves");
        }
    }
}
