using Application.Services.ClinicSrvs.ClinicProfileSrv;
using Application.Services.ClinicSrvs.ClinicProfileSrv.Dto;
using Application.Services.CompanionSrvs.ExpertiseSrv;
using Application.Services.CompanionSrvs.ExpertiseSrv.Dto;
using Application.Services.VenueMemberSrv;
using Entities.Entities.ClinicField;
using Entities.Entities.CompanionField;
using Entities.Entities.ProviderField;
using Entities.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static Application.Tests.Clinic.ClinicReserveTestKit;

namespace Application.Tests.Clinic;

/// <summary>نقش/تخصص اعضای تیم کلینیک (۲۰۲۶-۱۰-۲۸): تعیین توسط مالک، نمایش در لیست تیم و لیست عمومی اعضا، فیلتر تخصص با برچسب نقش.</summary>
public class VenueStaffExpertiseTests : IClassFixture<ClinicLocalDbFixture>
{
    private readonly ClinicLocalDbFixture _fixture;

    public VenueStaffExpertiseTests(ClinicLocalDbFixture fixture) => _fixture = fixture;

    private ClinicProfileService NewProfile() => new(_fixture.NewContext(), _fixture.Mapper, NullLogger<ClinicProfileService>.Instance);

    [Fact]
    public async Task Owner_sets_staff_roles_and_they_show_everywhere()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        long staffRowId, staffUserId, vetId, surgeonId, inactiveId;
        using (var context = _fixture.NewContext())
        {
            var tag = Random.Shared.NextInt64(1_000_000, 9_999_999).ToString();
            var role = context.ProviderRoles.FirstOrDefault(r => r.Label == "Clinic");
            if (role == null)
            {
                role = new ProviderRole { Name = "Clinic", Label = "Clinic", Active = true };
                context.ProviderRoles.Add(role);
                context.SaveChanges();
            }
            var vet = new Expertise { Name = "دامپزشک " + tag, Active = true, ProviderRoleId = role.Id };
            var surgeon = new Expertise { Name = "جراح " + tag, Active = true, ProviderRoleId = role.Id };
            var inactive = new Expertise { Name = "قدیمی " + tag, Active = false, ProviderRoleId = role.Id };
            context.Expertises.AddRange(vet, surgeon, inactive);
            var user = new User { FirstName = "Sara", LastName = "Vet", Mobile = "0931" + tag };
            context.Users.Add(user);
            context.SaveChanges();
            var staff = new ClinicUser { ClinicId = world.ClinicId, UserId = user.Id, UserAccept = true, Active = true, InviteDate = DateTime.Now };
            context.ClinicUsers.Add(staff);
            context.SaveChanges();
            (staffRowId, staffUserId, vetId, surgeonId, inactiveId) = (staff.Id, user.Id, vet.Id, surgeon.Id, inactive.Id);
        }

        // غریبه نمی‌تواند
        Assert.False((await NewProfile().SetStaffExpertiseAsync(new ClinicStaffExpertiseDto { Id = staffRowId, ExpertiseIds = { vetId } }, world.Seed.OtherUserId)).IsSuccess);
        // تخصص غیرفعال رد می‌شود
        Assert.False((await NewProfile().SetStaffExpertiseAsync(new ClinicStaffExpertiseDto { Id = staffRowId, ExpertiseIds = { inactiveId } }, world.Seed.OwnerUserId)).IsSuccess);

        var set = await NewProfile().SetStaffExpertiseAsync(new ClinicStaffExpertiseDto { Id = staffRowId, ExpertiseIds = { vetId, surgeonId } }, world.Seed.OwnerUserId);
        Assert.True(set.IsSuccess);
        Assert.Equal(vetId, set.Data.ExpertiseId); // اولی = اصلی
        Assert.Equal(2, set.Data.Expertises.Count);
        Assert.Contains("دامپزشک", set.Data.ExpertiseName);

        // لیست تیم
        var list = await NewProfile().ListStaffAsync(world.ClinicId, world.Seed.OwnerUserId);
        Assert.Equal(2, list.Data.Single(s => s.Id == staffRowId).Expertises.Count);

        // لیست عمومی اعضا: نقش واقعی به‌جای نام خدمت
        var members = await new VenueMemberService(_fixture.NewContext(), new MemoryCache(new MemoryCacheOptions { SizeLimit = 100 }))
            .ClinicMembersAsync(world.ClinicId, null, null);
        Assert.Contains("جراح", members.Data.Single(m => m.Id == staffUserId).RoleTitle);

        // جایگزینی کامل، و لیست خالی = پاک کردن
        Assert.Single((await NewProfile().SetStaffExpertiseAsync(new ClinicStaffExpertiseDto { Id = staffRowId, ExpertiseIds = { surgeonId } }, world.Seed.OwnerUserId)).Data.Expertises);
        var cleared = await NewProfile().SetStaffExpertiseAsync(new ClinicStaffExpertiseDto { Id = staffRowId }, world.Seed.OwnerUserId);
        Assert.Empty(cleared.Data.Expertises);
        Assert.Null(cleared.Data.ExpertiseId);
    }

    [Fact]
    public async Task Expertise_list_filters_by_role_label_and_includes_general_ones()
    {
        if (!_fixture.Available) return;
        var tag = Random.Shared.NextInt64(1_000_000, 9_999_999).ToString();
        using (var context = _fixture.NewContext())
        {
            var coach = new ProviderRole { Name = "Coach " + tag, Label = "CoachX" + tag, Active = true };
            context.ProviderRoles.Add(coach);
            context.SaveChanges();
            context.Expertises.AddRange(
                new Expertise { Name = "trainer " + tag, Active = true, ProviderRoleId = coach.Id },
                new Expertise { Name = "general " + tag, Active = true },
                new Expertise { Name = "other " + tag, Active = true, ProviderRoleId = null, Deleted = true });
            context.SaveChanges();
        }
        var service = new ExpertiseService(_fixture.NewContext(), _fixture.Mapper);
        var names = service.Search(new ExpertiseInputDto { ProviderRole = "CoachX" + tag, Q = tag, PageIndex = 1, PageSize = 50, Available = true })
            .List.Select(x => x.Name).ToList();
        Assert.Contains("trainer " + tag, names);
        Assert.Contains("general " + tag, names);
        Assert.DoesNotContain("other " + tag, names);
    }

    [Fact]
    public void Role_title_prefers_assigned_roles_over_service_names()
    {
        Assert.Equal("جراح", VenueMemberRules.RoleTitle(false, new[] { "Bath" }, new[] { "جراح" }));
        Assert.Equal("Bath", VenueMemberRules.RoleTitle(false, new[] { "Bath" }, Array.Empty<string>()));
        Assert.Equal(Resource.Lang.VenueMemberOwnerRole, VenueMemberRules.RoleTitle(true, null, new[] { "جراح" }));
    }
}
