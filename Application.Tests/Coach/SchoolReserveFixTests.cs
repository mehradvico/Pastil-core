using Application.Common.Interface;
using Application.Services.Dto;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.Order.RebateSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Application.Services.SchoolSrvs.SchoolCourseSrv;
using Application.Services.SchoolSrvs.SchoolReserveSrv;
using Application.Services.SchoolSrvs.SchoolReserveSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using Entities.Entities;
using Entities.Entities.SchoolField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Application.Tests.Coach;

/// <summary>ثبت‌نام مدرسه: جای نگه‌داشته‌شده‌ی پرداخت‌نشده، برگشت وجه لغو، و دسترسی به فایل درس.</summary>
public class SchoolReserveFixTests : IClassFixture<CoachLocalDbFixture>
{
    private readonly CoachLocalDbFixture _fixture;

    public SchoolReserveFixTests(CoachLocalDbFixture fixture) => _fixture = fixture;

    private sealed class Wallets
    {
        public readonly List<WalletDto> Credits = new();
    }

    private SchoolReserveService NewService(long userId, bool admin, Wallets wallets)
    {
        var serviceContext = _fixture.NewContext();
        var user = new CurrentUserDto { UserId = userId, RoleEnum = admin ? "Admin" : "User" };
        var currentUser = FakeProxy<ICurrentUserHelper>.Create((m, _) => m.Name == "get_CurrentUser" ? user : null).Instance;
        var walletFake = FakeProxy<IWalletService>.Create((m, args) =>
        {
            if (m.Name != nameof(IWalletService.InsertAsyncDto))
                return null;
            var dto = (WalletDto)args![0]!;
            wallets.Credits.Add(dto);
            // مثل WalletService واقعی روی همان DbContext (و همان تراکنش) سرویس می‌نویسد
            serviceContext.Wallets.Add(new Wallet { Name = dto.Name, Amount = dto.Amount, IsIncrease = dto.IsIncrease, UserId = dto.UserId, CreateDate = DateTime.Now });
            serviceContext.SaveChanges();
            return Task.FromResult(new Application.Common.Dto.Result.BaseResultDto<WalletDto>(true, dto));
        }).Instance;
        return new SchoolReserveService(serviceContext, _fixture.Mapper, currentUser,
            FakeProxy<IPushNotificationService>.Create().Instance, FakeProxy<IRebateService>.Create().Instance, walletFake,
            NullLogger<SchoolReserveService>.Instance, FakeProxy<ITripService>.Create().Instance);
    }

    private async Task<(CoachLocalDbFixture.SeedData Seed, long CourseId, long CompanionId)> BuildCourseAsync(int? capacity)
    {
        var seed = _fixture.Seed();
        using var context = _fixture.NewContext();
        var companion = new Companion { Name = "Sch", ReferralCode = "S" + Guid.NewGuid().ToString("N")[..8], OwnerId = seed.OwnerUserId, CityId = seed.CityId, ProviderId = seed.ProviderId, Phone = "021", AddressValue = "a", Active = true, Approved = true };
        context.Companions.Add(companion);
        await context.SaveChangesAsync();
        var school = new School { Name = "School", CompanionId = companion.Id, Active = true, StateId = seed.StateId, CityId = seed.CityId, AddressValue = "a" };
        context.Schools.Add(school);
        await context.SaveChangesAsync();
        var course = new SchoolCourse { Name = "Course", SchoolId = school.Id, CourseTypeId = 1, Price = 100_000, Capacity = capacity, Active = true };
        context.SchoolCourses.Add(course);
        await context.SaveChangesAsync();
        return (seed, course.Id, companion.Id);
    }

    private async Task<long> AddPetAsync(long userId, long petId)
    {
        using var context = _fixture.NewContext();
        var pet = new UserPet { Name = "P" + Guid.NewGuid().ToString("N")[..4], PetId = petId, UserId = userId, Size = "Small" };
        context.UserPets.Add(pet);
        await context.SaveChangesAsync();
        return pet.Id;
    }

    private async Task<long> AddReserveAsync(long courseId, long bookerId, long petId, bool paid, DateTime created, double price = 100_000)
    {
        using var context = _fixture.NewContext();
        var reserve = new SchoolReserve
        {
            ReserveCode = "R" + Guid.NewGuid().ToString("N")[..10], SchoolCourseId = courseId, BookerId = bookerId, UserPetId = petId,
            Price = price, PaymentPrice = price, IsReserved = paid, CreateDate = created,
            StatusId = paid ? (int)Application.Common.Enumerable.Code.SchoolReserveStatusEnum.Paid : (int)Application.Common.Enumerable.Code.SchoolReserveStatusEnum.Registered,
            SiteShare = paid ? 10_000 : 0, CompanionShare = paid ? 90_000 : 0,
        };
        context.SchoolReserves.Add(reserve);
        await context.SaveChangesAsync();
        return reserve.Id;
    }

    [Fact]
    public async Task UnpaidEnrollment_HoldsASeatOnlyBriefly()
    {
        if (!_fixture.Available) return;
        var (seed, courseId, _) = await BuildCourseAsync(capacity: 1);
        var pet1 = await AddPetAsync(seed.OwnerUserId, seed.PetId);
        var pet2 = await AddPetAsync(seed.OtherUserId, seed.PetId);
        var service = NewService(seed.OtherUserId, false, new Wallets());

        // ثبت‌نام پرداخت‌نشده‌ی کهنه (۳۰ دقیقه پیش) جایی نگه نمی‌دارد
        await AddReserveAsync(courseId, seed.OwnerUserId, pet1, paid: false, DateTime.Now.AddMinutes(-30));
        Assert.Equal(1, await service.GetRemainingCapacityAsync(courseId));
        var enrolled = await service.InsertAsyncDto(new SchoolReserveDto { SchoolCourseId = courseId, UserPetId = pet2, BookerId = seed.OtherUserId });
        Assert.True(enrolled.IsSuccess);

        // ثبت‌نام تازه‌ی پرداخت‌نشده جا نگه می‌دارد؛ نفر بعد پر می‌بیند
        Assert.Equal(0, await service.GetRemainingCapacityAsync(courseId));
        var third = _fixture.Seed();
        var pet3 = await AddPetAsync(third.OwnerUserId, third.PetId);
        var blocked = await NewService(third.OwnerUserId, false, new Wallets())
            .InsertAsyncDto(new SchoolReserveDto { SchoolCourseId = courseId, UserPetId = pet3, BookerId = third.OwnerUserId });
        Assert.False(blocked.IsSuccess);
    }

    [Fact]
    public async Task Cancel_RefundsPaidAmountOnce_AndZeroesUnsettledShares()
    {
        if (!_fixture.Available) return;
        var (seed, courseId, _) = await BuildCourseAsync(capacity: null);
        var pet = await AddPetAsync(seed.OwnerUserId, seed.PetId);
        var reserveId = await AddReserveAsync(courseId, seed.OwnerUserId, pet, paid: true, DateTime.Now);
        var wallets = new Wallets();
        var service = NewService(seed.OwnerUserId, false, wallets);

        // دلیل لازم است
        Assert.False((await service.UpdateCancelDto(new SchoolReserveCancelDto { Id = reserveId, IsCancel = true })).IsSuccess);
        Assert.Empty(wallets.Credits);

        var cancelled = await service.UpdateCancelDto(new SchoolReserveCancelDto { Id = reserveId, IsCancel = true, CancelDetail = "x" });
        Assert.True(cancelled.IsSuccess);
        var credit = Assert.Single(wallets.Credits);
        Assert.Equal(100_000, credit.Amount);
        Assert.True(credit.IsIncrease);
        Assert.Equal($"SchoolReserveRefund:{reserveId}", credit.Name);

        using var context = _fixture.NewContext();
        var row = await context.SchoolReserves.AsNoTracking().FirstAsync(r => r.Id == reserveId);
        Assert.True(row.IsCancel);
        Assert.Equal(0, row.SiteShare);
        Assert.Equal(0, row.CompanionShare);

        // لغو دوباره پولی برنمی‌گرداند
        Assert.False((await NewService(seed.OwnerUserId, false, wallets).UpdateCancelDto(new SchoolReserveCancelDto { Id = reserveId, IsCancel = true, CancelDetail = "x" })).IsSuccess);
        Assert.Single(wallets.Credits);
    }

    [Fact]
    public async Task Cancel_AfterSettlement_KeepsShares_AndStrangerCannotCancel()
    {
        if (!_fixture.Available) return;
        var (seed, courseId, _) = await BuildCourseAsync(capacity: null);
        var pet = await AddPetAsync(seed.OwnerUserId, seed.PetId);
        var reserveId = await AddReserveAsync(courseId, seed.OwnerUserId, pet, paid: true, DateTime.Now);
        using (var ctx = _fixture.NewContext())
            await ctx.SchoolReserves.Where(r => r.Id == reserveId).ExecuteUpdateAsync(s => s.SetProperty(r => r.Permitted, true));

        var wallets = new Wallets();
        Assert.False((await NewService(seed.OtherUserId, false, wallets).UpdateCancelDto(new SchoolReserveCancelDto { Id = reserveId, IsCancel = true, CancelDetail = "x" })).IsSuccess);
        Assert.Empty(wallets.Credits);

        Assert.True((await NewService(seed.OwnerUserId, false, wallets).UpdateCancelDto(new SchoolReserveCancelDto { Id = reserveId, IsCancel = true, CancelDetail = "x" })).IsSuccess);
        using var context = _fixture.NewContext();
        var row = await context.SchoolReserves.AsNoTracking().FirstAsync(r => r.Id == reserveId);
        Assert.Equal(90_000, row.CompanionShare); // قبلاً به نماینده پرداخت شده؛ برای پیگیری مالی می‌ماند
    }

    [Fact]
    public async Task LessonFile_IsReadableOnlyByPayingStudentOwnerOrAdmin()
    {
        if (!_fixture.Available) return;
        var (seed, courseId, companionId) = await BuildCourseAsync(capacity: null);
        long fileId;
        using (var ctx = _fixture.NewContext())
        {
            var file = new Entities.Entities.File { Name = "f", Url = "u", OrginalName = "f.mp4", Extension = ".mp4", ContentType = "video/mp4", CreateDate = DateTime.Now };
            ctx.Files.Add(file);
            await ctx.SaveChangesAsync();
            fileId = file.Id;
            ctx.SchoolCourseVideos.Add(new SchoolCourseVideo { Name = "v", SchoolCourseId = courseId, FileId = fileId, Active = true });
            await ctx.SaveChangesAsync();
        }
        var paying = _fixture.Seed();
        var unpaid = _fixture.Seed();
        var payingPet = await AddPetAsync(paying.OwnerUserId, paying.PetId);
        var unpaidPet = await AddPetAsync(unpaid.OwnerUserId, unpaid.PetId);
        await AddReserveAsync(courseId, paying.OwnerUserId, payingPet, paid: true, DateTime.Now);
        await AddReserveAsync(courseId, unpaid.OwnerUserId, unpaidPet, paid: false, DateTime.Now);

        var courses = new SchoolCourseService(_fixture.NewContext(), _fixture.Mapper);
        Assert.True(await courses.CanReadLessonFileAsync(fileId, paying.OwnerUserId, null, false));
        Assert.False(await courses.CanReadLessonFileAsync(fileId, unpaid.OwnerUserId, null, false));
        Assert.False(await courses.CanReadLessonFileAsync(fileId, seed.OtherUserId, null, false));
        Assert.True(await courses.CanReadLessonFileAsync(fileId, seed.OtherUserId, companionId, false));
        Assert.False(await courses.CanReadLessonFileAsync(fileId, seed.OtherUserId, companionId + 999, false));
        Assert.True(await courses.CanReadLessonFileAsync(fileId, seed.OtherUserId, null, true));
        Assert.True(await courses.CanReadLessonFileAsync(fileId + 12345, seed.OtherUserId, null, false)); // فایل غیردرسی
    }
}
