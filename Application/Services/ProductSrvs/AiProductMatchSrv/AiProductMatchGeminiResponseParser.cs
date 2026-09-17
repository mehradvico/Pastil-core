using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Application.Services.ProductSrvs.AiProductMatchSrv
{
    public class AiProductMatchShelfExtractedRow
    {
        public string DetectedName { get; set; }
        public string Brand { get; set; }
        public string AnimalType { get; set; }
        public double? PackageSizeValue { get; set; }
        public string PackageSizeUnit { get; set; }
        public double? PriceGuess { get; set; }
        public int? QuantityGuess { get; set; }
        public string Unit { get; set; }
    }

    public class AiProductMatchRankedCandidate
    {
        public int Index { get; set; }
        public int? PackageIndex { get; set; }
        public double Confidence { get; set; }
    }

    public class AiProductMatchRankedRowResult
    {
        public string RowId { get; set; }
        public List<AiProductMatchRankedCandidate> Ranked { get; set; } = new();
    }

    // مسئول پارس دو شکل کاملاً متفاوت از پاسخ Gemini؛ عمداً از ParseModelOutput چت پاستیل‌AI
    // جدا نگه داشته شده چون آن‌جا فقط شکل {answer, scope, isEmergency} را می‌فهمد.
    public static class AiProductMatchGeminiResponseParser
    {
        public static List<AiProductMatchShelfExtractedRow> ParseShelfExtraction(string rawJson)
        {
            var result = new List<AiProductMatchShelfExtractedRow>();
            var node = TryParse(rawJson);
            var items = node?["items"] as JsonArray;
            if (items == null)
                return result;

            foreach (var item in items)
            {
                var name = item?["detectedName"]?.GetValue<string>();
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                result.Add(new AiProductMatchShelfExtractedRow
                {
                    DetectedName = name.Trim(),
                    Brand = item?["brand"]?.GetValue<string>(),
                    AnimalType = item?["animalType"]?.GetValue<string>(),
                    PackageSizeValue = TryGetDouble(item?["packageSizeValue"]),
                    PackageSizeUnit = item?["packageSizeUnit"]?.GetValue<string>(),
                    PriceGuess = TryGetDouble(item?["priceGuess"]),
                    QuantityGuess = TryGetInt(item?["quantityGuess"]),
                    Unit = item?["unit"]?.GetValue<string>()
                });
            }

            return result;
        }

        public static List<AiProductMatchRankedRowResult> ParseMatchResults(string rawJson)
        {
            var result = new List<AiProductMatchRankedRowResult>();
            var node = TryParse(rawJson);
            var results = node?["results"] as JsonArray;
            if (results == null)
                return result;

            foreach (var row in results)
            {
                var rowId = row?["rowId"]?.GetValue<string>();
                if (string.IsNullOrWhiteSpace(rowId))
                    continue;

                var rowResult = new AiProductMatchRankedRowResult { RowId = rowId };
                if (row?["ranked"] is JsonArray ranked)
                {
                    foreach (var entry in ranked)
                    {
                        var index = TryGetInt(entry?["index"]);
                        if (index == null)
                            continue;

                        rowResult.Ranked.Add(new AiProductMatchRankedCandidate
                        {
                            Index = index.Value,
                            PackageIndex = TryGetInt(entry?["packageIndex"]),
                            Confidence = TryGetDouble(entry?["confidence"]) ?? 0
                        });
                    }
                }

                result.Add(rowResult);
            }

            return result;
        }

        private static JsonNode TryParse(string rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson))
                return null;

            try
            {
                var normalized = rawJson.Trim();
                if (normalized.StartsWith("```", StringComparison.Ordinal))
                {
                    normalized = normalized
                        .Replace("```json", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("```", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Trim();
                }

                return JsonNode.Parse(normalized);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static double? TryGetDouble(JsonNode node)
        {
            try { return node?.GetValue<double?>(); }
            catch (Exception) { return null; }
        }

        private static int? TryGetInt(JsonNode node)
        {
            try { return node?.GetValue<int?>(); }
            catch (Exception) { return null; }
        }
    }
}
