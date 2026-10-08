using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPansionReserveOwnerApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OwnerApprovalDeadline",
                table: "PansionReserves",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OwnerDecision",
                table: "PansionReserves",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "OwnerDecisionDate",
                table: "PansionReserves",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerDecisionReason",
                table: "PansionReserves",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerApprovalDeadline",
                table: "PansionReserves");

            migrationBuilder.DropColumn(
                name: "OwnerDecision",
                table: "PansionReserves");

            migrationBuilder.DropColumn(
                name: "OwnerDecisionDate",
                table: "PansionReserves");

            migrationBuilder.DropColumn(
                name: "OwnerDecisionReason",
                table: "PansionReserves");
        }
    }
}
