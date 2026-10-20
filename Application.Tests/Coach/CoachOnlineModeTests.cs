using Application.Common.Enumerable.Code;
using Application.Common.Interface;
using Application.Services.CoachSrvs;
using Application.Services.CoachSrvs.CoachReserveMessageSrv;
using Application.Services.CoachSrvs.CoachReserveMessageSrv.Dto;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.Dto;
using Entities.Entities.CoachField;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static Application.Tests.Coach.CoachReserveTestKit;

namespace Application.Tests.Coach;

public class CoachCallRulesTests
{
    private static readonly DateTime Start = new(2026, 11, 1, 10, 0, 0);
    private const int Online = (int)CoachServiceModeEnum.Online;

    [Fact]
    public void Key_NeverCollidesWithCompanionReservesOrSessions()
    {
        var key = CoachCallRules.KeyFor(123);
        Assert.True(key > 2_000_000_000_000_000L);
        Assert.True(CoachCallRules.IsCoachKey(key));
        Assert.Equal(123, CoachCallRules.ReserveIdOf(key));
        Assert.False(CoachCallRules.IsCoachKey(123));       // رزرو Companion
        Assert.False(CoachCallRules.IsCoachKey(-5));        // جلسه‌ی آنلاین
    }

    [Theory]
    [InlineData(Online, true, false, 0, -30, true)]    // دقیقاً ۳۰ دقیقه قبل
    [InlineData(Online, true, false, 2, 15, true)]     // تأییدشده، بعد از شروع
    [InlineData(Online, true, false, 0, -31, false)]   // زود است
    [InlineData(Online, false, false, 0, 0, false)]    // پرداخت‌نشده
    [InlineData(Online, true, true, 2, 0, false)]      // لغوشده
    [InlineData(Online, true, false, 1, 0, false)]     // منتظر تأیید مربی
    [InlineData(Online, true, false, 3, 0, false)]     // ردشده
    [InlineData((int)CoachServiceModeEnum.InPerson, true, false, 2, 0, false)] // حضوری تماس ندارد
    [InlineData((int)CoachServiceModeEnum.InPlace, true, false, 2, 0, false)]
    public void CanJoin_Matrix(int mode, bool paid, bool cancelled, int decision, int minutesFromStart, bool expected) =>
        Assert.Equal(expected, CoachCallRules.CanJoin(mode, paid, cancelled, decision, Start, null, Start.AddMinutes(minutesFromStart)));

    [Fact]
    public void CanJoin_False_WhenCallAlreadyEnded() =>
        Assert.False(CoachCallRules.CanJoin(Online, true, false, 2, Start, Start.AddMinutes(20), Start.AddMinutes(25)));
}

public class CoachOnlineModeTests : IClassFixture<CoachLocalDbFixture>
{
    private readonly CoachLocalDbFixture _fixture;

    public CoachOnlineModeTests(CoachLocalDbFixture fixture) => _fixture = fixture;

    private sealed class FakeCurrentUser : ICurrentUserHelper
    {
        public FakeCurrentUser(long userId, long roleId = 0) => CurrentUser = new CurrentUserDto { UserId = userId, RoleId = roleId };
        public CurrentUserDto CurrentUser { get; }
    }

    private CoachReserveMessageService NewChat(long userId, out FakeProxy<IPushNotificationService> push, long roleId = 0)
    {
        var context = _fixture.NewContext();
        push = FakeProxy<IPushNotificationService>.Create().Proxy;
        return new CoachReserveMessageService(context, _fixture.Mapper, new FakeCurrentUser(userId, roleId), (IPushNotificationService)(object)push);
    }

    /// <summary>پیام‌ها به ردیف‌های Codes (نوع پیام ۱۴۰ تا ۱۴۳) JOIN می‌شوند؛ در دیتابیس تست باید وجود داشته باشند.</summary>
    private async Task EnsureMessageTypeCodesAsync()
    {
        using var context = _fixture.NewContext();
        foreach (var (id, label) in new[] { (140, "Text"), (141, "Image"), (142, "Voice"), (143, "System") })
        {
            await context.Database.ExecuteSqlRawAsync(
                "IF NOT EXISTS (SELECT 1 FROM Codes WHERE Id = {0}) BEGIN SET IDENTITY_INSERT Codes ON; " +
                "INSERT INTO Codes (Id, Label, Value, CodeGroupId, Priority, Active, Name) VALUES ({0}, {1}, {1}, 1, 1, 1, {1}); SET IDENTITY_INSERT Codes OFF; END",
                id, label);
        }
    }

    private async Task<(World World, long ReserveId)> PaidOnlineReserveAsync()
    {
        await EnsureMessageTypeCodesAsync();
        var world = await BuildWorldAsync(_fixture, serviceOnline: true);
        var input = Input(world, Tomorrow(10));
        input.Mode = (int)CoachServiceModeEnum.Online;
        var created = await NewReserveService(_fixture).CreateAsync(input, world.BookerId);
        Assert.True(created.IsSuccess);
        return (world, (await PayAsync(_fixture, created.Data.Id)).Id);
    }

    private static CoachReserveMessageDto Text(long reserveId, long senderId, string content = "hello") => new()
    {
        CoachReserveId = reserveId,
        SenderUserId = senderId,
        CoachReserveMessageTypeId = (long)CompanionReserveMessageTypeEnum.CompanionReserveMessageType_Text,
        Content = content,
    };

    [Fact]
    public async Task Chat_OnlinePaidReserve_BookerAndCoachCanTalk_OthersCannot()
    {
        if (!_fixture.Available) return;
        var (world, reserveId) = await PaidOnlineReserveAsync();

        var fromBooker = await NewChat(world.BookerId, out var pushA).InsertAsyncDto(Text(reserveId, world.BookerId));
        Assert.True(fromBooker.IsSuccess);
        Assert.Contains("SendPushAsync", pushA.Calls);

        var fromOwner = await NewChat(world.Seed.OwnerUserId, out _).InsertAsyncDto(Text(reserveId, world.Seed.OwnerUserId, "hi"));
        Assert.True(fromOwner.IsSuccess);

        // غریبه، یا ارسال به نام دیگری، یا پیام سیستمی
        Assert.False((await NewChat(world.Seed.OtherUserId, out _).InsertAsyncDto(Text(reserveId, world.Seed.OtherUserId))).IsSuccess);
        Assert.False((await NewChat(world.BookerId, out _).InsertAsyncDto(Text(reserveId, world.Seed.OwnerUserId))).IsSuccess);
        var system = Text(reserveId, world.BookerId);
        system.CoachReserveMessageTypeId = (long)CompanionReserveMessageTypeEnum.CompanionReserveMessageType_System;
        Assert.False((await NewChat(world.BookerId, out _).InsertAsyncDto(system)).IsSuccess);
        Assert.False((await NewChat(world.BookerId, out _).InsertAsyncDto(Text(reserveId, world.BookerId, "  "))).IsSuccess);
    }

    [Fact]
    public async Task Chat_IsClosedForInPersonUnpaidAndCancelledReserves()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture, serviceOnline: true);

        // حضوری (پرداخت‌شده)
        var inPerson = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(9)), world.BookerId)).Data;
        await PayAsync(_fixture, inPerson.Id);
        Assert.False((await NewChat(world.BookerId, out _).InsertAsyncDto(Text(inPerson.Id, world.BookerId))).IsSuccess);

        // آنلاین ولی پرداخت‌نشده
        var onlineInput = Input(world, Tomorrow(12));
        onlineInput.Mode = (int)CoachServiceModeEnum.Online;
        var unpaid = (await NewReserveService(_fixture).CreateAsync(onlineInput, world.BookerId)).Data;
        Assert.False((await NewChat(world.BookerId, out _).InsertAsyncDto(Text(unpaid.Id, world.BookerId))).IsSuccess);

        // آنلاین، پرداخت‌شده و بعد لغو
        var cancelInput = Input(world, Tomorrow(14));
        cancelInput.Mode = (int)CoachServiceModeEnum.Online;
        var toCancel = (await NewReserveService(_fixture).CreateAsync(cancelInput, world.BookerId)).Data;
        await PayAsync(_fixture, toCancel.Id);
        Assert.True((await NewApprovalService(_fixture).CancelByBookerAsync(toCancel.Id, world.BookerId)).IsSuccess);
        Assert.False((await NewChat(world.BookerId, out _).InsertAsyncDto(Text(toCancel.Id, world.BookerId))).IsSuccess);
    }

    [Fact]
    public async Task Chat_SearchShowsOnlyParticipantsAndAdminSeesAll()
    {
        if (!_fixture.Available) return;
        var (world, reserveId) = await PaidOnlineReserveAsync();
        Assert.True((await NewChat(world.BookerId, out _).InsertAsyncDto(Text(reserveId, world.BookerId))).IsSuccess);

        int Count(long userId, long roleId = 0) =>
            NewChat(userId, out _, roleId).Search(new CoachReserveMessageInputDto { CoachReserveId = reserveId, PageIndex = 1, PageSize = 50 }).List.Count;

        Assert.Equal(1, Count(world.BookerId));
        Assert.Equal(1, Count(world.Seed.OwnerUserId));
        Assert.Equal(0, Count(world.Seed.OtherUserId));
        Assert.Equal(1, Count(world.Seed.OtherUserId, (long)Application.Common.Enumerable.RoleEnum.Admin));
    }

    private CoachNotifier NewNotifier(out FakeProxy<IPushNotificationService> push)
    {
        push = FakeProxy<IPushNotificationService>.Create().Proxy;
        return new CoachNotifier(_fixture.NewContext(), (IPushNotificationService)(object)push,
            FakeProxy<Application.Services.Setting.MessageSenderSrv.Iface.IMessageSenderService>.Create().Instance,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<CoachNotifier>.Instance);
    }

    [Fact]
    public async Task Paying_AnOnlineReserve_SchedulesTwoCoachReminders_AndInPersonSchedulesNone()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture, serviceOnline: true);

        var onlineInput = Input(world, Tomorrow(10));
        onlineInput.Mode = (int)CoachServiceModeEnum.Online;
        var online = (await NewReserveService(_fixture).CreateAsync(onlineInput, world.BookerId)).Data;
        var paidOnline = await PayAsync(_fixture, online.Id);
        var onlineNotifier = NewNotifier(out var onlinePush);
        await onlineNotifier.ReservePaidAsync(paidOnline.Id);
        // یک پوش «منتظر تأیید» به مالک + دو یادآور (۱۰ دقیقه قبل و سر ساعت)
        Assert.Equal(3, onlinePush.Calls.Count(c => c == "SendPushAsync"));

        var inPersonReserve = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(13)), world.BookerId)).Data;
        var paidInPerson = await PayAsync(_fixture, inPersonReserve.Id);
        var inPersonNotifier = NewNotifier(out var inPersonPush);
        await inPersonNotifier.ReservePaidAsync(paidInPerson.Id);
        Assert.Equal(1, inPersonPush.Calls.Count(c => c == "SendPushAsync")); // فقط «منتظر تأیید»
    }
}

