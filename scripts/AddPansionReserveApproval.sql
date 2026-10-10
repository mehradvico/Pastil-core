BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007110445_AddPansionReserveOwnerApproval'
)
BEGIN
    ALTER TABLE [PansionReserves] ADD [OwnerApprovalDeadline] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007110445_AddPansionReserveOwnerApproval'
)
BEGIN
    ALTER TABLE [PansionReserves] ADD [OwnerDecision] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007110445_AddPansionReserveOwnerApproval'
)
BEGIN
    ALTER TABLE [PansionReserves] ADD [OwnerDecisionDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007110445_AddPansionReserveOwnerApproval'
)
BEGIN
    ALTER TABLE [PansionReserves] ADD [OwnerDecisionReason] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007110445_AddPansionReserveOwnerApproval'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261007110445_AddPansionReserveOwnerApproval', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007110528_SeedPansionReserveApprovalPush'
)
BEGIN
    DECLARE @Types TABLE (Id bigint, Name nvarchar(200), Label nvarchar(200), Title nvarchar(200), Body nvarchar(500), Url nvarchar(200), Tag nvarchar(200));
    INSERT INTO @Types (Id, Name, Label, Title, Body, Url, Tag) VALUES
        (114, N'رزرو پانسیون/مهد منتظر تأیید مرکز (به مرکز)', N'PushPansionReserveApprovalRequired', N'رزرو جدید منتظر تأیید', N'رزرو جدید {1} در {0} منتظر تأیید شماست.', N'/companionProfile/pansionReserve', N'pansion-reserve-approval'),
        (115, N'رزرو پانسیون/مهد تأیید شد (به کاربر)', N'PushPansionReserveApproved', N'رزرو شما تأیید شد', N'رزرو {1} در {0} توسط مرکز تأیید شد.', N'/reserve', N'pansion-reserve-approved'),
        (116, N'رزرو پانسیون/مهد توسط مرکز رد شد (به کاربر)', N'PushPansionReserveRejected', N'رزرو شما رد شد', N'رزرو شما در {0} توسط مرکز رد شد و مبلغ به کیف پول شما برگشت. دلیل: {1}', N'/reserve', N'pansion-reserve-rejected'),
        (117, N'مرکز به رزرو پاسخ نداد، رزرو لغو شد (به کاربر)', N'PushPansionReserveExpired', N'رزرو شما لغو شد', N'مرکز {0} در مهلت مقرر به رزرو شما پاسخ نداد؛ رزرو لغو و مبلغ به کیف پول شما برگشت.', N'/reserve', N'pansion-reserve-expired');

    IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Id = s.Id WHERE t.Label <> s.Label)
        THROW 51000, 'A pansion reserve approval push type ID is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Label = s.Label WHERE t.Id <> s.Id)
        THROW 51000, 'A pansion reserve approval push label is already assigned to another ID.', 1;

    SET IDENTITY_INSERT PushTypes ON;
    INSERT INTO PushTypes (Id, Name, Label)
    SELECT s.Id, s.Name, s.Label FROM @Types s
    WHERE NOT EXISTS (SELECT 1 FROM PushTypes t WHERE t.Id = s.Id);
    SET IDENTITY_INSERT PushTypes OFF;

    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
    SELECT s.Id, s.Title, s.Body, s.Url, NULL, s.Tag, 1 FROM @Types s
    WHERE NOT EXISTS (SELECT 1 FROM PushPatterns p WHERE p.PushTypeId = s.Id);

    INSERT INTO PushSettings (PushPatternId, IsEnabled)
    SELECT pattern.Id, 1
    FROM PushPatterns pattern
    WHERE pattern.PushTypeId IN (114, 115, 116, 117)
      AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007110528_SeedPansionReserveApprovalPush'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261007110528_SeedPansionReserveApprovalPush', N'9.0.0');
END;

COMMIT;
GO

