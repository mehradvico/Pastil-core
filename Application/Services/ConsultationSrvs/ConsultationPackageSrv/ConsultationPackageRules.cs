using Application.Common.Enumerable;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.ConsultationSrvs.ConsultationPackageSrv
{
    // قواعد اعتبارسنجی ذخیره‌ی ماتریس پکیج مشاوره؛ مستقل از دیتابیس تا با تست واحد پوشش داده شود.
    public static class ConsultationPackageRules
    {
        public enum Problem
        {
            None,
            NoItems,
            UnknownCombination,
            DuplicateCombination,
            InvalidPrice,
            ActiveWithoutPrice
        }

        public static Problem Validate(IReadOnlyCollection<ConsultationPackageItemDto> items)
        {
            if (items == null || items.Count == 0)
                return Problem.NoItems;

            var seen = new HashSet<(int, int)>();
            foreach (var item in items)
            {
                if (!ConsultationRules.IsValidCombination(item.ChannelId, item.DurationMinutes))
                    return Problem.UnknownCombination;

                if (!seen.Add((item.ChannelId, item.DurationMinutes)))
                    return Problem.DuplicateCombination;

                if (double.IsNaN(item.Price) || double.IsInfinity(item.Price) || item.Price < 0)
                    return Problem.InvalidPrice;

                // پکیج رایگان/بی‌قیمت نباید برای کاربر باز شود
                if (item.Active && item.Price <= 0)
                    return Problem.ActiveWithoutPrice;
            }

            return Problem.None;
        }

        // ماتریس کامل ۸ خانه‌ای: ردیف‌های ذخیره‌شده + خانه‌های پیش‌فرض (غیرفعال، قیمت ۰) برای ترکیب‌های ذخیره‌نشده
        public static List<ConsultationPackageItemDto> BuildMatrix(IEnumerable<ConsultationPackageItemDto> stored)
        {
            var byKey = (stored ?? Enumerable.Empty<ConsultationPackageItemDto>())
                .GroupBy(s => (s.ChannelId, s.DurationMinutes))
                .ToDictionary(g => g.Key, g => g.First());

            return ConsultationRules.AllCombinations()
                .Select(c => byKey.TryGetValue((c.ChannelId, c.DurationMinutes), out var existing)
                    ? existing
                    : new ConsultationPackageItemDto { ChannelId = c.ChannelId, DurationMinutes = c.DurationMinutes })
                .ToList();
        }
    }
}
