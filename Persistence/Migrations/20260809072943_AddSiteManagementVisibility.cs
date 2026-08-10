using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteManagementVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowToSite",
                table: "Stores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowToSite",
                table: "Pansions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowToSite",
                table: "Companions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowToApp",
                table: "Banners",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowToSite",
                table: "Banners",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowToSite",
                table: "Assistances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE [Banners] SET [ShowToApp] = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowToSite",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "ShowToSite",
                table: "Pansions");

            migrationBuilder.DropColumn(
                name: "ShowToSite",
                table: "Companions");

            migrationBuilder.DropColumn(
                name: "ShowToApp",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "ShowToSite",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "ShowToSite",
                table: "Assistances");
        }
    }
}
