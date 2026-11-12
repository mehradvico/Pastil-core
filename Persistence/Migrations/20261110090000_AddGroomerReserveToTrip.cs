using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations
{
    [DbContext(typeof(DataBaseContext))]
    [Migration("20261110090000_AddGroomerReserveToTrip")]
    public partial class AddGroomerReserveToTrip : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(name: "GroomerReserveId", table: "Trips", type: "bigint", nullable: true);
            migrationBuilder.CreateIndex(name: "IX_Trips_GroomerReserveId", table: "Trips", column: "GroomerReserveId");
            migrationBuilder.AddForeignKey(name: "FK_Trips_GroomerReserves_GroomerReserveId", table: "Trips", column: "GroomerReserveId", principalTable: "GroomerReserves", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Trips_GroomerReserves_GroomerReserveId", table: "Trips");
            migrationBuilder.DropIndex(name: "IX_Trips_GroomerReserveId", table: "Trips");
            migrationBuilder.DropColumn(name: "GroomerReserveId", table: "Trips");
        }
    }
}
