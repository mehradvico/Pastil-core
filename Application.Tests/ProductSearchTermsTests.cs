using System.Linq;
using Application.Common.Helpers;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Xunit;

namespace Application.Tests;

// جستجوی نام محصول: متن کپی‌شده از اکسل با «ي/ك» عربی باید مثل تایپ فارسی عمل کند.
public class ProductSearchTermsTests
{
    [Fact]
    public void Arabic_letters_become_persian_words()
    {
        Assert.Equal(new[] { "غذای", "خشک", "گربه", "رویال", "کنین" },
            SearchNormalizeHelper.AllWordTerms("غذاي خشك گربه رويال كنين"));
    }

    [Fact]
    public void Terms_are_unique_capped_and_empty_for_punctuation()
    {
        Assert.Equal(new[] { "سگ" }, SearchNormalizeHelper.AllWordTerms("سگ  سگ"));
        Assert.Equal(8, SearchNormalizeHelper.AllWordTerms("a b c d e f g h i j").Length);
        Assert.Empty(SearchNormalizeHelper.AllWordTerms(" *** "));
    }

    [Fact]
    public void Digits_are_available_in_both_forms()
    {
        Assert.Equal(new[] { "85" }, SearchNormalizeHelper.AllWordTerms("۸۵"));
        Assert.Equal("۸۵", SearchNormalizeHelper.ToPersianDigits("85"));
    }

    [Fact]
    public void Name_filter_translates_to_sql()
    {
        var options = new DbContextOptionsBuilder<DataBaseContext>()
            .UseSqlServer("Server=.;Database=none;Encrypt=false", o => o.UseNetTopologySuite())
            .Options;
        using var context = new DataBaseContext(options);
        var term = "رویال";

        var sql = context.Products
            .Where(s => (s.Name + " " + (s.SecondName ?? "")).Replace("ي", "ی").Replace("ك", "ک").Contains(term))
            .Select(s => s.Id)
            .ToQueryString();

        Assert.Contains("REPLACE", sql);
        Assert.Contains("COALESCE", sql);
    }
}
