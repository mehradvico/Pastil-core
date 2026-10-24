using Application.Common.Enumerable;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.CompanionSrvs.ConsultationOnlineSrv;
using Application.Services.ConsultationSrvs.ConsultationAdminSrv;
using Application.Services.ConsultationSrvs.ConsultationBookingSrv;
using Application.Services.ConsultationSrvs.ConsultationBookingSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationNotificationSrv.Iface;
using Application.Services.ConsultationSrvs.ConsultationOwnerSrv;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationSessionSrv;
using Application.Services.ConsultationSrvs.ConsultationVenueSrv;
using Application.Services.FinanceSrvs.FinanceCompanionSrv.Iface;
using Application.Services.FinanceSrvs.FinanceStoreSrv.Iface;
using Application.Services.FinanceSrvs.SettlementSrv;
using Application.Services.FinanceSrvs.SettlementSrv.Dto;
using Application.Services.FinanceSrvs.UserBankCardSrv.Iface;
using Application.Services.ProviderSrvs.ProviderFinanceSrv;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Application.Tests.Coach;

/// <summary>
/// مشاوره‌ی آنلاین فوری مراکز مستقل (کلینیک/مربی): بسته، «آنلاینم»، برنامه‌ی رزرو، صف نماینده و تسویه
/// مستقیماً روی ClinicId / CoachId کلید می‌خورند؛ هیچ Companion «لنگری» ساخته نمی‌شود.
/// </summary>
public class VenueConsultationTests : IClassFixture<CoachLocalDbFixture>
{
    private readonly CoachLocalDbFixture _fixture;

    public VenueConsultationTests(CoachLocalDbFixture fixture) => _fixture = fixture;

    private ConsultationOnlineService NewOnline() => new(_fixture.NewContext());

    private VenueConsultationService NewService() => new(
        _fixture.NewContext(),
        new ConsultationPackageService(_fixture.NewContext(), NewOnline()),
        NewOnline(),
        new ConsultationBookingService(_fixture.NewContext()));

    private ConsultationSessionService NewSessions() => new(
        _fixture.NewContext(),
        FakeProxy<IPushNotificationService>.Create().Instance,
        FakeProxy<IConsultationNotificationService>.Create().Instance);

    private SettlementService NewSettlement() => new(_fixture.NewContext(), _fixture.Mapper);

    private ProviderFinanceService NewFinance() => new(
        _fixture.NewContext(),
        FakeProxy<IFinanceCompanionService>.Create().Instance,
        FakeProxy<IFinanceStoreService>.Create().Instance,
        FakeProxy<IUserBankCardService>.Create((method, _) =>
            method.Name == nameof(IUserBankCardService.Search)
                ? System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(method.ReturnType)
                : null).Instance);

    private async Task<long> AddCoachAsync(CoachLocalDbFixture.SeedData seed, long? ownerId = null, bool approved = true)
    {
        using var context = _fixture.NewContext();
        var coach = new Entities.Entities.CoachField.Coach
        {
            Name = "Coach", ProviderId = seed.ProviderId, OwnerId = ownerId ?? seed.OwnerUserId, Phone = "021", AddressValue = "a",
            StateId = seed.StateId, CityId = seed.CityId, Active = true, Approved = approved,
        };
        context.Coaches.Add(coach);
        await context.SaveChangesAsync();
        return coach.Id;
    }

    private async Task<long> AddClinicAsync(CoachLocalDbFixture.SeedData seed)
    {
        using var context = _fixture.NewContext();
        var clinic = new Entities.Entities.ClinicField.Clinic
        {
            Name = "Clinic", ProviderId = seed.ProviderId, OwnerId = seed.OwnerUserId, Phone = "021", AddressValue = "a",
            StateId = seed.StateId, CityId = seed.CityId, Active = true, Approved = true,
        };
        context.Clinics.Add(clinic);
        await context.SaveChangesAsync();
        return clinic.Id;
    }

    private async Task AddCoachMemberAsync(long coachId, long userId)
    {
        using var context = _fixture.NewContext();
        context.CoachUsers.Add(new Entities.Entities.CoachField.CoachUser { CoachId = coachId, UserId = userId, Active = true, UserAccept = true });
        await context.SaveChangesAsync();
    }

    private static ConsultationPackageItemDto Package(int channel, double price, string name) =>
        new() { ChannelId = channel, DurationMinutes = 30, Price = price, Active = true, Name = name };

    /// <summary>خرید مشاوره‌ی پرداخت‌شده/تکمیل‌شده با مالک مرکز (بدون گذر از پرداخت).</summary>
    private async Task<long> AddPurchaseAsync(ConsultationOwner owner, long userId, ConsultationPurchaseStatusEnum status,
        double paymentPrice = 100_000, double providerShare = 90_000, double siteShare = 10_000, bool permitted = false)
    {
        using var context = _fixture.NewContext();
        var purchase = new ConsultationPurchase
        {
            PurchaseCode = "T" + Guid.NewGuid().ToString("N")[..12], UserId = userId, ConsultationPackageId = 1,
            ChannelId = 1, DurationMinutes = 30, PackageName = "Chat", Price = paymentPrice, PaymentPrice = paymentPrice,
            CompanionShare = providerShare, SiteShare = siteShare, Status = (int)status, Permitted = permitted,
            PaidDate = DateTime.Now, CreateDate = DateTime.Now,
        };
        purchase.Assign(owner);
        context.ConsultationPurchases.Add(purchase);
        await context.SaveChangesAsync();
        return purchase.Id;
    }

    [Fact]
    public async Task Packages_AreKeyedOnTheVenueItself_AndNoCompanionIsCreated()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var service = NewService();
        var coachId = await AddCoachAsync(seed);
        var clinicId = await AddClinicAsync(seed);
        int companionsBefore;
        using (var ctx = _fixture.NewContext())
            companionsBefore = await ctx.Companions.CountAsync();

        Assert.Empty((await service.GetPublicAsync(VenueKind.Coach, coachId)).Data);

        // غریبه نمی‌تواند بسازد
        Assert.False((await service.CreateAsync(VenueKind.Coach, coachId, seed.OtherUserId, Package(1, 150_000, "Chat"))).IsSuccess);

        Assert.True((await service.CreateAsync(VenueKind.Coach, coachId, seed.OwnerUserId, Package(1, 150_000, "Chat"))).IsSuccess);
        Assert.True((await service.CreateAsync(VenueKind.Coach, coachId, seed.OwnerUserId, Package(2, 200_000, "Call"))).IsSuccess);
        Assert.True((await service.CreateAsync(VenueKind.Clinic, clinicId, seed.OwnerUserId, Package(1, 90_000, "Chat"))).IsSuccess);
        // نام تکراری زیر همان کانال همان مرکز رد می‌شود، ولی مرکز دیگر می‌تواند همان نام را داشته باشد
        Assert.False((await service.CreateAsync(VenueKind.Coach, coachId, seed.OwnerUserId, Package(1, 1_000, "chat"))).IsSuccess);

        using var context = _fixture.NewContext();
        var rows = await context.ConsultationPackages.AsNoTracking().Where(p => p.CoachId == coachId || p.ClinicId == clinicId).ToListAsync();
        Assert.Equal(3, rows.Count);
        Assert.All(rows, p => Assert.Null(p.CompanionId));
        Assert.Equal(2, rows.Count(p => p.CoachId == coachId && p.ClinicId == null));
        Assert.Single(rows, p => p.ClinicId == clinicId && p.CoachId == null);
        Assert.Equal(companionsBefore, await context.Companions.CountAsync()); // لنگر ساخته نشد

        // فهرست هر مرکز فقط مال خودش است
        Assert.Equal(2, (await service.GetListAsync(VenueKind.Coach, coachId, seed.OwnerUserId)).Data.Count);
        Assert.Single((await service.GetListAsync(VenueKind.Clinic, clinicId, seed.OwnerUserId)).Data);
        Assert.Equal(2, (await service.GetPublicAsync(VenueKind.Coach, coachId)).Data.Count);
        var publicRow = (await service.GetPublicAsync(VenueKind.Clinic, clinicId)).Data.Single();
        Assert.Equal(clinicId, publicRow.ClinicId);
        Assert.Null(publicRow.CompanionId);
        Assert.Null(publicRow.CoachId);

        // حذف فقط پکیج همان مرکز را برمی‌دارد
        var firstId = (await service.GetListAsync(VenueKind.Coach, coachId, seed.OwnerUserId)).Data.First().Id;
        Assert.False((await service.DeleteAsync(VenueKind.Clinic, clinicId, seed.OwnerUserId, firstId)).IsSuccess);
        Assert.True((await service.DeleteAsync(VenueKind.Coach, coachId, seed.OwnerUserId, firstId)).IsSuccess);
        Assert.Single((await service.GetListAsync(VenueKind.Coach, coachId, seed.OwnerUserId)).Data);
    }

    [Fact]
    public async Task Packages_RequireAnApprovedActiveVenue_ToWrite()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var service = NewService();
        var coachId = await AddCoachAsync(seed, approved: false);

        Assert.False((await service.CreateAsync(VenueKind.Coach, coachId, seed.OwnerUserId, Package(1, 150_000, "Chat"))).IsSuccess);
        Assert.Empty((await service.GetPublicAsync(VenueKind.Coach, coachId)).Data);
        Assert.True((await service.GetListAsync(VenueKind.Coach, coachId, seed.OwnerUserId)).IsSuccess); // خواندن برای مالک آزاد است
    }

    [Fact]
    public async Task Online_UsesTheVenueTeamTable_AndPublicStatusCountsTeam()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var service = NewService();
        var coachId = await AddCoachAsync(seed);
        var clinicId = await AddClinicAsync(seed);
        await AddCoachMemberAsync(coachId, seed.LikerUserId);

        // غریبه رد می‌شود
        Assert.False((await service.SetOnlineAsync(VenueKind.Coach, coachId, seed.OtherUserId, true)).IsSuccess);

        // مالک بدون ردیف عضویت: ردیف خودمالکی در CoachUsers ساخته می‌شود
        var ownerOn = await service.SetOnlineAsync(VenueKind.Coach, coachId, seed.OwnerUserId, true);
        Assert.True(ownerOn.IsSuccess);
        Assert.True(ownerOn.Data.Online);
        Assert.True((await service.GetOnlineAsync(VenueKind.Coach, coachId, seed.OwnerUserId)).Data.Online);

        // عضو تیم هم برای خودش روشن می‌کند
        Assert.True((await service.SetOnlineAsync(VenueKind.Coach, coachId, seed.LikerUserId, true)).Data.Online);
        var publicStatus = await NewOnline().GetPublicAsync(ConsultationOwner.Coach(coachId));
        Assert.True(publicStatus.Online);
        Assert.Equal(2, publicStatus.OnlineStaffCount);

        // خاموش کردن مالک؛ هنوز یک نفر آنلاین است
        Assert.False((await service.SetOnlineAsync(VenueKind.Coach, coachId, seed.OwnerUserId, false)).Data.Online);
        Assert.Equal(1, (await NewOnline().GetPublicAsync(ConsultationOwner.Coach(coachId))).OnlineStaffCount);

        // کلینیک و نمایندگی قدیمی از آنلاین‌بودن مربی بی‌خبرند
        Assert.False((await NewOnline().GetPublicAsync(ConsultationOwner.Clinic(clinicId))).Online);
        Assert.False((await NewOnline().GetPublicAsync(ConsultationOwner.Companion(coachId))).Online);

        // پکیج عمومی مربی چراغ آنلاین را از همین جدول می‌گیرد
        await service.CreateAsync(VenueKind.Coach, coachId, seed.OwnerUserId, Package(1, 150_000, "Chat"));
        Assert.True((await service.GetPublicAsync(VenueKind.Coach, coachId)).Data.Single().CompanionOnline);
    }

    [Fact]
    public async Task Availability_IsPerVenue()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var service = NewService();
        var coachId = await AddCoachAsync(seed);
        var clinicId = await AddClinicAsync(seed);

        Assert.False((await service.SaveAvailabilityAsync(VenueKind.Coach, coachId, seed.OtherUserId, new List<ConsultationAvailabilityWindowDto>())).IsSuccess);

        var saved = await service.SaveAvailabilityAsync(VenueKind.Coach, coachId, seed.OwnerUserId, new List<ConsultationAvailabilityWindowDto>
        {
            new() { WeekDayId = 1, Start = "09:00", End = "12:00", Capacity = 2 },
            new() { WeekDayId = 3, Start = "14:00", End = "18:00", Capacity = 1 },
        });
        Assert.True(saved.IsSuccess);
        Assert.Equal(2, saved.Data.Count);

        Assert.Empty((await service.GetAvailabilityAsync(VenueKind.Clinic, clinicId, seed.OwnerUserId)).Data);
        // جایگزینی کامل فقط برنامه‌ی همان مرکز را عوض می‌کند
        await service.SaveAvailabilityAsync(VenueKind.Clinic, clinicId, seed.OwnerUserId, new List<ConsultationAvailabilityWindowDto>
        {
            new() { WeekDayId = 2, Start = "10:00", End = "11:00", Capacity = 1 },
        });
        var replaced = await service.SaveAvailabilityAsync(VenueKind.Coach, coachId, seed.OwnerUserId, new List<ConsultationAvailabilityWindowDto>
        {
            new() { WeekDayId = 5, Start = "08:00", End = "09:00", Capacity = 1 },
        });
        Assert.Single(replaced.Data);
        Assert.Single((await service.GetAvailabilityAsync(VenueKind.Clinic, clinicId, seed.OwnerUserId)).Data);

        using var context = _fixture.NewContext();
        var rows = await context.ConsultationAvailabilities.AsNoTracking().Where(a => !a.Deleted && (a.CoachId == coachId || a.ClinicId == clinicId)).ToListAsync();
        Assert.All(rows, a => Assert.Null(a.CompanionId));
    }

    [Fact]
    public async Task AgentQueue_ShowsOnlyThePurchasesOfTheAgentsVenues()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var coachId = await AddCoachAsync(seed);
        var otherCoachId = await AddCoachAsync(seed, ownerId: seed.OtherUserId);
        var clinicId = await AddClinicAsync(seed);
        await AddCoachMemberAsync(coachId, seed.LikerUserId);

        var coachPurchase = await AddPurchaseAsync(ConsultationOwner.Coach(coachId), seed.OtherUserId, ConsultationPurchaseStatusEnum.Paid);
        var clinicPurchase = await AddPurchaseAsync(ConsultationOwner.Clinic(clinicId), seed.OtherUserId, ConsultationPurchaseStatusEnum.Paid);
        var foreignPurchase = await AddPurchaseAsync(ConsultationOwner.Coach(otherCoachId), seed.LikerUserId, ConsultationPurchaseStatusEnum.Paid);

        // مالک: مربی + کلینیک خودش را می‌بیند (نه مربی دیگران)، و می‌تواند تخصیص دهد
        var owner = (await NewSessions().GetForAgentAsync(seed.OwnerUserId)).Data;
        Assert.Equal(new[] { coachPurchase, clinicPurchase }.OrderBy(x => x), owner.Select(x => x.Id).OrderBy(x => x));
        var coachRow = owner.Single(x => x.Id == coachPurchase);
        Assert.Equal(coachId, coachRow.CoachId);
        Assert.Null(coachRow.CompanionId);
        Assert.Equal("Coach", coachRow.CompanionName);
        Assert.True(coachRow.CanAssign);
        Assert.Contains(coachRow.AssignableAgents, a => a.UserId == seed.LikerUserId);

        // عضو تیم مربی: فقط صف مربی؛ نمی‌تواند تخصیص دهد
        var member = (await NewSessions().GetForAgentAsync(seed.LikerUserId)).Data;
        Assert.Contains(member, x => x.Id == coachPurchase);
        Assert.DoesNotContain(member, x => x.Id == clinicPurchase);
        Assert.False(member.Single(x => x.Id == coachPurchase).CanAssign);
        Assert.False((await NewSessions().AssignAsync(seed.LikerUserId, coachPurchase, seed.LikerUserId)).Data);

        // تخصیص توسط مالک فقط به اعضای همان مرکز
        Assert.False((await NewSessions().AssignAsync(seed.OwnerUserId, coachPurchase, seed.OtherUserId)).Data);
        Assert.True((await NewSessions().AssignAsync(seed.OwnerUserId, coachPurchase, seed.LikerUserId)).Data);

        // نماینده‌ی مرکز دیگر خرید این مربی را نمی‌تواند شروع کند
        Assert.False((await NewSessions().StartAsync(seed.OtherUserId, coachPurchase)).IsSuccess);
        Assert.False((await NewSessions().StartAsync(seed.LikerUserId, foreignPurchase)).IsSuccess);
        Assert.Contains(foreignPurchase, (await NewSessions().GetForAgentAsync(seed.OtherUserId)).Data.Select(x => x.Id));
    }

    [Fact]
    public async Task Settlement_PaysCompletedConsultationsOfEachVenueKind_Once()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var coachId = await AddCoachAsync(seed);
        var clinicId = await AddClinicAsync(seed);
        using (var ctx = _fixture.NewContext())
        {
            ctx.UserBankCards.Add(new UserBankCard { UserId = seed.OwnerUserId, Approved = true });
            await ctx.SaveChangesAsync();
        }
        long cardId;
        using (var ctx = _fixture.NewContext())
            cardId = await ctx.UserBankCards.Where(c => c.UserId == seed.OwnerUserId).Select(c => c.Id).FirstAsync();

        var coachDone1 = await AddPurchaseAsync(ConsultationOwner.Coach(coachId), seed.OtherUserId, ConsultationPurchaseStatusEnum.Completed, 100_000, 90_000, 10_000);
        var coachDone2 = await AddPurchaseAsync(ConsultationOwner.Coach(coachId), seed.OtherUserId, ConsultationPurchaseStatusEnum.Completed, 200_000, 180_000, 20_000);
        var coachPaidOnly = await AddPurchaseAsync(ConsultationOwner.Coach(coachId), seed.OtherUserId, ConsultationPurchaseStatusEnum.Paid);
        var coachCancelled = await AddPurchaseAsync(ConsultationOwner.Coach(coachId), seed.OtherUserId, ConsultationPurchaseStatusEnum.Cancelled);
        var clinicDone = await AddPurchaseAsync(ConsultationOwner.Clinic(clinicId), seed.OtherUserId, ConsultationPurchaseStatusEnum.Completed, 50_000, 40_000, 10_000);

        // قبل از تسویه: لیست در انتظار و جمع‌های مالی مربی دو خرید تکمیل‌شده را می‌بینند
        var pending = (await NewFinance().GetPendingAsync(seed.ProviderId, SettlementKindEnum.Coach)).Data;
        Assert.Equal(2, pending.Items.Count(i => i.Type == "Consultation"));
        Assert.Equal(270_000, pending.ProviderShare);

        var coachSettle = await NewSettlement().InsertAsyncDto(new SettlementDto { ProviderId = seed.ProviderId, Kind = SettlementKindEnum.Coach, UserBankCardId = cardId });
        Assert.True(coachSettle.IsSuccess, string.Join(",", coachSettle.Messages?.Select(m => m.Item1) ?? Array.Empty<string>()));

        using (var ctx = _fixture.NewContext())
        {
            var saved = await ctx.Settlements.AsNoTracking().FirstAsync(s => s.Id == coachSettle.Data.Id);
            Assert.Equal(2, saved.ItemCount);
            Assert.Equal(270_000, saved.PaidPrice);
            var permitted = await ctx.ConsultationPurchases.AsNoTracking()
                .Where(p => p.CoachId == coachId || p.ClinicId == clinicId).ToDictionaryAsync(p => p.Id, p => p.Permitted);
            Assert.True(permitted[coachDone1]);
            Assert.True(permitted[coachDone2]);
            Assert.False(permitted[coachPaidOnly]);
            Assert.False(permitted[coachCancelled]);
            Assert.False(permitted[clinicDone]); // استخر کلینیک جداست
            Assert.Equal(2, await ctx.SettlementConsultations.CountAsync(l => l.SettlementId == coachSettle.Data.Id));
        }

        // بار دوم برای مربی چیزی نمانده؛ کلینیک خرید خودش را جدا تسویه می‌کند
        Assert.False((await NewSettlement().InsertAsyncDto(new SettlementDto { ProviderId = seed.ProviderId, Kind = SettlementKindEnum.Coach, UserBankCardId = cardId })).IsSuccess);
        var clinicSettle = await NewSettlement().InsertAsyncDto(new SettlementDto { ProviderId = seed.ProviderId, Kind = SettlementKindEnum.Clinic, UserBankCardId = cardId });
        Assert.True(clinicSettle.IsSuccess);
        using (var ctx = _fixture.NewContext())
        {
            var saved = await ctx.Settlements.AsNoTracking().FirstAsync(s => s.Id == clinicSettle.Data.Id);
            Assert.Equal(1, saved.ItemCount);
            Assert.Equal(40_000, saved.PaidPrice);
            Assert.True(await ctx.ConsultationPurchases.AsNoTracking().Where(p => p.Id == clinicDone).Select(p => p.Permitted).FirstAsync());
        }

        // جزئیات تسویه ردیف مشاوره را با نام مرکز نشان می‌دهد
        var items = (await NewSettlement().GetItemsAsync(coachSettle.Data.Id)).Data;
        Assert.Equal(2, items.Count(i => i.Type == "Consultation" && i.VenueName == "Coach"));
    }

    [Fact]
    public async Task Admin_SeesVenuePackagesAndPurchaseCounts_AndCanDelete()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var service = NewService();
        var coachId = await AddCoachAsync(seed);
        var clinicId = await AddClinicAsync(seed);
        await service.CreateAsync(VenueKind.Coach, coachId, seed.OwnerUserId, Package(1, 150_000, "Chat"));
        await service.CreateAsync(VenueKind.Coach, coachId, seed.OwnerUserId, Package(2, 250_000, "Call"));
        await service.CreateAsync(VenueKind.Clinic, clinicId, seed.OwnerUserId, Package(1, 90_000, "Chat"));
        await AddPurchaseAsync(ConsultationOwner.Coach(coachId), seed.OtherUserId, ConsultationPurchaseStatusEnum.Paid);

        var admin = new ConsultationAdminService(_fixture.NewContext(), new ConsultationPackageService(_fixture.NewContext(), NewOnline()));
        Assert.False((await admin.GetVenuesAsync("shop", null)).IsSuccess);

        var coach = (await admin.GetVenuesAsync("coach", "Coach")).Data.Single(v => v.VenueId == coachId);
        Assert.Equal(2, coach.ActivePackages);
        Assert.Equal(150_000, coach.MinPrice);
        Assert.Equal(250_000, coach.MaxPrice);
        Assert.Equal(1, coach.PurchasesCount);
        Assert.DoesNotContain((await admin.GetVenuesAsync("clinic", null)).Data, v => v.VenueId == coachId && v.Kind == "coach");

        var packages = (await admin.GetVenuePackagesAsync("coach", coachId)).Data;
        Assert.Equal(2, packages.Count);
        Assert.All(packages, p => Assert.Equal(coachId, p.VenueId));

        // حذف فقط با کلید همان مرکز کار می‌کند
        Assert.False((await admin.DeleteVenuePackageAsync("clinic", clinicId, packages[0].Id)).IsSuccess);
        Assert.True((await admin.DeleteVenuePackageAsync("coach", coachId, packages[0].Id)).IsSuccess);
        Assert.Single((await admin.GetVenuePackagesAsync("coach", coachId)).Data.Where(p => p.Active));
    }

    [Fact]
    public async Task Database_RequiresExactlyOneOwnerOnConsultationRows()
    {
        if (!_fixture.Available) return;
        using (var context = _fixture.NewContext())
        {
            // فیکسچر برای ساده‌شدن seed همه‌ی قیدها را NOCHECK می‌کند؛ قید مالک را دوباره فعال می‌کنیم
            foreach (var name in new[] { "ConsultationPackages", "ConsultationPurchases", "ConsultationAvailabilities" })
                await context.Database.ExecuteSqlRawAsync($"ALTER TABLE [{name}] WITH NOCHECK CHECK CONSTRAINT [CK_{name}_Owner]");
        }

        async Task<bool> TryInsertAsync(long? companionId, long? clinicId, long? coachId)
        {
            using var context = _fixture.NewContext();
            context.ConsultationPackages.Add(new ConsultationPackage
            {
                CompanionId = companionId, ClinicId = clinicId, CoachId = coachId, ChannelId = 1, DurationMinutes = 30,
                Name = "Owner check", Price = 1000, Active = true, CreateDate = DateTime.Now,
            });
            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        Assert.True(await TryInsertAsync(1, null, null));
        Assert.True(await TryInsertAsync(null, 1, null));
        Assert.True(await TryInsertAsync(null, null, 1));
        Assert.False(await TryInsertAsync(null, null, null));
        Assert.False(await TryInsertAsync(1, 1, null));
        Assert.False(await TryInsertAsync(null, 1, 1));
    }
}
