using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixTicketCategoryDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "TicketCategoryId",
                table: "Tickets",
                type: "bigint",
                nullable: false,
                defaultValue: 55L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValue: 10139L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "TicketCategoryId",
                table: "Tickets",
                type: "bigint",
                nullable: false,
                defaultValue: 10139L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValue: 55L);
        }
    }
}
