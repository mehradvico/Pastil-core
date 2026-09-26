using Application.Common.Enumerable;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Application.Services.ConsultationSrvs.ConsultationPackageSrv
{
    // قواعد اعتبارسنجی یک پکیج مشاوره‌ی نام‌دار؛ مستقل از دیتابیس تا با تست واحد پوشش داده شود.
    public static class ConsultationPackageRules
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 100;
        public const int DescriptionMaxLength = 500;

        public enum Problem
        {
            None,
            NoItem,
            UnknownChannel,
            UnknownDuration,
            InvalidName,
            DescriptionTooLong,
            InvalidPrice,
            ActiveWithoutPrice
        }

        private static readonly Regex Spaces = new(@"\s+", RegexOptions.Compiled);

        // نام: trim، فاصله‌های پشت‌سرهم یکی، «ي/ك» عربی به «ی/ک» فارسی (تا «مشاوره» با دو کیبورد دو نام جدا حساب نشود)
        public static string NormalizeName(string name) =>
            Spaces.Replace((name ?? string.Empty).Replace('ي', 'ی').Replace('ك', 'ک').Trim(), " ");

        public static string NormalizeDescription(string description)
        {
            var text = (description ?? string.Empty).Replace('ي', 'ی').Replace('ك', 'ک').Trim();
            return text.Length == 0 ? null : text;
        }

        // کلید مقایسه‌ی نام برای تشخیص نام تکراری زیر یک کانال (بدون حساسیت به حروف/فاصله‌ی اضافه)
        public static string NameKey(string name) => NormalizeName(name).ToLowerInvariant();

        public static Problem Validate(ConsultationPackageItemDto item)
        {
            if (item == null)
                return Problem.NoItem;

            if (!ConsultationRules.AllowedChannels.Contains(item.ChannelId))
                return Problem.UnknownChannel;

            if (!ConsultationRules.AllowedDurations.Contains(item.DurationMinutes))
                return Problem.UnknownDuration;

            var name = NormalizeName(item.Name);
            if (name.Length < NameMinLength || name.Length > NameMaxLength)
                return Problem.InvalidName;

            if (NormalizeDescription(item.Description)?.Length > DescriptionMaxLength)
                return Problem.DescriptionTooLong;

            if (double.IsNaN(item.Price) || double.IsInfinity(item.Price) || item.Price < 0)
                return Problem.InvalidPrice;

            // پکیج رایگان/بی‌قیمت نباید برای کاربر باز شود
            if (item.Active && item.Price <= 0)
                return Problem.ActiveWithoutPrice;

            return Problem.None;
        }
    }
}
