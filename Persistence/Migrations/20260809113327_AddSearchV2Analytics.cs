using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchV2Analytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SearchQueryLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Query = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedQuery = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ResultCount = table.Column<int>(type: "int", nullable: false),
                    TookMilliseconds = table.Column<long>(type: "bigint", nullable: false),
                    CreateDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchQueryLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchQueryLogs_Channel_CreateDateUtc",
                table: "SearchQueryLogs",
                columns: new[] { "Channel", "CreateDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SearchQueryLogs_CreateDateUtc",
                table: "SearchQueryLogs",
                column: "CreateDateUtc");

            migrationBuilder.CreateIndex(
                name: "IX_SearchQueryLogs_NormalizedQuery",
                table: "SearchQueryLogs",
                column: "NormalizedQuery");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchQueryLogs");
        }
    }
}
