using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

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
