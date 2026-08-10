using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Application.Services.Setting.NoticeSrv.Iface;
using Application.Services.Setting.SmsSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Entities.Entities.PastilClubField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.WalletSrv
{
    public class WalletService : IWalletService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ISmsService _smsService;
        private readonly INoticeService _notificationService;

        public WalletService(IDataBaseContext _context, INoticeService notificationService, IConfiguration config, IMapper mapper, ISmsService smsService)
        {
            this._context = _context;
            this.mapper = mapper;
            this._smsService = smsService;
            this._notificationService = notificationService;


        }

        public async Task<BaseResultDto> DeleteAsync(long id)
        {
            var item = await _context.Wallets.FirstOrDefaultAsync(s => s.Id == id && s.Painding);
            if (item != null)
            {
                item.Deleted = true;
                _context.Wallets.Update(item);
                await _context.SaveChangesAsync();
                return new BaseResultDto(true);
            }
            return new BaseResultDto(false);
        }

        public async Task<BaseResultDto<WalletVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.Wallets.Include(s => s.User).Include(s => s.ProductOrder).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<WalletVDto>(true, mapper.Map<WalletVDto>(item));
            }
            return new BaseResultDto<WalletVDto>(false, null);
        }

        public async Task<double> GetAmountValueAsync(long userId)
        {
            var sum = await _context.Wallets
                .AsNoTracking()
                .Where(s => s.UserId == userId && !s.Deleted && !s.Painding)
                .SumAsync(s => s.IsIncrease ? s.Amount : -s.Amount);
            sum = sum < 1 ? 0 : sum;
            return sum;
        }
        public async Task<BaseResultDto<double>> GetAmountAsync(long userId)
        {
            var amount = await GetAmountValueAsync(userId);
            return new BaseResultDto<double>(true, amount);
        }

        public async Task<double> GetSpendableAmountValueAsync(
            long userId,
            ClubRewardTargetTypeEnum scopeType,
            long? scopeId)
        {
            var cash = await GetAmountValueAsync(userId);
            var now = DateTimeOffset.UtcNow;
            var promotional = await _context.ClubPromotionalWalletCredits.AsNoTracking()
                .Where(item => item.UserId == userId &&
                    item.Status == ClubPromotionalCreditStatusEnum.Active &&
                    item.ExpiresAt > now &&
                    item.RemainingAmount > 0 &&
                    (item.ServiceScopeType == ClubRewardTargetTypeEnum.Global ||
                     item.ServiceScopeType == scopeType && item.ServiceScopeId == scopeId))
                .SumAsync(item => item.RemainingAmount);
            return cash + decimal.ToDouble(promotional);
        }

        public async Task<BaseResultDto<WalletDto>> InsertAsyncDto(WalletDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<WalletDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    if (dto.Amount == 0)
                    {
                        return new BaseResultDto<WalletDto>(false, Resource.Notification.PleaseEnterTheAmount, dto);
                    }
                    if (dto.Amount < 0)
                    {
                        dto.Amount = dto.Amount * -1;
                    }

                    var currentAmount = await GetAmountValueAsync(dto.UserId);
                    if (!dto.IsIncrease && dto.Amount > currentAmount)
                    {
                        return new BaseResultDto<WalletDto>(false, Resource.Notification.InsufficientFunds, dto);
                    }

                    var item = mapper.Map<Wallet>(dto);

                    item.CreateDate = DateTime.Now;
                    await _context.Wallets.AddAsync(item);
                    await _context.SaveChangesAsync();
                    return new BaseResultDto<WalletDto>(true, mapper.Map<WalletDto>(item));
                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto<WalletDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }


        public async Task<BaseResultDto<WalletDto>> InsertUpdateProductOrderAsync(WalletDto dto, bool complete)
        {
            return await InsertUpdateReferenceAsync(dto, complete);
        }
        public async Task<BaseResultDto<WalletDto>> InsertUpdateCargoAsync(WalletDto dto, bool complete)
        {
            return await InsertUpdateReferenceAsync(dto, complete);
        }
        public async Task<BaseResultDto<WalletDto>> InsertUpdateTripAsync(WalletDto dto, bool complete)
        {
            return await InsertUpdateReferenceAsync(dto, complete);
        }
        public async Task<BaseResultDto<WalletDto>> InsertUpdateReserveAsync(WalletDto dto, bool complete)
        {
            return await InsertUpdateReferenceAsync(dto, complete);
        }
        public async Task<BaseResultDto<WalletDto>> InsertUpdatePansionReserveAsync(WalletDto dto, bool complete)
        {
            return await InsertUpdateReferenceAsync(dto, complete);
        }
        public async Task<BaseResultDto<WalletDto>> InsertUpdateInsuranceAsync(WalletDto dto, bool complete)
        {
            return await InsertUpdateReferenceAsync(dto, complete);
        }
        public async Task<BaseResultDto<WalletDto>> InsertUpdatePastilAiSubscriptionAsync(WalletDto dto, bool complete)
        {
            return await InsertUpdateReferenceAsync(dto, complete);
        }

        private async Task<BaseResultDto<WalletDto>> InsertUpdateReferenceAsync(WalletDto dto, bool complete)
        {
            IQueryable<Wallet> query = _context.Wallets.AsTracking();

            if (!string.IsNullOrWhiteSpace(dto.ProductOrderId))
                query = query.Where(s => s.ProductOrderId == dto.ProductOrderId);
            else if (dto.CompanionReserveId.HasValue)
                query = query.Where(s => s.CompanionReserveId == dto.CompanionReserveId);
            else if (dto.PansionReserveId.HasValue)
                query = query.Where(s => s.PansionReserveId == dto.PansionReserveId);
            else if (dto.TripId.HasValue)
                query = query.Where(s => s.TripId == dto.TripId);
            else if (dto.CargoId.HasValue)
                query = query.Where(s => s.CargoId == dto.CargoId);
            else if (dto.CompanionInsurancePackageSaleId.HasValue)
                query = query.Where(s => s.CompanionInsurancePackageSaleId == dto.CompanionInsurancePackageSaleId);
            else if (dto.PastilAiSubscriptionId.HasValue)
                query = query.Where(s => s.PastilAiSubscriptionId == dto.PastilAiSubscriptionId);
            else
                return new BaseResultDto<WalletDto>(false, Resource.Notification.InvalidData, dto);

            if (complete)
            {
                var reference = await ResolveReferenceAsync(dto);
                if (reference != null)
                {
                    var alreadyConsumed = await _context.ClubPromotionalCreditUsages.AsNoTracking()
                        .Where(item => item.ReferenceKey == reference.ReferenceKey)
                        .SumAsync(item => item.Amount);
                    var promotionalAmount = alreadyConsumed > 0
                        ? alreadyConsumed
                        : await ConsumePromotionalCreditAsync(dto.UserId, Convert.ToDecimal(dto.Amount), reference);
                    dto.Amount = Math.Max(0, dto.Amount - decimal.ToDouble(promotionalAmount));
                    if (dto.Amount <= 0)
                        return new BaseResultDto<WalletDto>(true, dto);
                }
            }

            var item = await query.FirstOrDefaultAsync();
            if (item != null)
            {
                if (!complete)
                {
                    // A failed gateway callback must only release an old pending hold.
                    // It must never create or delete a completed wallet debit.
                    if (item.Painding && !item.Deleted)
                    {
                        item.Deleted = true;
                        await _context.SaveChangesAsync();
                    }
                    return new BaseResultDto<WalletDto>(true, mapper.Map<WalletDto>(item));
                }

                if (!item.Deleted && !item.Painding)
                    return new BaseResultDto<WalletDto>(true, mapper.Map<WalletDto>(item));

                var amount = dto.Amount > 0 ? dto.Amount : item.Amount;
                if (amount <= 0 || amount > await GetAmountValueAsync(dto.UserId))
                    return new BaseResultDto<WalletDto>(false, Resource.Notification.InsufficientFunds, dto);

                item.Amount = amount;
                item.UserId = dto.UserId;
                item.IsIncrease = false;
                item.Painding = false;
                item.Deleted = false;
                await _context.SaveChangesAsync();
                return new BaseResultDto<WalletDto>(true, mapper.Map<WalletDto>(item));
            }

            if (!complete)
                return new BaseResultDto<WalletDto>(true, dto);

            dto.IsIncrease = false;
            dto.Painding = false;
            return await InsertAsyncDto(dto);
        }

        private async Task<PromotionalReference> ResolveReferenceAsync(WalletDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.ProductOrderId))
            {
                var storeId = await _context.ProductOrderStores.AsNoTracking()
                    .Where(item => item.ProductOrderId == dto.ProductOrderId)
                    .Select(item => (long?)item.StoreId)
                    .FirstOrDefaultAsync();
                return new PromotionalReference(
                    ClubRewardApplicationMethodEnum.ProductOrder,
                    ClubRewardTargetTypeEnum.Store,
                    storeId,
                    $"product-order:{dto.ProductOrderId}");
            }
            if (dto.CompanionReserveId.HasValue)
            {
                var assistanceId = await _context.CompanionReserves.AsNoTracking()
                    .Where(item => item.Id == dto.CompanionReserveId.Value)
                    .Select(item => (long?)item.CompanionAssistance.AssistanceId)
                    .FirstOrDefaultAsync();
                return new PromotionalReference(
                    ClubRewardApplicationMethodEnum.CompanionReservation,
                    ClubRewardTargetTypeEnum.Assistance,
                    assistanceId,
                    $"companion-reserve:{dto.CompanionReserveId.Value}");
            }
            if (dto.PansionReserveId.HasValue)
            {
                var pansionId = await _context.PansionReserves.AsNoTracking()
                    .Where(item => item.Id == dto.PansionReserveId.Value)
                    .Select(item => (long?)item.PansionId)
                    .FirstOrDefaultAsync();
                return new PromotionalReference(
                    ClubRewardApplicationMethodEnum.PansionReservation,
                    ClubRewardTargetTypeEnum.Pansion,
                    pansionId,
                    $"pansion-reserve:{dto.PansionReserveId.Value}");
            }
            if (dto.PastilAiSubscriptionId.HasValue)
            {
                var planId = await _context.PastilAiSubscriptions.AsNoTracking()
                    .Where(item => item.Id == dto.PastilAiSubscriptionId.Value)
                    .Select(item => (long?)item.PlanId)
                    .FirstOrDefaultAsync();
                return new PromotionalReference(
                    ClubRewardApplicationMethodEnum.PastilAI,
                    ClubRewardTargetTypeEnum.PastilAIPlan,
                    planId,
                    $"pastil-ai:{dto.PastilAiSubscriptionId.Value}");
            }
            return null;
        }

        private async Task<decimal> ConsumePromotionalCreditAsync(
            long userId,
            decimal requestedAmount,
            PromotionalReference reference)
        {
            if (requestedAmount <= 0)
                return 0;
            var now = DateTimeOffset.UtcNow;
            var credits = await _context.ClubPromotionalWalletCredits.AsTracking()
                .Where(item => item.UserId == userId &&
                    item.Status == ClubPromotionalCreditStatusEnum.Active &&
                    item.ExpiresAt > now &&
                    item.RemainingAmount > 0 &&
                    (item.ServiceScopeType == ClubRewardTargetTypeEnum.Global ||
                     item.ServiceScopeType == reference.ScopeType && item.ServiceScopeId == reference.ScopeId))
                .OrderBy(item => item.ExpiresAt)
                .ThenBy(item => item.Id)
                .ToListAsync();

            decimal consumed = 0;
            foreach (var credit in credits)
            {
                var amount = Math.Min(credit.RemainingAmount, requestedAmount - consumed);
                if (amount <= 0)
                    break;
                credit.RemainingAmount -= amount;
                if (credit.RemainingAmount == 0)
                    credit.Status = ClubPromotionalCreditStatusEnum.Consumed;
                await _context.ClubPromotionalCreditUsages.AddAsync(new ClubPromotionalCreditUsage
                {
                    PromotionalCreditId = credit.Id,
                    UserId = userId,
                    Amount = amount,
                    ApplicationMethod = reference.ApplicationMethod,
                    ReferenceKey = reference.ReferenceKey,
                    CreateDate = DateTime.UtcNow
                });
                consumed += amount;
            }
            await _context.SaveChangesAsync();
            return consumed;
        }

        private record PromotionalReference(
            ClubRewardApplicationMethodEnum ApplicationMethod,
            ClubRewardTargetTypeEnum ScopeType,
            long? ScopeId,
            string ReferenceKey);
        public WalletSearchDto Search(WalletInputDto baseSearchDto)
        {
            var query = _context.Wallets.Include(s => s.User).Where(s => !s.Deleted).AsQueryable();

            if (baseSearchDto.UserId.HasValue)
            {
                query = query.Where(s => s.UserId == baseSearchDto.UserId);
            }
            if (baseSearchDto.IsIncrease.HasValue)
            {
                query = query.Where(s => s.IsIncrease == baseSearchDto.IsIncrease);
            }
            if (baseSearchDto.DateFrom.HasValue)
            {
                query = query.Where(s => s.CreateDate >= baseSearchDto.DateFrom);
            }
            if (baseSearchDto.DateTo.HasValue)
            {
                query = query.Where(s => s.CreateDate <= baseSearchDto.DateTo);
            }
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                query = query.Where(s => s.Name.Contains(baseSearchDto.Q) || s.ProductOrderId == baseSearchDto.Q);
            }

            switch (baseSearchDto.SortBy)
            {
                case Common.Enumerable.SortEnum.Default:
                    {
                        query = query.OrderByDescending(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.New:
                    {
                        query = query.OrderByDescending(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Old:
                    {
                        query = query.OrderBy(s => s.Id);
                        break;
                    }

                default:
                    break;
            }
            return new WalletSearchDto(baseSearchDto, query, mapper);
        }

        public async Task<BaseResultDto> WalletPaymentCallback(Payment payment)
        {
            var existingWallet = await _context.Wallets
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.PaymentId == payment.Id && !s.Deleted);
            if (existingWallet != null)
                return new BaseResultDto(true);

            var walletDto = new WalletDto()
            {
                ProductOrderId = null,
                Amount = payment.Amount,
                IsIncrease = true,
                UserId = payment.UserId,
                PaymentId = payment.Id,
                Painding = false,
                Name = Resource.Lang.OnlinePayment,
            };
            var result = await InsertAsyncDto(walletDto);
            if (!result.IsSuccess &&
                await _context.Wallets.AsNoTracking().AnyAsync(s => s.PaymentId == payment.Id && !s.Deleted))
            {
                return new BaseResultDto(true);
            }
            if (result.IsSuccess == true)
            {
                var user = payment.User ?? await _context.Users.AsNoTracking().FirstOrDefaultAsync(s => s.Id == payment.UserId);
                if (user != null)
                    await _smsService.SendSmsAsync(smsType: Common.Enumerable.Message.MessageTypeEnum.IncreaseWallet, user.Mobile, token1: user.FirstName, token2: payment.Amount.ToString());
            }
            return result;
        }
    }
}
