using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductOrderUserDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserReceiveNote",
                table: "ProductOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UserReceived",
                table: "ProductOrders",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UserReceivedDate",
                table: "ProductOrders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserReceiveNote",
                table: "ProductOrders");

            migrationBuilder.DropColumn(
                name: "UserReceived",
                table: "ProductOrders");

            migrationBuilder.DropColumn(
                name: "UserReceivedDate",
                table: "ProductOrders");
        }
    }
}
