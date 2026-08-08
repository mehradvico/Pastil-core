using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Services.Order.PaymentSrv.Iface;
using Application.Services.Order.PaymentSrv.Dto;
using Application.Services.PastilAISrv.Dto;
using Application.Services.PastilAISrv.Iface;
using Entities.Entities.PastilAIField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilAISrv
{
    public class PastilAiPlanService : IPastilAiPlanService
    {
        private readonly IDataBaseContext _context;
        private readonly IPaymentService _paymentService;

        public PastilAiPlanService(IDataBaseContext context, IPaymentService paymentService)
        {
            _context = context;
            _paymentService = paymentService;
        }

        public async Task<BaseResultDto<List<PastilAiPlanVDto>>> GetPlansAsync(bool admin, CancellationToken cancellationToken)
        {
            var query = _context.PastilAiPlans.AsNoTracking();
            if (!admin)
                query = query.Where(x => x.Active);
            var plans = await query.OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
            return new BaseResultDto<List<PastilAiPlanVDto>>(true, plans.Select(ToDto).ToList());
        }

        public async Task<BaseResultDto<PastilAiPlanVDto>> UpdateAsync(PastilAiPlanUpdateDto dto, CancellationToken cancellationToken)
        {
            if (dto.Id <= 0 || string.IsNullOrWhiteSpace(dto.Name) || dto.DurationDays <= 0 || dto.Price < 0 ||
                dto.DailyChatLimit is < 0 || dto.DailyImageLimit is < 0 ||
                dto.DailyAudioLimit is < 0 || dto.DailyVideoLimit is < 0)
                return new BaseResultDto<PastilAiPlanVDto>(false, Resource.Notification.InvalidData, null);

            var plan = await _context.PastilAiPlans.AsTracking().FirstOrDefaultAsync(x => x.Id == dto.Id, cancellationToken);
            if (plan == null)
                return new BaseResultDto<PastilAiPlanVDto>(false, Resource.Notification.NothingFound, null);
            if (string.Equals(plan.Code, PastilAiPlanCode.Free.ToString(), StringComparison.OrdinalIgnoreCase) && dto.PurchaseEnabled)
                return new BaseResultDto<PastilAiPlanVDto>(false, Resource.Notification.InvalidData, null);
            if (!string.Equals(plan.Code, PastilAiPlanCode.Free.ToString(), StringComparison.OrdinalIgnoreCase) &&
                dto.PurchaseEnabled && dto.Price < 10000)
                return new BaseResultDto<PastilAiPlanVDto>(false, "برای فعال‌کردن خرید، قیمت پلن باید بیشتر از صفر باشد.", null);

            plan.Name = dto.Name.Trim();
            plan.Description = dto.Description?.Trim();
            plan.Price = dto.Price;
            plan.DurationDays = dto.DurationDays;
            plan.DailyChatLimit = dto.DailyChatLimit;
            plan.DailyImageLimit = dto.DailyImageLimit;
            plan.DailyAudioLimit = dto.DailyAudioLimit;
            plan.DailyVideoLimit = dto.DailyVideoLimit;
            plan.PurchaseEnabled = dto.PurchaseEnabled;
            plan.Active = dto.Active;
            plan.SortOrder = dto.SortOrder;
            plan.UpdateDateUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return new BaseResultDto<PastilAiPlanVDto>(true, ToDto(plan));
        }

        public async Task<BaseResultDto<PastilAiQuotaDto>> GetQuotaAsync(long userId, CancellationToken cancellationToken)
        {
            var resolved = await ResolvePlanAsync(userId, cancellationToken);
            var today = DateTime.UtcNow.Date;
            var usage = await _context.PastilAiDailyUsages.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId && x.UsageDate == today, cancellationToken);
            return new BaseResultDto<PastilAiQuotaDto>(true, new PastilAiQuotaDto
            {
                PlanCode = resolved.Plan.Code,
                PlanName = resolved.Plan.Name,
                SubscriptionEndDateUtc = resolved.Subscription?.EndDateUtc,
                UsedChats = usage?.ChatCount ?? 0,
                UsedImages = usage?.ImageCount ?? 0,
                UsedAudio = usage?.AudioCount ?? 0,
                UsedVideo = usage?.VideoCount ?? 0,
                DailyChatLimit = resolved.Plan.DailyChatLimit,
                DailyImageLimit = resolved.Plan.DailyImageLimit,
                DailyAudioLimit = resolved.Plan.DailyAudioLimit,
                DailyVideoLimit = resolved.Plan.DailyVideoLimit
            });
        }

        public async Task<BaseResultDto> PurchaseAsync(long userId, PastilAiPurchaseDto dto, CancellationToken cancellationToken)
        {
            if (dto.PlanId <= 0 || dto.MerchantId <= 0)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var plan = await _context.PastilAiPlans.AsNoTracking().FirstOrDefaultAsync(x =>
                x.Id == dto.PlanId && x.Active && x.PurchaseEnabled && x.Price >= 10000 &&
                x.Code != PastilAiPlanCode.Free.ToString(), cancellationToken);
            if (plan == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            var typeId = await _context.Codes.AsNoTracking()
                .Where(x => x.Label == PaymentTypeEnum.PaymentType_PastilAI.ToString() && x.Active)
                .Select(x => (long?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (!typeId.HasValue)
                return new BaseResultDto(false, "نوع پرداخت PastilAI در تنظیمات سیستم ثبت نشده است.");

            var subscription = new PastilAiSubscription
            {
                UserId = userId,
                PlanId = plan.Id,
                Status = PastilAiSubscriptionStatus.PendingPayment,
                PriceSnapshot = plan.Price,
                CreateDateUtc = DateTime.UtcNow
            };
            await _context.PastilAiSubscriptions.AddAsync(subscription, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var paymentDto = new PaymentStartDto
            {
                MerchantId = dto.MerchantId,
                Amount = (double)plan.Price,
                UserId = userId,
                TypeId = typeId.Value,
                CallBackTypeLabel = PaymentCallbackTypeEnum.PastilAI.ToString(),
                CallBackId = subscription.Id.ToString()
            };
            var result = await _paymentService.StartPayment(paymentDto);
            if (!result.IsSuccess)
            {
                subscription.Status = PastilAiSubscriptionStatus.PaymentFailed;
                _context.PastilAiSubscriptions.Update(subscription);
                await _context.SaveChangesAsync(cancellationToken);
                return result;
            }

            var payment = await _context.Payments.AsNoTracking()
                .Where(x => x.UserId == userId && x.CallBackTypeLabel == PaymentCallbackTypeEnum.PastilAI.ToString() &&
                            x.CallBackId == subscription.Id.ToString())
                .OrderByDescending(x => x.Id).FirstOrDefaultAsync(cancellationToken);
            if (payment != null)
            {
                subscription.PaymentId = payment.Id;
                _context.PastilAiSubscriptions.Update(subscription);
                await _context.SaveChangesAsync(cancellationToken);
            }
            return result;
        }

        internal async Task<(PastilAiPlan Plan, PastilAiSubscription Subscription)> ResolvePlanAsync(long userId, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var subscription = await _context.PastilAiSubscriptions.AsNoTracking().Include(x => x.Plan)
                .Where(x => x.UserId == userId && x.Status == PastilAiSubscriptionStatus.Active &&
                            x.StartDateUtc <= now && x.EndDateUtc > now && x.Plan.Active)
                .OrderByDescending(x => x.Plan.SortOrder).ThenByDescending(x => x.EndDateUtc)
                .FirstOrDefaultAsync(cancellationToken);
            if (subscription != null)
                return (subscription.Plan, subscription);

            var free = await _context.PastilAiPlans.AsNoTracking()
                .FirstAsync(x => x.Code == PastilAiPlanCode.Free.ToString() && x.Active, cancellationToken);
            return (free, null);
        }

        private static PastilAiPlanVDto ToDto(PastilAiPlan x) => new()
        {
            Id = x.Id, Code = x.Code, Name = x.Name, Description = x.Description, Price = x.Price,
            DurationDays = x.DurationDays, DailyChatLimit = x.DailyChatLimit, DailyImageLimit = x.DailyImageLimit,
            DailyAudioLimit = x.DailyAudioLimit, DailyVideoLimit = x.DailyVideoLimit,
            PurchaseEnabled = x.PurchaseEnabled, Active = x.Active, SortOrder = x.SortOrder
        };
    }
}
