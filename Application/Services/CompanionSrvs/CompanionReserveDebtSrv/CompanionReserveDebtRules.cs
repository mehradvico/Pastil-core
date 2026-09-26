using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReserveDebtSrv
{
    // بدهی «هزینه‌ی پرداخت‌نشده‌ی خدمت»: اپراتور/پزشک هزینه‌ی نهایی را ثبت کرده ولی کاربر پرداخت نکرده است.
    // ۷ روز بعد از ثبت، کاربر تا پرداخت نمی‌تواند رزرو/خرید جدید (خدمت، پانسیون، مدرسه، مشاوره) انجام دهد.
    public static class CompanionReserveDebtRules
    {
        public const int LockAfterDays = 7;

        public static DateTime DueDate(DateTime unpaidDate) => unpaidDate.AddDays(LockAfterDays);

        // قفل: حداقل یک بدهی باز که مهلتش تمام شده باشد
        public static bool IsLocked(IEnumerable<DateTime?> openDebtDates, DateTime now)
            => (openDebtDates ?? Enumerable.Empty<DateTime?>()).Any(d => d.HasValue && now >= DueDate(d.Value));

        public static int DaysLeft(DateTime unpaidDate, DateTime now)
            => Math.Max(0, (int)Math.Ceiling((DueDate(unpaidDate) - now).TotalDays));

        public static async Task<bool> IsLockedAsync(IDataBaseContext context, long userId, DateTime now)
        {
            if (userId <= 0)
                return false;
            var threshold = now.AddDays(-LockAfterDays);
            return await context.CompanionReserves.AsNoTracking().AnyAsync(r =>
                r.BookerId == userId && r.OperatorUnpaid && r.OperatorDebtPaidDate == null && !r.IsCancel &&
                r.OperatorUnpaidDate != null && r.OperatorUnpaidDate <= threshold);
        }

        // سهم سایت از مبلغی که بعداً پرداخت شده (کمیسیون خدمت)
        public static double SiteShareOf(double amount, decimal commissionPercent)
            => (double)((decimal)amount * commissionPercent / 100m);
    }
}
