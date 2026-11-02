-- کارمندان پنل ارائه‌دهنده + نقش‌ها و دسترسی‌هایشان (ProviderUsers، ۲۰۲۶-۱۱-۰۳). تکرارپذیر: اگر قبلاً اجرا شده باشد کاری نمی‌کند.
-- فقط سه جدول جدید می‌سازد (ProviderUserRoles، ProviderUserRolePermissions، ProviderUsers)؛ به جدول موجودی دست نمی‌زند.
-- ترتیب: اول این اسکریپت، بعد دیپلوی Api. توضیح کامل: backend/Docs/PROVIDER_USERS_ROLES_PERMISSIONS_FA.md
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102221042_ProviderUsers'
)
BEGIN
    CREATE TABLE [ProviderUserRoles] (
        [Id] bigint NOT NULL IDENTITY,
        [ProviderId] bigint NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [Deleted] bit NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        CONSTRAINT [PK_ProviderUserRoles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProviderUserRoles_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102221042_ProviderUsers'
)
BEGIN
    CREATE TABLE [ProviderUserRolePermissions] (
        [Id] bigint NOT NULL IDENTITY,
        [ProviderUserRoleId] bigint NOT NULL,
        [PermissionKey] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_ProviderUserRolePermissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProviderUserRolePermissions_ProviderUserRoles_ProviderUserRoleId] FOREIGN KEY ([ProviderUserRoleId]) REFERENCES [ProviderUserRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102221042_ProviderUsers'
)
BEGIN
    CREATE TABLE [ProviderUsers] (
        [Id] bigint NOT NULL IDENTITY,
        [ProviderId] bigint NOT NULL,
        [UserId] bigint NOT NULL,
        [ProviderUserRoleId] bigint NOT NULL,
        [UserAccept] bit NULL,
        [InviteDate] datetime2 NULL,
        [Active] bit NOT NULL,
        [Deleted] bit NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        CONSTRAINT [PK_ProviderUsers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProviderUsers_ProviderUserRoles_ProviderUserRoleId] FOREIGN KEY ([ProviderUserRoleId]) REFERENCES [ProviderUserRoles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProviderUsers_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProviderUsers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102221042_ProviderUsers'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProviderUserRolePermissions_ProviderUserRoleId_PermissionKey] ON [ProviderUserRolePermissions] ([ProviderUserRoleId], [PermissionKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102221042_ProviderUsers'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ProviderUserRoles_ProviderId_Name] ON [ProviderUserRoles] ([ProviderId], [Name]) WHERE [Deleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102221042_ProviderUsers'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ProviderUsers_ProviderId_UserId] ON [ProviderUsers] ([ProviderId], [UserId]) WHERE [Deleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102221042_ProviderUsers'
)
BEGIN
    CREATE INDEX [IX_ProviderUsers_ProviderUserRoleId] ON [ProviderUsers] ([ProviderUserRoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102221042_ProviderUsers'
)
BEGIN
    CREATE INDEX [IX_ProviderUsers_UserId] ON [ProviderUsers] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102221042_ProviderUsers'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261102221042_ProviderUsers', N'9.0.0');
END;

COMMIT;
GO

