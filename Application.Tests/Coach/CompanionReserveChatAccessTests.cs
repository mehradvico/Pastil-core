using Application.Common.Interface;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.CompanionSrvs.CompanionReserveMessageSrv;
using Application.Services.CompanionSrvs.CompanionReserveMessageSrv.Dto;
using Application.Services.Dto;
using Entities.Entities;
using Entities.Entities.CompanionField;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Application.Tests.Coach;

/// <summary>گفتگوی رزرو نماینده: نیروی تخصیص‌یافته فقط تا وقتی تخصیص و عضویتش فعال است دسترسی دارد؛ مالک و رزروکننده همیشه.</summary>
public class CompanionReserveChatAccessTests : IClassFixture<CoachLocalDbFixture>
{
    private readonly CoachLocalDbFixture _fixture;

    public CompanionReserveChatAccessTests(CoachLocalDbFixture fixture) => _fixture = fixture;

    private CompanionReserveMessageService NewService(long userId)
    {
        var user = new CurrentUserDto { UserId = userId, RoleId = -1, RoleEnum = "User" };
        var currentUser = FakeProxy<ICurrentUserHelper>.Create((m, _) => m.Name == "get_CurrentUser" ? user : null).Instance;
        return new CompanionReserveMessageService(_fixture.NewContext(), _fixture.Mapper, currentUser, FakeProxy<IPushNotificationService>.Create().Instance);
    }

    private async Task<(CoachLocalDbFixture.SeedData Seed, long ReserveId, long AssigneeUserId, long CompanionId, long AssistanceUserId)> BuildAsync()
    {
        var seed = _fixture.Seed();
        using var context = _fixture.NewContext();
        var companion = new Companion { Name = "C", ReferralCode = "C" + Guid.NewGuid().ToString("N")[..8], OwnerId = seed.OwnerUserId, CityId = seed.CityId, ProviderId = seed.ProviderId, Phone = "021", AddressValue = "a", Active = true, Approved = true };
        context.Companions.Add(companion);
        await context.SaveChangesAsync();
        var assistance = new CompanionAssistance { CompanionId = companion.Id, AssistanceId = 1, Active = true };
        context.CompanionAssistances.Add(assistance);
        await context.SaveChangesAsync();
        // نیروی تخصیص‌یافته = کاربر «liker» (عضو فعال تیم)
        var assigned = new CompanionAssistanceUser { CompanionAssistanceId = assistance.Id, UserId = seed.LikerUserId, Active = true };
        context.CompanionAssistanceUsers.Add(assigned);
        context.CompanionUsers.Add(new CompanionUser { CompanionId = companion.Id, UserId = seed.LikerUserId, Active = true, UserAccept = true });
        await context.SaveChangesAsync();
        var reserve = new global::Entities.Entities.CompanionReserve
        {
            ReserveCode = "R" + Guid.NewGuid().ToString("N")[..10], BookerId = seed.OtherUserId, CompanionAssistanceId = assistance.Id,
            CompanionAssistanceUserId = assigned.Id, IsReserved = true, CreateDate = DateTime.Now, DoDate = DateTime.Now.AddDays(1),
        };
        context.CompanionReserves.Add(reserve);
        await context.SaveChangesAsync();
        var type = new Code { Name = "Text", Label = "T" + Guid.NewGuid().ToString("N")[..8], Active = true };
        context.Codes.Add(type);
        await context.SaveChangesAsync();
        context.CompanionReserveMessages.Add(new CompanionReserveMessage { CompanionReserveId = reserve.Id, SenderUserId = seed.OtherUserId, CompanionReserveMessageTypeId = type.Id, Content = "hi", CreateDate = DateTime.Now });
        await context.SaveChangesAsync();
        return (seed, reserve.Id, seed.LikerUserId, companion.Id, assigned.Id);
    }

    private static int Count(CompanionReserveMessageService service, long reserveId) =>
        service.Search(new CompanionReserveMessageInputDto { CompanionReserveId = reserveId, PageIndex = 1, PageSize = 20 }).List.Count;

    [Fact]
    public async Task AssignedStaff_LosesChatAccessWhenMembershipOrAssignmentIsDeactivated()
    {
        if (!_fixture.Available) return;
        var world = await BuildAsync();

        // مالک، رزروکننده و نیروی فعال می‌بینند؛ غریبه نه
        Assert.Equal(1, Count(NewService(world.Seed.OwnerUserId), world.ReserveId));
        Assert.Equal(1, Count(NewService(world.Seed.OtherUserId), world.ReserveId));
        Assert.Equal(1, Count(NewService(world.AssigneeUserId), world.ReserveId));
        var stranger = _fixture.Seed();
        Assert.Equal(0, Count(NewService(stranger.OwnerUserId), world.ReserveId));

        // عضویت نیرو غیرفعال شد
        using (var ctx = _fixture.NewContext())
            await ctx.CompanionUsers.Where(u => u.CompanionId == world.CompanionId && u.UserId == world.AssigneeUserId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.Active, false));
        Assert.Equal(0, Count(NewService(world.AssigneeUserId), world.ReserveId));
        Assert.Equal(1, Count(NewService(world.Seed.OwnerUserId), world.ReserveId));
        Assert.Equal(1, Count(NewService(world.Seed.OtherUserId), world.ReserveId));

        // عضویت فعال ولی تخصیص غیرفعال
        using (var ctx = _fixture.NewContext())
        {
            await ctx.CompanionUsers.Where(u => u.CompanionId == world.CompanionId && u.UserId == world.AssigneeUserId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.Active, true));
            await ctx.CompanionAssistanceUsers.Where(u => u.Id == world.AssistanceUserId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.Active, false));
        }
        Assert.Equal(0, Count(NewService(world.AssigneeUserId), world.ReserveId));
    }
}
