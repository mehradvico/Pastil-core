using System.Text.RegularExpressions;
using Xunit;

namespace Application.Tests;

/// <summary>نحوه ارائه (آنلاین/مرکز/در محل) روی پکیج تعیین می‌شود و حالت‌های خدمت از پکیج‌های فعال مشتق می‌شود؛ این نگهبان جلوی برگشت تصادفی را می‌گیرد.</summary>
public class CompanionPackageModesGuardTests
{
    private static string Read(string relative)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Pastil.sln")))
            directory = directory.Parent;
        return File.ReadAllText(Path.Combine(directory!.FullName, relative.Replace('/', Path.DirectorySeparatorChar)));
    }

    [Fact]
    public void Every_package_change_resynchronises_the_service_modes()
    {
        var source = Read("Application/Services/CompanionSrvs/CompanionAssistancePackageSrv/CompanionAssistancePackageService.cs");

        // ثبت، ویرایش، فعال/غیرفعال‌سازی توسط ادمین و حذف
        Assert.Equal(4, Regex.Matches(source, @"SyncServiceModes\(").Count - 1); // -1 = تعریف خود متد
        Assert.Contains("public override BaseResultDto DeleteDto(long id)", source);
    }

    [Fact]
    public void Package_modes_no_longer_have_to_be_pre_selected_on_the_service()
    {
        var package = Read("Application/Services/CompanionSrvs/CompanionAssistancePackageSrv/CompanionAssistancePackageService.cs");
        var service = Read("Application/Services/CompanionSrvs/CompanionAssistanceSrv/CompanionAssistanceService.cs");

        Assert.DoesNotContain("serviceTypeIds", package);
        Assert.DoesNotContain("SelectAtLeastOneType", service);
    }

    [Fact]
    public void Editing_a_service_without_modes_keeps_the_existing_modes()
    {
        var service = Read("Application/Services/CompanionSrvs/CompanionAssistanceSrv/CompanionAssistanceService.cs");

        Assert.Contains("var replaceModes = dto.CompanionAssistanceTypeIds != null && dto.CompanionAssistanceTypeIds.Any();", service);
    }

    [Fact]
    public void Webapp_step_one_no_longer_asks_for_the_delivery_modes()
    {
        var page = Read("../webapp/app/pages/companionProfile/companionAssistance/insert[prId].vue");

        Assert.DoesNotContain("انتخاب حداقل یک روش ارائه خدمت الزامی است", page);
        Assert.DoesNotContain("@click=\"toggleServiceType(t.id)\"", page);
    }
}
