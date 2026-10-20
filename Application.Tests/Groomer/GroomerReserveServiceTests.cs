using Application.Services.GroomerSrvs.GroomerReserveSrv;
using Application.Services.GroomerSrvs.GroomerReserveSrv.Dto;
using Entities.Entities;
using Entities.Entities.GroomerField;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static Application.Tests.Groomer.GroomerReserveTestKit;

namespace Application.Tests.Groomer;

public class GroomerReserveServiceTests : IClassFixture<GroomerLocalDbFixture>
{
    private readonly GroomerLocalDbFixture _fixture;

    public GroomerReserveServiceTests(GroomerLocalDbFixture fixture) => _fixture = fixture;

    private async Task<GroomerReserve> LoadAsync(long id)
    {
        using var context = _fixture.NewContext();
        return await context.GroomerReserves.AsNoTracking().FirstAsync(r => r.Id == id);
    }

    [Fact]
    public async Task Create_SnapshotsPricesAndGeneratesCode()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture, pickup: true, pickupFee: 50_000);
        var input = Input(world, Tomorrow(10));
        input.PickupRequested = true;

        var created = await NewReserveService(_fixture).CreateAsync(input, world.BookerId);

        Assert.True(created.IsSuccess);
        var r = created.Data;
        Assert.StartsWith("GRM", r.ReserveCode);
        Assert.Equal(1_050_000, r.Price);
        Assert.Equal(250_000, r.OnlineAmount);
        Assert.Equal(250_000, r.PaymentPrice);
        Assert.Equal(800_000, r.PayAtVenue);
        Assert.Equal(Tomorrow(11), r.EndDate);
        Assert.False(r.IsReserved);
        Assert.Equal((int)GroomerReserveStatusEnum.Registered, r.Status);
        Assert.Equal((int)GroomerReserveOwnerDecisionEnum.NotRequired, r.OwnerDecision);
        Assert.Equal(0, r.ProviderShare);
    }

    [Fact]
    public async Task Create_RejectsPastTooSoonAndForeignPet()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        var service = NewReserveService(_fixture);

        Assert.False((await service.CreateAsync(Input(world, DateTime.Today.AddDays(-1).AddHours(10)), world.BookerId)).IsSuccess);
        Assert.False((await service.CreateAsync(Input(world, DateTime.Now.AddMinutes(20)), world.BookerId)).IsSuccess);
        // پت متعلق به کاربر دیگری است
        Assert.False((await service.CreateAsync(Input(world, Tomorrow(10)), world.Seed.OtherUserId)).IsSuccess);
        var badPackage = Input(world, Tomorrow(10));
        badPackage.GroomerServicePackageId += 999;
        Assert.False((await service.CreateAsync(badPackage, world.BookerId)).IsSuccess);
        // آرایشگاه عوض‌شده (بسته مال آرایشگاه دیگری است)
        var wrongGroomer = Input(world, Tomorrow(10));
        wrongGroomer.GroomerId += 999;
        Assert.False((await service.CreateAsync(wrongGroomer, world.BookerId)).IsSuccess);
    }

    [Fact]
    public async Task Create_ChecksPetSizeAcceptedPetsAndMode()
    {
        if (!_fixture.Available) return;
        var sized = await BuildWorldAsync(_fixture, packageSize: "Large", petSize: "Small");
        Assert.False((await NewReserveService(_fixture).CreateAsync(Input(sized, Tomorrow(10)), sized.BookerId)).IsSuccess);

        var salonOnly = await BuildWorldAsync(_fixture, serviceInSalon: true, serviceAtHome: false);
        var home = Input(salonOnly, Tomorrow(10));
        home.IsHomeService = true;
        Assert.False((await NewReserveService(_fixture).CreateAsync(home, salonOnly.BookerId)).IsSuccess);

        var homeOnly = await BuildWorldAsync(_fixture, serviceInSalon: false, serviceAtHome: true);
        Assert.False((await NewReserveService(_fixture).CreateAsync(Input(homeOnly, Tomorrow(10)), homeOnly.BookerId)).IsSuccess);
        var homeNoAddress = Input(homeOnly, Tomorrow(10));
        homeNoAddress.IsHomeService = true;
        Assert.False((await NewReserveService(_fixture).CreateAsync(homeNoAddress, homeOnly.BookerId)).IsSuccess);

        var noPickup = Input(salonOnly, Tomorrow(10));
        noPickup.PickupRequested = true;
        Assert.False((await NewReserveService(_fixture).CreateAsync(noPickup, salonOnly.BookerId)).IsSuccess);

        // فقط گونه‌ی دیگری پذیرفته می‌شود
        var restricted = await BuildWorldAsync(_fixture);
        using (var context = _fixture.NewContext())
        {
            var otherPet = new Pet { Name = "Cat", Label = "cat" + Guid.NewGuid().ToString("N"), Active = true };
            context.Pets.Add(otherPet);
            await context.SaveChangesAsync();
            context.GroomerPets.Add(new GroomerPet { GroomerId = restricted.GroomerId, PetId = otherPet.Id });
            await context.SaveChangesAsync();
        }
        Assert.False((await NewReserveService(_fixture).CreateAsync(Input(restricted, Tomorrow(10)), restricted.BookerId)).IsSuccess);
    }

    [Fact]
    public async Task HomeService_NeedsOwnAddressInsideZone()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture, serviceInSalon: true, serviceAtHome: true);
        long ownAddress, foreignAddress, outOfZone;
        using (var context = _fixture.NewContext())
        {
            var otherCity = new Entities.Entities.LocationField.City { Name = "Far", StateId = world.Seed.StateId };
            context.Cities.Add(otherCity);
            await context.SaveChangesAsync();
            var a = new Address { UserId = world.BookerId, CityId = world.Seed.CityId, Name = "home" };
            var b = new Address { UserId = world.Seed.OtherUserId, CityId = world.Seed.CityId, Name = "other" };
            var c = new Address { UserId = world.BookerId, CityId = otherCity.Id, Name = "far" };
            context.Addresses.AddRange(a, b, c);
            await context.SaveChangesAsync();
            ownAddress = a.Id;
            foreignAddress = b.Id;
            outOfZone = c.Id;
        }

        GroomerReserveInputDto Home(long address, int hour)
        {
            var dto = Input(world, Tomorrow(hour));
            dto.IsHomeService = true;
            dto.AddressId = address;
            return dto;
        }

        // بدون محدوده‌ی تعریف‌شده: فقط شهر خود آرایشگاه
        Assert.True((await NewReserveService(_fixture).CreateAsync(Home(ownAddress, 9), world.BookerId)).IsSuccess);
        Assert.False((await NewReserveService(_fixture).CreateAsync(Home(foreignAddress, 12), world.BookerId)).IsSuccess);
        Assert.False((await NewReserveService(_fixture).CreateAsync(Home(outOfZone, 12), world.BookerId)).IsSuccess);

        // با محدوده‌ی تعریف‌شده: فقط همان شهرها
        using (var context = _fixture.NewContext())
        {
            context.GroomerZones.Add(new GroomerZone { GroomerId = world.GroomerId, StateId = world.Seed.StateId, CityId = outOfZone == 0 ? 0 : context.Addresses.Where(x => x.Id == outOfZone).Select(x => x.CityId).First() });
            await context.SaveChangesAsync();
        }
        Assert.True((await NewReserveService(_fixture).CreateAsync(Home(outOfZone, 15), world.BookerId)).IsSuccess);
        Assert.False((await NewReserveService(_fixture).CreateAsync(Home(ownAddress, 18), world.BookerId)).IsSuccess);
    }

    [Fact]
    public async Task Create_RespectsCapacityAndHoldsButNotCancelled()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture, capacity: 1);
        var second = new UserPet { Name = "Bella", PetId = world.Seed.PetId, UserId = world.Seed.OtherUserId, Size = "Small" };
        using (var context = _fixture.NewContext())
        {
            context.UserPets.Add(second);
            await context.SaveChangesAsync();
        }

        var first = await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(10)), world.BookerId);
        Assert.True(first.IsSuccess);

        var other = Input(world, Tomorrow(10, 30));
        other.UserPetId = second.Id;
        // نگه‌داشت پرداخت‌نشده‌ی اولی هنوز ظرفیت را می‌گیرد
        var blocked = await NewReserveService(_fixture).CreateAsync(other, world.Seed.OtherUserId);
        Assert.False(blocked.IsSuccess);

        // بعد از لغو، جا آزاد می‌شود
        Assert.True((await NewApprovalService(_fixture).CancelByBookerAsync(first.Data.Id, world.BookerId)).IsSuccess);
        Assert.True((await NewReserveService(_fixture).CreateAsync(other, world.Seed.OtherUserId)).IsSuccess);

        // رزرو پشت‌سرهم (پایان = شروع بعدی) هم‌پوشانی نیست
        var adjacent = Input(world, Tomorrow(11, 30));
        adjacent.UserPetId = world.UserPetId;
        Assert.True((await NewReserveService(_fixture).CreateAsync(adjacent, world.BookerId)).IsSuccess);
    }

    [Fact]
    public async Task Create_ReplacesOwnUnpaidHoldForSamePet()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture, capacity: 1);
        var first = await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(10)), world.BookerId);
        var again = await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(10)), world.BookerId);

        Assert.True(first.IsSuccess);
        Assert.True(again.IsSuccess);
        Assert.NotEqual(first.Data.Id, again.Data.Id);
        Assert.True((await LoadAsync(first.Data.Id)).IsCancel);
        Assert.False((await LoadAsync(again.Data.Id)).IsCancel);
    }

    [Fact]
    public async Task Availability_ShowsCapacityAndUsedSlots()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture, capacity: 2);
        var day = DateTime.Today.AddDays(2);
        var created = await NewReserveService(_fixture).CreateAsync(Input(world, day.AddHours(10)), world.BookerId);
        Assert.True(created.IsSuccess);

        var availability = await NewReserveService(_fixture).GetAvailabilityAsync(world.GroomerId, day, world.PackageId, null);
        Assert.True(availability.IsSuccess);
        Assert.Equal(60, availability.Data.DurationMinutes);
        var at10 = availability.Data.Slots.Single(s => s.Time == "10:00");
        var at0930 = availability.Data.Slots.Single(s => s.Time == "09:30");
        var at11 = availability.Data.Slots.Single(s => s.Time == "11:00");
        Assert.Equal(2, at10.Capacity);
        Assert.Equal(1, at10.Available);
        Assert.Equal(1, at0930.Available); // 9:30-10:30 با نوبت 10-11 هم‌پوشان است
        Assert.Equal(2, at11.Available); // 11:00 درست بعد از پایان نوبت
        Assert.Contains(availability.Data.Slots, s => s.Time == "00:00");
        Assert.DoesNotContain(availability.Data.Slots, s => s.Time == "23:30"); // 23:30 + 60 دقیقه از 23:59 می‌گذرد

        var wrongGroomer = await NewReserveService(_fixture).GetAvailabilityAsync(world.GroomerId + 999, day, world.PackageId, null);
        Assert.False(wrongGroomer.IsSuccess);
    }

    [Fact]
    public async Task Availability_HidesPastSlotsAndAssigneeConflicts()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture, capacity: 3);
        var today = await NewReserveService(_fixture).GetAvailabilityAsync(world.GroomerId, DateTime.Today, world.PackageId, null);
        Assert.All(today.Data.Slots, s => Assert.True(s.StartDate >= DateTime.Now.AddMinutes(59)));

        var day = DateTime.Today.AddDays(3);
        var input = Input(world, day.AddHours(10));
        input.AssigneeUserId = world.Seed.OwnerUserId;
        Assert.True((await NewReserveService(_fixture).CreateAsync(input, world.BookerId)).IsSuccess);
        var withAssignee = await NewReserveService(_fixture).GetAvailabilityAsync(world.GroomerId, day, world.PackageId, world.Seed.OwnerUserId);
        Assert.Equal(0, withAssignee.Data.Slots.Single(s => s.Time == "10:00").Available);
        var anyone = await NewReserveService(_fixture).GetAvailabilityAsync(world.GroomerId, day, world.PackageId, null);
        Assert.Equal(2, anyone.Data.Slots.Single(s => s.Time == "10:00").Available);
    }

    [Fact]
    public async Task Assignee_MustBeOwnerOrStaff()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        var bad = Input(world, Tomorrow(10));
        bad.AssigneeUserId = world.Seed.OtherUserId;
        Assert.False((await NewReserveService(_fixture).CreateAsync(bad, world.BookerId)).IsSuccess);

        using (var context = _fixture.NewContext())
        {
            context.GroomerUsers.Add(new GroomerUser { GroomerId = world.GroomerId, UserId = world.Seed.OtherUserId, Active = true, UserAccept = true });
            await context.SaveChangesAsync();
        }
        Assert.True((await NewReserveService(_fixture).CreateAsync(bad, world.BookerId)).IsSuccess);
    }

    [Fact]
    public async Task Quote_MatchesCreate_AndRejectsUnavailablePickup()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture, pickup: true, pickupFee: 30_000);
        var quote = await NewReserveService(_fixture).QuoteAsync(world.PackageId, true);
        Assert.True(quote.IsSuccess);
        Assert.Equal(1_030_000, quote.Data.Price);
        Assert.Equal(230_000, quote.Data.OnlineAmount);
        Assert.Equal(800_000, quote.Data.PayAtVenue);

        var noPickup = await BuildWorldAsync(_fixture);
        Assert.False((await NewReserveService(_fixture).QuoteAsync(noPickup.PackageId, true)).IsSuccess);
        Assert.False((await NewReserveService(_fixture).QuoteAsync(987654, false)).IsSuccess);
    }

    [Fact]
    public async Task Rebate_AppliesToOnlineAmountOnly_AndClearRestores()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        var created = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(10)), world.BookerId)).Data;

        var service = NewReserveService(_fixture, rebate: NewRebate(finalPrice: 50_000, rebateId: 7));
        Assert.True((await service.SetRebateCodeAsync(created.Id, "CODE", world.BookerId)).IsSuccess);
        var withRebate = await LoadAsync(created.Id);
        Assert.Equal(50_000, withRebate.RebatePrice);
        Assert.Equal(150_000, withRebate.PaymentPrice);
        Assert.Equal(200_000, withRebate.OnlineAmount);
        Assert.Equal(1_000_000, withRebate.Price);

        // تخفیف بیشتر از مبلغ آنلاین: صفر می‌شود نه منفی (و خود تخفیف هم به مبلغ آنلاین بسته می‌شود)
        var huge = NewReserveService(_fixture, rebate: NewRebate(finalPrice: 9_000_000, rebateId: 7));
        Assert.True((await huge.SetRebateCodeAsync(created.Id, "CODE", world.BookerId)).IsSuccess);
        var capped = await LoadAsync(created.Id);
        Assert.Equal(200_000, capped.RebatePrice);
        Assert.Equal(0, capped.PaymentPrice);

        // کد نامعتبر / کاربر دیگر
        var bad = NewReserveService(_fixture, rebate: NewRebate(succeed: false));
        Assert.False((await bad.SetRebateCodeAsync(created.Id, "NOPE", world.BookerId)).IsSuccess);
        Assert.False((await service.SetRebateCodeAsync(created.Id, "CODE", world.Seed.OtherUserId)).IsSuccess);
        Assert.False((await service.SetRebateCodeAsync(created.Id, " ", world.BookerId)).IsSuccess);

        Assert.True((await service.ClearRebateAsync(created.Id, world.BookerId)).IsSuccess);
        var cleared = await LoadAsync(created.Id);
        Assert.Equal(0, cleared.RebatePrice);
        Assert.Equal(200_000, cleared.PaymentPrice);
        Assert.False((await service.ClearRebateAsync(created.Id, world.BookerId)).IsSuccess);
    }

    [Fact]
    public async Task Wallet_FlagFollowsPaymentPrice_AndLocksAfterPayment()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        var created = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(10)), world.BookerId)).Data;
        var service = NewReserveService(_fixture, rebate: NewRebate(finalPrice: 20_000, rebateId: 3));

        Assert.True((await service.SetWalletAsync(created.Id, true, world.BookerId)).IsSuccess);
        Assert.Equal(200_000, (await LoadAsync(created.Id)).WalletPrice);
        // تخفیف بعد از کیف پول: سهم کیف پول با مبلغ جدید هماهنگ می‌شود
        Assert.True((await service.SetRebateCodeAsync(created.Id, "C", world.BookerId)).IsSuccess);
        var after = await LoadAsync(created.Id);
        Assert.Equal(180_000, after.PaymentPrice);
        Assert.Equal(180_000, after.WalletPrice);
        Assert.True((await service.SetWalletAsync(created.Id, false, world.BookerId)).IsSuccess);
        Assert.Equal(0, (await LoadAsync(created.Id)).WalletPrice);
        Assert.False((await service.SetWalletAsync(created.Id, true, world.Seed.OtherUserId)).IsSuccess);

        await PayAsync(_fixture, created.Id);
        var afterPay = NewReserveService(_fixture, rebate: NewRebate(finalPrice: 20_000, rebateId: 3));
        Assert.False((await afterPay.SetWalletAsync(created.Id, true, world.BookerId)).IsSuccess);
        Assert.False((await afterPay.SetRebateCodeAsync(created.Id, "C", world.BookerId)).IsSuccess);
    }

    [Fact]
    public async Task PaymentCallback_MarksPaid_SetsApprovalDeadline_AndSplitsCommission()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        var created = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(10)), world.BookerId)).Data;
        var (push, pushProxy) = NewPush();

        var before = DateTime.Now;
        var result = await NewReserveService(_fixture, push: push).GroomerReservePaymentCallback(created.Id);
        Assert.True(result.IsSuccess);

        var paid = await LoadAsync(created.Id);
        Assert.True(paid.IsReserved);
        Assert.Equal((int)GroomerReserveStatusEnum.Paid, paid.Status);
        Assert.Equal((int)GroomerReserveOwnerDecisionEnum.Pending, paid.OwnerDecision);
        Assert.NotNull(paid.PaidDate);
        Assert.True(paid.OwnerApprovalDeadline >= before.AddMinutes(29));
        Assert.True(paid.OwnerApprovalDeadline <= before.AddHours(12).AddMinutes(1));
        Assert.Equal(10m, paid.CommissionPercent); // درصد خود خدمت
        Assert.Equal(180_000, paid.ProviderShare);
        Assert.Equal(20_000, paid.SiteShare);
        Assert.Contains("ReservePaidAsync", pushProxy.Calls);

        // تماس دوباره‌ی callback (تلاش مجدد درگاه) بی‌اثر و موفق است
        Assert.True((await NewReserveService(_fixture).GroomerReservePaymentCallback(created.Id)).IsSuccess);
        var again = await LoadAsync(created.Id);
        Assert.Equal(paid.PaidDate, again.PaidDate);
        Assert.Equal(paid.OwnerApprovalDeadline, again.OwnerApprovalDeadline);
    }

    [Fact]
    public async Task PaymentCallback_CommissionFallsBackToGroomerDefault_AndUsesRebateAdjustedAmount()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        using (var context = _fixture.NewContext())
        {
            await context.GroomerServices.Where(s => s.Id == world.ServiceId).ExecuteUpdateAsync(s => s.SetProperty(x => x.CommissionPercent, 0m));
            await context.Groomers.Where(g => g.Id == world.GroomerId).ExecuteUpdateAsync(s => s.SetProperty(x => x.CommissionPercent, 25m));
        }
        var created = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(10)), world.BookerId)).Data;
        Assert.True((await NewReserveService(_fixture, rebate: NewRebate(finalPrice: 40_000, rebateId: 9)).SetRebateCodeAsync(created.Id, "C", world.BookerId)).IsSuccess);

        var paid = await PayAsync(_fixture, created.Id);
        Assert.Equal(160_000, paid.PaymentPrice);
        Assert.Equal(25m, paid.CommissionPercent);
        Assert.Equal(120_000, paid.ProviderShare);
        Assert.Equal(40_000, paid.SiteShare);
    }

    [Fact]
    public async Task PaymentCallback_FailsWhenSlotTakenInMeantime_PastOrCancelled()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture, capacity: 1);
        var second = new UserPet { Name = "Bella", PetId = world.Seed.PetId, UserId = world.Seed.OtherUserId, Size = "Small" };
        using (var context = _fixture.NewContext())
        {
            context.UserPets.Add(second);
            await context.SaveChangesAsync();
        }
        var a = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(10)), world.BookerId)).Data;

        // نگه‌داشت a را رها می‌کنیم (قدیمی می‌شود) تا کاربر دیگر بتواند همان زمان را بگیرد
        using (var context = _fixture.NewContext())
            await context.GroomerReserves.Where(r => r.Id == a.Id).ExecuteUpdateAsync(s => s.SetProperty(r => r.CreateDate, DateTime.Now.AddHours(-2)));
        var other = Input(world, Tomorrow(10));
        other.UserPetId = second.Id;
        var b = (await NewReserveService(_fixture).CreateAsync(other, world.Seed.OtherUserId)).Data;
        await PayAsync(_fixture, b.Id);

        var lateA = await NewReserveService(_fixture).GroomerReservePaymentCallback(a.Id);
        Assert.False(lateA.IsSuccess);
        Assert.False((await LoadAsync(a.Id)).IsReserved);

        // زمان نوبت گذشته
        var c = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(14)), world.BookerId)).Data;
        using (var context = _fixture.NewContext())
            await context.GroomerReserves.Where(r => r.Id == c.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.DoDate, DateTime.Now.AddHours(-3)).SetProperty(r => r.EndDate, DateTime.Now.AddHours(-2)));
        Assert.False((await NewReserveService(_fixture).GroomerReservePaymentCallback(c.Id)).IsSuccess);

        // رزرو لغوشده
        var d = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(16)), world.BookerId)).Data;
        await NewApprovalService(_fixture).CancelByBookerAsync(d.Id, world.BookerId);
        Assert.False((await NewReserveService(_fixture).GroomerReservePaymentCallback(d.Id)).IsSuccess);
        Assert.False((await NewReserveService(_fixture).GroomerReservePaymentCallback(987654)).IsSuccess);
    }

    [Fact]
    public async Task PaymentCallback_FailsWhenGroomerBecameUnavailable()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        var created = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(10)), world.BookerId)).Data;
        using (var context = _fixture.NewContext())
            await context.Groomers.Where(g => g.Id == world.GroomerId).ExecuteUpdateAsync(s => s.SetProperty(g => g.Active, false));

        Assert.False((await NewReserveService(_fixture).GroomerReservePaymentCallback(created.Id)).IsSuccess);
        Assert.False((await LoadAsync(created.Id)).IsReserved);
    }

    [Fact]
    public async Task PaymentCallback_WalletDebitFailureBlocksPayment()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        var created = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(10)), world.BookerId)).Data;
        await NewReserveService(_fixture).SetWalletAsync(created.Id, true, world.BookerId);

        var failing = NewReserveService(_fixture, wallet: NewWallet(_fixture, succeed: false).Wallet);
        Assert.False((await failing.GroomerReservePaymentCallback(created.Id, fromWallet: true)).IsSuccess);
        Assert.False((await LoadAsync(created.Id)).IsReserved);

        var (wallet, proxy) = NewWallet(_fixture);
        Assert.True((await NewReserveService(_fixture, wallet: wallet).GroomerReservePaymentCallback(created.Id, fromWallet: true)).IsSuccess);
        Assert.Contains("InsertUpdateGroomerReserveAsync", proxy.Calls);
        Assert.True((await LoadAsync(created.Id)).IsReserved);
    }

    [Fact]
    public async Task Complete_NeedsApprovalAndStartedAppointment()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        var created = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(10)), world.BookerId)).Data;
        var service = NewReserveService(_fixture);

        Assert.False((await NewReserveService(_fixture).CompleteAsync(created.Id, world.Seed.OwnerUserId)).IsSuccess); // پرداخت نشده
        await PayAsync(_fixture, created.Id);
        Assert.False((await NewReserveService(_fixture).CompleteAsync(created.Id, world.Seed.OwnerUserId)).IsSuccess); // منتظر تأیید
        Assert.True((await NewApprovalService(_fixture).ApproveAsync(created.Id, world.Seed.OwnerUserId)).IsSuccess);
        Assert.False((await NewReserveService(_fixture).CompleteAsync(created.Id, world.Seed.OwnerUserId)).IsSuccess); // هنوز زمان نوبت نرسیده

        using (var context = _fixture.NewContext())
            await context.GroomerReserves.Where(r => r.Id == created.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.DoDate, DateTime.Now.AddHours(-2)).SetProperty(r => r.EndDate, DateTime.Now.AddHours(-1)));
        Assert.False((await NewReserveService(_fixture).CompleteAsync(created.Id, world.Seed.OtherUserId)).IsSuccess); // مالک نیست
        Assert.True((await NewReserveService(_fixture).CompleteAsync(created.Id, world.Seed.OwnerUserId)).IsSuccess);
        var done = await LoadAsync(created.Id);
        Assert.Equal((int)GroomerReserveStatusEnum.Complete, done.Status);
        Assert.NotNull(done.DoneDate);
        Assert.False((await NewReserveService(_fixture).CompleteAsync(created.Id, null)).IsSuccess); // تکراری
    }

    [Fact]
    public async Task SearchAndFind_ScopeByBookerAndOwner_AndHideAccountingFromBooker()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        var created = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(10)), world.BookerId)).Data;
        await PayAsync(_fixture, created.Id);
        var service = NewReserveService(_fixture);

        Assert.Single(service.Search(new GroomerReserveSearchInputDto { BookerId = world.BookerId }).List);
        Assert.Empty(service.Search(new GroomerReserveSearchInputDto { BookerId = world.Seed.OtherUserId }).List);
        Assert.Single(service.Search(new GroomerReserveSearchInputDto { OwnerUserId = world.Seed.OwnerUserId, OnlyPaid = true }).List);
        Assert.Empty(service.Search(new GroomerReserveSearchInputDto { OwnerUserId = world.Seed.OtherUserId }).List);
        Assert.Single(service.Search(new GroomerReserveSearchInputDto { Q = created.ReserveCode, GroomerId = world.GroomerId }).List);

        var forBooker = await service.FindAsync(created.Id, world.BookerId, null);
        Assert.True(forBooker.IsSuccess);
        Assert.Equal(0, forBooker.Data.SiteShare);
        Assert.Equal(0, forBooker.Data.ProviderShare);
        Assert.True(forBooker.Data.FreeCancelAvailable);
        var forOwner = await service.FindAsync(created.Id, null, world.Seed.OwnerUserId);
        Assert.Equal(20_000, forOwner.Data.SiteShare);
        Assert.Equal("Rex", forOwner.Data.PetName);
        Assert.False((await service.FindAsync(created.Id, world.Seed.OtherUserId, null)).IsSuccess);
        Assert.False((await service.FindAsync(created.Id, null, world.Seed.OtherUserId)).IsSuccess);
    }
}
