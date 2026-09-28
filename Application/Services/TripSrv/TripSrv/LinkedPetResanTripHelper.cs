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

        public static Task<Dictionary<long, LinkedPetResanTripVDto>> ForPansionReservesAsync(IDataBaseContext context, IEnumerable<long> reserveIds) =>
            LoadAsync(context, t => t.PansionReserveId, reserveIds);

        public static Task<Dictionary<long, LinkedPetResanTripVDto>> ForSchoolReservesAsync(IDataBaseContext context, IEnumerable<long> reserveIds) =>
            LoadAsync(context, t => t.SchoolReserveId, reserveIds);

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
