using Application.Services.CompanionSrvs.CompanionReserveDebtSrv;
using System;
using Xunit;

namespace Application.Tests;

public class CompanionDebtReminderRulesTests
{
    private static DateTime At(int hour, int minute = 0) => new(2026, 10, 4, hour, minute, 0);

    [Theory]
    [InlineData(0, null)]
    [InlineData(9, null)]    // قبل از اولین بازه
    [InlineData(10, 1)]
    [InlineData(14, 1)]
    [InlineData(15, 2)]
    [InlineData(19, 2)]
    [InlineData(20, 3)]
    [InlineData(21, 3)]
    [InlineData(22, null)]   // ساعت سکوت
    [InlineData(23, null)]
    public void Reminder_slot_is_the_latest_slot_whose_hour_has_passed_and_none_at_night(int hour, int? expected)
        => Assert.Equal(expected, CompanionReserveDebtRules.CurrentReminderSlot(At(hour, 30)));

    [Fact]
    public void There_are_exactly_three_reminder_slots_per_day()
    {
        var slots = new System.Collections.Generic.HashSet<int>();
        for (var h = 0; h < 24; h++)
        {
            var s = CompanionReserveDebtRules.CurrentReminderSlot(At(h));
            if (s.HasValue) slots.Add(s.Value);
        }

        Assert.Equal(new[] { 1, 2, 3 }, slots.OrderBy(x => x));
    }

    [Fact]
    public void Reminder_key_is_unique_per_day_and_slot_but_stable_within_a_slot()
    {
        // job هر ۱۵ دقیقه اجرا می‌شود؛ همه‌ی اجراهای یک بازه باید یک کلید بدهند تا فقط یک پوش برود
        Assert.Equal(CompanionReserveDebtRules.ReminderKey(At(10, 0), 1), CompanionReserveDebtRules.ReminderKey(At(11, 45), 1));
        Assert.NotEqual(CompanionReserveDebtRules.ReminderKey(At(10), 1), CompanionReserveDebtRules.ReminderKey(At(15), 2));
        Assert.NotEqual(CompanionReserveDebtRules.ReminderKey(At(10), 1), CompanionReserveDebtRules.ReminderKey(At(10).AddDays(1), 1));
        Assert.Equal("debt:20261004-2", CompanionReserveDebtRules.ReminderKey(At(16), 2));
    }

    [Fact]
    public void Collected_key_is_per_reserve()
    {
        Assert.Equal("debtpaid:42", CompanionReserveDebtRules.CollectedKey(42));
        Assert.NotEqual(CompanionReserveDebtRules.CollectedKey(1), CompanionReserveDebtRules.CollectedKey(2));
    }

    [Theory]
    [InlineData(100_000, 100_000, true)]
    [InlineData(100_000, 100_000.005, true)]   // تلورانس ۰٫۰۱ مثل پرداخت دستی
    [InlineData(100_000, 100_000.5, false)]
    [InlineData(0, 1, false)]
    [InlineData(250_000, 100_000, true)]
    public void Wallet_covers_the_debt_only_when_the_balance_is_enough(double balance, double amount, bool expected)
        => Assert.Equal(expected, CompanionReserveDebtRules.WalletCovers(balance, amount));
}
