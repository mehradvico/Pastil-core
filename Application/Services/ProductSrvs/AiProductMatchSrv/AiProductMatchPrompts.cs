using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Application.Services.ProductSrvs.AiProductMatchSrv
{
    // متن‌های Prompt عمداً به انگلیسی نوشته شده‌اند (دقت بالاتر و توکن کمتر برای پیروی دقیق از
    // Schema ساختاریافته با مدل‌های Gemini)؛ این متن هرگز مستقیم به کاربر نمایش داده نمی‌شود.
    public static class AiProductMatchPrompts
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

        public const string ShelfExtractionSystemInstruction =
            "You are a product-detection assistant for an Iranian pet-supplies marketplace. " +
            "You will be given ONE photo of a store shelf or invoice. " +
            "List every distinct sellable product you can visually identify. " +
            "Do not invent products you cannot see. If text on packaging is Persian, keep the detected name in Persian. " +
            "Respond ONLY with strict JSON matching this schema, no prose, no markdown fences: " +
            "{\"items\":[{\"detectedName\":string,\"priceGuess\":number|null,\"quantityGuess\":number|null,\"unit\":string|null}]}. " +
            "priceGuess/quantityGuess should be null unless a price tag or count is clearly visible.";

        public const string ShelfExtractionUserText =
            "Identify every distinct product visible in this photo and return the JSON described in your instructions.";

        public const string MatchSystemInstruction =
            "You are a strict product-matching engine for an Iranian pet-supplies catalog. " +
            "For each input row you will receive a detected/raw product name and a short numbered list of real catalog " +
            "candidates (index, name, brand, code). " +
            "You must choose ONLY from the given candidate indexes for that row — never invent a product, never return an " +
            "index that was not listed for that row. If none of the candidates plausibly match, omit that row or return an " +
            "empty ranked list for it. Rank up to 3 plausible candidates per row, best first, with a confidence between 0 and 1 " +
            "(1 = certain exact match, 0.5 = plausible guess, below 0.3 = weak/unlikely). " +
            "Respond ONLY with strict JSON matching this schema, no prose, no markdown fences: " +
            "{\"results\":[{\"rowId\":string,\"ranked\":[{\"index\":number,\"confidence\":number}]}]}.";

        public static string BuildMatchUserText(
            string currency,
            IReadOnlyList<AiProductMatchWorkingRow> rows,
            IReadOnlyDictionary<string, List<AiProductMatchCandidateProduct>> candidatesByRow)
        {
            var payload = new
            {
                currency,
                rows = BuildRowsPayload(rows, candidatesByRow)
            };

            return JsonSerializer.Serialize(payload, JsonOptions);
        }

        private static List<object> BuildRowsPayload(
            IReadOnlyList<AiProductMatchWorkingRow> rows,
            IReadOnlyDictionary<string, List<AiProductMatchCandidateProduct>> candidatesByRow)
        {
            var result = new List<object>();
            foreach (var row in rows)
            {
                if (!candidatesByRow.TryGetValue(row.RowId, out var candidates) || candidates.Count == 0)
                    continue;

                var candidatesPayload = new List<object>();
                for (var index = 0; index < candidates.Count; index++)
                {
                    candidatesPayload.Add(new
                    {
                        index,
                        name = candidates[index].Name,
                        brand = candidates[index].BrandName,
                        code = candidates[index].CodeValue
                    });
                }

                result.Add(new
                {
                    rowId = row.RowId,
                    name = row.Name,
                    priceHint = row.Price,
                    quantityHint = row.Quantity,
                    candidates = candidatesPayload
                });
            }

            return result;
        }
    }
}
