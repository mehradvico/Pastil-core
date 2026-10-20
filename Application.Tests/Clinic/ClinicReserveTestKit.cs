using Application.Common.Dto.LocationPoint;
using Application.Common.Dto.Result;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.Accounting.ScoreTransactionSrv.Iface;
using Application.Services.PastilClubSrvs.PointEventSrv.Iface;
using Application.Services.ClinicSrvs;
using Application.Services.ClinicSrvs.ClinicCatalogSrv;
using Application.Services.ClinicSrvs.ClinicCatalogSrv.Dto;
using Application.Services.ClinicSrvs.ClinicReserveSrv;
using Application.Services.ClinicSrvs.ClinicSrv;
using Application.Services.ClinicSrvs.ClinicSrv.Dto;
using Application.Services.Order.RebateSrv.Dto;
using Application.Services.Order.RebateSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Entities.Entities;
using Entities.Entities.ClinicField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Persistence.Context;
using System.Reflection;
using Xunit;

namespace Application.Tests.Clinic;

/// <summary>پراکسی ساده برای ساخت Fake از رابط‌های بزرگ (فقط متدهایی که تست لازم دارد پیاده می‌شود).</summary>
public class FakeProxy<T> : DispatchProxy where T : class
{
    public Func<MethodInfo, object?[]?, object?>? Handler { get; set; }
    public List<string> Calls { get; } = new();

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        Calls.Add(targetMethod!.Name);
        var result = Handler?.Invoke(targetMethod, args);
        if (result != null)
            return result;
        var type = targetMethod.ReturnType;
        if (type == typeof(Task))
            return Task.CompletedTask;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var inner = type.GetGenericArguments()[0];
            var value = inner.IsValueType ? Activator.CreateInstance(inner) : null;
            return typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(inner).Invoke(null, new[] { value });
        }
        return type.IsValueType && type != typeof(void) ? Activator.CreateInstance(type) : null;
    }

    public static (T Instance, FakeProxy<T> Proxy) Create(Func<MethodInfo, object?[]?, object?>? handler = null)
    {
        var instance = Create<T, FakeProxy<T>>();
        var proxy = (FakeProxy<T>)(object)instance;
        proxy.Handler = handler;
        return (instance, proxy);
    }
}

public static class ClinicReserveTestKit
{
    public sealed record World(
        ClinicLocalDbFixture.SeedData Seed, long ClinicId, long ServiceId, long PackageId, long UserPetId, long BookerId,
        long AssistanceId);

    /// <summary>رفتار «ثبت در دفتر کیف پول» را روی همان دیتابیس تست اجرا می‌کند (برای تست برگشت وجه و ایدمپوتنسی).</summary>
    public static (IWalletService Wallet, FakeProxy<IWalletService> Proxy) NewWallet(ClinicLocalDbFixture fixture, bool succeed = true, DataBaseContext? sharedContext = null)
    {
        return FakeProxy<IWalletService>.Create((method, args) =>
        {
            if (method.Name == nameof(IWalletService.InsertAsyncDto))
            {
                var dto = (WalletDto)args![0]!;
                if (!succeed)
                    return Task.FromResult(new BaseResultDto<WalletDto>(false, "x", dto));
                // مثل WalletService واقعی: روی همان DbContext (و تراکنش) سرویس صدا زننده ثبت می‌شود.
                var context = sharedContext ?? fixture.NewContext();
                context.Wallets.Add(new Wallet { Name = dto.Name, Amount = dto.Amount, IsIncrease = dto.IsIncrease, UserId = dto.UserId, CreateDate = DateTime.Now });
                context.SaveChanges();
                return Task.FromResult(new BaseResultDto<WalletDto>(true, dto));
            }
            if (method.Name == nameof(IWalletService.InsertUpdateClinicReserveAsync))
                return Task.FromResult(new BaseResultDto<WalletDto>(succeed, (WalletDto)args![0]!));
            return null;
        });
    }

    public static IRebateService NewRebate(double finalPrice = 0, bool succeed = true, long rebateId = 0)
    {
        return FakeProxy<IRebateService>.Create((method, args) =>
        {
            if (method.Name == nameof(IRebateService.GetRebateByCodeAsync) && args!.Length == 3)
                return succeed
                    ? new BaseResultDto<RebateVDto>(true, new RebateVDto { Id = rebateId, FinalPrice = finalPrice })
                    : new BaseResultDto<RebateVDto>(false, "bad code", null);
            return null;
        }).Instance;
    }

    public static (IClinicNotifier Push, FakeProxy<IClinicNotifier> Proxy) NewPush() =>
        FakeProxy<IClinicNotifier>.Create();

    public static ClinicReserveService NewReserveService(
        ClinicLocalDbFixture fixture, IWalletService? wallet = null, IRebateService? rebate = null, IClinicNotifier? push = null) =>
        new(fixture.NewContext(), wallet ?? NewWallet(fixture).Wallet, rebate ?? NewRebate(), push ?? NewPush().Push,
            FakeProxy<IScoreTransactionService>.Create().Instance, FakeProxy<IClubPointIntegrationService>.Create().Instance,
            NullLogger<ClinicReserveService>.Instance);

    public static ClinicReserveApprovalService NewApprovalService(
        ClinicLocalDbFixture fixture, IWalletService? wallet = null, IClinicNotifier? push = null,
        Application.Services.TripSrv.TripSrv.Iface.ITripService? trip = null) =>
        NewApproval(fixture, wallet, push, trip);

    private static ClinicReserveApprovalService NewApproval(ClinicLocalDbFixture fixture, IWalletService? wallet, IClinicNotifier? push,
        Application.Services.TripSrv.TripSrv.Iface.ITripService? trip)
    {
        var context = fixture.NewContext();
        return new ClinicReserveApprovalService(context, wallet ?? NewWallet(fixture, true, context).Wallet, push ?? NewPush().Push,
            trip ?? FakeProxy<Application.Services.TripSrv.TripSrv.Iface.ITripService>.Create().Instance, NullLogger<ClinicReserveApprovalService>.Instance);
    }

    /// <summary>
    /// کلینیک تأییدشده با خدمت و بسته (قیمت ۱٬۰۰۰٬۰۰۰، پیش‌پرداخت ۲۰۰٬۰۰۰، مدت ۶۰ دقیقه)، ساعت کاری ۰۰:۰۰ تا ۲۳:۵۹ همه‌ی روزهای هفته
    /// با ظرفیت ۲، و یک پت متعلق به کاربر رزروکننده.
    /// </summary>
    public static async Task<World> BuildWorldAsync(ClinicLocalDbFixture fixture, int capacity = 2, bool serviceInPerson = true, bool serviceAtHome = false,
        bool serviceOnline = false, string? packageSize = null, string petSize = "Small", int maxConcurrentPets = 0)
    {
        var seed = fixture.Seed();
        using var context = fixture.NewContext();

        var assistance = new Assistance { Name = "Bath", Active = true, IsGrooming = false };
        context.Assistances.Add(assistance);
        var weekDays = Enumerable.Range(1, 7).Select(number => new WeekDay { Name = "d" + number, Label = "d" + number, Number = number }).ToList();
        context.WeekDays.AddRange(weekDays);
        await context.SaveChangesAsync();

        var core = new ClinicCoreService(context, fixture.Mapper, NullLogger<ClinicCoreService>.Instance);
        var clinic = await core.CreateAsync(new ClinicDto
        {
            Name = "Salon", ProviderId = seed.ProviderId, Phone = "021", AddressValue = "addr",
            StateId = seed.StateId, CityId = seed.CityId, Location = new PointDto(51, 35),
            ServiceInPerson = serviceInPerson, ServiceAtHome = serviceAtHome, ServiceOnline = serviceOnline,
            MaxConcurrentPets = maxConcurrentPets,
        }, byAdmin: true, actorUserId: 1);

        var catalog = new ClinicCatalogService(context, fixture.Mapper, NullLogger<ClinicCatalogService>.Instance);
        var service = (await catalog.AddServiceAsync(new ClinicServiceInputDto { ClinicId = clinic.Data.Id, AssistanceId = assistance.Id, DurationMinutes = 60 }, null)).Data;
        var package = (await catalog.AddPackageAsync(new ClinicServicePackageInputDto
        {
            ClinicServiceId = service.Id, Name = "Pkg", Price = 1_000_000, PrePaymentPrice = 200_000, PetSize = packageSize,
        }, null)).Data;
        await catalog.ApproveServiceAsync(new ClinicServiceApproveDto { Id = service.Id, Approve = true, CommissionPercent = 10 });

        foreach (var weekDay in weekDays)
            context.ClinicTimes.Add(new ClinicTime { ClinicId = clinic.Data.Id, WeekDayId = weekDay.Id, StartTime = "0:00", EndTime = "23:59", Capacity = capacity, Active = true });

        var pet = new UserPet { Name = "Rex", PetId = seed.PetId, UserId = seed.LikerUserId, Size = petSize };
        context.UserPets.Add(pet);
        await context.SaveChangesAsync();
        return new World(seed, clinic.Data.Id, service.Id, package.Id, pet.Id, seed.LikerUserId, assistance.Id);
    }

    /// <summary>زمان نوبتِ فردا ساعت مشخص (همیشه در آینده و بیرون از MinLead).</summary>
    public static DateTime Tomorrow(int hour, int minute = 0) => DateTime.Today.AddDays(1).AddHours(hour).AddMinutes(minute);

    public static Application.Services.ClinicSrvs.ClinicReserveSrv.Dto.ClinicReserveInputDto Input(World world, DateTime doDate) => new()
    {
        ClinicId = world.ClinicId,
        ClinicServicePackageId = world.PackageId,
        UserPetId = world.UserPetId,
        DoDate = doDate,
        Mode = (int)ClinicServiceModeEnum.InPerson,
    };

    /// <summary>پرداخت را مثل PaymentService شبیه‌سازی می‌کند (callback) و رزرو را «پرداخت‌شده» می‌کند.</summary>
    public static async Task<ClinicReserve> PayAsync(ClinicLocalDbFixture fixture, long reserveId, IWalletService? wallet = null, IRebateService? rebate = null)
    {
        var result = await NewReserveService(fixture, wallet, rebate).ClinicReservePaymentCallback(reserveId);
        Assert.True(result.IsSuccess, string.Join(",", result.Messages?.Select(m => m.Item1) ?? Array.Empty<string>()));
        using var context = fixture.NewContext();
        return await context.ClinicReserves.AsNoTracking().FirstAsync(r => r.Id == reserveId);
    }
}
