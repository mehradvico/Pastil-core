using System.Globalization;
using System.Text;

namespace Application.Services.Setting.SmsSrv
{
    // توکن Lookup کاوه‌نگار فاصله نمی‌پذیرد و حداکثر ۱۰۰ نویسه است (kavenegar.com/rest.html).
    // طبق ردیف‌های واقعی Smses (۹۰ روز تا ۱۴۰۵/۰۷/۲۰): نیم‌فاصله (ZWNJ) همیشه ۴۳۱ «ساختار کد صحیح نمی‌باشد» گرفته (۰ موفق)،
    // ولی «_» و «-» و «/» بی‌مشکل رفته‌اند. پس هر فاصله، «_» و نویسه‌ی نامرئی (ZWNJ، ZWJ، RLM/LRM… = UnicodeCategory.Format)
    // به یک «_» تبدیل می‌شود.
    public static class KavenegarLookupToken
    {
        public const int MaxLength = 100;
        private const char Separator = '_';

        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var token = new StringBuilder(value.Length);
            var pendingSeparator = false;
            foreach (var c in value)
            {
                if (char.IsWhiteSpace(c) || c == Separator || CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.Format)
                {
                    pendingSeparator = token.Length > 0;
                    continue;
                }
                if (pendingSeparator)
                {
                    token.Append(Separator);
                    pendingSeparator = false;
                }
                token.Append(c);
            }

            if (token.Length == 0)
                return null;
            var result = token.ToString();
            return result.Length > MaxLength ? result.Substring(0, MaxLength).TrimEnd(Separator) : result;
        }
    }
}
