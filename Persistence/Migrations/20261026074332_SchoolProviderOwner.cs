using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SchoolProviderOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schools_Companions_CompanionId",
                table: "Schools");

            migrationBuilder.AlterColumn<long>(
                name: "CompanionId",
                table: "Schools",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "ProviderId",
                table: "Schools",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schools_ProviderId",
                table: "Schools",
                column: "ProviderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_Companions_CompanionId",
                table: "Schools",
                column: "CompanionId",
                principalTable: "Companions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_Providers_ProviderId",
                table: "Schools",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id");

            // مدرسه‌های موجود: مالکِ ارائه‌دهنده از نمایندگی‌شان (اگر نمایندگی به ارائه‌دهنده وصل باشد)
            // EXEC: در اسکریپت SQL، ستون تازه در همان batch هنوز شناخته نمی‌شود
            migrationBuilder.Sql(@"EXEC(N'UPDATE s SET s.ProviderId = c.ProviderId
FROM Schools s INNER JOIN Companions c ON c.Id = s.CompanionId
WHERE s.ProviderId IS NULL AND c.ProviderId IS NOT NULL;');");

            // دست‌کم یک مالک
            migrationBuilder.Sql("EXEC(N'ALTER TABLE Schools ADD CONSTRAINT CK_Schools_Owner CHECK (CompanionId IS NOT NULL OR ProviderId IS NOT NULL);');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE Schools DROP CONSTRAINT CK_Schools_Owner;");

            migrationBuilder.DropForeignKey(
                name: "FK_Schools_Companions_CompanionId",
                table: "Schools");

            migrationBuilder.DropForeignKey(
                name: "FK_Schools_Providers_ProviderId",
                table: "Schools");

            migrationBuilder.DropIndex(
                name: "IX_Schools_ProviderId",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "ProviderId",
                table: "Schools");

            migrationBuilder.AlterColumn<long>(
                name: "CompanionId",
                table: "Schools",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_Companions_CompanionId",
                table: "Schools",
                column: "CompanionId",
                principalTable: "Companions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
