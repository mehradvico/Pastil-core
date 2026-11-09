using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PansionPetRates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "PansionPrice",
                table: "PansionPets",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PetSize",
                table: "PansionPets",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SchoolPrice",
                table: "PansionPets",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PansionPrice",
                table: "PansionPets");

            migrationBuilder.DropColumn(
                name: "PetSize",
                table: "PansionPets");

            migrationBuilder.DropColumn(
                name: "SchoolPrice",
                table: "PansionPets");
        }
    }
}
