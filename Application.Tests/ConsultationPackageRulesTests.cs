using Application.Common.Enumerable;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto;
using Xunit;

namespace Application.Tests;

public class ConsultationPackageRulesTests
{
    private const int Chat = (int)OnlineSessionChannelEnum.Chat;
    private const int Phone = (int)OnlineSessionChannelEnum.Phone;

    private static ConsultationPackageItemDto Item(int channel = Chat, int duration = 30, double price = 150_000, bool active = true, string name = "مشاوره فوری") =>
        new() { ChannelId = channel, DurationMinutes = duration, Price = price, Active = active, Name = name };

    [Fact]
    public void A_valid_named_package_passes()
    {
        Assert.Equal(ConsultationPackageRules.Problem.None, ConsultationPackageRules.Validate(Item()));
        Assert.Equal(ConsultationPackageRules.Problem.None, ConsultationPackageRules.Validate(Item(Phone, 90, 0, false, "پیگیری بعد از عمل")));
    }

    [Fact]
    public void Missing_item_is_rejected()
    {
        Assert.Equal(ConsultationPackageRules.Problem.NoItem, ConsultationPackageRules.Validate(null));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(-1)]
    public void Unknown_channel_is_rejected(int channel)
    {
        Assert.Equal(ConsultationPackageRules.Problem.UnknownChannel, ConsultationPackageRules.Validate(Item(channel)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(120)]
    [InlineData(-30)]
    public void Duration_must_come_from_the_fixed_list(int duration)
    {
        Assert.Equal(ConsultationPackageRules.Problem.UnknownDuration, ConsultationPackageRules.Validate(Item(duration: duration)));
    }

    [Theory]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(45)]
    [InlineData(60)]
    [InlineData(90)]
    public void Every_fixed_duration_is_accepted(int duration)
    {
        Assert.Equal(ConsultationPackageRules.Problem.None, ConsultationPackageRules.Validate(Item(duration: duration)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("آ")]
    public void Name_is_required_and_at_least_two_characters(string name)
    {
        Assert.Equal(ConsultationPackageRules.Problem.InvalidName, ConsultationPackageRules.Validate(Item(name: name)));
    }

    [Fact]
    public void Name_longer_than_one_hundred_characters_is_rejected()
    {
        Assert.Equal(ConsultationPackageRules.Problem.InvalidName, ConsultationPackageRules.Validate(Item(name: new string('ا', 101))));
        Assert.Equal(ConsultationPackageRules.Problem.None, ConsultationPackageRules.Validate(Item(name: new string('ب', 100))));
    }

    [Fact]
    public void Description_is_optional_but_capped_at_five_hundred_characters()
    {
        var ok = Item();
        ok.Description = new string('ج', 500);
        Assert.Equal(ConsultationPackageRules.Problem.None, ConsultationPackageRules.Validate(ok));

        var tooLong = Item();
        tooLong.Description = new string('ج', 501);
        Assert.Equal(ConsultationPackageRules.Problem.DescriptionTooLong, ConsultationPackageRules.Validate(tooLong));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Negative_or_non_finite_price_is_rejected(double price)
    {
        Assert.Equal(ConsultationPackageRules.Problem.InvalidPrice, ConsultationPackageRules.Validate(Item(price: price, active: false)));
    }

    [Fact]
    public void A_zero_price_package_can_not_be_active_but_can_be_saved_inactive()
    {
        Assert.Equal(ConsultationPackageRules.Problem.ActiveWithoutPrice, ConsultationPackageRules.Validate(Item(price: 0, active: true)));
        Assert.Equal(ConsultationPackageRules.Problem.None, ConsultationPackageRules.Validate(Item(price: 0, active: false)));
    }

    [Fact]
    public void Two_named_packages_with_the_same_channel_and_duration_are_both_valid()
    {
        // برخلاف ماتریس قبلی: زیر یک کانال و یک مدت هر تعداد پکیج با نام‌های متفاوت مجاز است
        Assert.Equal(ConsultationPackageRules.Problem.None, ConsultationPackageRules.Validate(Item(name: "اورژانسی")));
        Assert.Equal(ConsultationPackageRules.Problem.None, ConsultationPackageRules.Validate(Item(name: "مشاوره تغذیه")));
    }

    [Fact]
    public void Names_are_normalized_so_arabic_letters_and_extra_spaces_do_not_create_duplicates()
    {
        Assert.Equal("مشاوره فوري".Replace('ي', 'ی'), ConsultationPackageRules.NormalizeName("  مشاوره   فوري "));
        Assert.Equal(ConsultationPackageRules.NameKey("مشاوره كودك"), ConsultationPackageRules.NameKey("مشاوره  کودک "));
        Assert.NotEqual(ConsultationPackageRules.NameKey("مشاوره کودک"), ConsultationPackageRules.NameKey("مشاوره نوزاد"));
    }

    [Fact]
    public void Description_is_trimmed_and_empty_becomes_null()
    {
        Assert.Null(ConsultationPackageRules.NormalizeDescription("   "));
        Assert.Null(ConsultationPackageRules.NormalizeDescription(null));
        Assert.Equal("توضیح", ConsultationPackageRules.NormalizeDescription("  توضیح "));
    }

    [Fact]
    public void Fixed_durations_are_the_agreed_list()
    {
        Assert.Equal(new[] { 15, 30, 45, 60, 90 }, ConsultationRules.AllowedDurations);
    }
}
