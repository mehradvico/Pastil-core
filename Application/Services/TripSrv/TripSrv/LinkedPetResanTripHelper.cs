using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Services.TripSrv.TripSrv.Dto;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.TripSrv.TripSrv
{
    // آخرین سفر پت‌رسانِ لغونشده‌ی متصل به هر رزرو (خدمت/پانسیون/مدرسه)، برای نمایش در رزرو و گزارش مالی؛
    // یک کوئری دسته‌ای به‌ازای هر صفحه (نه N+1) — طراحی: درخواست «قیمت پت‌رسان توی رزروها/مدیریت مالی».
    public static class LinkedPetResanTripHelper
    {
        private const long Canceled = (long)TripStatusEnum.TripStatus_Canceled;

        public static Task<Dictionary<long, LinkedPetResanTripVDto>> ForCompanionReservesAsync(IDataBaseContext context, IEnumerable<long> reserveIds) =>
            LoadAsync(context, t => t.CompanionReserveId, reserveIds);

        public static Task<Dictionary<long, LinkedPetResanTripVDto>> ForClinicReservesAsync(IDataBaseContext context, IEnumerable<long> reserveIds) =>
            LoadAsync(context, t => t.ClinicReserveId, reserveIds);

        public static Task<Dictionary<long, LinkedPetResanTripVDto>> ForPansionReservesAsync(IDataBaseContext context, IEnumerable<long> reserveIds) =>
            LoadAsync(context, t => t.PansionReserveId, reserveIds);

        public static Task<Dictionary<long, LinkedPetResanTripVDto>> ForSchoolReservesAsync(IDataBaseContext context, IEnumerable<long> reserveIds) =>
            LoadAsync(context, t => t.SchoolReserveId, reserveIds);

        public static Task<Dictionary<long, LinkedPetResanTripVDto>> ForGroomerReservesAsync(IDataBaseContext context, IEnumerable<long> reserveIds) =>
            LoadAsync(context, t => t.GroomerReserveId, reserveIds);

        public static Task<Dictionary<long, LinkedPetResanTripVDto>> ForCoachReservesAsync(IDataBaseContext context, IEnumerable<long> reserveIds) =>
            LoadAsync(context, t => t.CoachReserveId, reserveIds);

        // ------------------------------------------------------------------ پت‌رسانِ همراه رزرو (یک پرداخت)

        /// <summary>سفرهای متصل به یک رزروِ مرکز؛ null یعنی این نوع پرداخت پت‌رسانِ همراه ندارد.</summary>
        public static System.Linq.Expressions.Expression<System.Func<Entities.Entities.Trip, bool>> ReserveMatch(PaymentCallbackTypeEnum type, long reserveId) => type switch
        {
            PaymentCallbackTypeEnum.ClinicReserve => t => t.ClinicReserveId == reserveId,
            PaymentCallbackTypeEnum.GroomerReserve => t => t.GroomerReserveId == reserveId,
            PaymentCallbackTypeEnum.CoachReserve => t => t.CoachReserveId == reserveId,
            PaymentCallbackTypeEnum.PansionReserve => t => t.PansionReserveId == reserveId,
            _ => null
        };

        /// <summary>سفرِ «منتظر پرداخت رزرو»: همراه رزرو ثبت شده، هنوز پرداخت نشده و لغو نشده.</summary>
        public static IQueryable<Entities.Entities.Trip> PendingBundled(IQueryable<Entities.Entities.Trip> trips, PaymentCallbackTypeEnum type, long reserveId)
        {
            var match = ReserveMatch(type, reserveId);
            if (match == null)
                return trips.Where(t => false);
            return trips.Where(match).Where(t => t.PaidWithReserve && !t.IsPaid && t.TripStatusId == (long)TripStatusEnum.TripStatus_Requested);
        }

        /// <summary>
        /// سفر متصل (قبل از «پت سوار شد») همراه رزرو جابه‌جا می‌شود: حرکت = زمان جدید نوبت منهای فاصله‌ی انتخابی کاربر. قیمت عوض نمی‌شود.
        /// ذخیره با فراخواننده است (داخل تراکنش خودش).
        /// </summary>
        public static async Task<Entities.Entities.Trip> RescheduleAsync(IDataBaseContext context, PaymentCallbackTypeEnum type, long reserveId, System.DateTime newStartAt, System.DateTime now)
        {
            var requested = (long)TripStatusEnum.TripStatus_Requested;
            var accepted = (long)TripStatusEnum.TripStatus_Accepted;
            var petPickedUp = (int)TripProgressStageEnum.PetPickedUp;
            var trip = await context.Trips.AsTracking()
                .Where(ReserveMatch(type, reserveId))
                .Where(t => (t.TripStatusId == requested || t.TripStatusId == accepted) && t.ProgressStageId < petPickedUp)
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();
            if (trip == null)
                return null;

            var departure = newStartAt.AddMinutes(-(trip.ScheduledLeadMinutes ?? 60));
            if (departure < now)
                departure = now;
            trip.ScheduledDepartureAt = departure;
            trip.TripStartDateTime = departure;
            trip.DriverReminderSentDate = null;
            if (!trip.DriverId.HasValue)
                trip.ScheduledDispatched = false;
            return trip;
        }

        private static async Task<Dictionary<long, LinkedPetResanTripVDto>> LoadAsync(
            IDataBaseContext context,
            System.Linq.Expressions.Expression<System.Func<Entities.Entities.Trip, long?>> reserveIdSelector,
            IEnumerable<long> reserveIds)
        {
            var ids = reserveIds?.Distinct().ToList() ?? new List<long>();
            if (ids.Count == 0)
                return new Dictionary<long, LinkedPetResanTripVDto>();

            var reserveIdOf = reserveIdSelector.Compile();

            var rows = await context.Trips.AsNoTracking()
                .Where(t => t.TripStatusId != Canceled)
                .Include(t => t.Driver)
                .Include(t => t.TripStatus)
                .Include(t => t.DriverStatus)
                .Where(BuildContainsPredicate(reserveIdSelector, ids))
                .ToListAsync();

            return rows
                .GroupBy(t => reserveIdOf(t)!.Value)
                .ToDictionary(g => g.Key, g =>
                {
                    // اگر بارها درخواست ثبت شده (مثلاً یکی لغو و دوباره درخواست شد)، جدیدترینِ لغونشده ملاک است
                    var t = g.OrderByDescending(x => x.Id).First();
                    return new LinkedPetResanTripVDto
                    {
                        TripId = t.Id,
                        Price = t.Price,
                        PaymentPrice = t.PaymentPrice,
                        IsPaid = t.IsPaid,
                        PaidWithReserve = t.PaidWithReserve,
                        ScheduledDepartureAt = t.ScheduledDepartureAt,
                        ScheduledLeadMinutes = t.ScheduledLeadMinutes,
                        TripStatusLabel = t.TripStatus?.Name,
                        DriverStatusLabel = t.DriverStatus?.Name,
                        DriverId = t.DriverId,
                        DriverName = t.Driver?.Name
                    };
                });
        }

        private static System.Linq.Expressions.Expression<System.Func<Entities.Entities.Trip, bool>> BuildContainsPredicate(
            System.Linq.Expressions.Expression<System.Func<Entities.Entities.Trip, long?>> selector,
            List<long> ids)
        {
            var parameter = selector.Parameters[0];
            var body = System.Linq.Expressions.Expression.Call(
                System.Linq.Expressions.Expression.Constant(ids),
                typeof(List<long>).GetMethod("Contains", new[] { typeof(long) })!,
                System.Linq.Expressions.Expression.Property(selector.Body, "Value"));
            var notNull = System.Linq.Expressions.Expression.NotEqual(selector.Body, System.Linq.Expressions.Expression.Constant(null, typeof(long?)));
            return System.Linq.Expressions.Expression.Lambda<System.Func<Entities.Entities.Trip, bool>>(
                System.Linq.Expressions.Expression.AndAlso(notNull, body), parameter);
        }
    }
}
