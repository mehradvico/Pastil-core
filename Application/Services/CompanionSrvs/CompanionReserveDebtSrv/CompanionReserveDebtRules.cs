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

        // ---- یادآوری بدهی: روزی ۳ بار (ساعت ۱۰، ۱۵، ۲۰ به ساعت سرور/ایران)، فقط وقتی کسر خودکار از کیف پول ممکن نشد
        public static readonly int[] ReminderSlotHours = { 10, 15, 20 };
        // بعد از این ساعت دیگر یادآوری فرستاده نمی‌شود (نیمه‌شب مزاحمت نشود)
        public const int ReminderQuietFromHour = 22;

        // شماره‌ی بازه‌ی فعلی (۱..۳) = آخرین بازه‌ای که ساعتش رسیده؛ قبل از اولین بازه یا از ساعت سکوت به بعد null.
        // job هر ۱۵ دقیقه اجرا می‌شود و به‌خاطر کلید یکتای هر بازه، هر کاربر در هر بازه حداکثر یک پوش می‌گیرد.
        public static int? CurrentReminderSlot(DateTime now)
        {
            if (now.Hour >= ReminderQuietFromHour || now.Hour < ReminderSlotHours[0])
                return null;
            var slot = 0;
            for (var i = 0; i < ReminderSlotHours.Length; i++)
                if (now.Hour >= ReminderSlotHours[i])
                    slot = i + 1;
            return slot;
        }

        // کلید یکتای (روز، بازه) برای جلوگیری از پوش تکراری
        public static string ReminderKey(DateTime now, int slot) =>
            string.Create(System.Globalization.CultureInfo.InvariantCulture, $"debt:{now:yyyyMMdd}-{slot}");

        // کلید یکتای پوش «کسر شد» برای هر رزرو
        public static string CollectedKey(long reserveId) =>
            string.Create(System.Globalization.CultureInfo.InvariantCulture, $"debtpaid:{reserveId}");

        // پرداخت از کیف پول فقط وقتی موجودی کل بدهی را پوشش دهد (با تلورانس ۰٫۰۱ مثل PayFromWalletAsync)
        public static bool WalletCovers(double balance, double amount) => balance + 0.01 >= amount;
    }
}
