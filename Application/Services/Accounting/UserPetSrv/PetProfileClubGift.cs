using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.PastilClubSrvs.PetProfileSrv.Iface;
using Entities.Entities.PastilClubField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Accounting.UserPetSrv
{
    public interface IPetProfileClubGift
    {
        // اگر شرایط برقرار باشد هدیه را در کلاب کاربر می‌گذارد و true برمی‌گرداند. خطا را بالا نمی‌دهد.
        Task<bool> GrantAsync(long userId, long userPetId);

        // وضعیت هدیه برای بنر/کارت وب‌اپ و اپ (چه چیزی نشان داده شود)
        Task<BaseResultDto<PetProfileGiftStatusDto>> GetStatusAsync(long userId, CancellationToken cancellationToken = default);
    }

    public enum PetProfileGiftStatusEnum
    {
        /// <summary>قالب هدیه در کلاب غیرفعال است؛ هیچ بنری نشان داده نشود.</summary>
        Unavailable = 0,
        /// <summary>کاربر هنوز هیچ پتی ثبت نکرده؛ بنر «پتت رو اضافه کن و ۲۰۰ هزار تومان بگیر».</summary>
        NoPet = 1,
        /// <summary>اولین پت ناقص است؛ بنر با فهرست MissingFields و لینک ویرایش FirstUserPetId.</summary>
        Incomplete = 2,
        /// <summary>اولین پت کامل است ولی پیش از راه‌اندازی هدیه کامل شده؛ با یک‌بار ذخیره‌ی پروفایل پت هدیه ساخته می‌شود.</summary>
        SaveToClaim = 3,
        /// <summary>هدیه در کلاب منتظر «فعال کن» است (OfferId، ExpiresAt).</summary>
        Ready = 4,
        /// <summary>فعال شده؛ مانده و انقضای اعتبار در CreditRemaining/CreditExpiresAt.</summary>
        Activated = 5,
        /// <summary>منقضی شد بدون فعال‌سازی.</summary>
        Expired = 6,
        /// <summary>اولین پت حذف شده و هدیه‌ای نگرفته؛ دیگر واجد شرایط نیست، بنری نشان داده نشود.</summary>
        NotEligible = 7
    }

    public class PetProfileGiftStatusDto
    {
        public PetProfileGiftStatusEnum Status { get; set; }
        public string StatusKey => Status.ToString();
        /// <summary>آیا بنر «پروفایل پتت رو کامل کن» نشان داده شود (NoPet، Incomplete، SaveToClaim).</summary>
        public bool ShowBanner => Status is PetProfileGiftStatusEnum.NoPet or PetProfileGiftStatusEnum.Incomplete or PetProfileGiftStatusEnum.SaveToClaim;
        public decimal Amount { get; set; }
        public long? FirstUserPetId { get; set; }
        /// <summary>کلیدهای PetProfileFields: name، petType، picture، breed، birthday.</summary>
        public List<string> MissingFields { get; set; } = new();
        public long? OfferId { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
        public decimal? CreditRemaining { get; set; }
        public DateTimeOffset? CreditExpiresAt { get; set; }
    }

    // هدیه‌ی «تکمیل پروفایل پت» (وعده‌ی پیامک خوش‌آمد ثبت‌نام) برای همه‌ی کاربران: وقتی اولین پتی که کاربر ثبت کرده
    // (کمترین Id، حتی اگر بعداً حذف شده) طبق IClubPetProfileCompletionService کامل شود، یک پیشنهاد کلاب رایگان از قالب
    // PetProfileGift (اعتبار تشویقی ۲۰۰ هزار تومانی، همه‌جا قابل خرج) با انقضای ۳۰ روز از همین لحظه ساخته می‌شود. کاربر در
    // کلاب «فعال کن» می‌زند (Redeem با امتیاز صفر) و اعتبار با همان تاریخ انقضا ساخته می‌شود. پت‌های بعدی هدیه ندارند.
    // یک‌بار برای هر کاربر: ایندکس یکتای ClubRewardOffers(UserId, RewardTemplateId) در دیتابیس ضامن آن است.
    public class PetProfileClubGift : IPetProfileClubGift
    {
        public const string TemplateName = "PetProfileGift";
        public static readonly TimeSpan Lifetime = TimeSpan.FromDays(30);

        private readonly IDataBaseContext _context;
        private readonly IPushNotificationService _pushService;
        private readonly IClubPetProfileCompletionService _completion;
        private readonly ILogger<PetProfileClubGift> _logger;

        public PetProfileClubGift(IDataBaseContext context, IPushNotificationService pushService, IClubPetProfileCompletionService completion, ILogger<PetProfileClubGift> logger)
        {
            _context = context;
            _pushService = pushService;
            _completion = completion;
            _logger = logger;
        }

        public async Task<BaseResultDto<PetProfileGiftStatusDto>> GetStatusAsync(long userId, CancellationToken cancellationToken = default)
        {
            var template = await _context.ClubRewardTemplates.AsNoTracking()
                .Where(t => t.Name == TemplateName && t.Active)
                .Select(t => new { t.Id, t.BenefitValue })
                .FirstOrDefaultAsync(cancellationToken);
            if (template == null)
                return Ok(new PetProfileGiftStatusDto { Status = PetProfileGiftStatusEnum.Unavailable });

            var dto = new PetProfileGiftStatusDto { Amount = template.BenefitValue ?? 0 };
            var offer = await _context.ClubRewardOffers.AsNoTracking()
                .Where(o => o.UserId == userId && o.RewardTemplateId == template.Id)
                .Select(o => new { o.Id, o.Status, o.ExpiresAt, CreditId = o.Redemption == null ? null : o.Redemption.BenefitReferenceId })
                .FirstOrDefaultAsync(cancellationToken);
            if (offer != null)
            {
                dto.OfferId = offer.Id;
                dto.ExpiresAt = offer.ExpiresAt;
                if (offer.Status == ClubRewardOfferStatusEnum.Redeemed)
                {
                    dto.Status = PetProfileGiftStatusEnum.Activated;
                    var credit = offer.CreditId.HasValue
                        ? await _context.ClubPromotionalWalletCredits.AsNoTracking()
                            .Where(c => c.Id == offer.CreditId.Value && c.UserId == userId)
                            .Select(c => new { c.RemainingAmount, c.ExpiresAt, c.Status })
                            .FirstOrDefaultAsync(cancellationToken)
                        : null;
                    if (credit != null)
                    {
                        dto.CreditRemaining = credit.Status == ClubPromotionalCreditStatusEnum.Active && credit.ExpiresAt > DateTimeOffset.UtcNow ? credit.RemainingAmount : 0;
                        dto.CreditExpiresAt = credit.ExpiresAt;
                    }
                }
                else
                {
                    dto.Status = offer.Status == ClubRewardOfferStatusEnum.Approved && offer.ExpiresAt > DateTimeOffset.UtcNow
                        ? PetProfileGiftStatusEnum.Ready
                        : PetProfileGiftStatusEnum.Expired;
                }
                return Ok(dto);
            }

            var firstPet = await _context.UserPets.AsNoTracking()
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (firstPet == null)
            {
                dto.Status = PetProfileGiftStatusEnum.NoPet;
                return Ok(dto);
            }

            dto.FirstUserPetId = firstPet.Id;
            if (firstPet.Deleted || !firstPet.Active)
            {
                dto.Status = PetProfileGiftStatusEnum.NotEligible;
                return Ok(dto);
            }

            dto.MissingFields = _completion.MissingFields(firstPet).ToList();
            dto.Status = dto.MissingFields.Count == 0 ? PetProfileGiftStatusEnum.SaveToClaim : PetProfileGiftStatusEnum.Incomplete;
            return Ok(dto);
        }

        private static BaseResultDto<PetProfileGiftStatusDto> Ok(PetProfileGiftStatusDto dto) => new(true, dto);

        public async Task<bool> GrantAsync(long userId, long userPetId)
        {
            ClubRewardOffer offer;
            try
            {
                if (!await IsFirstPetAsync(userId, userPetId))
                    return false;

                var templateId = await _context.ClubRewardTemplates.AsNoTracking()
                    .Where(t => t.Name == TemplateName && t.Active)
                    .Select(t => (long?)t.Id)
                    .FirstOrDefaultAsync();
                if (!templateId.HasValue)
                {
                    _logger.LogWarning("Club template {Template} is missing or inactive; pet profile gift skipped for user {UserId}.", TemplateName, userId);
                    return false;
                }

                // هر پیشنهاد قبلی از این قالب (فعال، استفاده‌شده، منقضی یا ردشده) یعنی کاربر هدیه‌اش را گرفته است
                if (await _context.ClubRewardOffers.AsNoTracking().AnyAsync(o => o.UserId == userId && o.RewardTemplateId == templateId.Value))
                    return false;

                var now = DateTimeOffset.UtcNow;
                offer = new ClubRewardOffer
                {
                    UserId = userId,
                    RewardTemplateId = templateId.Value,
                    SourceType = ClubRewardOfferSourceEnum.Automation,
                    Status = ClubRewardOfferStatusEnum.Approved,
                    PointCostSnapshot = 0,
                    GeneratedDate = now,
                    ApprovedDate = now,
                    ExpiresAt = now + Lifetime,
                    CreateDate = DateTime.UtcNow
                };
                await _context.ClubRewardOffers.AddAsync(offer);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    // درخواست هم‌زمان زودتر ساخته است (ایندکس یکتا)
                    _context.Entry(offer).State = EntityState.Detached;
                    return false;
                }
            }
            catch (Exception ex)
            {
                // ذخیره‌ی پت نباید به خاطر هدیه شکست بخورد
                _logger.LogError(ex, "Pet profile Club gift failed for user {UserId}, pet {UserPetId}.", userId, userPetId);
                return false;
            }

            _logger.LogInformation("Pet profile Club gift offer {OfferId} created for user {UserId} (pet {UserPetId}).", offer.Id, userId, userPetId);
            try
            {
                await _pushService.SendPushAsync(PushTypeEnum.PushClubPetProfileGift, userId, token1: offer.Id.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Pet profile gift push failed for user {UserId}.", userId);
            }
            return true;
        }

        private async Task<bool> IsFirstPetAsync(long userId, long userPetId)
        {
            var firstPetId = await _context.UserPets.AsNoTracking()
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.Id)
                .Select(p => (long?)p.Id)
                .FirstOrDefaultAsync();
            return firstPetId == userPetId;
        }
    }
}
