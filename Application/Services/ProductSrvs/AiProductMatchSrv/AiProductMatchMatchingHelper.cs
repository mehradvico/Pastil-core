using Application.Common.Helpers;
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

        // margin کوچک بین بهترین و دومین کاندید یعنی مدل واقعاً نمی‌تواند بین دو محصول شبیه‌هم تمایز بدهد.
        public static bool IsAmbiguous(double bestConfidence, double? secondConfidence, double minimumConfidence, double marginThreshold)
            => secondConfidence.HasValue
               && secondConfidence.Value >= minimumConfidence
               && (bestConfidence - secondConfidence.Value) < marginThreshold;

        // چند بسته از یک محصول در چند عکس هم‌پوشان نباید چند ردیف نتیجه‌ی جدا بشوند.
        public static List<AiProductMatchWorkingRow> DeduplicateByName(List<AiProductMatchWorkingRow> rows)
        {
            var seen = new HashSet<string>();
            var result = new List<AiProductMatchWorkingRow>();
            foreach (var row in rows)
            {
                var key = SearchNormalizeHelper.NormalizeNoSpace(row.Name);
                if (seen.Add(key))
                    result.Add(row);
            }
            return result;
        }

        private static HashSet<string> NormalizeTokens(string value)
            => string.IsNullOrWhiteSpace(value)
                ? new HashSet<string>()
                : SearchNormalizeHelper.Normalize(value).Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
    }
}
