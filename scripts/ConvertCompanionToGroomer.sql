-- تبدیل یک «درخواست نمایندگی قدیمی» (Companion بدون نوع/خدمت) به آرایشگاه (Groomer) + تأیید ارائه‌دهنده و نقش Groomer.
-- (همان ساختار ConvertCompanionToPansion.sql؛ برای حالتی که درخواست از مسیر قدیمی POST /EndUser/Companion ثبت شده.)
--
-- ایمنی:
--  * Idempotent: اگر Groomers.LegacyCompanionId = @CompanionId از قبل وجود داشته باشد، آرایشگاه دوباره ساخته نمی‌شود.
--  * ردیف Companion پاک نمی‌شود؛ با LegacyCompanionId از لیست نمایندگی‌ها پنهان می‌شود (CompanionMigrationFilter.NotFullyMigrated).
--  * اگر Companion خدمت/پرسنل/ساعت/رزرو داشته باشد اسکریپت متوقف می‌شود (برای آن‌ها از MigrateCompanionGroomersToGroomers.sql استفاده کنید).
--  * همه‌چیز در یک تراکنش است؛ اول @Commit = 0 (ROLLBACK + گزارش)، بعد @Commit = 1.

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

DECLARE @CompanionId bigint = 10025;
DECLARE @Commit bit = 0;   -- 0 = فقط بررسی (ROLLBACK)، 1 = ذخیره

BEGIN TRANSACTION;

DECLARE @OwnerId bigint, @ProviderId bigint, @GroomerId bigint;
SELECT @OwnerId = c.OwnerId, @ProviderId = c.ProviderId FROM Companions c WHERE c.Id = @CompanionId AND c.Deleted = 0;
IF @OwnerId IS NULL OR @ProviderId IS NULL
BEGIN
    THROW 50000, N'Companion پیدا نشد یا ProviderId ندارد.', 1;
END

IF EXISTS (SELECT 1 FROM CompanionAssistances WHERE CompanionId = @CompanionId)
   OR EXISTS (SELECT 1 FROM CompanionUsers WHERE CompanionId = @CompanionId)
   OR EXISTS (SELECT 1 FROM CompanionTimes WHERE CompanionId = @CompanionId)
BEGIN
    THROW 50000, N'این Companion خدمت/پرسنل/ساعت دارد؛ از MigrateCompanionGroomersToGroomers.sql استفاده کنید.', 1;
END

-- ۱) ارائه‌دهنده: تأیید و فعال (مثل تأیید درخواست Provider در پنل)
UPDATE Providers SET Approved = 1, Active = 1 WHERE Id = @ProviderId AND UserId = @OwnerId AND Deleted = 0;
UPDATE Users SET IsProvider = 1 WHERE Id = @OwnerId;

-- ۲) نقش Groomer
DECLARE @RoleId bigint = (SELECT Id FROM ProviderRoles WHERE Label = N'Groomer' AND Active = 1 AND Deleted = 0);
IF @RoleId IS NULL THROW 50000, N'نقش Groomer فعال نیست.', 1;

IF EXISTS (SELECT 1 FROM ProviderRoleAssignments WHERE ProviderId = @ProviderId AND ProviderRoleId = @RoleId AND Deleted = 0)
    UPDATE ProviderRoleAssignments SET Approved = 1 WHERE ProviderId = @ProviderId AND ProviderRoleId = @RoleId AND Deleted = 0;
ELSE
    INSERT INTO ProviderRoleAssignments (ProviderId, ProviderRoleId, Approved, Deleted) VALUES (@ProviderId, @RoleId, 1, 0);

-- ۳) آرایشگاه (مثل ساخت توسط ادمین: Active/Approved = 1، ShowToSite = 0)
SELECT @GroomerId = Id FROM Groomers WHERE LegacyCompanionId = @CompanionId AND Deleted = 0;
IF @GroomerId IS NULL
BEGIN
    INSERT INTO Groomers
        (Name, Summary, Description, ProviderId, OwnerId, LegacyCompanionId, IsPersonal, ReferralCode,
         PictureId, BackgroundPictureId, IconId, Phone, AddressValue, StateId, CityId, NeighborhoodId, Location,
         Active, ShowToSite, Approved, ActivationValue, Suggested, Deleted, DeleteDate, DeletedByUserId, CreateDate,
         CommentCount, RateAvg, RateCount, SearchKey,
         LicenseApproved, ServiceAtHome, ServiceInSalon, PickupAvailable, PickupFee, HasParking, HasWaitingRoom, HasCamera,
         MaxConcurrentPets, StaffCount, PortfolioCount, PortfolioLikeCount, CommissionPercent, SeoNoIndex, SeoNoFollow)
    SELECT
        c.Name, c.Summary, c.Description, @ProviderId, @OwnerId, c.Id, c.IsPersonal, c.ReferralCode,
        c.PictureId, c.BackgroundPictureId, c.IconId, c.Phone, c.AddressValue, ci.StateId, c.CityId, c.NeighborhoodId, c.Location,
        1, 0, 1, NULL, 0, 0, NULL, NULL, SYSDATETIME(),
        0, 0, 0, CONCAT_WS(N' ', c.Name, c.Summary, c.AddressValue, c.Phone),
        0, 0, 1, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0
    FROM Companions c
    LEFT JOIN Cities ci ON ci.Id = c.CityId
    WHERE c.Id = @CompanionId;
    SET @GroomerId = SCOPE_IDENTITY();
END

-- ۴) گزارش
SELECT 'Provider' AS Item, p.Id, p.Approved, p.Active FROM Providers p WHERE p.Id = @ProviderId;
SELECT 'Role' AS Item, a.ProviderId, a.ProviderRoleId, a.Approved FROM ProviderRoleAssignments a WHERE a.ProviderId = @ProviderId AND a.ProviderRoleId = @RoleId AND a.Deleted = 0;
SELECT 'Groomer' AS Item, g.Id, g.ProviderId, g.OwnerId, g.LegacyCompanionId, g.Active, g.Approved, g.CityId, g.StateId FROM Groomers g WHERE g.Id = @GroomerId;
SELECT 'User' AS Item, u.Id, u.IsProvider FROM Users u WHERE u.Id = @OwnerId;

IF @Commit = 1
BEGIN
    COMMIT TRANSACTION;
    PRINT N'COMMIT انجام شد.';
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT N'ROLLBACK شد (فقط بررسی). برای ذخیره @Commit را 1 کنید.';
END
