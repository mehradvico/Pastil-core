using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTripCancellationAndDriverExclusion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CancelInitiatorId",
                table: "Trips",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CancelReasonCodeId",
                table: "Trips",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelReasonDetail",
                table: "Trips",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PreviousTripId",
                table: "Trips",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TripDriverExclusions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TripId = table.Column<long>(type: "bigint", nullable: false),
                    DriverId = table.Column<long>(type: "bigint", nullable: false),
                    ReasonId = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripDriverExclusions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripDriverExclusions_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TripDriverExclusions_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trips_CancelReasonCodeId",
                table: "Trips",
                column: "CancelReasonCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_PreviousTripId",
                table: "Trips",
                column: "PreviousTripId");

            migrationBuilder.CreateIndex(
                name: "IX_TripDriverExclusions_DriverId",
                table: "TripDriverExclusions",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_TripDriverExclusions_TripId",
                table: "TripDriverExclusions",
                column: "TripId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Codes_CancelReasonCodeId",
                table: "Trips",
                column: "CancelReasonCodeId",
                principalTable: "Codes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Trips_PreviousTripId",
                table: "Trips",
                column: "PreviousTripId",
                principalTable: "Trips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Codes_CancelReasonCodeId",
                table: "Trips");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Trips_PreviousTripId",
                table: "Trips");

            migrationBuilder.DropTable(
                name: "TripDriverExclusions");

            migrationBuilder.DropIndex(
                name: "IX_Trips_CancelReasonCodeId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trips_PreviousTripId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "CancelInitiatorId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "CancelReasonCodeId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "CancelReasonDetail",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "PreviousTripId",
                table: "Trips");
        }
    }
}
