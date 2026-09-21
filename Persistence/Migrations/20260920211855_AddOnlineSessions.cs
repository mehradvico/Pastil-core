using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOnlineSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OnlineSessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InitiatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    TargetUserId = table.Column<long>(type: "bigint", nullable: false),
                    ChannelId = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnlineSessions_Users_InitiatorUserId",
                        column: x => x.InitiatorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OnlineSessions_Users_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OnlineSessionMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OnlineSessionId = table.Column<long>(type: "bigint", nullable: false),
                    SenderUserId = table.Column<long>(type: "bigint", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    DeliveredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineSessionMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnlineSessionMessages_OnlineSessions_OnlineSessionId",
                        column: x => x.OnlineSessionId,
                        principalTable: "OnlineSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OnlineSessionMessages_Users_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OnlineSessionMessages_OnlineSessionId_Id",
                table: "OnlineSessionMessages",
                columns: new[] { "OnlineSessionId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_OnlineSessionMessages_SenderUserId",
                table: "OnlineSessionMessages",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineSessions_InitiatorUserId_EndDate",
                table: "OnlineSessions",
                columns: new[] { "InitiatorUserId", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_OnlineSessions_TargetUserId_EndDate",
                table: "OnlineSessions",
                columns: new[] { "TargetUserId", "EndDate" });

            migrationBuilder.Sql(@"
DECLARE @PushTypes TABLE (Id BIGINT NOT NULL, Name NVARCHAR(200) NOT NULL, Label NVARCHAR(200) NOT NULL, Title NVARCHAR(200) NOT NULL, Body NVARCHAR(500) NOT NULL, Url NVARCHAR(500) NOT NULL, Tag NVARCHAR(100) NOT NULL);

INSERT INTO @PushTypes (Id, Name, Label, Title, Body, Url, Tag)
VALUES
    (66, N'دعوت به چت جلسه‌ی آنلاین', N'PushOnlineSessionChatInvite', N'دعوت به چت', N'{0} می‌خواهد از طریق چت با شما در ارتباط باشد. برای ورود به گفتگو ضربه بزنید.', N'/online-chat/{1}', N'online-session-chat-invite'),
    (67, N'پیام جدید چت جلسه‌ی آنلاین', N'PushOnlineSessionNewMessage', N'پیام جدید', N'{0}: {1}', N'/online-chat/{2}', N'online-session-new-message');

IF EXISTS (SELECT 1 FROM @PushTypes s INNER JOIN PushTypes t ON t.Id = s.Id WHERE t.Label <> s.Label)
    THROW 51000, 'An OnlineSession PushType ID is already assigned to another label.', 1;

IF EXISTS (SELECT 1 FROM @PushTypes s INNER JOIN PushTypes t ON t.Label = s.Label WHERE t.Id <> s.Id)
    THROW 51000, 'An OnlineSession PushType label is already assigned to another ID.', 1;

SET IDENTITY_INSERT PushTypes ON;

INSERT INTO PushTypes (Id, Name, Label)
SELECT s.Id, s.Name, s.Label FROM @PushTypes s WHERE NOT EXISTS (SELECT 1 FROM PushTypes t WHERE t.Id = s.Id);

SET IDENTITY_INSERT PushTypes OFF;

INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
SELECT s.Id, s.Title, s.Body, s.Url, NULL, s.Tag, 1 FROM @PushTypes s
WHERE NOT EXISTS (SELECT 1 FROM PushPatterns p WHERE p.PushTypeId = s.Id);

INSERT INTO PushSettings (PushPatternId, IsEnabled)
SELECT p.Id, 1 FROM PushPatterns p
WHERE p.PushTypeId IN (66, 67) AND NOT EXISTS (SELECT 1 FROM PushSettings x WHERE x.PushPatternId = p.Id);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE setting
FROM PushSettings setting
INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
WHERE pattern.PushTypeId IN (66, 67);

DELETE FROM PushPatterns WHERE PushTypeId IN (66, 67);
DELETE FROM PushTypes WHERE Id IN (66, 67) AND Label IN (N'PushOnlineSessionChatInvite', N'PushOnlineSessionNewMessage');
");

            migrationBuilder.DropTable(
                name: "OnlineSessionMessages");

            migrationBuilder.DropTable(
                name: "OnlineSessions");
        }
    }
}
