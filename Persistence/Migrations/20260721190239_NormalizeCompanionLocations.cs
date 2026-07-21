using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeCompanionLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE [dbo].[Companions]
                SET [Location] = geography::Point(
                    CASE
                        WHEN [Location].Lat BETWEEN 44 AND 64
                         AND [Location].Long BETWEEN 24 AND 40
                        THEN [Location].Long
                        ELSE 0
                    END,
                    CASE
                        WHEN [Location].Lat BETWEEN 44 AND 64
                         AND [Location].Long BETWEEN 24 AND 40
                        THEN [Location].Lat
                        ELSE 0
                    END,
                    4326)
                WHERE [Location] IS NOT NULL
                  AND [Location].Lat BETWEEN 44 AND 64
                  AND [Location].Long BETWEEN 24 AND 40;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Coordinate normalization is intentionally irreversible because
            // swapping all standard points back would corrupt valid new data.
        }
    }
}
