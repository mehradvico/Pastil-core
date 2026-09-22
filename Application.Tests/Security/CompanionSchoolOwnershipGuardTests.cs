using Xunit;

namespace Application.Tests.Security;

/// <summary>
/// ناحیه‌ی Companion فقط با [Authorize] محافظت می‌شود؛ پس ثابت‌ماندن این قفل‌ها در کنترلرهای مدرسه ضروری است
/// (کمیسیون دوره، فیلدهای تأیید مدرسه، مالکیت جزئیات ثبت‌نام).
/// </summary>
public class CompanionSchoolOwnershipGuardTests
{
    private static string Read(string name)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Pastil.sln")))
            directory = directory.Parent;
        var root = directory?.FullName ?? throw new InvalidOperationException("Pastil.sln not found.");
        return File.ReadAllText(Path.Combine(root, "Api", "Areas", "Companion", "Controllers", name));
    }

    [Fact]
    public void Course_commission_is_never_taken_from_the_companion()
    {
        var source = Read("SchoolCourseController.cs");
        Assert.Contains("dto.CommissionPercent = 0;", source);
        Assert.Contains("dto.CommissionPercent = existing.Data.CommissionPercent;", source);
        // مالکیت ویرایش روی دوره‌ی ذخیره‌شده است، نه SchoolId بدنه
        Assert.Contains("dto.SchoolId = existing.Data.SchoolId;", source);
    }

    [Fact]
    public void School_update_restores_approval_and_visibility_fields()
    {
        var source = Read("SchoolController.cs");
        foreach (var field in new[] { "Approve", "ApprovalValue", "Active", "ShowToSite", "Suggested" })
            Assert.Contains($"dto.{field} = existing.Data.{field};", source);
    }

    [Fact]
    public void School_reserve_detail_checks_ownership()
    {
        var source = Read("SchoolReserveController.cs");
        Assert.Contains("SchoolCourse?.School?.CompanionId != companionId.Value", source);
    }

    [Fact]
    public void Push_broadcast_never_falls_back_to_everyone_for_an_unknown_audience()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Pastil.sln")))
            directory = directory.Parent;
        var source = File.ReadAllText(Path.Combine(directory!.FullName, "Application", "Services", "CommonSrv", "PushBroadcastSrv", "PushBroadcastService.cs"));

        Assert.Contains("ResolveAudienceAsync", source);
        Assert.Contains("query.Where(x => false)", source);
    }
}
