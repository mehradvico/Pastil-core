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

        // مالک: هر دو نمایندگی‌اش را (نه فقط اولی) می‌شناسد
        Assert.Equal(approved.CompanionId, await ownership.CompanionOfSchoolAsync(approved.SchoolId, seed.OwnerUserId));
        Assert.Equal(approved.CompanionId, await ownership.CompanionOfCourseAsync(approved.CourseId, seed.OwnerUserId));
        Assert.Equal(approved.CompanionId, await ownership.CompanionOfSessionAsync(approved.SessionId, seed.OwnerUserId));

        // نمایندگی تأییدنشده: خواندن مال خودش آزاد، نوشتن بسته
        Assert.Null(await ownership.CompanionOfSchoolAsync(pending.SchoolId, seed.OwnerUserId));
        Assert.Equal(pending.CompanionId, await ownership.CompanionOfSchoolAsync(pending.SchoolId, seed.OwnerUserId, approvedOnly: false));

        // بعد از تأیید ادمین (بدون ورود دوباره) همان لحظه باز می‌شود
        using (var ctx = _fixture.NewContext())
            await ctx.Companions.Where(c => c.Id == pending.CompanionId).ExecuteUpdateAsync(s => s.SetProperty(c => c.Active, true).SetProperty(c => c.Approved, true));
        Assert.Equal(pending.CompanionId, await ownership.CompanionOfCourseAsync(pending.CourseId, seed.OwnerUserId));

        // غریبه هیچ‌چیز ندارد
        Assert.Null(await ownership.CompanionOfSchoolAsync(approved.SchoolId, seed.OtherUserId, approvedOnly: false));
        Assert.Null(await ownership.CompanionOfSessionAsync(approved.SessionId, seed.OtherUserId, approvedOnly: false));
        Assert.Null(await ownership.CompanionOfReserveAsync(123456, seed.OwnerUserId));

        // ساخت مدرسه‌ی جدید: با دو نمایندگی باید یکی را صریح بگوید
        Assert.Null(await ownership.ResolveCompanionForNewSchoolAsync(seed.OwnerUserId, null));
        Assert.Equal(approved.CompanionId, await ownership.ResolveCompanionForNewSchoolAsync(seed.OwnerUserId, approved.CompanionId));
        Assert.Null(await ownership.ResolveCompanionForNewSchoolAsync(seed.OwnerUserId, approved.CompanionId + 9999));
        Assert.Null(await ownership.ResolveCompanionForNewSchoolAsync(seed.OtherUserId, approved.CompanionId));
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
