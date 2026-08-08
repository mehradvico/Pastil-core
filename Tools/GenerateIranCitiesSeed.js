const fs = require("fs");
const path = require("path");

const citiesPath = process.argv[2];
const provincesPath = process.argv[3];
const outputPath = process.argv[4];

if (!citiesPath || !provincesPath || !outputPath) {
    throw new Error(
        "Usage: node GenerateIranCitiesSeed.js <cities.json> <provinces.json> <output.sql>"
    );
}

const cities = JSON.parse(fs.readFileSync(citiesPath, "utf8"));
const provinces = JSON.parse(fs.readFileSync(provincesPath, "utf8"));

const stateBySourceProvinceId = new Map([
    [100, { id: 22, name: "مرکزی" }],
    [101, { id: 19, name: "گیلان" }],
    [102, { id: 21, name: "مازندران" }],
    [103, { id: 1, name: "آذربایجان شرقی" }],
    [104, { id: 2, name: "آذربایجان غربی" }],
    [105, { id: 17, name: "کرمانشاه" }],
    [106, { id: 10, name: "خوزستان" }],
    [107, { id: 14, name: "فارس" }],
    [108, { id: 15, name: "کرمان" }],
    [109, { id: 30, name: "خراسان رضوی" }],
    [110, { id: 4, name: "اصفهان" }],
    [111, { id: 13, name: "سیستان و بلوچستان" }],
    [112, { id: 16, name: "کردستان" }],
    [113, { id: 24, name: "همدان" }],
    [114, { id: 8, name: "چهارمحال و بختیاری" }],
    [115, { id: 20, name: "لرستان" }],
    [116, { id: 5, name: "ایلام" }],
    [117, { id: 18, name: "کهگیلویه و بویراحمد" }],
    [118, { id: 6, name: "بوشهر" }],
    [119, { id: 11, name: "زنجان" }],
    [120, { id: 12, name: "سمنان" }],
    [121, { id: 25, name: "یزد" }],
    [122, { id: 23, name: "هرمزگان" }],
    [123, { id: 7, name: "تهران" }],
    [124, { id: 3, name: "اردبیل" }],
    [125, { id: 26, name: "قم" }],
    [126, { id: 28, name: "قزوین" }],
    [127, { id: 27, name: "گلستان" }],
    [128, { id: 31, name: "خراسان شمالی" }],
    [129, { id: 29, name: "خراسان جنوبی" }],
    [130, { id: 9, name: "البرز" }]
]);

if (cities.length !== 1659) {
    throw new Error(`Expected 1659 cities, but received ${cities.length}.`);
}

if (provinces.length !== 31) {
    throw new Error(`Expected 31 provinces, but received ${provinces.length}.`);
}

for (const province of provinces) {
    const mappedState = stateBySourceProvinceId.get(province.id);
    if (!mappedState || mappedState.name !== province.name) {
        throw new Error(
            `Province mapping mismatch for source province ${province.id}: ${province.name}`
        );
    }
}

for (const city of cities) {
    if (!stateBySourceProvinceId.has(city.province_id)) {
        throw new Error(
            `No StateId mapping exists for city ${city.name}, province ${city.province_id}.`
        );
    }
}

function sqlString(value) {
    return `N'${String(value).replaceAll("'", "''")}'`;
}

const rows = cities.map((city, index) => {
    const state = stateBySourceProvinceId.get(city.province_id);
    return `        (${index + 1}, ${state.id}, ${sqlString(city.name)})`;
});

function chunk(items, size) {
    const chunks = [];
    for (let index = 0; index < items.length; index += size) {
        chunks.push(items.slice(index, index + size));
    }
    return chunks;
}

const cityInsertStatements = chunk(rows, 1000)
    .map(cityRows => `    INSERT INTO @Cities ([Id], [StateId], [Name])
    VALUES
${cityRows.join(",\n")};`)
    .join("\n\n");

const stateChecks = [...stateBySourceProvinceId.values()]
    .sort((a, b) => a.id - b.id)
    .map(state => `        (${state.id}, ${sqlString(state.name)})`)
    .join(",\n");

const sql = `/*
    Pastil - Iran cities seed

    Source:
      https://github.com/sajaddp/list-of-cities-in-Iran
      Administrative divisions published for year 1402 (2023)
      License: MIT

    Counts:
      31 provinces
      1,659 cities

    Important:
      This public source contains the official city hierarchy, but does not
      contain legal municipal boundary polygons. Boundary is therefore NULL
      for newly inserted rows. Existing non-null Boundary values are preserved
      when this script is run again.

      County/ADM2 polygons must not be stored as city boundaries.

    IDs:
      City IDs are normalized to 1..1659.
      StateIds match Pastil_Iran_States_With_Boundaries.sql.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.States', N'U') IS NULL
    THROW 50001, N'Table dbo.States was not found. Run the State seed first.', 1;

IF OBJECT_ID(N'dbo.Cities', N'U') IS NULL
    THROW 50002, N'Table dbo.Cities was not found. Run mig_start first.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @ExpectedStates TABLE
    (
        [Id] bigint NOT NULL PRIMARY KEY,
        [Name] nvarchar(100) NOT NULL
    );

    INSERT INTO @ExpectedStates ([Id], [Name])
    VALUES
${stateChecks};

    IF EXISTS
    (
        SELECT 1
        FROM @ExpectedStates AS expected
        LEFT JOIN dbo.States AS actual
            ON actual.[Id] = expected.[Id]
           AND actual.[Name] = expected.[Name]
        WHERE actual.[Id] IS NULL
    )
        THROW 50003, N'State IDs/names do not match the Iran State seed.', 1;

    DECLARE @Cities TABLE
    (
        [Id] bigint NOT NULL PRIMARY KEY,
        [StateId] bigint NOT NULL,
        [Name] nvarchar(200) NOT NULL
    );

${cityInsertStatements}

    IF (SELECT COUNT_BIG(*) FROM @Cities) <> 1659
        THROW 50004, N'The seed must contain exactly 1659 cities.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM @Cities AS source
        INNER JOIN dbo.Cities AS target
            ON target.[Id] = source.[Id]
        WHERE target.[StateId] <> source.[StateId]
           OR ISNULL(target.[Name], N'') <> source.[Name]
    )
        THROW 50005, N'A City Id from 1 to 1659 is already used by another record.', 1;

    UPDATE target
       SET target.[Name] = source.[Name],
           target.[StateId] = source.[StateId]
    FROM dbo.Cities AS target
    INNER JOIN @Cities AS source
        ON source.[Id] = target.[Id];

    SET IDENTITY_INSERT dbo.Cities ON;

    INSERT INTO dbo.Cities ([Id], [StateId], [Name], [Boundary])
    SELECT
        source.[Id],
        source.[StateId],
        source.[Name],
        NULL
    FROM @Cities AS source
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Cities AS target
        WHERE target.[Id] = source.[Id]
    );

    SET IDENTITY_INSERT dbo.Cities OFF;

    IF
    (
        SELECT COUNT_BIG(*)
        FROM dbo.Cities
        WHERE [Id] BETWEEN 1 AND 1659
    ) <> 1659
        THROW 50006, N'One or more city rows are missing.', 1;

    COMMIT TRANSACTION;

    SELECT
        s.[Id] AS [StateId],
        s.[Name] AS [StateName],
        COUNT_BIG(c.[Id]) AS [CityCount],
        SUM(CASE WHEN c.[Boundary] IS NOT NULL THEN 1 ELSE 0 END) AS [CityBoundaryCount]
    FROM dbo.States AS s
    INNER JOIN dbo.Cities AS c
        ON c.[StateId] = s.[Id]
    WHERE c.[Id] BETWEEN 1 AND 1659
    GROUP BY s.[Id], s.[Name]
    ORDER BY s.[Id];
END TRY
BEGIN CATCH
    BEGIN TRY
        SET IDENTITY_INSERT dbo.Cities OFF;
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

const duplicateNameGroups = new Map();
for (const city of cities) {
    const key = `${city.province_id}|${city.name}`;
    duplicateNameGroups.set(key, (duplicateNameGroups.get(key) ?? 0) + 1);
}

process.stdout.write(
    JSON.stringify({
        cityCount: cities.length,
        stateCount: new Set(cities.map(city => city.province_id)).size,
        firstId: 1,
        lastId: cities.length,
        duplicateNamesWithinSameState: [...duplicateNameGroups.values()]
            .filter(count => count > 1)
            .length,
        outputPath
    }, null, 2)
);
