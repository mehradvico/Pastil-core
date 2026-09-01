using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Enumerable.Message;
using Application.Common.Geography.Iface;
using Application.Common.Helpers;
using Application.Common.Helpers.Iface;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
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
using Entities.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Persistence.Interface;
using System;
using System.Collections.Generic;
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
        private readonly ICurrentUserHelper _currentUser;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IGeographyService _geographyService;
        private readonly ILogger<TripService> _logger;
        public TripService(IDataBaseContext _context, IMapper mapper, IWalletService walletService, IRebateService rebateService, IAdminSettingHelper adminSettingHelper, IPriceCalculationService priceCalculationService, ITripOptionService tripOptionService, ICodeService codeService, IMessageSenderService messageSender, INoticeService noticeService, ICurrentUserHelper currentUser, IPushNotificationService pushNotificationService, IGeographyService geographyService, ILogger<TripService> logger) : base(_context, mapper)
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
            _currentUser = currentUser;
            _pushNotificationService = pushNotificationService;
            _geographyService = geographyService;
            _logger = logger;
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
            var item = await _context.Trips.Include(s => s.FromCity).Include(s => s.TripStop).Include(s => s.TripOptions).Include(s => s.User).Include(s => s.UserPet).ThenInclude(s => s.Pet).Include(s => s.UserPet).ThenInclude(s => s.User).Include(s => s.DriverStatus).Include(s => s.TripStatus).Include(s => s.CancelReasonCode).Include(s => s.Driver).Include(s => s.TripPets).ThenInclude(tp => tp.UserPet).ThenInclude(up => up.Pet).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<TripVDto>(true, mapper.Map<TripVDto>(item));
            }
            return new BaseResultDto<TripVDto>(false, mapper.Map<TripVDto>(item));
        }

        public override async Task<BaseResultDto<TripDto>> FindAsyncDto(long id)
        {
            var item = await _context.Trips.Include(s => s.FromCity).Include(s => s.TripStop).Include(s => s.TripOptions).Include(s => s.User).Include(s => s.UserPet).ThenInclude(s => s.Pet).Include(s => s.UserPet).ThenInclude(s => s.User).Include(s => s.DriverStatus).Include(s => s.TripStatus).Include(s => s.CancelReasonCode).Include(s => s.Driver).Include(s => s.TripPets).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<TripDto>(true, mapper.Map<TripDto>(item));
            }
            return new BaseResultDto<TripDto>(false, mapper.Map<TripDto>(item));
        }

        public TripSearchDto Search(TripInputDto baseSearchDto)
        {
            var model = _context.Trips.Include(s => s.FromCity).Include(s => s.TripStop).Include(s => s.TripOptions).Include(s => s.User).Include(s => s.UserPet).ThenInclude(s => s.Pet).Include(s => s.UserPet).ThenInclude(s => s.User).Include(s => s.DriverStatus).Include(s => s.TripStatus).Include(s => s.Driver).Include(s => s.TripPets).AsQueryable();

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
                    ApplyTripPets(item, dto.UserPetIds, dto.UserPetId);
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
                    .Include(s => s.TripPets)
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

            ApplyTripPets(trip, dto.UserPetIds, dto.UserPetId);

            if (isUpdate)
                _context.Trips.Update(trip);
            else
                await _context.Trips.AddAsync(trip);

            await _context.SaveChangesAsync();
            trip = await _context.Trips.Include(t => t.Driver).ThenInclude(t => t.ProfilePicture).Include(t => t.UserPet).Include(t => t.DriverStatus)
                                       .Include(t => t.TripStatus).Include(t => t.TripStop).Include(t => t.TripOptions).FirstOrDefaultAsync(t => t.Id == trip.Id);

            if (!isUpdate && trip.IsOnline)
            {
                if (dto.PreviousTripId.HasValue)
                {
                    await CarryForwardDriverExclusionsAsync(dto.PreviousTripId.Value, trip.Id, trip.UserId);
                }

                await BroadcastTripAvailableAsync(trip.Id);

                await _noticeService.CreateAsync(new NoticeCreateDto
                {
                    Label = NoticeTypeLabels.TripDriverRequested,
                    ActorUserId = trip.UserId,
                    ReferenceType = "Trip",
                    ReferenceId = trip.Id,
                    DeduplicationKey = $"{NoticeTypeLabels.TripDriverRequested}:{trip.Id}"
                });
            }

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
                if (trip == null)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                if (trip.IsPaid)
                    return new BaseResultDto(true);

                if (fromWallet && trip.FromWallet && trip.WalletPrice > 0)
                {
                    var walletItem = new WalletDto() { Painding = false, Amount = trip.WalletPrice, UserId = trip.UserPet.UserId, TripId = trip.Id };
                    var walletResult = await _walletService.InsertUpdateTripAsync(walletItem, true);
                    if (!walletResult.IsSuccess)
                        return new BaseResultDto(false);
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
            var trip = await _context.Trips.AsNoTracking().FirstOrDefaultAsync(s => s.Id == dto.Id);

            if (trip == null)
            {
                return new BaseResultDto<TripDriverChangeStatusDto>(false, Resource.Notification.NothingFound, dto);
            }

            if (trip.DriverId.HasValue && trip.DriverId != dto.DriverId)
            {
                return new BaseResultDto<TripDriverChangeStatusDto>(false, Resource.Notification.ThisTripHasBeenReservedForAnotherDriver, dto);
            }

            var driver = await _context.Drivers.FirstOrDefaultAsync(s => s.Id == dto.DriverId);
            var userPet = await _context.UserPets
                .Include(s => s.Pet)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == trip.UserPetId);

            if (dto.DriverStatusId == (long)DriverStatusEnum.DriverStatus_Accepted)
            {
                // آپدیت اتمیک با شرط DriverId == null در همون UPDATE — جلوگیری از race condition
                // وقتی چند راننده هم‌زمان می‌زنن قبول؛ فقط اولی که واقعاً commit بشه برنده‌ست.
                // راننده‌ای که قبلاً این سفر (یا نسخه‌ی قبلی‌اش) را رد/لغو کرده، دیگر نمی‌تواند آن را بپذیرد.
                var claimedRows = await _context.Trips
                    .Where(t => t.Id == dto.Id
                        && t.DriverId == null
                        && t.TripStatusId == (long)TripStatusEnum.TripStatus_Requested
                        && !_context.TripDriverExclusions.Any(e => e.TripId == t.Id && e.DriverId == dto.DriverId))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(t => t.DriverId, dto.DriverId)
                        .SetProperty(t => t.DriverStatusId, dto.DriverStatusId)
                        .SetProperty(t => t.TripStatusId, (long)TripStatusEnum.TripStatus_Accepted)
                        .SetProperty(t => t.ProgressStageId, (int)TripProgressStageEnum.EnRouteOrigin)
                        .SetProperty(t => t.ProgressUpdateDate, DateTime.Now));

                if (claimedRows == 0)
                {
                    return new BaseResultDto<TripDriverChangeStatusDto>(false, Resource.Notification.ThisTripHasBeenReservedForAnotherDriver, dto);
                }

                await _messageSender.SendMessageAsync(
                    messageType: MessageTypeEnum.DriverAccepted,
                    mobileReceptor: userPet.User.Mobile,
                    emailReceptor: null,
                    token1: driver.Name,
                    token2: userPet.Name,
                    sendDate: DateTime.Now
                    );
            }
            else if (dto.DriverStatusId == (long)DriverStatusEnum.DriverStatus_Rejected)
            {
                // این سفر همچنان بدون‌راننده باقی می‌ماند و برای بقیه‌ی راننده‌ها نمایش داده می‌شود — فقط
                // برای همین راننده (به‌صورت جداگانه، نه یک فیلد مشترک روی خود سفر) دیگر نمایش داده نمی‌شود.
                var stillOpen = await _context.Trips.AnyAsync(t => t.Id == dto.Id
                    && t.DriverId == null
                    && t.TripStatusId == (long)TripStatusEnum.TripStatus_Requested);

                if (!stillOpen)
                {
                    return new BaseResultDto<TripDriverChangeStatusDto>(false, Resource.Notification.ThisTripHasBeenReservedForAnotherDriver, dto);
                }

                var alreadyExcluded = await _context.TripDriverExclusions
                    .AnyAsync(e => e.TripId == dto.Id && e.DriverId == dto.DriverId);

                if (!alreadyExcluded)
                {
                    await _context.TripDriverExclusions.AddAsync(new TripDriverExclusion
                    {
                        TripId = dto.Id,
                        DriverId = dto.DriverId,
                        ReasonId = (int)TripDriverExclusionReasonEnum.Rejected,
                        CreateDate = DateTime.Now
                    });
                    await _context.SaveChangesAsync();

                    await _noticeService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.TripDriverSelectionRequired, ReferenceType = "Trip", ReferenceId = trip.Id, DeduplicationKey = $"{NoticeTypeLabels.TripDriverSelectionRequired}:{trip.Id}:{dto.DriverId}" });
                }
            }
            else
            {
                return new BaseResultDto<TripDriverChangeStatusDto>(false, Resource.Notification.PleaseChangeTheStatus, dto);
            }

            return new BaseResultDto<TripDriverChangeStatusDto>(true, Resource.Notification.Success, dto);
        }


        public async Task<BaseResultDto<TripUserChangeStatusDto>> UpdateTripUserStatusAsync(TripUserChangeStatusDto dto)
        {
            var trip = await _context.Trips.AsTracking()
                .FirstOrDefaultAsync(s => s.Id == dto.Id && s.UserId == _currentUser.CurrentUser.UserId);

            if (trip == null)
                return new BaseResultDto<TripUserChangeStatusDto>(false, Resource.Notification.NothingFound, dto);

            trip.TripStatusId = dto.TripStatusId;

            if (dto.TripStatusId == (long)TripStatusEnum.TripStatus_Requested || trip.TripStatusId == (long)TripStatusEnum.TripStatus_Compeleted)
            {
                return new BaseResultDto<TripUserChangeStatusDto>(false, Resource.Notification.PleaseChangeTheStatus, dto);
            }
            else if (dto.TripStatusId == (long)TripStatusEnum.TripStatus_Canceled)
            {
                // لغو سفر دیگر از این endpoint عمومی ممکن نیست — باید دلیل لغو ثبت شود (از UpdateTripUserStatusController.Put
                // به CancelByUserAsync/EndUser/TripUserCancel منتقل شده).
                return new BaseResultDto<TripUserChangeStatusDto>(false, Resource.Notification.PleaseChangeTheStatus, dto);
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

        /// <summary>
        /// انتخاب دستی راننده توسط ادمین — فقط برای سفرهایی که هنوز هیچ راننده‌ای قبول نکرده (معمولاً
        /// سفرهای رزرویی که تا نزدیکی موعد، کسی از راننده‌ها Broadcast رو قبول نکرده). دقیقاً همون اثر
        /// «راننده قبول کرد» رو داره — سفر مستقیم می‌ره روی Accepted، نه یک وضعیت میانی جدید.
        /// </summary>
        public async Task<BaseResultDto<TripAdminChooseDriverDto>> ChooseDriverAsync(TripAdminChooseDriverDto dto)
        {
            var trip = await _context.Trips.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id);
            if (trip == null)
                return new BaseResultDto<TripAdminChooseDriverDto>(false, Resource.Notification.NothingFound, dto);

            if (trip.DriverId.HasValue || trip.TripStatusId != (long)TripStatusEnum.TripStatus_Requested)
                return new BaseResultDto<TripAdminChooseDriverDto>(false, Resource.Notification.ThisTripHasBeenReservedForAnotherDriver, dto);

            var driver = await _context.Drivers.FirstOrDefaultAsync(s => s.Id == dto.DriverId && !s.Deleted);
            if (driver == null || !driver.Active || driver.StatusId != (long)DriverRequestStatusEnum.DriverRequestStatus_Accepted)
                return new BaseResultDto<TripAdminChooseDriverDto>(false, Resource.Notification.DriverRequesterUserInvalid, dto);

            var userPet = await _context.UserPets.Include(s => s.Pet).FirstOrDefaultAsync(s => s.Id == trip.UserPetId);

            trip.DriverId = dto.DriverId;
            trip.DriverStatusId = (long)DriverStatusEnum.DriverStatus_Accepted;
            trip.TripStatusId = (long)TripStatusEnum.TripStatus_Accepted;
            trip.ProgressStageId = (int)TripProgressStageEnum.EnRouteOrigin;
            trip.ProgressUpdateDate = DateTime.Now;

            _context.Trips.Update(trip);
            await _context.SaveChangesAsync();

            try
            {
                await _messageSender.SendMessageAsync(messageType: MessageTypeEnum.UserChooseDriver, mobileReceptor: driver.Phone, emailReceptor: null, token1: userPet?.Pet?.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notifying driver {DriverId} for admin-assigned trip {TripId} failed.", driver.Id, trip.Id);
            }

            try
            {
                await _pushNotificationService.SendPushAsync(PushTypeEnum.PushTripRequestAvailable, driver.OwnerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sending admin-assignment push for trip {TripId} failed.", trip.Id);
            }

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
            var item = await _context.Trips.AsTracking().FirstOrDefaultAsync(s =>
                s.Id == dto.Id &&
                s.UserId == _currentUser.CurrentUser.UserId &&
                !s.IsPaid &&
                s.TripStatusId == (long)TripStatusEnum.TripStatus_Accepted);

            if (item == null)
            {
                return new BaseResultDto<TripSetRebateCodeDto>(false, Resource.Notification.NothingFound, dto);
            }
            if (await HasActivePaymentAsync(item.Id))
                return new BaseResultDto(false, Resource.Notification.TripFinancialDataLockedAfterPaymentStarted);
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
            var item = await _context.Trips.AsTracking().FirstOrDefaultAsync(s =>
                s.Id == id && s.UserId == _currentUser.CurrentUser.UserId && !s.IsPaid);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (await HasActivePaymentAsync(item.Id))
                return new BaseResultDto(false, Resource.Notification.TripFinancialDataLockedAfterPaymentStarted);
            item.RebateId = null;
            item.RebatePrice = 0;
            item.PaymentPrice = item.Price;
            _context.Trips.Update(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }

        public async Task<BaseResultDto> SetWalletAsyncDto(TripSetWalletDto dto)
        {
            var item = await _context.Trips.Include(s => s.UserPet).AsTracking().FirstOrDefaultAsync(s =>
                s.Id == dto.Id &&
                s.UserId == _currentUser.CurrentUser.UserId &&
                s.TripStatusId == (long)TripStatusEnum.TripStatus_Accepted);
            if (item == null)
            {
                return new BaseResultDto<TripSetWalletDto>(false, Resource.Notification.NothingFound, dto);
            }
            if (await HasActivePaymentAsync(item.Id))
                return new BaseResultDto(false, Resource.Notification.TripFinancialDataLockedAfterPaymentStarted);
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

        private Task<bool> HasActivePaymentAsync(long id)
        {
            var callbackId = id.ToString();
            return _context.Payments.AsNoTracking().AnyAsync(s =>
                s.CallBackTypeLabel == PaymentCallbackTypeEnum.Trip.ToString() &&
                s.CallBackId == callbackId &&
                (s.IsSuccess == null || s.IsSuccess == true));
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

        // ===================== پت‌رسان =====================

        private static readonly TripProgressStageEnum[] ProgressOrder =
        {
            TripProgressStageEnum.EnRouteOrigin,
            TripProgressStageEnum.ArrivedOrigin,
            TripProgressStageEnum.PetPickedUp,
            TripProgressStageEnum.ArrivedDestination
        };

        private static void ApplyTripPets(Trip trip, List<long> userPetIds, long? legacyUserPetId)
        {
            var ids = (userPetIds != null && userPetIds.Any())
                ? userPetIds.Distinct().ToList()
                : (legacyUserPetId.HasValue ? new List<long> { legacyUserPetId.Value } : new List<long>());

            trip.TripPets ??= new List<TripPet>();
            trip.TripPets.Clear();

            foreach (var petId in ids)
            {
                trip.TripPets.Add(new TripPet { UserPetId = petId });
            }

            // فیلد قدیمی UserPetId برای سازگاری با کدهای قبلی (گزارش‌گیری سریع) به‌عنوان اولین پت نگه داشته می‌شود.
            trip.UserPetId = ids.FirstOrDefault();
        }

        private async Task SendTripProgressPushAsync(TripProgressStageEnum stage, long userId, string petName)
        {
            try
            {
                var pushType = stage switch
                {
                    TripProgressStageEnum.ArrivedOrigin => PushTypeEnum.PushTripArrivedOrigin,
                    TripProgressStageEnum.PetPickedUp => PushTypeEnum.PushTripPetPickedUp,
                    TripProgressStageEnum.ArrivedDestination => PushTypeEnum.PushTripArrivedDestination,
                    _ => (PushTypeEnum?)null
                };

                if (pushType == null)
                    return;

                await _pushNotificationService.SendPushAsync(pushType.Value, userId, token1: petName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sending trip progress push for stage {Stage} to user {UserId} failed.", stage, userId);
            }
        }

        /// <summary>
        /// وقتی کاربر بعد از لغوشدن یک سفر توسط راننده، از صفحه‌ی درخواست دوباره ثبت می‌کند (با همان یا اطلاعات
        /// ویرایش‌شده)، راننده‌ای که سفر قبلی را رد یا لغو کرده بود باید در سفر جدید هم مستثنا بماند.
        /// </summary>
        private async Task CarryForwardDriverExclusionsAsync(long previousTripId, long newTripId, long userId)
        {
            var previousTrip = await _context.Trips.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == previousTripId
                    && p.UserId == userId
                    && p.TripStatusId == (long)TripStatusEnum.TripStatus_Canceled);

            if (previousTrip == null)
                return;

            var carriedDriverIds = await _context.TripDriverExclusions
                .Where(e => e.TripId == previousTripId)
                .Select(e => e.DriverId)
                .ToListAsync();

            if (previousTrip.CancelInitiatorId == (int)TripCancelInitiatorEnum.Driver && previousTrip.DriverId.HasValue)
            {
                carriedDriverIds.Add(previousTrip.DriverId.Value);
            }

            carriedDriverIds = carriedDriverIds.Distinct().ToList();
            if (!carriedDriverIds.Any())
                return;

            foreach (var driverId in carriedDriverIds)
            {
                _context.TripDriverExclusions.Add(new TripDriverExclusion
                {
                    TripId = newTripId,
                    DriverId = driverId,
                    ReasonId = (int)TripDriverExclusionReasonEnum.CarriedFromPreviousTrip,
                    CreateDate = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// اعلام یک سفرِ بدون‌راننده به همه‌ی راننده‌های فعال و تاییدشده — پوش broadcast.
        /// </summary>
        private async Task BroadcastTripAvailableAsync(long tripId)
        {
            try
            {
                var excludedDriverIds = await _context.TripDriverExclusions
                    .Where(e => e.TripId == tripId)
                    .Select(e => e.DriverId)
                    .ToListAsync();

                var activeDriverOwnerIds = await _context.Drivers
                    .Where(d => d.Deleted == false && d.Active && d.Approved && !excludedDriverIds.Contains(d.Id))
                    .Select(d => d.OwnerId)
                    .ToListAsync();

                foreach (var ownerId in activeDriverOwnerIds)
                {
                    await _pushNotificationService.SendPushAsync(PushTypeEnum.PushTripRequestAvailable, ownerId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Broadcasting trip {TripId} availability to drivers failed.", tripId);
            }
        }

        /// <summary>
        /// همه‌ی سفرهای لحظه‌ایِ بدون‌راننده — برای مرور و پذیرفتن توسط هر راننده‌ای (اولین پذیرنده برنده است).
        /// سفرهایی که همین راننده قبلاً رد کرده یا (در نسخه‌ی قبلی‌شان) لغو کرده، نمایش داده نمی‌شوند.
        /// </summary>
        public async Task<BaseResultDto<List<TripVDto>>> GetAvailableTripsForDriverAsync(long driverId)
        {
            var trips = await _context.Trips
                .Where(t => t.TripStatusId == (long)TripStatusEnum.TripStatus_Requested
                    && t.DriverId == null
                    && t.IsOnline
                    && !_context.TripDriverExclusions.Any(e => e.TripId == t.Id && e.DriverId == driverId))
                .Include(s => s.TripStop)
                .Include(s => s.TripOptions)
                .Include(s => s.UserPet).ThenInclude(s => s.User)
                .Include(s => s.TripPets).ThenInclude(s => s.UserPet)
                .Include(s => s.DriverStatus)
                .Include(s => s.TripStatus)
                .OrderBy(t => t.CreateDate)
                .ToListAsync();

            return new BaseResultDto<List<TripVDto>>(true, mapper.Map<List<TripVDto>>(trips));
        }

        /// <summary>
        /// لغو یک سفرِ پذیرفته‌شده توسط همون راننده — سفر «مختومه» می‌شود (دیگر بازنشانی/Broadcast مجدد نمی‌شود)
        /// و دلیل لغو ثبت می‌شود. کاربر باید از صفحه‌ی درخواست سفر، درخواست جدیدی ثبت کند؛ آن درخواست جدید
        /// دیگر برای همین راننده نمایش داده نخواهد شد. بعد از تحویل‌گرفتن پت دیگر قابل لغو نیست.
        /// </summary>
        public async Task<BaseResultDto<TripVDto>> CancelByDriverAsync(TripDriverCancelDto dto, long driverId)
        {
            var trip = await _context.Trips.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id && s.DriverId == driverId);

            if (trip == null)
                return new BaseResultDto<TripVDto>(false, Resource.Notification.NothingFound, null);

            if (trip.TripStatusId != (long)TripStatusEnum.TripStatus_Accepted)
                return new BaseResultDto<TripVDto>(false, Resource.Notification.PleaseChangeTheStatus, null);

            if (trip.ProgressStageId >= (int)TripProgressStageEnum.PetPickedUp)
                return new BaseResultDto<TripVDto>(false, Resource.Notification.TripCannotBeCanceledByDriverAfterPetPickup, null);

            var reasonValid = await _context.Codes.Include(c => c.CodeGroup)
                .AnyAsync(c => c.Id == dto.CancelReasonCodeId && c.Active && c.CodeGroup.Label == TripCancelReasonCodeGroups.Driver);

            if (!reasonValid)
                return new BaseResultDto<TripVDto>(false, Resource.Notification.PleaseChangeTheStatus, null);

            trip.TripStatusId = (long)TripStatusEnum.TripStatus_Canceled;
            trip.CancelInitiatorId = (int)TripCancelInitiatorEnum.Driver;
            trip.CancelReasonCodeId = dto.CancelReasonCodeId;
            trip.CancelReasonDetail = dto.CancelDetail;
            trip.ProgressUpdateDate = DateTime.Now;

            _context.Trips.Update(trip);

            var alreadyExcluded = await _context.TripDriverExclusions
                .AnyAsync(e => e.TripId == trip.Id && e.DriverId == driverId);

            if (!alreadyExcluded)
            {
                await _context.TripDriverExclusions.AddAsync(new TripDriverExclusion
                {
                    TripId = trip.Id,
                    DriverId = driverId,
                    ReasonId = (int)TripDriverExclusionReasonEnum.DriverCanceled,
                    CreateDate = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            try
            {
                await _pushNotificationService.SendPushAsync(PushTypeEnum.PushTripDriverCanceled, trip.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sending driver-cancel push for trip {TripId} failed.", trip.Id);
            }

            await _noticeService.CreateAsync(new NoticeCreateDto
            {
                Label = NoticeTypeLabels.TripDriverCanceled,
                ActorUserId = trip.UserId,
                ReferenceType = "Trip",
                ReferenceId = trip.Id,
                DeduplicationKey = $"{NoticeTypeLabels.TripDriverCanceled}:{trip.Id}"
            });

            return await FindAsyncVDto(trip.Id);
        }

        /// <summary>
        /// لغو یک سفر توسط کاربر — سفر «مختومه» می‌شود و دلیل لغو ثبت می‌شود. اگر راننده‌ای پذیرفته بود،
        /// به او اطلاع داده می‌شود. بعد از تحویل‌گرفتن پت دیگر قابل لغو نیست.
        /// </summary>
        public async Task<BaseResultDto<TripVDto>> CancelByUserAsync(TripUserCancelDto dto, long userId)
        {
            var trip = await _context.Trips.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id && s.UserId == userId);

            if (trip == null)
                return new BaseResultDto<TripVDto>(false, Resource.Notification.NothingFound, null);

            if (trip.TripStatusId == (long)TripStatusEnum.TripStatus_Compeleted || trip.TripStatusId == (long)TripStatusEnum.TripStatus_Canceled)
                return new BaseResultDto<TripVDto>(false, Resource.Notification.PleaseChangeTheStatus, null);

            if (trip.ProgressStageId >= (int)TripProgressStageEnum.PetPickedUp)
                return new BaseResultDto<TripVDto>(false, Resource.Notification.TripCannotBeCanceledAfterPetPickup, null);

            var reasonValid = await _context.Codes.Include(c => c.CodeGroup)
                .AnyAsync(c => c.Id == dto.CancelReasonCodeId && c.Active && c.CodeGroup.Label == TripCancelReasonCodeGroups.User);

            if (!reasonValid)
                return new BaseResultDto<TripVDto>(false, Resource.Notification.PleaseChangeTheStatus, null);

            var previousDriverId = trip.DriverId;

            trip.TripStatusId = (long)TripStatusEnum.TripStatus_Canceled;
            trip.CancelInitiatorId = (int)TripCancelInitiatorEnum.User;
            trip.CancelReasonCodeId = dto.CancelReasonCodeId;
            trip.CancelReasonDetail = dto.CancelDetail;
            trip.ProgressUpdateDate = DateTime.Now;

            _context.Trips.Update(trip);
            await _context.SaveChangesAsync();

            if (previousDriverId.HasValue)
            {
                try
                {
                    var driver = await _context.Drivers.FindAsync(previousDriverId.Value);
                    if (driver != null)
                        await _pushNotificationService.SendPushAsync(PushTypeEnum.PushTripUserCanceled, driver.OwnerId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Sending user-cancel push for trip {TripId} failed.", trip.Id);
                }
            }

            await _noticeService.CreateAsync(new NoticeCreateDto
            {
                Label = NoticeTypeLabels.TripCancelledByUser,
                ActorUserId = trip.UserId,
                ReferenceType = "Trip",
                ReferenceId = trip.Id,
                DeduplicationKey = $"{NoticeTypeLabels.TripCancelledByUser}:{trip.Id}"
            });

            return await FindAsyncVDto(trip.Id);
        }

        /// <summary>
        /// راننده یکی از مراحل ریز سفر را اعلام می‌کند: به مبدا رسیدم / پت را تحویل گرفتم / به مقصد رسیدم.
        /// فقط انتقال دقیقاً بعدیِ توالی مجاز پذیرفته می‌شود. برای سفر رفت‌وبرگشت، رسیدن به مقصدِ رفت
        /// به‌جای اتمام سفر، مسیر برگشت را (با مبدا/مقصد جابه‌جا‌شده) دوباره از EnRouteOrigin شروع می‌کند.
        /// </summary>
        public async Task<BaseResultDto<TripVDto>> AdvanceTripProgressAsync(long tripId, long driverId, TripProgressStageEnum targetStage)
        {
            var trip = await _context.Trips
                .Include(s => s.UserPet).ThenInclude(s => s.User)
                .Include(s => s.UserPet).ThenInclude(s => s.Pet)
                .AsTracking()
                .FirstOrDefaultAsync(s => s.Id == tripId);

            if (trip == null)
                return new BaseResultDto<TripVDto>(false, Resource.Notification.NothingFound, null);

            if (trip.DriverId != driverId)
                return new BaseResultDto<TripVDto>(false, Resource.Notification.AccessDenied, null);

            if (trip.TripStatusId != (long)TripStatusEnum.TripStatus_Accepted)
                return new BaseResultDto<TripVDto>(false, Resource.Notification.TripNotInStatusToChangeStage, null);

            var currentIndex = Array.IndexOf(ProgressOrder, (TripProgressStageEnum)trip.ProgressStageId);
            var targetIndex = Array.IndexOf(ProgressOrder, targetStage);

            if (targetIndex < 0 || targetIndex != currentIndex + 1)
                return new BaseResultDto<TripVDto>(false, Resource.Notification.TripRequestedStageNotInAllowedSequence, null);

            trip.ProgressStageId = (int)targetStage;
            trip.ProgressUpdateDate = DateTime.Now;

            var isFinalArrival = true;

            if (targetStage == TripProgressStageEnum.ArrivedDestination && trip.RoundTrip && !trip.IsReturnLeg)
            {
                // سفر رفت‌وبرگشت: پایان مسیر رفت — مسیر برگشت با مبدا/مقصد جابه‌جا‌شده دوباره شروع می‌شود.
                (trip.Origin, trip.Destination) = (trip.Destination, trip.Origin);
                trip.IsReturnLeg = true;
                trip.ProgressStageId = (int)TripProgressStageEnum.EnRouteOrigin;
                isFinalArrival = false;
            }

            await _context.SaveChangesAsync();

            var petName = trip.UserPet?.Pet?.Name;
            if (targetStage != TripProgressStageEnum.ArrivedDestination || isFinalArrival)
            {
                await SendTripProgressPushAsync(targetStage, trip.UserId, petName);
            }

            if (targetStage == TripProgressStageEnum.PetPickedUp)
            {
                try
                {
                    await _noticeService.CreateAsync(new NoticeCreateDto
                    {
                        Label = NoticeTypeLabels.TripPetPickedUp,
                        ActorUserId = trip.UserId,
                        ReferenceType = "Trip",
                        ReferenceId = trip.Id,
                        DeduplicationKey = $"{NoticeTypeLabels.TripPetPickedUp}:{trip.Id}"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Creating admin notice for trip pet pickup {TripId} failed.", trip.Id);
                }
            }

            return await FindAsyncVDto(trip.Id);
        }

        /// <summary>
        /// موقعیت لحظه‌ای راننده + وضعیت فعلی سفر، برای کاربر — فقط وقتی سفر «پذیرفته‌شده» است پاسخ می‌دهد
        /// (خارج از یک سفر فعال، کلاینت اصلاً نباید Poll کند).
        /// </summary>
        public async Task<BaseResultDto<TripLiveDto>> GetLiveForUserAsync(long tripId, long userId)
        {
            var trip = await _context.Trips.Include(s => s.Driver).AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == tripId && s.UserId == userId);

            if (trip == null)
                return new BaseResultDto<TripLiveDto>(false, Resource.Notification.NothingFound, null);

            if (trip.TripStatusId != (long)TripStatusEnum.TripStatus_Accepted || trip.Driver == null)
                return new BaseResultDto<TripLiveDto>(false, Resource.Notification.TripIsNotCurrentlyActive, null);

            var location = await _context.UserCurrentLocations.AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == trip.Driver.OwnerId);

            return new BaseResultDto<TripLiveDto>(true, BuildLiveDto(trip, location));
        }

        /// <summary>موقعیت لحظه‌ای کاربر + وضعیت فعلی سفر، برای راننده.</summary>
        public async Task<BaseResultDto<TripLiveDto>> GetLiveForDriverAsync(long tripId, long driverId)
        {
            var trip = await _context.Trips.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == tripId && s.DriverId == driverId);

            if (trip == null)
                return new BaseResultDto<TripLiveDto>(false, Resource.Notification.NothingFound, null);

            if (trip.TripStatusId != (long)TripStatusEnum.TripStatus_Accepted)
                return new BaseResultDto<TripLiveDto>(false, Resource.Notification.TripIsNotCurrentlyActive, null);

            var location = await _context.UserCurrentLocations.AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == trip.UserId);

            return new BaseResultDto<TripLiveDto>(true, BuildLiveDto(trip, location));
        }

        private static TripLiveDto BuildLiveDto(Trip trip, UserCurrentLocation location)
        {
            return new TripLiveDto
            {
                TripStatusId = trip.TripStatusId,
                ProgressStageId = trip.ProgressStageId,
                IsReturnLeg = trip.IsReturnLeg,
                CounterpartLocation = location?.Location != null
                    ? new Application.Common.Dto.LocationPoint.PointDto(location.Location.X, location.Location.Y)
                    : null,
                CounterpartLocationUpdatedAt = location?.LastUpdateDate
            };
        }

        /// <summary>
        /// همه‌ی سفرهای فعال (پذیرفته‌شده) به همراه آخرین موقعیت راننده و کاربر — برای نقشه‌ی زنده‌ی ادمین.
        /// </summary>
        public async Task<BaseResultDto<List<TripAdminLiveDto>>> GetActiveTripsForAdminAsync()
        {
            var trips = await _context.Trips.AsNoTracking()
                .Include(s => s.Driver)
                .Include(s => s.User)
                .Where(s => s.TripStatusId == (long)TripStatusEnum.TripStatus_Accepted)
                .OrderByDescending(s => s.ProgressUpdateDate ?? s.CreateDate)
                .ToListAsync();

            var driverOwnerIds = trips.Where(s => s.Driver != null).Select(s => s.Driver.OwnerId).Distinct().ToList();
            var userIds = trips.Select(s => s.UserId).Distinct().ToList();
            var locationUserIds = driverOwnerIds.Concat(userIds).Distinct().ToList();

            var locations = await _context.UserCurrentLocations.AsNoTracking()
                .Where(s => locationUserIds.Contains(s.UserId))
                .ToDictionaryAsync(s => s.UserId);

            var result = trips.Select(trip =>
            {
                locations.TryGetValue(trip.Driver?.OwnerId ?? 0, out var driverLocation);
                locations.TryGetValue(trip.UserId, out var userLocation);

                return new TripAdminLiveDto
                {
                    Id = trip.Id,
                    TripStatusId = trip.TripStatusId,
                    ProgressStageId = trip.ProgressStageId,
                    IsReturnLeg = trip.IsReturnLeg,
                    FromAddress = trip.FromAddress,
                    ToAddress = trip.ToAddress,
                    Origin = trip.Origin != null ? new Application.Common.Dto.LocationPoint.PointDto(trip.Origin.X, trip.Origin.Y) : null,
                    Destination = trip.Destination != null ? new Application.Common.Dto.LocationPoint.PointDto(trip.Destination.X, trip.Destination.Y) : null,
                    DriverId = trip.DriverId,
                    DriverName = trip.Driver?.Name,
                    DriverPhone = trip.Driver?.Phone,
                    DriverLocation = driverLocation?.Location != null
                        ? new Application.Common.Dto.LocationPoint.PointDto(driverLocation.Location.X, driverLocation.Location.Y)
                        : null,
                    DriverLocationUpdatedAt = driverLocation?.LastUpdateDate,
                    UserId = trip.UserId,
                    UserName = trip.User != null ? $"{trip.User.FirstName} {trip.User.LastName}".Trim() : null,
                    UserPhone = trip.User?.Mobile,
                    UserLocation = userLocation?.Location != null
                        ? new Application.Common.Dto.LocationPoint.PointDto(userLocation.Location.X, userLocation.Location.Y)
                        : null,
                    UserLocationUpdatedAt = userLocation?.LastUpdateDate
                };
            }).ToList();

            foreach (var trip in result)
            {
                if (trip.Origin == null || trip.Destination == null)
                    continue;

                try
                {
                    trip.RouteCoordinates = await _geographyService.GetDrivingRouteAsync(trip.Origin, trip.Destination);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fetching driving route for trip {TripId} failed.", trip.Id);
                }
            }

            return new BaseResultDto<List<TripAdminLiveDto>>(true, result);
        }

        /// <summary>
        /// ساخت سفر پت‌رسانِ متصل به یک رزرو (حالت دو). راننده در همین لحظه انتخاب نمی‌شود —
        /// DispatchScheduledTripsAsync در لحظه‌ی ScheduledDepartureAt نزدیک‌ترین راننده‌ی فعال را اختصاص می‌دهد.
        /// </summary>
        public async Task<BaseResultDto<TripDto>> CreateReservationLinkedTripAsync(TripReservationCreateDto dto, long userId)
        {
            if (dto.ScheduledLeadMinutes != 60 && dto.ScheduledLeadMinutes != 120)
                return new BaseResultDto<TripDto>(false, Resource.Notification.TripDriverMovementIntervalMustBe60Or120, null);

            if (dto.Origin == null)
                return new BaseResultDto<TripDto>(false, Resource.Notification.PleaseSetOrigin, null);

            if (dto.Destination == null)
                return new BaseResultDto<TripDto>(false, Resource.Notification.PleaseSetDestination, null);

            var reserve = await _context.CompanionReserves.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == dto.CompanionReserveId && s.BookerId == userId);

            if (reserve == null)
                return new BaseResultDto<TripDto>(false, Resource.Notification.NothingFound, null);

            var scheduledDepartureAt = reserve.DoDate.AddMinutes(-dto.ScheduledLeadMinutes);
            if (scheduledDepartureAt <= DateTime.Now)
                return new BaseResultDto<TripDto>(false, Resource.Notification.TripInsufficientTimeToAppointmentForInterval, null);

            var alreadyLinked = await _context.Trips.AnyAsync(s =>
                s.CompanionReserveId == dto.CompanionReserveId &&
                s.TripStatusId != (long)TripStatusEnum.TripStatus_Canceled);

            if (alreadyLinked)
                return new BaseResultDto<TripDto>(false, Resource.Notification.TripPetDeliveryAlreadyExistsForReserve, null);

            var priceInput = new TripDto
            {
                Origin = dto.Origin,
                Destination = dto.Destination,
                FromAddress = dto.FromAddress,
                ToAddress = dto.ToAddress,
                TripStartDateTime = scheduledDepartureAt
            };

            var trip = new Trip
            {
                Origin = new Point(dto.Origin.x, dto.Origin.y) { SRID = 4326 },
                Destination = new Point(dto.Destination.x, dto.Destination.y) { SRID = 4326 },
                FromAddress = dto.FromAddress,
                ToAddress = dto.ToAddress,
                UserId = userId,
                IsOnline = true,
                CreateDate = DateTime.Now,
                TripStartDateTime = scheduledDepartureAt,
                DriverStatusId = (long)DriverStatusEnum.DriverStatus_Requested,
                TripStatusId = (long)TripStatusEnum.TripStatus_Requested,
                CompanionReserveId = dto.CompanionReserveId,
                ScheduledLeadMinutes = dto.ScheduledLeadMinutes,
                ScheduledDepartureAt = scheduledDepartureAt,
                OwnerRidesAlong = dto.OwnerRidesAlong,
                ScheduledDispatched = false
            };

            try
            {
                trip.Price = await _priceCalculationService.CalculateTripPrice(priceInput);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Calculating price for reservation-linked trip (reserve {ReserveId}) failed.", dto.CompanionReserveId);
                return new BaseResultDto<TripDto>(false, Resource.Notification.Unsuccess, null);
            }

            if (trip.Price <= 0)
            {
                // دلیل شایع‌ترین صفرشدن قیمت: هیچ ردیف PriceCalculation ساعتِ حرکتِ این سفر (نه ساعتِ ثبت
                // درخواست) رو پوشش نمی‌ده — چون سفر رزرویی برخلاف حالت لحظه‌ای می‌تونه به هر ساعتی از شبانه‌روز
                // بیفته. پیام مشخص بده تا به‌جای شکست خاموش، قابل تشخیص و رفع باشه.
                _logger.LogError("Calculated price for reservation-linked trip (reserve {ReserveId}) was {Price} at scheduled hour {Hour} — refusing to create a free trip. Check PriceCalculation coverage for this hour.", dto.CompanionReserveId, trip.Price, scheduledDepartureAt.Hour);
                return new BaseResultDto<TripDto>(false, Resource.Notification.FinalPriceIsNotAvailable, null);
            }

            trip.PaymentPrice = trip.Price;

            ApplyTripPets(trip, dto.UserPetIds, null);

            await _context.Trips.AddAsync(trip);
            await _context.SaveChangesAsync();

            // برخلاف حالتِ قبلی (اختصاصِ خودکارِ نزدیک‌ترین راننده در لحظه‌ی حرکت)، همون لحظه‌ی ثبت درخواست
            // به همه‌ی راننده‌های فعال Broadcast می‌شه — دقیقاً مثل سفر لحظه‌ای — تا راننده‌ها زودتر (نه فقط
            // دقیقه‌ی آخر) بتونن قبول کنن. ثبتِ خودِ رزرو معطلِ این نمی‌مونه (این متد فوراً برمی‌گرده).
            await BroadcastTripAvailableAsync(trip.Id);

            await _noticeService.CreateAsync(new NoticeCreateDto
            {
                Label = NoticeTypeLabels.TripDriverRequested,
                ActorUserId = trip.UserId,
                ReferenceType = "Trip",
                ReferenceId = trip.Id,
                DeduplicationKey = $"{NoticeTypeLabels.TripDriverRequested}:{trip.Id}"
            });

            return new BaseResultDto<TripDto>(true, mapper.Map<TripDto>(trip));
        }

        /// <summary>
        /// حالت سه — سفرِ پت‌رسانِ «تاریخ‌دار»: نه سفر لحظه‌ای (حالت یک)، نه متصل به رزرو کلینیک (حالت دو).
        /// کاربر خودش یک تاریخ/ساعت دلخواه انتخاب می‌کنه؛ درست مثل سفر لحظه‌ای به همه‌ی راننده‌ها Broadcast
        /// می‌شه و ثبتش معطل تاییدِ هیچ راننده‌ای نمی‌مونه — فقط راننده‌ی پذیرنده باید سرِ همون زمانِ مشخص‌شده
        /// در مبدا حاضر بشه. پرداخت (طبق قرارداد مشترکِ همه‌ی حالت‌ها) فقط بعد از پذیرشِ راننده امکان‌پذیره.
        /// </summary>
        public async Task<BaseResultDto<TripDto>> CreateScheduledTripAsync(TripScheduledCreateDto dto, long userId)
        {
            if (dto.Origin == null)
                return new BaseResultDto<TripDto>(false, Resource.Notification.PleaseSetOrigin, null);

            if (dto.Destination == null)
                return new BaseResultDto<TripDto>(false, Resource.Notification.PleaseSetDestination, null);

            if (dto.ScheduledDepartureAt <= DateTime.Now)
                return new BaseResultDto<TripDto>(false, Resource.Notification.CompanionReserveCannotSelectPastTime, null);

            var priceInput = new TripDto
            {
                Origin = dto.Origin,
                Destination = dto.Destination,
                FromAddress = dto.FromAddress,
                ToAddress = dto.ToAddress,
                TripStartDateTime = dto.ScheduledDepartureAt,
                RoundTrip = dto.RoundTrip,
                TripOptionIds = dto.TripOptionIds
            };

            var trip = new Trip
            {
                Origin = new Point(dto.Origin.x, dto.Origin.y) { SRID = 4326 },
                Destination = new Point(dto.Destination.x, dto.Destination.y) { SRID = 4326 },
                FromAddress = dto.FromAddress,
                ToAddress = dto.ToAddress,
                UserId = userId,
                IsOnline = true,
                RoundTrip = dto.RoundTrip,
                CreateDate = DateTime.Now,
                TripStartDateTime = dto.ScheduledDepartureAt,
                DriverStatusId = (long)DriverStatusEnum.DriverStatus_Requested,
                TripStatusId = (long)TripStatusEnum.TripStatus_Requested,
                CompanionReserveId = null,
                ScheduledDepartureAt = dto.ScheduledDepartureAt,
                OwnerRidesAlong = dto.OwnerRidesAlong,
                ScheduledDispatched = false
            };

            try
            {
                trip.Price = await _priceCalculationService.CalculateTripPrice(priceInput);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Calculating price for scheduled trip (user {UserId}) failed.", userId);
                return new BaseResultDto<TripDto>(false, Resource.Notification.Unsuccess, null);
            }

            if (trip.Price <= 0)
            {
                _logger.LogError("Calculated price for scheduled trip (user {UserId}) was {Price} at scheduled hour {Hour} — refusing to create a free trip. Check PriceCalculation coverage for this hour.", userId, trip.Price, dto.ScheduledDepartureAt.Hour);
                return new BaseResultDto<TripDto>(false, Resource.Notification.FinalPriceIsNotAvailable, null);
            }

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

            ApplyTripPets(trip, dto.UserPetIds, null);

            await _context.Trips.AddAsync(trip);
            await _context.SaveChangesAsync();

            await BroadcastTripAvailableAsync(trip.Id);

            await _noticeService.CreateAsync(new NoticeCreateDto
            {
                Label = NoticeTypeLabels.TripDriverRequested,
                ActorUserId = trip.UserId,
                ReferenceType = "Trip",
                ReferenceId = trip.Id,
                DeduplicationKey = $"{NoticeTypeLabels.TripDriverRequested}:{trip.Id}"
            });

            return new BaseResultDto<TripDto>(true, mapper.Map<TripDto>(trip));
        }

        /// <summary>
        /// سفرِ پت‌رسانِ متصل به یک رزرو مشخص (اگه وجود داشته باشه) — برای نمایش «فلان راننده تایید کرد»
        /// یا «هنوز کسی قبول نکرده» به کاربر توی صفحه‌ی همون رزرو.
        /// </summary>
        public async Task<BaseResultDto<TripVDto>> GetTripForReservationAsync(long companionReserveId, long userId)
        {
            var trip = await _context.Trips
                .Include(s => s.Driver)
                .Include(s => s.DriverStatus)
                .Include(s => s.TripStatus)
                .AsNoTracking()
                .Where(s => s.CompanionReserveId == companionReserveId && s.UserId == userId)
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync();

            if (trip == null)
                return new BaseResultDto<TripVDto>(false, Resource.Notification.NothingFound, null);

            return new BaseResultDto<TripVDto>(true, mapper.Map<TripVDto>(trip));
        }

        /// <summary>
        /// Job زمان‌بندی‌شده (Hangfire): دیگه خودکار نزدیک‌ترین راننده رو اختصاص نمی‌ده — چون سفرهای
        /// رزرویی از همون لحظه‌ی ثبت (نه فقط لحظه‌ی حرکت) Broadcast می‌شن و راننده‌ها می‌تونن زودتر قبول
        /// کنن (بخش CreateReservationLinkedTripAsync). این Job فقط برای سفرهایی که موعد حرکتشون رسیده
        /// ولی هنوز *هیچ* راننده‌ای قبولشون نکرده، یک یادآوری به ادمین می‌ده تا از پنل (TripChooseDriver)
        /// خودش یکی رو دستی انتخاب کنه.
        /// </summary>
        public async Task DispatchScheduledTripsAsync()
        {
            var due = await _context.Trips
                .Where(t =>
                    t.ScheduledDepartureAt.HasValue &&
                    !t.ScheduledDispatched &&
                    t.ScheduledDepartureAt <= DateTime.Now &&
                    t.TripStatusId == (long)TripStatusEnum.TripStatus_Requested &&
                    t.DriverId == null)
                .AsTracking()
                .ToListAsync();

            if (due.Count == 0)
                return;

            var adminMobile = _adminSettingHelper.BaseAdminSetting.AdminMobiles;

            foreach (var trip in due)
            {
                trip.ScheduledDispatched = true;

                try
                {
                    await _messageSender.SendMessageAsync(messageType: MessageTypeEnum.DriverNotAcceptedYet, mobileReceptor: adminMobile, emailReceptor: null, token1: "-", token2: "-", token3: trip.Id.ToString());
                    await _noticeService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.TripDriverSelectionRequired, ReferenceType = "Trip", ReferenceId = trip.Id, DeduplicationKey = $"ScheduledDispatchReminder:{trip.Id}" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Notifying admin about unaccepted scheduled trip {TripId} failed.", trip.Id);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
