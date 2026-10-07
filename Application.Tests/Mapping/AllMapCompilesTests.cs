using AutoMapper;
using Xunit;

namespace Application.Tests.Mapping;

// همه‌ی نگاشت‌های AutoMapper (AllMap) باید برای همه‌ی جفت‌نوع‌ها کامپایل شوند؛ خطای یک ForMember/DTO فقط وقت اجرای همان نگاشت (مثلاً افزودن به سبد) ۵۰۰ می‌شود.
public class AllMapCompilesTests
{
    [Fact]
    public void Every_mapping_plan_compiles()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile(new Application.Maping.AllMap()),
            Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        config.CompileMappings();
    }
}
