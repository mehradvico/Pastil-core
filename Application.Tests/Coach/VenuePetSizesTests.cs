using Application.Common.Helpers;
using Application.Services.CoachSrvs.CoachCatalogSrv;
using Application.Services.CoachSrvs.CoachCatalogSrv.Dto;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static Application.Tests.Coach.CoachReserveTestKit;

namespace Application.Tests.Coach;

/// <summary>چند سایز برای بسته‌ی خدمت (مربی/کلینیک/آرایشگاه): ذخیره به‌صورت «Small,Medium»، هر سه = همه (null).</summary>
public class VenuePetSizesTests
{
    [Theory]
    [InlineData(null, null, true, null)]
    [InlineData("", null, true, null)]
    [InlineData("small", null, true, "Small")]
    [InlineData("Large,small", null, true, "Small,Large")]
    [InlineData("Small,Medium,Large", null, true, null)] // هر سه = همه
    [InlineData("Small,Small", null, true, "Small")]
    [InlineData("Tiny", null, false, null)]
    [InlineData("ignored", "Medium|Large", true, "Medium,Large")] // لیست بر رشته مقدم است
    [InlineData(null, "Small|Medium|Large", true, null)]
    [InlineData(null, "Small|Huge", false, null)]
    public void TryNormalizeSet(string? csv, string? list, bool ok, string? expected)
    {
        var items = list?.Split('|').ToList();
        Assert.Equal(ok, VenuePetSizes.TryNormalizeSet(csv, items, out var normalized));
        if (ok)
            Assert.Equal(expected, normalized);
    }

    [Theory]
    [InlineData(null, "Small", true)]
    [InlineData(null, null, true)]
    [InlineData("Small,Medium", "medium", true)]
    [InlineData("Small,Medium", "Large", false)]
    [InlineData("Small", null, false)]
    public void Matches(string? stored, string? petSize, bool expected) =>
        Assert.Equal(expected, VenuePetSizes.Matches(stored, petSize));
}

public class VenuePetSizesServiceTests : IClassFixture<CoachLocalDbFixture>
{
    private readonly CoachLocalDbFixture _fixture;

    public VenuePetSizesServiceTests(CoachLocalDbFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task MultiSizePackage_SavedFilteredAndBookable()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture, petSize: "Medium");
        using var context = _fixture.NewContext();
        var catalog = new CoachCatalogService(context, _fixture.Mapper, NullLogger<CoachCatalogService>.Instance);

        var package = await catalog.AddPackageAsync(new CoachServicePackageInputDto
        {
            CoachServiceId = world.ServiceId, Name = "SmallMedium", Price = 100_000, PetSizes = new List<string> { "medium", "Small" },
        }, null);
        Assert.True(package.IsSuccess);
        Assert.Equal("Small,Medium", package.Data.PetSize);
        Assert.Equal(new[] { "Small", "Medium" }, package.Data.PetSizes);

        Assert.False((await catalog.AddPackageAsync(new CoachServicePackageInputDto
        {
            CoachServiceId = world.ServiceId, Name = "Bad", Price = 100_000, PetSizes = new List<string> { "Huge" },
        }, null)).IsSuccess);

        // فیلتر عمومی بر اساس سایز پت
        var forMedium = await catalog.ListPublicAsync(world.CoachId, "Medium");
        Assert.Contains(forMedium.Data.SelectMany(s => s.Packages), p => p.Id == package.Data.Id);
        var forLarge = await catalog.ListPublicAsync(world.CoachId, "Large");
        Assert.DoesNotContain(forLarge.Data.SelectMany(s => s.Packages), p => p.Id == package.Data.Id);

        // رزرو برای پت متوسط پذیرفته می‌شود
        var input = Input(world, Tomorrow(10));
        input.CoachServicePackageId = package.Data.Id;
        Assert.True((await NewReserveService(_fixture).CreateAsync(input, world.BookerId)).IsSuccess);

        // ویرایش به هر سه سایز = همه
        Assert.True((await catalog.UpdatePackageAsync(new CoachServicePackageInputDto
        {
            Id = package.Data.Id, Name = "All", Price = 100_000, PetSizes = new List<string> { "Small", "Medium", "Large" },
        }, null)).IsSuccess);
        var all = (await catalog.ListPublicAsync(world.CoachId, "Large")).Data.SelectMany(s => s.Packages).Single(p => p.Id == package.Data.Id);
        Assert.Null(all.PetSize);
        Assert.Empty(all.PetSizes);
    }
}
