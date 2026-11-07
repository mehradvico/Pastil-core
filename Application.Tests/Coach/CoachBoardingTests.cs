using Application.Services.CoachSrvs.CoachCatalogSrv;
using Application.Services.CoachSrvs.CoachCatalogSrv.Dto;
using Application.Services.CoachSrvs.CoachReserveSrv;
using Application.Services.CoachSrvs.CoachReserveSrv.Dto;
using Entities.Entities;
using Entities.Entities.CoachField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static Application.Tests.Coach.CoachReserveTestKit;

namespace Application.Tests.Coach;

/// <summary>
/// پانسیون آموزشی مربی (Mode=۳۸ در مرکز): مربی هزینه‌ی دوره، پیش‌پرداخت، تعداد روز و پانسیون روزانه را تعیین می‌کند؛
/// کاربر فقط روز تحویل پت را انتخاب می‌کند. آنلاین = فقط پیش‌پرداخت؛ در مرکز = کل − پیش‌پرداخت.
/// </summary>
public class CoachBoardingRulesTests
{
    [Theory]
    [InlineData(10_000_000, 2_000_000, 500_000d, 60, 40_000_000, 2_000_000)]
    [InlineData(10_000_000, 2_000_000, null, 60, 10_000_000, 2_000_000)] // بدون پانسیون روزانه = فقط دوره
    [InlineData(10_000_000, 0, 500_000d, 30, 25_000_000, 25_000_000)] // بدون پیش‌پرداخت = کل مبلغ آنلاین
    [InlineData(10_000_000, 15_000_000, 500_000d, 60, 40_000_000, 15_000_000)] // پیش‌پرداخت می‌تواند از هزینه‌ی دوره بیشتر باشد
    [InlineData(0, 0, 500_000d, 30, 15_000_000, 15_000_000)] // فقط پانسیون
    public void BoardingPackageAmounts(double course, double pre, double? daily, int days, double total, double online)
    {
        var (p, o) = CoachReserveRules.BoardingPackageAmounts(course, pre, daily, days);
        Assert.Equal(total, p);
        Assert.Equal(online, o);
    }

    [Fact]
    public void FreeCancel_StayNeeds48Hours()
    {
        var now = new DateTime(2026, 11, 10, 10, 0, 0);
        var start = now.AddHours(24);
        Assert.True(CoachReserveRules.IsFreeCancel(now, start, stay: false));
        Assert.False(CoachReserveRules.IsFreeCancel(now, start, stay: true));
        Assert.True(CoachReserveRules.IsFreeCancel(now, now.AddHours(48), stay: true));
    }
}

public class CoachBoardingServiceTests : IClassFixture<CoachLocalDbFixture>
{
    private const int Boarding = (int)CoachServiceModeEnum.InPerson;
    private readonly CoachLocalDbFixture _fixture;

    public CoachBoardingServiceTests(CoachLocalDbFixture fixture) => _fixture = fixture;

    /// <summary>دنیای پایه + ظرفیت پت مقیم + بسته‌ی پانسیون آموزشی (دوره ۱۰٬۰۰۰٬۰۰۰، پیش‌پرداخت ۲٬۰۰۰٬۰۰۰، ۶۰ روز × ۵۰۰٬۰۰۰).</summary>
    private async Task<(World World, long BoardingPackageId)> BuildAsync(int boardingCapacity = 1, int stayDays = 60)
    {
        var world = await BuildWorldAsync(_fixture, capacity: 2, serviceAtHome: true);
        using var context = _fixture.NewContext();
        await context.Coaches.Where(c => c.Id == world.CoachId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.BoardingCapacity, boardingCapacity));
        var catalog = new CoachCatalogService(context, _fixture.Mapper, NullLogger<CoachCatalogService>.Instance);
        var package = await catalog.AddPackageAsync(new CoachServicePackageInputDto
        {
            CoachServiceId = world.ServiceId,
            Name = "Boarding",
            Types = new List<CoachPackageTypeDto>
            {
                new() { Mode = Boarding, Price = 10_000_000, PrePaymentPrice = 2_000_000, StayDays = stayDays, DailyPrice = 500_000 },
                // روز و پانسیون روزانه فقط برای ۳۸ ذخیره می‌شود
                new() { Mode = (int)CoachServiceModeEnum.InPlace, Price = 8_000_000, PrePaymentPrice = 1_000_000, StayDays = 9, DailyPrice = 999 },
            },
        }, null);
        Assert.True(package.IsSuccess, string.Join(",", package.Messages?.Select(m => m.Item1) ?? Array.Empty<string>()));
        var inPlace = package.Data.Types.Single(t => t.Mode == (int)CoachServiceModeEnum.InPlace);
        Assert.Null(inPlace.DailyPrice);
        Assert.Null(inPlace.StayDays);
        return (world, package.Data.Id);
    }

    private static CoachReserveInputDto BoardingInput(World world, long packageId, DateTime dropOff, long? petId = null) => new()
    {
        CoachId = world.CoachId,
        CoachServicePackageId = packageId,
        UserPetId = petId ?? world.UserPetId,
        DoDate = dropOff,
        Mode = Boarding,
    };

    private async Task<long> AddPetAsync(World world)
    {
        using var context = _fixture.NewContext();
        var pet = new UserPet { Name = "Bella", PetId = world.Seed.PetId, UserId = world.Seed.OtherUserId, Size = "Small" };
        context.UserPets.Add(pet);
        await context.SaveChangesAsync();
        return pet.Id;
    }

    [Fact]
    public async Task Package_DailyPriceNeedsDays()
    {
        if (!_fixture.Available) return;
        var (world, _) = await BuildAsync();
        using var context = _fixture.NewContext();
        var catalog = new CoachCatalogService(context, _fixture.Mapper, NullLogger<CoachCatalogService>.Instance);
        var noDays = await catalog.AddPackageAsync(new CoachServicePackageInputDto
        {
            CoachServiceId = world.ServiceId, Name = "x",
            Types = new List<CoachPackageTypeDto> { new() { Mode = Boarding, Price = 1_000_000, DailyPrice = 100_000 } },
        }, null);
        Assert.Equal(Resource.Notification.CoachPackageBoardingDaysRequired, noDays.Messages[0].Item1);
        // پیش‌پرداخت تا سقف کل (۱٬۰۰۰٬۰۰۰ + ۱۰ × ۱۰۰٬۰۰۰ = ۲٬۰۰۰٬۰۰۰) مجاز، بیشتر از آن نه
        Assert.True((await catalog.AddPackageAsync(new CoachServicePackageInputDto
        {
            CoachServiceId = world.ServiceId, Name = "y",
            Types = new List<CoachPackageTypeDto> { new() { Mode = Boarding, Price = 1_000_000, PrePaymentPrice = 2_000_000, StayDays = 10, DailyPrice = 100_000 } },
        }, null)).IsSuccess);
        Assert.False((await catalog.AddPackageAsync(new CoachServicePackageInputDto
        {
            CoachServiceId = world.ServiceId, Name = "z",
            Types = new List<CoachPackageTypeDto> { new() { Mode = Boarding, Price = 1_000_000, PrePaymentPrice = 2_000_001, StayDays = 10, DailyPrice = 100_000 } },
        }, null)).IsSuccess);
    }

    [Fact]
    public async Task Quote_CourseFeePlusDailyTimesPackageDays()
    {
        if (!_fixture.Available) return;
        var (_, packageId) = await BuildAsync();
        var service = NewReserveService(_fixture);

        var quote = await service.QuoteAsync(new[] { packageId }, Boarding);
        Assert.True(quote.IsSuccess);
        Assert.Equal(40_000_000, quote.Data.Price);
        Assert.Equal(2_000_000, quote.Data.OnlineAmount);
        Assert.Equal(38_000_000, quote.Data.PayAtVenue);
        Assert.Equal(60, quote.Data.StayDays);
        Assert.Equal(10_000_000, quote.Data.BasePrice);
        Assert.Equal(500_000, quote.Data.DailyPrice);
        Assert.Equal(30_000_000, quote.Data.BoardingFee);
        Assert.Equal(2_000_000, quote.Data.CoursePrePayment);

        // در محل مشتری پانسیون ندارد
        Assert.Equal(8_000_000, (await service.QuoteAsync(new[] { packageId }, (int)CoachServiceModeEnum.InPlace)).Data.Price);
    }

    [Fact]
    public async Task Create_OnlyStartDate_SpansPackageDays_SeparateCapacity_PetBusy()
    {
        if (!_fixture.Available) return;
        var (world, packageId) = await BuildAsync(boardingCapacity: 1);
        var dropOff = Tomorrow(10);

        var created = await NewReserveService(_fixture).CreateAsync(BoardingInput(world, packageId, dropOff), world.BookerId);
        Assert.True(created.IsSuccess, string.Join(",", created.Messages?.Select(m => m.Item1) ?? Array.Empty<string>()));
        Assert.Equal(dropOff.AddDays(60), created.Data.EndDate);
        Assert.Equal(60, created.Data.StayDays);
        Assert.Equal(500_000, created.Data.DailyPrice);
        Assert.Equal(40_000_000, created.Data.Price);
        Assert.Equal(2_000_000, created.Data.OnlineAmount);
        Assert.Equal(38_000_000, created.Data.PayAtVenue);
        Assert.Equal(40_000_000, created.Data.Items.Single().PackagePrice);

        // ظرفیت پانسیون (۱) پر است
        var secondPet = await AddPetAsync(world);
        var full = await NewReserveService(_fixture).CreateAsync(BoardingInput(world, packageId, dropOff.AddDays(20), secondPet), world.Seed.OtherUserId);
        Assert.Equal(Resource.Notification.CoachReserveBoardingFull, full.Messages[0].Item1);

        // نوبت ساعتی (بسته‌ی ساده) ظرفیت جداگانه دارد
        var session = Input(world, dropOff.AddDays(5));
        session.UserPetId = secondPet;
        Assert.True((await NewReserveService(_fixture).CreateAsync(session, world.Seed.OtherUserId)).IsSuccess);

        // بعد از پرداخت، خودِ پت در کل اقامت مشغول است
        await PayAsync(_fixture, created.Data.Id);
        var busy = await NewReserveService(_fixture).CreateAsync(Input(world, dropOff.AddDays(10)), world.BookerId);
        Assert.Equal(Resource.Notification.CoachReservePetBusy, busy.Messages[0].Item1);

        // تقویم: شروع وسط اقامت ممکن نیست؛ بعد از پایان ممکن است
        var availability = await NewReserveService(_fixture).GetBoardingAvailabilityAsync(world.CoachId, packageId, dropOff.Date, 90);
        Assert.True(availability.IsSuccess);
        Assert.Equal(60, availability.Data.StayDays);
        Assert.False(availability.Data.Days.Single(d => d.Date == dropOff.Date.AddDays(30)).CanStart);
        var after = availability.Data.Days.Single(d => d.Date == dropOff.Date.AddDays(61));
        Assert.True(after.CanStart);
        Assert.Equal(dropOff.Date.AddDays(121), after.EndDate);
        Assert.NotEmpty(after.Windows);
    }

    [Fact]
    public async Task ZeroCapacity_MeansNoLimit()
    {
        if (!_fixture.Available) return;
        var (world, packageId) = await BuildAsync(boardingCapacity: 0);
        var dropOff = Tomorrow(10);
        Assert.True((await NewReserveService(_fixture).CreateAsync(BoardingInput(world, packageId, dropOff), world.BookerId)).IsSuccess);
        var secondPet = await AddPetAsync(world);
        Assert.True((await NewReserveService(_fixture).CreateAsync(BoardingInput(world, packageId, dropOff, secondPet), world.Seed.OtherUserId)).IsSuccess);
        var availability = await NewReserveService(_fixture).GetBoardingAvailabilityAsync(world.CoachId, packageId, dropOff.Date, 5);
        Assert.Null(availability.Data.Days[0].Available);
        Assert.True(availability.Data.Days[0].CanStart);
    }

    [Fact]
    public async Task DropOffOutsideWorkingHours_Rejected_AndClosedDayCannotStart()
    {
        if (!_fixture.Available) return;
        var (world, packageId) = await BuildAsync(boardingCapacity: 2);
        var dropOff = Tomorrow(20);
        using (var context = _fixture.NewContext())
        {
            await context.CoachTimes.Where(t => t.CoachId == world.CoachId)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.StartTime, "9:00").SetProperty(t => t.EndTime, "18:00"));
            var closed = CoachReserveRules.WeekDayNumber(dropOff.AddDays(1));
            await context.CoachTimes.Where(t => t.CoachId == world.CoachId && t.WeekDay.Number == closed)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.Active, false));
        }

        var result = await NewReserveService(_fixture).CreateAsync(BoardingInput(world, packageId, dropOff), world.BookerId);
        Assert.Equal(Resource.Notification.CoachReserveOutsideWorkingHours, result.Messages[0].Item1);

        var availability = await NewReserveService(_fixture).GetBoardingAvailabilityAsync(world.CoachId, packageId, dropOff.Date, 3);
        Assert.False(availability.Data.Days.Single(d => d.Date == dropOff.Date.AddDays(1)).CanStart);
        Assert.Empty(availability.Data.Days.Single(d => d.Date == dropOff.Date.AddDays(1)).Windows);
    }
}
