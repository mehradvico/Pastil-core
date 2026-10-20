using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProviderSettlements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Kind",
                table: "Settlements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "ProviderId",
                table: "Settlements",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_ProviderId_Kind",
                table: "Settlements",
                columns: new[] { "ProviderId", "Kind" });

            migrationBuilder.AddForeignKey(
                name: "FK_Settlements_Providers_ProviderId",
                table: "Settlements",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ---- backfill: تسویه‌های قدیمی (نمایندگی/فروشگاه‌محور) برچسب استخر و ارائه‌دهنده می‌گیرند ----
            // Kind: ردیفی که StoreId دارد → فروشگاه (2)، بقیه → نمایندگی (1). ProviderId از ارائه‌دهنده‌ی همان نمایندگی/فروشگاه.
            // ردیفی که نمایندگی/فروشگاهش ارائه‌دهنده ندارد ProviderId=null می‌ماند (کوئری بررسی ته scripts/ProviderSettlements.sql).
            migrationBuilder.Sql(@"
UPDATE Settlements SET Kind = CASE WHEN StoreId IS NOT NULL THEN 2 ELSE 1 END;

UPDATE s SET s.ProviderId = c.ProviderId
FROM Settlements s JOIN Companions c ON c.Id = s.CompanionId
WHERE s.CompanionId IS NOT NULL AND c.ProviderId IS NOT NULL;

UPDATE s SET s.ProviderId = st.ProviderId
FROM Settlements s JOIN Stores st ON st.Id = s.StoreId
WHERE s.StoreId IS NOT NULL AND st.ProviderId IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Settlements_Providers_ProviderId",
                table: "Settlements");

            migrationBuilder.DropIndex(
                name: "IX_Settlements_ProviderId_Kind",
                table: "Settlements");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "Settlements");

            migrationBuilder.DropColumn(
                name: "ProviderId",
                table: "Settlements");
        }
    }
}
