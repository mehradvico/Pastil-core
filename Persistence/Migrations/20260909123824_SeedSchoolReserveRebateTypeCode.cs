using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedSchoolReserveRebateTypeCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @RebateTypeGroupId BIGINT =
(
    SELECT TOP (1) CodeGroupId
    FROM Codes
    WHERE Label = 'RebateType_PansionReserve'
);

IF @RebateTypeGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'RebateType_SchoolReserve')
BEGIN
    DECLARE @NextPriority INT =
    (
        SELECT ISNULL(MAX(Priority), 0) + 1
        FROM Codes
        WHERE CodeGroupId = @RebateTypeGroupId
    );

    INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
    VALUES ('RebateType_SchoolReserve', 'school-reserve', @RebateTypeGroupId, @NextPriority, 1, N'تخفیف رزرو دوره مدرسه');
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM Codes
WHERE Label = 'RebateType_SchoolReserve'
  AND NOT EXISTS (SELECT 1 FROM Rebates WHERE TypeId = Codes.Id);
");
        }
    }
}
