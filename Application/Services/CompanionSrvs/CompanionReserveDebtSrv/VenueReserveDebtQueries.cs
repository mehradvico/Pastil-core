using Entities.Entities.CommonField;
using System;
using System.Linq;

namespace Application.Services.CompanionSrvs.CompanionReserveDebtSrv
{
    /// <summary>نوع رزروی که بدهی «پرداخت‌نشده» روی آن ثبت شده؛ ۰ = رزرو قدیمی همراه (CompanionReserve).</summary>
    public enum ReserveDebtKindEnum
    {
        Companion = 0,
        Coach = 1,
        Clinic = 2,
        Groomer = 3,
    }

    /// <summary>
    /// بدهی رزروهای مربی/کلینیک/آرایشگاه (IVenueCompletableReserve) با همان قاعده‌ی بدهی همراه: مهلت ۷ روز از DoneDate،
    /// کسر خودکار از کیف پول، یادآوری و قفل رزرو جدید. کوئری‌ها عمومی‌اند تا تست ترجمه‌ی SQL داشته باشند.
    /// </summary>
    public static class VenueReserveDebtQueries
    {
        public static IQueryable<T> Open<T>(IQueryable<T> source) where T : class, IVenueCompletableReserve =>
            source.Where(r => r.NotReceived && r.DebtAmount > 0 && r.DebtPaidDate == null && !r.IsCancel && r.DoneDate != null);

        // مهلت تمام‌شده: DoneDate <= now − ۷ روز
        public static IQueryable<T> Overdue<T>(IQueryable<T> source, long userId, DateTime now) where T : class, IVenueCompletableReserve
        {
            var threshold = now.AddDays(-CompanionReserveDebtRules.LockAfterDays);
            return Open(source).Where(r => r.BookerId == userId && r.DoneDate <= threshold);
        }
    }
}
