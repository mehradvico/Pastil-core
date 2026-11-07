BEGIN TRANSACTION;
ALTER TABLE [Pansions] DROP CONSTRAINT [FK_Pansions_Companions_CompanionId];

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Pansions]') AND [c].[name] = N'CompanionId');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Pansions] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Pansions] ALTER COLUMN [CompanionId] bigint NULL;

ALTER TABLE [Pansions] ADD [ProviderId] bigint NULL;

CREATE INDEX [IX_Pansions_ProviderId] ON [Pansions] ([ProviderId]);

ALTER TABLE [Pansions] ADD CONSTRAINT [FK_Pansions_Companions_CompanionId] FOREIGN KEY ([CompanionId]) REFERENCES [Companions] ([Id]);

ALTER TABLE [Pansions] ADD CONSTRAINT [FK_Pansions_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]);

EXEC(N'UPDATE p SET p.ProviderId = c.ProviderId
FROM Pansions p INNER JOIN Companions c ON c.Id = p.CompanionId
WHERE p.ProviderId IS NULL AND c.ProviderId IS NOT NULL;');

EXEC(N'ALTER TABLE Pansions ADD CONSTRAINT CK_Pansions_Owner CHECK (CompanionId IS NOT NULL OR ProviderId IS NOT NULL);');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261103102835_PansionProviderOwner', N'9.0.0');

COMMIT;
GO

