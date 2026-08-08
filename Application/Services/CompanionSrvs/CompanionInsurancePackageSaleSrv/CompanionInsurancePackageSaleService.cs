using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSaleSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSrv.Iface;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSrv.Dto;
using Application.Services.Content.CargoSrv.Dto;
using Application.Services.Order.RebateSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Application.Services.TripSrv.TripSrv.Dto;
using AutoMapper;
using Entities.Entities;
using Entities.Entities.CompanionField;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Triangulate.Tri;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZarinPal.Class;

namespace Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSrv
{
    public class CompanionInsurancePackageSaleService : CommonSrv<CompanionInsurancePackageSale, CompanionInsurancePackageSaleDto>, ICompanionInsurancePackageSaleService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;
        private readonly IRebateService _rebateService;
        private readonly IWalletService _walletService;

        public CompanionInsurancePackageSaleService(IDataBaseContext _context, IMapper mapper, IWalletService walletService, IRebateService rebateService, ICurrentUserHelper currentUser) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._currentUser = currentUser;
            this._rebateService = rebateService;
            this._walletService = walletService;
        }
        public override async Task<BaseResultDto<CompanionInsurancePackageSaleDto>> FindAsyncDto(long id)
        {
            var item = await _context.CompanionInsurancePackageSales.Include(s => s.Address).Include(s => s.UserPet).ThenInclude(s => s.User).Include(s => s.UserPet).ThenInclude(s => s.Pet).Include(s => s.CompanionInsurancePackage).ThenInclude(s => s.Companion).Include(s => s.CompanionInsurancePackage).ThenInclude(s => s.Pet).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<CompanionInsurancePackageSaleDto>(true, mapper.Map<CompanionInsurancePackageSaleDto>(item));
            }
            return new BaseResultDto<CompanionInsurancePackageSaleDto>(false, mapper.Map<CompanionInsurancePackageSaleDto>(item));
        }
        public async Task<BaseResultDto<CompanionInsurancePackageSaleVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.CompanionInsurancePackageSales.Include(s => s.Address).Include(s => s.UserPet).ThenInclude(s => s.User).Include(s => s.UserPet).ThenInclude(s => s.Pet).Include(s => s.CompanionInsurancePackage).ThenInclude(s => s.Companion).Include(s => s.CompanionInsurancePackage).ThenInclude(s => s.Pet).FirstOrDefaultAsync(s => s.Id == id && s.UserPet.UserId == _currentUser.CurrentUser.UserId);
            if (item != null)
            {
                return new BaseResultDto<CompanionInsurancePackageSaleVDto>(true, mapper.Map<CompanionInsurancePackageSaleVDto>(item));
            }
            return new BaseResultDto<CompanionInsurancePackageSaleVDto>(false, mapper.Map<CompanionInsurancePackageSaleVDto>(item));
        }

        public CompanionInsurancePackageSaleSearchDto Search(CompanionInsurancePackageSaleInputDto baseSearchDto)
        {
            var model = _context.CompanionInsurancePackageSales.Include(s => s.Address).Include(s => s.UserPet).ThenInclude(s => s.User).Include(s => s.UserPet).ThenInclude(s => s.Pet).Include(s => s.CompanionInsurancePackage).ThenInclude(s => s.Companion).Include(s => s.CompanionInsurancePackage).ThenInclude(s => s.Pet).AsQueryable();

            if (baseSearchDto.CompanionId.HasValue)
            {
                model = model.Where(s => s.CompanionInsurancePackage.CompanionId == baseSearchDto.CompanionId.Value);
            }
            if (baseSearchDto.IsPaid.HasValue)
            {
                model = model.Where(s => s.IsPaid == baseSearchDto.IsPaid.Value);
            }
            if (baseSearchDto.FromDate.HasValue)
            {
                model = model.Where(s => s.CreateDate >= baseSearchDto.FromDate.Value);
            }
            if (baseSearchDto.ToDate.HasValue)
            {
                model = model.Where(s => s.CreateDate <= baseSearchDto.ToDate.Value);
            }
            if (baseSearchDto.CompanionInsurancePackageId.HasValue)
            {
                model = model.Where(s => s.CompanionInsurancePackageId == baseSearchDto.CompanionInsurancePackageId.Value);
            }
            if (baseSearchDto.UserPetId.HasValue)
            {
                model = model.Where(s => s.UserPetId == baseSearchDto.UserPetId.Value);
            }
            if (baseSearchDto.UserId.HasValue)
            {
                model = model.Where(s => s.UserPet.UserId == baseSearchDto.UserId.Value);
            }
            if (baseSearchDto.ManualPay.HasValue)
            {
                model = model.Where(s => s.ManualPayDate.HasValue);
            }
            switch (baseSearchDto.SortBy)
            {
                case Common.Enumerable.SortEnum.New:
                    {
                        model = model.OrderByDescending(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Old:
                    {
                        model = model.OrderBy(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Expensive:
                    {
                        model = model.OrderByDescending(s => s.Price);
                        break;
                    }
                case Common.Enumerable.SortEnum.Inexpensive:
                    {
                        model = model.OrderBy(s => s.Price);
                        break;
                    }
                default:
                    break;
            }
            return new CompanionInsurancePackageSaleSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<CompanionInsurancePackageSaleDto>> InsertAsyncDto(CompanionInsurancePackageSaleDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<CompanionInsurancePackageSaleDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                var item = mapper.Map<CompanionInsurancePackageSale>(dto);
                item.IsPaid = false;
                item.RebateId = null;
                item.RebatePrice = 0;
                item.CreateDate = DateTime.Now;
                item.PaymentPrice = item.Price;

                var insurence = await _context.CompanionInsurancePackages.FirstOrDefaultAsync(s => s.Id == dto.CompanionInsurancePackageId);

                item.EndDate = dto.StartDate.Date.AddDays(insurence.DayCount);
                item.Price = insurence.Price;
                var petType = await _context.UserPets.Include(s => s.Pet).FirstOrDefaultAsync(s => s.Id == dto.UserPetId);
                if (petType.PetId != insurence.PetId)
                {
                    return new BaseResultDto<CompanionInsurancePackageSaleDto>(false, Resource.Notification.ThisInsuranceIsNotForThisPet, dto);
                }
                await _context.CompanionInsurancePackageSales.AddAsync(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto<CompanionInsurancePackageSaleDto>(true, mapper.Map<CompanionInsurancePackageSaleDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionInsurancePackageSaleDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }

        public async Task<BaseResultDto<CompanionInsurancePackageSaleManualPayDto>> CompanionInsurancePackageSaleManualPayAsync(CompanionInsurancePackageSaleManualPayDto dto)
        {
            var insurance = await _context.CompanionInsurancePackageSales.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id);

            insurance.PaymentPrice = insurance.Price;
            insurance.IsPaid = true;
            insurance.ManualPayDate = DateTime.Now;

            _context.CompanionInsurancePackageSales.Update(insurance);
            await _context.SaveChangesAsync();
            return new BaseResultDto<CompanionInsurancePackageSaleManualPayDto>(true, Resource.Notification.Success, dto);
        }

        public async Task<BaseResultDto> CompanionInsurancePackageSalePaymentCallback(long? insuranceId, bool fromWallet = false)
        {
            try
            {
                var insurance = await _context.CompanionInsurancePackageSales.Include(s => s.UserPet).AsTracking().FirstOrDefaultAsync(s => s.Id == insuranceId);
                if (insurance == null)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                if (insurance.IsPaid)
                    return new BaseResultDto(true);

                if (fromWallet && insurance.FromWallet && insurance.WalletPrice > 0)
                {
                    var walletItem = new WalletDto() { Painding = false, Amount = insurance.WalletPrice, UserId = insurance.UserPet.UserId, CompanionInsurancePackageSaleId = insurance.Id };
                    var walletResult = await _walletService.InsertUpdateInsuranceAsync(walletItem, true);
                    if (!walletResult.IsSuccess)
                        return new BaseResultDto(false);
                }
                insurance.IsPaid = true;
                _context.CompanionInsurancePackageSales.Update(insurance);
                await _context.SaveChangesAsync();
                _rebateService.IncreaseUseCount(insurance);
                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception)
            {
                return new BaseResultDto(false);

            }
        }

        public async Task<BaseResultDto> SetRebateCodeAsyncDto(CompanionInsurancePackageSaleSetRebateCodeDto dto)
        {
            var item = await _context.CompanionInsurancePackageSales.Include(s => s.UserPet).AsTracking().FirstOrDefaultAsync(s =>
                s.Id == dto.Id && s.UserPet.UserId == _currentUser.CurrentUser.UserId && !s.IsPaid);
            if (item == null)
            {
                return new BaseResultDto<CompanionInsurancePackageSaleSetRebateCodeDto>(false, Resource.Notification.NothingFound, dto);
            }
            if (item.Price == 0)
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.FinalPriceIsNotAvailable);
            }
            if (string.IsNullOrEmpty(dto.RebateCode))
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
            }
            var rebate = _rebateService.GetRebateByCodeAsync(item, dto.RebateCode);
            if (rebate.IsSuccess)
            {
                item.Rebate = null;
                item.RebateId = rebate.Data.Id;
                item.RebatePrice = rebate.Data.FinalPrice;
                item.PaymentPrice = item.Price - item.RebatePrice;

                _context.CompanionInsurancePackageSales.Update(item);
                await _context.SaveChangesAsync();
                return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
            }
            else
            {
                return new BaseResultDto(isSuccess: false, messages: rebate.Messages);
            }
        }

        public async Task<BaseResultDto> ClearRebateCodeAsync(long id)
        {
            var item = await _context.CompanionInsurancePackageSales.Include(s => s.UserPet).AsTracking().FirstOrDefaultAsync(s =>
                s.Id == id && s.UserPet.UserId == _currentUser.CurrentUser.UserId && !s.IsPaid);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            item.RebateId = null;
            item.RebatePrice = 0;
            item.PaymentPrice = item.Price;

            _context.CompanionInsurancePackageSales.Update(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }

        public async Task<BaseResultDto> SetWalletAsyncDto(CompanionInsurancePackageSaleSetWalletDto dto)
        {
            var item = await _context.CompanionInsurancePackageSales.Include(s => s.UserPet).AsTracking().FirstOrDefaultAsync(s =>
                s.Id == dto.Id &&
                s.UserPet.UserId == _currentUser.CurrentUser.UserId &&
                !s.IsPaid);
            if (item == null)
            {
                return new BaseResultDto<CompanionInsurancePackageSaleSetWalletDto>(false, Resource.Notification.NothingFound, dto);
            }
            if (dto.FromWallet)
            {
                item.FromWallet = true;
                item.WalletPrice = item.PaymentPrice;
            }
            else
            {
                item.FromWallet = false;
                item.WalletPrice = 0;
            }
            _context.CompanionInsurancePackageSales.Update(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }
    }
}
