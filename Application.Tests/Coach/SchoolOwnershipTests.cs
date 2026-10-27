using Application.Services.SchoolSrvs.SchoolCourseSrv;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Dto;
using Application.Services.SchoolSrvs.SchoolOwnershipSrv;
using Application.Services.SchoolSrvs.SchoolSrv;
using Application.Services.SchoolSrvs.SchoolSrv.Dto;
using Entities.Entities;
using Entities.Entities.SchoolField;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Application.Tests.Coach;

/// <summary>مالکیت مدرسه با Companion.OwnerId از دیتابیس سنجیده می‌شود، نه CompanionId توکن (که تا ورود دوباره کهنه می‌ماند).</summary>
public class SchoolOwnershipTests : IClassFixture<CoachLocalDbFixture>
{
    private readonly CoachLocalDbFixture _fixture;

    public SchoolOwnershipTests(CoachLocalDbFixture fixture) => _fixture = fixture;

    private SchoolOwnershipService NewOwnership() => new(_fixture.NewContext());

    private async Task<(long CompanionId, long SchoolId, long CourseId, long SessionId)> AddAsync(CoachLocalDbFixture.SeedData seed, bool approved)
    {
        using var context = _fixture.NewContext();
        var companion = new Companion { Name = "Sch", ReferralCode = "S" + Guid.NewGuid().ToString("N")[..8], OwnerId = seed.OwnerUserId, CityId = seed.CityId, ProviderId = seed.ProviderId, Phone = "021", AddressValue = "a", Active = approved, Approved = approved };
        context.Companions.Add(companion);
        await context.SaveChangesAsync();
        var school = new School { Name = "School " + Guid.NewGuid().ToString("N")[..4], CompanionId = companion.Id, Active = true, StateId = seed.StateId, CityId = seed.CityId, AddressValue = "a" };
        context.Schools.Add(school);
        await context.SaveChangesAsync();
        var course = new SchoolCourse { Name = "Course", SchoolId = school.Id, CourseTypeId = 1, Price = 1000, Active = true };
        context.SchoolCourses.Add(course);
        await context.SaveChangesAsync();
        var session = new SchoolCourseSession { SchoolCourseId = course.Id, SessionDate = DateTime.Today.AddDays(3), StartTime = "10:00", EndTime = "11:00", Active = true };
        context.SchoolCourseSessions.Add(session);
        await context.SaveChangesAsync();
        return (companion.Id, school.Id, course.Id, session.Id);
    }

    [Fact]
    public async Task Ownership_FollowsTheCompanionOwner_AndApprovalGatesWrites()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var approved = await AddAsync(seed, approved: true);
        var pending = await AddAsync(seed, approved: false); // «در حال بررسی»: توکن او هنوز CompanionId ندارد
        var ownership = NewOwnership();

        // مالک: هر دو نمایندگی‌اش را (نه فقط اولی) می‌شناسد؛ خروجی = شناسه‌ی مدرسه‌ی صاحب منبع
        Assert.Equal(approved.SchoolId, await ownership.SchoolOfSchoolAsync(approved.SchoolId, seed.OwnerUserId));
        Assert.Equal(approved.SchoolId, await ownership.SchoolOfCourseAsync(approved.CourseId, seed.OwnerUserId));
        Assert.Equal(approved.SchoolId, await ownership.SchoolOfSessionAsync(approved.SessionId, seed.OwnerUserId));

        // نمایندگی تأییدنشده: خواندن مال خودش آزاد، نوشتن بسته
        Assert.Null(await ownership.SchoolOfSchoolAsync(pending.SchoolId, seed.OwnerUserId));
        Assert.Equal(pending.SchoolId, await ownership.SchoolOfSchoolAsync(pending.SchoolId, seed.OwnerUserId, approvedOnly: false));

        // بعد از تأیید ادمین (بدون ورود دوباره) همان لحظه باز می‌شود
        using (var ctx = _fixture.NewContext())
            await ctx.Companions.Where(c => c.Id == pending.CompanionId).ExecuteUpdateAsync(s => s.SetProperty(c => c.Active, true).SetProperty(c => c.Approved, true));
        Assert.Equal(pending.SchoolId, await ownership.SchoolOfCourseAsync(pending.CourseId, seed.OwnerUserId));

        // غریبه هیچ‌چیز ندارد
        Assert.Null(await ownership.SchoolOfSchoolAsync(approved.SchoolId, seed.OtherUserId, approvedOnly: false));
        Assert.Null(await ownership.SchoolOfSessionAsync(approved.SessionId, seed.OtherUserId, approvedOnly: false));
        Assert.Null(await ownership.SchoolOfReserveAsync(123456, seed.OwnerUserId));

        // ساخت مدرسه‌ی جدید بدون نقش «مدرسه»: با دو نمایندگی باید یکی را صریح بگوید
        Assert.Equal(Resource.Notification.SchoolChooseCompanion, (await ownership.ResolveOwnerForNewSchoolAsync(seed.OwnerUserId, null)).Error);
        var legacy = await ownership.ResolveOwnerForNewSchoolAsync(seed.OwnerUserId, approved.CompanionId);
        Assert.True(legacy.Ok);
        Assert.Equal(approved.CompanionId, legacy.CompanionId);
        Assert.Equal(seed.ProviderId, legacy.ProviderId); // ارائه‌دهنده‌ی همان نمایندگی
        Assert.Equal(Resource.Notification.AccessDenied, (await ownership.ResolveOwnerForNewSchoolAsync(seed.OwnerUserId, approved.CompanionId + 9999)).Error);
        Assert.False((await ownership.ResolveOwnerForNewSchoolAsync(seed.OtherUserId, approved.CompanionId)).Ok);
    }

    private async Task GrantSchoolRoleAsync(long providerId)
    {
        using var context = _fixture.NewContext();
        var role = await context.ProviderRoles.FirstOrDefaultAsync(r => r.Label == Entities.Entities.ProviderField.ProviderRoleLabels.School);
        if (role == null)
        {
            role = new Entities.Entities.ProviderField.ProviderRole { Name = "School", Label = Entities.Entities.ProviderField.ProviderRoleLabels.School, Active = true };
            context.ProviderRoles.Add(role);
            await context.SaveChangesAsync();
        }
        context.ProviderRoleAssignments.Add(new Entities.Entities.ProviderField.ProviderRoleAssignment { ProviderId = providerId, ProviderRoleId = role.Id, Approved = true });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task ProviderWithSchoolRole_OwnsSchools_WithoutAnyCompanion()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var ownership = NewOwnership();

        // بدون نقش و بدون نمایندگی: پیام روشن (نه ۴۰۳ خالی)
        Assert.Equal(Resource.Notification.SchoolNeedsProviderRole, (await ownership.ResolveOwnerForNewSchoolAsync(seed.OwnerUserId, null)).Error);

        await GrantSchoolRoleAsync(seed.ProviderId);
        var owner = await ownership.ResolveOwnerForNewSchoolAsync(seed.OwnerUserId, null);
        Assert.True(owner.Ok);
        Assert.Equal(seed.ProviderId, owner.ProviderId);
        Assert.Null(owner.CompanionId);

        long schoolId, courseId;
        using (var context = _fixture.NewContext())
        {
            var school = new School { Name = "Provider school", ProviderId = seed.ProviderId, Active = true, StateId = seed.StateId, CityId = seed.CityId, AddressValue = "a" };
            context.Schools.Add(school);
            await context.SaveChangesAsync();
            var course = new SchoolCourse { Name = "Course", SchoolId = school.Id, CourseTypeId = 1, Price = 1000, Active = true };
            context.SchoolCourses.Add(course);
            await context.SaveChangesAsync();
            (schoolId, courseId) = (school.Id, course.Id);
        }

        Assert.Equal(schoolId, await ownership.SchoolOfSchoolAsync(schoolId, seed.OwnerUserId));
        Assert.Equal(schoolId, await ownership.SchoolOfCourseAsync(courseId, seed.OwnerUserId));
        Assert.Null(await ownership.SchoolOfSchoolAsync(schoolId, seed.OtherUserId, approvedOnly: false));

        var schools = new SchoolService(_fixture.NewContext(), _fixture.Mapper, FakeProxy<Application.Services.CommonSrv.PushNotificationSrv.Iface.IPushNotificationService>.Create().Instance, Microsoft.Extensions.Logging.Abstractions.NullLogger<SchoolService>.Instance);
        Assert.Contains(schoolId, schools.Search(new SchoolInputDto { OwnerUserId = seed.OwnerUserId, PageIndex = 1, PageSize = 50 }).List.Select(s => s.Id));

        // ارائه‌دهنده‌ی غیرفعال: نوشتن بسته، خواندن مال خودش آزاد
        using (var ctx = _fixture.NewContext())
            await ctx.Providers.Where(p => p.Id == seed.ProviderId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Active, false));
        Assert.Null(await ownership.SchoolOfSchoolAsync(schoolId, seed.OwnerUserId));
        Assert.Equal(schoolId, await ownership.SchoolOfSchoolAsync(schoolId, seed.OwnerUserId, approvedOnly: false));
    }

    [Fact]
    public async Task ListFilters_ByOwnerUser_CoverAllTheirCompanions_AndNobodyElses()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var first = await AddAsync(seed, approved: true);
        var second = await AddAsync(seed, approved: true);
        var stranger = _fixture.Seed();
        var foreign = await AddAsync(stranger, approved: true);

        var schools = new SchoolService(_fixture.NewContext(), _fixture.Mapper, FakeProxy<Application.Services.CommonSrv.PushNotificationSrv.Iface.IPushNotificationService>.Create().Instance, Microsoft.Extensions.Logging.Abstractions.NullLogger<SchoolService>.Instance);
        var mine = schools.Search(new SchoolInputDto { OwnerUserId = seed.OwnerUserId, PageIndex = 1, PageSize = 50 }).List.Select(s => s.Id).ToList();
        Assert.Contains(first.SchoolId, mine);
        Assert.Contains(second.SchoolId, mine);
        Assert.DoesNotContain(foreign.SchoolId, mine);

        var courses = new SchoolCourseService(_fixture.NewContext(), _fixture.Mapper);
        var myCourses = courses.Search(new SchoolCourseInputDto { OwnerUserId = seed.OwnerUserId, PageIndex = 1, PageSize = 50 }).List.Select(c => c.Id).ToList();
        Assert.Contains(first.CourseId, myCourses);
        Assert.Contains(second.CourseId, myCourses);
        Assert.DoesNotContain(foreign.CourseId, myCourses);
    }
}
