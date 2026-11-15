using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Application.Common.Enumerable.Code;
using Application.Services.CommonSrv.PushBroadcastSrv;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Application.Tests.Security;

// مخاطب پوش گروهی به تفکیک نقش ارائه‌دهنده: فقط ترجمه‌ی کوئری به SQL بررسی می‌شود (بدون اتصال به دیتابیس)
public class PushAudienceFilterTests
{
    private static string Sql(PushMessageTypeEnum type)
    {
        var options = new DbContextOptionsBuilder<Persistence.Context.DataBaseContext>()
            .UseSqlServer("Server=(local);Database=TranslationOnly;Integrated Security=true;TrustServerCertificate=true", sql => sql.UseNetTopologySuite())
            .Options;
        using var context = new Persistence.Context.DataBaseContext(options);

        // ApplyTypeFilter فقط از _context استفاده می‌کند؛ سازنده‌ی کامل (FCM، VAPID، ...) لازم نیست
        var service = (PushBroadcastService)RuntimeHelpers.GetUninitializedObject(typeof(PushBroadcastService));
        typeof(PushBroadcastService).GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(service, context);
        var method = typeof(PushBroadcastService).GetMethod("ApplyTypeFilter", BindingFlags.NonPublic | BindingFlags.Instance)!;

        var query = (IQueryable<PushSubscription>)method.Invoke(service, new object[] { context.Set<PushSubscription>().Where(x => x.IsActive), type })!;
        return query.ToQueryString();
    }

    [Theory]
    [InlineData(PushMessageTypeEnum.PushMessageType_Store, "Store")]
    [InlineData(PushMessageTypeEnum.PushMessageType_Clinic, "Clinic")]
    [InlineData(PushMessageTypeEnum.PushMessageType_Coach, "Coach")]
    [InlineData(PushMessageTypeEnum.PushMessageType_Groomer, "Groomer")]
    [InlineData(PushMessageTypeEnum.PushMessageType_Pansion, "Pansion")]
    [InlineData(PushMessageTypeEnum.PushMessageType_School, "School")]
    public void RoleAudience_TranslatesAndUsesApprovedProviderRole(PushMessageTypeEnum type, string roleLabel)
    {
        var sql = Sql(type);

        Assert.Contains("ProviderRoleAssignments", sql);
        Assert.Contains($"N'{roleLabel}'", sql);
        Assert.DoesNotContain("N'Admin'", sql);
    }

    [Fact]
    public void EachRoleAudience_UsesItsOwnRoleLabelOnly()
    {
        var clinic = Sql(PushMessageTypeEnum.PushMessageType_Clinic);
        Assert.DoesNotContain("N'Coach'", clinic);
        Assert.DoesNotContain("N'Groomer'", clinic);
    }
}
