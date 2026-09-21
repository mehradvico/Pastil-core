BEGIN TRANSACTION;
ALTER TABLE [UserBankCards] ADD [CardNumberHash] nvarchar(64) NULL;

CREATE INDEX [IX_UserBankCards_CardNumberHash] ON [UserBankCards] ([CardNumberHash]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260920112112_AddBankCardEncryption', N'9.0.0');

COMMIT;
GO

