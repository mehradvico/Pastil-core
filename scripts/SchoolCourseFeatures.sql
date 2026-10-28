BEGIN TRANSACTION;
ALTER TABLE [SchoolCourseSessions] ADD [DayReminderPushSentDate] datetime2 NULL;

ALTER TABLE [SchoolCourses] ADD [CertificateEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [SchoolCourses] ADD [FullRefundHoursBeforeStart] int NOT NULL DEFAULT 24;

ALTER TABLE [SchoolCourses] ADD [LateCancelRefundPercent] int NOT NULL DEFAULT 50;

ALTER TABLE [SchoolCourses] ADD [LateEnrollmentMode] int NOT NULL DEFAULT 0;

ALTER TABLE [SchoolCourses] ADD [Level] int NOT NULL DEFAULT 0;

ALTER TABLE [SchoolCourses] ADD [PrerequisiteCourseId] bigint NULL;

ALTER TABLE [SchoolCourses] ADD [RateAvg] float NOT NULL DEFAULT 0.0E0;

ALTER TABLE [SchoolCourses] ADD [RateCount] int NOT NULL DEFAULT 0;

ALTER TABLE [SchoolComments] ADD [SchoolCourseId] bigint NULL;

CREATE TABLE [SchoolAttendances] (
    [Id] bigint NOT NULL IDENTITY,
    [SchoolCourseSessionId] bigint NOT NULL,
    [SchoolReserveId] bigint NOT NULL,
    [Present] bit NOT NULL,
    [Note] nvarchar(500) NULL,
    [RecordedByUserId] bigint NOT NULL,
    [CreateDate] datetime2 NOT NULL,
    [UpdateDate] datetime2 NULL,
    [MissedPushSentDate] datetime2 NULL,
    CONSTRAINT [PK_SchoolAttendances] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SchoolAttendances_SchoolCourseSessions_SchoolCourseSessionId] FOREIGN KEY ([SchoolCourseSessionId]) REFERENCES [SchoolCourseSessions] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SchoolAttendances_SchoolReserves_SchoolReserveId] FOREIGN KEY ([SchoolReserveId]) REFERENCES [SchoolReserves] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [SchoolCertificates] (
    [Id] bigint NOT NULL IDENTITY,
    [SchoolReserveId] bigint NOT NULL,
    [Code] nvarchar(20) NOT NULL,
    [IssueDate] datetime2 NOT NULL,
    [IssuedByUserId] bigint NOT NULL,
    CONSTRAINT [PK_SchoolCertificates] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SchoolCertificates_SchoolReserves_SchoolReserveId] FOREIGN KEY ([SchoolReserveId]) REFERENCES [SchoolReserves] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [SchoolCourseInstructors] (
    [Id] bigint NOT NULL IDENTITY,
    [SchoolCourseId] bigint NOT NULL,
    [UserId] bigint NOT NULL,
    [IsLead] bit NOT NULL,
    [Deleted] bit NOT NULL,
    [CreateDate] datetime2 NOT NULL,
    CONSTRAINT [PK_SchoolCourseInstructors] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SchoolCourseInstructors_SchoolCourses_SchoolCourseId] FOREIGN KEY ([SchoolCourseId]) REFERENCES [SchoolCourses] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SchoolCourseInstructors_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [SchoolCourseSkills] (
    [Id] bigint NOT NULL IDENTITY,
    [SchoolCourseId] bigint NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [SortOrder] int NOT NULL,
    [Deleted] bit NOT NULL,
    CONSTRAINT [PK_SchoolCourseSkills] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SchoolCourseSkills_SchoolCourses_SchoolCourseId] FOREIGN KEY ([SchoolCourseId]) REFERENCES [SchoolCourses] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [SchoolCourseWaitlists] (
    [Id] bigint NOT NULL IDENTITY,
    [SchoolCourseId] bigint NOT NULL,
    [UserId] bigint NOT NULL,
    [UserPetId] bigint NOT NULL,
    [CreateDate] datetime2 NOT NULL,
    [NotifiedDate] datetime2 NULL,
    [Deleted] bit NOT NULL,
    CONSTRAINT [PK_SchoolCourseWaitlists] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SchoolCourseWaitlists_SchoolCourses_SchoolCourseId] FOREIGN KEY ([SchoolCourseId]) REFERENCES [SchoolCourses] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SchoolCourseWaitlists_UserPets_UserPetId] FOREIGN KEY ([UserPetId]) REFERENCES [UserPets] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SchoolCourseWaitlists_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [SchoolProgresses] (
    [Id] bigint NOT NULL IDENTITY,
    [SchoolReserveId] bigint NOT NULL,
    [SchoolCourseSkillId] bigint NOT NULL,
    [SchoolCourseSessionId] bigint NULL,
    [Score] int NOT NULL,
    [Note] nvarchar(500) NULL,
    [RecordedByUserId] bigint NOT NULL,
    [CreateDate] datetime2 NOT NULL,
    CONSTRAINT [PK_SchoolProgresses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SchoolProgresses_SchoolCourseSessions_SchoolCourseSessionId] FOREIGN KEY ([SchoolCourseSessionId]) REFERENCES [SchoolCourseSessions] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SchoolProgresses_SchoolCourseSkills_SchoolCourseSkillId] FOREIGN KEY ([SchoolCourseSkillId]) REFERENCES [SchoolCourseSkills] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SchoolProgresses_SchoolReserves_SchoolReserveId] FOREIGN KEY ([SchoolReserveId]) REFERENCES [SchoolReserves] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_SchoolCourses_PrerequisiteCourseId] ON [SchoolCourses] ([PrerequisiteCourseId]);

CREATE INDEX [IX_SchoolComments_SchoolCourseId] ON [SchoolComments] ([SchoolCourseId]);

CREATE UNIQUE INDEX [IX_SchoolAttendances_SchoolCourseSessionId_SchoolReserveId] ON [SchoolAttendances] ([SchoolCourseSessionId], [SchoolReserveId]);

CREATE INDEX [IX_SchoolAttendances_SchoolReserveId] ON [SchoolAttendances] ([SchoolReserveId]);

CREATE UNIQUE INDEX [IX_SchoolCertificates_Code] ON [SchoolCertificates] ([Code]);

CREATE UNIQUE INDEX [IX_SchoolCertificates_SchoolReserveId] ON [SchoolCertificates] ([SchoolReserveId]);

CREATE UNIQUE INDEX [IX_SchoolCourseInstructors_SchoolCourseId_UserId] ON [SchoolCourseInstructors] ([SchoolCourseId], [UserId]) WHERE [Deleted] = 0;

CREATE INDEX [IX_SchoolCourseInstructors_UserId] ON [SchoolCourseInstructors] ([UserId]);

CREATE INDEX [IX_SchoolCourseSkills_SchoolCourseId] ON [SchoolCourseSkills] ([SchoolCourseId]);

CREATE UNIQUE INDEX [IX_SchoolCourseWaitlists_SchoolCourseId_UserPetId] ON [SchoolCourseWaitlists] ([SchoolCourseId], [UserPetId]) WHERE [Deleted] = 0;

CREATE INDEX [IX_SchoolCourseWaitlists_UserId] ON [SchoolCourseWaitlists] ([UserId]);

CREATE INDEX [IX_SchoolCourseWaitlists_UserPetId] ON [SchoolCourseWaitlists] ([UserPetId]);

CREATE INDEX [IX_SchoolProgresses_SchoolCourseSessionId] ON [SchoolProgresses] ([SchoolCourseSessionId]);

CREATE INDEX [IX_SchoolProgresses_SchoolCourseSkillId] ON [SchoolProgresses] ([SchoolCourseSkillId]);

CREATE INDEX [IX_SchoolProgresses_SchoolReserveId] ON [SchoolProgresses] ([SchoolReserveId]);

ALTER TABLE [SchoolComments] ADD CONSTRAINT [FK_SchoolComments_SchoolCourses_SchoolCourseId] FOREIGN KEY ([SchoolCourseId]) REFERENCES [SchoolCourses] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [SchoolCourses] ADD CONSTRAINT [FK_SchoolCourses_SchoolCourses_PrerequisiteCourseId] FOREIGN KEY ([PrerequisiteCourseId]) REFERENCES [SchoolCourses] ([Id]) ON DELETE NO ACTION;

IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 169 AND Label <> N'PushSchoolWaitlistSeatOpen')
    THROW 51000, 'PushType ID 169 is already assigned to another label.', 1;
SET IDENTITY_INSERT PushTypes ON;
IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 169)
    INSERT INTO PushTypes (Id, Name, Label) VALUES (169, N'جا در دوره‌ی لیست انتظار خالی شد', N'PushSchoolWaitlistSeatOpen');
SET IDENTITY_INSERT PushTypes OFF;
IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 169)
    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
    VALUES (169, N'در دوره‌ی {0} جا باز شد', N'زودتر ثبت‌نام کنید؛ جای خالی برای همه‌ی افراد لیست انتظار اعلام شده است.', N'/school/course-{1}', NULL, N'school-waitlist-open', 1);
INSERT INTO PushSettings (PushPatternId, IsEnabled)
SELECT pattern.Id, 1 FROM PushPatterns pattern
WHERE pattern.PushTypeId = 169
  AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 170 AND Label <> N'PushSchoolClassReminderDayBefore')
    THROW 51000, 'PushType ID 170 is already assigned to another label.', 1;
SET IDENTITY_INSERT PushTypes ON;
IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 170)
    INSERT INTO PushTypes (Id, Name, Label) VALUES (170, N'یادآوری یک روز مانده به جلسه‌ی مدرسه', N'PushSchoolClassReminderDayBefore');
SET IDENTITY_INSERT PushTypes OFF;
IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 170)
    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
    VALUES (170, N'فردا کلاس دارید', N'جلسه‌ی دوره‌ی {0} فردا ساعت {1} برگزار می‌شود.', N'/school/course-{2}', NULL, N'school-class-reminder-day', 1);
INSERT INTO PushSettings (PushPatternId, IsEnabled)
SELECT pattern.Id, 1 FROM PushPatterns pattern
WHERE pattern.PushTypeId = 170
  AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 171 AND Label <> N'PushSchoolSessionMissed')
    THROW 51000, 'PushType ID 171 is already assigned to another label.', 1;
SET IDENTITY_INSERT PushTypes ON;
IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 171)
    INSERT INTO PushTypes (Id, Name, Label) VALUES (171, N'جلسه‌ی از دست رفته‌ی مدرسه', N'PushSchoolSessionMissed');
SET IDENTITY_INSERT PushTypes OFF;
IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 171)
    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
    VALUES (171, N'جلسه را از دست دادید', N'در جلسه‌ی دوره‌ی {0} غایب بودید. اگر ضبط جلسه موجود باشد از صفحه‌ی دوره می‌بینید.', N'/school/course-{1}', NULL, N'school-session-missed', 1);
INSERT INTO PushSettings (PushPatternId, IsEnabled)
SELECT pattern.Id, 1 FROM PushPatterns pattern
WHERE pattern.PushTypeId = 171
  AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 172 AND Label <> N'PushSchoolCertificateIssued')
    THROW 51000, 'PushType ID 172 is already assigned to another label.', 1;
SET IDENTITY_INSERT PushTypes ON;
IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 172)
    INSERT INTO PushTypes (Id, Name, Label) VALUES (172, N'صدور گواهی پایان دوره‌ی مدرسه', N'PushSchoolCertificateIssued');
SET IDENTITY_INSERT PushTypes OFF;
IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 172)
    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
    VALUES (172, N'گواهی پایان دوره صادر شد', N'گواهی دوره‌ی {0} برای {1} صادر شد. کد گواهی: {2}', N'/school/certificate-{2}', NULL, N'school-certificate', 1);
INSERT INTO PushSettings (PushPatternId, IsEnabled)
SELECT pattern.Id, 1 FROM PushPatterns pattern
WHERE pattern.PushTypeId = 172
  AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261028055824_SchoolCourseFeatures', N'9.0.0');

COMMIT;
GO

