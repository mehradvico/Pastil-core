using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_locationuodate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<MultiPolygon>(
                name: "Boundary",
                table: "States",
                type: "geography",
                nullable: true);

            migrationBuilder.AddColumn<MultiPolygon>(
                name: "Boundary",
                table: "Neighborhoods",
                type: "geography",
                nullable: true);

            migrationBuilder.AddColumn<MultiPolygon>(
                name: "Boundary",
                table: "Cities",
                type: "geography",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Boundary",
                table: "States");

            migrationBuilder.DropColumn(
                name: "Boundary",
                table: "Neighborhoods");

            migrationBuilder.DropColumn(
                name: "Boundary",
                table: "Cities");
        }
    }
}
