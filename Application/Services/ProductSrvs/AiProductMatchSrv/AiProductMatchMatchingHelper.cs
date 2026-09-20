using Application.Common.Helpers;
using Application.Services.ProductSrvs.AiProductMatchSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.ProductSrvs.AiProductMatchSrv
{
    // منطق خالص (بدون I/O) پشت دو قاعده‌ی سخت‌گیرانه‌ی تطبیق قفسه:
    // ۱) امتیاز نهایی هرگز فقط عدد خام مدل نیست — شباهت نام نرمال‌شده هم وزن دارد، چون مدل روی
    //    اسم‌های شبیه‌هم (طعم/سایز متفاوت از یک برند) اغلب بیش‌ازحد مطمئن است.
    // ۲) وقتی دو کاندید برتر به‌شدت نزدیک‌اند، انتخاب خودکار باید غیرفعال شود (تصمیم با فروشنده).
    public static class AiProductMatchMatchingHelper
    {
        // جریمه‌ی سنگین (نه صفر مطلق، چون خود OCR برند هم می‌تواند خطا داشته باشد) وقتی برندی که از
        // روی عکس با اطمینان خونده شده با برند کاندید نمی‌خونه — نباید هیچ‌وقت این ناهم‌خوانی با شباهت
        // متنی بقیه‌ی نام جبران بشه.
        private const double BrandMismatchPenaltyFactor = 0.3;

        public static double ComputeCompositeConfidence(
            double modelConfidence, string detectedName, string detectedBrand, string candidateName, string candidateBrandName)
        {
            var similarity = ComputeNameSimilarity(detectedName, candidateName, candidateBrandName);
            var composite = Math.Clamp(0.7 * modelConfidence + 0.3 * similarity, 0, 1);

            if (IsBrandMismatch(detectedBrand, candidateBrandName))
                composite *= BrandMismatchPenaltyFactor;

            return composite;
        }

        private static bool IsBrandMismatch(string detectedBrand, string candidateBrandName)
        {
            if (string.IsNullOrWhiteSpace(detectedBrand) || string.IsNullOrWhiteSpace(candidateBrandName))
                return false;

            return SearchNormalizeHelper.NormalizeNoSpace(detectedBrand) != SearchNormalizeHelper.NormalizeNoSpace(candidateBrandName);
        }

        public static double ComputeNameSimilarity(string detectedName, string candidateName, string candidateBrandName)
        {
            var detectedTokens = NormalizeTokens(detectedName);
            var candidateTokens = NormalizeTokens(candidateName);
            if (detectedTokens.Count == 0 || candidateTokens.Count == 0)
                return 0;

            var intersection = detectedTokens.Intersect(candidateTokens).Count();
            var union = detectedTokens.Union(candidateTokens).Count();
            var jaccard = union == 0 ? 0 : (double)intersection / union;

            var brandTokens = NormalizeTokens(candidateBrandName);
            var brandBonus = brandTokens.Count > 0 && brandTokens.All(detectedTokens.Contains) ? 0.15 : 0;

            return Math.Clamp(jaccard + brandBonus, 0, 1);
        }

        // شبکه‌ی ایمنی مرحله‌ی تطبیق: وقتی مدل برای یک ردیف هیچ نتیجه‌ای نداد (بودجه‌ی زمانی تمام شد،
        // Provider خطا/Timeout داد، یا پاسخ ناقص/بی‌rowId برگشت)، ردیف نباید «نامشخص» شود در حالی که
        // کاندیدهای واقعی دیتابیس همین‌جا در دست‌اند. این رتبه‌بندی فقط چند مقایسه‌ی رشته روی حداکثر
        // ~۲۵ کاندید است: صفر فراخوانی شبکه، صفر Query اضافه، عملاً بدون هزینه روی سرور.
        // عمداً فقط «پیشنهاد» تولید می‌کند و هرگز نباید به انتخاب خودکار منجر شود — چون بر خلاف مدل،
        // اینجا هیچ فیلتر سختی روی نوع حیوان/برند/سایز اعمال نشده و دو سایز مختلف یک محصول نمرات
        // بسیار نزدیکی می‌گیرند.
        public static AiProductMatchRankedRowResult RankLocally(
            string rowId,
            string detectedName,
            string detectedBrand,
            IReadOnlyList<AiProductMatchCandidateProduct> candidates,
            double minimumConfidence,
            int maxResults = 3)
        {
            var result = new AiProductMatchRankedRowResult { RowId = rowId };
            if (candidates == null || candidates.Count == 0)
                return result;

            result.Ranked.AddRange(candidates
                .Select((candidate, index) => new AiProductMatchRankedCandidate
                {
                    Index = index,
                    PackageIndex = null,
                    // همان فرمول مرحله‌ی عادی، با شباهت نام به‌جای نمره‌ی مدل؛ پس جریمه‌ی ناهم‌خوانی برند
                    // هم در ComputeCompositeConfidence پایین‌دست دقیقاً مثل مسیر عادی اعمال می‌شود.
                    Confidence = ComputeNameSimilarity(detectedName, candidate.Name, candidate.BrandName)
                })
                .Where(candidate => candidate.Confidence >= minimumConfidence)
                .OrderByDescending(candidate => candidate.Confidence)
                .Take(maxResults));

            return result;
        }

        // margin کوچک بین بهترین و دومین کاندید یعنی مدل واقعاً نمی‌تواند بین دو محصول شبیه‌هم تمایز بدهد.
        public static bool IsAmbiguous(double bestConfidence, double? secondConfidence, double minimumConfidence, double marginThreshold)
            => secondConfidence.HasValue
               && secondConfidence.Value >= minimumConfidence
               && (bestConfidence - secondConfidence.Value) < marginThreshold;

        // چند بسته از یک محصول در چند عکس هم‌پوشان (قفسه) یا چند اسکرین‌شات هم‌پوشان (جدول نرم‌افزار
        // انبار، از اسکرول صفحه‌به‌صفحه) نباید چند ردیف نتیجه‌ی جدا بشوند. اولویت با کد/بارکد دقیقاً
        // یکسان است (وقتی هر دو ردیف externalCode دارند)؛ در غیر این صورت نام نرمال‌شده کلید تشخیص است.
        public static List<AiProductMatchWorkingRow> DeduplicateRows(List<AiProductMatchWorkingRow> rows)
        {
            var kept = new Dictionary<string, AiProductMatchWorkingRow>();
            var result = new List<AiProductMatchWorkingRow>();
            foreach (var row in rows)
            {
                var key = !string.IsNullOrWhiteSpace(row.ExternalCode)
                    ? "code:" + SearchNormalizeHelper.NormalizeNoSpace(row.ExternalCode)
                    : "name:" + SearchNormalizeHelper.NormalizeNoSpace(row.Name);

                if (kept.TryGetValue(key, out var first))
                {
                    // ردیف تکراری حذف می‌شود ولی کادرهایش (هر بستهٔ قابل‌رؤیت از همان محصول) به ردیف اول اضافه می‌شود
                    first.BoundingBoxes.AddRange(row.BoundingBoxes);
                    if (row.NameConfidence > (first.NameConfidence ?? 0))
                        first.NameConfidence = row.NameConfidence;
                    continue;
                }

                kept[key] = row;
                result.Add(row);
            }
            return result;
        }

        private const double MinBoxSide = 0.02;

        // ورودی: [ymin, xmin, ymax, xmax] خام مدل. مقیاس ۰..۱۰۰۰ (قالب Gemini) یا ۰..۱ هر دو پذیرفته می‌شود؛
        // کادر خارج از عکس clamp می‌شود و کادر با عرض/ارتفاع زیر ۲٪ (یا معکوس/تهی) دور ریخته می‌شود.
        public static List<AiProductMatchBoundingBoxDto> NormalizeBoxes(IEnumerable<double[]> rawBoxes)
        {
            var result = new List<AiProductMatchBoundingBoxDto>();
            if (rawBoxes == null)
                return result;

            foreach (var raw in rawBoxes)
            {
                if (raw == null || raw.Length != 4 || raw.Any(v => !double.IsFinite(v)))
                    continue;

                var scale = raw.All(v => v <= 1.0) ? 1.0 : 1000.0;
                var ymin = Math.Clamp(raw[0] / scale, 0, 1);
                var xmin = Math.Clamp(raw[1] / scale, 0, 1);
                var ymax = Math.Clamp(raw[2] / scale, 0, 1);
                var xmax = Math.Clamp(raw[3] / scale, 0, 1);

                var width = xmax - xmin;
                var height = ymax - ymin;
                if (width < MinBoxSide || height < MinBoxSide)
                    continue;

                result.Add(new AiProductMatchBoundingBoxDto
                {
                    X = Math.Round(xmin, 4),
                    Y = Math.Round(ymin, 4),
                    Width = Math.Round(width, 4),
                    Height = Math.Round(height, 4)
                });
            }

            return result;
        }

        // «۴۰۰ گرم» — برای ساخت پیش‌نویس محصول ثبت‌نشده؛ اگر مقدار یا واحد خوانا نبود null.
        public static string FormatPackageSize(double? value, string unit)
        {
            if (!value.HasValue || value.Value <= 0 || !double.IsFinite(value.Value) || string.IsNullOrWhiteSpace(unit))
                return null;

            var unitFa = unit.Trim().ToLowerInvariant() switch
            {
                "kilogram" or "kg" => "کیلوگرم",
                "gram" or "g" => "گرم",
                "liter" or "litre" or "l" => "لیتر",
                "milliliter" or "ml" => "میلی‌لیتر",
                "count" or "piece" or "pcs" => "عدد",
                _ => unit.Trim()
            };

            var number = value.Value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
            var persianNumber = new string(number.Select(c => c >= '0' && c <= '9' ? (char)('۰' + (c - '0')) : c).ToArray());
            return $"{persianNumber} {unitFa}";
        }

        private static HashSet<string> NormalizeTokens(string value)
            => string.IsNullOrWhiteSpace(value)
                ? new HashSet<string>()
                : SearchNormalizeHelper.Normalize(value).Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
    }
}
