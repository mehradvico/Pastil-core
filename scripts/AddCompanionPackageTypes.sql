BEGIN TRANSACTION;
CREATE TABLE [CompanionAssistancePackageTypes] (
    [Id] bigint NOT NULL IDENTITY,
    [CompanionAssistancePackageId] bigint NOT NULL,
    [CompanionAssistanceTypeId] bigint NOT NULL,
    [Price] float NOT NULL,
    [PrePaymentPrice] float NOT NULL,
    [Deleted] bit NOT NULL,
    CONSTRAINT [PK_CompanionAssistancePackageTypes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CompanionAssistancePackageTypes_Codes_CompanionAssistanceTypeId] FOREIGN KEY ([CompanionAssistanceTypeId]) REFERENCES [Codes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CompanionAssistancePackageTypes_CompanionAssistancePackages_CompanionAssistancePackageId] FOREIGN KEY ([CompanionAssistancePackageId]) REFERENCES [CompanionAssistancePackages] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_CompanionAssistancePackageTypes_CompanionAssistancePackageId_CompanionAssistanceTypeId] ON [CompanionAssistancePackageTypes] ([CompanionAssistancePackageId], [CompanionAssistanceTypeId]);

CREATE INDEX [IX_CompanionAssistancePackageTypes_CompanionAssistanceTypeId] ON [CompanionAssistancePackageTypes] ([CompanionAssistanceTypeId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260920112939_AddCompanionPackageTypes', N'9.0.0');

COMMIT;
GO

