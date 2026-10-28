using Application.Common.Enumerable.Code;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.SchoolSrvs.SchoolCourseSrv;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Dto;
using Application.Services.SchoolSrvs.SchoolTeachingSrv;
using Application.Services.SchoolSrvs.SchoolWaitlistSrv;
using Entities.Entities;
using Entities.Entities.SchoolField;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Application.Tests.Coach;

/// <summary>ویژگی‌های تکمیلی مدرسه روی دیتابیس واقعی (LocalDB): روزهای دوره، حضور و غیاب، کارنامه، گواهی، لیست انتظار.</summary>
public class SchoolCourseFeaturesTests : IClassFixture<CoachLocalDbFixture>
{
    private readonly CoachLocalDbFixture _fixture;

    public SchoolCourseFeaturesTests(CoachLocalDbFixture fixture) => _fixture = fixture;

    private async Task<(long SchoolId, long CourseId)> AddCourseAsync(CoachLocalDbFixture.SeedData seed, int sessionCount, int? capacity = null, bool certificate = false)
    {
        using var context = _fixture.NewContext();
        var school = new School { Name = "Sch " + Guid.NewGuid().ToString("N")[..4], ProviderId = seed.ProviderId, Active = true, StateId = seed.StateId, CityId = seed.CityId, AddressValue = "a" };
        context.Schools.Add(school);
        await context.SaveChangesAsync();
        var course = new SchoolCourse
        {
            Name = "Course", SchoolId = school.Id, CourseTypeId = (int)SchoolCourseTypeEnum.InPerson, Price = 400_000, SessionCount = sessionCount,
            Capacity = capacity, Active = true, CertificateEnabled = certificate, FullRefundHoursBeforeStart = 24, LateCancelRefundPercent = 50,
        };
        context.SchoolCourses.Add(course);
        await context.SaveChangesAsync();
        return (school.Id, course.Id);
    }

    private async Task<long> AddPaidReserveAsync(long courseId, long userId, long petId, int status = (int)SchoolReserveStatusEnum.Paid)
    {
        using var context = _fixture.NewContext();
        var pet = new UserPet { Name = "Rex", PetId = petId, UserId = userId, Size = "Small" };
        context.UserPets.Add(pet);
        await context.SaveChangesAsync();
        var reserve = new SchoolReserve
        {
            ReserveCode = "S" + Guid.NewGuid().ToString("N")[..8], SchoolCourseId = courseId, BookerId = userId, UserPetId = pet.Id,
            Price = 400_000, PaymentPrice = 400_000, IsReserved = true, StatusId = status, CreateDate = DateTime.Now, CompanionShare = 360_000, SiteShare = 40_000,
        };
        context.SchoolReserves.Add(reserve);
        await context.SaveChangesAsync();
        return reserve.Id;
    }

    [Fact]
    public async Task Schedule_requires_exactly_the_course_session_count_and_no_overlap()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var (_, courseId) = await AddCourseAsync(seed, sessionCount: 3);
        var service = new SchoolCourseService(_fixture.NewContext(), _fixture.Mapper);
        var day = DateTime.Today.AddDays(5);

        SchoolCourseScheduleDto Schedule(params (int Day, string Start, string End)[] items) => new()
        {
            SchoolCourseId = courseId,
            Sessions = items.Select(i => new SchoolCourseScheduleItemDto { SessionDate = day.AddDays(i.Day), StartTime = i.Start, EndTime = i.End }).ToList(),
        };

        Assert.False((await service.SetScheduleAsync(Schedule((0, "10:00", "11:00"), (2, "10:00", "11:00")))).IsSuccess); // ۲ از ۳
        Assert.False((await new SchoolCourseService(_fixture.NewContext(), _fixture.Mapper)
            .SetScheduleAsync(Schedule((0, "10:00", "11:00"), (0, "10:30", "11:30"), (2, "10:00", "11:00")))).IsSuccess); // تداخل
        var ok = await new SchoolCourseService(_fixture.NewContext(), _fixture.Mapper)
            .SetScheduleAsync(Schedule((0, "10:00", "11:00"), (2, "17:00", "18:00"), (4, "10:00", "11:00")));
        Assert.True(ok.IsSuccess);
        Assert.Equal(3, ok.Data.Count);

        // دوباره زدن: جلسه‌های آینده جایگزین می‌شوند، اضافه نمی‌شوند
        var again = await new SchoolCourseService(_fixture.NewContext(), _fixture.Mapper)
            .SetScheduleAsync(Schedule((1, "09:00", "10:00"), (3, "09:00", "10:00"), (5, "09:00", "10:00")));
        Assert.True(again.IsSuccess);
        using var ctx = _fixture.NewContext();
        Assert.Equal(3, await ctx.SchoolCourseSessions.CountAsync(s => s.SchoolCourseId == courseId && !s.Deleted));
    }

    [Fact]
    public async Task Attendance_progress_and_certificate_flow()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var (_, courseId) = await AddCourseAsync(seed, sessionCount: 1, certificate: true);
        long sessionId;
        using (var ctx = _fixture.NewContext())
        {
            var session = new SchoolCourseSession { SchoolCourseId = courseId, SessionDate = DateTime.Today.AddDays(1), StartTime = "10:00", EndTime = "11:00", Active = true };
            ctx.SchoolCourseSessions.Add(session);
            await ctx.SaveChangesAsync();
            sessionId = session.Id;
        }
        var present = await AddPaidReserveAsync(courseId, seed.OtherUserId, seed.PetId);
        var absent = await AddPaidReserveAsync(courseId, seed.LikerUserId, seed.PetId);
        var (push, proxy) = FakeProxy<IPushNotificationService>.Create();
        var teaching = new SchoolTeachingService(_fixture.NewContext(), push);

        // مالکِ ارائه‌دهنده مدرس است؛ غریبه نه
        Assert.True(await teaching.CanTeachCourseAsync(courseId, seed.OwnerUserId));
        Assert.False(await teaching.CanTeachCourseAsync(courseId, seed.OtherUserId));

        Assert.Equal(2, (await teaching.RosterAsync(sessionId)).Data.Count);
        Assert.True((await teaching.SaveAttendanceAsync(new SchoolAttendanceSaveDto
        {
            SchoolCourseSessionId = sessionId,
            Items = { new() { SchoolReserveId = present, Present = true }, new() { SchoolReserveId = absent, Present = false } },
        }, seed.OwnerUserId)).IsSuccess);
        Assert.Single(proxy.Calls, c => c == nameof(IPushNotificationService.SendPushAsync)); // فقط غایب پیام می‌گیرد

        var skill = (await teaching.UpsertSkillAsync(new SchoolSkillDto { SchoolCourseId = courseId, Name = "نشستن" })).Data;
        Assert.False((await teaching.SaveProgressAsync(new SchoolProgressSaveDto { SchoolReserveId = present, Items = { new() { SchoolCourseSkillId = skill.Id, Score = 9 } } }, seed.OwnerUserId)).IsSuccess);
        Assert.True((await teaching.SaveProgressAsync(new SchoolProgressSaveDto { SchoolReserveId = present, Items = { new() { SchoolCourseSkillId = skill.Id, Score = 4 } } }, seed.OwnerUserId)).IsSuccess);

        var report = (await teaching.ReportAsync(present)).Data;
        Assert.Equal(1, report.AttendedCount);
        Assert.Equal(100, report.AttendancePercent);
        Assert.Equal(4, report.Skills.Single().LatestScore);

        // گواهی فقط بعد از «کامل‌شده»
        Assert.False((await teaching.IssueCertificateAsync(present, seed.OwnerUserId)).IsSuccess);
        using (var ctx = _fixture.NewContext())
            await ctx.SchoolReserves.Where(r => r.Id == present).ExecuteUpdateAsync(s => s.SetProperty(r => r.StatusId, (int)SchoolReserveStatusEnum.Complete));
        var cert = await new SchoolTeachingService(_fixture.NewContext(), push).IssueCertificateAsync(present, seed.OwnerUserId);
        Assert.True(cert.IsSuccess);
        Assert.StartsWith("PSC-", cert.Data.Code);
        var again = await new SchoolTeachingService(_fixture.NewContext(), push).IssueCertificateAsync(present, seed.OwnerUserId);
        Assert.Equal(cert.Data.Code, again.Data.Code); // تکرار = همان گواهی
        Assert.True((await new SchoolTeachingService(_fixture.NewContext(), push).VerifyCertificateAsync(cert.Data.Code.ToLowerInvariant())).IsSuccess);
    }

    [Fact]
    public async Task Waitlist_only_when_full_and_notified_when_a_seat_opens()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var (_, courseId) = await AddCourseAsync(seed, sessionCount: 1, capacity: 1);
        using (var ctx = _fixture.NewContext())
        {
            ctx.SchoolCourseSessions.Add(new SchoolCourseSession { SchoolCourseId = courseId, SessionDate = DateTime.Today.AddDays(3), StartTime = "10:00", EndTime = "11:00", Active = true });
            await ctx.SaveChangesAsync();
        }
        long waitingPetId;
        using (var ctx = _fixture.NewContext())
        {
            var pet = new UserPet { Name = "W", PetId = seed.PetId, UserId = seed.LikerUserId, Size = "Small" };
            ctx.UserPets.Add(pet);
            await ctx.SaveChangesAsync();
            waitingPetId = pet.Id;
        }
        var waitlist = new SchoolWaitlistService(_fixture.NewContext());

        // هنوز جا دارد ⇒ لیست انتظار نه
        Assert.False((await waitlist.JoinAsync(seed.LikerUserId, new SchoolWaitlistJoinDto { SchoolCourseId = courseId, UserPetId = waitingPetId })).IsSuccess);

        var taken = await AddPaidReserveAsync(courseId, seed.OtherUserId, seed.PetId);
        var joined = await new SchoolWaitlistService(_fixture.NewContext()).JoinAsync(seed.LikerUserId, new SchoolWaitlistJoinDto { SchoolCourseId = courseId, UserPetId = waitingPetId });
        Assert.True(joined.IsSuccess);
        Assert.Equal(1, joined.Data.Position);
        Assert.False((await new SchoolWaitlistService(_fixture.NewContext()).JoinAsync(seed.LikerUserId, new SchoolWaitlistJoinDto { SchoolCourseId = courseId, UserPetId = waitingPetId })).IsSuccess);

        // جا خالی شد ⇒ پوش به صف
        using (var ctx = _fixture.NewContext())
            await ctx.SchoolReserves.Where(r => r.Id == taken).ExecuteUpdateAsync(s => s.SetProperty(r => r.IsCancel, true));
        var (push, proxy) = FakeProxy<IPushNotificationService>.Create();
        await SchoolWaitlistNotifier.NotifySeatOpenAsync(_fixture.NewContext(), push, courseId, DateTime.Now);
        Assert.Single(proxy.Calls, c => c == nameof(IPushNotificationService.SendPushAsync));
    }
}
