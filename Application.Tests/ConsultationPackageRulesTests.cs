using Application.Common.Enumerable;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Application.Tests;

public class ConsultationPackageRulesTests
{
    private static ConsultationPackageItemDto Item(int channel, int duration, double price, bool active) =>
        new() { ChannelId = channel, DurationMinutes = duration, Price = price, Active = active };

    private const int Chat = (int)OnlineSessionChannelEnum.Chat;
    private const int Phone = (int)OnlineSessionChannelEnum.Phone;

    [Fact]
    public void Valid_items_pass()
    {
        var items = new List<ConsultationPackageItemDto> { Item(Chat, 30, 150_000, true), Item(Phone, 60, 0, false) };
        Assert.Equal(ConsultationPackageRules.Problem.None, ConsultationPackageRules.Validate(items));
    }

    [Fact]
    public void Empty_or_missing_items_are_rejected()
    {
        Assert.Equal(ConsultationPackageRules.Problem.NoItems, ConsultationPackageRules.Validate(null));
        Assert.Equal(ConsultationPackageRules.Problem.NoItems, ConsultationPackageRules.Validate(new List<ConsultationPackageItemDto>()));
    }

    [Theory]
    [InlineData(0, 30)]   // کانال ناشناخته
    [InlineData(5, 30)]
    [InlineData(1, 45)]   // مدت غیرمجاز
    [InlineData(1, 0)]
    [InlineData(1, 120)]
    public void Unknown_channel_or_duration_is_rejected(int channel, int duration)
    {
        var items = new List<ConsultationPackageItemDto> { Item(channel, duration, 1000, true) };
        Assert.Equal(ConsultationPackageRules.Problem.UnknownCombination, ConsultationPackageRules.Validate(items));
    }

    [Fact]
    public void Duplicate_cells_are_rejected()
    {
        var items = new List<ConsultationPackageItemDto> { Item(Chat, 30, 1000, true), Item(Chat, 30, 2000, false) };
        Assert.Equal(ConsultationPackageRules.Problem.DuplicateCombination, ConsultationPackageRules.Validate(items));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Negative_or_non_finite_price_is_rejected(double price)
    {
        var items = new List<ConsultationPackageItemDto> { Item(Chat, 30, price, false) };
        Assert.Equal(ConsultationPackageRules.Problem.InvalidPrice, ConsultationPackageRules.Validate(items));
    }

    [Fact]
    public void A_zero_price_package_can_not_be_active()
    {
        var items = new List<ConsultationPackageItemDto> { Item(Chat, 30, 0, true) };
        Assert.Equal(ConsultationPackageRules.Problem.ActiveWithoutPrice, ConsultationPackageRules.Validate(items));
    }

    [Fact]
    public void Matrix_always_has_eight_cells_and_defaults_to_inactive_zero_price()
    {
        var stored = new List<ConsultationPackageItemDto> { new() { Id = 7, ChannelId = Chat, DurationMinutes = 60, Price = 90_000, Active = true } };

        var matrix = ConsultationPackageRules.BuildMatrix(stored);

        Assert.Equal(8, matrix.Count);
        Assert.Equal(8, matrix.Select(m => (m.ChannelId, m.DurationMinutes)).Distinct().Count());
        var saved = matrix.Single(m => m.ChannelId == Chat && m.DurationMinutes == 60);
        Assert.Equal((7L, 90_000d, true), (saved.Id, saved.Price, saved.Active));
        Assert.All(matrix.Where(m => m.Id == 0), m => Assert.False(m.Active || m.Price != 0));
    }

    [Fact]
    public void Matrix_of_nothing_is_the_full_default_grid()
    {
        var matrix = ConsultationPackageRules.BuildMatrix(null);
        Assert.Equal(8, matrix.Count);
        Assert.All(matrix, m => Assert.True(m.Id == 0 && !m.Active && m.Price == 0));
    }

    [Fact]
    public void Allowed_combinations_are_four_channels_by_two_durations()
    {
        Assert.Equal(8, ConsultationRules.AllCombinations().Count());
        Assert.True(ConsultationRules.IsValidCombination(Phone, 30));
        Assert.False(ConsultationRules.IsValidCombination(Phone, 45));
    }
}
