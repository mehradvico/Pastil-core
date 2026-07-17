using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_noticeupdate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notices_Codes_TypeId",
                table: "Notices");

            migrationBuilder.DropForeignKey(
                name: "FK_Notices_Codes_UserTypeId",
                table: "Notices");

            migrationBuilder.DropForeignKey(
                name: "FK_Notices_Users_UserId",
                table: "Notices");

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_Notices_TypeId'
                      AND object_id = OBJECT_ID(N'[dbo].[Notices]')
                )
                    DROP INDEX [IX_Notices_TypeId] ON [dbo].[Notices];

                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_Notices_UserId'
                      AND object_id = OBJECT_ID(N'[dbo].[Notices]')
                )
                    DROP INDEX [IX_Notices_UserId] ON [dbo].[Notices];

                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_Notices_UserTypeId'
                      AND object_id = OBJECT_ID(N'[dbo].[Notices]')
                )
                    DROP INDEX [IX_Notices_UserTypeId] ON [dbo].[Notices];
                """);

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Notices");

            migrationBuilder.RenameColumn(
                name: "UserTypeId",
                table: "Notices",
                newName: "NoticeTypeId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Notices",
                newName: "ReferenceId");

            migrationBuilder.RenameColumn(
                name: "ReadDate",
                table: "Notices",
                newName: "ArchivedAtUtc");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "Notices",
                newName: "ActorUserId");

            migrationBuilder.RenameColumn(
                name: "CreateDate",
                table: "Notices",
                newName: "CreateDateUtc");

            migrationBuilder.AddColumn<long>(
                name: "NoticeId",
                table: "PushNotifications",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "NoticeTypes",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Label",
                table: "NoticeTypes",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Importance",
                table: "NoticeTypes",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "NoticeTypes",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "NavigationTemplate",
                table: "NoticeTypes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "NoticeTypes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchiveDueAtUtc",
                table: "Notices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeduplicationKey",
                table: "Notices",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Notices",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MetadataJson",
                table: "Notices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NavigationUrl",
                table: "Notices",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReferenceType",
                table: "Notices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Notices",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "NoticeReads",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoticeId = table.Column<long>(type: "bigint", nullable: false),
                    AdminId = table.Column<long>(type: "bigint", nullable: false),
                    AdminNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReadAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadMode = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoticeReads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoticeReads_Notices_NoticeId",
                        column: x => x.NoticeId,
                        principalTable: "Notices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NoticeReads_Users_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PushNotifications_NoticeId",
                table: "PushNotifications",
                column: "NoticeId");

            migrationBuilder.CreateIndex(
                name: "IX_NoticeTypes_Label",
                table: "NoticeTypes",
                column: "Label",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notices_ActorUserId",
                table: "Notices",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notices_ArchivedAtUtc_ArchiveDueAtUtc_CreateDateUtc",
                table: "Notices",
                columns: new[] { "ArchivedAtUtc", "ArchiveDueAtUtc", "CreateDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Notices_DeduplicationKey",
                table: "Notices",
                column: "DeduplicationKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notices_NoticeTypeId_CreateDateUtc",
                table: "Notices",
                columns: new[] { "NoticeTypeId", "CreateDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Notices_ReferenceType_ReferenceId",
                table: "Notices",
                columns: new[] { "ReferenceType", "ReferenceId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Notices_MetadataJson_IsJson",
                table: "Notices",
                sql: "[MetadataJson] IS NULL OR ISJSON([MetadataJson]) = 1");

            migrationBuilder.CreateIndex(
                name: "IX_NoticeReads_AdminId_ReadAtUtc",
                table: "NoticeReads",
                columns: new[] { "AdminId", "ReadAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_NoticeReads_NoticeId",
                table: "NoticeReads",
                column: "NoticeId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Notices_NoticeTypes_NoticeTypeId",
                table: "Notices",
                column: "NoticeTypeId",
                principalTable: "NoticeTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notices_Users_ActorUserId",
                table: "Notices",
                column: "ActorUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PushNotifications_Notices_NoticeId",
                table: "PushNotifications",
                column: "NoticeId",
                principalTable: "Notices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notices_NoticeTypes_NoticeTypeId",
                table: "Notices");

            migrationBuilder.DropForeignKey(
                name: "FK_Notices_Users_ActorUserId",
                table: "Notices");

            migrationBuilder.DropForeignKey(
                name: "FK_PushNotifications_Notices_NoticeId",
                table: "PushNotifications");

            migrationBuilder.DropTable(
                name: "NoticeReads");

            migrationBuilder.DropIndex(
                name: "IX_PushNotifications_NoticeId",
                table: "PushNotifications");

            migrationBuilder.DropIndex(
                name: "IX_NoticeTypes_Label",
                table: "NoticeTypes");

            migrationBuilder.DropIndex(
                name: "IX_Notices_ActorUserId",
                table: "Notices");

            migrationBuilder.DropIndex(
                name: "IX_Notices_ArchivedAtUtc_ArchiveDueAtUtc_CreateDateUtc",
                table: "Notices");

            migrationBuilder.DropIndex(
                name: "IX_Notices_DeduplicationKey",
                table: "Notices");

            migrationBuilder.DropIndex(
                name: "IX_Notices_NoticeTypeId_CreateDateUtc",
                table: "Notices");

            migrationBuilder.DropIndex(
                name: "IX_Notices_ReferenceType_ReferenceId",
                table: "Notices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Notices_MetadataJson_IsJson",
                table: "Notices");

            migrationBuilder.DropColumn(
                name: "NoticeId",
                table: "PushNotifications");

            migrationBuilder.DropColumn(
                name: "Importance",
                table: "NoticeTypes");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "NoticeTypes");

            migrationBuilder.DropColumn(
                name: "NavigationTemplate",
                table: "NoticeTypes");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "NoticeTypes");

            migrationBuilder.DropColumn(
                name: "ArchiveDueAtUtc",
                table: "Notices");

            migrationBuilder.DropColumn(
                name: "DeduplicationKey",
                table: "Notices");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "Notices");

            migrationBuilder.DropColumn(
                name: "MetadataJson",
                table: "Notices");

            migrationBuilder.DropColumn(
                name: "NavigationUrl",
                table: "Notices");

            migrationBuilder.DropColumn(
                name: "ReferenceType",
                table: "Notices");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Notices");

            migrationBuilder.RenameColumn(
                name: "ReferenceId",
                table: "Notices",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "NoticeTypeId",
                table: "Notices",
                newName: "UserTypeId");

            migrationBuilder.RenameColumn(
                name: "CreateDateUtc",
                table: "Notices",
                newName: "CreateDate");

            migrationBuilder.RenameColumn(
                name: "ArchivedAtUtc",
                table: "Notices",
                newName: "ReadDate");

            migrationBuilder.RenameColumn(
                name: "ActorUserId",
                table: "Notices",
                newName: "ItemId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "NoticeTypes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "Label",
                table: "NoticeTypes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AddColumn<long>(
                name: "TypeId",
                table: "Notices",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Notices_TypeId",
                table: "Notices",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Notices_UserId",
                table: "Notices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notices_UserTypeId",
                table: "Notices",
                column: "UserTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notices_Codes_TypeId",
                table: "Notices",
                column: "TypeId",
                principalTable: "Codes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notices_Codes_UserTypeId",
                table: "Notices",
                column: "UserTypeId",
                principalTable: "Codes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notices_Users_UserId",
                table: "Notices",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
