BEGIN TRANSACTION;
ALTER TABLE [ReminderCycles] ADD [UnitId] int NOT NULL DEFAULT 3;
GO

ALTER TABLE [ReminderCycles] ADD CONSTRAINT [CK_ReminderCycle_Unit] CHECK ([UnitId] IN (1, 2, 3));
GO

SET NOCOUNT ON;

INSERT INTO ReminderCycles ([Name], [Cycle], [UnitId], [Deleted])
SELECT N'هر روز', 1, 1, 0
WHERE NOT EXISTS (SELECT 1 FROM ReminderCycles WHERE Cycle = 1 AND UnitId = 1 AND Deleted = 0);

INSERT INTO ReminderCycles ([Name], [Cycle], [UnitId], [Deleted])
SELECT N'هر هفته', 1, 2, 0
WHERE NOT EXISTS (SELECT 1 FROM ReminderCycles WHERE Cycle = 1 AND UnitId = 2 AND Deleted = 0);

DECLARE @ReminderPush TABLE
(
    Id bigint NOT NULL,
    Name nvarchar(200) NOT NULL,
    Label nvarchar(200) NOT NULL,
    Url nvarchar(300) NOT NULL,
    Tag nvarchar(200) NOT NULL
);

INSERT INTO @ReminderPush (Id, Name, Label, Url, Tag)
VALUES
    (75, N'یادآور امروز', N'PushReminderToday', N'/pet/reminder', N'reminder-today');

IF EXISTS
(
    SELECT 1
    FROM @ReminderPush source
    INNER JOIN PushTypes target ON target.Id = source.Id
    WHERE target.Label <> source.Label
)
    THROW 51000, 'PushReminderToday push type ID is already assigned to another label.', 1;

IF EXISTS
(
    SELECT 1
    FROM @ReminderPush source
    INNER JOIN PushTypes target ON target.Label = source.Label
    WHERE target.Id <> source.Id
)
    THROW 51000, 'PushReminderToday label is already assigned to another ID.', 1;

SET IDENTITY_INSERT PushTypes ON;

INSERT INTO PushTypes (Id, Name, Label)
SELECT source.Id, source.Name, source.Label
FROM @ReminderPush source
WHERE NOT EXISTS (SELECT 1 FROM PushTypes target WHERE target.Id = source.Id);

SET IDENTITY_INSERT PushTypes OFF;

INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
SELECT source.Id, N'Push_Title', source.Label, source.Url, NULL, source.Tag, 1
FROM @ReminderPush source
WHERE NOT EXISTS
(
    SELECT 1 FROM PushPatterns pattern WHERE pattern.PushTypeId = source.Id
);

INSERT INTO PushSettings (PushPatternId, IsEnabled)
SELECT pattern.Id, 1
FROM PushPatterns pattern
WHERE pattern.PushTypeId = 75
  AND NOT EXISTS
  (
      SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id
  );

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260923071517_AddReminderCycleUnit', N'9.0.0');

COMMIT;
GO

