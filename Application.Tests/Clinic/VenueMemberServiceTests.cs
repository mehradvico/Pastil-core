using Application.Services.VenueMemberSrv;
using Entities.Entities.ClinicField;
using Entities.Entities.Security;
using Microsoft.Extensions.Caching.Memory;
using Xunit;
using static Application.Tests.Clinic.ClinicReserveTestKit;

namespace Application.Tests.Clinic;

public class VenueMemberServiceTests : IClassFixture<ClinicLocalDbFixture>
{
    private readonly ClinicLocalDbFixture _fixture;

    public VenueMemberServiceTests(ClinicLocalDbFixture fixture) => _fixture = fixture;

    private VenueMemberService NewService() => new(_fixture.NewContext(), new MemoryCache(new MemoryCacheOptions { SizeLimit = 100 }));

    [Fact]
    public async Task ClinicMembers_OwnerFirst_OnlyAcceptedActiveStaff_NoPrivateData()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        long accepted, pending, deleted;
        using (var context = _fixture.NewContext())
        {
            var tag = Random.Shared.NextInt64(1_000_000, 9_999_999).ToString();
            var a = new User { FirstName = "Ali", LastName = "Vet", Mobile = "0921" + tag };
            var p = new User { FirstName = "Pen", LastName = "Ding", Mobile = "0922" + tag };
            var d = new User { FirstName = "Del", LastName = "Eted", Mobile = "0923" + tag };
            context.Users.AddRange(a, p, d);
            context.SaveChanges();
            context.ClinicUsers.AddRange(
                new ClinicUser { ClinicId = world.ClinicId, UserId = a.Id, UserAccept = true, Active = true, InviteDate = DateTime.Now },
                new ClinicUser { ClinicId = world.ClinicId, UserId = p.Id, UserAccept = null, Active = true, InviteDate = DateTime.Now },
                new ClinicUser { ClinicId = world.ClinicId, UserId = d.Id, UserAccept = true, Active = true, Deleted = true, InviteDate = DateTime.Now });
            context.ClinicServiceUsers.Add(new ClinicServiceUser { ClinicServiceId = world.ServiceId, UserId = a.Id, Active = true });
            context.SaveChanges();
            (accepted, pending, deleted) = (a.Id, p.Id, d.Id);
        }

        var result = await NewService().ClinicMembersAsync(world.ClinicId, null, null);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data.Count);
        var owner = result.Data[0];
        Assert.Equal(world.Seed.OwnerUserId, owner.Id);
        Assert.Equal((int)VenueMemberRoleEnum.Owner, owner.RoleId);
        Assert.Equal(Resource.Lang.VenueMemberOwnerRole, owner.RoleTitle);
        Assert.Equal(1, owner.SortOrder);
        var staff = result.Data[1];
        Assert.Equal(accepted, staff.Id);
        Assert.Equal("Ali Vet", staff.FullName);
        Assert.Equal("Bath", staff.RoleTitle); // نام خدمتی که به آن وصل است
        Assert.Equal(2, staff.SortOrder);
        Assert.Null(staff.Picture);
        Assert.DoesNotContain(result.Data, m => m.Id == pending || m.Id == deleted);
        Assert.Null(typeof(VenueMemberVDto).GetProperty("Mobile"));

        var page2 = await NewService().ClinicMembersAsync(world.ClinicId, 2, 1);
        Assert.Equal(accepted, Assert.Single(page2.Data).Id);
    }

    [Fact]
    public async Task CoachMembers_UnknownVenue_IsEmptyNotError()
    {
        if (!_fixture.Available) return;
        var result = await NewService().CoachMembersAsync(long.MaxValue, null, null);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data);
    }

    [Theory]
    [InlineData(null, null, 0, 50)]
    [InlineData(2, 10, 10, 10)]
    [InlineData(0, 500, 0, 50)]
    [InlineData(-1, -5, 0, 50)]
    public void Page_IsBounded(int? page, int? size, int skip, int take) =>
        Assert.Equal((skip, take), VenueMemberRules.Page(page, size));

    [Fact]
    public void RoleTitle_OwnerServicesOrDefault()
    {
        Assert.Equal(Resource.Lang.VenueMemberOwnerRole, VenueMemberRules.RoleTitle(true, new[] { "x" }));
        Assert.Equal(Resource.Lang.VenueMemberDefaultRole, VenueMemberRules.RoleTitle(false, new[] { " ", null }));
        Assert.Equal("A" + Resource.Lang.VenueMemberRoleSeparator + "B", VenueMemberRules.RoleTitle(false, new[] { "A", "A", "B", "C" }));
        Assert.Equal(Resource.Lang.VenueMemberDefaultRole, VenueMemberRules.FullName(" ", null));
    }
}
