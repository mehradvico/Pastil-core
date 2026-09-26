using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReserveOperatorUnpaidDebt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OperatorDebtPaidByWallet",
                table: "CompanionReserves",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OperatorDebtPaidDate",
                table: "CompanionReserves",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OperatorUnpaid",
                table: "CompanionReserves",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "OperatorUnpaidAmount",
                table: "CompanionReserves",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "OperatorUnpaidDate",
                table: "CompanionReserves",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OperatorDebtPaidByWallet",
                table: "CompanionReserves");

            migrationBuilder.DropColumn(
                name: "OperatorDebtPaidDate",
                table: "CompanionReserves");

            migrationBuilder.DropColumn(
                name: "OperatorUnpaid",
                table: "CompanionReserves");

            migrationBuilder.DropColumn(
                name: "OperatorUnpaidAmount",
                table: "CompanionReserves");

            migrationBuilder.DropColumn(
                name: "OperatorUnpaidDate",
                table: "CompanionReserves");
        }
    }
}
