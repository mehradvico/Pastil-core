-- Equivalent of EF migration 20261118120000_VenueRebateFunding.
-- Run once against pastil_db in one transaction; does not change existing rebate rows.
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = N'20261116090000_ProviderOnboardingBankInfo')
    THROW 51000, 'Expected baseline migration is missing.', 1;
IF EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = N'20261118120000_VenueRebateFunding')
    THROW 51001, 'VenueRebateFunding is already applied.', 1;
IF COL_LENGTH('dbo.Rebate', 'SiteFundedPercent') IS NOT NULL OR COL_LENGTH('dbo.Rebate', 'ProviderFundedPercent') IS NOT NULL
    THROW 51002, 'Funding column already exists without migration history.', 1;

ALTER TABLE dbo.Rebate ADD
    SiteFundedPercent decimal(5,2) NULL,
    ProviderFundedPercent decimal(5,2) NULL;

EXEC(N'ALTER TABLE dbo.Rebate ADD CONSTRAINT CK_Rebate_FundingPercent CHECK (
    (SiteFundedPercent IS NULL AND ProviderFundedPercent IS NULL) OR
    (SiteFundedPercent IS NOT NULL AND ProviderFundedPercent IS NOT NULL AND
     SiteFundedPercent BETWEEN 0 AND 100 AND ProviderFundedPercent BETWEEN 0 AND 100 AND
     SiteFundedPercent + ProviderFundedPercent = 100)
)');

INSERT dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
VALUES (N'20261118120000_VenueRebateFunding', N'9.0.0');

COMMIT TRANSACTION;
