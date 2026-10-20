BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019060733_AddProviderRoles'
)
BEGIN
    CREATE TABLE [ProviderRoles] (
        [Id] bigint NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Label] nvarchar(50) NOT NULL,
        [Active] bit NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_ProviderRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019060733_AddProviderRoles'
)
BEGIN
    CREATE TABLE [ProviderRoleAssignments] (
        [Id] bigint NOT NULL IDENTITY,
        [ProviderId] bigint NOT NULL,
        [ProviderRoleId] bigint NOT NULL,
        [Approved] bit NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_ProviderRoleAssignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProviderRoleAssignments_ProviderRoles_ProviderRoleId] FOREIGN KEY ([ProviderRoleId]) REFERENCES [ProviderRoles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProviderRoleAssignments_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019060733_AddProviderRoles'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ProviderRoleAssignments_ProviderId_ProviderRoleId] ON [ProviderRoleAssignments] ([ProviderId], [ProviderRoleId]) WHERE [Deleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019060733_AddProviderRoles'
)
BEGIN
    CREATE INDEX [IX_ProviderRoleAssignments_ProviderRoleId] ON [ProviderRoleAssignments] ([ProviderRoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019060733_AddProviderRoles'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ProviderRoles_Label] ON [ProviderRoles] ([Label]) WHERE [Deleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019060733_AddProviderRoles'
)
BEGIN

    INSERT INTO ProviderRoles (Name, Label, Active, Deleted) VALUES
     (N'فروشگاه', N'Store', 1, 0),
     (N'کلینیک', N'Clinic', 1, 0),
     (N'مربی', N'Coach', 1, 0),
     (N'آرایشگاه', N'Groomer', 1, 0),
     (N'پانسیون', N'Pansion', 1, 0),
     (N'مدرسه', N'School', 1, 0);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019060733_AddProviderRoles'
)
BEGIN

    INSERT INTO ProviderRoleAssignments (ProviderId, ProviderRoleId, Approved, Deleted)
    SELECT c.ProviderId, r.Id, c.Approved, c.Deleted
    FROM ProviderCapabilities c
    JOIN ProviderRoles r ON r.Label = CASE c.[Type]
        WHEN 1 THEN N'Store' WHEN 2 THEN N'Groomer' WHEN 3 THEN N'Coach' WHEN 4 THEN N'Clinic' WHEN 5 THEN N'Pansion' END;

    -- ارائه‌دهندگانی که مدرسه دارند ولی نقش مدرسه ندارند
    INSERT INTO ProviderRoleAssignments (ProviderId, ProviderRoleId, Approved, Deleted)
    SELECT cp.ProviderId, r.Id, MAX(CAST(sc.Approve AS int)), 0
    FROM Schools sc
    JOIN Companions cp ON cp.Id = sc.CompanionId
    JOIN ProviderRoles r ON r.Label = N'School'
    WHERE cp.ProviderId IS NOT NULL AND sc.Deleted = 0
      AND NOT EXISTS (SELECT 1 FROM ProviderRoleAssignments a WHERE a.ProviderId = cp.ProviderId AND a.ProviderRoleId = r.Id AND a.Deleted = 0)
    GROUP BY cp.ProviderId, r.Id;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019060733_AddProviderRoles'
)
BEGIN
    DROP TABLE [ProviderCapabilities];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019060733_AddProviderRoles'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261019060733_AddProviderRoles', N'9.0.0');
END;

COMMIT;
GO

