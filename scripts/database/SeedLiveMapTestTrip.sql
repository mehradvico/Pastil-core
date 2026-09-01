-- یک سفرِ «پذیرفته‌شده» تستی می‌سازد (یا تازه می‌کند) تا نقشه‌ی زنده‌ی ادمین (/admin/trip/live) چیزی برای
-- نمایش داشته باشد: یک راننده‌ی فعال/تاییدشده + یک کاربر/پت موجود را به هم وصل می‌کند و برای هر دو طرف
-- یک ردیف UserCurrentLocation (لوکیشن زنده) ثبت/به‌روزرسانی می‌کند.
--
-- idempotent است — با اجرای دوباره، همون سفر تستی رو (به جای ساخت یک سفر تکراری) به‌روزرسانی می‌کنه؛
-- شناسه‌ش رو با متن مشخصِ '__LIVE_MAP_TEST_TRIP__' در UserComment ردیابی می‌کنه.
--
-- پیش‌نیاز: حداقل یک راننده‌ی Active=1 و Approved=1 و Deleted=0، و حداقل یک UserPet موجود باشه؛
-- اگه هیچ‌کدوم نبود، اسکریپت با یک پیام فارسی مشخص متوقف می‌شه (چیزی رو حدسی نمی‌سازه).

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @DriverId bigint = (SELECT TOP 1 Id FROM dbo.Drivers WHERE Active = 1 AND Approved = 1 AND Deleted = 0 ORDER BY Id);
IF @DriverId IS NULL
    THROW 51030, N'هیچ راننده‌ی فعال و تاییدشده‌ای پیدا نشد — اول یک راننده بساز (همونی که توی پنل دیدی کافیه).', 1;

DECLARE @DriverOwnerId bigint = (SELECT OwnerId FROM dbo.Drivers WHERE Id = @DriverId);

DECLARE @UserPetId bigint, @UserId bigint, @CityId bigint;
SELECT TOP 1 @UserPetId = Id, @UserId = UserId FROM dbo.UserPets WHERE Deleted = 0 ORDER BY Id;
IF @UserPetId IS NULL
    THROW 51031, N'هیچ پتِ ثبت‌شده‌ای (UserPet) پیدا نشد — اول یک کاربر با حداقل یک پت ثبت‌شده لازم داری.', 1;

SET @CityId = (SELECT TOP 1 Id FROM dbo.Cities ORDER BY Id);
IF @CityId IS NULL
    THROW 51032, N'هیچ شهری توی دیتابیس نیست — CityId لازمه و پیدا نشد.', 1;

-- مختصات دو تا لندمارک واقعی و مستندِ تهرانن (برج آزادی و برج میلاد — طبق ویکی‌پدیا)، نه حدسی:
--   برج آزادی: 35.69944, 51.33778   https://en.wikipedia.org/wiki/Azadi_Tower
--   برج میلاد: 35.74472, 51.37528   https://en.wikipedia.org/wiki/Milad_Tower
DECLARE @Marker nvarchar(50) = N'__LIVE_MAP_TEST_TRIP__';
DECLARE @Origin geography = geography::STPointFromText('POINT(51.33778 35.69944)', 4326);      -- مبدا: برج آزادی
DECLARE @Destination geography = geography::STPointFromText('POINT(51.37528 35.74472)', 4326); -- مقصد: برج میلاد
DECLARE @DriverLoc geography = geography::STPointFromText('POINT(51.341 35.702)', 4326);        -- راننده، چند صد متری برج آزادی
DECLARE @UserLoc geography = geography::STPointFromText('POINT(51.3385 35.6998)', 4326);        -- کاربر، نزدیک برج آزادی

DECLARE @TripId bigint = (SELECT TOP 1 Id FROM dbo.Trips WHERE UserComment = @Marker ORDER BY Id);

IF @TripId IS NULL
BEGIN
    INSERT INTO dbo.Trips
    (
        Origin, Destination, RouteLength, FromCityId, RoundTrip, FromAddress, ToAddress,
        DriverId, Price, UserDetail, UserComment, UserRate, ConnectionId, UserToken,
        CreateDate, TripStartDateTime, IsOnline, DriverStatusId, TripStatusId, IsPaid,
        UserPetId, UserId, DriverShare, SiteShare, RebatePrice, Discount, PaymentPrice,
        FromWallet, WalletPrice, ProgressStageId, IsReturnLeg, ProgressUpdateDate,
        OwnerRidesAlong, ScheduledDispatched
    )
    VALUES
    (
        @Origin, @Destination, 6000, @CityId, 0, N'برج آزادی، تهران', N'برج میلاد، تهران',
        @DriverId, 150000, NULL, @Marker, 0, NULL, NULL,
        GETDATE(), GETDATE(), 1, 80 /*DriverStatus_Accepted*/, 76 /*TripStatus_Accepted*/, 0,
        @UserPetId, @UserId, 0, 0, 0, 0, 150000,
        0, 0, 1 /*EnRouteOrigin*/, 0, GETDATE(),
        0, 0
    );

    SET @TripId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    UPDATE dbo.Trips
    SET
        Origin = @Origin, Destination = @Destination, DriverId = @DriverId,
        UserPetId = @UserPetId, UserId = @UserId, FromCityId = @CityId,
        FromAddress = N'برج آزادی، تهران', ToAddress = N'برج میلاد، تهران',
        DriverStatusId = 80, TripStatusId = 76, ProgressStageId = 1, IsOnline = 1,
        ProgressUpdateDate = GETDATE()
    WHERE Id = @TripId;
END

-- اگه سفر چند-پتی (TripPet) هنوز نداره، همین یک پت رو بهش وصل کن
IF NOT EXISTS (SELECT 1 FROM dbo.TripPets WHERE TripId = @TripId)
    INSERT INTO dbo.TripPets (TripId, UserPetId) VALUES (@TripId, @UserPetId);

-- لوکیشن زنده‌ی راننده (روی User.Id خودِ راننده، نه Driver.Id)
IF EXISTS (SELECT 1 FROM dbo.UserCurrentLocations WHERE UserId = @DriverOwnerId)
    UPDATE dbo.UserCurrentLocations SET Location = @DriverLoc, CityId = @CityId, LastUpdateDate = GETDATE() WHERE UserId = @DriverOwnerId;
ELSE
    INSERT INTO dbo.UserCurrentLocations (UserId, Location, CityId, NeighborhoodId, LastUpdateDate)
    VALUES (@DriverOwnerId, @DriverLoc, @CityId, NULL, GETDATE());

-- لوکیشن زنده‌ی کاربر (صاحب پت)
IF EXISTS (SELECT 1 FROM dbo.UserCurrentLocations WHERE UserId = @UserId)
    UPDATE dbo.UserCurrentLocations SET Location = @UserLoc, CityId = @CityId, LastUpdateDate = GETDATE() WHERE UserId = @UserId;
ELSE
    INSERT INTO dbo.UserCurrentLocations (UserId, Location, CityId, NeighborhoodId, LastUpdateDate)
    VALUES (@UserId, @UserLoc, @CityId, NULL, GETDATE());

COMMIT TRANSACTION;

SELECT
    t.Id AS TripId, t.TripStatusId, t.ProgressStageId, t.DriverId, d.Name AS DriverName,
    t.UserId, u.FirstName + N' ' + u.LastName AS UserName
FROM dbo.Trips AS t
INNER JOIN dbo.Drivers AS d ON d.Id = t.DriverId
INNER JOIN dbo.Users AS u ON u.Id = t.UserId
WHERE t.Id = @TripId;
GO
