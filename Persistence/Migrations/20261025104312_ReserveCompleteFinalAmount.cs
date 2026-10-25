using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReserveCompleteFinalAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompleteDetail",
                table: "GroomerReserves",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CompletedByUserId",
                table: "GroomerReserves",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DebtAmount",
                table: "GroomerReserves",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "DebtPaidByWallet",
                table: "GroomerReserves",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DebtPaidDate",
                table: "GroomerReserves",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FinalAmount",
                table: "GroomerReserves",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotReceived",
                table: "GroomerReserves",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "ReceivedAtVenue",
                table: "GroomerReserves",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "CompleteDetail",
                table: "CoachReserves",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CompletedByUserId",
                table: "CoachReserves",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DebtAmount",
                table: "CoachReserves",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "DebtPaidByWallet",
                table: "CoachReserves",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DebtPaidDate",
                table: "CoachReserves",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FinalAmount",
                table: "CoachReserves",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotReceived",
                table: "CoachReserves",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "ReceivedAtVenue",
                table: "CoachReserves",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "CompleteDetail",
                table: "ClinicReserves",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CompletedByUserId",
                table: "ClinicReserves",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DebtAmount",
                table: "ClinicReserves",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "DebtPaidByWallet",
                table: "ClinicReserves",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DebtPaidDate",
                table: "ClinicReserves",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FinalAmount",
                table: "ClinicReserves",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotReceived",
                table: "ClinicReserves",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "ReceivedAtVenue",
                table: "ClinicReserves",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompleteDetail",
                table: "GroomerReserves");

            migrationBuilder.DropColumn(
                name: "CompletedByUserId",
                table: "GroomerReserves");

            migrationBuilder.DropColumn(
                name: "DebtAmount",
                table: "GroomerReserves");

            migrationBuilder.DropColumn(
                name: "DebtPaidByWallet",
                table: "GroomerReserves");

            migrationBuilder.DropColumn(
                name: "DebtPaidDate",
                table: "GroomerReserves");

            migrationBuilder.DropColumn(
                name: "FinalAmount",
                table: "GroomerReserves");

            migrationBuilder.DropColumn(
                name: "NotReceived",
                table: "GroomerReserves");

            migrationBuilder.DropColumn(
                name: "ReceivedAtVenue",
                table: "GroomerReserves");

            migrationBuilder.DropColumn(
                name: "CompleteDetail",
                table: "CoachReserves");

            migrationBuilder.DropColumn(
                name: "CompletedByUserId",
                table: "CoachReserves");

            migrationBuilder.DropColumn(
                name: "DebtAmount",
                table: "CoachReserves");

            migrationBuilder.DropColumn(
                name: "DebtPaidByWallet",
                table: "CoachReserves");

            migrationBuilder.DropColumn(
                name: "DebtPaidDate",
                table: "CoachReserves");

            migrationBuilder.DropColumn(
                name: "FinalAmount",
                table: "CoachReserves");

            migrationBuilder.DropColumn(
                name: "NotReceived",
                table: "CoachReserves");

            migrationBuilder.DropColumn(
                name: "ReceivedAtVenue",
                table: "CoachReserves");

            migrationBuilder.DropColumn(
                name: "CompleteDetail",
                table: "ClinicReserves");

            migrationBuilder.DropColumn(
                name: "CompletedByUserId",
                table: "ClinicReserves");

            migrationBuilder.DropColumn(
                name: "DebtAmount",
                table: "ClinicReserves");

            migrationBuilder.DropColumn(
                name: "DebtPaidByWallet",
                table: "ClinicReserves");

            migrationBuilder.DropColumn(
                name: "DebtPaidDate",
                table: "ClinicReserves");

            migrationBuilder.DropColumn(
                name: "FinalAmount",
                table: "ClinicReserves");

            migrationBuilder.DropColumn(
                name: "NotReceived",
                table: "ClinicReserves");

            migrationBuilder.DropColumn(
                name: "ReceivedAtVenue",
                table: "ClinicReserves");
        }
    }
}
