using System.Text.RegularExpressions;
using Xunit;

namespace Application.Tests.Security;

/// <summary>
/// نگهبان‌های ساختاری برای SQL injection، XSS، CSRF و SSRF (فقط جلوی برگشت تصادفی قفل‌های بازبینی را می‌گیرند).
/// </summary>
public class InjectionAndBrowserSurfaceGuardTests
{
    private static string Backend => FindRoot("Pastil.sln");
    private static string Workspace => Path.GetFullPath(Path.Combine(Backend, ".."));

    // متغیرهایی که مجاز است داخل SQL رشته‌ای (interpolated) بنشینند؛ همه‌ی آن‌ها long/int داخلی‌اند، نه ورودی متنی کاربر.
    private static readonly HashSet<string> AllowedSqlInterpolations = new(StringComparer.Ordinal)
    {
        "id", "storeId", "categoryids"
    };

    [Fact]
    public void Interpolated_sql_strings_only_embed_reviewed_numeric_variables()
    {
        var offenders = new List<string>();
        var sqlLike = new Regex(@"\$@?""[^""\r\n]*(SELECT\s|UPDATE\s|DELETE\s+FROM|INSERT\s+INTO|WITH\s+\w+\s*AS)", RegexOptions.IgnoreCase);

        foreach (var path in Directory.EnumerateFiles(Path.Combine(Backend, "Application"), "*.cs", SearchOption.AllDirectories)
                     .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                                 && !p.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}")))
        {
            foreach (var line in File.ReadLines(path))
            {
                if (!sqlLike.IsMatch(line)) continue;
                // FromSqlInterpolated پارامتری می‌شود و امن است
                if (line.Contains("FromSqlInterpolated") || line.Contains("ExecuteSqlInterpolated")) continue;

                foreach (Match hole in Regex.Matches(line, @"\{(\w+)\}"))
                {
                    if (!AllowedSqlInterpolations.Contains(hole.Groups[1].Value))
                        offenders.Add($"{Path.GetFileName(path)}: {{{hole.Groups[1].Value}}}");
                }
            }
        }

        Assert.True(offenders.Count == 0,
            "Interpolated SQL embeds a variable that has not been reviewed as numeric — use Dapper/EF parameters instead: "
            + string.Join(", ", offenders.Distinct()));
    }

    [Fact]
    public void ExecuteSqlRaw_calls_use_positional_parameters_not_string_building()
    {
        var offenders = new List<string>();
        foreach (var path in Directory.EnumerateFiles(Path.Combine(Backend, "Application"), "*.cs", SearchOption.AllDirectories)
                     .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")))
        {
            foreach (var line in File.ReadLines(path))
            {
                if (!Regex.IsMatch(line, @"(ExecuteSqlRaw|FromSqlRaw)(Async)?\(")) continue;
                if (line.Contains("$\"") || Regex.IsMatch(line, @"\(""[^""]*""\s*\+")) offenders.Add(Path.GetFileName(path));
            }
        }

        Assert.True(offenders.Count == 0, "Raw SQL built by interpolation/concatenation: " + string.Join(", ", offenders.Distinct()));
    }

    [Fact]
    public void School_session_meeting_url_must_be_http_or_https()
    {
        var source = File.ReadAllText(Path.Combine(Backend, "Application", "Services", "SchoolSrvs", "SchoolCourseSrv", "SchoolCourseService.cs"));

        Assert.Contains("meetingUri.Scheme == System.Uri.UriSchemeHttps", source);
        Assert.Contains("meetingUri.Scheme == System.Uri.UriSchemeHttp", source);
    }

    [Fact]
    public void Webapp_bff_rejects_cross_site_state_changing_requests()
    {
        var source = File.ReadAllText(Path.Combine(Workspace, "webapp", "app", "server", "middleware", "csrf-guard.ts"));

        Assert.Contains("sec-fetch-site", source);
        Assert.Contains("cross-site", source);
        Assert.Contains("'POST', 'PUT', 'PATCH', 'DELETE'", source);
    }

    [Fact]
    public void Website_api_gateway_cannot_be_steered_outside_the_public_api_surface()
    {
        var source = File.ReadAllText(Path.Combine(Workspace, "website", "server", "api", "[...path].ts"));

        Assert.Contains("segment === '..'", source);
        Assert.Contains("startsWith('/api/')", source);
        Assert.Contains("'admin'", source);
    }

    [Fact]
    public void Website_html_sanitizer_escapes_unterminated_tags()
    {
        var source = File.ReadAllText(Path.Combine(Workspace, "website", "app", "composables", "useSanitizedHtml.ts"));

        Assert.Contains("|</gi", source);
        Assert.Contains("&lt;", source);
    }

    [Fact]
    public void Website_json_ld_is_serialised_with_the_script_safe_helper()
    {
        var pages = Path.Combine(Workspace, "website", "app");
        var offenders = Directory.EnumerateFiles(pages, "*.vue", SearchOption.AllDirectories)
            .Where(p => Regex.IsMatch(File.ReadAllText(p), @"innerHTML:\s*(computed\(\s*\(\)\s*=>\s*)?\s*JSON\.stringify\("))
            .Select(Path.GetFileName)
            .ToList();

        Assert.True(offenders.Count == 0, "JSON-LD built with raw JSON.stringify (use safeJsonLd): " + string.Join(", ", offenders));
    }

    [Fact]
    public void Website_json_ld_helper_really_escapes_script_breakout_characters()
    {
        var source = File.ReadAllText(Path.Combine(Workspace, "website", "app", "utils", "safeJsonLd.ts"));

        // JS source must contain a two-backslash escape ("\\u003c"), otherwise the replacement is a no-op that writes "<" back
        Assert.Contains("\"\\\\u003c\"", source);
        Assert.Contains("\"\\\\u003e\"", source);
        Assert.Contains("\"\\\\u0026\"", source);
        Assert.DoesNotContain('\u2028', source);
        Assert.DoesNotContain('\u2029', source);
    }

    private static string FindRoot(string marker)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, marker)))
            directory = directory.Parent;

        return directory?.FullName ?? throw new InvalidOperationException($"{marker} not found above the test output directory.");
    }
}
