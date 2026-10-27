BEGIN TRANSACTION;
ALTER TABLE [Schools] DROP CONSTRAINT [FK_Schools_Companions_CompanionId];

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Schools]') AND [c].[name] = N'CompanionId');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Schools] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Schools] ALTER COLUMN [CompanionId] bigint NULL;

ALTER TABLE [Schools] ADD [ProviderId] bigint NULL;

CREATE INDEX [IX_Schools_ProviderId] ON [Schools] ([ProviderId]);

ALTER TABLE [Schools] ADD CONSTRAINT [FK_Schools_Companions_CompanionId] FOREIGN KEY ([CompanionId]) REFERENCES [Companions] ([Id]);

ALTER TABLE [Schools] ADD CONSTRAINT [FK_Schools_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]);

EXEC(N'UPDATE s SET s.ProviderId = c.ProviderId
FROM Schools s INNER JOIN Companions c ON c.Id = s.CompanionId
WHERE s.ProviderId IS NULL AND c.ProviderId IS NOT NULL;');

EXEC(N'ALTER TABLE Schools ADD CONSTRAINT CK_Schools_Owner CHECK (CompanionId IS NOT NULL OR ProviderId IS NOT NULL);');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261026074332_SchoolProviderOwner', N'9.0.0');

COMMIT;
GO

