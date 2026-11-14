using Application.Services.Accounting.UserPetSrv;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.PastilClubSrvs.BenefitSrv;
using Application.Services.PastilClubSrvs.RewardOfferSrv;
using Application.Services.PastilClubSrvs.RewardRedemptionSrv;
using Application.Tests.Clinic;
using Entities.Entities;
using Entities.Entities.PastilClubField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Persistence.Context;
using Xunit;

namespace Application.Tests;

// هدیه‌ی ۲۰۰ هزار تومانی تکمیل پروفایل اولین پت در کلاب، روی LocalDB واقعی (ایندکس یکتا، تراکنش Serializable، قیدها)
public class PetProfileClubGiftTests : IClassFixture<ClinicLocalDbFixture>
{
    private readonly ClinicLocalDbFixture _db;
    public PetProfileClubGiftTests(ClinicLocalDbFixture db) => _db = db;

    private long EnsureTemplate()
    {
        using var context = _db.NewContext();
        var existing = context.ClubRewardTemplates.FirstOrDefault(t => t.Name == PetProfileClubGift.TemplateName);
        if (existing != null)
            return existing.Id;
        var template = new ClubRewardTemplate
        {
            Name = PetProfileClubGift.TemplateName, Title = "gift", ShortDescription = "s", Description = "d", Terms = "t",
            RewardType = ClubRewardTypeEnum.PromotionalWalletCredit, ApplicationMethod = ClubRewardApplicationMethodEnum.ProductOrder,
            PointCost = 0, ExpirationType = ClubRewardExpirationTypeEnum.ThirtyDays, BenefitValue = 200_000,
            FundingType = ClubRewardFundingTypeEnum.Pastil, IsAutomationAllowed = false, IsManualAllowed = false, Active = true,
            NotificationLevel = ClubRewardNotificationLevelEnum.VeryHighValue, CreateDate = DateTime.UtcNow
        };
        context.ClubRewardTemplates.Add(template);
        context.SaveChanges(); // قید CK_ClubRewardTemplate_PointCost حالا >= 0 است
        return template.Id;
    }

    private PetProfileClubGift NewGift(DataBaseContext context, out FakeProxy<IPushNotificationService> push)
    {
        var (instance, proxy) = FakeProxy<IPushNotificationService>.Create();
        push = proxy;
        return new PetProfileClubGift(context, instance, new Application.Services.PastilClubSrvs.PetProfileSrv.ClubPetProfileCompletionService(), NullLogger<PetProfileClubGift>.Instance);
    }

    private (long UserId, long FirstPetId, long SecondPetId) SeedUserWithTwoPets(bool firstDeleted = false)
    {
        var seed = _db.Seed();
        using var context = _db.NewContext();
        var first = new UserPet { Name = "a", UserId = seed.OwnerUserId, PetId = seed.PetId, Active = true, Deleted = firstDeleted, Birthday = DateTime.Today.AddYears(-1) };
        context.UserPets.Add(first);
        context.SaveChanges();
        var second = new UserPet { Name = "b", UserId = seed.OwnerUserId, PetId = seed.PetId, Active = true, Birthday = DateTime.Today.AddYears(-1) };
        context.UserPets.Add(second);
        context.SaveChanges();
        return (seed.OwnerUserId, first.Id, second.Id);
    }

    private List<ClubRewardOffer> Offers(long userId, long templateId)
    {
        using var context = _db.NewContext();
        return context.ClubRewardOffers.AsNoTracking().Where(o => o.UserId == userId && o.RewardTemplateId == templateId).ToList();
    }

    [Fact]
    public async Task OnlyFirstPet_GetsGift_Once_With30DayExpiry_AndPush()
    {
        if (!_db.Available) return;
        var templateId = EnsureTemplate();
        var (userId, firstPetId, secondPetId) = SeedUserWithTwoPets();

        using (var context = _db.NewContext())
            Assert.False(await NewGift(context, out _).GrantAsync(userId, secondPetId)); // پت دوم
        FakeProxy<IPushNotificationService> push;
        var before = DateTimeOffset.UtcNow;
        using (var context = _db.NewContext())
            Assert.True(await NewGift(context, out push).GrantAsync(userId, firstPetId));
        Assert.Contains("SendPushAsync", push.Calls);
        using (var context = _db.NewContext())
            Assert.False(await NewGift(context, out _).GrantAsync(userId, firstPetId)); // ویرایش دوباره

        var offer = Assert.Single(Offers(userId, templateId));
        Assert.Equal(ClubRewardOfferStatusEnum.Approved, offer.Status);
        Assert.Equal(0, offer.PointCostSnapshot);
        Assert.InRange(offer.ExpiresAt, before.AddDays(30).AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(30).AddMinutes(1));
    }

    [Fact]
    public async Task DeletedFirstPet_NewPetDoesNotGetGift()
    {
        if (!_db.Available) return;
        var templateId = EnsureTemplate();
        var (userId, _, secondPetId) = SeedUserWithTwoPets(firstDeleted: true);
        using (var context = _db.NewContext())
            Assert.False(await NewGift(context, out _).GrantAsync(userId, secondPetId));
        Assert.Empty(Offers(userId, templateId));
    }

    [Fact]
    public async Task UsedOrExpiredOffer_StillBlocksSecondGift()
    {
        if (!_db.Available) return;
        var templateId = EnsureTemplate();
        var (userId, firstPetId, _) = SeedUserWithTwoPets();
        using (var context = _db.NewContext())
            await NewGift(context, out _).GrantAsync(userId, firstPetId);
        using (var context = _db.NewContext())
        {
            var offer = context.ClubRewardOffers.AsTracking().Single(o => o.UserId == userId && o.RewardTemplateId == templateId);
            offer.ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1);
            context.SaveChanges();
        }
        using (var context = _db.NewContext())
            Assert.False(await NewGift(context, out _).GrantAsync(userId, firstPetId));
        Assert.Single(Offers(userId, templateId));
    }

    [Fact]
    public async Task ConcurrentSaves_CreateOnlyOneGift()
    {
        if (!_db.Available) return;
        var templateId = EnsureTemplate();
        var (userId, firstPetId, _) = SeedUserWithTwoPets();

        var results = await Task.WhenAll(Enumerable.Range(0, 6).Select(async _ =>
        {
            using var context = _db.NewContext();
            return await NewGift(context, out var _).GrantAsync(userId, firstPetId);
        }));

        Assert.Equal(1, results.Count(r => r));
        Assert.Single(Offers(userId, templateId));
    }

    [Fact]
    public async Task Activate_CreatesGlobal200kCredit_ExpiringWithTheOffer_EvenWithoutPointsOrWithDebt()
    {
        if (!_db.Available) return;
        var templateId = EnsureTemplate();
        var (userId, firstPetId, _) = SeedUserWithTwoPets();
        using (var context = _db.NewContext())
            await NewGift(context, out _).GrantAsync(userId, firstPetId);
        var offer = Offers(userId, templateId).Single();

        // کاربر هنوز حساب امتیاز ندارد
        using (var context = _db.NewContext())
        {
            var service = new ClubRewardRedemptionService(context, new ClubRewardEligibilityService(context), new ClubRewardBenefitFactory(context));
            var result = await service.RedeemAsync(userId, offer.Id);
            Assert.True(result.IsSuccess, string.Join(",", (result.Messages ?? new()).Select(m => m.Item1 + m.Item2)));
            Assert.Equal(0, result.Data.PointSpent);
        }
        using (var context = _db.NewContext())
        {
            var credit = Assert.Single(context.ClubPromotionalWalletCredits.AsNoTracking().Where(c => c.UserId == userId).ToList());
            Assert.Equal(200_000m, credit.OriginalAmount);
            Assert.Equal(200_000m, credit.RemainingAmount);
            Assert.Equal(ClubRewardTargetTypeEnum.Global, credit.ServiceScopeType);
            Assert.Equal(offer.ExpiresAt, credit.ExpiresAt);
            Assert.Equal(ClubRewardOfferStatusEnum.Redeemed, context.ClubRewardOffers.Single(o => o.Id == offer.Id).Status);
        }

        // دوباره «فعال کن»: اعتبار دوم ساخته نمی‌شود
        using (var context = _db.NewContext())
        {
            var service = new ClubRewardRedemptionService(context, new ClubRewardEligibilityService(context), new ClubRewardBenefitFactory(context));
            await service.RedeemAsync(userId, offer.Id);
        }
        using (var context = _db.NewContext())
            Assert.Single(context.ClubPromotionalWalletCredits.AsNoTracking().Where(c => c.UserId == userId).ToList());
    }

    private async Task<PetProfileGiftStatusDto> Status(long userId)
    {
        using var context = _db.NewContext();
        var result = await NewGift(context, out _).GetStatusAsync(userId);
        Assert.True(result.IsSuccess);
        return result.Data;
    }

    [Fact]
    public async Task Status_FollowsTheWholeJourney_AndHidesBannerAfterGiftEvenIfPetDeleted()
    {
        if (!_db.Available) return;
        EnsureTemplate();
        var seed = _db.Seed();
        var userId = seed.OwnerUserId;

        var noPet = await Status(userId);
        Assert.Equal(PetProfileGiftStatusEnum.NoPet, noPet.Status);
        Assert.True(noPet.ShowBanner);
        Assert.Equal(200_000m, noPet.Amount);

        long petId;
        using (var context = _db.NewContext())
        {
            var pet = new UserPet { Name = "a", UserId = userId, PetId = seed.PetId, Active = true, Birthday = DateTime.Today.AddYears(-1) };
            context.UserPets.Add(pet);
            context.SaveChanges();
            petId = pet.Id;
        }
        var incomplete = await Status(userId);
        Assert.Equal(PetProfileGiftStatusEnum.Incomplete, incomplete.Status);
        Assert.Equal(petId, incomplete.FirstUserPetId);
        Assert.Equal(new[] { "picture", "breed" }, incomplete.MissingFields);

        using (var context = _db.NewContext())
        {
            var pet = context.UserPets.AsTracking().Single(p => p.Id == petId);
            pet.PictureId = seed.PictureIds[0];
            pet.PetBreedId = 1;
            pet.IsMixBreed = true; // دورگه بدون نژاد دوم هم کامل است
            context.SaveChanges();
        }
        Assert.Equal(PetProfileGiftStatusEnum.SaveToClaim, (await Status(userId)).Status);

        using (var context = _db.NewContext())
            Assert.True(await NewGift(context, out _).GrantAsync(userId, petId));
        var ready = await Status(userId);
        Assert.Equal(PetProfileGiftStatusEnum.Ready, ready.Status);
        Assert.False(ready.ShowBanner);
        Assert.NotNull(ready.OfferId);
        Assert.NotNull(ready.ExpiresAt);

        // کاربر پت اول را حذف می‌کند: بنر دوباره ظاهر نمی‌شود چون هدیه را گرفته
        using (var context = _db.NewContext())
        {
            context.UserPets.AsTracking().Single(p => p.Id == petId).Deleted = true;
            context.SaveChanges();
        }
        Assert.Equal(PetProfileGiftStatusEnum.Ready, (await Status(userId)).Status);

        using (var context = _db.NewContext())
        {
            var service = new ClubRewardRedemptionService(context, new ClubRewardEligibilityService(context), new ClubRewardBenefitFactory(context));
            Assert.True((await service.RedeemAsync(userId, ready.OfferId!.Value)).IsSuccess);
        }
        var activated = await Status(userId);
        Assert.Equal(PetProfileGiftStatusEnum.Activated, activated.Status);
        Assert.Equal(200_000m, activated.CreditRemaining);
        Assert.Equal(ready.ExpiresAt, activated.CreditExpiresAt);
    }

    [Fact]
    public async Task Status_FirstPetDeletedWithoutGift_IsNotEligible_NoBanner()
    {
        if (!_db.Available) return;
        EnsureTemplate();
        var (userId, _, _) = SeedUserWithTwoPets(firstDeleted: true);
        var status = await Status(userId);
        Assert.Equal(PetProfileGiftStatusEnum.NotEligible, status.Status);
        Assert.False(status.ShowBanner);
    }
}
