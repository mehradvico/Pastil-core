using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_updatestory6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoryItems_Pictures_PictureId",
                table: "StoryItems");

            migrationBuilder.AlterColumn<long>(
                name: "PictureId",
                table: "StoryItems",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "StoryItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DayCount",
                table: "StoryItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpireDate",
                table: "StoryItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "FileId",
                table: "StoryItems",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoryItems_FileId",
                table: "StoryItems",
                column: "FileId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoryItems_Files_FileId",
                table: "StoryItems",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StoryItems_Pictures_PictureId",
                table: "StoryItems",
                column: "PictureId",
                principalTable: "Pictures",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoryItems_Files_FileId",
                table: "StoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_StoryItems_Pictures_PictureId",
                table: "StoryItems");

            migrationBuilder.DropIndex(
                name: "IX_StoryItems_FileId",
                table: "StoryItems");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "StoryItems");

            migrationBuilder.DropColumn(
                name: "DayCount",
                table: "StoryItems");

            migrationBuilder.DropColumn(
                name: "ExpireDate",
                table: "StoryItems");

            migrationBuilder.DropColumn(
                name: "FileId",
                table: "StoryItems");

            migrationBuilder.AlterColumn<long>(
                name: "PictureId",
                table: "StoryItems",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StoryItems_Pictures_PictureId",
                table: "StoryItems",
                column: "PictureId",
                principalTable: "Pictures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
