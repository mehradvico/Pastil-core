using System.Collections.Generic;
using System.Linq;
using Application.Common.Helpers;
using Application.Services.CommonSrv.SearchSrv;
using Application.Services.ProductSrvs.ProductSrv;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Xunit;

namespace Application.Tests;

// جستجوی هوشمند: کاربر اسم را هر جور می‌شنود می‌نویسد؛ باید همان محصول را بگیرد ولی چیز بی‌ربط نگیرد.
public class SmartSearchTests
{
    private static readonly SearchVocabulary Vocabulary = SearchVocabulary.Build(new[]
    {
        "غذای خشک سگ رویال کنین ماکسی ادالت 15 کیلوگرم", "Royal Canin Maxi Adult",
        "غذای خشک سگ رویال کنین ماکسی ادالت 4 کیلوگرم",
        "غذای خشک گربه رویال کنین گاسترو اینتستینال", "Royal Canin Gastro Intestinal",
        "کنسرو گربه گاسترو اینتستینال رویال کنین",
        "غذای خشک گربه جوسرا کته", "Josera Catelux",
        "شامپو سگ خوش‌بو آرایش پت", "کاسه استیل سگ", "کیسه خاک گربه",
        "غذای تر گربه ویسکاس", "تور ماهیگیری", "قلاده چرمی سگ", "قلاده طبی",
        "ویتامین مولتی سگ و گربه", "خاک گربه سالم", "اسباب بازی سلام",
        "تشویقی دنتال سگ", "شیر خشک گربه", "بیسکویت سگ"
    });

    private static SearchWordPlan Word(string query) => Vocabulary.Plan(query).Words.Single();

    private static bool Finds(string query, string token) =>
        Word(query).Tokens.Contains(token) || Word(query).Word == token;

    [Theory]
    [InlineData("مکسی", "ماکسی")]
    [InlineData("گسترو", "گاسترو")]
    [InlineData("ادلت", "ادالت")]
    [InlineData("رویل", "رویال")]
    [InlineData("کانین", "کنین")]
    [InlineData("ویتمین", "ویتامین")]
    [InlineData("بسکویت", "بیسکویت")]
    public void Missing_or_extra_vowel_still_finds_the_word(string typed, string inCatalog)
    {
        Assert.True(Finds(typed, inCatalog));
        Assert.False(Word(typed).Unmatched);
    }

    [Theory]
    [InlineData("ادولت", "ادالت")]   // مصوت اشتباه
    [InlineData("قلاذه", "قلاده")]   // حرف اشتباه
    [InlineData("جوصرا", "جوسرا")]   // حرف هم‌صدا
    [InlineData("اینتستینل", "اینتستینال")]
    public void Typos_are_corrected_only_to_catalog_words(string typed, string inCatalog)
    {
        var plan = Vocabulary.Plan(typed);
        Assert.True(Finds(typed, inCatalog), $"{typed} → {string.Join(",", plan.Words.Single().Tokens)}");
    }

    [Theory]
    [InlineData("maxi", "ماکسی")]
    [InlineData("gastro", "گاسترو")]
    [InlineData("roayl", "royal")]
    public void Latin_spelling_reaches_the_persian_or_latin_name(string typed, string inCatalog)
    {
        // «maxi» و «gastro» مستقیم در نام دوم هستند؛ پس خودشان کافی‌اند
        var word = Word(typed);
        Assert.False(word.Unmatched);
        if (typed == "roayl") Assert.Contains(inCatalog, word.Tokens);
    }

    [Fact]
    public void Latin_word_missing_from_catalog_is_transliterated()
    {
        var vocabulary = SearchVocabulary.Build(new[] { "غذای سگ رویال کنین ماکسی ادالت", "کنسرو گاسترو" });
        Assert.Contains("ماکسی", vocabulary.Plan("maxi").Words.Single().Tokens);
        Assert.Contains("رویال", vocabulary.Plan("royal").Words.Single().Tokens);
        Assert.Contains("کنین", vocabulary.Plan("canin").Words.Single().Tokens);
        Assert.Contains("ادالت", vocabulary.Plan("adult").Words.Single().Tokens);
        Assert.Contains("گاسترو", vocabulary.Plan("gastro").Words.Single().Tokens);
    }

    [Theory]
    [InlineData("کاسه", "کیسه")]   // دو کلمه‌ی متفاوت؛ جابه‌جایی مصوت قبول نیست
    [InlineData("کیسه", "کاسه")]
    [InlineData("سالم", "سلام")]
    [InlineData("تر", "تور")]      // کلمه‌ی کوتاه گسترش نمی‌یابد
    [InlineData("شیر", "شامپو")]
    [InlineData("سگ", "سلام")]
    public void Different_real_words_are_not_mixed(string typed, string other)
    {
        Assert.DoesNotContain(other, Word(typed).Tokens);
        Assert.Null(Word(typed).Corrected);
    }

    [Fact]
    public void Known_word_is_never_replaced_or_fuzzed()
    {
        var word = Word("قلاده");
        Assert.False(word.Unmatched);
        Assert.Null(word.Corrected);
        Assert.Empty(word.Tokens);
        Assert.Null(Vocabulary.Plan("قلاده چرمی").CorrectedQuery);
    }

    [Fact]
    public void Gibberish_is_ignored_only_next_to_a_strong_word()
    {
        var plan = Vocabulary.Plan("رویال زقثضصخ");
        Assert.False(plan.Words[0].Unmatched);
        Assert.True(plan.Words[1].Unmatched);
        Assert.Equal(new[] { "زقثضصخ" }, plan.IgnoredWords);
        Assert.Equal("رویال", plan.CorrectedQuery);

        // «کت» فقط تکه‌ای وسط کلمه‌های دیگر است؛ با کنار گذاشتن «هپی» نتیجه بی‌ربط می‌شد
        var weak = SearchVocabulary.Build(new[] { "پاکت خاک", "کتان" }).Plan("هپی کت");
        Assert.Empty(weak.IgnoredWords);
        Assert.Null(weak.CorrectedQuery);

        // همه‌ی کلمه‌ها ناشناخته: چیزی کنار گذاشته نمی‌شود (نتیجه صادقانه خالی است)
        Assert.Empty(Vocabulary.Plan("زقثضصخ ضصثقف").IgnoredWords);
    }

    [Theory]
    [InlineData("مینی")]   // کلمه‌ی ۴ حرفیِ ناشناخته حدس زده نمی‌شود
    [InlineData("پلان")]
    [InlineData("کاسم")]
    public void Short_unknown_words_are_not_guessed(string typed)
    {
        var vocabulary = SearchVocabulary.Build(new[] { "میشی", "متنی", "میلی", "پلاک", "اعلان", "کاسه", "کاست" });
        var word = vocabulary.Plan(typed).Words.Single();
        Assert.Empty(word.Tokens);
        Assert.True(word.Unmatched);
    }

    [Fact]
    public void Zh_and_j_sound_the_same_in_brand_names()
    {
        Assert.Contains("جوسرا", Word("ژوسرا").Tokens);
    }

    [Fact]
    public void Corrected_query_is_reported_only_when_something_changed()
    {
        Assert.Equal("ماکسی ادالت", Vocabulary.Plan("مکسی ادولت").CorrectedQuery);
        Assert.Null(Vocabulary.Plan("ماکسی ادالت").CorrectedQuery);
    }

    [Fact]
    public void Joined_words_are_split()
    {
        var word = Word("خاکگربه");
        Assert.Equal("خاک گربه", word.Corrected);
        Assert.Contains(word.LikeTerms, term => term.Length == 2);
    }

    [Fact]
    public void Wrong_keyboard_layout_is_detected()
    {
        // «v,dhg» همان «رویال» با کیبورد انگلیسی است
        var plan = Vocabulary.Plan("v,dhg");
        Assert.Equal("رویال", plan.CorrectedQuery);
        Assert.Equal("royal", Vocabulary.Plan("قخغشم").CorrectedQuery);
    }

    [Fact]
    public void Stored_spelling_is_used_for_database_like()
    {
        // کاربر «ارایش» می‌نویسد، در دیتابیس «آرایش» ذخیره شده
        var word = Word("ارایش");
        Assert.False(word.Unmatched);
        Assert.Contains(word.LikeTerms, term => term[0] == "آرایش");
    }

    [Fact]
    public void Like_terms_always_start_with_the_users_own_word()
    {
        var word = Word("مکسی");
        Assert.Equal("مکسی", word.LikeTerms[0][0]);
        Assert.Contains(word.LikeTerms, term => term[0] == "ماکسی");
    }

    [Fact]
    public void Empty_vocabulary_keeps_the_old_behaviour()
    {
        var plan = SearchVocabulary.Empty.Plan("غذاي خشك ۸۵");
        Assert.Equal(new[] { "غذای", "خشک", "85" }, plan.Words.Select(word => word.Word));
        Assert.All(plan.Words, word => Assert.False(word.Unmatched));
        Assert.Contains(plan.Words[2].LikeTerms, term => term[0] == "۸۵");
        Assert.Null(plan.CorrectedQuery);
    }

    [Fact]
    public void Global_terms_put_real_equivalents_before_generic_terms()
    {
        var plan = Vocabulary.Plan("مکسی ادالت");
        var terms = SmartSearchTerms.Build(plan.NormalizedQuery, plan, enableFuzzy: false);
        Assert.Equal("مکسی ادالت", terms[0]);
        Assert.Contains("ماکسی ادالت", terms);
        Assert.Contains("ماکسی", terms);

        var groups = SmartSearchTerms.WordGroups(plan);
        Assert.Equal(2, groups.Count);
        Assert.Contains("ماکسی", groups[0]);
    }

    [Fact]
    public void Product_name_filter_translates_to_parameterized_sql()
    {
        var options = new DbContextOptionsBuilder<DataBaseContext>()
            .UseSqlServer("Server=.;Database=none;Encrypt=false", o => o.UseNetTopologySuite())
            .Options;
        using var context = new DataBaseContext(options);

        var sql = context.Products
            .Where(ProductService.NameMatchesAny(new List<string[]> { new[] { "مکسی" }, new[] { "ماکسی" }, new[] { "خاک", "گربه" } }))
            .Select(s => s.Id)
            .ToQueryString();

        Assert.Contains("REPLACE", sql);
        Assert.Contains(" OR ", sql);
        Assert.Contains(" AND ", sql);
        Assert.Contains("DECLARE", sql); // مقدارها پارامترند، نه متن ثابت داخل کوئری
    }
}
