using Application.Common.Enumerable;
using Application.Common.Helpers;
using Xunit;

namespace Application.Tests;

// جستجوی سراسری: پوشش دسته‌های تازه‌ اضافه‌شده (مدرسه، دوره، پکیج مشاوره) و مترادف‌های دسته‌بندی‌های خدماتی.
public class GlobalSearchCoverageTests
{
    [Theory]
    [InlineData("مهد پت")]
    [InlineData("تربیت سگ")]
    [InlineData("آرایشگاه")]
    [InlineData("مدرسه")]
    [InlineData("دوره")]
    [InlineData("مشاوره")]
    public void Category_terms_expand_to_related_synonyms(string query)
    {
        var normalized = SearchNormalizeHelper.Normalize(query);
        var terms = SearchNormalizeHelper.BuildTerms(normalized, enableFuzzy: false);
        Assert.NotEmpty(terms);
    }

    [Fact]
    public void Grooming_synonym_maps_both_directions()
    {
        var fromShop = SearchNormalizeHelper.BuildTerms(SearchNormalizeHelper.Normalize("آرایشگاه"), enableFuzzy: false);
        var fromWord = SearchNormalizeHelper.BuildTerms(SearchNormalizeHelper.Normalize("گرومینگ"), enableFuzzy: false);
        Assert.Contains("گرومینگ", fromShop);
        Assert.Contains(SearchNormalizeHelper.Normalize("آرایشگاه"), fromWord);
    }

    [Fact]
    public void Daycare_query_maps_to_daycare_and_pansion_terms()
    {
        var terms = SearchNormalizeHelper.BuildTerms(SearchNormalizeHelper.Normalize("مهد"), enableFuzzy: false);
        Assert.Contains(terms, t => t.Contains("مهد") || t.Replace(" ", "") == "دیکر");
    }

    [Fact]
    public void Every_search_item_type_has_a_distinct_value()
    {
        var values = System.Enum.GetValues<SearchItemType>();
        var distinct = new System.Collections.Generic.HashSet<int>();
        foreach (var value in values)
            Assert.True(distinct.Add((int)value), $"Duplicate SearchItemType value for {value}");
    }

    [Fact]
    public void New_result_types_exist_on_the_enum()
    {
        Assert.True(System.Enum.IsDefined(typeof(SearchItemType), SearchItemType.School));
        Assert.True(System.Enum.IsDefined(typeof(SearchItemType), SearchItemType.SchoolCourse));
        Assert.True(System.Enum.IsDefined(typeof(SearchItemType), SearchItemType.ConsultationPackage));
    }
}
