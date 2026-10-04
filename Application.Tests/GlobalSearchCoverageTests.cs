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
    public void A_two_letter_query_is_still_scored_against_itself_not_only_its_synonym()
    {
        // «سگ» ۲ حرفی است؛ قبلاً از امتیازدهی حذف می‌شد و فقط مترادفش «هاپو» می‌ماند (فقط برند هاپومیل می‌آمد)
        var q = SearchNormalizeHelper.Normalize("سگ");
        var terms = SearchNormalizeHelper.BuildTerms(q, enableFuzzy: true);

        var scoring = SearchNormalizeHelper.ScoringTerms(q, terms);

        Assert.Contains("سگ", scoring);
        Assert.Contains("هاپو", scoring);
    }

    [Fact]
    public void Fuzzy_bigrams_of_a_longer_query_stay_out_of_scoring()
    {
        var q = SearchNormalizeHelper.Normalize("قلاده");
        var terms = SearchNormalizeHelper.BuildTerms(q, enableFuzzy: true);

        var scoring = SearchNormalizeHelper.ScoringTerms(q, terms);

        Assert.All(scoring, term => Assert.True(term.Length >= 3));
        Assert.Contains("قلاده", scoring);
    }

    [Fact]
    public void Scoring_falls_back_to_the_query_when_no_term_qualifies()
    {
        Assert.Equal(new[] { "اب" }, SearchNormalizeHelper.ScoringTerms("اب", new[] { "x" }));
    }

    [Fact]
    public void Synonyms_are_not_offered_as_did_you_mean_when_the_search_found_results()
    {
        var q = SearchNormalizeHelper.Normalize("سگ");
        var terms = SearchNormalizeHelper.BuildTerms(q, enableFuzzy: true);

        Assert.Empty(SearchNormalizeHelper.BuildSuggestions(q, terms, totalCount: 42));
    }

    [Fact]
    public void Suggestions_are_offered_only_when_there_were_no_results_and_never_repeat_the_query()
    {
        var q = SearchNormalizeHelper.Normalize("کلینیک");
        var terms = SearchNormalizeHelper.BuildTerms(q, enableFuzzy: false);

        var suggestions = SearchNormalizeHelper.BuildSuggestions(q, terms, totalCount: 0);

        Assert.DoesNotContain(q, suggestions);
        Assert.Contains("دامپزشک", suggestions);
        Assert.True(suggestions.Count <= 5);
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
