const fs = require("fs");
const path = require("path");

const sourcePath = process.argv[2];
const outputPath = process.argv[3];

if (!sourcePath || !outputPath) {
    throw new Error("Usage: node GenerateIranStatesSeed.js <source.geojson> <output.sql>");
}

const featureCollection = JSON.parse(fs.readFileSync(sourcePath, "utf8"));

const stateDefinitions = [
    { id: 1, enName: "East Azerbaijan", faName: "آذربایجان شرقی" },
    { id: 2, enName: "West Azerbaijan", faName: "آذربایجان غربی" },
    { id: 3, enName: "Ardabil", faName: "اردبیل" },
    { id: 4, enName: "Isfahan", faName: "اصفهان" },
    { id: 5, enName: "Ilam", faName: "ایلام" },
    { id: 6, enName: "Bushehr", faName: "بوشهر" },
    { id: 7, enName: "Tehran", faName: "تهران" },
    { id: 8, enName: "Chaharmahal and Bakhtiari", faName: "چهارمحال و بختیاری" },
    { id: 9, enName: "Alborz", faName: "البرز" },
    { id: 10, enName: "Khuzestan", faName: "خوزستان" },
    { id: 11, enName: "Zanjan", faName: "زنجان" },
    { id: 12, enName: "Semnan", faName: "سمنان" },
    { id: 13, enName: "Sistan and Baluchestan", faName: "سیستان و بلوچستان" },
    { id: 14, enName: "Fars", faName: "فارس" },
    { id: 15, enName: "Kerman", faName: "کرمان" },
    { id: 16, enName: "Kurdistan", faName: "کردستان" },
    { id: 17, enName: "Kermanshah", faName: "کرمانشاه" },
    { id: 18, enName: "Kohgiluyeh and Boyer-Ahmad", faName: "کهگیلویه و بویراحمد" },
    { id: 19, enName: "Gilan", faName: "گیلان" },
    { id: 20, enName: "Lorestan", faName: "لرستان" },
    { id: 21, enName: "Mazandaran", faName: "مازندران" },
    { id: 22, enName: "Markazi", faName: "مرکزی" },
    { id: 23, enName: "Hormozgan", faName: "هرمزگان" },
    { id: 24, enName: "Hamadan", faName: "همدان" },
    { id: 25, enName: "Yazd", faName: "یزد" },
    { id: 26, enName: "Qom", faName: "قم" },
    { id: 27, enName: "Golestan", faName: "گلستان" },
    { id: 28, enName: "Qazvin", faName: "قزوین" },
    { id: 29, enName: "South Khorasan", faName: "خراسان جنوبی" },
    { id: 30, enName: "Razavi Khorasan", faName: "خراسان رضوی" },
    { id: 31, enName: "North Khorasan", faName: "خراسان شمالی" }
];

const sourceNameAliases = new Map([
    ["Ardabil", "Ardabil"],
    ["Chaharmahal and Bakhtiari", "Chaharmahal and Bakhtiari"],
    ["East Azerbaijan", "East Azerbaijan"],
    ["Isfahan", "Isfahan"],
    ["Kohgiluyeh and Boyer-Ahmad", "Kohgiluyeh and Boyer-Ahmad"],
    ["Kurdistan", "Kurdistan"]
]);

function canonicalName(sourceName) {
    return sourceNameAliases.get(sourceName) ?? sourceName;
}

function signedArea(ring) {
    let area = 0;
    for (let i = 0; i < ring.length - 1; i++) {
        area += (ring[i][0] * ring[i + 1][1]) - (ring[i + 1][0] * ring[i][1]);
    }
    return area / 2;
}

function closeRing(ring) {
    if (ring.length === 0) {
        throw new Error("An empty linear ring was found.");
    }

    const first = ring[0];
    const last = ring[ring.length - 1];
    if (first[0] === last[0] && first[1] === last[1]) {
        return ring;
    }

    return [...ring, first];
}

function orientRing(ring, shouldBeCounterClockwise) {
    const closed = closeRing(ring);
    const isCounterClockwise = signedArea(closed) > 0;
    return isCounterClockwise === shouldBeCounterClockwise
        ? closed
        : [...closed].reverse();
}

function normalizePolygon(polygon) {
    return polygon.map((ring, index) => orientRing(ring, index === 0));
}

function geometryPolygons(geometry) {
    if (geometry.type === "Polygon") {
        return [normalizePolygon(geometry.coordinates)];
    }

    if (geometry.type === "MultiPolygon") {
        return geometry.coordinates.map(normalizePolygon);
    }

    throw new Error(`Unsupported geometry type: ${geometry.type}`);
}

function numberText(value) {
    if (!Number.isFinite(value)) {
        throw new Error(`Invalid coordinate: ${value}`);
    }
    return String(value);
}

function polygonWkt(polygon) {
    const rings = polygon
        .map(ring => `(${ring.map(point => `${numberText(point[0])} ${numberText(point[1])}`).join(",")})`)
        .join(",");
    return `(${rings})`;
}

function multiPolygonWkt(polygons) {
    return `MULTIPOLYGON (${polygons.map(polygonWkt).join(",")})`;
}

const polygonsByName = new Map();

for (const feature of featureCollection.features) {
    const name = canonicalName(feature.properties.shapeName);
    const polygons = polygonsByName.get(name) ?? [];
    polygons.push(...geometryPolygons(feature.geometry));
    polygonsByName.set(name, polygons);
}

const missing = stateDefinitions.filter(state => !polygonsByName.has(state.enName));
const unexpected = [...polygonsByName.keys()].filter(
    name => !stateDefinitions.some(state => state.enName === name)
);

if (missing.length || unexpected.length) {
    throw new Error(
        `Boundary/name mismatch. Missing: ${missing.map(x => x.enName).join(", ")}; ` +
        `Unexpected: ${unexpected.join(", ")}`
    );
}

const values = stateDefinitions.map(state => {
    const wkt = multiPolygonWkt(polygonsByName.get(state.enName));
    return [
        `        (${state.id},`,
        `         N'${state.faName}',`,
        `         N'${state.enName}',`,
        `         CAST(N'${wkt}' AS nvarchar(max)))`
    ].join("\n");
}).join(",\n\n");

const sql = `/*
    Pastil - Iran country and province seed with full ADM1 boundaries

    Source:
      geoBoundaries gbOpen, IRN ADM1
      Boundary source: OpenStreetMap / Wambacher
      Source update: 2023-01-19
      Dataset build: 2023-12-12
      License: Open Data Commons Open Database License 1.0 (ODbL)
      https://www.geoboundaries.org/api/current/gbOpen/IRN/ADM1/

    Notes:
      - All boundaries use WGS 84 / SRID 4326.
      - The two source features named Mazandaran are merged into one MultiPolygon.
      - IDs 1..31 follow the numeric order of the ISO 3166-2 province codes.
      - Re-running this script updates existing rows by EnName and does not duplicate them.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.Countries', N'U') IS NULL
    THROW 50001, N'Table dbo.Countries was not found. Run mig_start first.', 1;

IF OBJECT_ID(N'dbo.States', N'U') IS NULL
    THROW 50002, N'Table dbo.States was not found. Run mig_start first.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @CountryId bigint;

    SELECT TOP (1) @CountryId = [Id]
    FROM dbo.Countries
    WHERE [EnName] = N'Iran' OR [Name] = N'ایران'
    ORDER BY CASE WHEN [EnName] = N'Iran' THEN 0 ELSE 1 END, [Id];

    IF @CountryId IS NULL
    BEGIN
        INSERT INTO dbo.Countries ([Name], [EnName])
        VALUES (N'ایران', N'Iran');

        SET @CountryId = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.Countries
        SET [Name] = N'ایران',
            [EnName] = N'Iran'
        WHERE [Id] = @CountryId;
    END;

    DECLARE @States TABLE
    (
        [Id] bigint NOT NULL PRIMARY KEY,
        [Name] nvarchar(100) NOT NULL,
        [EnName] nvarchar(100) NOT NULL UNIQUE,
        [Wkt] nvarchar(max) NOT NULL
    );

    INSERT INTO @States ([Id], [Name], [EnName], [Wkt])
    VALUES
${values};

    IF (SELECT COUNT_BIG(*) FROM @States) <> 31
        THROW 50003, N'The seed must contain exactly 31 provinces.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM @States AS source
        INNER JOIN dbo.States AS target
            ON target.[Id] = source.[Id]
        WHERE target.[CountryId] <> @CountryId
           OR ISNULL(target.[EnName], N'') <> source.[EnName]
    )
        THROW 50004, N'A State Id from 1 to 31 is already used by another record.', 1;

    UPDATE target
       SET target.[Name] = source.[Name],
           target.[Boundary] = geography::STGeomFromText(source.[Wkt], 4326).MakeValid()
    FROM dbo.States AS target
    INNER JOIN @States AS source
        ON source.[EnName] = target.[EnName]
    WHERE target.[CountryId] = @CountryId;

    SET IDENTITY_INSERT dbo.States ON;

    INSERT INTO dbo.States ([Id], [Name], [EnName], [CountryId], [Boundary])
    SELECT
        source.[Id],
        source.[Name],
        source.[EnName],
        @CountryId,
        geography::STGeomFromText(source.[Wkt], 4326).MakeValid()
    FROM @States AS source
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.States AS target
        WHERE target.[CountryId] = @CountryId
          AND target.[EnName] = source.[EnName]
    );

    SET IDENTITY_INSERT dbo.States OFF;

    IF
    (
        SELECT COUNT_BIG(*)
        FROM dbo.States
        WHERE [CountryId] = @CountryId
          AND [EnName] IN (SELECT [EnName] FROM @States)
          AND [Boundary] IS NOT NULL
          AND [Boundary].STSrid = 4326
          AND [Boundary].STIsValid() = 1
    ) <> 31
        THROW 50005, N'One or more province boundaries are missing or invalid.', 1;

    COMMIT TRANSACTION;

    SELECT
        s.[Id],
        s.[Name],
        s.[EnName],
        s.[Boundary].STGeometryType() AS [GeometryType],
        s.[Boundary].STSrid AS [SRID],
        s.[Boundary].STIsValid() AS [IsValid],
        s.[Boundary].STNumPoints() AS [PointCount]
    FROM dbo.States AS s
    WHERE s.[CountryId] = @CountryId
      AND s.[EnName] IN (SELECT [EnName] FROM @States)
    ORDER BY s.[Id];
END TRY
BEGIN CATCH
    BEGIN TRY
        SET IDENTITY_INSERT dbo.States OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
`;

fs.mkdirSync(path.dirname(outputPath), { recursive: true });
fs.writeFileSync(outputPath, sql, "utf8");

const pointCount = [...polygonsByName.values()]
    .flat(2)
    .reduce((total, ring) => total + ring.length, 0);

process.stdout.write(
    JSON.stringify({
        stateCount: stateDefinitions.length,
        sourceFeatureCount: featureCollection.features.length,
        mergedMazandaranFeatureCount: featureCollection.features.filter(
            feature => feature.properties.shapeName === "Mazandaran"
        ).length,
        pointCount,
        outputPath
    }, null, 2)
);
