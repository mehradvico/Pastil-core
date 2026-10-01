using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPansionSchoolSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteDate",
                table: "Schools",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Schools",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Schools",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteDate",
                table: "Pansions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Pansions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Pansions",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Pansions");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Pansions");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Pansions");
        }
    }
}
