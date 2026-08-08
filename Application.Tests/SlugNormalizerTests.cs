using System.ComponentModel.DataAnnotations;
using Entities.Entities.CommonField;
using Xunit;

namespace Application.Tests;

public class SlugNormalizerTests
{
    [Theory]
    [InlineData("SDFSD dsf fsdf", "sdfsd-dsf-fsdf")]
    [InlineData("  Hello   World  ", "hello-world")]
    [InlineData("A___B---C", "a-b-c")]
    [InlineData("Hello !@#$%^&*() World", "hello-world")]
    [InlineData("Product 123", "product-123")]
    public void Normalize_CreatesCanonicalEnglishSlug(string label, string expected)
    {
        var result = SlugNormalizer.Normalize(label);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("محصول")]
    [InlineData("Product محصول")]
    [InlineData("café")]
    public void Normalize_RejectsNonAsciiLetters(string label)
    {
        Assert.Throws<ValidationException>(() => SlugNormalizer.Normalize(label));
    }

    [Theory]
    [InlineData("!@#$%^&*()")]
    [InlineData("   ---   ")]
    public void Normalize_RejectsValueWithoutEnglishLettersOrDigits(string label)
    {
        Assert.Throws<ValidationException>(() => SlugNormalizer.Normalize(label));
    }

    [Fact]
    public void Normalize_AllowsEmptyOptionalLabel()
    {
        Assert.Null(SlugNormalizer.Normalize(null));
    }
}
