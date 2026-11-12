using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Services.TripSrv.TripSrv;
using Application.Services.TripSrv.TripSrv.Dto;
using Entities.Entities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Application.Tests
{
    // پت‌رسانِ همراه رزرو (۲۰۲۶-۱۱-۱۱): یک پرداخت برای رزرو + سفر؛ مبلغ سفر برای مرکز پنهان.
    public class ReservePetResanBundleTests
    {
        [Fact]
        public void Center_sees_no_trip_amounts()
        {
            var trip = new LinkedPetResanTripVDto { TripId = 1, Price = 300_000, PaymentPrice = 250_000, PaidWithReserve = true };
            Assert.Equal(250_000, trip.Payable());
            trip.HideAmounts();
            Assert.True(trip.AmountsHidden);
            Assert.Equal(0, trip.Price);
            Assert.Equal(0, trip.PaymentPrice);
        }

        [Fact]
        public void Payable_falls_back_to_price_when_no_payment_price()
        {
            Assert.Equal(120_000, new LinkedPetResanTripVDto { Price = 120_000 }.Payable());
        }

        [Theory]
        [InlineData(PaymentCallbackTypeEnum.ClinicReserve)]
        [InlineData(PaymentCallbackTypeEnum.GroomerReserve)]
        [InlineData(PaymentCallbackTypeEnum.CoachReserve)]
        [InlineData(PaymentCallbackTypeEnum.PansionReserve)]
        public void Pending_bundled_matches_only_unpaid_requested_trip_of_that_reserve(PaymentCallbackTypeEnum type)
        {
            const long reserveId = 7;
            Trip Make(long id, bool bundled, bool paid, TripStatusEnum status, long linked)
            {
                var t = new Trip { Id = id, PaidWithReserve = bundled, IsPaid = paid, TripStatusId = (long)status };
                switch (type)
                {
                    case PaymentCallbackTypeEnum.ClinicReserve: t.ClinicReserveId = linked; break;
                    case PaymentCallbackTypeEnum.GroomerReserve: t.GroomerReserveId = linked; break;
                    case PaymentCallbackTypeEnum.CoachReserve: t.CoachReserveId = linked; break;
                    default: t.PansionReserveId = linked; break;
                }
                return t;
            }

            var trips = new List<Trip>
            {
                Make(1, true, false, TripStatusEnum.TripStatus_Requested, reserveId),   // ✓
                Make(2, true, true, TripStatusEnum.TripStatus_Requested, reserveId),    // پرداخت‌شده
                Make(3, false, false, TripStatusEnum.TripStatus_Requested, reserveId),  // پرداخت جدا
                Make(4, true, false, TripStatusEnum.TripStatus_Canceled, reserveId),    // لغوشده
                Make(5, true, false, TripStatusEnum.TripStatus_Requested, 99),          // رزرو دیگر
            }.AsQueryable();

            var ids = LinkedPetResanTripHelper.PendingBundled(trips, type, reserveId).Select(t => t.Id).ToList();
            Assert.Equal(new[] { 1L }, ids);
        }

        [Fact]
        public void Other_payment_types_have_no_bundled_trip()
        {
            Assert.Null(LinkedPetResanTripHelper.ReserveMatch(PaymentCallbackTypeEnum.Trip, 1));
        }

        // تا رزرو پرداخت نشده، سفرِ همراه به راننده نمایش داده/سپرده نمی‌شود و jobها سراغش نمی‌روند.
        [Fact]
        public void Unpaid_bundled_trip_is_hidden_from_drivers_and_jobs()
        {
            var source = System.IO.File.ReadAllText(Path.Combine(FindBackendRoot(), "Application", "Services", "TripSrv", "TripSrv", "TripService.cs"));
            var guards = source.Split("(!t.PaidWithReserve || t.IsPaid)").Length - 1;
            Assert.True(guards >= 6, $"expected the unpaid-bundled guard in driver accept/list, dispatch, admin notice and auto-cancel; found {guards}");
            Assert.Contains("if (trip.PaidWithReserve && !trip.IsPaid)", source);
            Assert.Contains("if (!trip.PaidWithReserve)\n                await AnnounceLinkedTripAsync(trip);", source.Replace("\r\n", "\n"));
        }

        private static string FindBackendRoot()
        {
            var dir = new DirectoryInfo(System.AppContext.BaseDirectory);
            while (dir != null && !System.IO.File.Exists(Path.Combine(dir.FullName, "Application", "Application.csproj")))
                dir = dir.Parent;
            return dir?.FullName ?? throw new DirectoryNotFoundException("backend root");
        }
    }
}
