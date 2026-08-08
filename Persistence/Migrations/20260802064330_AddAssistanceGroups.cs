using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAssistanceGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AssistanceGroupId",
                table: "Assistances",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssistanceGroups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistanceGroups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assistances_AssistanceGroupId",
                table: "Assistances",
                column: "AssistanceGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assistances_AssistanceGroups_AssistanceGroupId",
                table: "Assistances",
                column: "AssistanceGroupId",
                principalTable: "AssistanceGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assistances_AssistanceGroups_AssistanceGroupId",
                table: "Assistances");

            migrationBuilder.DropTable(
                name: "AssistanceGroups");

            migrationBuilder.DropIndex(
                name: "IX_Assistances_AssistanceGroupId",
                table: "Assistances");

            migrationBuilder.DropColumn(
                name: "AssistanceGroupId",
                table: "Assistances");
        }
    }
}
