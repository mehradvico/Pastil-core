using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_updatestoryitemlike : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserStoryLikes");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "StoryItems");

            migrationBuilder.CreateTable(
                name: "StoryUserLikes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoryItemId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryUserLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoryUserLikes_StoryItems_StoryItemId",
                        column: x => x.StoryItemId,
                        principalTable: "StoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StoryUserLikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoryItems_StoreId",
                table: "StoryItems",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_StoryUserLikes_StoryItemId",
                table: "StoryUserLikes",
                column: "StoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StoryUserLikes_UserId",
                table: "StoryUserLikes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoryItems_Stores_StoreId",
                table: "StoryItems",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoryItems_Stores_StoreId",
                table: "StoryItems");

            migrationBuilder.DropTable(
                name: "StoryUserLikes");

            migrationBuilder.DropIndex(
                name: "IX_StoryItems_StoreId",
                table: "StoryItems");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "StoryItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserStoryLikes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoryItemId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    IsLiked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStoryLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserStoryLikes_StoryItems_StoryItemId",
                        column: x => x.StoryItemId,
                        principalTable: "StoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserStoryLikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserStoryLikes_StoryItemId",
                table: "UserStoryLikes",
                column: "StoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStoryLikes_UserId",
                table: "UserStoryLikes",
                column: "UserId");
        }
    }
}
