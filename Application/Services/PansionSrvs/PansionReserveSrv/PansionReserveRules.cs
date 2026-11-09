using Application.Common.Helpers;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionReserveSrv
{
    // قواعد قیمت و زمان رزرو پانسیون/مهد؛ همه‌ی اعداد مالی فقط از همین‌جا و از روی داده‌ی سرور ساخته می‌شوند، نه از بدنه‌ی درخواست
    public static class PansionReserveRules
    {
        // تعرفه‌ی پانسیون «هر شب» است: ورود ۲۰ و خروج ۲۱ = ۱ شب. قبلاً روزها شمرده می‌شد ((to-from)+1) و هر رزرو یک شب اضافه می‌گرفت.
        public static int NightCount(DateTime fromDate, DateTime toDate) => (toDate.Date - fromDate.Date).Days;

        // مهد ساعتی است و ساعت ناقص یک ساعت کامل حساب می‌شود (۰۸:۰۰ تا ۰۹:۳۰ = ۲ ساعت)
        public static int DaycareHourCount(TimeSpan start, TimeSpan end) => (int)Math.Ceiling((end - start).TotalHours);

        // دو اقامت شبانه فقط وقتی تداخل دارند که شبی مشترک داشته باشند؛ روز خروج یکی می‌تواند روز ورود دیگری باشد
        public static bool StaysOverlap(DateTime firstFrom, DateTime firstTo, DateTime secondFrom, DateTime secondTo) =>
            firstFrom.Date < secondTo.Date && secondFrom.Date < firstTo.Date;

        // OpenHour/CloseHour متن آزادِ پنل است («08:00»، «8»، «24:00»)؛ اگر قابل خواندن نبود چک ساعت کاری انجام نمی‌شود
        public static bool TryParseHour(string value, out TimeSpan hour)
        {
            hour = default;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var text = value.Trim();
            if (text == "24" || text == "24:00")
            {
                hour = TimeSpan.FromHours(24);
                return true;
            }

            if (int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var wholeHour) && wholeHour >= 0 && wholeHour < 24)
            {
                hour = TimeSpan.FromHours(wholeHour);
                return true;
            }

            return TimeSpan.TryParseExact(text, new[] { "hh\\:mm", "h\\:mm" }, CultureInfo.InvariantCulture, out hour) &&
                   hour >= TimeSpan.Zero && hour < TimeSpan.FromHours(24);
        }

        public enum PetRateMatch { Accepted, PetNotAccepted, SizeNotAccepted }

        // فهرست خالی یعنی مرکز همه را می‌پذیرد. ردیف هم‌سایز بر ردیف «همه‌ی سایزها» مقدم است؛ پت بدون سایز فقط با ردیف «همه‌ی سایزها» می‌خواند.
        public static PetRateMatch MatchPetRate(IReadOnlyCollection<Entities.Entities.PansionField.PansionPet> pansionPets, long petId, string petSize, out Entities.Entities.PansionField.PansionPet rate)
        {
            rate = null;
            if (pansionPets == null || pansionPets.Count == 0)
                return PetRateMatch.Accepted;

            var ofPet = pansionPets.Where(s => s.PetId == petId).ToList();
            if (ofPet.Count == 0)
                return PetRateMatch.PetNotAccepted;

            var size = VenuePetSizes.One(petSize);
            rate = (size == null ? null : ofPet.FirstOrDefault(s => VenuePetSizes.One(s.PetSize) == size))
                   ?? ofPet.FirstOrDefault(s => VenuePetSizes.One(s.PetSize) == null);
            return rate == null ? PetRateMatch.SizeNotAccepted : PetRateMatch.Accepted;
        }

        public static double NightPrice(Entities.Entities.PansionField.Pansion pansion, Entities.Entities.PansionField.PansionPet rate) =>
            rate?.PansionPrice > 0 ? rate.PansionPrice.Value : pansion.PansionPrice;

        public static double HourPrice(Entities.Entities.PansionField.Pansion pansion, Entities.Entities.PansionField.PansionPet rate) =>
            rate?.SchoolPrice > 0 ? rate.SchoolPrice.Value : pansion.SchoolPrice;

        // ردیف تعرفه‌ی پتِ این رزرو؛ null = تعرفه‌ی پایه‌ی مرکز.
        public static async Task<Entities.Entities.PansionField.PansionPet> FindRateAsync(IDataBaseContext context, long pansionId, long userPetId)
        {
            var pet = await context.UserPets.AsNoTracking()
                .Where(s => s.Id == userPetId)
                .Select(s => new { s.PetId, s.Size })
                .FirstOrDefaultAsync();
            if (pet == null)
                return null;
            var pansionPets = await context.PansionPets.AsNoTracking()
                .Where(s => s.PansionId == pansionId && !s.Deleted)
                .ToListAsync();
            MatchPetRate(pansionPets, pet.PetId, pet.Size, out var rate);
            return rate;
        }

        public enum RepriceResult { Unchanged, Changed, Invalid }

        // شروع پرداخت: مبلغ پایه دوباره از تعرفه‌ی فعلی مرکز × شب/ساعت ساخته می‌شود و همان به درگاه می‌رود (رزروهای قدیمی‌ترِ پرداخت‌نشده
        // با فرمول «روز + ۱» هم این‌جا اصلاح می‌شوند). تخفیفِ اعمال‌شده دست نمی‌خورد؛ اگر با پایه‌ی جدید نخواند، اعتبارسنجی تخفیف ردش می‌کند.
        public static RepriceResult Reprice(Entities.Entities.PansionField.PansionReserve reserve, Entities.Entities.PansionField.Pansion pansion, Entities.Entities.PansionField.PansionPet rate = null)
        {
            if (reserve == null || pansion == null)
                return RepriceResult.Invalid;

            int dayCount = 0, hourCount = 0;
            double basePrice;
            if (reserve.FromDate.HasValue && reserve.ToDate.HasValue)
            {
                dayCount = NightCount(reserve.FromDate.Value, reserve.ToDate.Value);
                if (dayCount <= 0)
                    return RepriceResult.Invalid;
                basePrice = NightPrice(pansion, rate) * dayCount;
            }
            else if (TimeSpan.TryParseExact(reserve.StartTime, "hh\\:mm", CultureInfo.InvariantCulture, out var start) &&
                     TimeSpan.TryParseExact(reserve.EndTime, "hh\\:mm", CultureInfo.InvariantCulture, out var end) && end > start)
            {
                hourCount = DaycareHourCount(start, end);
                basePrice = HourPrice(pansion, rate) * hourCount;
            }
            else
            {
                return RepriceResult.Invalid;
            }

            if (basePrice <= 0)
                return RepriceResult.Invalid;
            if (Math.Abs(basePrice - reserve.Price) < 0.01 && reserve.DayCount == dayCount && reserve.HourCount == hourCount)
                return RepriceResult.Unchanged;

            reserve.Price = basePrice;
            reserve.DayCount = dayCount;
            reserve.HourCount = hourCount;
            reserve.PaymentPrice = Math.Max(0, basePrice - reserve.RebatePrice);
            return RepriceResult.Changed;
        }

        public static bool IsWithinWorkingHours(string openHour, string closeHour, TimeSpan start, TimeSpan end)
        {
            if (!TryParseHour(openHour, out var open) || !TryParseHour(closeHour, out var close) || close <= open)
                return true;
            return start >= open && end <= close;
        }
    }
}
