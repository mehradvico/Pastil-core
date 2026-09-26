using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations
{
    [DbContext(typeof(DataBaseContext))]
    [Migration("20260924000000_AddPetResanServiceTripOptions")]
    public partial class AddPetResanServiceTripOptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PetResanServiceTripOption",
                columns: table => new
                {
                    PetResanServicesId = table.Column<long>(type: "bigint", nullable: false),
                    TripOptionsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetResanServiceTripOption", x => new { x.PetResanServicesId, x.TripOptionsId });
                    table.ForeignKey(
                        name: "FK_PetResanServiceTripOption_PetResanServices_PetResanServicesId",
                        column: x => x.PetResanServicesId,
                        principalTable: "PetResanServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PetResanServiceTripOption_TripOptions_TripOptionsId",
                        column: x => x.TripOptionsId,
                        principalTable: "TripOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PetResanServiceTripOption_TripOptionsId",
                table: "PetResanServiceTripOption",
                column: "TripOptionsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PetResanServiceTripOption");
        }
    }
}
