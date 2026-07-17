using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Enumerable.Message;
using Application.Common.Helpers;
using Application.Common.Helpers.Iface;
using Application.Common.Service;
using Application.Services.Order.RebateSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Application.Services.Setting.CodeSrv.Iface;
using Application.Services.Setting.MessageSenderSrv.Iface;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using Application.Services.TripSrv.PriceCalculationSrv.Iface;
using Application.Services.TripSrv.TripOptionSrv.Iface;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using AutoMapper;
using DocumentFormat.OpenXml.Office.CustomUI;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.TripSrv.TripSrv
{
    public class TripService : CommonSrv<Trip, TripDto>, ITripService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ITripOptionService _tripOptionService;
        private readonly ICodeService _codeService;
        private readonly IMessageSenderService _messageSender;
        private readonly INoticeService _noticeService;
        private readonly IAdminSettingHelper _adminSettingHelper;
        private readonly IRebateService _rebateService;
        private readonly IWalletService _walletService;
        public TripService(IDataBaseContext _context, IMapper mapper, IWalletService walletService, IRebateService rebateService, IAdminSettingHelper adminSettingHelper, IPriceCalculationService priceCalculationService, ITripOptionService tripOptionService, ICodeService codeService, IMessageSenderService messageSender, INoticeService noticeService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            _priceCalculationService = priceCalculationService;
            _tripOptionService = tripOptionService;
            _codeService = codeService;
            _messageSender = messageSender;
            _noticeService = noticeService;
            _adminSettingHelper = adminSettingHelper;
            this._rebateService = rebateService;
            this._walletService = walletService;
        }
        //public async Task<BaseResultDto<TripVDto>> UpdateVDtoAsync(TripVDto vDto)
        //{
        //    var current = _context.Trips.FirstOrDefault(s => s.Id == vDto.Id);
        //    if (current != null)
        //    {
        //        if (current.DriverStatusId != (long)DriverStatusEnum.DriverStatus_Requested || current.TripStatusId != (long)TripStatusEnum.TripStatus_Requested)
        //        {
        //            return new BaseResultDto<TripVDto>(false, val: Resource.Notification.Unsuccess, null);

        //        }
        //    }
        //    var item = mapper.Map<TripDto>(vDto);
        //    var update = UpdateDto(item);
        //    if (update.IsSuccess)
        //    {
        //        return await FindAsyncVDto(item.Id);
        //    }
        //    else
        //        return new BaseResultDto<TripVDto>(false, update.Messages, null);
        //}
        //public async Task<BaseResultDto> GetCurrentTripForDriverAsync(long driverId)
        //{
        //    var trip = await _context.Trips.Include(s => s.FromCity).Include(s => s.DriverStatus).Include(s => s.TripStatus).Include(s => s.Driver).FirstOrDefaultAsync(s => s.DriverId == driverId && s.IsOnline && s.DriverStatus.Label == DriverStatusEnum.DriverStatus_Accepted.ToString() && s.TripStatus.Label == TripStatusEnum.TripStatus_Accepted.ToString());
        //    return new BaseResultDto<TripVDto>(trip != null, mapper.Map<TripVDto>(trip));
        //}
        //public async Task<BaseResultDto<TripVDto>> GetCurrentTripForPassengerAsync(long passengerId, double minute = 10)
        //{
        //    var trip = await _context.Trips.Include(s => s.FromCity).Include(s => s.DriverStatus).Include(s => s.TripStatus).Include(s => s.Driver).FirstOrDefaultAsync(s => s.UserPet.UserId == passengerId
        //    && s.IsOnline
        //    && ((s.DriverStatusId == (long)DriverStatusEnum.DriverStatus_Accepted
        //    && s.TripStatusId == (long)TripStatusEnum.TripStatus_Accepted)
        //    || (s.DriverStatusId == (long)DriverStatusEnum.DriverStatus_Requested
        //    && s.TripStatusId == (long)TripStatusEnum.TripStatus_Requested && s.CreateDate.AddMinutes(minute) > DateTime.Now)));

        //    return new BaseResultDto<TripVDto>(trip != null, mapper.Map<TripVDto>(trip));
        //}

        public async Task<BaseResultDto<TripVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.Trips.Include(s => s.FromCity).Include(s => s.TripStop).Include(s => s.TripOptions).Include(s => s.UserPet).ThenInclude(s => s.Pet).Include(s => s.UserPet).ThenInclude(s => s.User).Include(s => s.DriverStatus).Include(s => s.TripStatus).Include(s => s.Driver).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<TripVDto>(true, mapper.Map<TripVDto>(item));
            }
            return new BaseResultDto<TripVDto>(false, mapper.Map<TripVDto>(item));
        }

        public override async Task<BaseResultDto<TripDto>> FindAsyncDto(long id)
        {
            var item = await _context.Trips.Include(s => s.FromCity).Include(s => s.TripStop).Include(s => s.TripOptions).Include(s => s.UserPet).ThenInclude(s => s.Pet).Include(s => s.UserPet).ThenInclude(s => s.User).Include(s => s.DriverStatus).Include(s => s.TripStatus).Include(s => s.Driver).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<TripDto>(true, mapper.Map<TripDto>(item));
            }
            return new BaseResultDto<TripDto>(false, mapper.Map<TripDto>(item));
        }

        public TripSearchDto Search(TripInputDto baseSearchDto)
        {
            var model = _context.Trips.Include(s => s.FromCity).Include(s => s.TripStop).Include(s => s.TripOptions).Include(s => s.UserPet).ThenInclude(s => s.Pet).Include(s => s.UserPet).ThenInclude(s => s.User).Include(s => s.DriverStatus).Include(s => s.TripStatus).Include(s => s.Driver).AsQueryable();

            if (baseSearchDto.FromCityId.HasValue)
            {
                model = model.Where(s => s.FromCityId == baseSearchDto.FromCityId);
            }
            if (baseSearchDto.FromDate.HasValue)
            {
                model = model.Where(s => s.CreateDate >= baseSearchDto.FromDate);
            }
            if (baseSearchDto.ToDate.HasValue)
            {
                model = model.Where(s => s.CreateDate <= baseSearchDto.ToDate);
            }
            if (baseSearchDto.PassengerId.HasValue)
            {
                model = model.Where(s => s.UserPet.UserId == baseSearchDto.PassengerId);
            }
            if (baseSearchDto.DriverId.HasValue)
            {
                model = model.Where(s => s.DriverId == baseSearchDto.DriverId);
            }
            if (baseSearchDto.IsPaid.HasValue)
            {
                model = model.Where(s => s.IsPaid == baseSearchDto.IsPaid);
            }
            if (baseSearchDto.ManualPay.HasValue)
            {
                model = model.Where(s => s.ManualPayDate.HasValue);
            }
            if (baseSearchDto.DriverStatusId.HasValue)
            {
                model = model.Where(s => s.DriverStatusId == baseSearchDto.DriverStatusId);
            }
            if (baseSearchDto.TripStatusId.HasValue)
            {
                model = model.Where(s => s.TripStatusId == baseSearchDto.TripStatusId);
            }
            if (baseSearchDto.Point != null)
            {
                var targetLocation = new Point(baseSearchDto.Point.x, baseSearchDto.Point.y) { SRID = 4326 }; // مثلاً یک نقطه در تهران
                model = model.Where(s => s.Origin.Distance(targetLocation) < baseSearchDto.Point.DistanceMeter);
            }
            if (baseSearchDto.ToMinute != null)
            {
                model = model.Where(s => s.CreateDate.AddMinutes(baseSearchDto.ToMinute.Value) < DateTime.Now);
            }
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                model = model.Where(s => s.UserPet.User.FirstName.Contains(baseSearchDto.Q) || s.UserPet.User.LastName.Contains(baseSearchDto.Q) || s.UserPet.User.Mobile.Contains(baseSearchDto.Q));
            }
            switch (baseSearchDto.SortBy)
            {
                case SortEnum.New:
                    {
                        model = model.OrderByDescending(s => s.Id);
                        break;
                    }
                case SortEnum.Old:
                    {
                        model = model.OrderBy(s => s.Id);
                        break;
                    }
                default:
                    break;
            }
            return new TripSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<TripDto>> InsertAsyncDto(TripDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<TripDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    if (dto.Origin == null)
                    {
                        return new BaseResultDto<TripDto>(false, Resource.Notification.PleaseSetOrigin, dto);
                    }
                    if (dto.Destination == null)
                    {
                        return new BaseResultDto<TripDto>(false, Resource.Notification.PleaseSetDestination, dto);
                    }
                    var item = mapper.Map<Trip>(dto);

                    if (!item.TripStartDateTime.HasValue)
                    {
                        item.TripStartDateTime = item.CreateDate;
                    }
                    item.CreateDate = DateTime.Now;
                    item.ManualPayDate = null;
                    item.DriverStatusId = (long)DriverStatusEnum.DriverStatus_Requested;
                    item.TripStatusId = (long)TripStatusEnum.TripStatus_Requested;
                    await _context.Trips.AddAsync(item);
                    await _context.SaveChangesAsync();
                    var driver = await _context.Drivers.FindAsync(item.DriverId);
                    var userPet = await _context.UserPets.Include(s => s.Pet).FirstOrDefaultAsync(s => s.Id == item.UserPetId);
                    await _messageSender.SendMessageAsync(messageType: MessageTypeEnum.DriverRequest, mobileReceptor: driver.Phone, emailReceptor: null, token1: userPet.Pet.Name);
                    await _noticeService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.TripDriverRequested, ActorUserId = item.UserId, ReferenceType = "Trip", ReferenceId = item.Id, DeduplicationKey = $"{NoticeTypeLabels.TripDriverRequested}:{item.Id}" });
                    return new BaseResultDto<TripDto>(true, mapper.Map<TripDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<TripDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }

        //public async Task<BaseResultDto<TripVDto>> InsertVDtoAsync(TripVDto vDto)
        //{
        //    var dto = mapper.Map<TripDto>(vDto);
        //    dto.TripStartDateTime = null;
        //    var item = await InsertAsyncDto(dto);
        //    if (item.IsSuccess)
        //        return await FindAsyncVDto(item.Data.Id);
        //    else
        //        return new BaseResultDto<TripVDto>(false, null);

        //}
        //public async Task ChangeTripStatusAsync(long id, TripStatusEnum status)
        //{
        //    var item = await _context.Trips.FindAsync(id);
        //    if (item != null)
        //    {

        //    }
        //}
        //public async Task ChangeTripConnectionIdAsync(TripVDto trip)
        //{
        //    var item = await _context.Trips.FindAsync(trip.Id);
        //    if (item != null)
        //    {
        //        item.ConnectionId = trip.ConnectionId;
        //        _context.Trips.Update(item);
        //        await _context.SaveChangesAsync();
        //    }
        //}
        //public async Task ChangeTripTokenAsync(TripVDto trip)
        //{
        //    var item = await _context.Trips.FindAsync(trip.Id);
        //    if (item != null)
        //    {
        //        item.UserToken = trip.UserToken;
        //        _context.Trips.Update(item);
        //        await _context.SaveChangesAsync();
        //    }
        //}
        public async Task<BaseResultDto> InsertOrUpdateAsync(TripDto dto)
        {
            Trip trip;

            if (dto.Origin == null)
            {
                return new BaseResultDto<TripDto>(false, Resource.Notification.PleaseSetOrigin, dto);
            }

            if (dto.Destination == null)
            {
                return new BaseResultDto<TripDto>(false, Resource.Notification.PleaseSetDestination, dto);
            }


            bool isUpdate = dto.Id > 0;

            if (isUpdate)
            {
                trip = await _context.Trips
                    .Include(s => s.TripOptions)
                    .FirstOrDefaultAsync(s => s.Id == dto.Id);

                if (trip == null || trip.TripStatusId != (long)TripStatusEnum.TripStatus_Requested)
                {
                    return new BaseResultDto(false);
                }

                mapper.Map(dto, trip);
                trip.TripOptions.Clear();
            }
            else
            {
                if (dto.IsOnline)
                {
                    var hasActiveOnlineTrip = await _context.Trips.AnyAsync(s => s.UserId == dto.UserId && s.IsOnline && (s.TripStatusId == (long)TripStatusEnum.TripStatus_Requested || s.TripStatusId == (long)TripStatusEnum.TripStatus_Accepted));
                    if (hasActiveOnlineTrip)
                    {
                        return new BaseResultDto<TripDto>(false, Resource.Notification.YouAlreadyHaveOnlineTrip, dto);
                    }
                    dto.DriverId = null;
                }
                else
                {
                    if (dto.DriverId == null)
                    {
                        return new BaseResultDto<TripDto>(false, Resource.Notification.PleaseEnterTheDriver, dto);
                    }
                }
                trip = mapper.Map<Trip>(dto);
                trip.CreateDate = DateTime.Now;
            }

            if (!trip.TripStartDateTime.HasValue)
            {
                trip.IsOnline = true;
                trip.TripStartDateTime = DateTime.Now;
            }
            else
            {
                trip.IsOnline = false;
            }

            trip.Price = await _priceCalculationService.CalculateTripPrice(dto);
            trip.DriverStatusId = (long)DriverStatusEnum.DriverStatus_Requested;
            trip.TripStatusId = (long)TripStatusEnum.TripStatus_Requested;
            trip.ManualPayDate = null;
            trip.RebateId = null;
            trip.RebatePrice = 0;
            trip.PaymentPrice = trip.Price;

            if (dto.TripOptionIds != null && dto.TripOptionIds.Any())
            {
                trip.TripOptions = dto.TripOptionIds
                    .Select(id => new TripOption { Id = id })
                    .ToList();

                foreach (var option in trip.TripOptions)
                {
                    _context.Entry(option).State = EntityState.Unchanged;
                }
            }


            if (isUpdate)
                _context.Trips.Update(trip);
            else
                await _context.Trips.AddAsync(trip);

            await _context.SaveChangesAsync();
            trip = await _context.Trips.Include(t => t.Driver).ThenInclude(t => t.ProfilePicture).Include(t => t.UserPet).Include(t => t.DriverStatus)
                                       .Include(t => t.TripStatus).Include(t => t.TripStop).Include(t => t.TripOptions).FirstOrDefaultAsync(t => t.Id == trip.Id);

            return new BaseResultDto<TripDto>(true, mapper.Map<TripDto>(trip));
        }


        public async Task<BaseResultDto<TripVDto>> GetUserCurrentTrip(long userId)
        {
            var item = await _context.Trips.Include(s => s.TripStop).Include(s => s.TripOptions).Include(s => s.UserPet).ThenInclude(s => s.User)
                .Include(s => s.DriverStatus).Include(s => s.TripStatus).Include(s => s.Driver).ThenInclude(t => t.ProfilePicture)
                .FirstOrDefaultAsync(s => s.UserPet.UserId == userId && s.IsOnline && (s.TripStatusId == (long)TripStatusEnum.TripStatus_Requested || s.TripStatusId == (long)TripStatusEnum.TripStatus_Accepted));
            if (item != null)
            {
                return new BaseResultDto<TripVDto>(true, mapper.Map<TripVDto>(item));
            }
            return new BaseResultDto<TripVDto>(false, mapper.Map<TripVDto>(item));
        }

        public async Task<BaseResultDto<TripVDto>> GetDriverCurrentTrip(long driverId)
        {
            var item = await _context.Trips.Include(s => s.TripStop).Include(s => s.TripOptions).Include(s => s.UserPet).ThenInclude(s => s.User)
                .Include(s => s.DriverStatus).Include(s => s.TripStatus).Include(s => s.Driver).ThenInclude(t => t.ProfilePicture)
                .FirstOrDefaultAsync(s => s.DriverId == driverId && s.IsOnline && (s.TripStatusId == (long)TripStatusEnum.TripStatus_Requested || s.TripStatusId == (long)TripStatusEnum.TripStatus_Accepted) && s.DriverStatusId != (long)DriverRequestStatusEnum.DriverRequestStatus_Rejected);
            if (item != null)
            {
                return new BaseResultDto<TripVDto>(true, mapper.Map<TripVDto>(item));
            }
            return new BaseResultDto<TripVDto>(false, mapper.Map<TripVDto>(item));
        }

        public async Task<BaseResultDto> TripPaymentCallback(long? tripId, bool fromWallet = false)
        {
            try
            {
                var trip = await _context.Trips.Include(s => s.UserPet).AsTracking().FirstOrDefaultAsync(s => s.Id == tripId);

                if (fromWallet)
                {
                    var amount = await _walletService.GetAmountValueAsync(trip.UserPet.UserId);
                    if (amount >= trip.WalletPrice)
                    {
                        var walletItem = new WalletDto() { Painding = false, Amount = trip.WalletPrice, UserId = trip.UserPet.UserId, TripId = trip.Id };
                        await _walletService.InsertUpdateTripAsync(walletItem, true);
                    }
                    else
                    {
                        return new BaseResultDto(false);
                    }
                }
                trip.IsPaid = true;
                _context.Trips.Update(trip);
                await _context.SaveChangesAsync();
                _rebateService.IncreaseUseCount(trip);
                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception)
            {
                return new BaseResultDto(false);

            }
        }

        public async Task<BaseResultDto<ManualPayTripDto>> ManualTripPaymentAsync(ManualPayTripDto dto)
        {
            var trip = await _context.Trips.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id);

            trip.IsPaid = true;
            trip.ManualPayDate = DateTime.Now;

            _context.Trips.Update(trip);
            await _context.SaveChangesAsync();
            return new BaseResultDto<ManualPayTripDto>(true, Resource.Notification.Success, dto);

        }

        public async Task<BaseResultDto<TripDriverChangeStatusDto>> UpdateTripDriverStatusAsync(TripDriverChangeStatusDto dto)
        {
            var trip = await _context.Trips.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id);

            if (trip.DriverId.HasValue && trip.DriverId != dto.DriverId)
            {
                return new BaseResultDto<TripDriverChangeStatusDto>(false, Resource.Notification.ThisTripHasBeenReservedForAnotherDriver, dto);
            }
            trip.DriverStatusId = dto.DriverStatusId;

            var driver = await _context.Drivers.FirstOrDefaultAsync(s => s.Id == dto.DriverId);
            var userPet = await _context.UserPets
                .Include(s => s.Pet)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == trip.UserPetId);

            if (trip.DriverStatusId == (long)DriverStatusEnum.DriverStatus_Accepted)
            {
                trip.DriverId = dto.DriverId;
                var tripChangeStatusDto = new TripChangeStatusDto
                {
                    Id = trip.Id,
                    TripStatusId = (long)TripStatusEnum.TripStatus_Accepted
                };

                await TripChangeStatusAsync(tripChangeStatusDto);
            }
            else if (trip.DriverStatusId == (long)DriverStatusEnum.DriverStatus_Rejected)
            {
                await _noticeService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.TripDriverSelectionRequired, ReferenceType = "Trip", ReferenceId = trip.Id, DeduplicationKey = $"{NoticeTypeLabels.TripDriverSelectionRequired}:{trip.Id}" });
            }
            else
            {
                return new BaseResultDto<TripDriverChangeStatusDto>(false, Resource.Notification.PleaseChangeTheStatus, dto);
            }
            _context.Trips.Update(trip);
            await _context.SaveChangesAsync();

            await _messageSender.SendMessageAsync(
                messageType: MessageTypeEnum.DriverAccepted,
                mobileReceptor: userPet.User.Mobile,
                emailReceptor: null,
                token1: driver.Name,
                token2: userPet.Name,
                sendDate: DateTime.Now
                );
            return new BaseResultDto<TripDriverChangeStatusDto>(true, Resource.Notification.Success, dto);
        }


        public async Task<BaseResultDto<TripUserChangeStatusDto>> UpdateTripUserStatusAsync(TripUserChangeStatusDto dto)
        {
            var trip = await _context.Trips.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id);
            trip.TripStatusId = dto.TripStatusId;

            if (dto.TripStatusId == (long)TripStatusEnum.TripStatus_Requested || trip.TripStatusId == (long)TripStatusEnum.TripStatus_Compeleted)
            {
                return new BaseResultDto<TripUserChangeStatusDto>(false, Resource.Notification.PleaseChangeTheStatus, dto);
            }
            else if (dto.TripStatusId == (long)TripStatusEnum.TripStatus_Canceled)
            {
                await _noticeService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.TripCancelledByUser, ActorUserId = trip.UserId, ReferenceType = "Trip", ReferenceId = trip.Id, DeduplicationKey = $"{NoticeTypeLabels.TripCancelledByUser}:{trip.Id}" });
            }
            else
            {
                trip.TripStatusId = (long)TripStatusEnum.TripStatus_Accepted;
                var user = await _context.Users.FirstOrDefaultAsync(s => s.Id == trip.UserId);

                if (!trip.IsOnline)
                {
                    var allDrivers = await _context.Drivers.Where(s => s.Deleted == false && s.Active).ToListAsync();
                    foreach (var d in allDrivers)
                    {
                        await _messageSender.SendMessageAsync(messageType: MessageTypeEnum.RequestAllDrivers, mobileReceptor: d.Phone, emailReceptor: null, token1: trip.TripStartDateTime?.Date.ToString("yyyy-MM-dd"), token2: trip.TripStartDateTime.Value.ToString("HH:mm"));
                    }
                }
                else
                {

                    var adminMobile = _adminSettingHelper.BaseAdminSetting.AdminMobiles;

                    await _messageSender.SendMessageAsync(
                        messageType: MessageTypeEnum.AdminNotifyTrip,
                        mobileReceptor: adminMobile,
                        emailReceptor: null,
                        token1: user.FirstName,
                        token2: user.Mobile,
                        sendDate: DateTime.Now

                    );

                    await _noticeService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.TripDriverRequested, ActorUserId = trip.UserId, ReferenceType = "Trip", ReferenceId = trip.Id, DeduplicationKey = $"{NoticeTypeLabels.TripDriverRequested}:{trip.Id}" });
                }
            }

            _context.Trips.Update(trip);
            await _context.SaveChangesAsync();
            return new BaseResultDto<TripUserChangeStatusDto>(true, Resource.Notification.Success, dto);
        }

        public async Task<BaseResultDto<TripAdminChooseDriverDto>> ChooseDriverAsync(TripAdminChooseDriverDto dto)
        {
            var trip = await _context.Trips.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id);
            trip.DriverId = dto.DriverId;
            var driver = await _context.Drivers.FindAsync(trip.DriverId);
            var userPet = await _context.UserPets.Include(s => s.Pet).FirstOrDefaultAsync(s => s.Id == trip.UserPetId);
            await _messageSender.SendMessageAsync(messageType: MessageTypeEnum.UserChooseDriver, mobileReceptor: driver.Phone, emailReceptor: null, token1: userPet.Pet.Name);
            _context.Trips.Update(trip);
            await _context.SaveChangesAsync();
            return new BaseResultDto<TripAdminChooseDriverDto>(true, Resource.Notification.Success, dto);
        }

        public async Task<BaseResultDto<TripChangeStatusDto>> TripChangeStatusAsync(TripChangeStatusDto dto)
        {
            var trip = await _context.Trips.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id);
            trip.TripStatusId = dto.TripStatusId;
            _context.Trips.Update(trip);
            await _context.SaveChangesAsync();
            return new BaseResultDto<TripChangeStatusDto>(true, Resource.Notification.Success, dto);
        }

        public async Task SyncDriverAcceptAsync()
        {
            var halfHourAgo = DateTime.Now.AddMinutes(-30);

            var pendingTrips = await _context.Trips.Where(t => t.IsOnline == true && t.DriverId.HasValue && t.DriverStatusId == (long)DriverStatusEnum.DriverStatus_Requested && t.CreateDate <= halfHourAgo).ToListAsync();

            foreach (var trip in pendingTrips)
            {
                var driver = await _context.Drivers.FindAsync(trip.DriverId);
                var userPet = await _context.UserPets.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == trip.UserPetId);

                var adminMobile = _adminSettingHelper.BaseAdminSetting.AdminMobiles;

                await _messageSender.SendMessageAsync(messageType: MessageTypeEnum.DriverNotAcceptedYet, mobileReceptor: adminMobile, emailReceptor: null, token1: driver.Name, token2: userPet.User.Mobile, token3: trip.Id.ToString());
            }
        }

        public async Task<BaseResultDto> SetRebateCodeAsyncDto(TripSetRebateCodeDto dto)
        {
            var item = await _context.Trips.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id && s.TripStatusId == (long)TripStatusEnum.TripStatus_Accepted);

            if (item == null)
            {
                return new BaseResultDto<TripSetRebateCodeDto>(false, Resource.Notification.NothingFound, dto);
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
                _context.Trips.Update(item);
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
            var item = await _context.Trips.AsTracking().FirstOrDefaultAsync(s => s.Id == id);
            item.RebateId = null;
            item.RebatePrice = 0;
            item.PaymentPrice = item.Price;
            _context.Trips.Update(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }

        public async Task<BaseResultDto> SetWalletAsyncDto(TripSetWalletDto dto)
        {
            var item = await _context.Trips.Include(s => s.UserPet).AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id && s.TripStatusId == (long)TripStatusEnum.TripStatus_Accepted);
            if (item == null)
            {
                return new BaseResultDto<TripSetWalletDto>(false, Resource.Notification.NothingFound, dto);
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
            _context.Trips.Update(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }

        public async Task<BaseResultDto<TripShareDto>> UpdateTripShareAsync(TripShareDto dto)
        {
            var trip = await _context.Trips.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id);
            if (trip.TripStatusId == (long)TripStatusEnum.TripStatus_Compeleted)
            {
                //var sharePercent = _adminSettingHelper.SharePrice.TripDriverShare;
                var total = trip.PaymentPrice;

                //trip.DriverShare = (total * sharePercent) / 100;
                trip.SiteShare = total - trip.DriverShare;
            }
            else
            {
                return new BaseResultDto<TripShareDto>(false, Resource.Notification.TriphasNotCompletedYet, dto);
            }
            _context.Trips.Update(trip);
            await _context.SaveChangesAsync();
            return new BaseResultDto<TripShareDto>(true, Resource.Notification.Success, dto);
        }
    }
}
