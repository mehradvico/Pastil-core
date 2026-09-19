using System.Text.RegularExpressions;
using Xunit;

namespace Application.Tests.Security;

/// <summary>
/// همگام‌سازی permissionها (پنل ← «همگام‌سازی») کلاً متوقف می‌شود اگر حتی یک کنترلر Admin در
/// AdminPermissionCatalog نباشد («کنترلرهای بدون گروه دسترسی پیدا شدند»). این تست همان خطا را
/// قبل از deploy می‌گیرد: هر *Controller.cs داخل Api/Areas/Admin/Controllers باید در کاتالوگ بیاید.
/// </summary>
public class AdminPermissionCatalogCoverageTests
{
    [Fact]
    public void Every_admin_controller_is_listed_in_the_permission_catalog()
    {
        var backendRoot = FindBackendRoot();
        var controllersDirectory = Path.Combine(backendRoot, "Api", "Areas", "Admin", "Controllers");
        var catalogSource = File.ReadAllText(Path.Combine(backendRoot, "Utility", "Reflection", "AdminPermissionCatalog.cs"));

        var cataloged = Regex.Matches(catalogSource, "\"(?<name>[A-Za-z0-9_]+)\"")
            .Select(match => match.Groups["name"].Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // کنترلرهای «Compile Remove» در Api.csproj (مثل *Language) اصلاً ساخته نمی‌شوند؛ پس در sync هم دیده نمی‌شوند.
        var apiProject = File.ReadAllText(Path.Combine(backendRoot, "Api", "Api.csproj"));
        var excludedFiles = Regex.Matches(apiProject, "<Compile Remove=\"(?<path>[^\"]+)\"")
            .Select(match => Path.GetFullPath(Path.Combine(backendRoot, "Api", match.Groups["path"].Value.Replace('\\', Path.DirectorySeparatorChar))))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // نام مسیر/دسترسی = نام «کلاس» (نه لزوماً نام فایل)؛ مثلاً CargoUpdateStatusController.cs کلاس UpdateCargoStatusController است.
        var missing = Directory.EnumerateFiles(controllersDirectory, "*Controller.cs", SearchOption.AllDirectories)
            .Where(path => !excludedFiles.Contains(Path.GetFullPath(path)))
            .SelectMany(path => Regex.Matches(File.ReadAllText(path), @"public\s+(?:sealed\s+)?class\s+(?<name>\w+)Controller")
                .Select(match => match.Groups["name"].Value))
            .Where(name => !cataloged.Contains(name))
            .Distinct()
            .OrderBy(name => name)
            .ToList();

        Assert.True(
            missing.Count == 0,
            "Admin controllers missing from AdminPermissionCatalog.cs (permission sync will fail): " + string.Join(", ", missing));
    }

    private static string FindBackendRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Pastil.sln")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new InvalidOperationException("Pastil.sln not found above the test output directory.");
    }
}
