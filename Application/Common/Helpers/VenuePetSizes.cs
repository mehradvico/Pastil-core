using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Common.Helpers
{
    /// <summary>
    /// سایز پت در بسته‌های خدمت مرکزها (مربی/کلینیک/آرایشگاه). بسته می‌تواند چند سایز داشته باشد؛ در ستون PetSize به‌صورت
    /// «Small,Medium» (به ترتیب ثابت Small، Medium، Large) ذخیره می‌شود. خالی یا هر سه سایز = همه‌ی سایزها (null).
    /// نام سایزها زیررشته‌ی هم نیستند، پس فیلتر SQL با Contains امن است.
    /// </summary>
    public static class VenuePetSizes
    {
        public const string Small = "Small";
        public const string Medium = "Medium";
        public const string Large = "Large";
        public static readonly string[] All = { Small, Medium, Large };

        /// <summary>یک سایز (سایز پت کاربر یا فیلتر): نام استاندارد، یا null اگر خالی/نامعتبر.</summary>
        public static string One(string value) =>
            string.IsNullOrWhiteSpace(value) ? null : All.FirstOrDefault(s => string.Equals(s, value.Trim(), StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// سایزهای بسته از لیست (اولویت) یا رشته‌ی جداشده با کاما. false = سایز نامعتبر.
        /// normalized: null برای «همه» (خالی یا هر سه)، وگرنه رشته‌ی مرتب‌شده مثل «Small,Large».
        /// </summary>
        public static bool TryNormalizeSet(string csv, IEnumerable<string> list, out string normalized)
        {
            normalized = null;
            var tokens = (list != null && list.Any() ? list : (csv ?? string.Empty).Split(','))
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();
            var picked = new HashSet<string>();
            foreach (var token in tokens)
            {
                var size = One(token);
                if (size == null)
                    return false;
                picked.Add(size);
            }
            if (picked.Count == 0 || picked.Count == All.Length)
                return true;
            normalized = string.Join(",", All.Where(picked.Contains));
            return true;
        }

        /// <summary>لیست سایزهای ذخیره‌شده؛ خالی = همه‌ی سایزها.</summary>
        public static List<string> Split(string stored) =>
            string.IsNullOrWhiteSpace(stored)
                ? new List<string>()
                : stored.Split(',').Select(One).Where(s => s != null).Distinct().ToList();

        /// <summary>بسته با سایز پت می‌خواند؟ بسته‌ی بدون سایز با همه می‌خواند؛ پت بدون سایز فقط با بسته‌ی بدون سایز.</summary>
        public static bool Matches(string stored, string petSize)
        {
            var sizes = Split(stored);
            if (sizes.Count == 0)
                return true;
            var size = One(petSize);
            return size != null && sizes.Contains(size);
        }
    }
}
