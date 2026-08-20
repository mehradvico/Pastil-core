/*
  Seed 24 selected Tehran parks (idempotent).
  Run SeedTehranNeighborhoods.sql first. Neighborhood IDs are resolved from
  polygon boundaries and do not depend on identity values.
  Coordinates: OpenStreetMap / Nominatim, checked 2026-08-18 (ODbL).
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @TehranCityId bigint;

    SELECT TOP (1) @TehranCityId = city.[Id]
    FROM dbo.[Cities] AS city
    INNER JOIN dbo.[States] AS state ON state.[Id] = city.[StateId]
    WHERE REPLACE(REPLACE(LTRIM(RTRIM(city.[Name])), N'ي', N'ی'), N'ك', N'ک') = N'تهران'
    ORDER BY
        CASE WHEN REPLACE(REPLACE(LTRIM(RTRIM(state.[Name])), N'ي', N'ی'), N'ك', N'ک') = N'تهران' THEN 0 ELSE 1 END,
        city.[Id];

    IF @TehranCityId IS NULL
        THROW 51000, N'شهر تهران در جدول Cities پیدا نشد.', 1;

    CREATE TABLE #TehranParkSeed
    (
        [SeedOrder] int NOT NULL PRIMARY KEY,
        [Name] nvarchar(250) NOT NULL,
        [AddressValue] nvarchar(1000) NOT NULL,
        [Location] geography NOT NULL,
        [Suggested] bit NOT NULL
    );

    -- geography::Point(latitude, longitude, SRID)
    INSERT INTO #TehranParkSeed
        ([SeedOrder], [Name], [AddressValue], [Location], [Suggested])
    VALUES
        (1,  N'بوستان ملت',              N'خیابان ولیعصر، بالاتر از بزرگراه نیایش',                         geography::Point(35.7780523, 51.4099930, 4326), 1),
        (2,  N'بوستان آب و آتش',         N'بزرگراه حقانی، بعد از چهارراه جهان کودک',                        geography::Point(35.7547483, 51.4186950, 4326), 1),
        (3,  N'بوستان جنگلی طالقانی',    N'بزرگراه حقانی، روبه‌روی باغ کتاب تهران',                         geography::Point(35.7541386, 51.4225795, 4326), 1),
        (4,  N'بوستان جمشیدیه',          N'نیاوران، خیابان باهنر، خیابان امیدوار',                          geography::Point(35.8261207, 51.4650281, 4326), 1),
        (5,  N'بوستان قیطریه',           N'قیطریه، خیابان قیطریه، نرسیده به میدان پیروز',                   geography::Point(35.7932463, 51.4492426, 4326), 1),
        (6,  N'بوستان ساعی',             N'خیابان ولیعصر، پایین‌تر از میدان ونک',                           geography::Point(35.7357290, 51.4128977, 4326), 1),
        (7,  N'بوستان لاله',             N'بلوار کشاورز، تقاطع خیابان کارگر شمالی',                         geography::Point(35.7114412, 51.3928998, 4326), 1),
        (8,  N'پارک شهر',                N'خیابان وحدت اسلامی، محدوده میدان حسن‌آباد',                       geography::Point(35.6831964, 51.4137496, 4326), 1),
        (9,  N'پارک جنگلی چیتگر',        N'آزادراه تهران–کرج، بعد از خروجی شهرک راه‌آهن',                    geography::Point(35.7267464, 51.1848754, 4326), 1),
        (10, N'بوستان نهج البلاغه',       N'بزرگراه همت، بعد از بزرگراه یادگار امام',                         geography::Point(35.7524191, 51.3418412, 4326), 1),
        (11, N'بوستان پردیسان',          N'بزرگراه حکیم، بین یادگار امام و شیخ فضل‌الله نوری',               geography::Point(35.7449927, 51.3534854, 4326), 1),
        (12, N'بوستان گفتگو',            N'بزرگراه چمران، ورودی کوی نصر',                                    geography::Point(35.7319611, 51.3830310, 4326), 1),
        (13, N'بوستان نیاوران',          N'میدان شهید باهنر، خیابان پاسداران',                               geography::Point(35.8088694, 51.4704689, 4326), 1),
        (14, N'بوستان هنرمندان',         N'خیابان ایرانشهر، خیابان برفروشان',                                geography::Point(35.7097132, 51.4210362, 4326), 1),
        (15, N'پارک پلیس',               N'تهرانپارس، فلکه چهارم، خیابان توحید',                             geography::Point(35.7533985, 51.5380767, 4326), 1),
        (16, N'پارک جنگلی شیان لویزان',  N'شیان، بزرگراه امام علی، خروجی شیان',                              geography::Point(35.7630000, 51.5137000, 4326), 1),
        (17, N'بوستان بعثت',             N'بزرگراه بعثت، خیابان شهید رجایی',                                 geography::Point(35.6460322, 51.4282509, 4326), 1),
        (18, N'بوستان رازی',             N'میدان قزوین، خیابان قزوین',                                       geography::Point(35.6711751, 51.3908481, 4326), 1),
        (19, N'بوستان شفق',              N'یوسف‌آباد، خیابان جمال‌الدین اسدآبادی',                           geography::Point(35.7297492, 51.4084564, 4326), 1),
        (20, N'بوستان جوانمردان ایران',  N'بلوار جوانمردان، حدفاصل بزرگراه همت و حکیم',                      geography::Point(35.7477647, 51.2687172, 4326), 1),
        (21, N'بوستان ولایت',            N'انتهای بزرگراه نواب، تقاطع بزرگراه چراغی',                        geography::Point(35.6452029, 51.3711497, 4326), 1),
        (22, N'بوستان دانشجو',           N'چهارراه ولیعصر، مجاورت مجموعه تئاتر شهر',                         geography::Point(35.7004305, 51.4063460, 4326), 1),
        (23, N'باغ ایرانی',              N'ده ونک، خیابان صابری',                                             geography::Point(35.7719913, 51.3903530, 4326), 1),
        (24, N'بوستان جنگلی یاس فاطمی', N'بزرگراه شهید بابایی، بعد از تقاطع خیابان استخر',                  geography::Point(35.7680516, 51.5411936, 4326), 1);

    CREATE TABLE #ResolvedTehranPark
    (
        [SeedOrder] int NOT NULL PRIMARY KEY,
        [Name] nvarchar(250) NOT NULL,
        [AddressValue] nvarchar(1000) NOT NULL,
        [Location] geography NOT NULL,
        [Suggested] bit NOT NULL,
        [NeighborhoodId] bigint NULL
    );

    INSERT INTO #ResolvedTehranPark
        ([SeedOrder], [Name], [AddressValue], [Location], [Suggested], [NeighborhoodId])
    SELECT
        seed.[SeedOrder], seed.[Name], seed.[AddressValue], seed.[Location], seed.[Suggested],
        matchedNeighborhood.[Id]
    FROM #TehranParkSeed AS seed
    OUTER APPLY
    (
        SELECT TOP (1) neighborhood.[Id]
        FROM dbo.[Neighborhoods] AS neighborhood
        WHERE neighborhood.[CityId] = @TehranCityId
          AND neighborhood.[Boundary] IS NOT NULL
          AND neighborhood.[Boundary].STIntersects(seed.[Location]) = 1
        ORDER BY neighborhood.[Boundary].STArea(), neighborhood.[Id]
    ) AS matchedNeighborhood;

    IF EXISTS (SELECT 1 FROM #ResolvedTehranPark WHERE [NeighborhoodId] IS NULL)
    BEGIN
        DECLARE @UnmatchedParks nvarchar(max);
        DECLARE @UnmatchedParksError nvarchar(2048);

        SELECT @UnmatchedParks = STRING_AGG([Name], N'، ')
        FROM #ResolvedTehranPark
        WHERE [NeighborhoodId] IS NULL;

        SET @UnmatchedParksError = N'محله این پارک‌ها از روی مختصات پیدا نشد: ' + @UnmatchedParks;
        THROW 51001, @UnmatchedParksError, 1;
    END;

    UPDATE park
    SET
        park.[NeighborhoodId] = seed.[NeighborhoodId],
        park.[Suggested] = seed.[Suggested],
        park.[AddressValue] = seed.[AddressValue],
        park.[Location] = seed.[Location]
    FROM dbo.[Parks] AS park
    INNER JOIN dbo.[Neighborhoods] AS neighborhood ON neighborhood.[Id] = park.[NeighborhoodId]
    INNER JOIN #ResolvedTehranPark AS seed
        ON REPLACE(REPLACE(LTRIM(RTRIM(seed.[Name])), N'ي', N'ی'), N'ك', N'ک') =
           REPLACE(REPLACE(LTRIM(RTRIM(park.[Name])), N'ي', N'ی'), N'ك', N'ک')
    WHERE neighborhood.[CityId] = @TehranCityId;

    INSERT INTO dbo.[Parks]
        ([Name], [NeighborhoodId], [Suggested], [PictureId], [AddressValue], [Location])
    SELECT seed.[Name], seed.[NeighborhoodId], seed.[Suggested], NULL, seed.[AddressValue], seed.[Location]
    FROM #ResolvedTehranPark AS seed
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.[Parks] AS park
        INNER JOIN dbo.[Neighborhoods] AS neighborhood ON neighborhood.[Id] = park.[NeighborhoodId]
        WHERE neighborhood.[CityId] = @TehranCityId
          AND REPLACE(REPLACE(LTRIM(RTRIM(park.[Name])), N'ي', N'ی'), N'ك', N'ک') =
              REPLACE(REPLACE(LTRIM(RTRIM(seed.[Name])), N'ي', N'ی'), N'ك', N'ک')
    );

    COMMIT TRANSACTION;

    SELECT
        park.[Id], park.[Name], neighborhood.[Name] AS [NeighborhoodName],
        park.[AddressValue], park.[Suggested],
        park.[Location].Lat AS [Latitude], park.[Location].Long AS [Longitude]
    FROM dbo.[Parks] AS park
    INNER JOIN dbo.[Neighborhoods] AS neighborhood ON neighborhood.[Id] = park.[NeighborhoodId]
    INNER JOIN #TehranParkSeed AS seed
        ON REPLACE(REPLACE(LTRIM(RTRIM(seed.[Name])), N'ي', N'ی'), N'ك', N'ک') =
           REPLACE(REPLACE(LTRIM(RTRIM(park.[Name])), N'ي', N'ی'), N'ك', N'ک')
    WHERE neighborhood.[CityId] = @TehranCityId
    ORDER BY seed.[SeedOrder];
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
