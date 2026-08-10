using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

using System;

namespace Application.Common.Helpers
{
    public static class SearchNormalizeHelper
    {
        public static string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var text = input.Trim();

            text = text
                .Replace('۰', '0').Replace('۱', '1').Replace('۲', '2').Replace('۳', '3').Replace('۴', '4')
                .Replace('۵', '5').Replace('۶', '6').Replace('۷', '7').Replace('۸', '8').Replace('۹', '9')
                .Replace('٠', '0').Replace('١', '1').Replace('٢', '2').Replace('٣', '3').Replace('٤', '4')
                .Replace('٥', '5').Replace('٦', '6').Replace('٧', '7').Replace('٨', '8').Replace('٩', '9')
                .Replace('\u200c', ' ');

            text = text
                .Replace("ي", "ی")
                .Replace("ك", "ک")
                .Replace("ة", "ه")
                .Replace("ؤ", "و")
                .Replace("إ", "ا")
                .Replace("أ", "ا")
                .Replace("ٱ", "ا");

            text = RemoveDiacritics(text);

            text = Regex.Replace(text, @"[^\p{L}\p{N}\s]", " ");
            text = Regex.Replace(text, @"\s+", " ");
            text = text.ToLowerInvariant();

            return text;
        }

        public static string NormalizeNoSpace(string input)
            => Normalize(input).Replace(" ", "");

        public static bool IsLatin(string input)
            => !string.IsNullOrWhiteSpace(input) && Regex.IsMatch(input, @"^[a-zA-Z0-9\s]+$");

        public static string[] BuildTerms(string normalizedQuery, bool enableFuzzy = true)
        {
            if (string.IsNullOrWhiteSpace(normalizedQuery))
                return [];

            var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                normalizedQuery,
                NormalizeNoSpace(normalizedQuery)
            };

            AddSynonyms(normalizedQuery, values);

            foreach (var token in normalizedQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                values.Add(token);
                AddSynonyms(token, values);
            }

            if (IsLatin(normalizedQuery))
            {
                var keyboardValue = ConvertEnglishKeyboardToPersian(normalizedQuery);
                if (!string.IsNullOrWhiteSpace(keyboardValue))
                    values.Add(Normalize(keyboardValue));
            }

            if (enableFuzzy)
            {
                foreach (var token in normalizedQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(token => token.Length >= 4))
                {
                    for (var index = 0; index < token.Length - 1; index++)
                        values.Add(token.Substring(index, 2));
                }
            }

            return values.Where(value => value.Length >= 2).Take(20).ToArray();
        }

        private static void AddSynonyms(string value, ISet<string> values)
        {
            if (!Synonyms.TryGetValue(value, out var synonyms))
                return;

            foreach (var synonym in synonyms)
            {
                var normalized = Normalize(synonym);
                values.Add(normalized);
                values.Add(NormalizeNoSpace(normalized));
            }
        }

        private static string ConvertEnglishKeyboardToPersian(string value)
        {
            const string english = "qwertyuiop[]asdfghjkl;'zxcvbnm,";
            const string persian = "ضصثقفغعهخحجچشسیبلاتنمکگظطزرذدئو";
            var result = new StringBuilder(value.Length);

            foreach (var character in value.ToLowerInvariant())
            {
                var index = english.IndexOf(character);
                result.Append(index >= 0 ? persian[index] : character);
            }

            return result.ToString();
        }

        private static readonly IReadOnlyDictionary<string, string[]> Synonyms =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["غذا"] = ["خوراک"],
                ["خوراک"] = ["غذا"],
                ["دامپزشک"] = ["دام پزشک", "کلینیک"],
                ["کلینیک"] = ["دامپزشک", "درمانگاه"],
                ["پت شاپ"] = ["فروشگاه حیوانات", "فروشگاه"],
                ["فروشگاه"] = ["پت شاپ"],
                ["پانسیون"] = ["اقامتگاه", "نگهداری"],
                ["اقامتگاه"] = ["پانسیون"],
                ["اصلاح"] = ["آرایش", "گرومینگ"],
                ["آرایش"] = ["اصلاح", "گرومینگ"],
                ["واکسن"] = ["واکسیناسیون"],
                ["واکسیناسیون"] = ["واکسن"],
                ["گربه"] = ["پیشی"],
                ["سگ"] = ["هاپو"]
            };

        private static string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var ch in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
