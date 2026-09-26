using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedPastilAiPaymentTypeCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @PaymentTypeGroupId BIGINT =
(
    SELECT TOP (1) CodeGroupId
    FROM Codes
    WHERE Label = 'PaymentType_PansionReserve'
);

IF @PaymentTypeGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'PaymentType_PastilAI')
BEGIN
    DECLARE @NextPriority INT =
    (
        SELECT ISNULL(MAX(Priority), 0) + 1
        FROM Codes
        WHERE CodeGroupId = @PaymentTypeGroupId
    );

    INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
    VALUES ('PaymentType_PastilAI', 'pastil-ai', @PaymentTypeGroupId, @NextPriority, 1, N'پرداخت اشتراک پاستیل‌ای‌آی');
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM Codes
WHERE Label = 'PaymentType_PastilAI'
  AND NOT EXISTS (SELECT 1 FROM Payments WHERE TypeId = Codes.Id);
");
        }
    }
}
