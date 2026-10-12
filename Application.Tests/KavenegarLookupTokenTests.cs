using Application.Services.Setting.SmsSrv;
using Xunit;

namespace Application.Tests;

public class KavenegarLookupTokenTests
{
    [Fact]
    public void Spaces_and_newlines_become_single_underscores()
    {
        Assert.Equal("پلی_کلینیک_پورشعبان", KavenegarLookupToken.Normalize(" پلی  کلینیک \n پورشعبان "));
    }

    [Fact]
    public void Zwnj_and_other_invisible_marks_never_reach_kavenegar()
    {
        // نیم‌فاصله در عمل همیشه ۴۳۱ گرفته است
        Assert.Equal("پت_شاپ_نبات", KavenegarLookupToken.Normalize("پت‌شاپ ‌نبات"));
        Assert.Equal("کلینیک_دکتر", KavenegarLookupToken.Normalize("‏کلینیک‍ دکتر﻿"));
        Assert.Equal("a_b", KavenegarLookupToken.Normalize("a__ _b"));
    }

    [Fact]
    public void Proven_separators_are_kept()
    {
        Assert.Equal("1405/07/20", KavenegarLookupToken.Normalize("1405/07/20"));
        Assert.Equal("ORD-14050718-1332-1966", KavenegarLookupToken.Normalize("ORD-14050718-1332-1966"));
    }

    [Fact]
    public void Token_is_capped_at_100_characters_without_trailing_underscore()
    {
        Assert.Equal(KavenegarLookupToken.MaxLength, KavenegarLookupToken.Normalize(new string('ا', 250)).Length);
        Assert.False(KavenegarLookupToken.Normalize(new string('ا', 99) + " ب").EndsWith('_'));
    }

    [Fact]
    public void Empty_token_is_skipped()
    {
        Assert.Null(KavenegarLookupToken.Normalize("   "));
        Assert.Null(KavenegarLookupToken.Normalize("‌"));
        Assert.Null(KavenegarLookupToken.Normalize(null));
    }
}
