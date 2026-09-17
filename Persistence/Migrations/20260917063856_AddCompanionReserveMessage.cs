using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanionReserveMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanionReserveMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanionReserveId = table.Column<long>(type: "bigint", nullable: false),
                    SenderUserId = table.Column<long>(type: "bigint", nullable: true),
                    CompanionReserveMessageTypeId = table.Column<long>(type: "bigint", nullable: false),
                    ReplyToMessageId = table.Column<long>(type: "bigint", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionReserveMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionReserveMessages_Codes_CompanionReserveMessageTypeId",
                        column: x => x.CompanionReserveMessageTypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionReserveMessages_CompanionReserveMessages_ReplyToMessageId",
                        column: x => x.ReplyToMessageId,
                        principalTable: "CompanionReserveMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CompanionReserveMessages_CompanionReserves_CompanionReserveId",
                        column: x => x.CompanionReserveId,
                        principalTable: "CompanionReserves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionReserveMessages_Users_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CompanionReserveMessageAttachments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanionReserveMessageId = table.Column<long>(type: "bigint", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: true),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionReserveMessageAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionReserveMessageAttachments_CompanionReserveMessages_CompanionReserveMessageId",
                        column: x => x.CompanionReserveMessageId,
                        principalTable: "CompanionReserveMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanionReserveMessageReactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanionReserveMessageId = table.Column<long>(type: "bigint", nullable: false),
                    ReactorUserId = table.Column<long>(type: "bigint", nullable: false),
                    Reaction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionReserveMessageReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionReserveMessageReactions_CompanionReserveMessages_CompanionReserveMessageId",
                        column: x => x.CompanionReserveMessageId,
                        principalTable: "CompanionReserveMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionReserveMessageReactions_Users_ReactorUserId",
                        column: x => x.ReactorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserveMessageAttachments_CompanionReserveMessageId",
                table: "CompanionReserveMessageAttachments",
                column: "CompanionReserveMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserveMessageReactions_CompanionReserveMessageId",
                table: "CompanionReserveMessageReactions",
                column: "CompanionReserveMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserveMessageReactions_ReactorUserId",
                table: "CompanionReserveMessageReactions",
                column: "ReactorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserveMessages_CompanionReserveId",
                table: "CompanionReserveMessages",
                column: "CompanionReserveId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserveMessages_CompanionReserveMessageTypeId",
                table: "CompanionReserveMessages",
                column: "CompanionReserveMessageTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserveMessages_ReplyToMessageId",
                table: "CompanionReserveMessages",
                column: "ReplyToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserveMessages_SenderUserId",
                table: "CompanionReserveMessages",
                column: "SenderUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanionReserveMessageAttachments");

            migrationBuilder.DropTable(
                name: "CompanionReserveMessageReactions");

            migrationBuilder.DropTable(
                name: "CompanionReserveMessages");
        }
    }
}
