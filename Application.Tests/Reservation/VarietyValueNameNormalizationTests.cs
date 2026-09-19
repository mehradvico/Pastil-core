using Application.Common.Helpers;
using Xunit;

namespace Application.Tests.Reservation;

/// <summary>
/// یکتایی نام مقدار تنوع (VarietyItemService.NameIsUnique) بر پایه‌ی همین نرمال‌سازی است؛ این تست‌ها تضمین می‌کنند
/// تفاوت‌های صرفاً املایی «تکراری» حساب می‌شوند و مقدارهای واقعاً متفاوت نه.
/// </summary>
public class VarietyValueNameNormalizationTests
{
    private static string N(string value) => SearchNormalizeHelper.NormalizeNoSpace(value);

    [Theory]
    [InlineData("قرمز", "قرمز ")]              // فاصله‌ی اضافه
    [InlineData("قرمز", "قرم‌ز")]              // نیم‌فاصله
    [InlineData("مشکی", "مشکي")]               // ی عربی
    [InlineData("کرم", "كرم")]                 // ک عربی
    [InlineData("۱۲ عدد", "12 عدد")]           // ارقام فارسی/لاتین
    [InlineData("12 عدد", "12‌عدد")]           // نیم‌فاصله بین عدد و کلمه
    [InlineData("Red", "red")]                // حروف بزرگ/کوچک
    [InlineData("آبی", "ابی")]                 // آ = ا (رفتار فعلی helper، تصمیم ۹-۷)
    public void SpellingOnlyDifferences_AreTreatedAsTheSameValue(string first, string second)
    {
        Assert.Equal(N(first), N(second));
    }

    [Theory]
    [InlineData("قرمز", "سبز")]
    [InlineData("قرمز", "سرخ")]               // مترادف = کار ادمین، نه سیستم
    [InlineData("1 کیلو", "2 کیلو")]
    public void ReallyDifferentValues_AreNotTreatedAsDuplicates(string first, string second)
    {
        Assert.NotEqual(N(first), N(second));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("---")]
    public void NamesWithoutAnyLetterOrDigit_NormalizeToEmpty_SoTheyAreRejected(string name)
    {
        Assert.True(string.IsNullOrEmpty(N(name)));
    }
}
