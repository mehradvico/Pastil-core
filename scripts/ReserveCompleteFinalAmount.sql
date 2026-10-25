BEGIN TRANSACTION;
ALTER TABLE [GroomerReserves] ADD [CompleteDetail] nvarchar(500) NULL;

ALTER TABLE [GroomerReserves] ADD [CompletedByUserId] bigint NULL;

ALTER TABLE [GroomerReserves] ADD [DebtAmount] float NOT NULL DEFAULT 0.0E0;

ALTER TABLE [GroomerReserves] ADD [DebtPaidByWallet] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [GroomerReserves] ADD [DebtPaidDate] datetime2 NULL;

ALTER TABLE [GroomerReserves] ADD [FinalAmount] float NULL;

ALTER TABLE [GroomerReserves] ADD [NotReceived] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [GroomerReserves] ADD [ReceivedAtVenue] float NOT NULL DEFAULT 0.0E0;

ALTER TABLE [CoachReserves] ADD [CompleteDetail] nvarchar(500) NULL;

ALTER TABLE [CoachReserves] ADD [CompletedByUserId] bigint NULL;

ALTER TABLE [CoachReserves] ADD [DebtAmount] float NOT NULL DEFAULT 0.0E0;

ALTER TABLE [CoachReserves] ADD [DebtPaidByWallet] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [CoachReserves] ADD [DebtPaidDate] datetime2 NULL;

ALTER TABLE [CoachReserves] ADD [FinalAmount] float NULL;

ALTER TABLE [CoachReserves] ADD [NotReceived] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [CoachReserves] ADD [ReceivedAtVenue] float NOT NULL DEFAULT 0.0E0;

ALTER TABLE [ClinicReserves] ADD [CompleteDetail] nvarchar(500) NULL;

ALTER TABLE [ClinicReserves] ADD [CompletedByUserId] bigint NULL;

ALTER TABLE [ClinicReserves] ADD [DebtAmount] float NOT NULL DEFAULT 0.0E0;

ALTER TABLE [ClinicReserves] ADD [DebtPaidByWallet] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [ClinicReserves] ADD [DebtPaidDate] datetime2 NULL;

ALTER TABLE [ClinicReserves] ADD [FinalAmount] float NULL;

ALTER TABLE [ClinicReserves] ADD [NotReceived] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [ClinicReserves] ADD [ReceivedAtVenue] float NOT NULL DEFAULT 0.0E0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261025104312_ReserveCompleteFinalAmount', N'9.0.0');

COMMIT;
GO

