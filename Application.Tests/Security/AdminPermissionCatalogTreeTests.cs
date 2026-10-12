using System.Collections;
using System.Reflection;
using Xunit;

namespace Application.Tests.Security;

/// <summary>
/// درخت دسترسی‌ها «گروه ← بخش ← زیربخش» است: هر کنترلر کمکی باید زیر یک بخش اصلیِ همان گروه بنشیند
/// و خودِ آن بخش اصلی نباید زیربخش کس دیگری باشد (عمق ثابت، تا صفحه‌ی دسترسی‌ها و منو کامل لود شوند).
/// تکرار یک کنترلر در کاتالوگ هم همین‌جا (هنگام ساخت دیکشنری) خطا می‌دهد.
/// </summary>
public class AdminPermissionCatalogTreeTests
{
    [Fact]
    public void Every_child_controller_hangs_under_a_top_level_section_of_the_same_group()
    {
        var catalogType = Type.GetType("Utility.Reflection.AdminPermissionCatalog, Utility", throwOnError: true)!;
        var controllersField = catalogType.GetField("Controllers", BindingFlags.NonPublic | BindingFlags.Static)!;
        var controllers = (IDictionary)controllersField.GetValue(null)!;

        var definitions = controllers.Keys.Cast<string>().ToDictionary(
            key => key,
            key => controllers[key]!,
            StringComparer.OrdinalIgnoreCase);

        string Host(object definition) => (string)definition.GetType().GetProperty("HostController")!.GetValue(definition)!;
        object Group(object definition) => definition.GetType().GetProperty("Parent")!.GetValue(definition)!;

        var problems = new List<string>();
        foreach (var (name, definition) in definitions)
        {
            var host = Host(definition);
            if (host == null)
                continue;

            if (!definitions.TryGetValue(host, out var hostDefinition))
                problems.Add($"{name}: host '{host}' is not in the catalog");
            else if (Host(hostDefinition) != null)
                problems.Add($"{name}: host '{host}' is itself a child controller");
            else if (!Equals(Group(hostDefinition), Group(definition)))
                problems.Add($"{name}: host '{host}' is in a different group");
        }

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }
}
