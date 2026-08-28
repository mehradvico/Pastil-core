using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedPetBirthdayPush : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 56 AND ISNULL(Label, N'') <> N'PushPetBirthday')
    THROW 51000, 'PushType Id 56 is already assigned to another label.', 1;

IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 56)
BEGIN
    SET IDENTITY_INSERT PushTypes ON;
    INSERT INTO PushTypes (Id, Name, Label)
    VALUES (56, N'تولد پت', N'PushPetBirthday');
    SET IDENTITY_INSERT PushTypes OFF;
END;

IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 56)
BEGIN
    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
    VALUES (56, N'Push_Title', N'PushPetBirthday', N'/pets', NULL, N'pet-birthday', 1);

    DECLARE @PetBirthdayPushPatternId bigint = SCOPE_IDENTITY();
    INSERT INTO PushSettings (PushPatternId, IsEnabled)
    VALUES (@PetBirthdayPushPatternId, 1);
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
WHERE p.PushTypeId = 56;

DELETE s
FROM PushSettings s
INNER JOIN PushPatterns p ON p.Id = s.PushPatternId
WHERE p.PushTypeId = 56;

DELETE FROM PushPatterns WHERE PushTypeId = 56;
DELETE FROM PushTypes WHERE Id = 56 AND Label = N'PushPetBirthday';
");
        }
    }
}
