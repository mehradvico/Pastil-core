using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_updatepush10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PushTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PushPatterns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PushTypeId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushPatterns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PushPatterns_PushTypes_PushTypeId",
                        column: x => x.PushTypeId,
                        principalTable: "PushTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PushNotifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    PushPatternId = table.Column<long>(type: "bigint", nullable: false),
                    IsSend = table.Column<bool>(type: "bit", nullable: false),
                    Token1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Token2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Token3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Token4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Token5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: true),
                    StatusText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SendDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PushNotifications_PushPatterns_PushPatternId",
                        column: x => x.PushPatternId,
                        principalTable: "PushPatterns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PushSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PushPatternId = table.Column<long>(type: "bigint", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PushSettings_PushPatterns_PushPatternId",
                        column: x => x.PushPatternId,
                        principalTable: "PushPatterns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PushNotifications_PushPatternId",
                table: "PushNotifications",
                column: "PushPatternId");

            migrationBuilder.CreateIndex(
                name: "IX_PushPatterns_PushTypeId",
                table: "PushPatterns",
                column: "PushTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PushSettings_PushPatternId",
                table: "PushSettings",
                column: "PushPatternId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PushNotifications");

            migrationBuilder.DropTable(
                name: "PushSettings");

            migrationBuilder.DropTable(
                name: "PushPatterns");

            migrationBuilder.DropTable(
                name: "PushTypes");
        }
    }
}
