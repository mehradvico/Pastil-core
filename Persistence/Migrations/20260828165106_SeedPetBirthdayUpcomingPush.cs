using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedPetBirthdayUpcomingPush : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 57 AND ISNULL(Label, N'') <> N'PushPetBirthdayUpcoming')
    THROW 51000, 'PushType Id 57 is already assigned to another label.', 1;

IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 57)
BEGIN
    SET IDENTITY_INSERT PushTypes ON;
    INSERT INTO PushTypes (Id, Name, Label)
    VALUES (57, N'یادآوری تولد نزدیک پت', N'PushPetBirthdayUpcoming');
    SET IDENTITY_INSERT PushTypes OFF;
END;

IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 57)
BEGIN
    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
    VALUES (57, N'Push_Title', N'PushPetBirthdayUpcoming', N'/pets', NULL, N'pet-birthday-upcoming', 1);

    DECLARE @PetBirthdayUpcomingPushPatternId bigint = SCOPE_IDENTITY();
    INSERT INTO PushSettings (PushPatternId, IsEnabled)
    VALUES (@PetBirthdayUpcomingPushPatternId, 1);
END;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE n
FROM PushNotifications n
INNER JOIN PushPatterns p ON p.Id = n.PushPatternId
WHERE p.PushTypeId = 57;

DELETE s
FROM PushSettings s
INNER JOIN PushPatterns p ON p.Id = s.PushPatternId
WHERE p.PushTypeId = 57;

DELETE FROM PushPatterns WHERE PushTypeId = 57;
DELETE FROM PushTypes WHERE Id = 57 AND Label = N'PushPetBirthdayUpcoming';
");
        }
    }
}
