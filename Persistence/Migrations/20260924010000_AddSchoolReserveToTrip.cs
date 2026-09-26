using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations
{
    [DbContext(typeof(DataBaseContext))]
    [Migration("20260924010000_AddSchoolReserveToTrip")]
    public partial class AddSchoolReserveToTrip : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(name: "SchoolReserveId", table: "Trips", type: "bigint", nullable: true);
            migrationBuilder.CreateIndex(name: "IX_Trips_SchoolReserveId", table: "Trips", column: "SchoolReserveId");
            migrationBuilder.AddForeignKey(name: "FK_Trips_SchoolReserves_SchoolReserveId", table: "Trips", column: "SchoolReserveId", principalTable: "SchoolReserves", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Trips_SchoolReserves_SchoolReserveId", table: "Trips");
            migrationBuilder.DropIndex(name: "IX_Trips_SchoolReserveId", table: "Trips");
            migrationBuilder.DropColumn(name: "SchoolReserveId", table: "Trips");
        }
    }
}
