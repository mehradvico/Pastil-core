BEGIN TRANSACTION;
ALTER TABLE [PriceCalculations] ADD [ExtraPetPrice] float NOT NULL DEFAULT 0.0E0;

ALTER TABLE [PriceCalculations] ADD [PickupVehicleExtraPrice] float NOT NULL DEFAULT 0.0E0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260923090616_AddTripExtraPetAndVehiclePrice', N'9.0.0');

COMMIT;
GO

