using Application.Services.ClinicSrvs.ClinicReserveSrv;
using Application.Services.TripSrv.TripSrv.Iface;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static Application.Tests.Clinic.ClinicReserveTestKit;

namespace Application.Tests.Clinic;

public class ClinicTripLinkTests : IClassFixture<ClinicLocalDbFixture>
{
    private readonly ClinicLocalDbFixture _fixture;

    public ClinicTripLinkTests(ClinicLocalDbFixture fixture) => _fixture = fixture;

    private async Task<long> AddLinkedTripAsync(long reserveId, long userId, int lead = 60)
    {
        using var context = _fixture.NewContext();
        var trip = new Trip
        {
            UserId = userId,
            ClinicReserveId = reserveId,
            IsOnline = true,
            CreateDate = DateTime.Now,
            TripStatusId = (long)Application.Common.Enumerable.Code.TripStatusEnum.TripStatus_Requested,
            DriverStatusId = (long)Application.Common.Enumerable.Code.DriverStatusEnum.DriverStatus_Requested,
            ScheduledLeadMinutes = lead,
            ScheduledDepartureAt = DateTime.Now.AddHours(20),
            ScheduledDispatched = true,
        };
        context.Trips.Add(trip);
        await context.SaveChangesAsync();
        return trip.Id;
    }

    [Fact]
    public async Task Reschedule_MovesLinkedTripDeparture_AndNotifiesTheDriver()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        var created = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(10)), world.BookerId)).Data;
        var paid = await PayAsync(_fixture, created.Id);
        var tripId = await AddLinkedTripAsync(paid.Id, world.BookerId, 120);

        var (notifier, proxy) = NewPush();
        var moved = await NewReserveService(_fixture, push: notifier).RescheduleAsync(paid.Id, Tomorrow(16), world.Seed.OwnerUserId);
        Assert.True(moved.IsSuccess);

        using var context = _fixture.NewContext();
        var trip = await context.Trips.AsNoTracking().FirstAsync(t => t.Id == tripId);
        Assert.Equal(Tomorrow(14), trip.ScheduledDepartureAt);   // ۱۶:۰۰ منهای ۱۲۰ دقیقه
        Assert.Equal(Tomorrow(14), trip.TripStartDateTime);
        Assert.False(trip.ScheduledDispatched);                  // راننده ندارد → دوباره به صف اعزام
        Assert.Contains("TripRescheduledForDriverAsync", proxy.Calls);
    }

    [Fact]
    public async Task EveryCancelPath_CancelsTheLinkedTrip()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);

        async Task<long> PaidAsync(int hour)
        {
            var created = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(hour)), world.BookerId)).Data;
            return (await PayAsync(_fixture, created.Id)).Id;
        }

        var viaReject = await PaidAsync(9);
        var rejectTrip = FakeProxy<ITripService>.Create();
        Assert.True((await NewApprovalService(_fixture, trip: rejectTrip.Instance).RejectAsync(viaReject, "busy", world.Seed.OwnerUserId)).IsSuccess);
        Assert.Contains("CancelLinkedTripForClinicReserveAsync", rejectTrip.Proxy.Calls);

        var viaBooker = await PaidAsync(11);
        var bookerTrip = FakeProxy<ITripService>.Create();
        Assert.True((await NewApprovalService(_fixture, trip: bookerTrip.Instance).CancelByBookerAsync(viaBooker, world.BookerId)).IsSuccess);
        Assert.Contains("CancelLinkedTripForClinicReserveAsync", bookerTrip.Proxy.Calls);

        var viaAdmin = await PaidAsync(13);
        var adminTrip = FakeProxy<ITripService>.Create();
        Assert.True((await NewApprovalService(_fixture, trip: adminTrip.Instance).CancelByAdminAsync(viaAdmin, "support", true)).IsSuccess);
        Assert.Contains("CancelLinkedTripForClinicReserveAsync", adminTrip.Proxy.Calls);
    }
}
