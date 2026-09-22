using Xunit;

namespace Application.Tests;

/// <summary>
/// «اپراتور خدمت را کامل کرد» باید واقعاً منتظر تأیید صریح کاربر بماند (نه این‌که همان لحظه به‌صورت خودکار
/// UserResponse=true بشود، وگرنه دکمه‌ی «بله تایید می‌کنم» روی پوش و بخش تأیید در وب‌اپ هیچ‌وقت دیده نمی‌شوند)
/// و پوش «PushCompleteReserveUser» باید همان دکمه را داشته باشد. تست‌های سطح سورس مثل بقیه‌ی این پروژه.
/// </summary>
public class CompanionReserveOperatorConfirmPushTests
{
    private static string Root
    {
        get
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Pastil.sln")))
                directory = directory.Parent;
            return directory?.FullName ?? throw new InvalidOperationException("Pastil.sln not found.");
        }
    }

    private static string Read(params string[] relativeParts) =>
        File.ReadAllText(Path.Combine(new[] { Root }.Concat(relativeParts).ToArray()));

    [Fact]
    public void Completing_a_reserve_leaves_user_response_pending_for_explicit_confirmation()
    {
        var source = Read("Application", "Services", "CompanionSrvs", "CompanionReserveSrv", "CompanionReserveService.cs");
        Assert.Contains("item.UserResponse = dto.OperatorStateId == (long)CompanionReserveOperatorStateEnum.OperatorState_Cancelled", source);
        Assert.DoesNotContain("item.UserResponse = true;", source);
    }

    [Fact]
    public void The_completion_push_carries_a_confirm_action_button()
    {
        var source = Read("Application", "Services", "CommonSrv", "PushNotificationSrv", "PushNotificationService.cs");
        Assert.Contains("pattern.PushTypeId == (long)PushTypeEnum.PushCompleteReserveUser", source);
        Assert.Contains("new PushActionDto { Action = \"confirmReserve\", Title = \"بله تایید می‌کنم\" }", source);
    }

    [Fact]
    public void Service_worker_maps_the_confirm_action_to_a_confirm_query_param()
    {
        var source = Read("..", "webapp", "app", "service-worker", "sw.js");
        Assert.Contains("event.action === 'confirmReserve'", source);
        Assert.Contains("url.searchParams.set('confirm', '1')", source);
    }
}
