using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    public partial class mig_ticketupdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // حذف امن Foreign Keyهای قبلی
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_TicketItems_Files_FileId'
)
    ALTER TABLE [dbo].[TicketItems]
    DROP CONSTRAINT [FK_TicketItems_Files_FileId];

IF EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_Tickets_Files_FileId'
)
    ALTER TABLE [dbo].[Tickets]
    DROP CONSTRAINT [FK_Tickets_Files_FileId];

IF EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_Tickets_Products_ProductId'
)
    ALTER TABLE [dbo].[Tickets]
    DROP CONSTRAINT [FK_Tickets_Products_ProductId];

IF EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_Tickets_Users_AdminId'
)
    ALTER TABLE [dbo].[Tickets]
    DROP CONSTRAINT [FK_Tickets_Users_AdminId];
");

            // حذف امن Indexهای قبلی
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Tickets_AdminId'
      AND object_id = OBJECT_ID(N'dbo.Tickets')
)
    DROP INDEX [IX_Tickets_AdminId] ON [dbo].[Tickets];

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Tickets_FileId'
      AND object_id = OBJECT_ID(N'dbo.Tickets')
)
    DROP INDEX [IX_Tickets_FileId] ON [dbo].[Tickets];

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Tickets_StatusId'
      AND object_id = OBJECT_ID(N'dbo.Tickets')
)
    DROP INDEX [IX_Tickets_StatusId] ON [dbo].[Tickets];

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Tickets_UserId'
      AND object_id = OBJECT_ID(N'dbo.Tickets')
)
    DROP INDEX [IX_Tickets_UserId] ON [dbo].[Tickets];

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TicketItems_TicketId'
      AND object_id = OBJECT_ID(N'dbo.TicketItems')
)
    DROP INDEX [IX_TicketItems_TicketId] ON [dbo].[TicketItems];
");

            // جلوگیری از خطا هنگام تبدیل Name به Required
            migrationBuilder.Sql(@"
UPDATE [dbo].[Tickets]
SET [Name] = N'تیکت شماره ' + CONVERT(nvarchar(20), [Id])
WHERE [Name] IS NULL
   OR LTRIM(RTRIM([Name])) = N'';
");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tickets",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CloseDate",
                table: "Tickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TicketCategoryId",
                table: "Tickets",
                type: "bigint",
                nullable: false,
                defaultValue: 10139L);

            migrationBuilder.AddColumn<bool>(
                name: "IsSeen",
                table: "TicketItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ReplyToTicketItemId",
                table: "TicketItems",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SeenDate",
                table: "TicketItems",
                type: "datetime2",
                nullable: true);

            // پیام‌های قدیمی را خوانده‌شده در نظر می‌گیریم
            migrationBuilder.Sql(@"
UPDATE [dbo].[TicketItems]
SET
    [IsSeen] = 1,
    [SeenDate] = COALESCE([SeenDate], [CreateDate]);
");

            // انتقال اولین پیام قدیمی Ticket به TicketItems
            migrationBuilder.Sql(@"
INSERT INTO [dbo].[TicketItems]
(
    [Body],
    [UserId],
    [CreateDate],
    [FileId],
    [TicketId],
    [Deleted],
    [IsSeen],
    [SeenDate],
    [ReplyToTicketItemId]
)
SELECT
    ticket.[Body],
    ticket.[UserId],
    ticket.[CreateDate],
    ticket.[FileId],
    ticket.[Id],
    ticket.[Deleted],
    1,
    ticket.[CreateDate],
    NULL
FROM [dbo].[Tickets] ticket
WHERE
    (
        NULLIF(LTRIM(RTRIM(ticket.[Body])), N'') IS NOT NULL
        OR ticket.[FileId] IS NOT NULL
    )
    AND NOT EXISTS
    (
        SELECT 1
        FROM [dbo].[TicketItems] item
        WHERE item.[TicketId] = ticket.[Id]
          AND item.[UserId] = ticket.[UserId]
          AND item.[CreateDate] = ticket.[CreateDate]
          AND ISNULL(item.[Body], N'') = ISNULL(ticket.[Body], N'')
          AND ISNULL(item.[FileId], -1) = ISNULL(ticket.[FileId], -1)
    );
");

            // مقداردهی تاریخ بسته‌شدن Ticketهای قدیمی
            migrationBuilder.Sql(@"
UPDATE [dbo].[Tickets]
SET [CloseDate] = [UpdateDate]
WHERE [StatusId] = 31
  AND [CloseDate] IS NULL;
");

            // پس از انتقال امن اطلاعات، ستون‌های قدیمی حذف می‌شوند
            migrationBuilder.DropColumn(
                name: "Body",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "FileId",
                table: "Tickets");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AdminId_StatusId_UpdateDate",
                table: "Tickets",
                columns: new[] { "AdminId", "StatusId", "UpdateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_StatusId_TicketCategoryId_UpdateDate",
                table: "Tickets",
                columns: new[] { "StatusId", "TicketCategoryId", "UpdateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TicketCategoryId",
                table: "Tickets",
                column: "TicketCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UserId_UpdateDate",
                table: "Tickets",
                columns: new[] { "UserId", "UpdateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketItems_ReplyToTicketItemId",
                table: "TicketItems",
                column: "ReplyToTicketItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketItems_TicketId_Id",
                table: "TicketItems",
                columns: new[] { "TicketId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketItems_TicketId_IsSeen_UserId",
                table: "TicketItems",
                columns: new[] { "TicketId", "IsSeen", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_TicketItems_Files_FileId",
                table: "TicketItems",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketItems_TicketItems_ReplyToTicketItemId",
                table: "TicketItems",
                column: "ReplyToTicketItemId",
                principalTable: "TicketItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Codes_TicketCategoryId",
                table: "Tickets",
                column: "TicketCategoryId",
                principalTable: "Codes",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Products_ProductId",
                table: "Tickets",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_AdminId",
                table: "Tickets",
                column: "AdminId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_TicketItems_Files_FileId'
)
    ALTER TABLE [dbo].[TicketItems]
    DROP CONSTRAINT [FK_TicketItems_Files_FileId];

IF EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_TicketItems_TicketItems_ReplyToTicketItemId'
)
    ALTER TABLE [dbo].[TicketItems]
    DROP CONSTRAINT [FK_TicketItems_TicketItems_ReplyToTicketItemId];

IF EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_Tickets_Codes_TicketCategoryId'
)
    ALTER TABLE [dbo].[Tickets]
    DROP CONSTRAINT [FK_Tickets_Codes_TicketCategoryId];

IF EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_Tickets_Products_ProductId'
)
    ALTER TABLE [dbo].[Tickets]
    DROP CONSTRAINT [FK_Tickets_Products_ProductId];

IF EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_Tickets_Users_AdminId'
)
    ALTER TABLE [dbo].[Tickets]
    DROP CONSTRAINT [FK_Tickets_Users_AdminId];
");

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Tickets_AdminId_StatusId_UpdateDate'
      AND object_id = OBJECT_ID(N'dbo.Tickets')
)
    DROP INDEX [IX_Tickets_AdminId_StatusId_UpdateDate] ON [dbo].[Tickets];

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Tickets_StatusId_TicketCategoryId_UpdateDate'
      AND object_id = OBJECT_ID(N'dbo.Tickets')
)
    DROP INDEX [IX_Tickets_StatusId_TicketCategoryId_UpdateDate] ON [dbo].[Tickets];

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Tickets_TicketCategoryId'
      AND object_id = OBJECT_ID(N'dbo.Tickets')
)
    DROP INDEX [IX_Tickets_TicketCategoryId] ON [dbo].[Tickets];

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Tickets_UserId_UpdateDate'
      AND object_id = OBJECT_ID(N'dbo.Tickets')
)
    DROP INDEX [IX_Tickets_UserId_UpdateDate] ON [dbo].[Tickets];

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TicketItems_ReplyToTicketItemId'
      AND object_id = OBJECT_ID(N'dbo.TicketItems')
)
    DROP INDEX [IX_TicketItems_ReplyToTicketItemId] ON [dbo].[TicketItems];

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TicketItems_TicketId_Id'
      AND object_id = OBJECT_ID(N'dbo.TicketItems')
)
    DROP INDEX [IX_TicketItems_TicketId_Id] ON [dbo].[TicketItems];

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TicketItems_TicketId_IsSeen_UserId'
      AND object_id = OBJECT_ID(N'dbo.TicketItems')
)
    DROP INDEX [IX_TicketItems_TicketId_IsSeen_UserId] ON [dbo].[TicketItems];
");

            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileId",
                table: "Tickets",
                type: "bigint",
                nullable: true);

            // انتقال اولین پیام به ساختار قدیمی
            migrationBuilder.Sql(@"
SELECT
    source.[Id],
    source.[TicketId],
    source.[Body],
    source.[FileId]
INTO #FirstTicketItems
FROM
(
    SELECT
        item.[Id],
        item.[TicketId],
        item.[Body],
        item.[FileId],
        ROW_NUMBER() OVER
        (
            PARTITION BY item.[TicketId]
            ORDER BY item.[CreateDate], item.[Id]
        ) AS RowNumber
    FROM [dbo].[TicketItems] item
    WHERE item.[Deleted] = 0
) source
WHERE source.RowNumber = 1;

UPDATE ticket
SET
    ticket.[Body] = firstItem.[Body],
    ticket.[FileId] = firstItem.[FileId]
FROM [dbo].[Tickets] ticket
INNER JOIN #FirstTicketItems firstItem
    ON firstItem.[TicketId] = ticket.[Id];

DELETE item
FROM [dbo].[TicketItems] item
INNER JOIN #FirstTicketItems firstItem
    ON firstItem.[Id] = item.[Id];

DROP TABLE #FirstTicketItems;
");

            migrationBuilder.DropColumn(
                name: "CloseDate",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TicketCategoryId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "IsSeen",
                table: "TicketItems");

            migrationBuilder.DropColumn(
                name: "ReplyToTicketItemId",
                table: "TicketItems");

            migrationBuilder.DropColumn(
                name: "SeenDate",
                table: "TicketItems");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AdminId",
                table: "Tickets",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_FileId",
                table: "Tickets",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_StatusId",
                table: "Tickets",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketItems_TicketId",
                table: "TicketItems",
                column: "TicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketItems_Files_FileId",
                table: "TicketItems",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Files_FileId",
                table: "Tickets",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Products_ProductId",
                table: "Tickets",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_AdminId",
                table: "Tickets",
                column: "AdminId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}