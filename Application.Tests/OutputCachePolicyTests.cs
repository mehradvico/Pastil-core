using Application.Common.Helpers;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Application.Tests;

public class OutputCachePolicyTests
{
    private static IConfiguration Config(params (string Key, string Value)[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value)))
            .Build();

    [Fact]
    public void DisableAll_without_enabled_keys_turns_every_cache_off()
    {
        var cfg = Config(("CacheSettings:DisableAll", "true"));
        Assert.False(OutputCachePolicy.IsEnabled(cfg, "ProductSearch"));
        Assert.False(OutputCachePolicy.IsEnabled(cfg, "BannerSearch"));
    }

    [Fact]
    public void Enabled_key_is_cached_even_when_DisableAll_is_true_and_others_stay_off()
    {
        var cfg = Config(("CacheSettings:DisableAll", "true"), ("CacheSettings:EnabledKeys:0", "ProductSearch"));
        Assert.True(OutputCachePolicy.IsEnabled(cfg, "ProductSearch"));
        Assert.True(OutputCachePolicy.IsEnabled(cfg, "productsearch"));
        Assert.False(OutputCachePolicy.IsEnabled(cfg, "PermissionMenu"));
        Assert.False(OutputCachePolicy.IsEnabled(cfg, "BannerSearch"));
    }

    [Fact]
    public void DisableAll_false_enables_everything()
    {
        var cfg = Config(("CacheSettings:DisableAll", "false"));
        Assert.True(OutputCachePolicy.IsEnabled(cfg, "PermissionMenu"));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(9_999, 1)]
    [InlineData(90_000, 9)]
    [InlineData(200_000, 20)]
    public void Entry_size_grows_with_response_length(int chars, int expected) =>
        Assert.Equal(expected, OutputCachePolicy.EntrySize(chars));

    [Fact]
    public void Huge_responses_are_never_stored()
    {
        Assert.True(OutputCachePolicy.ShouldStore(OutputCachePolicy.MaxCachedChars));
        Assert.False(OutputCachePolicy.ShouldStore(OutputCachePolicy.MaxCachedChars + 1));
    }

    [Fact]
    public void Checked_in_appsettings_caches_only_product_search_for_a_short_time()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "Api", "appsettings.json"))) dir = Path.GetDirectoryName(dir);
        Assert.NotNull(dir);
        var cfg = new ConfigurationBuilder().AddJsonFile(Path.Combine(dir!, "Api", "appsettings.json")).Build();
        Assert.True(OutputCachePolicy.IsEnabled(cfg, "ProductSearch"));
        Assert.False(OutputCachePolicy.IsEnabled(cfg, "PermissionMenu"));
        var seconds = cfg.GetValue<int>("CacheSettings:Durations:ProductSearch");
        Assert.InRange(seconds, 10, 120);
    }
}
