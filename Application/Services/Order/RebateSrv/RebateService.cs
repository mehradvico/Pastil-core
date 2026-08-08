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
            catch (Exception ex)
            {
                return new BaseResultDto<RebateDto>(isSuccess: false, val: ex.Message, data: dto);
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

            if (rebate.ProductId.HasValue)
            {
                var cartItem = _context.CartItems.FirstOrDefault(s =>
                    s.CartStore.CartId == cart.Id && s.ProductItem.ProductId == rebate.ProductId);

                if (cartItem != null)
                {
                    basePrice = cartItem.ProductItem.Price;
                }
                else
                {
                    return new BaseResultDto<RebateVDto>(false, Resource.Notification.NotPossibleUseRebateCode, null);
                }
            }

            var rebateDto = mapper.Map<RebateVDto>(rebate);
            rebateDto.FinalPrice = rebateDto.IsPriceRebate
                ? rebateDto.PriceValue
                : Math.Round(basePrice * (rebateDto.PriceValue / 100));

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

            var rebateDto = mapper.Map<RebateVDto>(rebate);
            rebateDto.FinalPrice = rebateDto.IsPriceRebate
                ? rebateDto.PriceValue
                : Math.Round(basePrice * (rebateDto.PriceValue / 100));

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
                .FirstOrDefault(x => !x.Deleted && x.CodeValue == normalizedCode);
        }

        private BaseResultDto<RebateVDto> ValidateRebateCommon(Rebate rebate, string typeLabel, DateTime justNow, long? userId, long? rebateUserId, double basePrice)
        {
            if (rebate == null || !rebate.Active || rebate.Deleted)
            {
                return new BaseResultDto<RebateVDto>(false, Resource.Notification.NothingFound, null);
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
            rebateDto.FinalPrice = rebateDto.IsPriceRebate
                ? rebateDto.PriceValue
                : Math.Round(basePrice * (rebateDto.PriceValue / 100));

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
            rebateDto.FinalPrice = rebateDto.IsPriceRebate
                ? rebateDto.PriceValue
                : Math.Round(basePrice * (rebateDto.PriceValue / 100));

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
            rebateDto.FinalPrice = rebateDto.IsPriceRebate
                ? rebateDto.PriceValue
                : Math.Round(basePrice * (rebateDto.PriceValue / 100));

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

            var rebateDto = mapper.Map<RebateVDto>(rebate);
            rebateDto.FinalPrice = rebateDto.IsPriceRebate
                ? rebateDto.PriceValue
                : Math.Round(basePrice * (rebateDto.PriceValue / 100));

            return new BaseResultDto<RebateVDto>(true, Resource.Notification.Success, rebateDto);
        }

        public BaseResultDto<RebateVDto> GetRebateByCodeAsync(double basePrice, long userId, string typeLabel, string code)
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

            var rebateDto = mapper.Map<RebateVDto>(rebate);
            rebateDto.FinalPrice = rebateDto.IsPriceRebate
                ? rebateDto.PriceValue
                : Math.Round(basePrice * (rebateDto.PriceValue / 100));
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

        public void IncreaseUseCount(ProductOrder order)
        {
            if (order.Rebate != null)
                UpdateUsageStatistics(order.Rebate, order.UserId);
        }

        public void IncreaseUseCount(CompanionReserve reserve)
        {
            if (reserve.Rebate != null)
                UpdateUsageStatistics(reserve.Rebate, reserve.BookerId);
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
                UpdateUsageStatistics(pansion.Rebate, pansion.BookerId);
        }

        public void IncreaseUseCount(Rebate rebate, long userId)
        {
            UpdateUsageStatistics(rebate, userId);
        }
    }
}
