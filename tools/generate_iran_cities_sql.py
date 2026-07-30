#!/usr/bin/env python3
from __future__ import annotations

import json
import math
import re
import unicodedata
import urllib.request
import zipfile
from pathlib import Path
from typing import Any

SOURCE_URL = (
    "https://raw.githubusercontent.com/hemedani/yademan/"
    "b05b9ada40f94b72ee5104453392c31bdca0a13d/"
    "back/assets/geojson/irn_admin2_simplified_converted.json"
)
OUTPUT_SQL = Path("artifacts/Insert_Iran_Cities_Offline.sql")
OUTPUT_ZIP = Path("artifacts/Insert_Iran_Cities_Offline.zip")
EXPECTED_FEATURES = 432

STATE_IDS = {
    "آذربایجان شرقی": 1,
    "آذربایجان غربی": 2,
    "اردبیل": 3,
    "اصفهان": 4,
    "ایلام": 5,
    "بوشهر": 6,
    "تهران": 7,
    "چهارمحال و بختیاری": 8,
    "البرز": 9,
    "خوزستان": 10,
    "زنجان": 11,
    "سمنان": 12,
    "سیستان و بلوچستان": 13,
    "فارس": 14,
    "کرمان": 15,
    "کردستان": 16,
    "کرمانشاه": 17,
    "کهگیلویه و بویراحمد": 18,
    "گیلان": 19,
    "لرستان": 20,
    "مازندران": 21,
    "مرکزی": 22,
    "هرمزگان": 23,
    "همدان": 24,
    "یزد": 25,
    "قم": 26,
    "گلستان": 27,
    "قزوین": 28,
    "خراسان جنوبی": 29,
    "خراسان رضوی": 30,
    "خراسان شمالی": 31,
}


def normalize_fa(value: Any) -> str:
    text = unicodedata.normalize("NFKC", str(value or ""))
    text = (
        text.replace("ي", "ی")
        .replace("ى", "ی")
        .replace("ك", "ک")
        .replace("ۀ", "ه")
        .replace("ة", "ه")
        .replace("\u200c", " ")
    )
    return re.sub(r"\s+", " ", text).strip()


def normalize_province(value: Any) -> str:
    name = normalize_fa(value)
    if name.startswith("استان "):
        name = name[6:].strip()
    aliases = {
        "چهار محال و بختیاری": "چهارمحال و بختیاری",
        "کهگیلویه وبویراحمد": "کهگیلویه و بویراحمد",
        "کهگیلویه و بویر احمد": "کهگیلویه و بویراحمد",
    }
    return aliases.get(name, name)


def normalize_city(value: Any) -> str:
    name = normalize_fa(value)
    if name.startswith("شهرستان "):
        name = name[len("شهرستان ") :].strip()
    return name


def escape_sql(value: str) -> str:
    return value.replace("'", "''")


def format_number(value: Any) -> str:
    number = float(value)
    if not math.isfinite(number):
        raise ValueError(f"Invalid coordinate: {value!r}")
    rounded = round(number, 7)
    if rounded == 0:
        return "0"
    text = f"{rounded:.7f}".rstrip("0").rstrip(".")
    return text


def same_point(a: list[Any], b: list[Any]) -> bool:
    return len(a) >= 2 and len(b) >= 2 and float(a[0]) == float(b[0]) and float(a[1]) == float(b[1])


def close_ring(ring: list[list[Any]]) -> list[list[Any]]:
    if len(ring) < 3:
        raise ValueError("Polygon ring has fewer than three vertices")
    result = [list(point[:2]) for point in ring]
    if not same_point(result[0], result[-1]):
        result.append(list(result[0]))
    if len(result) < 4:
        raise ValueError("Closed polygon ring has fewer than four vertices")
    return result


def signed_area(ring: list[list[Any]]) -> float:
    area = 0.0
    for current, following in zip(ring, ring[1:]):
        x1, y1 = float(current[0]), float(current[1])
        x2, y2 = float(following[0]), float(following[1])
        area += x1 * y2 - x2 * y1
    return area / 2.0


def orient_ring(ring: list[list[Any]], *, counter_clockwise: bool) -> list[list[Any]]:
    result = close_ring(ring)
    is_ccw = signed_area(result) > 0
    if is_ccw != counter_clockwise:
        result = list(reversed(result))
    return result


def ring_wkt(ring: list[list[Any]], *, counter_clockwise: bool) -> str:
    oriented = orient_ring(ring, counter_clockwise=counter_clockwise)
    points = ",".join(f"{format_number(point[0])} {format_number(point[1])}" for point in oriented)
    return f"({points})"


def polygon_wkt(coordinates: list[Any]) -> str:
    if not coordinates:
        raise ValueError("Polygon has no rings")
    rings = [ring_wkt(coordinates[0], counter_clockwise=True)]
    rings.extend(ring_wkt(ring, counter_clockwise=False) for ring in coordinates[1:])
    return f"({','.join(rings)})"


def geometry_wkt(geometry: dict[str, Any]) -> str:
    geometry_type = geometry.get("type")
    coordinates = geometry.get("coordinates")
    if geometry_type == "Polygon":
        return f"POLYGON{polygon_wkt(coordinates)}"
    if geometry_type == "MultiPolygon":
        if not coordinates:
            raise ValueError("MultiPolygon has no polygons")
        return "MULTIPOLYGON(" + ",".join(polygon_wkt(polygon) for polygon in coordinates) + ")"
    raise ValueError(f"Unsupported geometry type: {geometry_type!r}")


def sql_wkt_assignment(wkt: str, chunk_size: int = 3500) -> str:
    chunks = [escape_sql(wkt[index : index + chunk_size]) for index in range(0, len(wkt), chunk_size)]
    lines = [f"        SET @Wkt = CONVERT(nvarchar(max), N'{chunks[0]}');"]
    lines.extend(f"        SET @Wkt += N'{chunk}';" for chunk in chunks[1:])
    return "\n".join(lines)


def download_geojson() -> dict[str, Any]:
    request = urllib.request.Request(
        SOURCE_URL,
        headers={"User-Agent": "Pastil-Iran-Cities-SQL-Builder/1.0", "Accept": "application/json"},
    )
    with urllib.request.urlopen(request, timeout=120) as response:
        payload = response.read()
    data = json.loads(payload.decode("utf-8-sig"))
    if data.get("type") != "FeatureCollection" or not isinstance(data.get("features"), list):
        raise ValueError("Source is not a GeoJSON FeatureCollection")
    return data


def build_rows(data: dict[str, Any]) -> list[dict[str, Any]]:
    features = data["features"]
    if len(features) != EXPECTED_FEATURES:
        raise ValueError(f"Expected {EXPECTED_FEATURES} features, got {len(features)}")

    normalized_state_ids = {normalize_province(name): state_id for name, state_id in STATE_IDS.items()}
    rows: list[dict[str, Any]] = []
    keys: set[tuple[int, str]] = set()

    for index, feature in enumerate(features, start=1):
        properties = feature.get("properties") or {}
        province = normalize_province(properties.get("adm1_name1") or properties.get("adm1_name"))
        state_id = normalized_state_ids.get(province)
        if state_id is None:
            raise ValueError(f"Feature {index}: unknown province {province!r}")

        city_name = normalize_city(properties.get("adm2_name1") or properties.get("adm2_name"))
        if not city_name:
            raise ValueError(f"Feature {index}: empty city name")

        key = (state_id, city_name)
        if key in keys:
            raise ValueError(f"Feature {index}: duplicate city key {key!r}")
        keys.add(key)

        rows.append(
            {
                "state_id": state_id,
                "city_name": city_name,
                "source_code": normalize_fa(properties.get("adm2_pcode")),
                "wkt": geometry_wkt(feature.get("geometry") or {}),
            }
        )

    rows.sort(key=lambda row: (row["state_id"], row["city_name"]))
    return rows


def build_sql(rows: list[dict[str, Any]]) -> str:
    blocks: list[str] = []
    for row in rows:
        city_name = escape_sql(row["city_name"])
        source_code = escape_sql(row["source_code"])
        blocks.append(
            f"""        -- {source_code} | {city_name}
{sql_wkt_assignment(row['wkt'])}
        SET @Boundary = geography::STGeomFromText(@Wkt, 4326).MakeValid();
        IF @Boundary.EnvelopeAngle() > 90
            SET @Boundary = @Boundary.ReorientObject();

        INSERT INTO [dbo].[Cities] ([StateId], [Boundary], [Name])
        VALUES ({row['state_id']}, @Boundary, N'{city_name}');
"""
        )

    body = "\n".join(blocks)
    return f"""/*
    Pastil - Iran Counties Imported as Cities

    Target table:
      [pastil_db].[dbo].[Cities]
      Id       bigint IDENTITY
      StateId  bigint NOT NULL
      Boundary geography NULL
      Name     nvarchar(max) NULL

    Embedded records: {len(rows)}
    Boundary SRID: 4326
    No HTTP, OLE Automation, xp_cmdshell, Node.js, or external file is required.

    WARNING: This script deletes every existing row from dbo.Cities first.
*/

USE [pastil_db];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

        IF (SELECT COUNT(*) FROM [dbo].[States]) <> 31
            THROW 50001, N'تعداد استان‌های جدول States برابر ۳۱ نیست.', 1;

        DELETE FROM [dbo].[Cities];
        DBCC CHECKIDENT (N'[dbo].[Cities]', RESEED, 0) WITH NO_INFOMSGS;

        DECLARE @Wkt nvarchar(max);
        DECLARE @Boundary geography;

{body}
        IF (SELECT COUNT(*) FROM [dbo].[Cities]) <> {len(rows)}
            THROW 50002, N'تعداد شهرهای درج‌شده با داده مبنا برابر نیست.', 1;

        IF EXISTS
        (
            SELECT 1
            FROM [dbo].[Cities]
            WHERE [Boundary] IS NULL
               OR [Boundary].STSrid <> 4326
               OR [Boundary].STIsValid() <> 1
        )
            THROW 50003, N'اعتبارسنجی نهایی محدوده شهرها ناموفق بود.', 1;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

SELECT
    s.[Id] AS [StateId],
    s.[Name] AS [StateName],
    COUNT(c.[Id]) AS [CityCount],
    SUM(CASE WHEN c.[Boundary] IS NOT NULL THEN 1 ELSE 0 END) AS [CityBoundaryCount]
FROM [dbo].[States] AS s
LEFT JOIN [dbo].[Cities] AS c
    ON c.[StateId] = s.[Id]
GROUP BY s.[Id], s.[Name]
ORDER BY s.[Id];
GO
"""


def main() -> None:
    data = download_geojson()
    rows = build_rows(data)
    sql = build_sql(rows)

    OUTPUT_SQL.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT_SQL.write_text(sql, encoding="utf-8-sig", newline="\n")

    with zipfile.ZipFile(OUTPUT_ZIP, "w", compression=zipfile.ZIP_DEFLATED, compresslevel=9) as archive:
        archive.write(OUTPUT_SQL, arcname=OUTPUT_SQL.name)

    print(f"Generated {OUTPUT_SQL} ({OUTPUT_SQL.stat().st_size:,} bytes)")
    print(f"Generated {OUTPUT_ZIP} ({OUTPUT_ZIP.stat().st_size:,} bytes)")
    print(f"Records: {len(rows)}")


if __name__ == "__main__":
    main()
