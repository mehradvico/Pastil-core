using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationReferralAttribution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReferralCode",
                table: "Users",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReferredByCompanionId",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReferredByStoreId",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReferredByUserId",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegistrationReferralSource",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UsedReferralCode",
                table: "Users",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralCode",
                table: "Stores",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReferralCode",
                table: "Companions",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
                UPDATE [Companions]
                SET [ReferralCode] = '1' + RIGHT(
                    '000000000' + CONVERT(varchar(9), ABS(CONVERT(bigint, CHECKSUM(NEWID()))) % 1000000000),
                    9);

                WHILE EXISTS (
                    SELECT [ReferralCode]
                    FROM [Companions]
                    GROUP BY [ReferralCode]
                    HAVING COUNT(*) > 1)
                BEGIN
                    WITH [DuplicateRows] AS (
                        SELECT [Id], ROW_NUMBER() OVER (
                            PARTITION BY [ReferralCode]
                            ORDER BY [Id]) AS [RowNumber]
                        FROM [Companions])
                    UPDATE [Companions]
                    SET [ReferralCode] = '1' + RIGHT(
                        '000000000' + CONVERT(varchar(9), ABS(CONVERT(bigint, CHECKSUM(NEWID()))) % 1000000000),
                        9)
                    WHERE [Id] IN (
                        SELECT [Id]
                        FROM [DuplicateRows]
                        WHERE [RowNumber] > 1);
                END;

                UPDATE [Stores]
                SET [ReferralCode] = '2' + RIGHT(
                    '000000000' + CONVERT(varchar(9), ABS(CONVERT(bigint, CHECKSUM(NEWID()))) % 1000000000),
                    9);

                WHILE EXISTS (
                    SELECT [ReferralCode]
                    FROM [Stores]
                    GROUP BY [ReferralCode]
                    HAVING COUNT(*) > 1)
                BEGIN
                    WITH [DuplicateRows] AS (
                        SELECT [Id], ROW_NUMBER() OVER (
                            PARTITION BY [ReferralCode]
                            ORDER BY [Id]) AS [RowNumber]
                        FROM [Stores])
                    UPDATE [Stores]
                    SET [ReferralCode] = '2' + RIGHT(
                        '000000000' + CONVERT(varchar(9), ABS(CONVERT(bigint, CHECKSUM(NEWID()))) % 1000000000),
                        9)
                    WHERE [Id] IN (
                        SELECT [Id]
                        FROM [DuplicateRows]
                        WHERE [RowNumber] > 1);
                END;");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ReferralCode",
                table: "Users",
                column: "ReferralCode",
                unique: true,
                filter: "[ReferralCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ReferredByCompanionId",
                table: "Users",
                column: "ReferredByCompanionId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ReferredByStoreId",
                table: "Users",
                column: "ReferredByStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ReferredByUserId",
                table: "Users",
                column: "ReferredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RegistrationReferralSource_ReferredByUserId_ReferredByCompanionId_ReferredByStoreId",
                table: "Users",
                columns: new[] { "RegistrationReferralSource", "ReferredByUserId", "ReferredByCompanionId", "ReferredByStoreId" });

            migrationBuilder.CreateIndex(
                name: "IX_Stores_ReferralCode",
                table: "Stores",
                column: "ReferralCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companions_ReferralCode",
                table: "Companions",
                column: "ReferralCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Companions_ReferredByCompanionId",
                table: "Users",
                column: "ReferredByCompanionId",
                principalTable: "Companions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Stores_ReferredByStoreId",
                table: "Users",
                column: "ReferredByStoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_ReferredByUserId",
                table: "Users",
                column: "ReferredByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Companions_ReferredByCompanionId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Stores_ReferredByStoreId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_ReferredByUserId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ReferralCode",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ReferredByCompanionId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ReferredByStoreId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ReferredByUserId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_RegistrationReferralSource_ReferredByUserId_ReferredByCompanionId_ReferredByStoreId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Stores_ReferralCode",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Companions_ReferralCode",
                table: "Companions");

            migrationBuilder.DropColumn(
                name: "ReferredByCompanionId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ReferredByStoreId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ReferredByUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RegistrationReferralSource",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UsedReferralCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ReferralCode",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "ReferralCode",
                table: "Companions");

            migrationBuilder.AlterColumn<string>(
                name: "ReferralCode",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);
        }
    }
}
