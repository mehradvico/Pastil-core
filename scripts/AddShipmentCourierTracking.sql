BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017170431_AddShipmentCourierTracking'
)
BEGIN
    ALTER TABLE [Shipments] ADD [CourierLatitude] float NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017170431_AddShipmentCourierTracking'
)
BEGIN
    ALTER TABLE [Shipments] ADD [CourierLocationAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017170431_AddShipmentCourierTracking'
)
BEGIN
    ALTER TABLE [Shipments] ADD [CourierLongitude] float NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017170431_AddShipmentCourierTracking'
)
BEGIN
    ALTER TABLE [Shipments] ADD [CourierName] nvarchar(200) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017170431_AddShipmentCourierTracking'
)
BEGIN
    ALTER TABLE [Shipments] ADD [CourierPhone] nvarchar(40) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017170431_AddShipmentCourierTracking'
)
BEGIN
    ALTER TABLE [Shipments] ADD [LastMiareEventAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017170431_AddShipmentCourierTracking'
)
BEGIN
    ALTER TABLE [Shipments] ADD [LastMiareState] nvarchar(60) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017170431_AddShipmentCourierTracking'
)
BEGIN
    CREATE TABLE [ShipmentEvents] (
        [Id] bigint NOT NULL IDENTITY,
        [ShipmentId] bigint NOT NULL,
        [State] nvarchar(60) NULL,
        [ReceivedAtUtc] datetime2 NOT NULL,
        [CourierName] nvarchar(200) NULL,
        [Latitude] float NULL,
        [Longitude] float NULL,
        [PayloadHash] nvarchar(64) NULL,
        [PayloadJson] nvarchar(max) NULL,
        CONSTRAINT [PK_ShipmentEvents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ShipmentEvents_Shipments_ShipmentId] FOREIGN KEY ([ShipmentId]) REFERENCES [Shipments] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017170431_AddShipmentCourierTracking'
)
BEGIN
    CREATE INDEX [IX_ShipmentEvents_ShipmentId_ReceivedAtUtc] ON [ShipmentEvents] ([ShipmentId], [ReceivedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017170431_AddShipmentCourierTracking'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261017170431_AddShipmentCourierTracking', N'9.0.0');
END;

COMMIT;
GO

