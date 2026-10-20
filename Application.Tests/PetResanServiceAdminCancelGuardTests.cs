using Xunit;

namespace Application.Tests;

/// <summary>
/// باگ ۲۰۲۶-۱۰-۱۸: ادمین فقط «لغو سفر» داشت که همان نوبت را لغو و بلافاصله نوبت هفته‌ی بعد را می‌ساخت؛ لغو خودکار
/// نوبت بدون راننده هم همین کار را می‌کرد، پس سرویس هفتگی هرگز متوقف نمی‌شد و هر روز درخواست تازه ساخته می‌شد.
/// این نگهبان مطمئن می‌شود لغو کل سرویس اول سرویس را غیرفعال می‌کند و خودش هیچ نوبت تازه‌ای نمی‌سازد.
/// </summary>
public class PetResanServiceAdminCancelGuardTests
{
    private static string Read(string relative)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Pastil.sln")))
            directory = directory.Parent;
        return File.ReadAllText(Path.Combine(directory!.FullName, relative.Replace('/', Path.DirectorySeparatorChar)));
    }

    // از امضای متد تا امضای متد بعدی (public/private) در همان کلاس
    private static string MethodBody(string source, string signature)
    {
        var start = source.IndexOf(signature, StringComparison.Ordinal);
        Assert.True(start >= 0, "method not found: " + signature);
        var ends = new[] { "\n        public ", "\n        private " }
            .Select(marker => source.IndexOf(marker, start + signature.Length, StringComparison.Ordinal))
            .Where(index => index >= 0)
            .ToList();
        return ends.Count == 0 ? source[start..] : source[start..ends.Min()];
    }

    [Fact]
    public void Stopping_a_service_deactivates_it_before_canceling_occurrences()
    {
        var source = Read("Application/Services/TripSrv/TripSrv/TripService.cs");
        var body = MethodBody(source, "private async Task<int> StopPetResanServiceAsync(");

        var deactivate = body.IndexOf("service.Active = false", StringComparison.Ordinal);
        var cancelLoop = body.IndexOf("CancelTripByAdminCoreAsync(", StringComparison.Ordinal);
        Assert.True(deactivate >= 0 && cancelLoop > deactivate, "service must be inactive before its occurrences are canceled");
        Assert.DoesNotContain("await CreateNextServiceOccurrenceAsync(", body);

        // لغو ادمین از سفر، لغو گروهی و توقف خودکار همه از همین مسیر
        Assert.Contains("StopPetResanServiceAsync(", MethodBody(source, "public async Task<BaseResultDto<int>> AdminCancelPetResanServiceAsync("));
        Assert.Contains("StopPetResanServiceAsync(", MethodBody(source, "private async Task<bool> StopServiceAfterRepeatedNoDriverAsync("));
    }

    [Fact]
    public void Auto_cancel_checks_the_no_driver_streak_before_creating_next_weeks_occurrence()
    {
        var body = MethodBody(Read("Application/Services/TripSrv/TripSrv/TripService.cs"), "private async Task AutoCancelUnassignedTripAsync(");
        Assert.Contains("!await StopServiceAfterRepeatedNoDriverAsync(trip.Id))", body);
        Assert.True(body.IndexOf("StopServiceAfterRepeatedNoDriverAsync", StringComparison.Ordinal) < body.IndexOf("await CreateNextServiceOccurrenceAsync(", StringComparison.Ordinal));
    }

    [Fact]
    public void Next_occurrence_is_never_created_for_an_inactive_service()
    {
        var source = Read("Application/Services/TripSrv/TripSrv/TripService.cs");
        var body = MethodBody(source, "private async Task CreateNextServiceOccurrenceAsync(");
        Assert.Contains("!service.Active", body);
        Assert.Contains("s.PetResanService.Active", MethodBody(source, "private async Task<int> GenerateUpcomingOccurrencesAsync("));
    }

    [Fact]
    public void Admin_can_reach_the_service_cancel_from_the_trip_controller()
    {
        var controller = Read("Api/Areas/Admin/Controllers/TripChangeStatusController.cs");
        Assert.Contains("[HttpPost(\"cancel-service\")]", controller);
        Assert.Contains("AdminCancelPetResanServiceAsync", controller);
    }
}
