using System.Globalization;

namespace Application.Common.Helpers
{
    public static class PersianPushTextHelper
    {
        public const string DefaultTitle = "پاستیل";
        public const string DefaultBody = "اعلان جدیدی برای شما ارسال شده است.";

        private static readonly CultureInfo PersianCulture = CultureInfo.GetCultureInfo("fa");

        public static string ResolvePattern(string resourceKeyOrText, string fallback)
        {
            if (string.IsNullOrWhiteSpace(resourceKeyOrText))
                return fallback;

            var localized = Resource.Pattern.ResourceManager.GetString(resourceKeyOrText, PersianCulture);
            if (ContainsPersian(localized))
                return localized;

            return EnsurePersian(resourceKeyOrText, fallback);
        }

        public static string EnsurePersian(string value, string fallback)
        {
            return ContainsPersian(value) ? value.Trim() : fallback;
        }

        public static bool ContainsPersian(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            foreach (var character in value)
            {
                if ((character >= '\u0600' && character <= '\u06FF') ||
                    (character >= '\u0750' && character <= '\u077F') ||
                    (character >= '\u08A0' && character <= '\u08FF') ||
                    (character >= '\uFB50' && character <= '\uFDFF') ||
                    (character >= '\uFE70' && character <= '\uFEFF'))
                    return true;
            }

            return false;
        }
    }
}
