using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Enumerable.Message;
using Application.Common.Helpers;
using Application.Common.Helpers.Iface;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CompanionSrvs.CompanionReserveSrv.Dto;
using Application.Services.Content.CargoSrv.Dto;
using Application.Services.Content.CargoSrv.Iface;
using Application.Services.Order.RebateSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Application.Services.Setting.MessageSenderSrv.Iface;
using Application.Services.Setting.NoticeSrv.Iface;
using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Content.CargoSrv
{
    public class CargoService : CommonSrv<Cargo, CargoDto>, ICargoService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IAdminSettingHelper _adminSettingHelper;
        private readonly ICurrentUserHelper _currentUser;
        private readonly IMessageSenderService _messageSenderService;
        private readonly IRebateService _rebateService;
        private readonly IWalletService _walletService;
        private readonly INoticeService _notificationService;

        public CargoService(IDataBaseContext _context, IMapper mapper, INoticeService notificationService, IWalletService walletService, IRebateService rebateService, IMessageSenderService messageSenderService, IAdminSettingHelper adminSettingHelper, ICurrentUserHelper currentUser) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._adminSettingHelper = adminSettingHelper;
            this._currentUser = currentUser;
            this._messageSenderService = messageSenderService;
            this._rebateService = rebateService;
            this._walletService = walletService;
            this._notificationService = notificationService;
        }
        public async Task<BaseResultDto<CargoVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.Cargoes.Include(s => s.FromState).ThenInclude(s => s.Country).Include(s => s.ToState).ThenInclude(s => s.Country).Include(s => s.UserPet).ThenInclude(s => s.User).Include(s => s.UserPet).ThenInclude(s => s.Pet).Include(s => s.Status).FirstOrDefaultAsync(s => s.Id == id && s.UserPet.UserId == _currentUser.CurrentUser.UserId);
            if (item != null)
            {
                return new BaseResultDto<CargoVDto>(true, mapper.Map<CargoVDto>(item));
            }
            return new BaseResultDto<CargoVDto>(false, mapper.Map<CargoVDto>(item));
        }

        public override async Task<BaseResultDto<CargoDto>> FindAsyncDto(long id)
        {
            var item = await _context.Cargoes.Include(s => s.FromState).ThenInclude(s => s.Country).Include(s => s.ToState).ThenInclude(s => s.Country).Include(s => s.UserPet).ThenInclude(s => s.User).Include(s => s.UserPet).ThenInclude(s => s.Pet).Include(s => s.Status).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<CargoDto>(true, mapper.Map<CargoDto>(item));
            }
            return new BaseResultDto<CargoDto>(false, mapper.Map<CargoDto>(item));
        }

        public CargoSearchDto Search(CargoInputDto baseSearchDto)
        {
            var query = _context.Cargoes.Include(s => s.FromState).ThenInclude(s => s.Country).Include(s => s.ToState).ThenInclude(s => s.Country).Include(s => s.UserPet).ThenInclude(s => s.User).Include(s => s.UserPet).ThenInclude(s => s.Pet).Include(s => s.Status).AsQueryable();
            DateTime now = DateTime.Now;

            if (baseSearchDto.UserPetId.HasValue)
            {
                query = query.Where(s => s.UserPetId == baseSearchDto.UserPetId.Value);
            }
            if (baseSearchDto.UserId.HasValue)
            {
                query = query.Where(s => s.UserPet.UserId == baseSearchDto.UserId.Value);
            }
            if (baseSearchDto.FromStateId.HasValue)
            {
                query = query.Where(s => s.FromStateId == baseSearchDto.FromStateId.Value);
            }
            if (baseSearchDto.ToStateId.HasValue)
            {
                query = query.Where(s => s.ToStateId == baseSearchDto.ToStateId.Value);
            }
            if (baseSearchDto.FromCountryId.HasValue)
            {
                query = query.Where(s => s.FromState.CountryId == baseSearchDto.FromCountryId.Value);
            }
            if (baseSearchDto.ToCountryId.HasValue)
            {
                query = query.Where(s => s.ToState.CountryId == baseSearchDto.ToCountryId.Value);
            }
            if (baseSearchDto.IsAccompany.HasValue)
            {
                query = query.Where(s => s.Accompany == baseSearchDto.IsAccompany.Value);
            }
            if (baseSearchDto.IsPaid.HasValue)
            {
                query = query.Where(s => s.IsPaid == baseSearchDto.IsPaid.Value);
            }
            if (baseSearchDto.StatusId.HasValue)
            {
                query = query.Where(s => s.StatusId == baseSearchDto.StatusId.Value);
            }
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                query = query.Where(s => s.UserPet.User.FirstName.Contains(baseSearchDto.Q) || s.UserPet.User.LastName.Contains(baseSearchDto.Q) || s.UserPet.User.Mobile.Contains(baseSearchDto.Q));
            }
            switch (baseSearchDto.SortBy)
            {
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
                case Common.Enumerable.SortEnum.Expensive:
                    {
                        query = query.OrderByDescending(s => s.Price);
                        break;
                    }
                case Common.Enumerable.SortEnum.Inexpensive:
                    {
                        query = query.OrderBy(s => s.Price);
                        break;
                    }
                default:
                    break;
            }
            return new CargoSearchDto(baseSearchDto, query, mapper);
        }

        public override async Task<BaseResultDto<CargoDto>> InsertAsyncDto(CargoDto dto)
        {
            try
            {
                var item = mapper.Map<Cargo>(dto);
                var modelCheker = ModelHelper<CargoDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }

                var fromState = await _context.States.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.FromStateId);
                var toState = await _context.States.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.ToStateId);

                //var defaultDomesticPrice = _adminSettingHelper.CargoPrice.DefaultDomesticPrice;
                //var defaultForeignPrice = _adminSettingHelper.CargoPrice.DefaultForeignPrice;
                //var notAccompanyPrice = _adminSettingHelper.CargoPrice.NotAccompanyPrice;
                //var returnPrice = _adminSettingHelper.CargoPrice.ReturnPrice;

                //double defaultPrice = fromState.CountryId == toState.CountryId
                //    ? defaultDomesticPrice
                //    : defaultForeignPrice;

                //double totalPrice = defaultPrice;
                //item.DefaultPrice = defaultPrice;

                //if (!dto.Accompany)
                //{
                //    totalPrice += notAccompanyPrice;
                //    item.NotAccompanyPrice = notAccompanyPrice;
                //}

                //if (dto.DateReturn.HasValue)
                //{
                //    totalPrice += returnPrice;
                //    item.ReturnPrice = returnPrice;
                //}
                item.RebateId = null;
                //item.Price = totalPrice;
                item.IsPaid = false;
                item.CreateDate = DateTime.Now;
                item.StatusId = (long)CargoStatusEnum.CargoStatus_Requested;
                item.PaymentPrice = item.Price;

                await _context.Cargoes.AddAsync(item);
                await _context.SaveChangesAsync();

                var user = _context.UserPets.Include(s => s.User).FirstOrDefault(s => s.Id == item.UserPetId);
                var admin = _adminSettingHelper.BaseAdminSetting.AdminMobiles;

                await _messageSenderService.SendMessageAsync(messageType: MessageTypeEnum.CargoUser, mobileReceptor: user.User.Mobile, emailReceptor: null, token1: user.User.FirstName);
                await _messageSenderService.SendMessageAsync(messageType: MessageTypeEnum.CargoAdmin, mobileReceptor: admin, emailReceptor: null, token1: user.User.Mobile);


                return new BaseResultDto<CargoDto>(true, mapper.Map<CargoDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CargoDto>(false, ex.Message, dto);
            }
        }

        public async Task<BaseResultDto> CargoPaymentCallback(long? cargoId, bool fromWallet = false)
        {
            try
            {
                var cargo = await _context.Cargoes.Include(s => s.UserPet).AsTracking().FirstOrDefaultAsync(s => s.Id == cargoId);
                if (cargo == null)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                if (cargo.IsPaid)
                    return new BaseResultDto(true);

                if (fromWallet && cargo.FromWallet && cargo.WalletPrice > 0)
                {
                    var walletItem = new WalletDto() { Painding = false, Amount = cargo.WalletPrice, UserId = cargo.UserPet.UserId, CargoId = cargo.Id };
                    var walletResult = await _walletService.InsertUpdateCargoAsync(walletItem, true);
                    if (!walletResult.IsSuccess)
                        return new BaseResultDto(false);
                }
                cargo.IsPaid = true;
                _context.Cargoes.Update(cargo);
                await _context.SaveChangesAsync();
                _rebateService.IncreaseUseCount(cargo);
                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception)
            {
                return new BaseResultDto(false);

            }
        }

        public async Task<BaseResultDto<CargoUpdateStatusDto>> CargoUpdateStatusAsyncDto(CargoUpdateStatusDto dto)
        {
            var trip = await _context.Cargoes.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id);
            if (dto.StatusId == (long)CargoStatusEnum.CargoStatus_Requested)
            {
                return new BaseResultDto<CargoUpdateStatusDto>(false, Resource.Notification.PleaseChangeTheStatus, dto);
            }
            if (dto.StatusId == (long)CargoStatusEnum.CargoStatus_Canceled)
            {
                if (!string.IsNullOrEmpty(dto.StatusDetail))
                {
                    return new BaseResultDto<CargoUpdateStatusDto>(false, Resource.Notification.PleaseEnterTheAdminDetail, dto);
                }
                trip.StatusId = dto.StatusId;
                trip.StatusDetail = dto.StatusDetail;
            }
            else
            {
                trip.StatusId = dto.StatusId;
                trip.StatusDetail = dto.StatusDetail;
            }
            _context.Cargoes.Update(trip);
            await _context.SaveChangesAsync();
            return new BaseResultDto<CargoUpdateStatusDto>(true, Resource.Notification.Success, dto);
        }

        public async Task<BaseResultDto> SetRebateCodeAsyncDto(CargoSetRebateCodeDto dto)
        {
            var item = await _context.Cargoes.Include(s => s.UserPet).AsTracking().FirstOrDefaultAsync(s =>
                s.Id == dto.Id &&
                s.UserPet.UserId == _currentUser.CurrentUser.UserId &&
                s.StatusId == (long)CargoStatusEnum.CargoStatus_Accepted);

            if (item == null)
            {
                return new BaseResultDto<CargoSetRebateCodeDto>(false, Resource.Notification.NothingFound, dto);
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
                _context.Cargoes.Update(item);
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
            var item = await _context.Cargoes.Include(s => s.UserPet).AsTracking().FirstOrDefaultAsync(s =>
                s.Id == id && s.UserPet.UserId == _currentUser.CurrentUser.UserId && !s.IsPaid);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            item.RebateId = null;
            item.RebatePrice = 0;
            item.PaymentPrice = item.Price;

            _context.Cargoes.Update(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }

        public async Task<BaseResultDto> SetWalletAsyncDto(CargoSetWalletDto dto)
        {
            var item = await _context.Cargoes.Include(s => s.UserPet).AsTracking().FirstOrDefaultAsync(s =>
                s.Id == dto.Id &&
                s.UserPet.UserId == _currentUser.CurrentUser.UserId &&
                !s.IsPaid &&
                s.StatusId == (long)CargoStatusEnum.CargoStatus_Accepted);
            if (item == null)
            {
                return new BaseResultDto<CargoSetWalletDto>(false, Resource.Notification.NothingFound, dto);
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
            _context.Cargoes.Update(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }
    }
}
