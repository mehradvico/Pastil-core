using Application.Services.ProductSrvs.WalletSrv;
using Application.Tests.Clinic;
using Entities.Entities;
using Entities.Entities.PastilClubField;
using Xunit;

namespace Application.Tests;

// موجودی نمایشی کیف پول = نقد + اعتبار فعال کلاب با دامنه‌ی Global (هدیه‌ی پروفایل پت). باگ ۱۴۰۵/۰۸/۲۴: بعد از «فعال کن» کیف پول صفر می‌ماند.
public class WalletDisplayBalanceTests : IClassFixture<ClinicLocalDbFixture>
{
    private readonly ClinicLocalDbFixture _db;
    public WalletDisplayBalanceTests(ClinicLocalDbFixture db) => _db = db;

    [Fact]
    public async Task Balance_AddsOnlyActiveGlobalClubCredit_ToCash()
    {
        if (!_db.Available) return;
        var userId = _db.Seed().OwnerUserId;
        var future = DateTimeOffset.UtcNow.AddDays(10);
        using (var context = _db.NewContext())
        {
            context.Wallets.Add(new Wallet { UserId = userId, Amount = 30_000, IsIncrease = true, Name = "cash", CreateDate = DateTime.Now });
            context.ClubPromotionalWalletCredits.AddRange(
                Credit(userId, 200_000, ClubRewardTargetTypeEnum.Global, future, ClubPromotionalCreditStatusEnum.Active),
                Credit(userId, 50_000, ClubRewardTargetTypeEnum.Store, future, ClubPromotionalCreditStatusEnum.Active),      // فقط یک فروشگاه
                Credit(userId, 70_000, ClubRewardTargetTypeEnum.Global, DateTimeOffset.UtcNow.AddDays(-1), ClubPromotionalCreditStatusEnum.Active), // منقضی
                Credit(userId, 90_000, ClubRewardTargetTypeEnum.Global, future, ClubPromotionalCreditStatusEnum.Consumed));
            context.SaveChanges();
        }

        using var read = _db.NewContext();
        var balance = await new WalletService(read, null, null, _db.Mapper, null, null).GetDisplayBalanceAsync(userId);

        Assert.Equal(30_000, balance.Cash);
        Assert.Equal(200_000, balance.Credit);
        Assert.Equal(230_000, balance.Total);
        Assert.NotNull(balance.CreditExpiresAt);
    }

    [Fact]
    public async Task Balance_WithoutCredit_IsJustCash()
    {
        if (!_db.Available) return;
        var userId = _db.Seed().OwnerUserId;
        using var read = _db.NewContext();
        var balance = await new WalletService(read, null, null, _db.Mapper, null, null).GetDisplayBalanceAsync(userId);
        Assert.Equal(0, balance.Total);
        Assert.Null(balance.CreditExpiresAt);
    }

    private static ClubPromotionalWalletCredit Credit(long userId, decimal amount, ClubRewardTargetTypeEnum scope, DateTimeOffset expires, ClubPromotionalCreditStatusEnum status) => new()
    {
        UserId = userId, RewardRedemptionId = Random.Shared.NextInt64(1_000_000, long.MaxValue), OriginalAmount = amount, RemainingAmount = status == ClubPromotionalCreditStatusEnum.Consumed ? 0 : amount,
        ServiceScopeType = scope, ServiceScopeId = scope == ClubRewardTargetTypeEnum.Global ? null : 5,
        ExpiresAt = expires, Status = status, CreateDate = DateTime.UtcNow
    };
}
