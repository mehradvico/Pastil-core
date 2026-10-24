using Application.Services.CoachSrvs.CoachCatalogSrv;
using Application.Services.CoachSrvs.CoachCatalogSrv.Dto;
using Application.Services.CoachSrvs.CoachSrv;
using Entities.Entities.CoachField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static Application.Tests.Coach.CoachReserveTestKit;

namespace Application.Tests.Coach;

/// <summary>گزینه‌ی «آنلاین» برای مشتری فقط وقتی دیده می‌شود که حداقل یک بسته‌ی فعال در حالت آنلاین رزروشدنی باشد.</summary>
public class CoachOnlineFlagTests : IClassFixture<CoachLocalDbFixture>
{
    private readonly CoachLocalDbFixture _fixture;

    public CoachOnlineFlagTests(CoachLocalDbFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task PublicServiceOnline_FollowsRealOnlineBookablePackages()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture, serviceOnline: true);

        // بسته‌ی ساخته‌شده‌ی دنیای تست بدون ردیف قیمت است (همه‌ی حالت‌ها) ⇒ آنلاین رزروشدنی
        using (var context = _fixture.NewContext())
        {
            var view = (await new CoachCoreService(context, _fixture.Mapper, NullLogger<CoachCoreService>.Instance).FindPublicAsync(world.CoachId)).Data;
            Assert.True(view.ServiceOnline);
        }

        long onlineOnly, inPersonOnly;
        using (var context = _fixture.NewContext())
        {
            // همه‌ی بسته‌های فعلی غیرفعال؛ فقط یک بسته‌ی «حضوری»
            await context.CoachServicePackages.Where(p => p.CoachService.CoachId == world.CoachId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Active, false));
            var catalog = new CoachCatalogService(context, _fixture.Mapper, NullLogger<CoachCatalogService>.Instance);
            inPersonOnly = (await catalog.AddPackageAsync(new CoachServicePackageInputDto
            {
                CoachServiceId = world.ServiceId, Name = "InPerson", Price = 100_000,
                Types = new List<CoachPackageTypeDto> { new() { Mode = (int)CoachServiceModeEnum.InPerson, Price = 100_000, PrePaymentPrice = 0 } },
            }, null)).Data.Id;
        }
        using (var context = _fixture.NewContext())
        {
            var view = (await new CoachCoreService(context, _fixture.Mapper, NullLogger<CoachCoreService>.Instance).FindPublicAsync(world.CoachId)).Data;
            Assert.False(view.ServiceOnline); // پرچم ذخیره‌شده true است ولی بسته‌ی آنلاین نیست
        }
        using (var context = _fixture.NewContext())
        {
            Assert.True((await context.Coaches.AsNoTracking().FirstAsync(c => c.Id == world.CoachId)).ServiceOnline); // پرچم ذخیره‌شده دست‌نخورده
            var catalog = new CoachCatalogService(context, _fixture.Mapper, NullLogger<CoachCatalogService>.Instance);
            onlineOnly = (await catalog.AddPackageAsync(new CoachServicePackageInputDto
            {
                CoachServiceId = world.ServiceId, Name = "Online", Price = 200_000,
                Types = new List<CoachPackageTypeDto> { new() { Mode = (int)CoachServiceModeEnum.Online, Price = 200_000, PrePaymentPrice = 50_000 } },
            }, null)).Data.Id;
        }
        using (var context = _fixture.NewContext())
        {
            var view = (await new CoachCoreService(context, _fixture.Mapper, NullLogger<CoachCoreService>.Instance).FindPublicAsync(world.CoachId)).Data;
            Assert.True(view.ServiceOnline);
        }
        Assert.NotEqual(onlineOnly, inPersonOnly);
    }
}
