using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedCompanionReserveMessageTypeCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM CodeGroups WHERE Label = 'CompanionReserveMessageType')
    INSERT INTO CodeGroups (Name, Label) VALUES (N'نوع پیام چت خدمات آنلاین', 'CompanionReserveMessageType');

DECLARE @CodeGroupId BIGINT = (SELECT TOP (1) Id FROM CodeGroups WHERE Label = 'CompanionReserveMessageType');

DECLARE @CompanionReserveMessageTypes TABLE
(
    Id BIGINT NOT NULL,
    Label NVARCHAR(200) NOT NULL,
    Value NVARCHAR(200) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Priority INT NOT NULL
);

INSERT INTO @CompanionReserveMessageTypes (Id, Label, Value, Name, Priority)
VALUES
    (140, 'CompanionReserveMessageType_Text', 'text', N'متن', 1),
    (141, 'CompanionReserveMessageType_Image', 'image', N'تصویر', 2),
    (142, 'CompanionReserveMessageType_Voice', 'voice', N'صوت', 3),
    (143, 'CompanionReserveMessageType_System', 'system', N'سیستمی', 4);

IF EXISTS
(
    SELECT 1
    FROM @CompanionReserveMessageTypes source
    INNER JOIN Codes target ON target.Id = source.Id
    WHERE target.Label <> source.Label
)
    THROW 51000, 'A CompanionReserveMessageType Code ID is already assigned to another label.', 1;

IF EXISTS
(
    SELECT 1
    FROM @CompanionReserveMessageTypes source
    INNER JOIN Codes target ON target.Label = source.Label
    WHERE target.Id <> source.Id
)
    THROW 51000, 'A CompanionReserveMessageType Code label is already assigned to another ID.', 1;

SET IDENTITY_INSERT Codes ON;

INSERT INTO Codes (Id, Label, Value, CodeGroupId, Priority, Active, Name)
SELECT source.Id, source.Label, source.Value, @CodeGroupId, source.Priority, 1, source.Name
FROM @CompanionReserveMessageTypes source
WHERE NOT EXISTS (SELECT 1 FROM Codes target WHERE target.Id = source.Id);

SET IDENTITY_INSERT Codes OFF;

DECLARE @PushTypeId BIGINT = 65;
DECLARE @PushTypeLabel NVARCHAR(200) = N'PushCompanionReserveNewMessage';

IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = @PushTypeId AND Label <> @PushTypeLabel)
    THROW 51000, 'PushType ID 65 is already assigned to another label.', 1;

IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = @PushTypeLabel AND Id <> @PushTypeId)
    THROW 51000, 'PushCompanionReserveNewMessage label is already assigned to another ID.', 1;

SET IDENTITY_INSERT PushTypes ON;

IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = @PushTypeId)
    INSERT INTO PushTypes (Id, Name, Label) VALUES (@PushTypeId, N'پیام جدید چت خدمات آنلاین', @PushTypeLabel);

SET IDENTITY_INSERT PushTypes OFF;

IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = @PushTypeId)
    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
    VALUES
    (
        @PushTypeId,
        N'پیام جدید در چت خدمات آنلاین',
        N'{0}: {1}',
        N'/companion-chat/{2}',
        NULL,
        N'companion-reserve-new-message',
        1
    );

INSERT INTO PushSettings (PushPatternId, IsEnabled)
SELECT pattern.Id, 1
FROM PushPatterns pattern
WHERE pattern.PushTypeId = @PushTypeId
  AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE setting
FROM PushSettings setting
INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
WHERE pattern.PushTypeId = 65;

DELETE FROM PushPatterns WHERE PushTypeId = 65;
DELETE FROM PushTypes WHERE Id = 65 AND Label = N'PushCompanionReserveNewMessage';
DELETE FROM Codes WHERE Id IN (140, 141, 142, 143) AND NOT EXISTS (SELECT 1 FROM CompanionReserveMessages WHERE CompanionReserveMessageTypeId = Codes.Id);
DELETE FROM CodeGroups WHERE Label = 'CompanionReserveMessageType' AND NOT EXISTS (SELECT 1 FROM Codes WHERE CodeGroupId = CodeGroups.Id);
");
        }
    }
}
