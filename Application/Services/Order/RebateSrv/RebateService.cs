using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.Order.RebateSrv.Dto;
using Application.Services.Order.RebateSrv.Iface;
using AutoMapper;
using DocumentFormat.OpenXml.Drawing.Charts;
using Entities.Entities;
using Entities.Entities.CompanionField;
using Entities.Entities.PansionField;
using Entities.Entities.PastilClubField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Order.RebateSrv
{
    public class RebateService : CommonSrv<Rebate, RebateDto>, IRebateService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;

        public RebateService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }
        public BaseSearchDto<RebateDto> Search(BaseInputDto baseSearchDto)
        {
            var model = _context.Rebate.Where(s => s.Deleted == false).AsQueryable();
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                model = model.Where(s => s.Name.Contains(baseSearchDto.Q) || s.CodeValue.Contains(baseSearchDto.Q)).OrderByDescending(s => s.Id);
            }
            return new BaseSearchDto<Rebate, RebateDto>(baseSearchDto, model, mapper);
        }
        public override async Task<BaseResultDto<RebateDto>> InsertAsyncDto(RebateDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<RebateDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    DateTime justNow = DateTime.Now.Date;
                    if (string.IsNullOrEmpty(dto.Name))
                    {
                        return new BaseResultDto<RebateDto>(isSuccess: false, val1: Resource.Notification.PleaseEnterTheName, val2: nameof(dto.Name), data: dto);
                    }
                    if (dto.PriceValue < 1)
                    {
                        return new BaseResultDto<RebateDto>(isSuccess: false, val1: Resource.Notification.TheEnteredNumberNotCorrect, val2: nameof(dto.PriceValue), data: dto);

                    }
                    if (dto.UseCount < 1 || dto.MaxUsePerUser < 1 || dto.MaxUsePerUser > dto.UseCount)
                    {
                        return new BaseResultDto<RebateDto>(
                            false,
                            Resource.Notification.TheEnteredNumberNotCorrect,
                            nameof(dto.MaxUsePerUser),
                            dto);
                    }
                    dto.CodeValue = dto.CodeValue?.Replace(" ", "").Trim().ToLower();
                    if (!CodeIsUnique(dto.CodeValue))
                    {
                        return new BaseResultDto<RebateDto>(isSuccess: false, val1: Resource.Notification.TheCodeIsDuplicate, val2: nameof(dto.CodeValue), data: dto);
                    }
                    if (dto.IsPriceRebate == false)
                    {
                        if (dto.PriceValue > 99)
                        {
                            return new BaseResultDto<RebateDto>(isSuccess: false, val1: Resource.Notification.ThePercentageRangeNotCorrect, val2: nameof(dto.PriceValue), data: dto);

                        }
                    }
                    else
                    {

                        if (dto.PriceValue + 1000 > dto.MinCartPrice)
                        {
                            return new BaseResultDto<RebateDto>(isSuccess: false, val1: string.Format(Resource.Pattern.RebateMinCartPrice, (dto.PriceValue + 1000).ToCurency()), val2: nameof(dto.MinCartPrice), data: dto);


                        }
                    }
                    dto.StartDatetime = dto.StartDatetime.Date;
                    dto.EndDatetime = dto.EndDatetime.Date;
                    if (dto.StartDatetime < justNow || dto.EndDatetime < justNow || dto.StartDatetime > dto.EndDatetime)
                    {
                        return new BaseResultDto<RebateDto>(isSuccess: false, val1: Resource.Notification.TheDateNotCorrect, val2: nameof(dto.StartDatetime), data: dto);
                    }

                    var item = mapper.Map<Rebate>(dto);
                    await _context.Rebate.AddAsync(item);

                    _context.SaveChanges();
                    return new BaseResultDto<RebateDto>(true, mapper.Map<RebateDto>(item));
                }

            }
            catch (Exception)
            {
                return new BaseResultDto<RebateDto>(isSuccess: false, val: Resource.Notification.Unsuccess, data: dto);
            }

        }

        public BaseResultDto<RebateVDto> GetRebateByCodeAsync(Cart cart, string code)
        {
            DateTime justNow = DateTime.Now.Date;
            double basePrice = cart.Price;
            var rebate = GetRebateByCodeValue(code);

            var commonCheck = ValidateRebateCommon(
                rebate: rebate,
                typeLabel: RebateTypeLabels.Cart,
                justNow: justNow,
                userId: cart.UserId,
                rebateUserId: rebate?.UserId,
                basePrice: basePrice
            );

            if (!commonCheck.IsSuccess)
                return commonCheck;

            if (!IsClubTargetValid(rebate, cart))
                return new BaseResultDto<RebateVDto>(false, Resource.Notification.NotPossibleUseRebateCode, null);

            if (rebate.ProductId.HasValue)
            {
                var productTotal = _context.CartItems
                    .Where(s => s.CartStore.CartId == cart.Id && s.ProductItem.ProductId == rebate.ProductId)
                    .Select(s => (double?)(s.ProductItem.Price * s.Count))
                    .Sum();

                if (productTotal.HasValue && productTotal.Value > 0)
                {
                    basePrice = productTotal.Value;
                }
                else
                {
                    return new BaseResultDto<RebateVDto>(false, Resource.Notification.NotPossibleUseRebateCode, null);
                }
            }

            var rebateDto = mapper.Map<RebateVDto>(rebate);
            rebateDto.FinalPrice = ApplyClubMaximum(rebate, rebateDto.IsPriceRebate
                ? rebateDto.PriceValue
                : Math.Round(basePrice * (rebateDto.PriceValue / 100)));

            return new BaseResultDto<RebateVDto>(true, Resource.Notification.Success, rebateDto);
        }
        public BaseResultDto<RebateVDto> GetRebateByCodeAsync(CompanionReserve companionReserve, string code)
        {
            DateTime justNow = DateTime.Now.Date;
            double basePrice = companionReserve.PrePaymentPrice;
            var rebate = GetRebateByCodeValue(code);

            var commonCheck = ValidateRebateCommon(
                rebate: rebate,
                typeLabel: RebateTypeLabels.CompanionReserve,
                justNow: justNow,
                userId: companionReserve.BookerId,
                rebateUserId: rebate?.UserId,
                basePrice: basePrice
            );

            if (!commonCheck.IsSuccess)
                return commonCheck;

            if (!IsClubTargetValid(rebate, companionReserve))
                return new BaseResultDto<RebateVDto>(false, Resource.Notification.NotPossibleUseRebateCode, null);

            var rebateDto = mapper.Map<RebateVDto>(rebate);
            rebateDto.FinalPrice = ApplyClubMaximum(rebate, rebateDto.IsPriceRebate
                ? rebateDto.PriceValue
                : Math.Round(basePrice * (rebateDto.PriceValue / 100)));

            return new BaseResultDto<RebateVDto>(true, Resource.Notification.Success, rebateDto);
        }
        bool CodeIsUnique(string codeValue)
        {
            var item = GetRebateByCodeValue(codeValue);
            if (item == null)
                return true;
            return false;
        }
        Rebate GetRebateByCodeValue(string CodeValue)
        {
            var normalizedCode = CodeValue?.Replace(" ", "").Trim().ToLower();
            return _context.Rebate.Include(x => x.Type)
                .Include(x => x.ClubCoupon)
                    .ThenInclude(x => x.RewardRedemption)
                        .ThenInclude(x => x.RewardTemplate)
                            .ThenInclude(x => x.Targets)
                .FirstOrDefault(x => !x.Deleted && x.CodeValue == normalizedCode);
        }

        private BaseResultDto<RebateVDto> ValidateRebateCommon(Rebate rebate, string typeLabel, DateTime justNow, long? userId, long? rebateUserId, double basePrice)
        {
            if (rebate == null || !rebate.Active || rebate.Deleted)
            {
                return new BaseResultDto<RebateVDto>(false, Resource.Notification.NothingFound, null);
            }
            if (rebate.ClubCoupon != null &&
                (rebate.ClubCoupon.Used || rebate.ClubCoupon.ExpiresAt <= DateTimeOffset.UtcNow))
            {
                return new BaseResultDto<RebateVDto>(false, Resource.Notification.ThisDiscountCodeExpired, null);
            }
            if (!string.Equals(rebate.Type?.Label, typeLabel, StringComparison.Ordinal))
            {
                return new BaseResultDto<RebateVDto>(false, Resource.Notification.NothingFound, null);
            }
            if (rebateUserId.HasValue && rebateUserId != userId)
            {
                return new BaseResultDto<RebateVDto>(false, Resource.Notification.NothingFound, null);
            }
            if (rebate.StartDatetime.Date > justNow)
            {
                return new BaseResultDto<RebateVDto>(false, Resource.Notification.TheTimeUseCodeNotArrived, null);
            }
            if (rebate.EndDatetime.Date < justNow)
            {
                return new BaseResultDto<RebateVDto>(false, Resource.Notification.ThisDiscountCodeExpired, null);
            }
            if (rebate.UsedCount >= rebate.UseCount)
            {
                return new BaseResultDto<RebateVDto>(false, Resource.Notification.TheLimitUsesDiscountCodeReached, null);
            }
            if (userId.HasValue)
            {
                var userUsage = _context.UserRebates.FirstOrDefault(x => x.UserId == userId.Value && x.RebateId == rebate.Id);

                if (userUsage != null && userUsage.UsageCount >= rebate.MaxUsePerUser)
                {
                    return new BaseResultDto<RebateVDto>(false, Resource.Notification.TheLimitUsesDiscountCodeReached, null);
                }
            }

            if (rebate.IsPriceRebate && rebate.MinCartPrice > basePrice)
            {
                return new BaseResultDto<RebateVDto>(
                    false,
                    string.Format(Resource.Pattern.RebateMinCartPrice, rebate.MinCartPrice.ToCurency()),
                    null
                );
            }
            return new BaseResultDto<RebateVDto>(true, Resource.Notification.Success, null);
        }

        public BaseResultDto<RebateVDto> GetRebateByCodeAsync(Cargo cargo, string Code)
        {
            DateTime justNow = DateTime.Now.Date;
            double basePrice = cargo.Price;
            var rebate = GetRebateByCodeValue(Code);

            var commonCheck = ValidateRebateCommon(
                rebate: rebate,
                typeLabel: RebateTypeLabels.Cargo,
                justNow: justNow,
                userId: cargo.UserPet.UserId,
                rebateUserId: rebate?.UserId,
                basePrice: basePrice
            );

            if (!commonCheck.IsSuccess)
                return commonCheck;

            var rebateDto = mapper.Map<RebateVDto>(rebate);
            rebateDto.FinalPrice = ApplyClubMaximum(rebate, rebateDto.IsPriceRebate
                ? rebateDto.PriceValue
                : Math.Round(basePrice * (rebateDto.PriceValue / 100)));

            return new BaseResultDto<RebateVDto>(true, Resource.Notification.Success, rebateDto);
        }

        public BaseResultDto<RebateVDto> GetRebateByCodeAsync(CompanionInsurancePackageSale insurance, string Code)
        {
            DateTime justNow = DateTime.Now.Date;
            double basePrice = insurance.Price;
            var rebate = GetRebateByCodeValue(Code);

            var commonCheck = ValidateRebateCommon(
                rebate: rebate,
                typeLabel: RebateTypeLabels.InsurancePackageSale,
                justNow: justNow,
                userId: insurance.UserPet.UserId,
                rebateUserId: rebate?.UserId,
                basePrice: basePrice
            );

            if (!commonCheck.IsSuccess)
                return commonCheck;

            var rebateDto = mapper.Map<RebateVDto>(rebate);
            rebateDto.FinalPrice = ApplyClubMaximum(rebate, rebateDto.IsPriceRebate
                ? rebateDto.PriceValue
                : Math.Round(basePrice * (rebateDto.PriceValue / 100)));

            return new BaseResultDto<RebateVDto>(true, Resource.Notification.Success, rebateDto);
        }

        public BaseResultDto<RebateVDto> GetRebateByCodeAsync(Trip trip, string Code)
        {
            DateTime justNow = DateTime.Now.Date;
            double basePrice = trip.Price;
            var rebate = GetRebateByCodeValue(Code);

            var commonCheck = ValidateRebateCommon(
                rebate: rebate,
                typeLabel: RebateTypeLabels.Trip,
                justNow: justNow,
                userId: trip.UserPet.UserId,
                rebateUserId: rebate?.UserId,
                basePrice: basePrice
            );

            if (!commonCheck.IsSuccess)
                return commonCheck;

            var rebateDto = mapper.Map<RebateVDto>(rebate);
            rebateDto.FinalPrice = ApplyClubMaximum(rebate, rebateDto.IsPriceRebate
                ? rebateDto.PriceValue
                : Math.Round(basePrice * (rebateDto.PriceValue / 100)));

            return new BaseResultDto<RebateVDto>(true, Resource.Notification.Success, rebateDto);
        }

        public BaseResultDto<RebateVDto> GetRebateByCodeAsync(PansionReserve pansion, string Code)
        {
            DateTime justNow = DateTime.Now.Date;
            double basePrice = pansion.Price;
            var rebate = GetRebateByCodeValue(Code);

            var commonCheck = ValidateRebateCommon(
                rebate: rebate,
                typeLabel: RebateTypeLabels.PansionReserve,
                justNow: justNow,
                userId: pansion.BookerId,
                rebateUserId: rebate?.UserId,
                basePrice: basePrice
            );

            if (!commonCheck.IsSuccess)
                return commonCheck;

            if (!IsClubTargetValid(rebate, pansion))
                return new BaseResultDto<RebateVDto>(false, Resource.Notification.NotPossibleUseRebateCode, null);

            var rebateDto = mapper.Map<RebateVDto>(rebate);
            rebateDto.FinalPrice = ApplyClubMaximum(rebate, rebateDto.IsPriceRebate
                ? rebateDto.PriceValue
                : Math.Round(basePrice * (rebateDto.PriceValue / 100)));

            return new BaseResultDto<RebateVDto>(true, Resource.Notification.Success, rebateDto);
        }

        public BaseResultDto<RebateVDto> GetRebateByCodeAsync(double basePrice, long userId, string typeLabel, string code, long? targetId = null)
        {
            var rebate = GetRebateByCodeValue(code);
            var commonCheck = ValidateRebateCommon(
                rebate,
                typeLabel,
                DateTime.Now.Date,
                userId,
                rebate?.UserId,
                basePrice);
            if (!commonCheck.IsSuccess)
                return commonCheck;
            if (!IsClubTargetValid(rebate, typeLabel, targetId))
                return new BaseResultDto<RebateVDto>(false, Resource.Notification.NotPossibleUseRebateCode, null);

            var rebateDto = mapper.Map<RebateVDto>(rebate);
            rebateDto.FinalPrice = ApplyClubMaximum(rebate, rebateDto.IsPriceRebate
                ? rebateDto.PriceValue
                : Math.Round(basePrice * (rebateDto.PriceValue / 100)));
            return new BaseResultDto<RebateVDto>(true, Resource.Notification.Success, rebateDto);
        }
        private void UpdateUsageStatistics(Rebate rebate, long userId)
        {
            if (rebate == null) return;
            rebate.UsedCount++;
            _context.Rebate.Update(rebate);

            var userUsage = _context.UserRebates.FirstOrDefault(x => x.UserId == userId && x.RebateId == rebate.Id);
            if (userUsage == null)
            {
                _context.UserRebates.Add(new UserRebate
                {
                    UserId = userId,
                    RebateId = rebate.Id,
                    UsageCount = 1
                });
            }
            else
            {
                userUsage.UsageCount++;
                _context.UserRebates.Update(userUsage);
            }

            _context.SaveChanges();
        }

        private static double ApplyClubMaximum(Rebate rebate, double calculatedValue)
        {
            var maximum = rebate?.ClubCoupon?.RewardRedemption?.RewardTemplate?.MaximumBenefitValue;
            return maximum.HasValue
                ? Math.Min(calculatedValue, decimal.ToDouble(maximum.Value))
                : calculatedValue;
        }

        private static bool IsClubTargetValid(Rebate rebate, Cart cart)
        {
            var targets = rebate?.ClubCoupon?.RewardRedemption?.RewardTemplate?.Targets;
            if (targets == null || targets.Count == 0 ||
                targets.Any(item => item.TargetType == ClubRewardTargetTypeEnum.Global))
                return true;

            var activeStore = cart.CartStores?.FirstOrDefault(item => item.Active);
            if (activeStore == null)
                return false;
            return targets.Any(target => target.TargetType switch
            {
                ClubRewardTargetTypeEnum.Store => target.TargetId == activeStore.StoreId,
                ClubRewardTargetTypeEnum.Product => activeStore.CartItems.Any(item => item.ProductItem.ProductId == target.TargetId),
                ClubRewardTargetTypeEnum.ProductCategory => activeStore.CartItems.Any(item => item.ProductItem.Product.CategoryId == target.TargetId),
                ClubRewardTargetTypeEnum.City => cart.Address?.CityId == target.TargetId,
                _ => false
            });
        }

        private bool IsClubTargetValid(Rebate rebate, CompanionReserve reserve)
        {
            var targets = rebate?.ClubCoupon?.RewardRedemption?.RewardTemplate?.Targets;
            if (targets == null || targets.Count == 0 || targets.Any(item => item.TargetType == ClubRewardTargetTypeEnum.Global))
                return true;
            var assistance = _context.CompanionAssistances.AsNoTracking()
                .FirstOrDefault(item => item.Id == reserve.CompanionAssistanceId);
            if (assistance == null)
                return false;
            return targets.Any(target => target.TargetType switch
            {
                ClubRewardTargetTypeEnum.Companion => target.TargetId == assistance.CompanionId,
                ClubRewardTargetTypeEnum.Assistance => target.TargetId == assistance.AssistanceId,
                ClubRewardTargetTypeEnum.CompanionPackage => reserve.CompanionAssistancePackages != null && reserve.CompanionAssistancePackages.Any(item => item.Id == target.TargetId),
                _ => false
            });
        }

        private static bool IsClubTargetValid(Rebate rebate, PansionReserve reserve)
        {
            var targets = rebate?.ClubCoupon?.RewardRedemption?.RewardTemplate?.Targets;
            return targets == null || targets.Count == 0 ||
                targets.Any(item => item.TargetType == ClubRewardTargetTypeEnum.Global ||
                    item.TargetType == ClubRewardTargetTypeEnum.Pansion && item.TargetId == reserve.PansionId);
        }

        private static bool IsClubTargetValid(Rebate rebate, string typeLabel, long? targetId)
        {
            var targets = rebate?.ClubCoupon?.RewardRedemption?.RewardTemplate?.Targets;
            if (targets == null || targets.Count == 0 || targets.Any(item => item.TargetType == ClubRewardTargetTypeEnum.Global))
                return true;
            if (typeLabel != RebateTypeLabels.PastilAI)
                return true;
            return typeLabel == RebateTypeLabels.PastilAI && targetId.HasValue &&
                targets.Any(item => item.TargetType == ClubRewardTargetTypeEnum.PastilAIPlan && item.TargetId == targetId);
        }

        public void IncreaseUseCount(ProductOrder order)
        {
            if (order.Rebate != null)
            {
                UpdateUsageStatistics(order.Rebate, order.UserId);
                CompleteClubCoupon(order.Rebate.Id, order.RebatePrice, order.Id, null);
            }
        }

        public void IncreaseUseCount(CompanionReserve reserve)
        {
            if (reserve.Rebate != null)
            {
                UpdateUsageStatistics(reserve.Rebate, reserve.BookerId);
                CompleteClubCoupon(reserve.Rebate.Id, reserve.RebatePrice, null, reserve.Id);
            }
        }

        public void IncreaseUseCount(Cargo cargo)
        {
            if (cargo.Rebate != null)
                UpdateUsageStatistics(cargo.Rebate, cargo.UserPet.UserId);
        }

        public void IncreaseUseCount(Trip trip)
        {
            if (trip.Rebate != null)
                UpdateUsageStatistics(trip.Rebate, trip.UserPet.UserId);
        }

        public void IncreaseUseCount(CompanionInsurancePackageSale insurance)
        {
            if (insurance.Rebate != null)
                UpdateUsageStatistics(insurance.Rebate, insurance.UserPet.UserId);
        }

        public void IncreaseUseCount(PansionReserve pansion)
        {
            if (pansion.Rebate != null)
            {
                UpdateUsageStatistics(pansion.Rebate, pansion.BookerId);
                CompleteClubCoupon(pansion.Rebate.Id, pansion.RebatePrice, null, pansion.Id);
            }
        }

        public void IncreaseUseCount(Rebate rebate, long userId, double fundedValue = 0)
        {
            UpdateUsageStatistics(rebate, userId);
            CompleteClubCoupon(rebate?.Id, fundedValue, null, null);
        }

        private void CompleteClubCoupon(long? rebateId, double fundedValue, string orderId, long? reservationId)
        {
            if (!rebateId.HasValue)
                return;
            var coupon = _context.ClubCoupons.AsTracking()
                .Include(item => item.RewardRedemption)
                    .ThenInclude(item => item.RewardTemplate)
                .FirstOrDefault(item => item.RebateId == rebateId.Value && !item.Used);
            if (coupon == null)
                return;

            coupon.Used = true;
            coupon.UsedDate = DateTimeOffset.UtcNow;
            coupon.OrderId = orderId;
            coupon.ReservationId = reservationId;
            var target = _context.ClubRewardTargets.AsNoTracking()
                .FirstOrDefault(item => item.RewardTemplateId == coupon.RewardRedemption.RewardTemplateId &&
                    item.TargetType != ClubRewardTargetTypeEnum.Global);
            _context.ClubRewardCostTransactions.Add(new ClubRewardCostTransaction
            {
                RewardRedemptionId = coupon.RewardRedemptionId,
                UserId = coupon.UserId,
                BusinessType = target?.TargetType ?? ClubRewardTargetTypeEnum.Global,
                BusinessId = target?.TargetId,
                RewardType = coupon.RewardRedemption.RewardTemplate.RewardType,
                GrossValue = Convert.ToDecimal(fundedValue),
                PastilFundedValue = Convert.ToDecimal(fundedValue),
                OrderId = orderId,
                ReservationId = reservationId,
                CreateDate = DateTime.UtcNow
            });
            _context.SaveChanges();
        }
    }
}
