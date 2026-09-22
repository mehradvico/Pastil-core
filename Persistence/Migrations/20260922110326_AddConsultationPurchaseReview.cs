using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConsultationPurchaseReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rate",
                table: "ConsultationPurchases",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewText",
                table: "ConsultationPurchases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "ConsultationPurchases",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rate",
                table: "ConsultationPurchases");

            migrationBuilder.DropColumn(
                name: "ReviewText",
                table: "ConsultationPurchases");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "ConsultationPurchases");
        }
    }
}
