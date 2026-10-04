using Application.Common.Enumerable;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.Accounting.PetMicrochipSrv
{
    // قواعد خالص (بدون دیتابیس) برای تست‌پذیری. طراحی: backend/Docs/PET_MICROCHIP_LOOKUP_FA.md
    public static class PetMicrochipRules
    {
        public const int MinDigits = 9;
        public const int MaxDigits = 15;
        public static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(24);

        private static bool TryDigit(char ch, out char digit)
        {
            digit = ch;
            if (ch >= '0' && ch <= '9') return true;
            if (ch >= '۰' && ch <= '۹') { digit = (char)('0' + (ch - '۰')); return true; }
            if (ch >= '٠' && ch <= '٩') { digit = (char)('0' + (ch - '٠')); return true; }
            return false;
        }

        /// <summary>ارقام فارسی/عربی را انگلیسی می‌کند، فاصله/خط‌تیره/نقطه را حذف می‌کند؛ اگر نتیجه ۹ تا ۱۵ رقم نباشد null.</summary>
        public static string NormalizeMicrochip(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            var sb = new StringBuilder();
            foreach (var ch in input.Trim())
            {
                if (TryDigit(ch, out var d)) sb.Append(d);
                else if (char.IsWhiteSpace(ch) || ch == '-' || ch == '.' || ch == '_' || ch == '‌' || ch == '‏' || ch == '‎') continue;
                else return null;
            }
            var s = sb.ToString();
            return s.Length >= MinDigits && s.Length <= MaxDigits ? s : null;
        }

        /// <summary>موبایل ایران: 09xxxxxxxxx (از +98/98/ارقام فارسی نرمال می‌شود)؛ نامعتبر = null</summary>
        public static string NormalizeMobile(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            var sb = new StringBuilder();
            foreach (var ch in input.Trim())
            {
                if (TryDigit(ch, out var d)) sb.Append(d);
                else if (char.IsWhiteSpace(ch) || ch == '-' || ch == '(' || ch == ')') continue;
                else if (ch == '+' && sb.Length == 0) sb.Append("00");
                else return null;
            }
            var digits = sb.ToString();
            if (digits.StartsWith("0098")) digits = "0" + digits.Substring(4);
            else if (digits.StartsWith("98") && digits.Length == 12) digits = "0" + digits.Substring(2);
            else if (digits.Length == 10 && digits.StartsWith("9")) digits = "0" + digits;
            return digits.Length == 11 && digits.StartsWith("09") ? digits : null;
        }

        public static string NewToken() => Guid.NewGuid().ToString("N");

        public static bool IsTokenFresh(DateTime createDate, DateTime now) => now - createDate <= TokenLifetime;

        public static bool IsValidStatus(int status) => Enum.IsDefined(typeof(PetMicrochipRequestStatusEnum), status);

        /// <summary>انتقال‌های مجاز ادمین: پیگیری‌شده → در حال پیگیری → حل‌شده/بسته؛ باز کردن مجدد بسته/حل‌شده فقط به «در حال پیگیری».</summary>
        public static List<int> AllowedNext(int from)
        {
            switch ((PetMicrochipRequestStatusEnum)from)
            {
                case PetMicrochipRequestStatusEnum.FollowUpRequested: return new List<int> { 3, 4, 5 };
                case PetMicrochipRequestStatusEnum.InProgress: return new List<int> { 4, 5 };
                case PetMicrochipRequestStatusEnum.Resolved: return new List<int> { 3 };
                case PetMicrochipRequestStatusEnum.Closed: return new List<int> { 3 };
                default: return new List<int>();
            }
        }

        public static bool CanTransition(int from, int to) => AllowedNext(from).Contains(to);

        public static bool IsTerminal(int status) =>
            status == (int)PetMicrochipRequestStatusEnum.Resolved || status == (int)PetMicrochipRequestStatusEnum.Closed;
    }
}
