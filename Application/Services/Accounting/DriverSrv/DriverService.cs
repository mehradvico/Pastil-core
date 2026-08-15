using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.Accounting.DriverSrv.Dto;
using Application.Services.Accounting.DriverSrv.Iface;
using Application.Services.Accounting.UserSrv.Iface;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using AutoMapper;
using DocumentFormat.OpenXml.Office.CustomUI;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Services.Accounting.DriverSrv
{
    public class DriverService : CommonSrv<Driver, DriverDto>, IDriverService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly ICurrentUserHelper _currentUser;
        private readonly INoticeService _noticeService;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ILogger<DriverService> _logger;
        public DriverService(IDataBaseContext _context, IMapper mapper, IUserService userService, ICurrentUserHelper currentUser, INoticeService noticeService, IPushNotificationService pushNotificationService, ILogger<DriverService> logger) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._userService = userService;
            this._currentUser = currentUser;
            this._noticeService = noticeService;
            this._pushNotificationService = pushNotificationService;
            this._logger = logger;
        }

        public async Task<BaseResultDto<DriverVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.Drivers.Include(s => s.CertificatePicture).Include(s => s.ProfilePicture).Include(s => s.VehicleCardPicture).Include(s => s.City).ThenInclude(s => s.State).Include(s => s.Neighborhood).Include(s => s.Owner).Include(s => s.Status).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<DriverVDto>(true, mapper.Map<DriverVDto>(item));
            }
            return new BaseResultDto<DriverVDto>(false, mapper.Map<DriverVDto>(item));
        }

        public DriverSearchDto Search(DriverInputDto baseSearchDto)
        {
            var model = _context.Drivers.Include(s => s.CertificatePicture).Include(s => s.ProfilePicture).Include(s => s.VehicleCardPicture).Include(s => s.City).ThenInclude(s => s.State).Include(s => s.Neighborhood).Include(s => s.Owner).Include(s => s.Status).AsQueryable().Where(s => !s.Deleted);

            if (baseSearchDto.Available.HasValue)
            {
                model = model.Where(s => s.Active == baseSearchDto.Available.Value);
            }
            if (baseSearchDto.NeighborhoodId.HasValue)
            {
                model = model.Where(s => s.Neighborhood.Id == baseSearchDto.NeighborhoodId.Value);
            }
            if (baseSearchDto.CityId.HasValue)
            {
                model = model.Where(s => s.City.Id == baseSearchDto.CityId.Value);
            }
            if (baseSearchDto.StatusId.HasValue)
            {
                model = model.Where(s => s.StatusId == baseSearchDto.StatusId.Value);
            }
            if (baseSearchDto.Approved.HasValue)
            {
                model = model.Where(s => s.Approved == baseSearchDto.Approved.Value);
            }
            if (baseSearchDto.OwnerId.HasValue)
            {
                model = model.Where(s => s.Owner.Id == baseSearchDto.OwnerId.Value);
            }
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                model = model.Where(s => s.Name.Contains(baseSearchDto.Q));
            }
            if (!string.IsNullOrEmpty(baseSearchDto.Vehicle))
            {
                model = model.Where(s => s.Vehicle.Contains(baseSearchDto.Vehicle));
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
                default:
                    break;
            }
            return new DriverSearchDto(baseSearchDto, model, mapper);
        }
        public override async Task<BaseResultDto<DriverDto>> InsertAsyncDto(DriverDto dto)
        {
            try
            {
                var modelCheker = await InsertCheckerAsync(dto);
                if (!modelCheker.IsSuccess)
                {
                    return new BaseResultDto<DriverDto>(false, dto);
                }
                else
                {
                    var item = mapper.Map<Driver>(dto);
                    item.StatusId = (long)DriverRequestStatusEnum.DriverRequestStatus_Requested;
                    item.Active = false;
                    item.Approved = false;
                    item.Rate = 0;
                    item.AdminDetail = null;
                    if (dto.Rate != 0 && (dto.Rate > 5 || dto.Rate < 1))
                    {
                        return new BaseResultDto<DriverDto>(false, val1: Resource.Notification.TheRangeEnteredIsNotCorrect, val2: nameof(dto.Rate), data: dto);
                    }
                    var ownerId = dto.OwnerId;
                    var model = await _context.Users.Include(s => s.Driver).FirstOrDefaultAsync(s => s.Id == ownerId && !s.Deleted);
                    if (model == null)
                    {
                        return new BaseResultDto<DriverDto>(false, Resource.Notification.NothingFound, dto);
                    }
                    if (model.Driver != null)
                    {
                        return new BaseResultDto<DriverDto>(false, Resource.Notification.AlreadyIsDriver, dto);
                    }
                    await _context.Drivers.AddAsync(item);
                    await _context.SaveChangesAsync();
                    try
                    {
                        await _noticeService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.DriverSubmitted, ActorUserId = item.OwnerId, ReferenceType = "Driver", ReferenceId = item.Id, DeduplicationKey = $"{NoticeTypeLabels.DriverSubmitted}:{item.Id}", Metadata = new Dictionary<string, string> { { "driverName", item.Name } } });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Creating admin notice for driver request {DriverId} failed.", item.Id);
                    }
                    return new BaseResultDto<DriverDto>(true, mapper.Map<DriverDto>(item));

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Creating driver request for owner {OwnerId} failed.", dto?.OwnerId);
                return new BaseResultDto<DriverDto>(false, "خطا در ثبت درخواست رانندگی. اطلاعات فرم را بررسی کرده و دوباره تلاش کنید.", dto);
            }
        }
        public async Task<BaseResultDto> InsertCheckerAsync(DriverDto dto)
        {
            dto.Phone = await dto.Phone?.Trim().ToEnglishDigitsAsync();
            var errors = new List<Tuple<string, string>>();

            if (string.IsNullOrEmpty(dto.Name))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.PleaseEnterTheName, nameof(dto.Name)));
            }
            if (string.IsNullOrEmpty(dto.Phone))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.PleaseEnterThePhone, nameof(dto.Phone)));
            }
            if (string.IsNullOrEmpty(dto.Vehicle))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.PleaseEnterTheVehicle, nameof(dto.Vehicle)));
            }
            if (string.IsNullOrEmpty(dto.LicensePlateNumber))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.PleaseEnterTheLicensePlateNumber, nameof(dto.LicensePlateNumber)));
            }
            if (dto.OwnerId <= 0 || !await _context.Users.AnyAsync(s => s.Id == dto.OwnerId && !s.Deleted))
                errors.Add(Tuple.Create("کاربر درخواست‌دهنده معتبر نیست.", nameof(dto.OwnerId)));
            if (dto.CityId <= 0 || !await _context.Cities.AnyAsync(s => s.Id == dto.CityId))
                errors.Add(Tuple.Create("شهر انتخاب‌شده معتبر نیست.", nameof(dto.CityId)));
            if (dto.NeighborhoodId.HasValue &&
                !await _context.Neighborhoods.AnyAsync(s => s.Id == dto.NeighborhoodId.Value && s.CityId == dto.CityId))
                errors.Add(Tuple.Create("محله انتخاب‌شده متعلق به شهر انتخاب‌شده نیست.", nameof(dto.NeighborhoodId)));
            if (!dto.CertificatePictureId.HasValue || dto.CertificatePictureId.Value <= 0 ||
                !await _context.Pictures.AnyAsync(s => s.Id == dto.CertificatePictureId.Value))
                errors.Add(Tuple.Create("تصویر گواهینامه معتبر را بارگذاری کنید.", nameof(dto.CertificatePictureId)));
            if (!dto.VehicleCardPictureId.HasValue || dto.VehicleCardPictureId.Value <= 0 ||
                !await _context.Pictures.AnyAsync(s => s.Id == dto.VehicleCardPictureId.Value))
                errors.Add(Tuple.Create("تصویر کارت خودرو معتبر را بارگذاری کنید.", nameof(dto.VehicleCardPictureId)));
            if (dto.ProfilePictureId.HasValue && dto.ProfilePictureId.Value > 0 &&
                !await _context.Pictures.AnyAsync(s => s.Id == dto.ProfilePictureId.Value))
                errors.Add(Tuple.Create("تصویر پروفایل معتبر نیست.", nameof(dto.ProfilePictureId)));
            if (errors.Any())
            {
                return new BaseResultDto(isSuccess: false, messages: errors);
            }
            return new BaseResultDto(true);
        }
        public override BaseResultDto UpdateDto(DriverDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<DriverDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<Driver>(dto);
                    var driver = _context.Drivers.FirstOrDefault(x => x.Id == item.Id);
                    _context.Drivers.Attach(item);
                    _context.Entry(item).State = EntityState.Modified;
                    _context.SaveChanges();
                    return new BaseResultDto(isSuccess: true);
                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }

        public async Task<BaseResultDto> UpdateAsyncDto(DriverDto dto)
        {
            var result = UpdateDto(dto);
            if (result.IsSuccess)
                await _noticeService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.DriverUpdated, ActorUserId = dto.OwnerId, ReferenceType = "Driver", ReferenceId = dto.Id, DeduplicationKey = $"{NoticeTypeLabels.DriverUpdated}:{dto.Id}:{DateTime.UtcNow.Ticks}", Metadata = new Dictionary<string, string> { { "driverName", dto.Name } } });
            return result;
        }

        public async Task<BaseResultDto> ResubmitAsyncDto(DriverDto dto, long ownerId)
        {
            if (dto == null || dto.Id <= 0)
                return new BaseResultDto(false, "شناسه درخواست رانندگی معتبر نیست.");

            dto.OwnerId = ownerId;
            var checker = await InsertCheckerAsync(dto);
            if (!checker.IsSuccess)
                return checker;

            var item = await _context.Drivers
                .AsTracking()
                .FirstOrDefaultAsync(s => s.Id == dto.Id && s.OwnerId == ownerId && !s.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            if (item.Approved)
                return new BaseResultDto(false, "درخواست رانندگی تأیید شده است و از مسیر ارسال مجدد قابل تغییر نیست.");

            mapper.Map(dto, item);
            item.OwnerId = ownerId;
            item.StatusId = (long)DriverRequestStatusEnum.DriverRequestStatus_Requested;
            item.Active = false;
            item.Approved = false;
            item.Rate = 0;
            item.AdminDetail = null;
            await _context.SaveChangesAsync();

            try
            {
                await _noticeService.CreateAsync(new NoticeCreateDto
                {
                    Label = NoticeTypeLabels.DriverUpdated,
                    ActorUserId = ownerId,
                    ReferenceType = "Driver",
                    ReferenceId = item.Id,
                    DeduplicationKey = $"{NoticeTypeLabels.DriverUpdated}:{item.Id}:{DateTime.UtcNow.Ticks}",
                    Metadata = new Dictionary<string, string> { { "driverName", item.Name } }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Creating admin notice for resubmitted driver {DriverId} failed.", item.Id);
            }

            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> DriverUpdateStatusAsyncDto(DriverUpdateStatusDto dto)
        {
            if (dto.StatusId != (long)DriverRequestStatusEnum.DriverRequestStatus_Accepted &&
                dto.StatusId != (long)DriverRequestStatusEnum.DriverRequestStatus_Rejected)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            if (dto.StatusId == (long)DriverRequestStatusEnum.DriverRequestStatus_Rejected &&
                string.IsNullOrWhiteSpace(dto.AdminDetail))
                return new BaseResultDto(false, Resource.Notification.PleaseEnterTheAdminDetail);

            var driver = await _context.Drivers
                .AsTracking()
                .FirstOrDefaultAsync(s => s.Id == dto.Id && !s.Deleted);
            if (driver == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            var approved = dto.StatusId == (long)DriverRequestStatusEnum.DriverRequestStatus_Accepted;
            driver.StatusId = dto.StatusId;
            driver.Approved = approved;
            driver.Active = approved;
            driver.AdminDetail = approved ? null : dto.AdminDetail.Trim();
            await _context.SaveChangesAsync();

            try
            {
                await _pushNotificationService.SendPushAsync(
                    approved ? PushTypeEnum.PushDriverRequestApproved : PushTypeEnum.PushDriverRequestRejected,
                    driver.OwnerId,
                    token1: driver.Name,
                    token2: driver.AdminDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sending driver decision push for {DriverId} failed.", driver.Id);
            }

            return new BaseResultDto(true);
        }

        public BaseResultDto DriverUpdateStatusDto(DriverUpdateStatusDto dto)
        {
            try
            {
                var driver = _context.Drivers.FirstOrDefault(x => x.Id == dto.Id);
                if (dto.StatusId == (long)DriverRequestStatusEnum.DriverRequestStatus_Rejected)
                {
                    if (string.IsNullOrEmpty(dto.AdminDetail))
                    {
                        new Tuple<string, string>(Resource.Notification.PleaseEnterTheAdminDetail, nameof(dto.AdminDetail));
                    }
                    driver.AdminDetail = dto.AdminDetail;
                    driver.StatusId = dto.StatusId;
                    driver.Approved = false;
                }
                else
                {
                    driver.StatusId = dto.StatusId;
                    driver.Approved = true;
                }
                _context.Drivers.Update(driver);
                _context.SaveChanges();
                return new BaseResultDto(isSuccess: true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }
    }
}
