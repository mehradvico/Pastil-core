using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations
{
    // پت‌رسانِ همراه رزرو: اتصال سفر به رزرو مربی + پرچم «هزینه‌ی سفر همراه رزرو پرداخت می‌شود».
    [DbContext(typeof(DataBaseContext))]
    [Migration("20261111090000_ReservePetResanBundle")]
    public partial class ReservePetResanBundle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(name: "CoachReserveId", table: "Trips", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<bool>(name: "PaidWithReserve", table: "Trips", type: "bit", nullable: false, defaultValue: false);
            migrationBuilder.CreateIndex(name: "IX_Trips_CoachReserveId", table: "Trips", column: "CoachReserveId");
            migrationBuilder.AddForeignKey(name: "FK_Trips_CoachReserves_CoachReserveId", table: "Trips", column: "CoachReserveId", principalTable: "CoachReserves", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Trips_CoachReserves_CoachReserveId", table: "Trips");
            migrationBuilder.DropIndex(name: "IX_Trips_CoachReserveId", table: "Trips");
            migrationBuilder.DropColumn(name: "PaidWithReserve", table: "Trips");
            migrationBuilder.DropColumn(name: "CoachReserveId", table: "Trips");
        }
    }
}
