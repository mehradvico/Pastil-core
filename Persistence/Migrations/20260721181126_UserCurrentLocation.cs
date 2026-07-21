using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UserCurrentLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserCurrentLocations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Location = table.Column<Point>(type: "geography", nullable: false),
                    CityId = table.Column<long>(type: "bigint", nullable: false),
                    NeighborhoodId = table.Column<long>(type: "bigint", nullable: true),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCurrentLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCurrentLocations_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserCurrentLocations_Neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserCurrentLocations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCurrentLocations_CityId",
                table: "UserCurrentLocations",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCurrentLocations_NeighborhoodId",
                table: "UserCurrentLocations",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCurrentLocations_UserId",
                table: "UserCurrentLocations",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCurrentLocations");
        }
    }
}
