using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations
{
    [DbContext(typeof(DataBaseContext))]
    [Migration("20260917160000_AddPetResanIdempotencyAndOccurrenceUniqueness")]
    public partial class AddPetResanIdempotencyAndOccurrenceUniqueness : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Do not silently choose or delete a financially meaningful historic trip. Deployment
            // must stop for an operator to review any old duplicate before enforcing the invariant.
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM [Trips]
    WHERE [PetResanServiceScheduleId] IS NOT NULL
      AND [TripStartDateTime] IS NOT NULL
    GROUP BY [PetResanServiceScheduleId], [TripStartDateTime]
    HAVING COUNT(*) > 1
)
BEGIN
    ;THROW 51000, 'Duplicate PetResan service occurrences exist. Resolve them before applying this migration.', 1;
END;");

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "PetResanServices",
                type: "varchar(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PetResanServices_UserId_IdempotencyKey",
                table: "PetResanServices",
                columns: new[] { "UserId", "IdempotencyKey" },
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_PetResanServiceScheduleId_TripStartDateTime",
                table: "Trips",
                columns: new[] { "PetResanServiceScheduleId", "TripStartDateTime" },
                unique: true,
                filter: "[PetResanServiceScheduleId] IS NOT NULL AND [TripStartDateTime] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PetResanServices_UserId_IdempotencyKey",
                table: "PetResanServices");

            migrationBuilder.DropIndex(
                name: "IX_Trips_PetResanServiceScheduleId_TripStartDateTime",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "PetResanServices");
        }
    }
}
